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
        SetDefaultCursor();
    }

    public void SetDefaultCursor()
    {
        PartyUi.Instance.Cursor.sprite = defaultCursor;
    }
    public void SetObserveCursor()
    {
        PartyUi.Instance.Cursor.sprite = observeCursor;
    }
    public void SetThrowCursor()
    {
        PartyUi.Instance.Cursor.sprite = throwCursor;
    }
}