using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
           SceneManager.LoadScene("GameScene");
        }
    }
    
    private void OnGUI() {
        GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 128, 32), "PlasticRunner");
    }
}
