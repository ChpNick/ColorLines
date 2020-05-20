using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;

public class BaseReady : MonoBehaviour
{
    [Header("Игровые координаты объекта")]
    [SerializeField]
    private Vector3Int gameCoords = Vector3Int.zero;
    
    private void OnMouseDown()
    {
        Operate();
    }
    
    
    public virtual void Operate() {
        // поведение конкретного объекта шара ли платформы
    }

    public void SetGameCoords(int x, int y, int z)
    {
        gameCoords.Set(x, y, z);
    }

    public void SetGameCoords(Vector3Int coords)
    {
        gameCoords = coords;
    }

    public Vector3Int GetGameCoords()
    {
        return gameCoords;
    }

}


