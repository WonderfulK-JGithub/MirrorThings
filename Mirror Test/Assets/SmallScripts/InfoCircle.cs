using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoCircle : MonoBehaviour,IHoverable
{
    [SerializeField] float fadeSpeed;
    [SerializeField] GameObject contents;

    List<Image> imgInContents = new List<Image>();
    List<TextMeshProUGUI> textInContents = new List<TextMeshProUGUI>();

    bool see;
    float a;

    private void Awake()
    {
        foreach (var _child in contents.GetComponentsInChildren<Image>())
        {
            imgInContents.Add(_child);
        }
        foreach (var _child in contents.GetComponentsInChildren<TextMeshProUGUI>())
        {
            textInContents.Add(_child);
        }
    }

    private void Update()
    {
        if (see)
        {
            a = Mathf.MoveTowards(a, 1f, fadeSpeed * Time.deltaTime);
        }
        else
        {
            a = Mathf.MoveTowards(a, 0f, fadeSpeed * Time.deltaTime);
        }

        foreach (var _image in imgInContents)
        {
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, a);
        }
        foreach (var _text in textInContents)
        {
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, a);
        }
    }

    public void StartHover()
    {
        see = true;
    }
    public void EndHover()
    {
        see = false;
    }
}
