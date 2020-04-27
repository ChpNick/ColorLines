using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SceneManager))] // Гарантируем существование различных диспетчеров. 
public class Managers : MonoBehaviour {
    // Статические свойства, которыми остальной код пользуется для доступа к диспетчерам.
    public static SceneManager Scene { get; private set; }
    
    void Awake() {
        DontDestroyOnLoad(gameObject); // Команда Unity для сохранения объекта между сценами.
        
        Scene = GetComponent<SceneManager>();
    }

    private void Start()
    {
        Scene.GenerateBallLevel();
    }
}