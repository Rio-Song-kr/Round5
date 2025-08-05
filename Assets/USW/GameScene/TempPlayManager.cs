using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class TempPlayManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _PlayerPrefab;
    [SerializeField] private Camera _camera;

    private PoolManager _poolManager;
    
    
    [Header("Player Spawn Positions")]
    [SerializeField] private Transform[] playerSpawnPoints = new Transform[2];
    [SerializeField] private Vector2[] defaultSpawnPositions = new Vector2[2] 
    { 
        new Vector2(-5f, -0.7f),  
        new Vector2(5f, -0.7f)   
    };

    private bool hasSpawned = false;

    private void Start()
    {
        _poolManager = FindFirstObjectByType<PoolManager>();
        _poolManager.InitializePool("Player", _PlayerPrefab, 1, 2);
        // SpawnPlayer();
    }

    public override void OnJoinedRoom()
    {
        // 중복 생성 방지
        if (hasSpawned) return;
        SpawnPlayer();
    }
    
    /// <summary>
    /// 플레이어를 고정된 위치에 생성
    /// </summary>
    private void SpawnPlayer()
    {
        Vector2 spawnPosition = GetPlayerSpawnPosition();
        
        GameObject player = PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity);
        hasSpawned = true;
    }
    
    /// <summary>
    /// 플레이어 순서에 따른 스폰 위치 결정
    /// </summary>
    private Vector2 GetPlayerSpawnPosition()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        
        // Transform이 설정되어 있으면 Transform 사용
        // 각 씬마다 배치하기 위함.
        if (playerSpawnPoints != null && playerSpawnPoints.Length > playerIndex && 
            playerSpawnPoints[playerIndex] != null)
        {
            return playerSpawnPoints[playerIndex].position;
        }
        
        // Transform이 없으면 기본 위치 사용
        // 이건 임시용
        if (playerIndex < defaultSpawnPositions.Length)
        {
            return defaultSpawnPositions[playerIndex];
        }
        
        // 예외 상황 - 기본값 반환
        return Vector2.zero;
    }
}