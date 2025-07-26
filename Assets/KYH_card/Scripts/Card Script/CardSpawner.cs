using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//public class CardSpawner : MonoBehaviour
//{
//    [SerializeField] private GameObject cardPrefab;
//    [SerializeField] private Transform[] spawnPoints;
//    [SerializeField] private List<CardDataSO> cardDatabase;
//
//    void Start()
//    {
//        SpawnCards(3);
//    }
//
//    void SpawnCards(int count)
//    {
//        var selected = cardDatabase.OrderBy(x => Random.value).Take(count).ToList();
//
//        for (int i = 0; i < count; i++)
//        {
//            GameObject card = Instantiate(cardPrefab, spawnPoints[i].position, Quaternion.identity);
//            var flip = card.GetComponent<FlipCard>();
//            flip.SetData(selected[i]);
//        }
//    }
//}
