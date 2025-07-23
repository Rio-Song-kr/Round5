using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTest : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _shotInterval;

    private CameraShake _cameraShake;
    
    private float _shotTimer = 0f;

    void Awake()
    {
        _cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    void Update()
    {
        LookAtMouse();
        ShotCycle();
    }

    private void ShotCycle()
    {
        _shotTimer += Time.deltaTime;
        if (_shotTimer >= _shotInterval)
        {
            Shot();
            _shotTimer = 0f;
        }
    }

    private void Shot()
    {
        GameObject bullet = Instantiate(_bulletPrefab, transform.position, transform.rotation);
        _cameraShake.ShakeCaller(0.2f, 0.05f);
    }

    private void LookAtMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the z-coordinate is zero for 2D
        Vector3 direction = (mousePosition - transform.position).normalized;
        transform.up = direction; // Set the bullet's rotation to face the mouse
    }
}
