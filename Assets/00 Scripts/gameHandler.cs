using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class gameHandler : MonoBehaviour
{

    public GameObject dropDown;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleCursor();

        if (dropDown) dropDown.SetActive(Cursor.visible);
    }

    /////////////////////////////////////////////////////////////  

    void QuitGame()
    {

        Application.Quit();

        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #endif
    }

    void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    ////////////////////////////////////////////////////////////



}
