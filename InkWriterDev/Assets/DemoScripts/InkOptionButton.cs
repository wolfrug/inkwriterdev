using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InkEngine {

    [System.Serializable]
    public class InkOptionButtonImage {
        public string id;
        public Image target;
        [SerializeField] private GameObject parent;
        public GameObject Parent {
            get {
                if (parent == null) {
                    return target.gameObject;
                } else {
                    return parent;
                }
            }
        }
    }

    [System.Serializable]
    public class InkOptionButtonText {
        public string id;
        public TextMeshProUGUI target;
        [SerializeField] private GameObject parent;
        public GameObject Parent {
            get {
                if (parent == null) {
                    return target.gameObject;
                } else {
                    return parent;
                }
            }
        }
    }
    public class InkOptionButton : MonoBehaviour {
        // Start is called before the first frame update
        private InkDialogueLine m_assignedLine;
        public Button m_button;
        public TextMeshProUGUI m_buttonText;
        public List<InkSpriteWithID> m_spriteList = new List<InkSpriteWithID> { };
        public InkStoryVariableData m_defaultSprites;
        public List<GameObject> m_extraElements = new List<GameObject> { };
        public List<InkOptionButtonImage> m_imageList = new List<InkOptionButtonImage> { };
        public List<InkOptionButtonText> m_textList = new List<InkOptionButtonText> { };

        void Awake () {
            if (m_defaultSprites != null) {
                foreach (InkSpriteWithID sprite in m_defaultSprites.m_sprites) {
                    m_spriteList.Add (sprite);
                }
            }
        }

        public void SetDialogueLine (InkDialogueLine newLine) {
            m_assignedLine = newLine;
            m_buttonText.SetText (newLine.displayText);
            UpdateSelf ();
        }

        public void UpdateSelf () { // Game-specific update stuff goes here
            /* Form:
            SET_BUTTON_GRAPHIC(imageId, spriteId)
            SET_BUTTON_TEXT(textId, text string)
            */

            foreach (InkTextVariable variable in m_assignedLine.inkVariables) {
                if (variable.variableName == "SET_BUTTON_GRAPHIC") {
                    SetImage (variable.VariableArguments[0], variable.VariableArguments[1]);
                }
                if (variable.variableName == "SET_BUTTON_TEXT") {
                    SetText (variable.VariableArguments[0], variable.VariableArguments[1]);
                }
            }
        }

        void SetImage (string imageId, string spriteId) {
            Sprite targetSprite = GetSprite (spriteId);
            InkOptionButtonImage targetImage = GetImage (imageId);
            if (targetImage == null) {
                Debug.LogError ("InkOptionButton: Could not find image component with Id " + imageId, gameObject);
                return;
            }
            if (targetSprite == null) {
                targetImage.Parent.SetActive (false);
            } else {
                targetImage.Parent.SetActive (true);
                targetImage.target.sprite = targetSprite;
            }
        }
        void SetText (string textId, string text) {
            InkOptionButtonText textObj = GetText (textId);
            if (textObj != null) {
                textObj.target.SetText (text);
                textObj.Parent.SetActive (true);
            } else {
                Debug.LogError ("InkOptionButton: Could not find text component with Id " + textId, gameObject);
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
        InkOptionButtonImage GetImage (string imageId) {
            InkOptionButtonImage imageObj = m_imageList.Find ((x) => x.id == imageId);
            if (imageObj != null) {
                return imageObj;
            } else {
                return null;
            }
        }
        InkOptionButtonText GetText (string textId) {
            InkOptionButtonText textObj = m_textList.Find ((x) => x.id == textId);
            if (textObj != null) {
                return textObj;
            } else {
                return null;
            }
        }
    }
}