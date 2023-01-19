using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

namespace InkEngine {

    [CreateAssetMenu (fileName = "Data", menuName = "InkEngine/Ink Story Variable", order = 1)]
    public class InkStoryVariableData : ScriptableObject {

        public string m_ID = "default";
        public List<InkTextVariable> m_variables;
    }
}