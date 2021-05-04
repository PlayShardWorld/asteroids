using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Player _author;         // Автор снаряда
    private float _speed;           // Скорость полёта снаряда
    private Vector3 _direction;     // Направление полёта
    private Camera _gameCamera;     // Кэширование ссылки на игровую камеру
    private bool _isUsed;           // Флаг для защиты от повторного использования снаряда

    public Projectile Initialize(Player author, float speed, Vector3 direction)
    {
        // Вращение снаряда в сторону полёта
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);

        _author = author;
        _speed = speed;
        _direction = direction.normalized;
        return this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Получение ссылки на камеру и её кэширование
        _gameCamera = FindObjectOfType<Camera>();

        // Удаляем объект, если у него нет автора
        if (_author == null)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // Смещение объекта на игровом поле
        transform.position = transform.position + _direction * _speed * Time.deltaTime;

        // Уничтожение при столкновении с границей игрового поля (viewport'a)
        Vector3 currentScreenPosition = _gameCamera.WorldToScreenPoint(transform.position);
        if (currentScreenPosition.x < 0.0f ||
            currentScreenPosition.x > _gameCamera.scaledPixelWidth ||
            currentScreenPosition.y < 0.0f ||
            currentScreenPosition.y > _gameCamera.scaledPixelHeight)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (!_isUsed && enemy != null)
        {
            // Наносим урон другой сущности и удаляем объект
            enemy.GiveDamage(_author);
            _isUsed = true;
            Destroy(gameObject);
        }
    }
}
