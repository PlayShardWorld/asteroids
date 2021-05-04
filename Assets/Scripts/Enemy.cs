using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public UnityAction<Enemy> OnKilled;                         // ������� ��� ����������� �������� �������

    [SerializeField] int _scoreForKill = 20;                    // ���������� ����� �� �����������

    public virtual void GiveDamage(Player author)
    {
        // ��������� ���� � �������
        GameManager.Instance.ChangeScore(_scoreForKill);

        // ��������� ������� � ����������� �������
        OnKilled?.Invoke(this);
        Destroy(gameObject);
    }
}
