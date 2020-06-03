using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SceneManager : MonoBehaviour
{
    private BallReady _activeBall;

    private GameObject[,] ballLevelObject;

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
        {0, 0, 1, 1, 1,  1, 1, 0, 0},
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

    private const int EmptyBallId = 0; // Значение пустой клетки в уровне шаров
    private const int EmptyPlatformId = 0; // Значение пустой клетки в уровне платформ

    public int cutBallsCount = 5; // количество шаров, которое необходимо вырезать
    public int addBalls = 2; // Количество добавляемых шариков на сцену

    public void GenerateLevel()
    {
        Debug.Log("Генерируем уровень");

        rows = ballLevel.GetLength(0);
        cols = ballLevel.GetLength(1);

        Country = new GameObject("Country");
        ballLevelObject = new GameObject[rows, cols];

        Debug.Log("Размер массива: " + rows + "х" + cols);

        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
        {
            if (platformLevel[z, x] != EmptyPlatformId)
                CreatePlatform(x, -1, z, platformLevel[z, x]);

            if (ballLevel[z, x] != EmptyBallId)
                CreateBall(x, 0, z, ballLevel[z, x]);
        }
    }

    public void CreateBall(int x, int y, int z, int ballId)
    {
        // ballPrefabs[ballId - 1] --- "-1" так как нумерация в массиве с 1 идет
        Assert.IsTrue(ballId > EmptyBallId);

        GameObject ball = Instantiate(ballPrefabs[ballId - 1], Country.transform);
        BallReady ballReady = ball.GetComponent<BallReady>(); 
        ballReady.Move(x, y, z);
        ballReady.SetId(ballId);
        ballLevelObject[z, x] = ball;
    }

    public void CreateBall(Vector3Int gameCoords, int ballId)
    {
        CreateBall(gameCoords.x, gameCoords.y, gameCoords.z, ballId);
    }

    public void CreatePlatform(int x, int y, int z, int platformId)
    {
        GameObject platform = Instantiate(platformPrefabs[platformId - 1], Country.transform);
        platform.GetComponent<PlatformReady>().Move(x, y, z);
    }

    public void CreatePlatform(Vector3Int gameCoords, int platformId)
    {
        CreatePlatform(gameCoords.x, gameCoords.y, gameCoords.z, platformId);
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

            if (GetBallLevelId(finish) != EmptyBallId || !CheckPath(start, finish))
                return;

            MoveActiveBall(finish + Vector3Int.up);
            if (!CutLines())
            {
                AddRandomBalls();
                CutLines();
            }
        }
    }

    private bool CutLines()
    {
        List<GameObject> balls = new List<GameObject>();

        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
        {
            balls.AddRange(CulculateLine(x, z, 1, 0));
            balls.AddRange(CulculateLine(x, z, 0, 1));
            balls.AddRange(CulculateLine(x, z, 1, 1));
            balls.AddRange(CulculateLine(x, z, -1, 1));
        }

        if (balls.Count == 0) return false;

        Debug.Log("Уничтожаем объекты");
        foreach (GameObject ball in balls)
        {
            Vector3Int gameCoords = ball.GetComponent<BallReady>().GetGameCoords();
            ballLevelObject[gameCoords.z, gameCoords.x] = null;
            Destroy(ball, 0.5f);
        }

        balls.Clear();
        return true;
    }

    private List<GameObject> CulculateLine(int x0, int z0, int dx, int dz)
    {
        List<GameObject> balls = new List<GameObject>();

        int ball = GetBallLevelId(x0, z0);
        if (ball == EmptyBallId) return balls;

        for (int x = x0, z = z0; GetBallLevelId(x, z) == ball; x += dx, z += dz)
        {
            balls.Add(ballLevelObject[z, x]);
        }

        if (balls.Count < cutBallsCount)
            balls.Clear();

        return balls;
    }

    private void MoveActiveBall(Vector3Int finish)
    {
        Vector3Int start = _activeBall.GetGameCoords();

        SwapBall(start, finish);
        _activeBall.Move(finish);

        _activeBall.SetUnActive();
        _activeBall = null;
    }

    private void SwapBall(Vector3Int start, Vector3Int finish)
    {
        ballLevelObject[finish.z, finish.x] = ballLevelObject[start.z, start.x];
        ballLevelObject[start.z, start.x] = null;
    }

    public int[,] BallObjectToIntArray()
    {
        int[,] Map = new int[rows, cols];
        for (int z = 0; z < rows; z++)
        for (int x = 0; x < cols; x++)
            Map[z, x] = GetBallLevelId(x, z);

        return Map;
    }

    public bool CheckPath(Vector3Int start, Vector3Int finish)
    {
        int[,] Map = BallObjectToIntArray();

        bool add = true;
        int z, x, step = EmptyBallId;

        Map[start.z, start.x] = EmptyBallId; // Обнуляем старт, для обозначения волны
        Map[finish.z, finish.x] = step - 1; // Начинаем с финиша

        int rows = Map.GetLength(0);
        int cols = Map.GetLength(1);

        while (add)
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
                    if (x - 1 >= 0 && Map[z, x - 1] == EmptyBallId && platformLevel[z, x - 1] != EmptyPlatformId)
                        Map[z, x - 1] = step - 1;

                    if (z - 1 >= 0 && Map[z - 1, x] == EmptyBallId && platformLevel[z - 1, x] != EmptyPlatformId)
                        Map[z - 1, x] = step - 1;

                    if (x + 1 < cols && Map[z, x + 1] == EmptyBallId && platformLevel[z, x + 1] != EmptyPlatformId)
                        Map[z, x + 1] = step - 1;

                    if (z + 1 < rows && Map[z + 1, x] == EmptyBallId && platformLevel[z + 1, x] != EmptyPlatformId)
                        Map[z + 1, x] = step - 1;
                }
            }

            if (Map[start.z, start.x] != EmptyBallId)
                add = false;
        }

        if (Map[start.z, start.x] != EmptyBallId)
        {
            Debug.Log("Путь найден");
            return true;
        }

        Debug.Log("Путь не найден");
        return false;
    }

    private void AddRandomBalls()
    {
        for (int i = 0; i < addBalls; i++)
            AddRandomBall();
    }

    private void AddRandomBall()
    {
        List<Vector3Int> emptyCell = GetEmptyCell();
        if (emptyCell.Count == 0) return;
        
        Vector3Int gameCoords = emptyCell[Random.Range(0, emptyCell.Count)];
        
        int ballId = Random.Range(0, ballPrefabs.Length) + 1;
        CreateBall(gameCoords, ballId);
    }

    private List<Vector3Int> GetEmptyCell()
    {
        List<Vector3Int> emptyCell = new List<Vector3Int>();
        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
            if (GetBallLevelId(x, z) == EmptyBallId && platformLevel[z, x] != EmptyPlatformId)
                emptyCell.Add(new Vector3Int(x, 0, z));
        return emptyCell;
    }

    public Vector3 GameCoordsToPosition(int gameX, int gameY, int gameZ)
    {
        float posX = gameX * offsetX - cols / 2;
        float posY = gameY;
        float posZ = -(gameZ * offsetY - rows / 2);
        return new Vector3(posX, posY, posZ);
    }

    public Vector3 GameCoordsToPosition(Vector3Int gameCoords)
    {
        return GameCoordsToPosition(gameCoords.x, gameCoords.y, gameCoords.z);
    }

    private bool OnLevel(int x, int z)
    {
        return z >= 0 && z <= rows - 1 && x >= 0 && x <= cols - 1;
    }

    private int GetBallLevelId(int x, int z)
    {
        if (!OnLevel(x, z)) return EmptyBallId;
        if (ballLevelObject[z, x] == null) 
            return 0;
        return ballLevelObject[z, x].GetComponent<BallReady>().GetId();
     
    }

    private int GetBallLevelId(Vector3Int gameCoords)
    {
        return GetBallLevelId(gameCoords.x, gameCoords.z);
    }
}