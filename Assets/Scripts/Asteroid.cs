using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : Enemy
{
    [SerializeField] private Asteroid _asteroidDebrisPrototype;     // Прототип для осколков астероида
    [SerializeField] private int _asteroidPartsCount;               // Количество осколков астероида

    [SerializeField] private float _movementSpeed = 2.0f;   // Скорость перемещения объекта
    private Vector3 _movementDirection;                     // Направление перемещения объекта

    // Start is called before the first frame update
    void Start()
    {
        // Рандомизация и нормализация направления объекта
        _movementDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
        _movementDirection = _movementDirection.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        // Смещение объекта в пространстве
        transform.position = transform.position + _movementDirection * _movementSpeed * Time.deltaTime;
    }

    public override void GiveDamage(Player author)
    {
        AudioManager.Instance.PlayAsteroidDead();

        // Спавним и регистрируем части астероидов, если они должны быть
        if (_asteroidDebrisPrototype != null)
        {
            for (int i = 0; i < _asteroidPartsCount; ++i)
            {
                Asteroid asteroid = Instantiate(_asteroidDebrisPrototype, transform.position, Quaternion.identity);
                GameManager.Instance.RegisterEnemy(asteroid);
            }
        }

        base.GiveDamage(author);
    }
}
