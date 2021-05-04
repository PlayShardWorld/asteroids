using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ������������� �������� � ��������������� ������� Viewport'�
 * ��� ����������� ������� Transform'��
 */
public class BorderTeleport : MonoBehaviour
{
    private Camera _gameCamera;  // ������ �� ������� ������ (�����������)

    // Start is called before the first frame update
    void Start()
    {
        _gameCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // ������������ ��� ������ �� �������
        Vector3 currentScreenPosition = _gameCamera.WorldToScreenPoint(transform.position);
        if (currentScreenPosition.x < 0.0f) currentScreenPosition.x = _gameCamera.scaledPixelWidth;
        if (currentScreenPosition.x > _gameCamera.scaledPixelWidth) currentScreenPosition.x = 0.0f;
        if (currentScreenPosition.y < 0.0f) currentScreenPosition.y = _gameCamera.scaledPixelHeight;
        if (currentScreenPosition.y > _gameCamera.scaledPixelHeight) currentScreenPosition.y = 0.0f;
        transform.position = _gameCamera.ScreenToWorldPoint(currentScreenPosition);
    }
}
