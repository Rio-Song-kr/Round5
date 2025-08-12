using Photon.Pun;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class RandomMapPresetCreator : MonoBehaviourPun
{
    // ���� Resources�� ������ �Ÿ� �ش� ������� ���� �ʿ�
    [SerializeField] private GameObject[] mapResources;

    // �� ��ġ ������
    [SerializeField] private float mapTransformOffset = 50;

    [SerializeField] private MapController controller;

    private Coroutine mapUpdateCoroutine;

    public float MapTransformOffset => mapTransformOffset;

    // ���� ���� ��
    [SerializeField] private int gameCycleNum = 3;

    [SerializeField] private Transform[] mapListTransform;

    private WeightedRandom<GameObject> mapWeightedRandom = new WeightedRandom<GameObject>();

    private PoolManager pools;

    public PoolManager Pools => pools;

    private void Awake()
    {
        pools = FindObjectOfType<PoolManager>();
        for (int i = 0; i < mapResources.Length; i++)
        {
            pools.InitializePool(mapResources[i].name, mapResources[i], 3, 9);
        }
    }

    private void OnEnable()
    {
        InGameManager.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        InGameManager.OnGameStart -= OnGameStart;
    }

    private void OnGameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < mapListTransform.Length; i++)
            {
                RandomInit();
                RandomMapSelect(i);
                mapWeightedRandom.ClearList();
                // Debug.Log("�ݺ�");
            }
            MapUpdate(InGameManager.Instance.CurrentMatch);
        }
    }

    /// <summary>
    /// ���� Ȯ�� �ʱ� ����
    /// </summary>
    private void RandomInit()
    {
        for (int i = 0; i < mapResources.Length; i++)
        {
            mapWeightedRandom.Add(mapResources[i], 1);
        }
    }

    /// <summary>
    /// ���� �� ���� - �� �� ������ ���� ���� Ȯ������ �ƿ� ���ܵǹǷ� ���� �ߺ����� �ʰ� ��
    /// </summary>
    private void RandomMapSelect(int round)
    {
        for (int i = 0; i < gameCycleNum; i++)
        {
            var selectedMap = mapWeightedRandom.GetRandomItemBySub();
            var selectedMapPosition = new Vector3((i + 1) * mapTransformOffset, 0, 5);
            var map = PhotonNetwork.Instantiate(selectedMap.name, selectedMapPosition, Quaternion.identity);
            //GameObject map = Instantiate(selectedMap, selectedMapPosition, Quaternion.identity);
            map.transform.SetParent(mapListTransform[round]);

            var mapView = map.GetComponent<PhotonView>();
            mapView.RPC(nameof(MapDynamicMovement.SetParentToRound), RpcTarget.OthersBuffered, round);
            // Debug.Log("����");
        }
    }

    public Transform GetRoundTransform(int round) => mapListTransform[round];

    public void MapUpdate(int match)
    {
        mapUpdateCoroutine = StartCoroutine(MapUpdateCoroutine(match));
    }

    private IEnumerator MapUpdateCoroutine(int match)
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < mapListTransform.Length; i++)
        {
            var MapView = mapListTransform[i].GetComponent<PhotonView>();
            if (match == i)
            {
                Debug.Log($"{match} - Activate");
                MapView.RPC(nameof(RoundActivation.RoundActivate), RpcTarget.All, true);
            }
            else
            {
                Debug.Log($"{match} - Deactivate");
                MapView.RPC(nameof(RoundActivation.RoundActivate), RpcTarget.All, false);
            }
        }
        controller.GoToNextStage();

        mapUpdateCoroutine = null;
    }

    [PunRPC]
    public void ActivateMapCreator(bool activation)
    {
        gameObject.SetActive(activation);
    }
}