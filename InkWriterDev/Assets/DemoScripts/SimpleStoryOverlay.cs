using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InkEngine {

    [System.Serializable]
    public class InkStoryOverlayImage {
        public string id;
        public Image target;
    }

    [System.Serializable]
    public class InkStoryOverlayText {
        public string id;
        public TextMeshProUGUI target;
    }
    public class SimpleStoryOverlay : MonoBehaviour {

        public GameObject m_mainStoryOverlayGO;
        public List<InkSpriteWithID> m_spriteList = new List<InkSpriteWithID> { };
        public InkStoryVariableData m_defaultSprites;
        public List<InkStoryOverlayImage> m_imageList = new List<InkStoryOverlayImage> { };
        public List<InkStoryOverlayText> m_textList = new List<InkStoryOverlayText> { };

        void Awake () {
            if (m_defaultSprites != null) {
                foreach (InkSpriteWithID sprite in m_defaultSprites.m_sprites) {
                    m_spriteList.Add (sprite);
                }
            }
        }
        public void EventListener (InkDialogueLine line, InkTextVariable variable) {
            switch (variable.variableName) {
                case "SET_STORY_IMAGE":
                    {
                        SetImage (variable.VariableArguments[0], variable.VariableArguments[1]);
                        break;
                    }
                case "SET_STORY_TEXT":
                    {
                        SetText (variable.VariableArguments[0], variable.VariableArguments[1]);
                        break;
                    }
            }
        }

        void SetImage (string imageId, string spriteId) {
            Sprite targetSprite = GetSprite (spriteId);
            Image targetImage = GetImage (imageId);
            if (targetImage == null) {
                Debug.LogError ("StoryOverlay: Could not find image component with Id " + imageId, gameObject);
                return;
            }
            if (targetSprite == null) {
                targetImage.gameObject.SetActive (false);
            } else {
                targetImage.gameObject.SetActive (true);
                targetImage.sprite = targetSprite;
            }
        }
        void SetText (string textId, string text) {
            TextMeshProUGUI textObj = GetText (textId);
            if (textObj != null) {
                textObj.SetText (text);
            } else {
                Debug.LogError ("StoryOverlay: Could not find text component with Id " + textId, gameObject);
            }
        }

        Sprite GetSprite (string spriteId) {
            InkSpriteWithID spriteObj = m_spriteList.Find ((x) => x.id == spriteId);
            if (spriteObj != null) {
                return spriteObj.sprite;
            } else {
                return null;
            }
        }
        Image GetImage (string imageId) {
            InkStoryOverlayImage imageObj = m_imageList.Find ((x) => x.id == imageId);
            if (imageObj != null) {
                return imageObj.target;
            } else {
                return null;
            }
        }
        TextMeshProUGUI GetText (string textId) {
            InkStoryOverlayText textObj = m_textList.Find ((x) => x.id == textId);
            if (textObj != null) {
                return textObj.target;
            } else {
                return null;
            }
        }

    }
}