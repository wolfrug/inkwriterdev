using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (InkCustomChoiceButtons))]
public class InkUsableInventory : MonoBehaviour {
    [Tooltip ("Inventory that is watched and targeted. Note that the inventory items need to have Buttons assigned to them.")]
    public SimpleInkInventory m_targetInventory;
    [Tooltip ("Required component, setup to own liking. REMEMBER TO ASSIGN A WRITER!")]
    public InkCustomChoiceButtons m_choiceButtonSetup;
    [Tooltip ("This is the function used to find buttons, set up to own liking")]
    public InkFunctionEvent m_buttonFindFunction = new InkFunctionEvent ("USE_ITEM", new List<string> (), ArgumentRequirements.ALL, new TextFunctionFoundEvent ());
    // Start is called before the first frame update
    void Start () {
        if (m_choiceButtonSetup == null) {
            m_choiceButtonSetup = GetComponent<InkCustomChoiceButtons> ();
        }
        m_choiceButtonSetup.m_targetWriter.m_storyData.AddSearchableFunction (new InkTextVariable {
            variableName = m_buttonFindFunction.targetVariable
        });
    }

    public void SetupUsableInventory () {
        foreach (SimpleInkInventoryBox box in m_targetInventory.m_spawnedBoxes) {
            if (box.m_selectable != null) {
                // We use the ID of the item as the event's argument
                InkFunctionEvent copyEvent = new InkFunctionEvent (m_buttonFindFunction.targetVariable, new List<string> { box.m_data.m_id }, m_buttonFindFunction.argumentRequirement, m_buttonFindFunction.onEvent);
                CustomInkChoiceButton newButton = new CustomInkChoiceButton (copyEvent, (Button) box.m_selectable);
                m_choiceButtonSetup.AddNewFunctionEvent (newButton);
            } else {
                Debug.LogWarning ("Tried to add a usable item with id " + box.m_data.m_id + " but the prefab had no Selectable setup! Please make sure the prefabs have buttons assigned.");
            }
        }
    }

    public void UpdateInkUsableInventoryListener (InkEngine.InkDialogueLine dialogueLine, InkEngine.InkTextVariable variable) {
        if (variable.VariableArguments[0] == m_targetInventory.m_inventoryId) {
            SetupUsableInventory ();
        }
    }
}