using UnityEngine;

public class SaveMenuUI : MonoBehaviour
{
    public GameObject saveSlotsMenu; // Assign in the Inspector

    public void ToggleSaveSlots()
    {
        if (saveSlotsMenu != null)
        {
            saveSlotsMenu.SetActive(!saveSlotsMenu.activeSelf);
        }
    }
}
