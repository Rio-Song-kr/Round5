using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestPlayer : MonoBehaviour, IDamagable
{
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody2D _rb2d;
    [SerializeField] private GameObject _testEffect;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private Image _hpBar;

    private Vector2 _moveInput;
    private bool _isGrounded;
    private float _maxHp = 50f;
    private float _hp;


    private void Awake()
    {
        _hp = _maxHp;
        SetUI();
    }

    private void Update()
    {
        PlayerHandler();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            _isGrounded = true;
        }

        if (collision.gameObject.layer == 11)
        {
            GameObject effect = Instantiate(_testEffect, collision.contacts[0].point, Quaternion.identity);
            effect.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            _isGrounded = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

    }

    public void TakeDamage(float damage)
    {
        _hp -= damage;
        SetUI();
    }

    private void PlayerHandler()
    {
        InputHandler();
        MoveHandler();
        JumpHandler();
    }

    private void InputHandler()
    {
        _moveInput.x = Input.GetAxis("Horizontal");
    }

    private void MoveHandler()
    {

    }

    private void JumpHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rb2d.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        }
    }

    private void SetUI()
    {
        _hpText.text = $"{_maxHp.ToString("F1")} / {_hp.ToString("F1")}";
        _hpBar.fillAmount = _hp / _maxHp;
    }
}
