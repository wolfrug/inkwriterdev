using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;

[CreateAssetMenu (fileName = "Data", menuName = "InkEngine/Ink Inventory Item", order = 1)]
public class InkInventoryItemData : ScriptableObject {

    public InkStoryData m_storyData;
    public string m_id;
    public string m_displayName;
    //[NaughtyAttributes.ShowAssetPreview]
    public Sprite m_image;
    [TextArea]
    public string m_description;
    [Tooltip ("If it is set to stackable, make sure you have an Ink variable named <ID of item>_stack!")]
    public bool m_stackable = false;
    public GameObject m_prefab;
    private int m_currentStack = 1;

    private string m_stackVariableSuffix = "_stack"; // if an item is stackable, this would be the name of the variable containing the stack, e.g. "Test_stack"

    // You must handle max / min stacks in Ink itself
    public int Stack {
        get {
            return m_currentStack;
        }
        set {
            m_currentStack = value;
        }
    }

    public void UpdateStackFromInk () { // update stack from ink
        if (m_stackable) { // if it is stackable that is
            if (m_storyData != null) {
                int stackVariable = (int) m_storyData.InkStory.variablesState[m_id + m_stackVariableSuffix];
                Stack = stackVariable;
            }
        }
    }
    public void UpdateStackToInk () { // update ink variable with current stack
        if (m_stackable) { // we don't bother even trying if maxstack is just 1
            if (m_storyData != null) {
                m_storyData.InkStory.variablesState[m_id + m_stackVariableSuffix] = Stack;
            }
        }
    }
}