using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ink.Runtime;
using UnityEngine;

namespace InkEngine {

    public class InkStoryStateManager : MonoBehaviour {
        public InkStoryData m_storyObject;
        public bool m_startOnInit = true;

        void Awake () {
            if (m_startOnInit) {
                InitStory ();
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
        
    }
}