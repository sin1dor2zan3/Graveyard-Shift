using UnityEngine;
using UnityEngine.UI;

public class MotionSettingsUI : MonoBehaviour
{
    [SerializeField] private Toggle reducedMotionToggle;

    private void OnEnable()
    {
        reducedMotionToggle.SetIsOnWithoutNotify(
            PlayerPrefs.GetInt("Settings.ReducedMotion", 0) == 1
        );

        reducedMotionToggle.onValueChanged.AddListener(ChangeMotion);
    }

    private void OnDisable()
    {
        reducedMotionToggle.onValueChanged.RemoveListener(ChangeMotion);
    }

    private void ChangeMotion(bool enabled)
    {
        PlayerPrefs.SetInt("Settings.ReducedMotion", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}