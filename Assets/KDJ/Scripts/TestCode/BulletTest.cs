using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTest : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    private float shotInterval = 0.5f;
    private float shotTimer = 0f;

    void Update()
    {
        LookAtMouse();
        ShotCycle();
    }

    private void ShotCycle()
    {
        shotTimer += Time.deltaTime;
        if (shotTimer >= shotInterval)
        {
            Shot();
            shotTimer = 0f;
        }
    }

    private void Shot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
    }

    private void LookAtMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the z-coordinate is zero for 2D
        Vector3 direction = (mousePosition - transform.position).normalized;
        transform.up = direction; // Set the bullet's rotation to face the mouse
    }
}
