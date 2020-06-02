using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private const int ADD_BALLS = 2;

    // private Random _random = new Random();
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
            if (platformLevel[z, x] > 0)
                CreatePlatform(x, z, platformLevel[z, x] - 1);

            if (ballLevel[z, x] > 0)
                CreateBall(x, z, ballLevel[z, x] - 1);
        }
    }

    public void CreateBall(int x, int z, int ballId)
    {
        GameObject ball = Instantiate(ballPrefabs[ballId], Country.transform);
        ball.GetComponent<BallReady>().Move(x, 0, z);
        ballLevelObject[z, x] = ball;
    }

    public void CreatePlatform(int x, int z, int platformId)
    {
        GameObject platform = Instantiate(platformPrefabs[platformId], Country.transform);
        platform.GetComponent<PlatformReady>().Move(x, -1, z);
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
            ballLevel[gameCoords.z, gameCoords.x] = 0;
            ballLevelObject[gameCoords.z, gameCoords.x] = null;
            Destroy(ball, 0.5f);
        } 
        
        balls.Clear();
        return true;

    }

    private List<GameObject> CulculateLine(int x0, int z0, int dx, int dz)
    {
        List<GameObject> balls = new List<GameObject>();

        int ball = ballLevel[z0, x0];
        if (ball == 0)
            return balls;

        for (int x = x0, z = z0; GetBallLevelId(x, z) == ball; x += dx, z += dz)
        {
            balls.Add(ballLevelObject[z, x]);
        }

        if (balls.Count < 5)
            balls.Clear();

        return balls;
    }

    private void MoveActiveBall(Vector3Int finish)
    {
        Vector3Int start = _activeBall.GetGameCoords();

        ballLevel[finish.z, finish.x] = ballLevel[start.z, start.x];
        ballLevel[start.z, start.x] = 0;

        ballLevelObject[finish.z, finish.x] = ballLevelObject[start.z, start.x];
        ballLevelObject[start.z, start.x] = null;

        _activeBall.Move(finish);

        _activeBall.SetUnActive();
        _activeBall = null;
    }

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

    private void AddRandomBalls()
    {
        for (int i = 0; i < ADD_BALLS; i++)
            AddRandomBall();
    }

    private void AddRandomBall()
    {
        List<Vector3Int> emptyCell = GetEmptyCell();
        if (emptyCell.Count == 0) return;
        
        Vector3Int cell = emptyCell[Random.Range(0, emptyCell.Count)];
        int z = cell.z;
        int x = cell.x;
        
        ballLevel[z, x] = Random.Range(0, ballPrefabs.Length) + 1; // зафиксить присвоение, перенести в креате балл
        CreateBall(x, z, ballLevel[z, x] - 1);
    }

    private List<Vector3Int> GetEmptyCell()
    {
        List<Vector3Int> emptyCell = new List<Vector3Int>();
        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
            if (GetBallLevelId(x, z) == 0 && platformLevel[z, x] > 0)
                emptyCell.Add(new Vector3Int(x, 0 , z));
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
        if (!OnLevel(x, z)) return 0;
        return ballLevel[z, x];
    }
}