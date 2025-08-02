using UnityEngine;
using UnityEngine.VFX;

public class HookPreview : MonoBehaviour
{
    [SerializeField] private VisualEffect _trajectoryEffect;
    [SerializeField] private LayerMask _hookableLayer = -1;
    
    private RaycastHit2D[] _hits = new RaycastHit2D[10];
    private RopeManager _ropeManager;
    private bool _isPreviewActive;

    private void Awake()
    {
        _ropeManager = GetComponent<RopeManager>();
        if (_trajectoryEffect != null)
            _trajectoryEffect.enabled = false;
    }

    private void Update()
    {
        UpdateAiming();
        HandleTrajectoryPreview();
    }

    private void UpdateAiming()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        if (Vector3.Distance(mousePos, transform.position) > 0.5f)
        {
            Vector3 aimDirection = (mousePos - transform.position).normalized;
            transform.up = aimDirection;
        }
    }

    private void HandleTrajectoryPreview()
    {
        // 갈고리가 없고 마우스를 누르고 있을 때만 궤적 표시
        if (!_ropeManager.hook && Input.GetMouseButton(0))
        {
            ShowTrajectoryPreview();
        }
        else
        {
            HideTrajectoryPreview();
        }
    }

    private void ShowTrajectoryPreview()
    {
        if (_trajectoryEffect == null) return;

        _trajectoryEffect.enabled = true;
        _trajectoryEffect.SetVector3("StartPos", transform.position);

        // 갈고리가 닿을 수 있는 거리 계산
        float maxDistance = _ropeManager.maxLength;
        
        if (Physics2D.RaycastNonAlloc(transform.position, transform.up, _hits, maxDistance, _hookableLayer) > 0)
        {
            _trajectoryEffect.SetVector3("EndPos", _hits[0].point);
            _trajectoryEffect.SetBool("HitTarget", true);
        }
        else
        {
            _trajectoryEffect.SetVector3("EndPos", transform.position + transform.up * maxDistance);
            _trajectoryEffect.SetBool("HitTarget", false);
        }

        _isPreviewActive = true;
    }

    private void HideTrajectoryPreview()
    {
        if (_trajectoryEffect != null && _isPreviewActive)
        {
            _trajectoryEffect.enabled = false;
            _isPreviewActive = false;
        }
    }
}