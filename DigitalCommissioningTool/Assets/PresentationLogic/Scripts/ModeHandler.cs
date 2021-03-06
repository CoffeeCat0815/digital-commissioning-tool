﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Camera = UnityEngine.Camera;

public class ModeHandler : MonoBehaviour
{
    public GameObject EditorModeCamera;
    public GameObject MosimCamera;
    public string Mode { get; private set; }

    public void SwitchMode()
    {
        EditorModeCamera.GetComponent<Camera>().enabled = !EditorModeCamera.GetComponent<Camera>().enabled;
        MosimCamera.GetComponent<Camera>().enabled = !MosimCamera.GetComponent<Camera>().enabled;

        if (EditorModeCamera.GetComponent<Camera>().enabled)
        {
            Mode = "EditorMode";
        }
        else
        {
            Mode = "MosimMode";
        }
    }
    public void ChangeMode(string mode)
    {
        
        if (mode.Equals("MosimMode"))
        {
            Mode = mode;
            Debug.Log(mode);
            MosimCamera.GetComponent<Camera>().enabled = true;
            EditorModeCamera.GetComponent<Camera>().enabled = false;
        }
        else if (mode.Equals("EditorMode"))
        {
            Mode = mode;
            Debug.Log(mode);
            EditorModeCamera.GetComponent<Camera>().enabled = true;
            MosimCamera.GetComponent<Camera>().enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Mode = "MosimMode";
        MosimCamera.GetComponent<Camera>().enabled = true;
        EditorModeCamera.GetComponent<Camera>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
