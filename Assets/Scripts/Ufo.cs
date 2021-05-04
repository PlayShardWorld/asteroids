using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ufo : Enemy
{
    [SerializeField] private float _lifetime = 10.0f;       // ����� ����� �������
    [SerializeField] private float _movementSpeed = 2.0f;   // �������� ����������� �������
    private float _lifetimeRemaining;                       // ���������� ����� ����� �������
    private Vector3 _movementDirection;                     // ����������� ����������� �������

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DirectionRandomization());
        _lifetimeRemaining = _lifetime;
    }

    // Update is called once per frame
    void Update()
    {
        // �������� ������� � ������������
        transform.position = transform.position + _movementDirection * _movementSpeed * Time.deltaTime;

        _lifetimeRemaining -= Time.deltaTime;
        if (_lifetimeRemaining <= 0.0f)
        {
            AudioManager.Instance.PlayUfoDead();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        AudioManager.Instance.PlayUfoDead();
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnTriggerEnter2D(collision.collider);
    }

    public override void GiveDamage(Player author)
    {
        AudioManager.Instance.PlayUfoDead();
        base.GiveDamage(author);
    }

    private IEnumerator DirectionRandomization()
    {
        do
        {
            // ������������ ����������� �������
            _movementDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
            _movementDirection = _movementDirection.normalized;

            yield return new WaitForSeconds(Random.Range(2.0f, 4.0f));
        } while (true);
    }
}
