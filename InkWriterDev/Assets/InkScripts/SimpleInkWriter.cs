using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InkEngine {

    [System.Serializable]
    public class TextTagFoundEvent : UnityEvent<InkDialogueLine, string> { }

    [System.Serializable]
    public class TextFunctionFoundEvent : UnityEvent<InkDialogueLine, InkTextVariable> { }

    [System.Serializable]
    public class DialoguePresentedEvent : UnityEvent<InkDialogueLine> { }

    [System.Serializable]
    public class DialogueOptionsPresentedEvent : UnityEvent < List < (InkChoiceLine, Button) >> { }

    [System.Serializable]
    public class WriterEvent : UnityEvent<SimpleInkWriter> { }
    public class SimpleInkWriter : MonoBehaviour {

        public InkStoryData m_storyData;
        public SimpleInkDialogBox m_currentDialogBox;

        [Tooltip ("For specific text functions, e.g. PLAYER(sad, right)")]
        public TextFunctionFoundEvent m_textFunctionFoundEvent;
        [Tooltip ("For regular ink tags, e.g. #playmusic (do not include the hashtag)")]
        public TextTagFoundEvent m_inkTagFoundEvent;
        public DialoguePresentedEvent m_dialogueShownEvent;
        public DialogueOptionsPresentedEvent m_choicesShownEvent;
        public WriterEvent m_writerFinishedEvent;
        public WriterEvent m_writerStartedEvent;
        private bool m_optionPressed = false;
        private bool m_waitingOnOptionPress = false;
        private Coroutine m_displayCoroutine = null;
        void Awake () {
            if (m_storyData == null) {
                m_storyData = Resources.LoadAll<InkStoryData> ("InkStoryData") [0];
            }
            // Add the default searchable INTERACTABLE tag - used by choices to make them inactive if this function tag is found
            m_storyData.AddSearchableFunction (new InkTextVariable {
                variableName = "INTERACTABLE",
                    VariableArguments = new List<string> { "false" }
            });
        }

        public void PlayKnot (string knotName) { // play directly from a knot
            if (m_storyData.IsLoaded ()) {
                List<InkChoiceLine> gatherChoices = new List<InkChoiceLine> { };
                InkDialogueLine[] dialogueLines = m_storyData.CreateStringArrayKnot (knotName, gatherChoices);
                if (m_displayCoroutine != null) {
                    StopCoroutine (m_displayCoroutine);
                } else {
                    m_writerStartedEvent.Invoke (this);
                }
                m_displayCoroutine = StartCoroutine (DisplayText (dialogueLines, gatherChoices));
            };
        }
        public void PlayChoice (Choice choice) { // play from a choice - mainly used internally
            if (m_storyData.IsLoaded ()) {
                List<InkChoiceLine> gatherChoices = new List<InkChoiceLine> { };
                InkDialogueLine[] dialogueLines = m_storyData.CreateStringArrayChoice (choice, gatherChoices);
                if (m_displayCoroutine != null) {
                    StopCoroutine (m_displayCoroutine);
                } else {
                    m_writerStartedEvent.Invoke (this); // We invoke the started event when starting a whole new thing
                }
                m_displayCoroutine = StartCoroutine (DisplayText (dialogueLines, gatherChoices));
            };
        }

        public void PlayDialogueLines (InkDialogueLine[] targetLines) { // just provide the lines directly
            if (m_storyData.IsLoaded ()) {
                if (m_displayCoroutine != null) {
                    StopCoroutine (m_displayCoroutine);
                } else {
                    m_writerStartedEvent.Invoke (this); // We invoke the started event when starting a whole new thing
                }
                m_displayCoroutine = StartCoroutine (DisplayText (targetLines, null));
            };
        }
        // Where most of the magic happens: takes the line of dialogue + possible expected choices, displays them one by one by spawning text objects
        // And then displayes the options by spawning buttons
        IEnumerator DisplayText (InkDialogueLine[] dialogueLines, List<InkChoiceLine> gatherChoices) {
            if (m_currentDialogBox.HasContinueButton) {
                m_currentDialogBox.SetContinueButtonActive (true);
                m_currentDialogBox.m_canContinue = false;
            } else {
                m_currentDialogBox.m_canContinue = true;
            }
            for (int i = 0; i < dialogueLines.Length; i++) {
                InkDialogueLine currentLine = dialogueLines[i];
                InvokeDialogueEvents (currentLine);
                yield return StartCoroutine (ParseSpecialTags (currentLine.inkTags));
                m_currentDialogBox.SpawnTextObject (currentLine.displayText);
                m_dialogueShownEvent.Invoke (currentLine);
                yield return new WaitUntil (() => m_currentDialogBox.m_canContinue);
                if (m_currentDialogBox.HasContinueButton) {
                    m_currentDialogBox.m_canContinue = false;
                }
            }
            if (gatherChoices != null) {
                if (gatherChoices.Count > 0) {
                    List < (InkChoiceLine, Button) > allButtons = new List < (InkChoiceLine, Button) > { };
                    foreach (InkChoiceLine choice in gatherChoices) {
                        GameObject buttonGO = m_currentDialogBox.SpawnButtonObject (choice.choiceText.displayText);
                        Button button = buttonGO.GetComponent<Button> ();
                        button.interactable = SetChoiceInteractable (choice.choiceText);
                        allButtons.Add ((choice, button));
                    }
                    foreach ((InkChoiceLine, Button) set in allButtons) {
                        set.Item2.onClick.AddListener (() => PressOptionButton (set.Item1, set.Item2, allButtons));
                    }
                    m_optionPressed = false;
                    m_waitingOnOptionPress = true;
                    m_currentDialogBox.SetContinueButtonActive (false);
                    m_choicesShownEvent.Invoke (allButtons);
                    yield return new WaitUntil (() => m_optionPressed);
                } else { // we only invoke the writer finished event if there really are no more choices
                    m_writerFinishedEvent.Invoke (this);
                    m_displayCoroutine = null;
                }
            } else { // we only invoke the writer finished event if there really are no more choices
                m_writerFinishedEvent.Invoke (this);
                m_displayCoroutine = null;
            };
        }

        void PressOptionButton (InkChoiceLine optionButton, Button selectedButton, List < (InkChoiceLine, Button) > allButtons) {
            // We only press one
            m_optionPressed = true;
            m_waitingOnOptionPress = false;
            InvokeDialogueEvents (optionButton.choiceText);
            PlayChoice (optionButton.choice);
            // Make the selected button uninteractable but stay behind, hide all the others
            selectedButton.interactable = false;
            foreach ((InkChoiceLine, Button) set in allButtons) {
                if (set.Item2 != selectedButton) {
                    set.Item2.gameObject.SetActive (false);
                }
            }
        }

        void InvokeDialogueEvents (InkDialogueLine currentLine) {
            if (currentLine.inkVariables.Count > 0) {
                foreach (InkTextVariable variable in currentLine.inkVariables) {
                    m_textFunctionFoundEvent.Invoke (currentLine, variable);
                    Debug.Log ("Invoked ink function: " + variable.variableName + "(" + variable.VariableArguments + ")");
                }
            }
            if (currentLine.inkTags.Count > 0) {
                foreach (string tag in currentLine.inkTags) {
                    m_inkTagFoundEvent.Invoke (currentLine, tag);
                }
            }
        }

        IEnumerator ParseSpecialTags (List<string> tags) {
            // These are just hard-coded Ink tags we might want to use for various things to do with the writer...
            if (tags.Contains ("hideDialogue")) {
                CloseCurrentDialogBox (false);
            }
            if (tags.Contains ("showDialogue")) {
                OpenCurrentDialogBox (false);
            }
            if (tags.Contains ("continue")) {
                m_currentDialogBox.m_canContinue = true;
            }
            foreach (string tag in tags) {
                if (tag.Contains ("wait.")) {
                    string tagNr = tag.Replace ("wait.", ""); // remove the wait
                    float waitTime = float.Parse (tagNr);
                    yield return new WaitForSeconds (waitTime);
                }
            }
            // Keep this one at the bottom, or it will pause before the other tags!
            if (tags.Contains ("pause")) {
                PauseWriter (true);
                yield return new WaitUntil (() => m_currentDialogBox.m_canContinue);
            }
        }

        public bool SetChoiceInteractable (InkDialogueLine choiceLine) {
            // Hard coded thing to set a button to interactable or not...
            if (choiceLine.HasVariableWithArgument ("INTERACTABLE", "false")) {
                return false;
            }
            return true;
        }

        public void PauseWriter (bool pause = true) {
            if (pause) {
                if (m_displayCoroutine != null) {
                    m_currentDialogBox.m_canContinue = false;
                    m_currentDialogBox.SetContinueButtonActive (false);
                }
            } else {
                if (m_displayCoroutine != null) {
                    m_currentDialogBox.m_canContinue = true;
                    if (!m_waitingOnOptionPress) {
                        m_currentDialogBox.SetContinueButtonActive (true);
                    };
                }
            }
        }

        public void CloseCurrentDialogBox (bool clear = true) {
            if (clear) {
                m_currentDialogBox.ClearAllText ();
                m_currentDialogBox.ClearAllOptions ();
            }
            m_currentDialogBox.Active = false;
        }
        public void OpenCurrentDialogBox (bool clear = true) {
            if (clear) {
                m_currentDialogBox.ClearAllText ();
                m_currentDialogBox.ClearAllOptions ();
            }
            m_currentDialogBox.Active = true;
        }
        public void ChangeCurrentDialogBox (SimpleInkDialogBox newBox, bool closeOld = true, bool openNew = true) {
            if (closeOld) {
                CloseCurrentDialogBox ();
            }
            m_currentDialogBox = newBox;
            if (openNew) {
                OpenCurrentDialogBox ();
            }
        }
        public void ChangeCurrentDialogBox (SimpleInkDialogBox newBox) { // for unity events ;)
            ChangeCurrentDialogBox (newBox, true, true);
        }
    }
}