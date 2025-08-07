using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TestWeapon : MonoBehaviour
{   
    [SerializeField] protected Transform _firePoint;
    public abstract void Attack();
}
