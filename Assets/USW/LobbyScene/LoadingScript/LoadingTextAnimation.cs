using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class LoadingTextAnimation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private string[] hexColors;
    [SerializeField] private float animationDuration = 3f;

    private Color[] colors;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        colors = new Color[hexColors.Length];
        for (int i = 0; i < hexColors.Length; i++)
        {
            Color c;
            if(ColorUtility.TryParseHtmlString("#" + hexColors[i], out c))
                colors[i] = c;
            else
                colors[i] = Color.white;
        }
    }

    private void OnEnable()
    {
        if (colors != null)
        {
            StartCoroutine(AnimateGradientColors());
        }
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }

    IEnumerator AnimateGradientColors()
    {
        tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmp.textInfo;

        while (true)
        {
            float time = 0f;
            while (time < animationDuration)
            {
                time += Time.deltaTime;
                float progress = time / animationDuration;

                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible) continue;

                    // 각 글자의 현재 컬러 인덱스와 다음 인덱스
                    int colorIdxA = (i + Mathf.FloorToInt(progress * colors.Length)) % colors.Length;
                    int colorIdxB = (colorIdxA + 1) % colors.Length;
                    
                    Color cA = colors[colorIdxA];
                    Color cB = colors[colorIdxB];

                    // 부드럽게 보간
                    Color lerped = Color.Lerp(cA, cB, progress * colors.Length % 1f);

                    int meshIdx = textInfo.characterInfo[i].materialReferenceIndex;
                    int vertexIdx = textInfo.characterInfo[i].vertexIndex;

                    var vertexColors = textInfo.meshInfo[meshIdx].colors32;
                    for (int v = 0; v < 4; v++)
                        vertexColors[vertexIdx + v] = lerped;
                }

                // 변경 적용
                for (int m = 0; m < textInfo.meshInfo.Length; m++)
                {
                    textInfo.meshInfo[m].mesh.colors32 = textInfo.meshInfo[m].colors32;
                    tmp.UpdateGeometry(textInfo.meshInfo[m].mesh, m);
                }

                yield return null;
            }
        }
    }

    public void BeginGradation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            Debug.Log("기존애니메이션");
        }
        // 애니메이션 코루틴이 있으면 실행중지 떄리면 끝아님 ? 
        animationCoroutine = StartCoroutine(AnimateGradientColors());
    }
    
}
