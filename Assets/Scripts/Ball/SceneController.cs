using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    private static BallReady _activeBall;
    private int[,] level =
    {
        {0, 0, 0, 0, 0, 0, 1, 0, 0},
        {0, 0, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 1, 0, 2, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 1, 0, 0, 0},
        {0, 2, 0, 1, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 1, 0, 0, 2, 0, 1, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0},
    };

    private int[,] platform =
    {
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
    };

    [SerializeField] private GameObject[] ballPrefabs;
    [SerializeField] private GameObject[] platformPrefabs;

    // стартовая позиция для генерации расположения шаров на уровне
    public float startPosY = 1;

    // смещение шаров относительно друг друга
    public float offsetX = 1.05f;
    public float offsetY = 1.05f;
    void Start()
    {
        GenerateBallLevel();
    }
    
    void Update()
    {
        
    }

    private void GenerateBallLevel()
    {
        Debug.Log("Генерируем уровень");
        
        int rows = level.GetLength(0);
        int cols = level.GetLength(1);
        Debug.Log("Размер массива: " + rows + "х" + cols);

        for (int i = 0; i < rows; ++i) {
            for (int j = 0; j < cols; ++j)
            {
                float posX = j * offsetX - cols / 2;
                float posY = startPosY;
                float posZ = i * offsetY - rows / 2;
                
                if (level[i, j] > 0)
                    Instantiate(ballPrefabs[level[i, j] - 1], new Vector3(posX, posY, posZ), Quaternion.identity);
                
                if (platform[i, j] > 0)
                    Instantiate(platformPrefabs[platform[i, j] - 1], new Vector3(posX, posY - 1, posZ), Quaternion.identity);
            }
        }
    }
    public static void BallClicked(BallReady ball)
    {    
        Debug.Log("Контроллер получил шар");
        if (_activeBall) _activeBall.SetUnActive();

        if (_activeBall == ball) _activeBall = null;
        else
        {
            _activeBall = ball;
            _activeBall.SetActive();
        }
    }
}
