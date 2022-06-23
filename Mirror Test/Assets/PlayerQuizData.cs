using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using Mirror;
using TMPro;

public class PlayerQuizData : NetworkBehaviour
{
    public string playerName;

    public int score;

    public bool isLeader;

    public int currentChoice;

    public float answerTime;
    public int streak;

    public override void OnStartAuthority()
    {
        CmdSetName(QuizMainMenu.DisplayName,QuizMainMenu.UsedQuiz);

        QuizNetworkManager.myPlayer = this;
        
    }
    [Command]
    public void CmdSetName(string _name,Quiz _quiz)
    {
        if(isLeader) QuizManager.quiz = _quiz;

        playerName = _name;
        RpcSetName(_name);

        QuizManager.current.SetNames();
    }

    [ClientRpc]
    public void RpcSetName(string _name)
    {
        playerName = _name;
        
    }

    [Command]
    public void CmdMakeChoice(int _choice)
    {
        answerTime = QuizManager.current.AnswerTime;

        currentChoice = _choice;
        QuizManager.current.RecivedAnswer();
    }

    [Command]
    public void CmdStartQuiz()
    {
        QuizManager.current.ServerStartQuiz();
    }

    [Command]
    public void CmdSkip()
    {
        QuizManager.current.QuizSkip();
    }

    public void Disconnect()
    {
        if (isLeader)
        {
            QuizNetworkManager.current.StopHost();
        }
        else
        {
            QuizNetworkManager.current.StopClient();
        }
    }

    [Server]
    public void ValidateScore(bool[] _corrections)
    {
        if (currentChoice != -1 && _corrections[currentChoice])
        {
            int _prevScore = score;

            int _points = QuizManager.quiz.questions[QuizManager.current.currentQuestion].points;
            score += _points;
            score += Mathf.RoundToInt(answerTime * _points);

            streak++;
            if(streak > 2)
            {
                score += Mathf.RoundToInt(QuizManager.current.streakBonus * _points * (streak - 2));
            }

            RpcRecivePoints(score - _prevScore, true);
        }
        else
        {
            RpcRecivePoints(0, false);

            streak = 0;
        }
    }

    [ClientRpc]
    public void RpcRecivePoints(int _points,bool _correct)
    {
        TextMeshProUGUI _text = QuizManager.current.scoreGainText;

        if (_correct)
        {
            _text.text = _points.ToString();
            _text.color = Color.green;
        }
        else
        {
            _text.text = "x";
            _text.color = Color.red;
        }

        _text.gameObject.SetActive(true);
    }
}
