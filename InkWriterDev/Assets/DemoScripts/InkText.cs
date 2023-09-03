using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InkEngine {
    [RequireComponent (typeof (TextMeshProUGUI))]
    public class InkText : MonoBehaviour {
        public InkStoryData m_storyData;
        [Tooltip ("The specificed knot where the text should be taken from. Can use stitches, so stringtable.desc is fine")]
        public string m_targetKnot;
        public string m_lineBreak = "\n";
        public bool m_updateOnEnable = true;
        public bool m_sendGlobalFunctionEvents = true;

        [Tooltip ("For specific text functions, e.g. PLAYER(sad, right)")]
        public TextFunctionFoundEvent m_textFunctionFoundEvent;

        [Tooltip ("For regular ink tags, e.g. #playmusic (do not include the hashtag)")]
        public TextTagFoundEvent m_inkTagFoundEvent;
        private TextMeshProUGUI m_targetText;
        void Awake () {
            if (m_storyData == null) {
                m_storyData = Resources.LoadAll<InkStoryData> ("InkStoryData") [0];
            }
            m_targetText = GetComponent<TextMeshProUGUI> ();
            //UpdateSelf ();
        }

        public void UpdateSelf () {
            string finalString = "";
            InkDialogueLine[] text = m_storyData.CreateStringArrayKnot (m_targetKnot, null, "stringTable");
            for (int i = 0; i < text.Length; i++) {
                InkDialogueLine line = text[i];
                finalString += line.displayText;
                if (i < text.Length) {
                    finalString += m_lineBreak;
                }
                InvokeDialogueEvents (line);
            }
            m_targetText.SetText (finalString);
        }
        void OnEnable () {
            if (m_updateOnEnable) {
                UpdateSelf ();
            };
        }

        void InvokeDialogueEvents (InkDialogueLine currentLine) {
            if (currentLine.inkVariables.Count > 0) {
                foreach (InkTextVariable variable in currentLine.inkVariables) {
                    m_textFunctionFoundEvent.Invoke (currentLine, variable);
                    if (m_sendGlobalFunctionEvents) {
                        m_storyData.m_textFunctionFoundEvent.Invoke (currentLine, variable);
                    }
                    Debug.Log ("Invoked ink function: " + variable.variableName + "(" + string.Join ("\n", variable.VariableArguments) + ")");
                }
            }
            if (currentLine.inkTags.Count > 0) {
                foreach (string tag in currentLine.inkTags) {
                    m_inkTagFoundEvent.Invoke (currentLine, tag);
                    if (m_sendGlobalFunctionEvents) {
                        m_storyData.m_inkTagFoundEvent.Invoke (currentLine, tag);
                    }
                }
            }
        }
    }
}