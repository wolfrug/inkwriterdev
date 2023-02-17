using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;
using UnityEngine.UI;

public class SimpleInkInventory : MonoBehaviour {
    public Transform m_itemParent;
    public string m_inventoryId = "";
    public InkInventoryData m_data;
    public InkInventoryManager m_manager;

    void Start () {
        if (m_manager == null) {
            m_manager = FindObjectOfType<InkInventoryManager> ();
        }
        if (m_data == null && m_inventoryId != "" && m_manager != null) {
            if (m_manager.HasInventory (m_inventoryId)) {
                UpdateInventory (m_manager.GetInventory (m_inventoryId));
            } else {
                m_data = m_manager.CreateNewInventory (m_inventoryId, m_inventoryId);
                UpdateCurrentInventory ();
            };
        }
    }

    public void UpdateInventory (InkInventoryData data) {
        m_data = data;
        ClearInventory ();
        if (m_data != null) {
            m_inventoryId = m_data.m_id;
            foreach (InkInventoryItemData item in data.m_contents) {
                Instantiate (item.m_prefab, m_itemParent).GetComponent<SimpleInkInventoryBox> ().SetItem (item);
            }
        };
    }
    public void UpdateInkInventoryListener (InkEngine.InkDialogueLine dialogueLine, InkEngine.InkTextVariable variable) {
        // This is a convenience function to let you update directly from the standard listener
        if (variable.VariableArguments[0] == m_inventoryId) {
            string id = variable.VariableArguments[0];
            InkInventoryData inventory = m_manager.CreateNewInventory (id); // create a new one, or update the old one
            if (inventory != null) {
                m_data = inventory;
                inventory.UpdateFromInk ();
                UpdateCurrentInventory ();
            }
        }
    }

    public void UpdateCurrentInventory () {
        m_data.UpdateFromInk ();
        UpdateInventory (m_data);
    }
    void ClearInventory () {
        foreach (Transform child in m_itemParent) {
            Destroy (child.gameObject);
        }
    }
}