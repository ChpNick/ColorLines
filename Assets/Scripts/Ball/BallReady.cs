using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReady : MonoBehaviour
{
    private Animator _animator;
    private bool _active = false;
    public SceneController sceneController;

    private static readonly int Ready = Animator.StringToHash("Ready");

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnMouseDown()
    {
        if (sceneController)
        {
            sceneController.BallClicked(this);
        }
        else
        {
            Debug.LogWarning("Не обозначен контроллер для шара");
            _active = !_active;
            _animator.SetBool(Ready, _active);
        }
    }
    
    public void setActive()
    {
        _active = true;
        _animator.SetBool(Ready, _active);
    }

    public void setUnActive()
    {
        _active = false;
        _animator.SetBool(Ready, _active);
    }
}
