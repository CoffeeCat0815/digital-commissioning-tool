﻿using System.Collections;
using System.Collections.Generic;
using SystemFacade;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] labels = GameObject.FindGameObjectsWithTag("Resource");

        foreach (GameObject label in labels)
        {
            string key = label.GetComponent<UnityEngine.UI.Text>().text.TrimStart('<').TrimEnd('>');
            string localizedText = StringResourceManager.LoadString("@" + key);
            label.GetComponent<UnityEngine.UI.Text>().text = localizedText;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void NewProject()
    {
        LoadDefaultScene();
    }
    public void OpenProject()
    {
        LoadDefaultScene();
    }
    public void Settings()
    {
    }
    public void Exit()
    {
        Debug.Log("Application.Quit() called");
        Application.Quit();
    }
    public void LoadDefaultScene()
    {
        if (!SceneManager.GetSceneByName("WarehouseWithMOSIM").isLoaded)
        {
            SceneManager.LoadScene("WarehouseWithMOSIM", LoadSceneMode.Additive);
            GameObject.Find("Background").SetActive(false);
        }
        GameObject[] gameObjects = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
        foreach (GameObject g in gameObjects)
        {
            if (g.name.Equals("Canvas"))
            {
                g.SetActive(false);
            }
        }
    }
}
