using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InkEngine {

    [System.Serializable]
    public class DialoguePresentedEvent : UnityEvent<InkDialogueLine> { }

    [System.Serializable]
    public class DialogueOptionsPresentedEvent : UnityEvent < List < (InkChoiceLine, Button) >> { }

    [System.Serializable]
    public class WriterEvent : UnityEvent<InkWriter> { }
    public class InkWriter : MonoBehaviour // Base class for Ink Writers, with only the core components
    {
        public InkStoryData m_storyData;
        [Tooltip ("If you want this writer to use a separate story flow, change this string")]
        public string m_storyFlow = "default";
        public InkDialogBox m_currentDialogBox;
        [Tooltip ("For specific text functions, e.g. PLAYER(sad, right)")]
        public TextFunctionFoundEvent m_textFunctionFoundEvent;
        [Tooltip ("For regular ink tags, e.g. #playmusic (do not include the hashtag)")]
        public TextTagFoundEvent m_inkTagFoundEvent;
        public DialoguePresentedEvent m_dialogueShownEvent;
        public DialogueOptionsPresentedEvent m_choicesShownEvent;
        public WriterEvent m_writerFinishedEvent;
        public WriterEvent m_writerStartedEvent;
        protected bool m_optionPressed = false;
        protected bool m_waitingOnOptionPress = false;
        protected Coroutine m_displayCoroutine = null;
        public virtual void Awake () {
            if (m_storyData == null) {
                m_storyData = Resources.LoadAll<InkStoryData> ("InkStoryData") [0];
            }
        }

        public virtual InkDialogBox CurrentDialogBox {
            get {
                return m_currentDialogBox;
            }
            set {
                m_currentDialogBox = value;
            }
        }

        public virtual void PlayKnot (string knotName) { // play directly from a knot
            if (m_storyData.IsLoaded ()) {
                List<InkChoiceLine> gatherChoices = new List<InkChoiceLine> { };
                InkDialogueLine[] dialogueLines = m_storyData.CreateStringArrayKnot (knotName, gatherChoices, m_storyFlow);
                if (m_displayCoroutine != null) {
                    StopCoroutine (m_displayCoroutine);
                } else {
                    m_writerStartedEvent.Invoke (this);
                }
                m_displayCoroutine = StartCoroutine (DisplayText (dialogueLines, gatherChoices));
            };
        }
        public virtual void PlayChoice (Choice choice) { // play from a choice - mainly used internally
            if (m_storyData.IsLoaded ()) {
                List<InkChoiceLine> gatherChoices = new List<InkChoiceLine> { };
                InkDialogueLine[] dialogueLines = m_storyData.CreateStringArrayChoice (choice, gatherChoices, m_storyFlow);
                if (m_displayCoroutine != null) {
                    StopCoroutine (m_displayCoroutine);
                } else {
                    m_writerStartedEvent.Invoke (this); // We invoke the started event when starting a whole new thing
                }
                m_displayCoroutine = StartCoroutine (DisplayText (dialogueLines, gatherChoices));
            };
        }

        public virtual void PlayDialogueLines (InkDialogueLine[] targetLines) { // just provide the lines directly
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
        public virtual IEnumerator DisplayText (InkDialogueLine[] dialogueLines, List<InkChoiceLine> gatherChoices) {
            for (int i = 0; i < dialogueLines.Length; i++) {
                InkDialogueLine currentLine = dialogueLines[i];
                InvokeDialogueEvents (currentLine);
                if (currentLine.displayText.Trim() != "") { // Do not spawn a text object if there is no displayable text to display
                    CurrentDialogBox.SpawnTextObject (currentLine.displayText);
                }
                m_dialogueShownEvent.Invoke (currentLine);
            }
            if (gatherChoices != null) {
                if (gatherChoices.Count > 0) {
                    List < (InkChoiceLine, Button) > allButtons = new List < (InkChoiceLine, Button) > { };
                    foreach (InkChoiceLine choice in gatherChoices) {
                        GameObject buttonGO = CurrentDialogBox.SpawnButtonObject (choice.choiceText.displayText);
                        Button button = buttonGO.GetComponent<Button> ();
                        allButtons.Add ((choice, button));
                    }
                    foreach ((InkChoiceLine, Button) set in allButtons) {
                        set.Item2.onClick.AddListener (() => PressOptionButton (set.Item1, set.Item2, allButtons));
                    }
                    m_optionPressed = false;
                    m_waitingOnOptionPress = true;
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
        protected virtual void PressOptionButton (InkChoiceLine optionButton, Button selectedButton, List < (InkChoiceLine, Button) > allButtons) {
            // We only press one
            m_optionPressed = true;
            m_waitingOnOptionPress = false;
            InvokeDialogueEvents (optionButton.choiceText);
            PlayChoice (optionButton.choice);
            foreach ((InkChoiceLine, Button) set in allButtons) {
                set.Item2.gameObject.SetActive (false);
            }
        }

        protected virtual void InvokeDialogueEvents (InkDialogueLine currentLine) {
            if (currentLine.inkVariables.Count > 0) {
                foreach (InkTextVariable variable in currentLine.inkVariables) {
                    m_textFunctionFoundEvent.Invoke (currentLine, variable);
                    m_storyData.InvokeFunctionEvent(currentLine, variable);
                    Debug.Log ("Invoked ink function: " + variable.variableName + "(" + string.Join ("\n", variable.VariableArguments) + ")");
                }
            }
            if (currentLine.inkTags.Count > 0) {
                foreach (string tag in currentLine.inkTags) {
                    m_storyData.InvokeTagEvent(currentLine, tag);
                    m_inkTagFoundEvent.Invoke (currentLine, tag);
                }
            }
        }
    }
}