using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuzzSawController : MonoBehaviour
{
    [SerializeField] private float speed = 1;

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(new Vector3(0, 0, speed));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            // TODO : 플레이어 데미지 구현
        }
    }
}
