using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;

public class InkInventoryManager : MonoBehaviour {
    public InkInventoryItemData m_baseItemData;
    public InkInventoryData m_baseInventoryData;
    public List<InkInventoryData> m_spawnedInventories = new List<InkInventoryData> { };
    public static List<InkInventoryItemData> m_itemDatas = new List<InkInventoryItemData> { };
    public static List<InkInventoryData> m_inventoryDatas = new List<InkInventoryData> { };

    void Awake () {
        LoadAllItemDatas ();
        LoadAllInventoryDatas ();
    }
    void LoadAllItemDatas () {
        // Load all item datas!
        if (m_itemDatas.Count == 0) {
            Object[] loadedDatas = Resources.LoadAll ("Data/ItemData", typeof (InkInventoryItemData));
            foreach (Object obj in loadedDatas) {
                m_itemDatas.Add (obj as InkInventoryItemData);
            }
        }
    }
    void LoadAllInventoryDatas () {
        // Load all item datas!
        if (m_itemDatas.Count == 0) {
            Object[] loadedDatas = Resources.LoadAll ("Data/InventoryData", typeof (InkInventoryData));
            foreach (Object obj in loadedDatas) {
                m_inventoryDatas.Add (obj as InkInventoryData);
            }
        }
    }

    public void UpdateInkInventoryListener (InkEngine.InkDialogueLine dialogueLine, InkEngine.InkTextVariable variable) {
        // This is a convenience function to let you update directly from the standard listener
        string id = variable.VariableArguments[0];
        InkInventoryData inventory = CreateNewInventory (id); // create a new one, or update the old one
        if (inventory != null) {
            inventory.UpdateFromInk ();
        }
    }

    public InkInventoryData CreateNewInventory (string id, string baseInventoryId = "") {
        InkInventoryData baseInventory = m_baseInventoryData;
        if (baseInventoryId != "") {
            baseInventory = m_inventoryDatas.Find ((x) => x.m_id == baseInventoryId);
            if (baseInventory == null) {
                Debug.LogWarning ("Cannot find a base inventory with id " + baseInventoryId + ", reverting to default.");
                baseInventory = m_baseInventoryData;
            }
        }
        // Make sure we only have one
        if (!HasInventory (id)) {
            InkInventoryData newInventory = Instantiate (baseInventory);
            newInventory.m_id = id;
            m_spawnedInventories.Add (newInventory);
            return newInventory;
        } else {
            Debug.LogWarning ("Tried to create a second copy of inventory with id " + id + ", returning existing copy.");
            return m_spawnedInventories.Find ((x) => x.m_id == id);
        }
    }
    public InkInventoryData GetInventory (string id) {
        if (HasInventory (id)) {
            return m_spawnedInventories.Find ((x) => x.m_id == id);
        } else {
            Debug.LogWarning ("Could not find an inventory with id " + id);
            return null;
        }
    }

    public void UpdateInventory (string id) {
        if (HasInventory (id)) {
            m_spawnedInventories.Find ((x) => x.m_id == id).UpdateFromInk ();
        } else {
            Debug.LogWarning ("Tried to update inventory with id " + id + " but could not find it, ignoring.");
        }
    }

    public bool HasInventory (string id) {
        return m_spawnedInventories.Find ((x) => x.m_id == id) != null;
    }
}