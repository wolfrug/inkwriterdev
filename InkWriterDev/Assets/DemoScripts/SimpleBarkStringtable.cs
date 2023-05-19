using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;
public class SimpleBarkStringtable : MonoBehaviour {
    // Example of how to use the ink dialogue line stuff to create lists of barks
    public SimpleInkWriter m_targetWriter;
    public string m_stringtableKnot;

    public void PlayRandomBark (string targetBarker) { // makes a stringtable from the target knot, plays a random entry
        InkDialogueLine[] allLines = m_targetWriter.m_storyData.CreateStringArrayKnot (m_stringtableKnot, null);
        List<InkDialogueLine> targetLines = new List<InkDialogueLine> (allLines);
        targetLines = targetLines.FindAll ((x) => x.HasVariable (targetBarker));
        /*foreach (InkDialogueLine line in targetLines) {
            Debug.Log (line.displayText);
        }*/
        if (targetLines.Count > 0) {
            InkDialogueLine[] randomLine = new InkDialogueLine[1] { targetLines[Random.Range (0, targetLines.Count)] };
            m_targetWriter.PlayDialogueLines (randomLine);
        } else {
            Debug.Log ("Found no lines for random bark");
        }
    }
    public void PlayFullArgumentBark (string targetBarker, string targetArgument) { // makes a stringtable and plays everything from the given argument 
        InkDialogueLine[] allLines = m_targetWriter.m_storyData.CreateStringArrayKnot (m_stringtableKnot, null);
        List<InkDialogueLine> targetLines = new List<InkDialogueLine> (allLines);
        targetLines = targetLines.FindAll ((x) => x.HasVariableWithArgument (targetBarker, targetArgument));
        /*foreach (InkDialogueLine line in targetLines) {
            Debug.Log (line.displayText);
        }*/
        if (targetLines.Count > 0) {
            m_targetWriter.PlayDialogueLines (targetLines.ToArray ());
        } else {
            Debug.Log ("Found no lines for argument bark");
        }
    }
    public void DebugPlayArgumentBark (string targetArgument) // Just for use with the unity onclick event
    {
        PlayFullArgumentBark ("PLAYER_BARK", targetArgument);
    }
}