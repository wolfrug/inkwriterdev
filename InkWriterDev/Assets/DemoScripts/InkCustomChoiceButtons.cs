using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InkEngine {

    [System.Serializable]
    public class CustomInkChoiceButton {
        public InkFunctionEvent targetButtonFunction;
        public Button assignableButton;
        public CustomInkChoiceButton (InkFunctionEvent newButtonFunction, Button newAssignableButton) {
            targetButtonFunction = newButtonFunction;
            assignableButton = newAssignableButton;
        }
    }
    public class InkCustomChoiceButtons : MonoBehaviour { // This script lets you set up custom choice buttons based on what's in the gathered choices
        public SimpleInkWriter m_targetWriter; // writer that is used to play any preceding text lines & that we watch for appropriate gather choices
        public List<CustomInkChoiceButton> m_customButtons = new List<CustomInkChoiceButton> { };

        public bool m_hideWriterIfFound = true; // hides the dialog box if a custom button is found, e.g. because it's on a map or something
        public bool m_hideOriginalIfFound = true; // hides the original button if reassigned to a custom button
        public bool m_setTextIfFound = true; // sets the text on the assignable button in an ugly way to the button text of the...button
        public bool m_keepListsClean = true; // automatically cleans up the dictionaries/lists before checking anything - set to false if you prefer to do it yourself
        public DialogueOptionsPresentedEvent m_onButtonFoundEvent;
        private Dictionary<string, List<CustomInkChoiceButton>> m_inkFunctionDict = new Dictionary<string, List<CustomInkChoiceButton>> { };

        void Awake () {
            foreach (CustomInkChoiceButton btn in m_customButtons) { // adds the ones added in the editor initially
                AddNewFunctionEvent (btn);
                SetButtonInteractable (btn, false);
            }
            // Adds all of the target button function thingies to the ink writer too

        }
        void Start () {
            m_targetWriter.m_choicesShownEvent.AddListener (CheckChoices); // Add the listener to any choices!
            foreach (CustomInkChoiceButton btn in m_customButtons) {
                m_targetWriter.m_storyData.AddSearchableFunction (new InkTextVariable {
                    variableName = btn.targetButtonFunction.targetVariable,
                        VariableArguments = btn.targetButtonFunction.targetArguments.ToList ()
                });
            }
        }

        public void AddNewFunctionEvent (CustomInkChoiceButton evt) {
            if (m_inkFunctionDict.ContainsKey (evt.targetButtonFunction.targetVariable)) {
                m_inkFunctionDict[evt.targetButtonFunction.targetVariable].Add (evt);
            } else {
                m_inkFunctionDict.Add (evt.targetButtonFunction.targetVariable, new List<CustomInkChoiceButton> { evt });
            }
            if (!m_customButtons.Contains (evt)) {
                m_customButtons.Add (evt);
            };
        }
        public void SetButtonInteractable (CustomInkChoiceButton button, bool interactable) {
            button.assignableButton.interactable = interactable;
        }

        void CheckChoices (List < (InkChoiceLine, Button) > args) {
            // Check if any of the button choicelines have our arguments
            bool buttonFound = false;
            foreach ((InkChoiceLine, Button) button in args) {
                CustomInkChoiceButton foundButton = HasFunctionEvent (button.Item1.choiceText);
                // We found a button, yay!
                if (foundButton != null) {
                    buttonFound = true;
                    Debug.Log ("Found a button matching variable " + foundButton.targetButtonFunction.targetVariable + " and the button is " + foundButton.assignableButton);
                    // We disable the 'real' button first so it can't be clicked
                    if (m_hideOriginalIfFound) {
                        button.Item2.gameObject.SetActive (false);
                    };
                    // We move the onclick to the new button
                    foundButton.assignableButton.onClick = button.Item2.onClick;
                    // We add a listener to -all- the other buttons, so we know if one has been clicked
                    foreach (var otherbutton in args) {
                        otherbutton.Item2.onClick.AddListener (() => ClickedButton (foundButton));
                    }
                    // And set it to interactable, if the original button is
                    SetButtonInteractable (foundButton, button.Item2.interactable);
                    // And change the displayed text, in a rather ugly way
                    if (m_setTextIfFound) {
                        foundButton.assignableButton.gameObject.GetComponentInChildren<TextMeshProUGUI> ().SetText (button.Item1.choiceText.displayText);
                    };
                };
            }
            if (buttonFound) {
                m_onButtonFoundEvent.Invoke (args);
                // Also hide the dialog box, if bool'd
                if (m_hideWriterIfFound) {
                    m_targetWriter.CloseCurrentDialogBox (false);
                }
            }
        }
        void ClickedButton (CustomInkChoiceButton targetButton) {
            SetButtonInteractable (targetButton, false);
            targetButton.assignableButton.onClick.RemoveAllListeners (); // we also remove all listeners, just in case
        }

        CustomInkChoiceButton HasFunctionEvent (InkDialogueLine dialogueLine) {
            if (dialogueLine.inkVariables.Count < 1) {
                Debug.Log ("This choice line had no variables");
                return null;
            }
            if (m_keepListsClean) { CleanFunctionDictionary (); };
            foreach (CustomInkChoiceButton btn in m_customButtons) {
                string variableName = btn.targetButtonFunction.targetVariable;
                List<CustomInkChoiceButton> functionEvents = new List<CustomInkChoiceButton> { };
                m_inkFunctionDict.TryGetValue (variableName, out functionEvents);
                Debug.Log ("Looking for hits for variable " + variableName);
                if (functionEvents != null) {
                    if (functionEvents.Count > 0) {
                        foreach (CustomInkChoiceButton evt in functionEvents) {
                            foreach (InkTextVariable textvar in dialogueLine.inkVariables) {
                                if (evt.targetButtonFunction.ArgumentMatch (textvar.VariableArguments)) {
                                    evt.targetButtonFunction.onEvent.Invoke (dialogueLine, textvar);
                                    return evt;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        void CleanFunctionDictionary () {
            // Cleans the function dictionary of any null entries
            List<string> variablesToBeRemoved = new List<string> ();
            foreach (KeyValuePair<string, List<CustomInkChoiceButton>> kvp in m_inkFunctionDict) {
                foreach (CustomInkChoiceButton nullEntry in kvp.Value.FindAll ((x) => x.assignableButton == null)) {
                    kvp.Value.Remove (nullEntry);
                    // Also remove it from m_customButtons list
                    if (m_customButtons.Contains (nullEntry)) {
                        m_customButtons.Remove (nullEntry);
                    }
                }
                if (kvp.Value.Count < 1) {
                    variablesToBeRemoved.Add (kvp.Key);
                }
            }
            foreach (string variable in variablesToBeRemoved) {
                m_inkFunctionDict.Remove (variable);
            }
        }

    }
}