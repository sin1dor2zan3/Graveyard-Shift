using UnityEngine;

public class GhostBobbing : MonoBehaviour
{
    [SerializeField] private float height = 0.08f;
    [SerializeField] private float cyclesPerSecond = 0.4f;

    private Vector3 restingPosition;
    private float elapsed;

    private void OnEnable()
    {
        restingPosition = transform.localPosition;
        elapsed = 0f;
    }

    private void Update()
    {
        if (PlayerPrefs.GetInt("Settings.ReducedMotion", 0) == 1)
        {
            transform.localPosition = restingPosition;
            elapsed = 0f;
            return;
        }

        elapsed += Time.deltaTime;

        float offset = Mathf.Sin(
            elapsed * cyclesPerSecond * Mathf.PI * 2f
        ) * height;

        transform.localPosition =
            restingPosition + Vector3.up * offset;
    }

    private void OnDisable()
    {
        transform.localPosition = restingPosition;
    }
}