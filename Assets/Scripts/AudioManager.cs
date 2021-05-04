using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Preparing
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("Для начала добавьте AudioManager на сцену");

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
        instance.name = "Manager: Audio";
    }
    #endregion

    [SerializeField] private AudioSource _playerDeadSound;              // Звук при смерти персонажа
    [SerializeField] private AudioSource _playerFireSound;              // Звук при выстреле
    [SerializeField] private AudioSource _playerNozzleSound;            // Звук при работе двигателя
    [SerializeField] private AudioSource _asteroidDeadSound;            // Звук при смерти персонажа
    [SerializeField] private AudioSource _ufoDeadSound;                 // Звук при смерти персонажа

    public void EnablePlayerNozzle(bool flag)
    {
        if (_playerNozzleSound != null)
        {
            if (flag)
            {
                if (!_playerNozzleSound.isPlaying)
                    _playerNozzleSound.Play();
            }
            else
            {
                if (_playerNozzleSound.isPlaying)
                    _playerNozzleSound.Stop();
            }
        }
    }

    public void PlayPlayerFire()
    {
        if (_playerFireSound != null)
        {
            _playerFireSound.PlayOneShot(_playerFireSound.clip);
        }
    }

    public void PlayPlayerDead()
    {
        if (_playerDeadSound != null)
        {
            _playerDeadSound.PlayOneShot(_playerDeadSound.clip);
        }
    }

    public void PlayAsteroidDead()
    {
        if (_asteroidDeadSound != null)
        {
            _asteroidDeadSound.PlayOneShot(_asteroidDeadSound.clip);
        }
    }

    public void PlayUfoDead()
    {
        if (_ufoDeadSound != null)
        {
            _ufoDeadSound.PlayOneShot(_ufoDeadSound.clip);
        }
    }
}
