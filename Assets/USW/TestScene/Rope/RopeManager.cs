using UnityEngine;

public class RopeManager : MonoBehaviour 
{
    public float ropeHookSpeed = 150;
    public float ropeHookSpeedDamp = 0.1f;
    public float ropeClimbSpeed = 1;
    public float maxLength = 30;
    public bool hooked;

    [HideInInspector]
    public GameObject hook;
    [HideInInspector]
    public RopeLogic hookScript; 
    [HideInInspector]
    public Transform crosshair;
    
    private HookPreview trajectoryPreview;
    
    void Awake()
    {
        crosshair = transform.Find("Crosshair");
        trajectoryPreview = GetComponent<HookPreview>();
        
        // 필요한 컴포넌트들이 없으면 추가
        if (GetComponent<RopeRenderer>() == null)
        {
            gameObject.AddComponent<RopeRenderer>();
        }
        if (trajectoryPreview == null)
        {
            gameObject.AddComponent<HookPreview>();
        }
    }

    void Update() 
    {
        if (Input.GetMouseButtonUp(0))  // 마우스 버튼을 뗄 때 갈고리 발사
        {
            if (hook)
            {
                DestroyHook();
            }
            else
                SpawnHook();
        }
        
        if (Input.GetButtonDown("Jump") && hook)
        {
            DestroyHook();
        }
    }

    void SpawnHook()
    {
        hook = Instantiate(Resources.Load("RopeHook"), crosshair.position + crosshair.up * 1.5f, crosshair.rotation) as GameObject;
        hookScript = hook.GetComponent<RopeLogic>();
        hookScript.owner = gameObject;
    }

    public void DestroyHook()
    {
        if (hook != null)
        {
            Destroy(hook);
        }
        gameObject.GetComponent<SpringJoint2D>().enabled = false;
        hook = null;
        hookScript = null;
    }
}