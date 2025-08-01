using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCursor : MonoBehaviour
{
    [SerializeField] private GameObject _cursor;

    private void Awake()
    {
        Cursor.visible = false; // 커서를 숨깁니다.
    }

    void Update()
    {
        MoveCursor();
    }

    private void MoveCursor()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 2D 게임이므로 z축은 0으로 설정
        _cursor.transform.position = mousePos;
    }
}
