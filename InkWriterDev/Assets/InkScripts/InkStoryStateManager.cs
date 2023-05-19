using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ink.Runtime;
using UnityEngine;

namespace InkEngine {

    [System.Serializable]
    public class InkTextVariable {
        // This represents a text variable found (and removed) from a piece of ink text
        public string variableName; // the name of the variable, e.g. PLAYER
        private List<string> variableArguments = new List<string> { }; // any potential variable arguments, e.g. PLAYER(sad, left)
        public List<string> VariableArguments {
            get {
                return variableArguments;
            }
            set {
                variableArguments.Clear ();
                variableArguments.AddRange (value);
            }
        }
        public bool HasArgument (string argument) {
            return variableArguments.Contains (argument);
        }
    }

    [System.Serializable]
    public class InkDialogueLine {
        // This is a piece of data that contains all the information needed from a piece of ink dialogue - arrays of these are used to display longer dialogues
        public string text_unmodified; // the unmodified text
        public string displayText; // displayable text, with string variables removed
        public List<string> inkTags = new List<string> { }; // ink tags found in text
        public List<InkTextVariable> inkVariables = new List<InkTextVariable> { }; // ink variables found in text

        // Just some helpful functions
        public bool HasArgument (string argument) {
            foreach (InkTextVariable inkVariable in inkVariables) {
                if (inkVariable.HasArgument (argument)) {
                    return true;
                }
            }
            return false;
        }
        public bool HasVariable (string variable) {
            foreach (InkTextVariable inkVariable in inkVariables) {
                if (inkVariable.variableName == variable) {
                    return true;
                }
            }
            return false;
        }

