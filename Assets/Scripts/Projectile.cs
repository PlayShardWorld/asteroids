using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Player _author;         // ����� �������
    private float _speed;           // �������� ����� �������
    private Vector3 _direction;     // ����������� �����
    private Camera _gameCamera;     // ����������� ������ �� ������� ������
    private bool _isUsed;           // ���� ��� ������ �� ���������� ������������� �������

    public Projectile Initialize(Player author, float speed, Vector3 direction)
    {
        // �������� ������� � ������� �����
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);

        _author = author;
        _speed = speed;
        _direction = direction.normalized;
        return this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // ��������� ������ �� ������ � � �����������
        _gameCamera = FindObjectOfType<Camera>();

        // ������� ������, ���� � ���� ��� ������
        if (_author == null)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // �������� ������� �� ������� ����
        transform.position = transform.position + _direction * _speed * Time.deltaTime;

        // ����������� ��� ������������ � �������� �������� ���� (viewport'a)
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
            // ������� ���� ������ �������� � ������� ������
            enemy.GiveDamage(_author);
            _isUsed = true;
            Destroy(gameObject);
        }
    }
}
