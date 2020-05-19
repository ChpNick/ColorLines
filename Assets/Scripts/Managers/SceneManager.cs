using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private BallReady _activeBall;

    private int[,] level =
    {
        {0, 0, 0, 0, 0, 0, 1, 0, 0},
        {0, 0, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 1, 0, 0, 0, 0},
        {0, 0, 1, 0, 2, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 1, 0, 0, 0},
        {0, 2, 0, 1, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 1, 0, 0, 2, 0, 1, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0},
    };

    private int[,] platformLevel =
    {
        {0, 1, 1, 1, 0, 1, 1, 1, 0},
        {1, 1, 1, 1, 0, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {0, 0, 1, 1, 1, 1, 1, 0, 0},
        {0, 0, 1, 1, 1, 1, 1, 0, 0},
        {0, 0, 1, 1, 1, 1, 1, 0, 0},
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 0, 1, 1, 1, 1},
        {0, 1, 1, 1, 0, 1, 1, 1, 0},
    };

    [SerializeField] private GameObject[] ballPrefabs;
    [SerializeField] private GameObject[] platformPrefabs;
    public GameObject Country { get; private set; } // объект сцены со всеми шарами и платформой

    // стартовая позиция для генерации расположения шаров на уровне
    public float startPosY = 1;

    // смещение шаров относительно друг друга
    public float offsetX = 1.05f;
    public float offsetY = 1.05f;

    public void GenerateBallLevel()
    {
        Debug.Log("Генерируем уровень");

        Country = new GameObject("Country");

        int rows = level.GetLength(0);
        int cols = level.GetLength(1);
        Debug.Log("Размер массива: " + rows + "х" + cols);

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                float posX = j * offsetX - cols / 2;
                float posY = startPosY;
                float posZ = i * offsetY - rows / 2;

                if (level[i, j] > 0)
                {
                    GameObject ball = Instantiate(ballPrefabs[level[i, j] - 1], new Vector3(posX, posY, posZ),
                        Quaternion.identity, Country.transform);
                    ball.GetComponent<BallReady>().SetGameCoords(i, 0, j);
                }

                if (platformLevel[i, j] > 0)
                {
                    GameObject platform = Instantiate(platformPrefabs[platformLevel[i, j] - 1],
                        new Vector3(posX, posY - 1, posZ), Quaternion.identity, Country.transform);
                    platform.GetComponent<PlatformReady>().SetGameCoords(i, -1, j);
                }
            }
        }
    }

    public void BallClicked(BallReady ball)
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

    public void PlatformClicked(PlatformReady platform)
    {
        Debug.Log("Контроллер получил платформу");

        if (_activeBall)
        {
            // здесь запускаем алгоритм проверки есть ли свободный путь до нужного места в платформе

            Vector3 newPosition = platform.transform.position;
            var ballTransform = _activeBall.transform;
            newPosition.y = ballTransform.position.y;

            ballTransform.position = newPosition;

            _activeBall.SetUnActive();

            _activeBall = null;
        }
    }
}