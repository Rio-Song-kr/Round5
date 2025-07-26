using System.Collections;
using UnityEngine;

public class IngameCameraMovement : MonoBehaviour
{
    [SerializeField] private bool isRoundOver = false;
    [SerializeField] private bool isRoundSetOver = false;

    [SerializeField] private float moveLeftDuration = 0.1f;
    [SerializeField] private float moveLeftDistance = 2f;
    [SerializeField] private float moveRightDuration = 0.5f;

    // 게임매니저가 없어서 일단 Update로 처리 후 테스트
    // 후에 이벤트로 라운드 및 라운드셋 종료 여부를 받아오는 방법 고려중

    private RandomMapPresetCreator creator;
    private Camera mainCamera;
    private Vector2 startPosition;
    private Vector2 targetPosition;

    private Coroutine cameraCoroutine;

    private void OnEnable()
    {
        creator = GetComponent<RandomMapPresetCreator>();
        mainCamera = Camera.main;
        startPosition = Camera.main.transform.position;
    }

    private void Update()
    {
        if(isRoundSetOver)
        {
            SceneChange();
        }
        else if (isRoundOver)
        {
            IngameCameraMove();
        }
    }

    private void IngameCameraMove()
    {
        float offset = creator.GetTransformOffset();
        targetPosition = startPosition + new Vector2(offset, 0);
        cameraCoroutine = StartCoroutine(MoveCamera());

        isRoundOver = false;
    }

    IEnumerator MoveCamera()
    {
        float elapsedTime = 0f;
        while (elapsedTime < moveLeftDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime/moveLeftDuration;
            
            mainCamera.transform.position = Vector2.Lerp(startPosition, startPosition - new Vector2(moveLeftDistance, 0), Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        mainCamera.transform.position = startPosition - new Vector2(moveLeftDistance, 0);
        startPosition = Camera.main.transform.position;

        float elaspedTime = 0f;

        while(elaspedTime < moveRightDuration)
        {
            elaspedTime += Time.deltaTime;
            float t = elaspedTime/moveRightDuration;

            mainCamera.transform.position = Vector2.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        mainCamera.transform.position = targetPosition;
        startPosition = Camera.main.transform.position;
        cameraCoroutine = null;
    }

    private void SceneChange()
    {
        // 씬 로드 - 용호님 비동기 로드 씬이 어느거지?
    }
}