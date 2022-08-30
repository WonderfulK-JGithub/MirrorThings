using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonCommunicator : MonoBehaviour
{
    public void Answer(int _choice)
    {
        QuizNetworkManager.myPlayer.CmdMakeChoice(_choice);
        QuizManager.current.AnswerMade(QuestionType.Normal);
    }
    public void WriteAnswer()
    {
        if(QuizManager.mode != PlayMode.BigScreen)QuizNetworkManager.myPlayer.CmdMakeWriteChoice(QuizManager.current.answerField.text);
        else QuizNetworkManager.myPlayer.CmdMakeWriteChoice(QuizManager.current.answerField_SCP.text);
        QuizManager.current.AnswerMade(QuestionType.Write);
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
        SceneTransition.current.Exit();
        SceneTransition.OnTransitionExit += QuizNetworkManager.myPlayer.Disconnect;

    }
}
