using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuizBoxBehavior : MonoBehaviour,IClickable
{
    [SerializeField] TextMeshProUGUI text;
    int id;

    [SerializeField] bool mogus;
    public void SetText(string _text, int _id)
    {
        text.text = _text;
        id = _id;
    }
    public void Click()
    {
        if (!mogus) CreateManager.current.SelectQuiz(id);
        else QuizMainMenu.current.SelectQuiz(id);
    }
}
