using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectAutoScroller : MonoBehaviour {
    public ScrollRect m_targetRect;

    public void ScrollDown () {
        UpdateLayout (m_targetRect.transform);
        m_targetRect.verticalNormalizedPosition = 0f;
    }

    public static void UpdateLayout (Transform xform) {
        Canvas.ForceUpdateCanvases ();
        UpdateLayout_Internal (xform);
    }

    private static void UpdateLayout_Internal (Transform xform) {
        if (xform == null || xform.Equals (null)) {
            return;
        }

        // Update children first
        for (int x = 0; x < xform.childCount; ++x) {
            UpdateLayout_Internal (xform.GetChild (x));
        }

        // Update any components that might resize UI elements
        foreach (LayoutGroup layout in xform.GetComponents<LayoutGroup> ()) {
            if (layout.GetComponent<RectTransform>() != null) {
                layout.CalculateLayoutInputVertical ();
                layout.CalculateLayoutInputHorizontal ();
            }
        }
        foreach (ContentSizeFitter fitter in xform.GetComponents<ContentSizeFitter> ()) {
            if (fitter.GetComponent<RectTransform>() != null) {
                fitter.SetLayoutVertical ();
                fitter.SetLayoutHorizontal ();
            }
        }
    }
}