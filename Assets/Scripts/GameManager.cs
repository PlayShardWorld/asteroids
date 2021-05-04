using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    #region Preparing
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("��� ������ �������� GameManager �� �����");

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance.gameObject);
        instance.name = "Manager: Game";
    }
    #endregion

    public UnityAction<int> OnScoreChanged;
    public UnityAction<int> OnLivesChanged;

    [SerializeField] private UIGame _uiGame;                        // ������ �� ������� ���������
    [SerializeField] private Camera _gameCamera;                    // ������ �� ������� ������
    [SerializeField] private Player _playerPrototype;               // �������� ������
    [SerializeField] private Asteroid _asteroidPrototype;           // �������� ��������� ���������
    [SerializeField] private Ufo _ufoPrototype;                     // �������� �������� �������

    // ���������� ��� �������� ��������� ����
    [SerializeField] private int _enemyStartCount = 4;              // ���������� ����������� �� ������ �����
    [SerializeField] private int _enemyDeltaCount = 2;              // ���������� ���������� ����������� � ������ ������
    [SerializeField] private int _enemyMaxCount = 12;               // ������������ ���������� �����������
    [SerializeField] private float _UfoCooldownTime = 30.0f;        // ����� ����� �������� ���
    private int _enemyLastWaveCount;                                // ���������� ����������� �� ��������� ���������� �����

    private GameState _currentGameState;                            // ������� ������� ���������
    private Player _activePlayer;                                   // ������ �� ��������� ������
    private List<Enemy> _activeAsteroidList = new List<Enemy>();    // ������ �������� ���������� ��� ������������

    [SerializeField] private int _defaultLifeCount = 3;             // ���������� ������ �� ���������
    private int _lifeCount = 0;                                     // ������� ���������� ������
    private int _playerScore;                                       // ������� ���������� �����

    public int CurrentLifeCount => _lifeCount;
    public int CurrentPlayerScore => _playerScore;

    private Coroutine coroutineUfoGenerator;
    private Coroutine coroutineGameOver;

    // Start is called before the first frame update
    void Start()
    {
        // ��� ������ ������� ���� ��������� ��������� � Reset
        ResetGame();
    }

    // Update is called once per frame
    void Update()
    {
        // GameState.Demo -> GameState.Game
        if (_currentGameState == GameState.Demo && Input.anyKeyDown)
        {
            StartCoroutine(StartGame());
        }

        // GameState.Crushed -> GameState.Game
        else if (_currentGameState == GameState.Crushed && Input.anyKeyDown)
        {
            SpawnPlayer();
            _uiGame.HideGameMessage();
            _currentGameState = GameState.Game;
        }

        // GameState.GameOver -> GameState.Demo
        else if (_currentGameState == GameState.GameOver && Input.anyKeyDown)
        {
            if (coroutineGameOver != null)
            {
                StopCoroutine(coroutineGameOver);
                coroutineGameOver = null;
            }

            ResetGame();
        }
    }

    #region Score & Lives
    public void ChangeScore(int value)
    {
        _playerScore += value;

        OnScoreChanged?.Invoke(_playerScore);
    }

    public void ResetScore()
    {
        _playerScore = 0;

        OnScoreChanged?.Invoke(_playerScore);
    }

    public void ChangeLives(int value)
    {
        _lifeCount += value;

        OnLivesChanged?.Invoke(_lifeCount);
    }

    public void ResetLives(int count = 0)
    {
        _lifeCount = count;

        OnLivesChanged?.Invoke(_lifeCount);
    }
    #endregion

    #region States
    // ������� ��������� ���� � GameState.Demo
    private void ResetGame()
    {
        // ��������� ��������� �������� �������
        EnableUfoGenerator(false);

        // ������� ��� �������� � �������� ����
        RemoveAllCreatures();

        // ���������� ������� ����� � ������
        ResetScore();
        ResetLives();

        // ��������� ������������ ���� (������ ������� 6 �����)
        SpawnWave(6);

        // ������� ��������� � �������� ������ ����
        _uiGame.ShowGameMessage("CLICK TO START", true);

        _currentGameState = GameState.Demo;
    }

    // ������� ��������� ���� � GameState.Game
    private IEnumerator StartGame()
    {
        // ������� ��� �������� � �������� ����
        RemoveAllCreatures();

        // ���������� ������� ����� � ������
        ResetScore();
        ResetLives(_defaultLifeCount);

        _uiGame.ShowGameMessage("READY");

        _currentGameState = GameState.Loading;

        // ������� ����� ��� �������� ������ ��������
        yield return new WaitForSeconds(2.0f);

        _uiGame.HideGameMessage();

        // ������� ������
        SpawnPlayer();

        // ������� �����
        SpawnWave(_enemyStartCount);

        // ��������� ������� ��������� �������� �������
        EnableUfoGenerator(true);

        _currentGameState = GameState.Game;
    }

    private IEnumerator GameOver()
    {
        _uiGame.ShowGameMessage($"GAME OVER.\nYOUR SCORE: {_playerScore}");

        _currentGameState = GameState.GameOver;

        yield return new WaitForSeconds(10.0f);

        ResetGame();
    }
    #endregion

    // ����������� ������ ������
    public void PlayerDied()
    {
        _activePlayer = null;

        // �������� ����� � ������
        ChangeLives(-1);

        // ���� ����� ����������� - GameOver, ����� - ��� ����������� ������������ � ������ ������
        if (_lifeCount == 0)
        {
            if (coroutineGameOver == null)
            {
                coroutineGameOver = StartCoroutine(GameOver());
            }
        }
        else
        {
            _uiGame.ShowGameMessage("CLICK TO RESPAWN", true);
            _currentGameState = GameState.Crushed;
        }
    }

    // ����������� ����������
    public void RegisterEnemy(Enemy enemy)
    {
        enemy.OnKilled += OnEnemyKilledHandler;
        _activeAsteroidList.Add(enemy);
    }

    // ���������� ������� ������ ����������
    private void OnEnemyKilledHandler(Enemy enemy)
    {
        _activeAsteroidList.Remove(enemy);

        // ���� ���������� �� ����� ����������� - ��������� ����� �����
        if (_activeAsteroidList.Count == 0)
        {
            SpawnWave(Mathf.Clamp(_enemyLastWaveCount + _enemyDeltaCount, 0, _enemyMaxCount));
        }
    }

    // ����� ����� �����
    private void SpawnWave(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            Asteroid go = Instantiate(_asteroidPrototype, 
                _gameCamera.ScreenToWorldPoint(new Vector3(0.0f, 
                                                          Random.Range(0.0f, _gameCamera.scaledPixelHeight),
                                                          -_gameCamera.transform.position.z)),
                Quaternion.identity);

            // ������������� �� ������� ������ ���������
            go.OnKilled += OnEnemyKilledHandler;

            // ������������ ������ ��������
            _activeAsteroidList.Add(go);
        }

        _enemyLastWaveCount = count;
    }

    // �������� ���� ������� ��������� � ����
    private void RemoveAllCreatures()
    {
        // ������� ��� ��������� � �������� ����
        for (int i = 0; i < _activeAsteroidList.Count; ++i)
        {
            // ������������ �� ������� ������ ��������� � ���������� ���
            _activeAsteroidList[i].OnKilled -= OnEnemyKilledHandler;
            DeathEffect de = _activeAsteroidList[i].GetComponent<DeathEffect>();
            if (de != null) de.Enable(false);
            Destroy(_activeAsteroidList[i].gameObject);
        }
        _activeAsteroidList.Clear();

        // ������� ��� �������� ������� (� ���������� �����������, ���� ��� ���������)
        Enemy[] lastEnemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < lastEnemies.Length; ++i)
        {
            // ������������ �� ������� ������ �������� � ���������� �
            lastEnemies[i].OnKilled -= OnEnemyKilledHandler;
            DeathEffect de = lastEnemies[i].GetComponent<DeathEffect>();
            if (de != null) de.Enable(false);
            Destroy(lastEnemies[i].gameObject);
        }

        // ������� ��������� � �������� ����
        if (_activePlayer != null)
        {
            DeathEffect de = _activePlayer.GetComponent<DeathEffect>();
            if (de != null) de.Enable(false);
            Destroy(_activePlayer.gameObject);
            _activePlayer = null;
        }
    }

    // ����� ������
    private void SpawnPlayer()
    {
        if (_activePlayer == null)
        {
            _activePlayer = Instantiate(_playerPrototype, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError("������� ���������� ��� ������ ��������� ������");
        }
    }

    #region UFO Generator
    public void EnableUfoGenerator(bool flag)
    {
        if (flag)
        {
            if (coroutineUfoGenerator == null)
            {
                coroutineUfoGenerator = StartCoroutine(UfoGenerator(_UfoCooldownTime));
            }
        }
        else
        {
            if (coroutineUfoGenerator != null)
            {
                StopCoroutine(coroutineUfoGenerator);
                coroutineUfoGenerator = null;
            }
        }
    }

    private IEnumerator UfoGenerator(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);

        while (true)
        {
            Ufo go = Instantiate(_ufoPrototype,
                _gameCamera.ScreenToWorldPoint(new Vector3(0.0f,
                                                          Random.Range(0.0f, _gameCamera.scaledPixelHeight),
                                                          -_gameCamera.transform.position.z)),
                Quaternion.identity);

            yield return new WaitForSeconds(cooldownTime);
        }
    }
    #endregion
}
