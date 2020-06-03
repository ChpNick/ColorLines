using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SceneManager : MonoBehaviour
{
    private BallReady _activeBall; // шар который выбран для игры

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
    
    private int[,] _platform; // текущий уровень платформ полученный от менеджера
    private BallReady[,] _level; // массив с шарами

    public void StartLevel()
    {
        Debug.Log("Генерируем уровень");

        int[,] level = Managers.LevelManager.GetLevel()[0];
        _platform = Managers.LevelManager.GetLevel()[1];

        rows = level.GetLength(0);
        cols = level.GetLength(1);

        Country = new GameObject("Country");
        _level = new BallReady[rows, cols];

        Debug.Log("Размер массива: " + rows + "х" + cols);

        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
        {
            if (_platform[z, x] != EmptyPlatformId)
                CreatePlatform(x, -1, z, _platform[z, x]);

            if (level[z, x] != EmptyBallId)
                CreateBall(x, 0, z, level[z, x]);
        }
    }

    /// <summary>
    ///   <para>Создает на сцене шар по игровым координатам.</para>
    /// </summary>
    public void CreateBall(int x, int y, int z, int ballId)
    {
        // ballPrefabs[ballId - 1] --- "-1" так как нумерация в массиве с 1 идет
        Assert.IsTrue(ballId > EmptyBallId);

        GameObject ball = Instantiate(ballPrefabs[ballId - 1], Country.transform);
        BallReady ballReady = ball.GetComponent<BallReady>();
        ballReady.Move(x, y, z);
        ballReady.SetId(ballId);
        _level[z, x] = ballReady;
    }

    public void CreateBall(Vector3Int gameCoords, int ballId)
    {
        CreateBall(gameCoords.x, gameCoords.y, gameCoords.z, ballId);
    }

    /// <summary>
    ///   <para>Создает на сцене платформу по игровым координатам.</para>
    /// </summary>
    public void CreatePlatform(int x, int y, int z, int platformId)
    {
        GameObject platform = Instantiate(platformPrefabs[platformId - 1], Country.transform);
        platform.GetComponent<PlatformReady>().Move(x, y, z);
    }

    public void CreatePlatform(Vector3Int gameCoords, int platformId)
    {
        CreatePlatform(gameCoords.x, gameCoords.y, gameCoords.z, platformId);
    }

    /// <summary>
    ///   <para>Вызывается при нажатии на шарик.</para>
    /// </summary>
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

    /// <summary>
    ///   <para>Вызывается при нажатии на платформу.</para>
    /// </summary>
    public void PlatformClicked(PlatformReady platform)
    {
        Debug.Log("Контроллер получил платформу");

        if (_activeBall)
        {
            Vector3Int start = _activeBall.GetGameCoords();
            Vector3Int finish = platform.GetGameCoords();

            if (GetBallIdInGameCoords(finish) != EmptyBallId || !CheckPath(start, finish))
                return;

            MoveActiveBall(finish + Vector3Int.up);
            if (!CutLines())
            {
                AddRandomBalls();
                CutLines();
            }
        }
    }

    /// <summary>
    ///   <para>Вырезает все найденные линии из шариков и возвращает вырезал или нет.</para>
    /// </summary>
    private bool CutLines()
    {
        List<BallReady> balls = new List<BallReady>();

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
        foreach (BallReady ball in balls)
        {
            Vector3Int gameCoords = ball.GetGameCoords();
            _level[gameCoords.z, gameCoords.x] = null;
            Destroy(ball.gameObject, 0.5f);
        }

        balls.Clear();
        return true;
    }

    /// <summary>
    ///   <para>Этот метод ищет линии которую можно вырезать.</para>
    /// </summary>
    private List<BallReady> CulculateLine(int x0, int z0, int dx, int dz)
    {
        List<BallReady> balls = new List<BallReady>();

        int ballId = GetBallIdInGameCoords(x0, z0);
        if (ballId == EmptyBallId) return balls;

        for (int x = x0, z = z0; GetBallIdInGameCoords(x, z) == ballId; x += dx, z += dz)
        {
            balls.Add(_level[z, x]);
        }

        if (balls.Count < cutBallsCount)
            balls.Clear();

        return balls;
    }

    /// <summary>
    ///   <para>Перемещяет шарик по игровым координатам.</para>
    /// </summary>
    private void MoveActiveBall(Vector3Int finish)
    {
        Vector3Int start = _activeBall.GetGameCoords();

        SwapBall(start, finish);
        _activeBall.Move(finish);

        _activeBall.SetUnActive();
        _activeBall = null;
    }

    /// <summary>
    ///   <para>Меняет местами значения в игровом массиве.</para>
    /// </summary>
    private void SwapBall(Vector3Int start, Vector3Int finish)
    {
        _level[finish.z, finish.x] = _level[start.z, start.x];
        _level[start.z, start.x] = null;
    }

    /// <summary>
    ///   <para>Конвертит массив с шарами в массив с id.</para>
    /// </summary>
    public int[,] BallObjectToIntArray()
    {
        int[,] Map = new int[rows, cols];
        for (int z = 0; z < rows; z++)
        for (int x = 0; x < cols; x++)
            Map[z, x] = GetBallIdInGameCoords(x, z);

        return Map;
    }

    /// <summary>
    ///   <para>Проверяет есть ли путь в игровом массиве.</para>
    /// </summary>
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
                    if (x - 1 >= 0 && Map[z, x - 1] == EmptyBallId && _platform[z, x - 1] != EmptyPlatformId)
                        Map[z, x - 1] = step - 1;

                    if (z - 1 >= 0 && Map[z - 1, x] == EmptyBallId && _platform[z - 1, x] != EmptyPlatformId)
                        Map[z - 1, x] = step - 1;

                    if (x + 1 < cols && Map[z, x + 1] == EmptyBallId && _platform[z, x + 1] != EmptyPlatformId)
                        Map[z, x + 1] = step - 1;

                    if (z + 1 < rows && Map[z + 1, x] == EmptyBallId && _platform[z + 1, x] != EmptyPlatformId)
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

    /// <summary>
    ///   <para>Создает рандомные шарики равное addBalls </para>
    /// </summary>
    private void AddRandomBalls()
    {
        for (int i = 0; i < addBalls; i++)
            AddRandomBall();
    }

    /// <summary>
    ///   <para>Создает один рандомный шарик</para>
    /// </summary>
    private void AddRandomBall()
    {
        List<Vector3Int> emptyCell = GetEmptyCell();
        if (emptyCell.Count == 0) return;

        Vector3Int gameCoords = emptyCell[Random.Range(0, emptyCell.Count)];

        int ballId = Random.Range(0, ballPrefabs.Length) + 1;
        CreateBall(gameCoords, ballId);
    }

    /// <summary>
    ///   <para>Выбирает все пустые ящейки в игровом массиве</para>
    /// </summary>
    private List<Vector3Int> GetEmptyCell()
    {
        List<Vector3Int> emptyCell = new List<Vector3Int>();
        for (int z = 0; z < rows; ++z)
        for (int x = 0; x < cols; ++x)
            if (GetBallIdInGameCoords(x, z) == EmptyBallId && _platform[z, x] != EmptyPlatformId)
                emptyCell.Add(new Vector3Int(x, 0, z));
        return emptyCell;
    }

    /// <summary>
    ///   <para>Конвертит игровую координату в координату на сцене</para>
    /// </summary>
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

    /// <summary>
    ///   <para>Проверяет не вышли ли мы за пределы игрового массива</para>
    /// </summary>
    private bool OnLevel(int x, int z)
    {
        return z >= 0 && z <= rows - 1 && x >= 0 && x <= cols - 1;
    }

    /// <summary>
    ///   <para>возвращяет id шарика в игровом массиве если шарик там есть иначе EmptyBallId</para>
    /// </summary>
    private int GetBallIdInGameCoords(int x, int z)
    {
        if (!OnLevel(x, z)) return EmptyBallId;
        if (_level[z, x] == null)
            return EmptyBallId;
        return _level[z, x].GetId();
    }

    private int GetBallIdInGameCoords(Vector3Int gameCoords)
    {
        return GetBallIdInGameCoords(gameCoords.x, gameCoords.z);
    }
}