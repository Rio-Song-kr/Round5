using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardBase : ScriptableObject
{
    [Header("카드 정보")]
    public string CardName;
    [TextArea]
    public string CardDescription;
    public Sprite CardImage;
}
