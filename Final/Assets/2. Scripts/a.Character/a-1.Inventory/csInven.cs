using UnityEngine;

public class csInven : MonoBehaviour
{
    public static csInven Instance;

    public GameObject pnlInven;

    public csSlot[] hotbarSlots = new csSlot[4];

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryActive();
        }
    }

    void InventoryActive()
    {
        if (pnlInven.activeSelf == false)
        {
            pnlInven.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!csButtonManager.isMultiplayer)
            {
                Time.timeScale = 0f;
            }
        }
        else
        {
            pnlInven.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!csButtonManager.isMultiplayer)
            {
                Time.timeScale = 1f;
            }
        }
    }

    public ItemData GetHotbarItem(int index)
    {
        if (index >= 0 && index < hotbarSlots.Length && hotbarSlots[index] != null)
        {
            return hotbarSlots[index].itemData;
        }
        return null;
    }
}
