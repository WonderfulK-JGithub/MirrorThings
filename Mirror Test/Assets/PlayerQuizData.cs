using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
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
    public string currentChoiceWrite;

    public float answerTime;
    public int streak;

    public override void OnStartAuthority()
    {
        CmdSetName(QuizMainMenu.DisplayName);

        QuizNetworkManager.myPlayer = this;
        
    }
    [Command]
    public void CmdSetName(string _name)
    {
        //print(connectionToClient.connectionId == QuizNetworkManager.firstConn.connectionId);

        isLeader = connectionToClient.connectionId == QuizNetworkManager.firstConn.connectionId;

        if (isLeader)
        {
            QuizManager.quiz = QuizMainMenu.UsedQuiz;
            QuizManager.mode = QuizMainMenu.UsedMode;

            if (QuizManager.mode != PlayMode.Default)
            {
                
                QuizNetworkManager.current.playerList.Remove(this);
                return;
            }
        }

        playerName = _name;
        RpcSetName(_name, QuizManager.mode);

        QuizManager.current.SetNames();
    }

    [ClientRpc]
    public void RpcSetName(string _name,PlayMode _mode)
    {
        playerName = _name;
        QuizManager.mode = _mode;
    }

    [Command]
    public void CmdMakeChoice(int _choice)
    {
        answerTime = QuizManager.current.AnswerTime;

        currentChoice = _choice;
        QuizManager.current.RecivedAnswer();
    }
    [Command]
    public void CmdMakeWriteChoice(string _answer)
    {
        answerTime = QuizManager.current.AnswerTime;

        currentChoiceWrite = _answer;
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

        SceneTransition.OnTransitionExit -= Disconnect;
    }

    [TargetRpc]
    public void DisconnectMe(NetworkConnection _target)
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
    [Server]
    public void ValidateScore(List<string> _answers)
    {
        bool _correct = false;

        foreach (var _answer in _answers)
        {
            if(_answer == currentChoiceWrite)
            {
                _correct = true;
                break;
            }
        }

        if (_correct)
        {
            int _prevScore = score;

            int _points = QuizManager.quiz.questions[QuizManager.current.currentQuestion].points;
            score += _points;
            score += Mathf.RoundToInt(answerTime * _points);

            streak++;
            if (streak > 2)
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
        TextMeshProUGUI _text;
        if (!hasAuthority) return;
        if (QuizManager.mode == PlayMode.BigScreen) _text = QuizManager.current.scoreGainText_SCP;
        else _text = QuizManager.current.scoreGainText;

        if (_correct)
        {
            streak++;

            _text.text = _points.ToString();
            _text.color = Color.green;
        }
        else
        {
            streak = 0;

            _text.text = "x";
            _text.color = Color.red;
        }
    }
}
