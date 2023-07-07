using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InkDialogBox : MonoBehaviour {

    [Tooltip ("This is the box that can be safely turned off when not in use")]
    public GameObject m_activeSelf;
    public Transform m_textParent;
    public Transform m_optionsParent;
    public GameObject m_textBoxPrefab;
    public GameObject m_optionBoxPrefab;
    
    // Start is called before the first frame update
    public virtual void Start () {
    }

    public virtual bool Active {
        get {
            return m_activeSelf.activeInHierarchy;
        }
        set {
            m_activeSelf.SetActive (value);
        }
    }

    public virtual GameObject SpawnTextObject (string text) {
        GameObject inkTextObject = Instantiate (m_textBoxPrefab, m_textParent);
        inkTextObject.GetComponentInChildren<TextMeshProUGUI> ().SetText (text);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_textParent.GetComponent<RectTransform> ());
        return inkTextObject;
    }
    public virtual GameObject SpawnButtonObject (string text) {
        GameObject inkOptionButton = Instantiate (m_optionBoxPrefab, m_optionsParent);
        inkOptionButton.GetComponentInChildren<TextMeshProUGUI> ().SetText (text);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_optionsParent.GetComponent<RectTransform> ());
        return inkOptionButton;
    }
    public virtual GameObject SpawnPrefabInText (GameObject prefab) {
        GameObject inkObject = Instantiate (prefab, m_textParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_textParent.GetComponent<RectTransform> ());
        return inkObject;
    }
    public virtual GameObject SpawnPrefabInOptions (GameObject prefab) {
        GameObject inkObject = Instantiate (prefab, m_optionsParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_optionsParent.GetComponent<RectTransform> ());
        return inkObject;
    }
    public virtual void ClearAllText () {
        foreach (Transform child in m_textParent) {
            Destroy (child.gameObject);
        }
    }
    public virtual void ClearAllOptions () {
        foreach (Transform child in m_optionsParent) {
            Destroy (child.gameObject);
        }
    }

}