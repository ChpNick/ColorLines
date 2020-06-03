using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Гарантируем существование различных диспетчеров.
[RequireComponent(typeof(SceneManager))] 
[RequireComponent(typeof(LevelManager))] 
public class Managers : MonoBehaviour {
    // Статические свойства, которыми остальной код пользуется для доступа к диспетчерам.
    public static SceneManager Scene { get; private set; }
    public static LevelManager LevelManager { get; private set; }
    
    void Awake() {
        DontDestroyOnLoad(gameObject); // Команда Unity для сохранения объекта между сценами.
        
        Scene = GetComponent<SceneManager>();
        LevelManager = GetComponent<LevelManager>();
    }

    private void Start()
    {
        Scene.StartLevel();
    }
}