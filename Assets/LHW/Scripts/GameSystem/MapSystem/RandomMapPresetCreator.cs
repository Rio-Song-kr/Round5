using Photon.Pun;
using UnityEngine;

public class RandomMapPresetCreator : MonoBehaviourPun
{
    // ���� Resources�� ������ �Ÿ� �ش� ������� ���� �ʿ�
    [SerializeField] GameObject[] mapResources;

    // �� ��ġ ������
    [SerializeField] private float mapTransformOffset = 35;

    public float MapTransformOffset
    {
        get { return mapTransformOffset; }
    }

    // ���� ���� ��
    [SerializeField] int gameCycleNum = 3;

    [SerializeField] Transform[] mapListTransform;

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
        InGameManager.OnRoundStart += OnRoundStart;
    }

    private void OnDisable()
    {
        InGameManager.OnGameStart -= OnGameStart;
        InGameManager.OnRoundStart -= OnRoundStart;
    }

    void OnGameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < mapListTransform.Length; i++)
            {
                RandomInit();
                RandomMapSelect(i);
                mapWeightedRandom.ClearList();
                Debug.Log("�ݺ�");
            }
        }
    }

    void OnRoundStart()
    {
        if (InGameManager.Instance)
        {
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
            GameObject selectedMap = mapWeightedRandom.GetRandomItemBySub();
            Vector3 selectedMapPosition = new Vector3((i + 1) * mapTransformOffset, 0, 5);
            GameObject map = PhotonNetwork.Instantiate(selectedMap.name, selectedMapPosition, Quaternion.identity);
            //GameObject map = Instantiate(selectedMap, selectedMapPosition, Quaternion.identity);
            map.transform.SetParent(mapListTransform[round]);

            PhotonView mapView = map.GetComponent<PhotonView>();
            mapView.RPC(nameof(MapDynamicMovement.SetParentToRound), RpcTarget.OthersBuffered, round);
            Debug.Log("����");
        }
    }

    public Transform GetRoundTransform(int round)
    {
        return mapListTransform[round];
    }

    public void MapUpdate(int round)
    {
        for (int i = 0; i < mapListTransform.Length; i++)
        {
            PhotonView MapView = mapListTransform[i].GetComponent<PhotonView>();
            if (round == i)
            {
                MapView.RPC(nameof(RoundActivation.RoundActivate), RpcTarget.All, true);
            }
            else
            {
                MapView.RPC(nameof(RoundActivation.RoundActivate), RpcTarget.All, false);
            }
        }
    }

    [PunRPC]
    public void ActivateMapCreator(bool activation)
    {
        gameObject.SetActive(activation);
    }
}