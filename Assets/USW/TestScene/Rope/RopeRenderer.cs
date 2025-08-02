using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;

public class RopeRenderer : MonoBehaviour
{
    [SerializeField] private VisualEffect _ropeEffect;
    [SerializeField] private int _maxRopePoints = 20;
    
    private List<Vector3> _ropePoints = new List<Vector3>();
    private bool _isRopeActive;

    private void Awake()
    {
        if (_ropeEffect != null)
        {
            _ropeEffect.enabled = false;
            // VFX Graph에서 사용할 최대 포인트 수 설정
            _ropeEffect.SetInt("MaxPoints", _maxRopePoints);
        }
    }

    public void UpdateRope(List<Vector2> anchors, Vector3 playerPos, Vector3 hookPos, bool hooked)
    {
        if (_ropeEffect == null) return;

        _ropePoints.Clear();

        if (anchors.Count > 0)
        {
            // 갈고리 위치부터 시작
            if (hooked)
                _ropePoints.Add(hookPos);
            else
                _ropePoints.Add(hookPos);

            // 앵커 포인트들 추가 (역순으로)
            for (int i = anchors.Count - 1; i >= 0; i--)
            {
                _ropePoints.Add(anchors[i]);
            }

            // 플레이어 위치 추가
            _ropePoints.Add(playerPos);

            UpdateVFXRope();
        }
        else
        {
            HideRope();
        }
    }

    private void UpdateVFXRope()
    {
        if (!_isRopeActive)
        {
            _ropeEffect.enabled = true;
            _isRopeActive = true;
        }

        // VFX에 포인트 개수 전달
        _ropeEffect.SetInt("PointCount", _ropePoints.Count);

        // 각 포인트를 VFX에 전달
        for (int i = 0; i < _ropePoints.Count && i < _maxRopePoints; i++)
        {
            _ropeEffect.SetVector3($"Point{i}", _ropePoints[i]);
        }

        // 로프 텐션 계산 (선택적)
        float tension = CalculateRopeTension();
        _ropeEffect.SetFloat("Tension", tension);
    }

    private float CalculateRopeTension()
    {
        if (_ropePoints.Count < 2) return 0f;

        float totalLength = 0f;
        for (int i = 0; i < _ropePoints.Count - 1; i++)
        {
            totalLength += Vector3.Distance(_ropePoints[i], _ropePoints[i + 1]);
        }

        // 텐션을 0-1 범위로 정규화
        return Mathf.Clamp01(totalLength / 30f);
    }

    public void HideRope()
    {
        if (_isRopeActive && _ropeEffect != null)
        {
            _ropeEffect.enabled = false;
            _isRopeActive = false;
        }
    }
}
