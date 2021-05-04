using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private ParticleSystem _nozzleEffect;              // Эффект огня из сопла при перемещении

    // Управление стрельбой
    [SerializeField] private Projectile _projectilePrototype;           // Прототип снаряда
    [SerializeField] private float _projectileSpeed = 20.0f;            // Скорость снаряда
    [SerializeField] private float _projectileCooldownDefault = 0.3f;   // Откат выстрела
    private float _projectileCooldownRemaining;                         // Текущий остаток отката

    // Управление перемещением
    [SerializeField] private float _rotationSpeed = 180.0f;             // Скорость вращения кнопками
    [SerializeField] private float _acceleration = 5.0f;                // Ускорение перемещения
    private Vector3 _currentMovementInertia;                            // Текущая инерция

    private Camera _gameCamera;                                         // Ссылка на игровую камеру (кэширование)

    // Start is called before the first frame update
    void Start()
    {
        _gameCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        #region Control
        if (Input.GetKey(KeyCode.W))
        {
            _currentMovementInertia = _currentMovementInertia + transform.up * _acceleration * Time.deltaTime;
            var emission = _nozzleEffect.emission;
            emission.enabled = true;
            AudioManager.Instance.EnablePlayerNozzle(true);
        }
        else
        {
            var emission = _nozzleEffect.emission;
            emission.enabled = false;
            AudioManager.Instance.EnablePlayerNozzle(false);
        }

        if (Input.GetMouseButton(0))
        {
            // Вращение по направлению к клику мыши
            Vector2 mousePosition = _gameCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg - 90);
        }

        if (Input.GetMouseButton(1))
        {
            // Вращение по направлению к клику мыши
            Vector2 mousePosition = _gameCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg - 90);
            TryFire(mousePosition);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0.0f, 0.0f, _rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0.0f, 0.0f, -_rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            TryFire(transform.up);
        }
        #endregion

        // Перемещение игрока в пространстве
        transform.position = transform.position + _currentMovementInertia * Time.deltaTime;

        // Откат выстрела
        _projectileCooldownRemaining -= Time.deltaTime;
        _projectileCooldownRemaining = Mathf.Clamp(_projectileCooldownRemaining, 0.0f, float.MaxValue);
    }

    public void TryFire(Vector2 fireAim)
    {
        if (_projectileCooldownRemaining == 0.0f)
        {
            Instantiate<Projectile>(_projectilePrototype, transform.position, Quaternion.identity)
                .Initialize(this, _projectileSpeed, fireAim);

            AudioManager.Instance.PlayPlayerFire();

            _projectileCooldownRemaining = _projectileCooldownDefault;
        }
    }

    public void Die()
    {
        GameManager.Instance.PlayerDied();
        AudioManager.Instance.PlayPlayerDead();
        AudioManager.Instance.EnablePlayerNozzle(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Die();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Enemy>() != null)
        {
            OnTriggerEnter2D(collision.collider);
        }
    }
}
