using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
    [SerializeField] private GameObject _deadEffect1;
    [SerializeField] private GameObject _deadEffect2;

    private Vector2 _moveInput;
    private bool _isGrounded;
    private float _maxHp = 50f;
    private float _hp;

    public event Action OnDeath;

    public bool IsAlive => throw new NotImplementedException();

    private void Awake()
    {
        _hp = _maxHp;
        SetUI();
    }

    private void Update()
    {
        //PlayerHandler();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            _isGrounded = true;
        }

        if (collision.gameObject.layer == 11)
        {
            var effect = Instantiate(_testEffect, collision.contacts[0].point, Quaternion.identity);
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

    public void TakeDamage(float damage, Vector2 position, Vector2 direction)
    {
        SoundManager.Instance.PlaySFX("PlayerHitSound" + UnityEngine.Random.Range(1, 3));
        _hp -= damage;
        SetUI();

        if (_hp <= 0)
        {
            _hp = 0;

            var effect1 = Instantiate(_deadEffect1, transform.position, Quaternion.identity);
            var effect2 = Instantiate(_deadEffect2, transform.position, Quaternion.identity);
            effect1.transform.LookAt(position + direction);
            effect2.transform.LookAt(position + direction);

            Destroy(gameObject);
        }
    }

    private void PlayerHandler()
    {
        InputHandler();
        MoveHandler();
    }

    private void InputHandler()
    {
        _moveInput.x = Input.GetAxis("Horizontal");
    }

    private void MoveHandler()
    {
    }

    private void SetUI()
    {
        _hpText.text = $"{_hp.ToString("F1")} / {_maxHp.ToString("F1")}";
        _hpBar.fillAmount = _hp / _maxHp;
    }
}