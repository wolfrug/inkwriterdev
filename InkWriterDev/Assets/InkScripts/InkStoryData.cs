using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace InkEngine {

    [System.Serializable]
    public class TextTagFoundEvent : UnityEvent<InkDialogueLine, string> { }

    [System.Serializable]
    public class TextFunctionFoundEvent : UnityEvent<InkDialogueLine, InkTextVariable> { }

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

    [CreateAssetMenu (fileName = "Data", menuName = "InkEngine/Ink Story Object", order = 1)]
    public class InkStoryData : ScriptableObject {

        public string m_ID = "default";
        public TextAsset m_inkJsonAsset;
        private Story m_inkStory = null;

        public InkStoryVariableData m_defaultTextVariables; // a scriptable object with the default ones, for quick lookup
        public List<InkTextVariable> m_searchableTextVariables = new List<InkTextVariable> { }; // which text variables we are searching for & parsing

        [Tooltip ("For specific text functions, e.g. PLAYER(sad, right)")]
        [HideInInspector] public TextFunctionFoundEvent m_textFunctionFoundEvent;
        [Tooltip ("For regular ink tags, e.g. #playmusic (do not include the hashtag)")]
        [HideInInspector] public TextTagFoundEvent m_inkTagFoundEvent;

        public Story InkStory {
            get {

                if (m_inkStory != null) {
                    return m_inkStory;
                } else {
                    InitStory ();
                    return m_inkStory;
                }
            }
            set {
                m_inkStory = value;
            }
        }

        public void SaveStory () {
            string json = InkStory.state.ToJson ();
            PlayerPrefs.SetString ("InkWriter_ExampleSave", json);
        }

        public void InitStory () { // inits a story by either loading it or starting a new one
            if (m_inkStory == null) {
                InkStory = new Story (m_inkJsonAsset.text);
            };
            if (SavedStory) { // We load if we can
                string savedJson = PlayerPrefs.GetString ("InkWriter_ExampleSave");
                InkStory.state.LoadJson (savedJson);
                Debug.Log ("Loaded story state");
            } else { // If we can't, we start from beginning
                Debug.Log ("No saved story found - starting new story");
            }
            if (m_defaultTextVariables != null) {
                foreach (InkTextVariable el in m_defaultTextVariables.m_variables) {
                    AddSearchableFunction (el);
                }
            }
        }

        public void ClearSavedStory () {
            PlayerPrefs.DeleteKey ("InkWriter_ExampleSave");
        }

        public bool SavedStory {
            get {
                return PlayerPrefs.HasKey ("InkWriter_ExampleSave");
            }
        }
        public bool IsLoaded () {
            return InkStory != null;
        }

        public void AddSearchableFunction (InkTextVariable newVariable) {
            if (m_searchableTextVariables.FindAll ((x) => x.variableName == newVariable.variableName).Count == 0) { // Only add if it hasn't been added already
                m_searchableTextVariables.Add (newVariable);
            };
        }

        // Creates a string array of all the strings in a specific knot, and optionally the choices at the end
        // Note: create the List<Choice> when calling this and the list will be automagically modified (we don't return a new list)
        public InkDialogueLine[] CreateStringArrayKnot (string targetKnot, List<InkChoiceLine> gatherChoices, string optionalFlow = "default") {
            if (optionalFlow != "default") {
                InkStory.SwitchFlow (optionalFlow);
            }
            InkStory.ChoosePathString (targetKnot);
            InkDialogueLine[] returnArray = CreateDialogueArray (optionalFlow);
            if (gatherChoices != null) {
                foreach (Choice choice in InkStory.currentChoices) {
                    gatherChoices.Add (ParseInkChoice (choice));
                    //Debug.Log ("Added end choice with name: " + choice.text);
                }
            }
            if (optionalFlow != "default") {
                InkStory.RemoveFlow (optionalFlow);
            }
            return returnArray;
        }

        // Creates a list of strings starting from a choice, and then optionally gathers the choices
        public InkDialogueLine[] CreateStringArrayChoice (Choice startChoice, List<InkChoiceLine> gatherChoices, string optionalFlow = "default") {
            if (optionalFlow != "default") {
                InkStory.SwitchFlow (optionalFlow);
            }
            if (InkStory.currentChoices.Contains (startChoice)) {
                InkStory.ChooseChoiceIndex (startChoice.index);
            } else {
                Debug.LogWarning ("Tried to choose a choice that is no longer among the Inkwriter's available choices!");
                InkStory.ChoosePath (startChoice.targetPath);
            }
            InkDialogueLine[] returnArray = CreateDialogueArray (optionalFlow);
            if (gatherChoices != null) {
                foreach (Choice choice in InkStory.currentChoices) {
                    gatherChoices.Add (ParseInkChoice (choice));
                    //Debug.Log ("Added end choice with name: " + choice.text);
                }
            }
            if (optionalFlow != "default") {
                InkStory.RemoveFlow (optionalFlow);
            }
            return returnArray;
        }

        // where the magic happens - actually goes through the ink file and "continues" things
        InkDialogueLine[] CreateDialogueArray (string targetFlow = "default") {
            string returnText = "";
            List<InkDialogueLine> returnArray = new List<InkDialogueLine> { };
            while (InkStory.canContinue) {
                returnText = InkStory.Continue ();
                InkDialogueLine newLine = ParseInkText (returnText);
                if (InkStory.currentTags.Count > 0) {
                    newLine.inkTags.AddRange (new List<string> (InkStory.currentTags));
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

        public void InvokeFunctionEvent(InkDialogueLine currentLine, InkTextVariable variable){
            Debug.Log ("Invoked global ink function event: " + variable.variableName + "(" + string.Join ("\n", variable.VariableArguments) + ")");
            m_textFunctionFoundEvent.Invoke(currentLine, variable);
        }
        public void InvokeTagEvent(InkDialogueLine currentLine, string tag){
            Debug.Log("Invoked global tag event: " + tag);
            m_inkTagFoundEvent.Invoke(currentLine, tag);
        }
    }
}