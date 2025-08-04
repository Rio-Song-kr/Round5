using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinnerTextController : MonoBehaviourPun
{
    private TMP_Text winnerText;

    Color textColor;
    string leftTextColor = "#FF8400";
    string rightTextColor = "#009EFF";

    private void Awake()
    {
        winnerText = GetComponent<TMP_Text>();
    }

    [PunRPC]
    public void RPC_TextInit(string winner, int left, int right)
    {
        if (winner == "Left")
        {
            if (ColorUtility.TryParseHtmlString(leftTextColor, out textColor))
            {
                winnerText.color = textColor;
            }

            if (left == 1)
            {
                winnerText.text = "Half Orange";
            }
            else if (left == 2)
            {
                winnerText.text = "Round Orange";
            }
        }
        else if (winner == "Right")
        {
            if (ColorUtility.TryParseHtmlString(rightTextColor, out textColor))
            {
                winnerText.color = textColor;
            }

            if (right == 1)
            {
                winnerText.text = "Half Blue";
            }
            else if (right == 2)
            {
                winnerText.text = "Round Blue";
            }
        }
    }
}
