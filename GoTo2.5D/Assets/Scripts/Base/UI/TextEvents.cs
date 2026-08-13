using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEvents : MonoBehaviour
{
    public void UpdateText(string newText)
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = newText;
    }

    public void UpdateTextWithRichText(string newText)
    {
        GetComponent<TMPro.TextMeshProUGUI>().text =  "<wave>" + newText + "</wave>";
    }
}
