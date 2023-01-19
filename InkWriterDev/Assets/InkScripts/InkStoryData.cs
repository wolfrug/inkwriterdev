using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

namespace InkEngine {

    [CreateAssetMenu (fileName = "Data", menuName = "InkEngine/Ink Story Object", order = 1)]
    public class InkStoryData : ScriptableObject {

        public string m_ID = "default";
        public TextAsset m_inkJsonAsset;
        public Story m_inkStory = null;

        public void SaveStory () {
            string json = m_inkStory.state.ToJson ();
            PlayerPrefs.SetString ("InkWriter_ExampleSave", json);
        }

        public void InitStory () { // inits a story by either loading it or starting a new one
            if (m_inkStory == null) {
                m_inkStory = new Story (m_inkJsonAsset.text);
            };
            if (SavedStory) { // We load if we can
                string savedJson = PlayerPrefs.GetString ("InkWriter_ExampleSave");
                m_inkStory.state.LoadJson (savedJson);
                Debug.Log ("Loaded story state");
            } else { // If we can't, we start from beginning
                Debug.Log ("No saved story found - starting new story");
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
            return m_inkStory != null;
        }
    }
}