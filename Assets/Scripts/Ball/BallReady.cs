using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReady : MonoBehaviour
{
    private Animator _animator;
    private bool _ready = false;

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
        _ready = !_ready;
        _animator.SetBool(Ready, _ready);
    }
}
