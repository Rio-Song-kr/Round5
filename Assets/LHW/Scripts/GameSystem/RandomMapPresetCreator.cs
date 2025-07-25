using System.Security.Cryptography;
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

    private void RandomInit()
    {
        for(int i = 0; i < mapResources.Length; i++)
        {
            mapWeightedRandom.Add(mapResources[i], 1);
        }
    }

    private void RandomMapSelect()
    {
        for(int i = 0; i < gameCycleNum; i++)
        {
            GameObject selectedMap = mapWeightedRandom.GetRandomItemBySub();
            Vector2 selectedMapPosition = new Vector2(i * mapTransformOffset, 0);
            Instantiate(selectedMap, selectedMapPosition, Quaternion.identity);
        }
    }
}