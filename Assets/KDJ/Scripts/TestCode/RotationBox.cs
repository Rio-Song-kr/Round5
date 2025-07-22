using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationBox : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.forward * Time.deltaTime * -50f);
    }
}
