using UnityEngine;

public class SkillsWindowToggle : MonoBehaviour
{
    [SerializeField]
    private GameObject skillsWindow;

    public void ToggleSkillsWindow()
    {
        if (skillsWindow == null)
            return;

        skillsWindow.SetActive(
            !skillsWindow.activeSelf);
    }
}