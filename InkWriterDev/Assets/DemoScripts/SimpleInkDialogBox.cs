using System.Collections;
using System.Collections.Generic;
using InkEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SimpleInkDialogBox : InkDialogBox {
    public InkStoryVariableData m_otherPrefabs;

    public ScrollRectAutoScroller[] m_autoScrollers;
    public Button m_continueButton;
    public Button m_skipButton; // put this -above- the continue button ;)
    public bool m_canContinue;
    // Start is called before the first frame update
    public override void Start () {
        if (m_continueButton != null) {
            m_continueButton.onClick.AddListener (() => m_canContinue = true);
        } else {
            m_canContinue = true;
        }
    }

    public void SetContinueButtonActive (bool active) {
        if (HasContinueButton) {
            m_continueButton.gameObject.SetActive (active);
        }
    }

    public void SetSkipButtonActive (bool active) {
        if (HasSkipButton) {
            m_skipButton.gameObject.SetActive (active);
        }
    }

    public bool HasContinueButton {
        get {
            return m_continueButton != null;
        }
    }
    public bool HasSkipButton {
        get {
            return m_skipButton != null;
        }
    }

    public GameObject SpawnTextObject (string text, string textObjectPrefabId = "default") {
        GameObject textBoxPrefab = textObjectPrefabId == "default" ? m_textBoxPrefab : m_otherPrefabs.m_prefabs.Find ((x) => x.id == textObjectPrefabId).prefab;
        GameObject inkTextObject = Instantiate (textBoxPrefab, m_textParent);
        inkTextObject.GetComponentInChildren<TextMeshProUGUI> ().SetText (text);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_textParent.GetComponent<RectTransform> ());
        RunScrollRects();
        return inkTextObject;
    }
    public GameObject SpawnButtonObject (string text, string textObjectPrefabId = "default") {
        GameObject optionBoxPrefab = textObjectPrefabId == "default" ? m_optionBoxPrefab : m_otherPrefabs.m_prefabs.Find ((x) => x.id == textObjectPrefabId).prefab;
        GameObject inkOptionButton = Instantiate (optionBoxPrefab, m_optionsParent);
        inkOptionButton.GetComponentInChildren<TextMeshProUGUI> ().SetText (text);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_optionsParent.GetComponent<RectTransform> ());
        RunScrollRects();
        return inkOptionButton;
    }

    public override GameObject SpawnPrefabInText (GameObject prefab) {
        GameObject inkObject = Instantiate (prefab, m_textParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_textParent.GetComponent<RectTransform> ());
        RunScrollRects();
        return inkObject;
    }
    public override GameObject SpawnPrefabInOptions (GameObject prefab) {
        GameObject inkObject = Instantiate (prefab, m_optionsParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate (m_optionsParent.GetComponent<RectTransform> ());
        RunScrollRects();
        return inkObject;
    }

    public void RunScrollRects(){
        foreach(ScrollRectAutoScroller autoScroller in m_autoScrollers){
            autoScroller.ScrollDown();
        }
    }

}