using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    [SerializeField] private Text _scoreText;                   // Для текста с количеством очков
    [SerializeField] private Text _gameMessageText;             // Для текста сообщения игроку
    [SerializeField] private float _messageBlinkingTime = 0.8f; // Время между сменой состояния активности
    [SerializeField] private List<Image> _liveImages;           // Иконки жизни

    private Coroutine _coroutineMessageBlinking = null;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnLivesChanged += UpdateLives;

        // Обновление в начале жизненного цикла (нет гарантии порядка спавна)
        UpdateScore(GameManager.Instance.CurrentPlayerScore);
        UpdateLives(GameManager.Instance.CurrentLifeCount);
    }

    private void UpdateScore(int value)
    {
        _scoreText.text = $"{value}";
    }

    private void UpdateLives(int value)
    {
        for (int i = 0; i < _liveImages.Count && i < value; ++i)
        {
            _liveImages[i].gameObject.SetActive(true);
        }

        for (int i = value; i <_liveImages.Count; ++i)
        {
            _liveImages[i].gameObject.SetActive(false);
        }
    }

    public void ShowGameMessage(string message, bool blinking = false)
    {
        _gameMessageText.text = message;
        _gameMessageText.gameObject.SetActive(true);

        if (blinking && _coroutineMessageBlinking == null)
        {
            _coroutineMessageBlinking = StartCoroutine(CoroutineMessageBlinking());
        }
        else if (_coroutineMessageBlinking != null)
        {
            StopCoroutine(_coroutineMessageBlinking);
            _coroutineMessageBlinking = null;
        }
    }

    public void HideGameMessage()
    {
        if (_coroutineMessageBlinking != null)
        {
            StopCoroutine(_coroutineMessageBlinking);
            _coroutineMessageBlinking = null;
        }

        _gameMessageText.gameObject.SetActive(false);
    }

    private IEnumerator CoroutineMessageBlinking()
    {
        _gameMessageText.gameObject.SetActive(false);

        while (true)
        {
            _gameMessageText.gameObject.SetActive(!_gameMessageText.gameObject.activeSelf);
            yield return new WaitForSeconds(_messageBlinkingTime);
        }
    }
}
