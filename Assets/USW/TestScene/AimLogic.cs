using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AimLogic : MonoBehaviour {

    private MeshRenderer meshRenderer;

    
    void Start () 
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }
    
    
    void Update () 
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 playerPos = transform.parent.position;
        Vector3 aimVector = (mousePos - playerPos).normalized;

        
        if(Vector3.Distance(mousePos, playerPos) > 0.5f)
        {
            float angle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg - 90f;
            transform.eulerAngles = new Vector3(0, 0, angle);
            meshRenderer.enabled = true;
        }
        else
            meshRenderer.enabled = false;
    }
}