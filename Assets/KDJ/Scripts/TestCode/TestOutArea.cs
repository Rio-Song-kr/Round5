using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestOutArea : MonoBehaviourPun
{
    [SerializeField] private GameObject _borderEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            var rb2d = collision.gameObject.GetComponent<Rigidbody2D>();
            var damagable = collision.gameObject.GetComponent<IDamagable>();
            var playerPhotonView = collision.gameObject.GetComponent<PhotonView>();

            if (rb2d != null && damagable != null)
            {
                rb2d.velocity = Vector2.zero; // 속도 초기화
                // Debug.Log("Border Triggered: " + gameObject.name);
                // Debug.Log("Collision with: " + collision.gameObject.name);
                // Debug.Log("Player PhotonView ID: " + playerPhotonView.ViewID);
                // Debug.Log("Player PhotonView IsMine: " + playerPhotonView.IsMine);
                switch (gameObject.name)
                {
                    case "Left":
                        var effectL = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectL.transform.LookAt(collision.transform.position + Vector3.right);
                        rb2d.AddForce(Vector2.right * 20f, ForceMode2D.Impulse);
                        if (SceneManager.GetActiveScene().name == "KDJ_WeaponTestScene" || !playerPhotonView.IsMine) break;
                        // Debug.Log("경계면 데미지 호출");
                        damagable.TakeDamage(6, collision.transform.position, Vector2.right);
                        break;
                    case "Right":
                        var effectR = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectR.transform.LookAt(collision.transform.position + Vector3.left);
                        rb2d.AddForce(Vector2.left * 20f, ForceMode2D.Impulse);
                        if (SceneManager.GetActiveScene().name == "KDJ_WeaponTestScene" || !playerPhotonView.IsMine) break;
                        // Debug.Log("경계면 데미지 호출");
                        damagable.TakeDamage(6, collision.transform.position, Vector2.left);
                        break;
                    case "Up":
                        //GameObject effectU = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        //effectU.transform.LookAt(collision.transform.position + Vector3.up);
                        //rb2d.AddForce(Vector2.down * 17f, ForceMode2D.Impulse);
                        break;
                    case "Down":
                        var effectD = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectD.transform.LookAt(collision.transform.position + Vector3.up);
                        rb2d.AddForce(Vector2.up * 20f, ForceMode2D.Impulse);
                        if (SceneManager.GetActiveScene().name == "KDJ_WeaponTestScene" || !playerPhotonView.IsMine) break;
                        // Debug.Log("경계면 데미지 호출");
                        damagable.TakeDamage(6, collision.transform.position, Vector2.up);
                        break;
                }
            }
        }
    }
}