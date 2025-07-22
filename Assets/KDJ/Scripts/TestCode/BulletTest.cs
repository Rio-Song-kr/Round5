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
}
