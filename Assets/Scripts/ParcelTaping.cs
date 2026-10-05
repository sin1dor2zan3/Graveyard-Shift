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

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null ||
            tapeStrip == null ||
            startMarker == null ||
            instructionText == null)
        {
            Debug.LogError(
                "ParcelTaping: assign the tape, marker, and instruction text."
            );

            enabled = false;
            return;
        }

        fullScale = tapeStrip.transform.localScale;
        fullCenter = tapeStrip.transform.position;

        leftEdge = fullCenter.x - fullScale.x / 2f;
        rightEdge = fullCenter.x + fullScale.x / 2f;

        ready = true;
        UpdateVisuals();
    }

    private void Update()
    {
        if (!ready)
            return;

        OrderSession session = OrderSession.Instance;

        if (session == null || !session.HasActiveOrder ||
            !session.PackingApproved || !session.IsWeighed)
        {
            dragging = false;
            startMarker.gameObject.SetActive(false);
            instructionText.text = "Weigh the parcel before sealing it.";
            return;
        }

        if (session.IsSealed)
        {
            progress = 1f;
            dragging = false;
            UpdateVisuals();
            instructionText.text = "Sealed! Your parcel is ready for a label.";
            return;
        }

        startMarker.gameObject.SetActive(true);

        if (!dragging)
        {
            instructionText.text =
                "Drag from the mint dot along the seam to the right.";

            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                TryBegin(
                    Touchscreen.current.primaryTouch.position.ReadValue(),
                    true
                );
            }
            else if (Mouse.current != null &&
                     Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryBegin(Mouse.current.position.ReadValue(), false);
            }
        }

        if (!dragging)
            return;

        Vector2 screenPosition;
        bool pressed;

        if (usingTouch)
        {
            if (Touchscreen.current == null)
            {
                dragging = false;
                return;
            }

            var touch = Touchscreen.current.primaryTouch;

            if (touch.phase.ReadValue() ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                dragging = false;
                return;
            }

            screenPosition = touch.position.ReadValue();
            pressed = touch.press.isPressed;
        }
        else
        {
            if (Mouse.current == null)
            {
                dragging = false;
                return;
            }

            screenPosition = Mouse.current.position.ReadValue();
            pressed = Mouse.current.leftButton.isPressed;
        }

        Vector3 position = ScreenToWorld(screenPosition);

        if (Mathf.Abs(position.y - fullCenter.y) > seamTolerance)
        {
            dragging = false;
            instructionText.text =
                "Follow the seam. Start again from the mint dot.";
            return;
        }

        float currentEdge = Mathf.Lerp(leftEdge, rightEdge, progress);

        float movement = position.x - previousPointerX;
        previousPointerX = position.x;

        float nextEdge = currentEdge + Mathf.Max(0f, movement);
        progress = Mathf.Clamp01(
            (nextEdge - leftEdge) / (rightEdge - leftEdge)
        );

        UpdateVisuals();
        instructionText.text = "Keep dragging along the seam.";

        if (progress >= 1f)
        {
            session.SealParcel();
            dragging = false;
            startMarker.gameObject.SetActive(false);
            instructionText.text = "Sealed! Your parcel is ready for a label.";
        }
        else if (!pressed)
        {
            dragging = false;
        }
    }

    private float previousPointerX;

    private void TryBegin(Vector2 screenPosition, bool touchInput)
    {
        if (IsOverUI(screenPosition))
            return;

        Vector3 position = ScreenToWorld(screenPosition);

        if (Vector2.Distance(position, startMarker.position) > grabRadius)
            return;

        usingTouch = touchInput;
        dragging = true;
        previousPointerX = position.x;
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
        float visibleWidth = fullScale.x * progress;

        tapeStrip.enabled = progress > 0f;

        tapeStrip.transform.localScale = new Vector3(
            visibleWidth,
            fullScale.y,
            fullScale.z
        );

        tapeStrip.transform.position = new Vector3(
            leftEdge + visibleWidth / 2f,
            fullCenter.y,
            fullCenter.z
        );

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
            dragging = false;
    }

    private void OnDisable()
    {
        dragging = false;
    }
}