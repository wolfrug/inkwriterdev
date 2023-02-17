using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkInventoryItemAnimator : MonoBehaviour {
    public Animator m_targetAnimator;
    public SimpleInkInventoryBox m_targetBox;

    public int m_maxStack = 5;
    public string m_animFloatName = "progress";
    // Start is called before the first frame update
    void Start () {

    }

    // Update is called once per frame
    void FixedUpdate () {
        if (m_targetBox != null) {
            if (m_targetBox.m_data != null) {
                float targetFloat = (float) m_targetBox.m_data.Stack / (float) m_maxStack;
                // float lerpFloat = Mathf.Lerp (m_targetAnimator.GetFloat (m_animFloatName), targetFloat, Time.deltaTime);
                m_targetAnimator.SetFloat (m_animFloatName, targetFloat);
            };
        }
    }
}