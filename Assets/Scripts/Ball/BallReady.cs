using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReady : BaseReady
{
    private Animator _animator;

    private static readonly int Ready = Animator.StringToHash("Ready");
    
    void Start()
    {
        _animator = GetComponent<Animator>();
    }
    
    public override void Operate()
    {
        Managers.Scene.BallClicked(this);
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
