using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
[RequireComponent(typeof(TextMeshProUGUI))]
public class FancyTypingText : MonoBehaviour
{
    [TextArea]
    public string fullText;
    public float typingDelay = 0.1f;
    public float glowDelay = 0.05f;
    public float fadeOutDelay = 1.6f;

    private TextMeshProUGUI tmp;
    private Coroutine animationRoutine;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.ForceMeshUpdate();
        animationRoutine = StartCoroutine(PlayTextEffect());
    }

    IEnumerator PlayTextEffect()
    {
        // 1. 타이핑 효과
        for (int i = 0; i < fullText.Length; i++)
        {
            tmp.text += fullText[i];
            tmp.ForceMeshUpdate();
            yield return new WaitForSeconds(typingDelay);
        }

        // 2. 빛나는 효과 (앞글자부터 뒤로)
        int charCount = tmp.textInfo.characterCount;

        for (int i = 0; i < charCount; i++)
        {
            if (!tmp.textInfo.characterInfo[i].isVisible)
                continue;

            tmp.ForceMeshUpdate();
            var colors = tmp.textInfo.meshInfo[tmp.textInfo.characterInfo[i].materialReferenceIndex].colors32;
            int vertexIndex = tmp.textInfo.characterInfo[i].vertexIndex;

            for (int j = 0; j < 4; j++)
            {
                colors[vertexIndex + j] = new Color32(255, 255, 255, 255); // 밝게
            }

            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return new WaitForSeconds(glowDelay);
        }

        // 3. 다시 어둡게 되돌리기
        for (int i = 0; i < charCount; i++)
        {
            if (!tmp.textInfo.characterInfo[i].isVisible)
                continue;

            tmp.ForceMeshUpdate();
            var colors = tmp.textInfo.meshInfo[tmp.textInfo.characterInfo[i].materialReferenceIndex].colors32;
            int vertexIndex = tmp.textInfo.characterInfo[i].vertexIndex;

            for (int j = 0; j < 4; j++)
            {
                colors[vertexIndex + j] = new Color32(160, 160, 160, 255); // 원래 색으로 (살짝 어두움)
            }

            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        // 4. 페이드 아웃
        yield return new WaitForSeconds(fadeOutDelay);
        tmp.DOFade(0, 1f);
    }
}
