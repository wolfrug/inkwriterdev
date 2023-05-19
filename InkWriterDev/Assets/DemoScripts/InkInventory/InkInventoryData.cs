using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;

[CreateAssetMenu (fileName = "Data", menuName = "InkEngine/Ink Inventory", order = 1)]
public class InkInventoryData : ScriptableObject {

    public InkStoryData m_storyData;
    public string m_id;
    public string m_displayName;
    [NaughtyAttributes.ShowAssetPreview]
    public Sprite m_image;
    [Multiline]
    public string m_description;
    public int m_maxSize = 9;
    public List<InkInventoryItemData> m_contents = new List<InkInventoryItemData> { };

    public int AddItem (InkInventoryItemData newItem) {
        // First we do this the easy way
        if (m_contents.Count < m_maxSize && !HasItem (newItem)) { // we don't add duplicates
            m_contents.Add (newItem);
            newItem.UpdateStackFromInk ();
            return 1;
        }
        return 0;
    }
    public int AddItem (string id) {
        InkInventoryItemData baseItem = InkInventoryManager.m_itemDatas.Find ((x) => x.m_id == id);
        if (baseItem != null) {
            return AddItem (Instantiate (baseItem));
        } else {
            Debug.LogWarning ("Could not find item with id " + id);
        }
        return 0;
    }
    public int RemoveItem (InkInventoryItemData itemToRemove) {
        // Easy way
        if (m_contents.Find ((x) => x.m_id == itemToRemove.m_id) != null) {
            m_contents.Remove (m_contents.Find ((x) => x.m_id == itemToRemove.m_id));
            return 1;
        }
        return 0;
    }
    public int RemoveItem (string id) {
        InkInventoryItemData baseItem = InkInventoryManager.m_itemDatas.Find ((x) => x.m_id == id);
        if (baseItem != null) {
            return RemoveItem (baseItem);
        } else {
            Debug.LogWarning ("Could not find item with id " + id);
        }
        return 0;
    }
    public void ClearInventory () {
        m_contents.Clear ();
    }

    public bool HasItem (InkInventoryItemData item) {
        return m_contents.Find ((x) => x.m_id == item.m_id) != null;
    }
    public bool HasItem (string id) {
        return m_contents.Find ((x) => x.m_id == id) != null;
    }

    public void UpdateFromInk () {
        //Debug.Log ("Attempting to update from ink for id " + m_id);
        // Id must equal the name of the variable in Ink
        if (m_storyData != null) {
            var newList = m_storyData.InkStory.variablesState[m_id] as Ink.Runtime.InkList;
            ClearInventory ();
            foreach (var item in newList) {
                AddItem (item.Key.itemName);
            }
        } else {
            Debug.LogWarning ("Cannot find loaded Ink Story");
        }
    }
    public void UpdateToInk () {
        // Note: there must be a LIST idName definition in ink, with -all possible entries- to that list for this to work properly
        if (m_storyData != null) {
            var newList = new Ink.Runtime.InkList (m_id, m_storyData.InkStory);
            foreach (InkInventoryItemData item in m_contents) {
                newList.AddItem (item.m_id);
                item.UpdateStackToInk ();
            }
            m_storyData.InkStory.variablesState[m_id] = newList;
        } else {
            Debug.LogWarning ("Cannot find loaded Ink Story");
        }
    }

}