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
            selected.CanRotate;
        rotateButton.interactable = canRotate;

        if (buttonLabel != null)
        {
            buttonLabel.text = canRotate
                ? "Rotate Item"
                : "Select Item";
        }
    }

    public void RotateSelection()
    {
        DraggableItem selected = DraggableItem.SelectedItem;

        if (selected != null)
            selected.RotateSelected();
    }
}
