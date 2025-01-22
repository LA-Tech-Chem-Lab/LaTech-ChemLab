using UnityEngine;
using UnityEngine.UI;

public class DropdownBoxController : MonoBehaviour
{
    public GameObject contentPanel; // The panel that expands/collapses
    public Button toggleButton; // The button to expand/collapse
    private bool isExpanded = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure content starts collapsed
        contentPanel.SetActive(false);

        // Add listener to the button
        toggleButton.onClick.AddListener(ToggleDropdown);
    }

    void ToggleDropdown()
    {
        isExpanded = !isExpanded;
        contentPanel.SetActive(isExpanded);
    }
}
