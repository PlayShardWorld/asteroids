using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public UnityAction<Enemy> OnKilled;                         // Событие при уничтожении сущности игроком

    [SerializeField] int _scoreForKill = 20;                    // Количество очков за уничтожение

    public virtual void GiveDamage(Player author)
    {
        // Добавляем очки в счётчик
        GameManager.Instance.ChangeScore(_scoreForKill);

        // Генерация события и уничтожение объекта
        OnKilled?.Invoke(this);
        Destroy(gameObject);
    }
}
