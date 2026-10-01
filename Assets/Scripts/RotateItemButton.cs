using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RotateItemButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonLabel;

    private Button rotateButton;

    private void Awake()
    {
        rotateButton = GetComponent<Button>();
    }

    private void Update()
    {
        DraggableItem selected = DraggableItem.SelectedItem;

        bool canRotate = selected != null &&
            selected.isActiveAndEnabled &&
            selected.Width != selected.Height;

        rotateButton.interactable = canRotate;

        if (buttonLabel != null)
        {
            buttonLabel.text = canRotate
                ? "Rotate Letter"
                : "Select Letter";
        }
    }

    public void RotateSelection()
    {
        DraggableItem selected = DraggableItem.SelectedItem;

        if (selected != null)
            selected.RotateSelected();
    }
}
