using System.Collections;
using UnityEngine;

public class IngameCameraMovement : MonoBehaviour
{
    // 단일 게임 종료
    [SerializeField] private bool isRoundOver = false;
    // 게임 세트 종료
    [SerializeField] private bool isRoundSetOver = false;

    // 카메라 왼쪽 이동 시간
    [SerializeField] private float moveLeftDuration = 0.1f;
    // 카메라 왼쪽 이동 오프셋
    [SerializeField] private float moveLeftDistance = 2f;
    // 카메라 오른쪽 이동 시간
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
        // 게임(한 세트)이 종료되었을 때
        if(isRoundSetOver)
        {
            SceneChange();
        }
        // 게임(단일 게임)이 종료되었을 때
        else if (isRoundOver)
        {
            IngameCameraMove();
        }
    }

    /// <summary>
    /// 카메라 움직임 제어
    /// </summary>
    private void IngameCameraMove()
    {
        float offset = creator.GetTransformOffset();
        targetPosition = startPosition + new Vector2(offset, 0);
        cameraCoroutine = StartCoroutine(MoveCamera());

        isRoundOver = false;
    }

    /// <summary>
    /// 카메라 움직임 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveCamera()
    {
        // 카메라를 왼쪽으로 이동
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

        // 카메라를 오른쪽으로 이동
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