using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InkEngine {
    public class SimpleInkWriter : InkWriter {
        private SimpleInkDialogBox m_simpleDialogBox;
        public override void Awake () {
            base.Awake ();
            // Add the default searchable INTERACTABLE tag - used by choices to make them inactive if this function tag is found
            m_storyData.AddSearchableFunction (new InkTextVariable {
                variableName = "INTERACTABLE",
                    VariableArguments = new List<string> { "false" }
            });
            m_simpleDialogBox = m_currentDialogBox as SimpleInkDialogBox;
        }

        public SimpleInkDialogBox CurrentDialogBoxSimple {
            get {
                return m_simpleDialogBox;
            }
            set {
                m_simpleDialogBox = value;
            }
        }
        // Where most of the magic happens: takes the line of dialogue + possible expected choices, displays them one by one by spawning text objects
        // And then displayes the options by spawning buttons
        public override IEnumerator DisplayText (InkDialogueLine[] dialogueLines, List<InkChoiceLine> gatherChoices) {
            if (CurrentDialogBoxSimple.HasContinueButton) {
                CurrentDialogBoxSimple.SetContinueButtonActive (true);
                CurrentDialogBoxSimple.m_canContinue = false;
            } else {
                CurrentDialogBoxSimple.m_canContinue = true;
            }
            for (int i = 0; i < dialogueLines.Length; i++) {
                InkDialogueLine currentLine = dialogueLines[i];
                InvokeDialogueEvents (currentLine);
                yield return StartCoroutine (ParseSpecialTags (currentLine.inkTags));
                if (currentLine.displayText.Trim () != "") {
                    SpawnTextObject (currentLine);
                    m_dialogueShownEvent.Invoke (currentLine);
                };
                yield return new WaitUntil (() => CurrentDialogBoxSimple.m_canContinue);
                if (CurrentDialogBoxSimple.HasContinueButton) {
                    CurrentDialogBoxSimple.m_canContinue = false;
                }
            }
            if (gatherChoices != null) {
                if (gatherChoices.Count > 0) {
                    List < (InkChoiceLine, Button) > allButtons = new List < (InkChoiceLine, Button) > { };
                    foreach (InkChoiceLine choice in gatherChoices) {
                        Button button = SpawnOptionObject (choice);
                        button.interactable = SetChoiceInteractable (choice.choiceText);
                        allButtons.Add ((choice, button));
                    }
                    foreach ((InkChoiceLine, Button) set in allButtons) {
                        set.Item2.onClick.AddListener (() => PressOptionButton (set.Item1, set.Item2, allButtons));
                    }
                    m_optionPressed = false;
                    m_waitingOnOptionPress = true;
                    CurrentDialogBoxSimple.SetContinueButtonActive (false);
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

        IEnumerator ParseSpecialTags (List<string> tags) {
            // These are just hard-coded Ink tags we might want to use for various things to do with the writer...
            if (tags.Contains ("hideDialogue")) {
                CloseCurrentDialogBox (false);
            }
            if (tags.Contains ("showDialogue")) {
                OpenCurrentDialogBox (false);
            }
            if (tags.Contains ("continue")) {
                CurrentDialogBoxSimple.m_canContinue = true;
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
                yield return new WaitUntil (() => CurrentDialogBoxSimple.m_canContinue);
            }
        }

        bool SetChoiceInteractable (InkDialogueLine choiceLine) {
            // Hard coded thing to set a button to interactable or not...
            if (choiceLine.HasVariableWithArgument ("INTERACTABLE", "false")) {
                return false;
            }
            return true;
        }
        protected override void PressOptionButton (InkChoiceLine optionButton, Button selectedButton, List < (InkChoiceLine, Button) > allButtons) {
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
        public void PauseWriter (bool pause = true) {
            if (pause) {
                if (m_displayCoroutine != null) {
                    CurrentDialogBoxSimple.m_canContinue = false;
                    CurrentDialogBoxSimple.SetContinueButtonActive (false);
                }
            } else {
                if (m_displayCoroutine != null) {
                    CurrentDialogBoxSimple.m_canContinue = true;
                    if (!m_waitingOnOptionPress) {
                        CurrentDialogBoxSimple.SetContinueButtonActive (true);
                    };
                }
            }
        }

        void SpawnTextObject (InkDialogueLine currentLine) {
            string boxPrefab = "default";
            if (currentLine.HasVariable ("SET_TEXTBOX")) {
                boxPrefab = currentLine.GetVariable ("SET_TEXTBOX").VariableArguments[0];
            }
            CurrentDialogBoxSimple.SpawnTextObject (currentLine.displayText, boxPrefab);
        }
        Button SpawnOptionObject (InkChoiceLine choice) {
            string optionButtonPrefab = "default";
            if (choice.choiceText.HasVariable ("SET_OPTIONBOX")) {
                optionButtonPrefab = choice.choiceText.GetVariable ("SET_OPTIONBOX").VariableArguments[0];
            }
            GameObject buttonGO = CurrentDialogBoxSimple.SpawnButtonObject (choice.choiceText.displayText, optionButtonPrefab);
            if (buttonGO.GetComponent<InkEngine.InkOptionButton> () != null) {
                buttonGO.GetComponent<InkEngine.InkOptionButton> ().SetDialogueLine (choice.choiceText);
            }
            Button button = buttonGO.GetComponent<Button> ();
            return button;
        }

        public void CloseCurrentDialogBox (bool clear = true) {
            if (clear) {
                CurrentDialogBoxSimple.ClearAllText ();
                CurrentDialogBoxSimple.ClearAllOptions ();
            }
            CurrentDialogBoxSimple.Active = false;
        }
        public void OpenCurrentDialogBox (bool clear = true) {
            if (clear) {
                CurrentDialogBoxSimple.ClearAllText ();
                CurrentDialogBoxSimple.ClearAllOptions ();
            }
            CurrentDialogBoxSimple.Active = true;
        }
        public void ChangeCurrentDialogBox (SimpleInkDialogBox newBox, bool closeOld = true, bool openNew = true) {
            if (closeOld) {
                CloseCurrentDialogBox ();
            }
            CurrentDialogBoxSimple = newBox;
            if (openNew) {
                OpenCurrentDialogBox ();
            }
        }
        public void ChangeCurrentDialogBox (SimpleInkDialogBox newBox) {
            ChangeCurrentDialogBox (newBox, true, true);
        }
    }
}