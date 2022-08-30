using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestionBoxBehavior : MonoBehaviour, IClickable
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Button deleteButton;

    int a;

    bool holding;

    float lastMouseY;

    private void Update()
    {
        if (holding)
        {
            if(!Input.GetMouseButton(0))
            {
                holding = false;
                return;
            }
            transform.localPosition += new Vector3(0f, Input.mousePosition.y - lastMouseY, 0f);
            transform.localPosition = new Vector3(0f, Mathf.Clamp(transform.localPosition.y, (CreateManager.current.Questions.Count - 1) * -CreateManager.current.boxDistance, 0f), 0f);
            lastMouseY = Input.mousePosition.y;

            float _dist = CreateManager.current.boxDistance;
            float _dif = transform.localPosition.y - a * -_dist;
            if (Mathf.Abs(_dif) >= _dist)
            {
                CreateManager.current.SwitchQuestionPlace(Math.Sign(_dif) * -1 + a,a);
            }
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, a * -CreateManager.current.boxDistance, 0f), Time.deltaTime * 20f);
        }
    }

    public void SetText(int _number)
    {
        text.text = _number.ToString();
        a = _number - 1;

        if (CreateManager.current.Questions.Count == 1) deleteButton.interactable = false;
        else deleteButton.interactable = true;
    }

    public void Click()
    {
        CreateManager.current.currentQuestionIndex = a;
        CreateManager.current.LoadQuestion();        

        holding = true;
        lastMouseY = Input.mousePosition.y;
    }

    public void Delete()
    {
        CreateManager.current.DeleteQuestion(a);
        
    }
}
