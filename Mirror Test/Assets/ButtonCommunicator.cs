using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonCommunicator : MonoBehaviour
{
    public void Answer(int _choice)
    {
        QuizNetworkManager.myPlayer.CmdMakeChoice(_choice);
        QuizManager.current.AnswerMade();
    }
    public void StartQuiz()
    {
        QuizNetworkManager.myPlayer.CmdStartQuiz();
    }

    public void Skip()
    {
        QuizNetworkManager.myPlayer.CmdSkip();
    }

    public void Quit()
    {
        QuizNetworkManager.myPlayer.Disconnect();

    }
}