        public bool HasVariableWithArgument (string variable, string argument) {
            if (!HasVariable (variable)) {
                return false;
            }
            foreach (InkTextVariable inkVariable in inkVariables) {
                if (inkVariable.variableName == variable) {
                    if (inkVariable.HasArgument (argument)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public InkTextVariable GetVariable (string variableName) {
            if (!HasVariable (variableName)) {
                return null;
            }
            return inkVariables.Find ((x) => x.variableName == variableName);
        }
        public InkTextVariable GetVariableWithArgument (string variable, string argument) {
            foreach (InkTextVariable inkVariable in inkVariables) {
                if (inkVariable.variableName == variable) {
                    if (inkVariable.HasArgument (argument)) {
                        return inkVariable;
                    }
                }
            }
            return null;
        }
    }

    [System.Serializable]
    public class InkChoiceLine {
        public Choice choice; // the choice itself
        public InkDialogueLine choiceText; // the ink dialogue line with commands, tags etc
    }

    public class InkStoryStateManager : MonoBehaviour {
        public InkStoryData m_storyObject;
        public InkStoryVariableData m_defaultTextVariables; // a scriptable object with the default ones, for quick lookup
        public List<InkTextVariable> m_searchableTextVariables = new List<InkTextVariable> { }; // which text variables we are searching for & parsing
        public bool m_startOnInit = true;

        void Awake () {
            if (m_startOnInit) {
                InitStory ();
            }
            if (m_defaultTextVariables != null) {
                foreach (InkTextVariable el in m_defaultTextVariables.m_variables) {
                    AddSearchableFunction (el);
                }
            }
        }
        public void SaveStory () {
            m_storyObject.SaveStory ();
        }
        public void InitStory () { // Init -or- load
            m_storyObject.InitStory ();
        }
        public bool SavedStory {
            get {
                return m_storyObject.SavedStory;
            }
        }
        public bool IsLoaded () {
            return m_storyObject.IsLoaded ();
        }

        public void AddSearchableFunction (InkTextVariable newVariable) {
            if (m_searchableTextVariables.FindAll ((x) => x.variableName == newVariable.variableName).Count == 0) { // Only add if it hasn't been added already
                m_searchableTextVariables.Add (newVariable);
            };
        }

        // Creates a string array of all the strings in a specific knot, and optionally the choices at the end
        // Note: create the List<Choice> when calling this and the list will be automagically modified (we don't return a new list)
        public InkDialogueLine[] CreateStringArrayKnot (string targetKnot, List<InkChoiceLine> gatherChoices) {
            m_storyObject.m_inkStory.ChoosePathString (targetKnot);
            InkDialogueLine[] returnArray = CreateDialogueArray ();
            if (gatherChoices != null) {
                foreach (Choice choice in m_storyObject.m_inkStory.currentChoices) {
                    gatherChoices.Add (ParseInkChoice (choice));
                    //Debug.Log ("Added end choice with name: " + choice.text);
                }
            }
            return returnArray;
        }

        // Creates a list of strings starting from a choice, and then optionally gathers the choices
        public InkDialogueLine[] CreateStringArrayChoice (Choice startChoice, List<InkChoiceLine> gatherChoices) {
            if (m_storyObject.m_inkStory.currentChoices.Contains (startChoice)) {
                m_storyObject.m_inkStory.ChooseChoiceIndex (startChoice.index);
            } else {
                Debug.LogWarning ("Tried to choose a choice that is no longer among the Inkwriter's available choices!");
                m_storyObject.m_inkStory.ChoosePath (startChoice.targetPath);
            }
            InkDialogueLine[] returnArray = CreateDialogueArray ();
            if (gatherChoices != null) {
                foreach (Choice choice in m_storyObject.m_inkStory.currentChoices) {
                    gatherChoices.Add (ParseInkChoice (choice));
                    //Debug.Log ("Added end choice with name: " + choice.text);
                }
            }
            return returnArray;
        }

        // where the magic happens - actually goes through the ink file and "continues" things
        InkDialogueLine[] CreateDialogueArray () {
            string returnText = "";
            List<InkDialogueLine> returnArray = new List<InkDialogueLine> { };
            while (m_storyObject.m_inkStory.canContinue) {
                returnText = m_storyObject.m_inkStory.Continue ();
                InkDialogueLine newLine = ParseInkText (returnText);
                if (m_storyObject.m_inkStory.currentTags.Count > 0) {
                    newLine.inkTags.AddRange (new List<string> (m_storyObject.m_inkStory.currentTags));
                };
                returnArray.Add (newLine);
            }
            return returnArray.ToArray ();
        }

        // this creates an actual ink dialogue line by parsing out any functions, gathering tags and creating a 'displayable' text line
        public InkDialogueLine ParseInkText (string inkContinueText) {
            // Parses ink text for any defined text tags and creates a proper dialogue line object
            InkDialogueLine returnLine = new InkDialogueLine ();

            // Creates the list of text tags for the regex
            string regexList = "";
            foreach (InkTextVariable textPiece in m_searchableTextVariables) {
                regexList += textPiece.variableName + "|";
            }
            regexList = regexList.Remove (regexList.Length  -  1,  1); // remove last |

            string input = inkContinueText;
            string pattern = @"\b(?:(?:" + regexList + @")\((?:[^()]|(?<open>\()|(?<-open>\)))*(?(open)(?!))\))";
            string cleanedText = input;

            // Creates a list where the first entry is e.g. PLAYER, and subsequent entries are variables
            List<List<string>> substrings = new List<List<string>> ();
            foreach (Match match in Regex.Matches (input, pattern)) {
                // Clean the text from the match
                cleanedText = cleanedText.Replace (match.Value, "");
                InkTextVariable newVariable = new InkTextVariable ();
                string innerPattern = @"^([^(]*)\(([^)]*)\)$";
                Match innerMatch = Regex.Match (match.Value, innerPattern);
                // Group 0 = original match
                // Group 1 = variable name
                newVariable.variableName = innerMatch.Groups[1].Value;
                //Debug.Log ("Variable name: " + newVariable.variableName);
                // Group 2 = arguments
                if (innerMatch.Groups.Count > 2) {
                    foreach (string splitString in innerMatch.Groups[2].Value.Split (',')) {
                        newVariable.VariableArguments.Add (splitString.Trim ());
                        //Debug.Log ("Argument: " + newVariable.VariableArguments[newVariable.VariableArguments.Count - 1]);
                    }
                }
                returnLine.inkVariables.Add (newVariable);
            }
            returnLine.text_unmodified = inkContinueText;
            returnLine.displayText = cleanedText;
            //Debug.Log ("Cleaned text: " + cleanedText);

            return returnLine;
        }

        public InkChoiceLine ParseInkChoice (Choice newChoice) {
            InkChoiceLine returnVal = new InkChoiceLine ();
            returnVal.choice = newChoice;
            returnVal.choiceText = ParseInkText (newChoice.text);
            return returnVal;
        }
    }
}