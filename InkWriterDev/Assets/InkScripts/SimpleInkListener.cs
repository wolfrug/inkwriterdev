using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InkEngine {

    public class SimpleInkListener : InkListener {
        // Use this one if you want to target and listen to events only from a specific writer, instead of globally
        public InkWriter m_targetWriter;
        private Dictionary<string, List<InkFunctionEvent>> m_inkFunctionDict = new Dictionary<string, List<InkFunctionEvent>> { };

        public override void Start () {
            foreach (InkFunctionEvent evt in m_functionEvents) { // adds the ones added in the editor initially
                AddNewFunctionEvent (evt);
            }
            if (m_targetWriter == null) {
                Debug.LogError ("No writer assigned to listener! Please assign a writer to it in the editor.");
            }
            m_targetWriter.m_textFunctionFoundEvent.AddListener (OnFunctionEvent);
            m_targetWriter.m_inkTagFoundEvent.AddListener (OnTagEvent);
        }
    }
}