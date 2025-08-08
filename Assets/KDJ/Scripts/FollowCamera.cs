using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private CinemachineVirtualCamera _virtualCamera;
    private GameObject _player;

    private void Awake()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        FindPlayer();
    }


    private void FindPlayer()
    {
        if (_player != null) return;

        _player = GameObject.FindGameObjectWithTag("Player");

        if (_player != null)
        {
            _virtualCamera.Follow = _player.transform;
            _virtualCamera.LookAt = _player.transform;
        }
    }
}
