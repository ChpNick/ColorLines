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

        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
        {
            float posX = x * offsetX - cols / 2;
            float posY = startPosY;
            float posZ = z * offsetY - rows / 2;

            if (level[z, x] > 0)
            {
                GameObject ball = Instantiate(ballPrefabs[level[z, x] - 1], new Vector3(posX, posY, posZ),
                    Quaternion.identity, Country.transform);
                ball.GetComponent<BallReady>().SetGameCoords(x, 0, z);
            }

            if (platformLevel[z, x] > 0)
            {
                GameObject platform = Instantiate(platformPrefabs[platformLevel[z, x] - 1],
                    new Vector3(posX, posY - 1, posZ), Quaternion.identity, Country.transform);
                platform.GetComponent<PlatformReady>().SetGameCoords(x, -1, z);
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
            Vector3Int start = _activeBall.GetGameCoords();
            Vector3Int finish = platform.GetGameCoords();

            if (level[finish.z, finish.x] != 0 || !CheckPath(start, finish))
                return;

            level[finish.z, finish.x] = level[start.z, start.x];
            level[start.z, start.x] = 0;

            _activeBall.SetGameCoords(finish); // todo тут надо зафиксить так как не верная координата Y 

            Vector3 newBallPosition = platform.transform.localPosition;

            newBallPosition.y = _activeBall.transform.localPosition.y;

            _activeBall.transform.localPosition = newBallPosition;
            _activeBall.SetUnActive();
            _activeBall = null;
        }
    }

    public bool CheckPath(Vector3Int start, Vector3Int finish)
    {
        int[,] Map = level.Clone() as int[,];

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