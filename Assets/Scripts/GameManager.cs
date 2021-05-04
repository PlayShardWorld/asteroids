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
                Debug.LogError("Для начала добавьте GameManager на сцену");

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

    [SerializeField] private UIGame _uiGame;                        // Ссылка на игровой интерфейс
    [SerializeField] private Camera _gameCamera;                    // Ссылка на игровую камеру
    [SerializeField] private Player _playerPrototype;               // Прототип игрока
    [SerializeField] private Asteroid _asteroidPrototype;           // Прототип основного астероида
    [SerializeField] private Ufo _ufoPrototype;                     // Прототип летающей тарелки

    // Переменные для контроля сложности волн
    [SerializeField] private int _enemyStartCount = 4;              // Количество противников на первой волне
    [SerializeField] private int _enemyDeltaCount = 2;              // Приращение количества противников с каждой волной
    [SerializeField] private int _enemyMaxCount = 12;               // Максимальное количество противников
    [SerializeField] private float _UfoCooldownTime = 30.0f;        // Откат между спавнами НЛО
    private int _enemyLastWaveCount;                                // Количество противников на последней запущенной волне

    private GameState _currentGameState;                            // Текущее игровое состояние
    private Player _activePlayer;                                   // Ссылка на персонажа игрока
    private List<Enemy> _activeAsteroidList = new List<Enemy>();    // Список активных астероидов для отслеживания

    [SerializeField] private int _defaultLifeCount = 3;             // Количество жизней по умолчанию
    private int _lifeCount = 0;                                     // Текущее количество жизней
    private int _playerScore;                                       // Текущее количество очков

    public int CurrentLifeCount => _lifeCount;
    public int CurrentPlayerScore => _playerScore;

    private Coroutine coroutineUfoGenerator;
    private Coroutine coroutineGameOver;

    // Start is called before the first frame update
    void Start()
    {
        // При первом запуске игры переводим состояние в Reset
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
    // Перевод состояния игры в GameState.Demo
    private void ResetGame()
    {
        // Выключаем генератор летающих тарелок
        EnableUfoGenerator(false);

        // Удаляем все сущности с игрового поля
        RemoveAllCreatures();

        // Сбрасываем счётчик очков и жизней
        ResetScore();
        ResetLives();

        // Запускаем демонстрацию игры (просто спавним 6 комет)
        SpawnWave(6);

        // Выводим сообщение с призывом начать игру
        _uiGame.ShowGameMessage("CLICK TO START", true);

        _currentGameState = GameState.Demo;
    }

    // Перевод состояния игры в GameState.Game
    private IEnumerator StartGame()
    {
        // Удаляем все сущности с игрового поля
        RemoveAllCreatures();

        // Сбрасываем счётчик очков и жизней
        ResetScore();
        ResetLives(_defaultLifeCount);

        _uiGame.ShowGameMessage("READY");

        _currentGameState = GameState.Loading;

        // Корутин чисто для имитации экрана загрузки
        yield return new WaitForSeconds(2.0f);

        _uiGame.HideGameMessage();

        // Спавним игрока
        SpawnPlayer();

        // Спавним волну
        SpawnWave(_enemyStartCount);

        // Запускаем корутин генерации летающих тарелок
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

    // Регистрация смерти игрока
    public void PlayerDied()
    {
        _activePlayer = null;

        // Отнимаем жизнь у игрока
        ChangeLives(-1);

        // Если жизни закончились - GameOver, иначе - даём возможность заспавниться в нужный момент
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

    // Регистрация противника
    public void RegisterEnemy(Enemy enemy)
    {
        enemy.OnKilled += OnEnemyKilledHandler;
        _activeAsteroidList.Add(enemy);
    }

    // Обработчик события смерти противника
    private void OnEnemyKilledHandler(Enemy enemy)
    {
        _activeAsteroidList.Remove(enemy);

        // Если противники на волне закончились - запускаем новую волну
        if (_activeAsteroidList.Count == 0)
        {
            SpawnWave(Mathf.Clamp(_enemyLastWaveCount + _enemyDeltaCount, 0, _enemyMaxCount));
        }
    }

    // Спавн новой волны
    private void SpawnWave(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            Asteroid go = Instantiate(_asteroidPrototype, 
                _gameCamera.ScreenToWorldPoint(new Vector3(0.0f, 
                                                          Random.Range(0.0f, _gameCamera.scaledPixelHeight),
                                                          -_gameCamera.transform.position.z)),
                Quaternion.identity);

            // Подписываемся на событие смерти астероида
            go.OnKilled += OnEnemyKilledHandler;

            // Регистрируем каждый астероид
            _activeAsteroidList.Add(go);
        }

        _enemyLastWaveCount = count;
    }

    // Удаление всех игровых сущностей с поля
    private void RemoveAllCreatures()
    {
        // Удаляем все астероиды с игрового поля
        for (int i = 0; i < _activeAsteroidList.Count; ++i)
        {
            // Отписываемся от события смерти астероида и уничтожаем его
            _activeAsteroidList[i].OnKilled -= OnEnemyKilledHandler;
            DeathEffect de = _activeAsteroidList[i].GetComponent<DeathEffect>();
            if (de != null) de.Enable(false);
            Destroy(_activeAsteroidList[i].gameObject);
        }
        _activeAsteroidList.Clear();

        // Удаляем все летающие тарелки (и оставшихся противников, если так случилось)
        Enemy[] lastEnemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < lastEnemies.Length; ++i)
        {
            // Отписываемся от события смерти сущности и уничтожаем её
            lastEnemies[i].OnKilled -= OnEnemyKilledHandler;
            DeathEffect de = lastEnemies[i].GetComponent<DeathEffect>();
            if (de != null) de.Enable(false);
            Destroy(lastEnemies[i].gameObject);
        }

        // Удаляем персонажа с игрового поля
        if (_activePlayer != null)
        {
            DeathEffect de = _activePlayer.GetComponent<DeathEffect>();
            if (de != null) de.Enable(false);
            Destroy(_activePlayer.gameObject);
            _activePlayer = null;
        }
    }

    // Спавн игрока
    private void SpawnPlayer()
    {
        if (_activePlayer == null)
        {
            _activePlayer = Instantiate(_playerPrototype, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Попытка заспавнить ещё одного персонажа игрока");
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
