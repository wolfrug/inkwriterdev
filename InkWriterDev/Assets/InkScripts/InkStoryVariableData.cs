using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InkEngine {

    [System.Serializable]
    public class InkSpriteWithID {
        public string id;
        public Sprite sprite;
    }

    [System.Serializable]
    public class InkPrefabWithID {
        public string id;
        public GameObject prefab;
    }

    [CreateAssetMenu (fileName = "Data", menuName = "InkEngine/Ink Story Variable", order = 1)]
    public class InkStoryVariableData : ScriptableObject {

        public string m_ID = "default";
        public List<InkTextVariable> m_variables = new List<InkTextVariable> { };
        public List<InkSpriteWithID> m_sprites = new List<InkSpriteWithID> { };
        public List<InkPrefabWithID> m_prefabs = new List<InkPrefabWithID> { };

    }
}