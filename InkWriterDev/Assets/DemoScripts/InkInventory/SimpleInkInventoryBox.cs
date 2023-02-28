using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleInkInventoryBox : MonoBehaviour {
    public InkInventoryItemData m_data;
    public Image m_image;
    public TextMeshProUGUI m_nameText;
    public TextMeshProUGUI m_descriptionText;
    public TextMeshProUGUI m_stackText;
    [Tooltip ("To add UI selectables, e.g. buttons")]
    public Selectable m_selectable;

    public void SetItem (InkInventoryItemData data) {
        m_data = data;
        m_image.sprite = m_data.m_image;
        m_nameText.SetText (m_data.m_displayName);
        m_descriptionText.SetText (m_data.m_description);
        m_stackText.SetText (m_data.Stack.ToString ());
    }
}