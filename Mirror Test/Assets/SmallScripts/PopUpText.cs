using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopUpText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] RectTransform image;

    [SerializeField,TextArea(1,5)] string setText;

    private void Awake()
    {
        SetText(setText);
    }

    public void SetText(string _text)
    {
        text.text = _text;

        image.sizeDelta = text.GetPreferredValues() + new Vector2(40f,40f);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
