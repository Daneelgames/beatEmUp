using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalCursorController : MonoBehaviour
{
    public static UniversalCursorController Instance;
    public Sprite defaultCursor;
    public Sprite observeCursor;
    public Sprite throwCursor;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        SetDefaultCursor(true);
    }

    public void SetDefaultCursor(bool hide)
    {
        print(hide);
        if (hide)
        {
            Cursor.lockState = CursorLockMode.Confined;
            PartyUi.Instance.Cursor.color = Color.clear;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            PartyUi.Instance.Cursor.sprite = defaultCursor;
            PartyUi.Instance.Cursor.color = Color.white;
        }
    }
    public void SetObserveCursor()
    {
        PartyUi.Instance.Cursor.sprite = observeCursor;
        PartyUi.Instance.Cursor.color = Color.white;
    }
    public void SetThrowCursor()
    {
        PartyUi.Instance.Cursor.sprite = throwCursor;
        PartyUi.Instance.Cursor.color = Color.white;
    }
}