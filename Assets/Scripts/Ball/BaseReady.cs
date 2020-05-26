using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;

public abstract class BaseReady : MonoBehaviour
{
    [Header("Игровые координаты объекта")]
    [SerializeField]
    private Vector3Int gameCoords = Vector3Int.zero;
    
    private void OnMouseDown()
    {
        Operate();
    }
    
    // поведение конкретного объекта шара или платформы
    public abstract void Operate();

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

    /// <summary>
    ///   <para>Перемещает объект по игровым координатам.</para>
    /// </summary>
    public void Move(Vector3Int gameCoords)
    {
        SetGameCoords(gameCoords);
        Vector3 position = Managers.Scene.GameCoordsToPosition(gameCoords);
        position.y += transform.localPosition.y % 10; // берем остаток от анимации прыгания(на случай шарика)
        transform.localPosition = position;
    }

    public void Move(int x, int y, int z)
    {
        Move(new Vector3Int(x, y, z));
    }
}


