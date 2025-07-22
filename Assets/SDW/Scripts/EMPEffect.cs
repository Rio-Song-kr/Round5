using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMPEffect : MonoBehaviour
{
    [Header("Expansion Settings")]
    [SerializeField] private float _expansionSpeed = 3f; // 확대 속도
    [SerializeField] private float _maxRadius = 5f; // 최대 반지름
    [SerializeField] private LayerMask _obstacleLayer; // 장애물 레이어

    [Header("Visual Settings")]
    [SerializeField] private GameObject _dotPrefab; // 점선을 구성할 점 프리팹
    [SerializeField] private int _dotCount = 30; // 점의 개수
    [SerializeField] private float _dotSize = 0.5f; // 점의 크기

    [Header("Collision Settings")]
    [SerializeField] private float _collisionCheckRadius = 0.05f; // 충돌 검사 반지름

    private CircleCollider2D _empCollider; // EMP 콜라이더
    private List<GameObject> _dots = new List<GameObject>();
    private List<bool> _dotVisible = new List<bool>(); // 각 점의 표시 여부
    private float _currentRadius = 0.5f;
    private bool _isExpanding = true;

    private void Start()
    {
        SetupCollider();
        CreateDots();
        StartCoroutine(ExpandEffect());
    }

    private void SetupCollider()
    {
        // EMP 콜라이더 설정
        _empCollider = GetComponent<CircleCollider2D>();
        if (_empCollider == null)
        {
            _empCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        _empCollider.isTrigger = true;
        _empCollider.radius = _currentRadius; // 현재 반지름으로 시작
    }

    private void CreateDots()
    {
        // 점선 원을 구성할 점들 생성
        for (int i = 0; i < _dotCount; i++)
        {
            GameObject dot;

            if (_dotPrefab != null)
            {
                dot = Instantiate(_dotPrefab, transform);
            }
            else
            {
                // 기본 점 생성 (원형 스프라이트)
                dot = new GameObject($"Dot_{i}");
                dot.transform.SetParent(transform);

                var spriteRenderer = dot.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateCircleSprite();
            }

            dot.transform.localScale = Vector3.one * _dotSize;
            dot.SetActive(false); // 처음에는 비활성화
            _dots.Add(dot);
            _dotVisible.Add(true);
        }
    }

    private Sprite CreateCircleSprite()
    {
        // 간단한 원형 스프라이트 생성
        var texture = new Texture2D(32, 32);
        var center = new Vector2(16, 16);

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 15)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    private IEnumerator ExpandEffect()
    {
        while (_isExpanding && _currentRadius < _maxRadius)
        {
            _currentRadius += _expansionSpeed * Time.deltaTime;
            UpdateDotPositions();

            if (_currentRadius >= _maxRadius)
            {
                _isExpanding = false;
                // 효과 종료 후 페이드아웃 또는 제거
                StartCoroutine(FadeOut());
            }

            yield return null;
        }
    }

    private void UpdateDotPositions()
    {
        // 콜라이더 크기도 함께 업데이트
        _empCollider.radius = _currentRadius;

        for (int i = 0; i < _dots.Count; i++)
        {
            if (!_dotVisible[i]) continue;

            float angle = i * 2f * Mathf.PI / _dots.Count;
            var localPosition = new Vector2(
                Mathf.Cos(angle) * _currentRadius,
                Mathf.Sin(angle) * _currentRadius
            );

            // 점 위치 설정
            _dots[i].transform.localPosition = localPosition;

            // 점 회전 설정 (각도에 맞춰 회전)
            float rotationAngle = angle * Mathf.Rad2Deg;
            _dots[i].transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);

            // 점 활성화
            _dots[i].SetActive(true);

            // 충돌 검사
            CheckDotCollision(i, _dots[i].transform.position);
        }
    }

    private void CheckDotCollision(int dotIndex, Vector3 worldPosition)
    {
        // 해당 위치에서 장애물 검사 (작은 원형 영역으로 검사)
        var hitCollider = Physics2D.OverlapCircle(worldPosition, _collisionCheckRadius, _obstacleLayer);

        if (hitCollider != null)
        {
            // 장애물에 부딪힌 점은 사라짐
            _dotVisible[dotIndex] = false;
            _dots[dotIndex].SetActive(false);
        }
    }

    private IEnumerator FadeOutHitEffect(GameObject effect)
    {
        var spriteRenderer = effect.GetComponent<SpriteRenderer>();
        var originalColor = spriteRenderer.color;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(effect);
    }

    private IEnumerator FadeOut()
    {
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            foreach (var dot in _dots)
            {
                if (dot != null && dot.activeInHierarchy)
                {
                    var spriteRenderer = dot.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        var color = spriteRenderer.color;
                        spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
                    }
                }
            }

            yield return null;
        }

        // 효과 종료
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // 생성된 모든 점들 정리
        foreach (var dot in _dots)
        {
            if (dot != null)
                DestroyImmediate(dot);
        }
        _dots.Clear();
    }
}