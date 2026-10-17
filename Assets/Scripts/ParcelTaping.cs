using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ParcelTaping : MonoBehaviour
{
    [SerializeField] private SpriteRenderer tapeStrip;
    [SerializeField] private Transform startMarker;
    [SerializeField] private TMP_Text instructionText;

    [Header("Touch and Mouse Tolerance")]
    [SerializeField] private float grabRadius = 0.55f;
    [SerializeField] private float seamTolerance = 0.65f;

    private Camera mainCamera;
    private Vector3 fullScale;
    private Vector3 fullCenter;

    private float leftEdge;
    private float rightEdge;
    private float progress;

    private bool dragging;
    private bool usingTouch;
    private bool ready;
    [SerializeField] private Collider2D tapeGunCollider;
    [SerializeField] private Transform tapeTip;

    private Vector3 gunHome;
    private Vector3 gunGrabOffset;
    private bool applyingTape;

    private void ReturnGun()
    {
        dragging = false;
        applyingTape = false;

        if (tapeGunCollider != null)
            tapeGunCollider.transform.position = gunHome;
    }

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null ||
            tapeStrip == null ||
            startMarker == null ||
            tapeGunCollider == null ||
            tapeTip == null ||
            instructionText == null)
        {
            Debug.LogError(
                "ParcelTaping: assign the tape, marker, and instruction text."
            );

            enabled = false;
            return;
        }

        fullScale = tapeStrip.transform.localScale;

        Bounds tapeBounds = tapeStrip.bounds;
        fullCenter = tapeBounds.center;

        leftEdge = tapeBounds.min.x;
        rightEdge = tapeBounds.max.x;

        if (rightEdge - leftEdge <= 0.001f)
        {
            Debug.LogError("Tape sprite must have a visible width.");
            enabled = false;
            return;
        }
        gunHome = tapeGunCollider.transform.position;
        ready = true;
        UpdateVisuals();
    }

    private void Update()
    {
        if (!ready)
            return;

        if (PauseMenu.IsPaused)
        {
            ReturnGun();
            return;
        }

        OrderSession session = OrderSession.Instance;

        if (session == null || !session.HasActiveOrder ||
            !session.PackingApproved || !session.IsWeighed)
        {
            ReturnGun();
            startMarker.gameObject.SetActive(false);
            instructionText.text = "Weigh the parcel before sealing.";
            return;
        }

        if (session.IsSealed)
        {
            ReturnGun();
            instructionText.text = "Sealed! Print and attach the label.";
            return;
        }

        if (!dragging)
        {
            instructionText.text =
                "Pick up the tape gun. Bring its tip to the left end of the seam.";

            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                TryBegin(
                    Touchscreen.current.primaryTouch.position.ReadValue(),
                    true);
            }
            else if (Mouse.current != null &&
                     Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryBegin(Mouse.current.position.ReadValue(), false);
            }
        }

        if (!dragging)
            return;

        Vector2 pointer;
        bool held;

        if (usingTouch)
        {
            if (Touchscreen.current == null)
            {
                ReturnGun();
                return;
            }

            var touch = Touchscreen.current.primaryTouch;

            if (touch.phase.ReadValue() ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                ReturnGun();
                return;
            }

            pointer = touch.position.ReadValue();
            held = touch.press.isPressed;
        }
        else
        {
            if (Mouse.current == null)
            {
                ReturnGun();
                return;
            }

            pointer = Mouse.current.position.ReadValue();
            held = Mouse.current.leftButton.isPressed;
        }

        tapeGunCollider.transform.position =
            ScreenToWorld(pointer) + gunGrabOffset;

        if (!held)
        {
            ReturnGun();
            return;
        }

        Vector3 tip = tapeTip.position;
        float currentEdge = Mathf.Lerp(leftEdge, rightEdge, progress);

        if (!applyingTape)
        {
            Vector2 target = new Vector2(currentEdge, fullCenter.y);

            if (Vector2.Distance(tip, target) <= grabRadius)
                applyingTape = true;
            else
            {
                instructionText.text =
                    "Bring the gun tip to the end of the tape seam.";
                return;
            }
        }

        if (Mathf.Abs(tip.y - fullCenter.y) > seamTolerance)
        {
            applyingTape = false;
            instructionText.text = "Return the gun tip to the tape end.";
            return;
        }

        progress = Mathf.Max(
            progress,
            Mathf.Clamp01((tip.x - leftEdge) / (rightEdge - leftEdge)));

        UpdateVisuals();
        instructionText.text = "Drag the gun right along the seam.";

        if (progress >= 1f && session.SealParcel())
        {
            ReturnGun();
            startMarker.gameObject.SetActive(false);
            instructionText.text = "Sealed! Print and attach the label.";
        }
    }

    private float previousPointerX;

    private void TryBegin(Vector2 screenPosition, bool touchInput)
    {
        if (IsOverUI(screenPosition))
            return;

        Vector3 position = ScreenToWorld(screenPosition);

        if (!tapeGunCollider.OverlapPoint(position))
            return;

        usingTouch = touchInput;
        dragging = true;
        applyingTape = false;

        gunGrabOffset =
            tapeGunCollider.transform.position - position;
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        Vector3 position = mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                fullCenter.z - mainCamera.transform.position.z
            )
        );

        position.z = fullCenter.z;
        return position;
    }

    private bool IsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = screenPosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        return results.Count > 0;
    }

    private void UpdateVisuals()
    {
        float visibleWidth = (rightEdge - leftEdge) * progress;

        tapeStrip.enabled = progress > 0f;

        tapeStrip.transform.localScale = new Vector3(
            fullScale.x * progress,
            fullScale.y,
            fullScale.z
        );

        Vector3 visibleCenter = new Vector3(
            leftEdge + visibleWidth / 2f,
            fullCenter.y,
            fullCenter.z
        );

        Vector3 pivotOffset = tapeStrip.transform.TransformVector(
            tapeStrip.sprite.bounds.center);

        tapeStrip.transform.position = visibleCenter - pivotOffset;

        startMarker.position = new Vector3(
            Mathf.Lerp(leftEdge, rightEdge, progress),
            fullCenter.y,
            fullCenter.z
        );

        startMarker.gameObject.SetActive(progress < 1f);
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ReturnGun();
    }

    private void OnDisable()
    {
        ReturnGun();
    }
}