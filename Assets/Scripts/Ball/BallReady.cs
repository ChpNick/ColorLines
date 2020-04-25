using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReady : MonoBehaviour
{
    private Animator _animator;

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
        SceneController.BallClicked(this);
    }
    
    public void SetActive()
    {
        _animator.SetBool(Ready, true);
    }

    public void SetUnActive()
    {
        _animator.SetBool(Ready, false);
    }
}
