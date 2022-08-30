using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameText : MonoBehaviour,IHoverable,IClickable
{
    [SerializeField] Color targetColor;
    public int id;

    TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        SetText(null);
    }
    
    void Update()
    {
        text.color = Color.Lerp(text.color, targetColor, Time.deltaTime * 5f);
    }

    public void SetText(string _text)
    {
        text.text = _text;

        text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
    }

    public void StartHover()
    {
        targetColor = Color.red;
    }
    public void EndHover()
    {
        targetColor = Color.white;
    }

    public void Click()
    {
        QuizManager.current.KickPlayer(id);
    }
}
