using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _effect;    // ������ ��� ������ �� ���������
    private bool _isUsed = true;                        // ���� ���������� �������
    private bool _isQuitting = false;                   // ���� ���������� ������ ����������

    private void Awake()
    {
        Application.quitting += () => _isQuitting = true;
    }

    private void OnDestroy()
    {
        if (!_isQuitting && _isUsed && _effect != null)
        {
            Instantiate(_effect, transform.position, Quaternion.identity);
        }
    }

    public void Enable(bool flag)
    {
        _isUsed = flag;
    }
}
