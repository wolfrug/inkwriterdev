using System.Collections;
using System.Collections.Generic;
using InkEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {

    public void LoadSceneUsingInkFunction (InkDialogueLine line, InkTextVariable variable) {
        // We run this from the simple ink listener, easier that way - we know the variable already has the name of the scene
        LoadScene (variable.VariableArguments[0]);
    }
    public void LoadScene (string sceneName) {
        SceneManager.LoadScene (sceneName);
    }
}