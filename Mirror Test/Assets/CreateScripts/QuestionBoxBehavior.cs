using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class QuestionBoxBehavior : MonoBehaviour, IClickable
{
    [SerializeField] TextMeshProUGUI text;

    int a;

    public void SetText(int _number)
    {
        text.text = _number.ToString();
        a = _number - 1;
    }

    public void Click()
    {
        CreateManager.current.currentQuestionIndex = a;
        CreateManager.current.LoadQuestion();
    }
}
