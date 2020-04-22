﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField UsernameField;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
        else if (instance != this)
        {
            Debug.Log("instance already existing, destroying object");
            Destroy(this);

        }
    }

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        UsernameField.interactable = false;

        Client.instance.ConnectToServer();  
    }

}
