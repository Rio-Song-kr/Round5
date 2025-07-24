using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Arc들을 생성하고, 모든 Arc가 사라졌는지 확인하여 자신을 파괴하는 역할만 수행
/// 모든 확장 및 이동 로직은 ArcController가 독립적으로 처리
/// </summary>
public class EMPEffect : MonoBehaviour
{
    [Header("Expansion Settings")]
    // # 초기 확장 속도
    [SerializeField] private float _initialExpansionSpeed = 10f;
    // # 최소 확장 속도
    [SerializeField] private float _minExpansionSpeed = 2f;
    // # 빠른 확장이 끝나는 지점의 반경
    [SerializeField] private float _fastExpansionRadius = 3f;
    // # 빠른 확장 속도에서 최소 속도로 감속되는 데 걸리는 시간
    [SerializeField] private float _decelerationDuration = 2f;

    [Header("Arc Settings")]
    // # 원형 확장에 사용될 개별 Arc 프리팹
    [SerializeField] private GameObject _arcPrefab;
    // # 생성할 Arc의 총 개수
    [SerializeField] private int _arcCount = 30;
    // # 생성될 Arc의 크기
    [SerializeField] private float _arcSize = 2f;

    // # 생성된 Arc 오브젝트들을 관리하는 리스트
    private List<GameObject> _arcs = new List<GameObject>();

    /// <summary>
    /// Arc를 생성하고, 모든 Arc가 완료되었는지 확인하는 코루틴을 시작
    /// </summary>
    private void Start()
    {
        CreateArcs();
        StartCoroutine(CheckForCompletion());
    }

    /// <summary>
    /// 설정된 개수만큼 Arc를 생성하고, 각 ArcController에 필요한 모든 설정값을 전달하여 초기화
    /// </summary>
    private void CreateArcs()
    {
        for (int i = 0; i < _arcCount; i++)
        {
            GameObject arcInstance;
            if (_arcPrefab != null)
            {
                // # 프리팹이 지정된 경우, 해당 프리팹을 사용해 Arc를 생성함
                arcInstance = Instantiate(_arcPrefab, transform.position, Quaternion.identity, transform);
            }
            else
            {
                // # 프리팹이 없는 경우, 필요한 컴포넌트를 가진 기본 Arc를 동적으로 생성함
                arcInstance = new GameObject($"Arc_{i}");
                arcInstance.transform.position = transform.position;

                var spriteRenderer = arcInstance.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateCircleSprite();

                var rb = arcInstance.AddComponent<Rigidbody2D>();
                rb.isKinematic = true;
                rb.gravityScale = 0;

                arcInstance.AddComponent<BoxCollider2D>().isTrigger = true;
                arcInstance.AddComponent<ArcController>();
            }

            arcInstance.transform.localScale = Vector3.one * _arcSize;

            // # Arc가 확장될 방향과 초기 회전값을 계산
            float angle = i * 2f * Mathf.PI / _arcCount;

            // # 이 방향을 전달
            Vector3 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            arcInstance.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

            // # ArcController를 가져와서 필요한 정보를 전달하며 초기화
            var arcController = arcInstance.GetComponent<ArcController>();
            if (arcController != null)
            {
                arcController.Initialize(transform.position, direction, _initialExpansionSpeed, _minExpansionSpeed,
                    _fastExpansionRadius, _decelerationDuration);
            }

            // # 비활성화 상태로 리스트에 추가
            arcInstance.SetActive(false);
            _arcs.Add(arcInstance);
        }
    }

    /// <summary>
    /// 모든 Arc가 비활성화될 때까지 주기적으로 확인하여 자신을 파괴하는 코루틴
    /// </summary>
    private IEnumerator CheckForCompletion()
    {
        // # 모든 Arc를 활성화하여 동작을 시작
        foreach (var arc in _arcs)
        {
            arc.SetActive(true);
        }

        // # 모든 Arc가 비활성화될 때까지 대기
        yield return new WaitUntil(() => _arcs.All(arc => !arc.activeSelf));

        // # 모든 작업이 완료되었으므로 자신을 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// Arc 프리팹이 없을 때 사용할 기본 원형 스프라이트를 생성
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        var texture = new Texture2D(32, 32);
        var center = new Vector2(16, 16);

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= 15 ? Color.white : Color.clear);
            }
        }

        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// 이펙트 오브젝트가 파괴될 때, 생성했던 모든 Arc 오브젝트를 함께 파괴하여 메모리 누수를 방지
    /// </summary>
    private void OnDestroy()
    {
        foreach (var arc in _arcs)
        {
            if (arc != null)
            {
                Destroy(arc);
            }
        }

        _arcs.Clear();
    }
}