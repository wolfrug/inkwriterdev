using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InkEngine {

    public enum ArgumentRequirements {
        ANY_OF = 0000, // any of the given arguments
        EXACTLY = 0001, // exactly these arguments
        NONE_OF = 0002, // none of these arguments (but others allowed)
        ALL = 0003, // as long as it's the right variable, it is ok
    }

    [System.Serializable]
    public class InkFunctionEvent { // these are called when an ink function is found in the -currently displayed- string
        public string targetVariable;
        public string[] targetArguments;
        public ArgumentRequirements argumentRequirement;
        public TextFunctionFoundEvent onEvent;

        public bool ArgumentMatch (List<string> arguments) {
            if (argumentRequirement == ArgumentRequirements.ANY_OF) {
                foreach (string trg in targetArguments) {
                    if (arguments.Contains (trg)) {
                        return true;
                    }
                }
                return false;
            }
            if (argumentRequirement == ArgumentRequirements.EXACTLY) {
                if (targetArguments.Length != arguments.Count) {
                    return false;
                }
                foreach (string trg in targetArguments) {
                    if (!arguments.Contains (trg)) {
                        return false;
                    }
                }
                return true;
            }
            if (argumentRequirement == ArgumentRequirements.NONE_OF) {
                foreach (string trg in targetArguments) {
                    if (arguments.Contains (trg)) {
                        return false;
                    }
                }
                return true;
            }
            if (argumentRequirement == ArgumentRequirements.ALL) {
                return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public class InkTagEvent { // this is called when a tag is found in the currently displayed string
        public string targetTag;
        public TextTagFoundEvent onEvent;
    }
    public class SimpleInkListener : MonoBehaviour {
        public SimpleInkWriter m_listenTarget;
        public List<InkFunctionEvent> m_functionEvents = new List<InkFunctionEvent> { };
        public List<InkTagEvent> m_tagEvents = new List<InkTagEvent> { };

        private Dictionary<string, List<InkFunctionEvent>> m_inkFunctionDict = new Dictionary<string, List<InkFunctionEvent>> { };

        void Start () {
            foreach (InkFunctionEvent evt in m_functionEvents) { // adds the ones added in the editor initially
                AddNewFunctionEvent (evt);
            }
            if (m_listenTarget == null) {
                Debug.LogError ("No writer assigned to listener! Please assign a writer to it in the editor.");
            }
            m_listenTarget.m_textFunctionFoundEvent.AddListener (OnFunctionEvent);
            m_listenTarget.m_inkTagFoundEvent.AddListener (OnTagEvent);
        }

        public void AddNewFunctionEvent (InkFunctionEvent evt) {
            if (!m_functionEvents.Contains (evt)) {
                m_functionEvents.Add (evt);
            }
            if (m_inkFunctionDict.ContainsKey (evt.targetVariable)) {
                m_inkFunctionDict[evt.targetVariable].Add (evt);
            } else {
                m_inkFunctionDict.Add (evt.targetVariable, new List<InkFunctionEvent> { evt });
            }
        }

        void OnFunctionEvent (InkDialogueLine dialogueLine, InkTextVariable variable) {
            List<InkFunctionEvent> functionEvents = new List<InkFunctionEvent> { };
            m_inkFunctionDict.TryGetValue (variable.variableName, out functionEvents);
            if (functionEvents != null) {
                if (functionEvents.Count > 0) {
                    foreach (InkFunctionEvent evt in functionEvents) {
                        if (evt.ArgumentMatch (variable.VariableArguments)) {
                            evt.onEvent.Invoke (dialogueLine, variable);
                        }
                    }
                }
            }
        }
        void OnTagEvent (InkDialogueLine dialogueLine, string tag) {
            foreach (InkTagEvent evt in m_tagEvents) {
                if (evt.targetTag == tag) {
                    evt.onEvent.Invoke (dialogueLine, tag);
                }
            }
        }

    }
}