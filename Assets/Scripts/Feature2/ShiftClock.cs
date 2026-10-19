using TMPro;
using UnityEngine;

public class ShiftClock : MonoBehaviour
{
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private TMP_Text nightText;

    [Tooltip("Adjust if the hand artwork does not point up at zero rotation.")]
    [SerializeField] private float hourRotationOffset;

    [SerializeField] private float minuteRotationOffset;
    [SerializeField, Min(1f)]
    private float secondsPerMinuteHandRevolution = 60f;

    private float minuteAngle;

    private void Update()
    {
        ShiftController shift = ShiftController.Instance;

        if (shift == null || shift.NightNumber < 1)
            return;

        if (nightText != null)
            nightText.text = $"Night {shift.NightNumber}";

        int completed = shift.SuccessfulOrders + shift.FailedOrders;

        // Midnight at the start; 6 a.m. after all three orders.
        float hours = 6f * Mathf.Clamp01(
            completed / (float)Mathf.Max(1, shift.OrderCount));

        if (hourHand != null)
        {
            hourHand.localRotation = Quaternion.Euler(
                0f, 0f, hourRotationOffset - hours * 30f);
        }

        if (minuteHand != null)
        {
            bool shiftFinished =
                completed >= shift.OrderCount;

            if (!PauseMenu.IsPaused && !shiftFinished)
            {
                minuteAngle = Mathf.Repeat(
                    minuteAngle +
                    360f / secondsPerMinuteHandRevolution * Time.deltaTime,
                    360f);
            }

            minuteHand.localRotation = Quaternion.Euler(
                0f, 0f, minuteRotationOffset - minuteAngle);
        }
    }
}