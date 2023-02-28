using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;
using UnityEngine.UI;

public class SimpleInkInventory : MonoBehaviour {
    public GameObject m_activeSelf;
    public Transform m_itemParent;
    public string m_inventoryId = "";
    public InkInventoryData m_data;
    public InkInventoryManager m_manager;
    public List<SimpleInkInventoryBox> m_spawnedBoxes = new List<SimpleInkInventoryBox> { };

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
        if (m_activeSelf == null) {
            m_activeSelf = m_itemParent.gameObject;
        }
    }
    public bool Active {
        get {
            return m_activeSelf.activeSelf;
        }
        set {
            m_activeSelf.SetActive (value);
        }
    }

    public void UpdateInventory (InkInventoryData data) {
        m_data = data;
        ClearInventory ();
        if (m_data != null) {
            m_inventoryId = m_data.m_id;
            foreach (InkInventoryItemData item in data.m_contents) {
                SimpleInkInventoryBox newBox = Instantiate (item.m_prefab, m_itemParent).GetComponent<SimpleInkInventoryBox> ();
                newBox.SetItem (item);
                m_spawnedBoxes.Add (newBox);
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
        if (m_data != null) {
            m_data.UpdateFromInk ();
            UpdateInventory (m_data);
        } else {
            Debug.LogWarning ("Cannot update inventory with name " + gameObject.name + " because it has no inventory data set!");
        }
    }
    void ClearInventory () {
        foreach (Transform child in m_itemParent) {
            Destroy (child.gameObject);
        }
        m_spawnedBoxes.Clear ();
    }
}