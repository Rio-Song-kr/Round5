using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelAlpha : MonoBehaviour
{
    public Image targetImage;
    public float duration = 2f; // 한 사이클 시간
    public float fixedAlpha = 200f / 255f;
    
    void Start()
    {
        StartCoroutine(ColorCycle());
    }
    
    IEnumerator ColorCycle()
    {
        while (true)
        {
            for (float i = 0; i <= 1; i += Time.deltaTime / duration)
            {
                Color newColor = Color.HSVToRGB(i, 1f, 1f);
                newColor.a = fixedAlpha;
                targetImage.color = newColor;
                yield return null;
            }
        }
    }
}