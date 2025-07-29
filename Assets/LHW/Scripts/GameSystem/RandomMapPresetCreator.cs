using UnityEngine;

public class RandomMapPresetCreator : MonoBehaviour
{
    // 맵을 Resources로 저장할 거면 해당 방식으로 변경 필요
    [SerializeField] GameObject[] mapResources;

    // 맵 위치 오프셋
    [SerializeField] float mapTransformOffset = 35;    

    // 단일 라운드 수
    [SerializeField] int gameCycleNum = 3;

    private WeightedRandom<GameObject> mapWeightedRandom = new WeightedRandom<GameObject>();

    private void OnEnable()
    {
        RandomInit();
        RandomMapSelect();
    }

    /// <summary>
    /// 랜덤 확률 초기 세팅
    /// </summary>
    private void RandomInit()
    {
        for(int i = 0; i < mapResources.Length; i++)
        {
            mapWeightedRandom.Add(mapResources[i], 1);
        }
    }

    /// <summary>
    /// 랜덤 맵 선택 - 한 번 선택한 맵은 랜덤 확률에서 아예 제외되므로 맵이 중복되지 않게 됨
    /// </summary>
    private void RandomMapSelect()
    {
        for(int i = 0; i < gameCycleNum; i++)
        {
            GameObject selectedMap = mapWeightedRandom.GetRandomItemBySub();
            Vector3 selectedMapPosition = new Vector3(i * mapTransformOffset, 0, 5);
            //PhotonNetwork.Instantiate(selectedMap.name, selectedMapPosition, Quaternion.identity);
            Instantiate(selectedMap, selectedMapPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// 맵의 위치를 반환(x축)
    /// </summary>
    /// <returns></returns>
    public float GetTransformOffset()
    {
        return mapTransformOffset;
    }
}