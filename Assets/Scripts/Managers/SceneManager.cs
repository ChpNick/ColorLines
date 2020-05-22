using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private BallReady _activeBall;

    private int[,] ballLevel =
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
    
    // смещение шаров относительно друг друга
    public float offsetX = 1.05f;
    public float offsetY = 1.05f;

    public int rows; // количество строк в уровне
    public int cols; // количество колонок в уровне

    public void GenerateBallLevel()
    {
        Debug.Log("Генерируем уровень");

        Country = new GameObject("Country");

        rows = ballLevel.GetLength(0);
        cols = ballLevel.GetLength(1);
        Debug.Log("Размер массива: " + rows + "х" + cols);

        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
        {
            if (platformLevel[z, x] > 0)
                CreatePlatform(x, z, platformLevel[z, x] - 1);
            
            if (ballLevel[z, x] > 0)
                CreateBall(x, z, ballLevel[z, x] - 1);
        }
    }

    public void CreateBall(int x, int z, int ballId)
    {
        Vector3 position = GameCoordsToPosition(x, 0, z);
        GameObject ball = Instantiate(ballPrefabs[ballId], position, Quaternion.identity, Country.transform);
        ball.GetComponent<BallReady>().SetGameCoords(x, 0, z);
    }

    public void CreatePlatform(int x, int z, int platformId)
    {
        Vector3 position = GameCoordsToPosition(x, -1, z);
        GameObject platform = Instantiate(platformPrefabs[platformId], position, Quaternion.identity, Country.transform);
        platform.GetComponent<PlatformReady>().SetGameCoords(x, -1, z);
    }

    public Vector3 GameCoordsToPosition(int gameX, int gameY, int gameZ)
    {
        float posX = gameX * offsetX - cols / 2;
        float posY = gameY;
        float posZ = -(gameZ * offsetY - rows / 2);
        return new Vector3(posX, posY, posZ);
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
            Vector3Int start = _activeBall.GetGameCoords();
            Vector3Int finish = platform.GetGameCoords();

            if (ballLevel[finish.z, finish.x] != 0 || !CheckPath(start, finish))
                return;

            ballLevel[finish.z, finish.x] = ballLevel[start.z, start.x];
            ballLevel[start.z, start.x] = 0;
            
            _activeBall.SetGameCoords(finish + Vector3Int.up); 

            Vector3 newBallPosition = platform.transform.localPosition;

            newBallPosition.y = _activeBall.transform.localPosition.y;

            _activeBall.transform.localPosition = newBallPosition;
            _activeBall.SetUnActive();
            _activeBall = null;
        }
    }
    
    // public void MoveBall(GameObject ball, Vector3Int)

    public bool CheckPath(Vector3Int start, Vector3Int finish)
    {
        int[,] Map = ballLevel.Clone() as int[,];

        bool add = true;
        int z, x, step = 0;

        Map[start.z, start.x] = 0; // Обнуляем старт, для обозначения волны
        Map[finish.z, finish.x] = step - 1; // Начинаем с финиша

        int rows = Map.GetLength(0);
        int cols = Map.GetLength(1);

        while (add == true)
        {
            add = false;
            step--;

            for (z = 0; z < rows; z++)
            for (x = 0; x < cols; x++)
            {
                if (Map[z, x] == step)
                {
                    add = true;
                    //Ставим значение шага-1 в соседние ячейки (если они проходимы)
                    if (x - 1 >= 0 && Map[z, x - 1] == 0 && platformLevel[z, x - 1] != 0)
                        Map[z, x - 1] = step - 1;

                    if (z - 1 >= 0 && Map[z - 1, x] == 0 && platformLevel[z - 1, x] != 0)
                        Map[z - 1, x] = step - 1;

                    if (x + 1 < cols && Map[z, x + 1] == 0 && platformLevel[z, x + 1] != 0)
                        Map[z, x + 1] = step - 1;

                    if (z + 1 < rows && Map[z + 1, x] == 0 && platformLevel[z + 1, x] != 0)
                        Map[z + 1, x] = step - 1;
                }
            }

            if (Map[start.z, start.x] != 0)
                add = false;
        }

        if (Map[start.z, start.x] != 0)
        {
            Debug.Log("Путь найден");
            return true;
        }

        Debug.Log("Путь не найден");
        return false;
    }
}