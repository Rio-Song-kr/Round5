using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSound : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SoundManager.Instance.PlayBGM("BGM1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SoundManager.Instance.PlayBGM("BGM2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SoundManager.Instance.PlayBGM("BGM3");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SoundManager.Instance.PlaySFX("HookSound");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SoundManager.Instance.PlaySFX("JumpSound");
        }

        if (Input.GetMouseButtonDown(0))
        {
            SoundManager.Instance.PlaySFX("ShotSound");
        }
    }
}
