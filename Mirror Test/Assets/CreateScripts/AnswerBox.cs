using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerBox : MonoBehaviour
{
    public Button button;
    public TMP_InputField field;
    public int id;
    public void DeleteButton()
    {
        CreateManager.current.RemoveWriteAnswer(id);
    }
}
