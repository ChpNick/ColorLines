using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    private BallReady _activeBall;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BallClicked(BallReady ball)
    {    
        Debug.Log("Контроллер получил шар");
        if (_activeBall) _activeBall.setUnActive();

        if (_activeBall == ball) _activeBall = null;
        else
        {
            _activeBall = ball;
            _activeBall.setActive();
        }
    }
}
