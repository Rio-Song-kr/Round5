using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCardSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _cardSelectScene;
    public static TestCardSceneManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        OnCardSelectScene();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnCardSelectScene();
        }
    }

    public void OnCardSelectScene()
    {
        _cardSelectScene.SetActive(true);
    }

    public void OffCardSelectScene()
    {
        _cardSelectScene.SetActive(false);
    }
}
