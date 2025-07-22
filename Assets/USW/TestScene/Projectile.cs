using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour {

    //public float speed; 
    public GameObject owner;
    protected RopeManager ropeManager;
    protected  float speed;
    public int layerPlayer = 8;
    public int layerHook = 9;

    protected virtual void Awake(){
    }

    
    protected virtual void Start () {
        ropeManager = owner.GetComponent<RopeManager>();
    }
}