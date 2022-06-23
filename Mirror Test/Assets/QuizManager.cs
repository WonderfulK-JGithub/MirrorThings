using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class QuizManager : NetworkBehaviour
{
    public static QuizManager current;
    public static Quiz quiz;

    public List<PlayerQuizData> playerList
    {
        get
        {
            return QuizNetworkManager.current.playerList;
        }
    }

    public float AnswerTime
    {
        get
        {
            return timer / ogQuestionTime;
        }
    }

    [Header("General")]
    public float streakBonus;

    [Header("--Waiting--")]
    [SerializeField] GameObject waitPanel;
    [SerializeField] TextMeshProUGUI[] nameTexts;
    [SerializeField] GameObject startButton;
    [SerializeField] TextMeshProUGUI quizNameText;
    [SerializeField] Image quizNameBackground;
    [SerializeField] float shakeMagnitude;
    [SerializeField] float shakeTime;
    [SerializeField] float shakeAmount;

    [Header("--Question--")]
    [SerializeField] GameObject questionPanel;
    [SerializeField] float readQuestionTime;
    [SerializeField] Slider questionBar;
    [SerializeField] TextMeshProUGUI questionText;

    [Header("--Answering--")]
    [SerializeField] GameObject answeringPanel;
    [SerializeField] GameObject skipButton;
    [SerializeField] TextMeshProUGUI skipButtonText;
    [SerializeField] Slider answeringBar;
    [SerializeField] TextMeshProUGUI[] alternetives;
    [SerializeField] GameObject[] alternetiveButtons;
    [SerializeField] Image[] alternetiveImages;
    [SerializeField] Color[] alternetiveColors;
    [SerializeField] Image answerBorder;
    public TextMeshProUGUI scoreGainText;
    [SerializeField] RawImage questionImage;
    [SerializeField] int maxImageWidth;
    [SerializeField] int maxImageHeight;

    [Header("--ScoreShowcase--")]
    [SerializeField] GameObject scoreShowcasePanel;
    [SerializeField] TextMeshProUGUI[] top5Names;
    [SerializeField] TextMeshProUGUI[] top5Scores;

    [Header("--End--")]
    [SerializeField] GameObject endPanel;
    [SerializeField] TextMeshProUGUI[] top5NamesEnd;

    QuizState state;

    bool shake;

    float timer;
    float shakeTimer;

    float ogQuestionTime;

    [HideInInspector] public int currentQuestion;

    int totalAnsersSent;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        if(QuizNetworkManager.myPlayer.isLeader)
        {
            startButton.SetActive(true);
        }
    }

    private void Update()
    {
        if (isServer)
        {
            ServerUpdate();
        }
        else
        {
            ClientUpdate();
        }
            
            
    }
    [ServerCallback]
    void ServerUpdate()
    {
        switch (state)
        {
            case QuizState.Waiting:
                
                if (shake)
                {
                    shakeTimer -= Time.deltaTime;

                    if (shakeTimer > 0f)
                    {
                        float _scale = Mathf.Sin(shakeTimer * (shakeAmount * 4f / shakeTime)) * shakeMagnitude * (shakeTimer / shakeTime) + 1;
                        nameTexts[playerList.Count - 1].transform.localScale = new Vector3(_scale, _scale, 1f);
                    }
                    else
                    {
                        nameTexts[playerList.Count - 1].transform.localScale = Vector3.one;
                        shake = false;
                    }
                }
                break;
            case QuizState.Question:

                timer -= Time.deltaTime;

                if(timer <= 0f)
                {
                    ServerStartAnswering();
                }

                questionBar.value = timer / readQuestionTime;
                break;
            case QuizState.Answering:
                timer -= Time.deltaTime;

                answeringBar.value = timer / ogQuestionTime;

                if(timer <= 0f)
                {
                    ServerRevealAnswer();
                }
                break;
        }
    }
    [ClientCallback]
    void ClientUpdate()
    {
        switch (state)
        {
            case QuizState.Waiting:
                if (shake)
                {
                    shakeTimer -= Time.deltaTime;

                    if(shakeTimer > 0f)
                    {
                        float _scale = Mathf.Sin(shakeTimer * (shakeAmount * 4f / shakeTime)) * shakeMagnitude * (shakeTimer / shakeTime) + 1;
                        nameTexts[playerList.Count - 1].transform.localScale = new Vector3(_scale, _scale, 1f);
                    }
                    else
                    {
                        nameTexts[playerList.Count - 1].transform.localScale = Vector3.one;
                        shake = false;
                    }
                }

                break;
            case QuizState.Question:
                timer -= Time.deltaTime;

                questionBar.value = timer / readQuestionTime;
                break;
            case QuizState.Answering:
                timer -= Time.deltaTime;

                answeringBar.value = timer / ogQuestionTime;
                break;
        }
    }

    [Server]
    public void SetNames()
    {
        List<PlayerQuizData> _data = QuizNetworkManager.current.playerList;
        string[] _names = new string[_data.Count];

        for (int i = 0; i < _data.Count; i++)
        {
            _names[i] = _data[i].playerName;
        }

        RpcSetNames(_names,quiz.name);
        
    }

    [ClientRpc]
    public void RpcSetNames(string[] _names,string _quizName)
    {

        quizNameText.text = _quizName;

        float _border = 10f;

        quizNameText.rectTransform.sizeDelta = new Vector2(quizNameText.preferredWidth + _border, quizNameText.preferredHeight + _border);

        quizNameBackground.rectTransform.sizeDelta = quizNameText.rectTransform.sizeDelta;

        for (int i = 0; i < nameTexts.Length; i++)
        {
            if(i < _names.Length)
            {
                nameTexts[i].text = _names[i];
            }
            else
            {
                nameTexts[i].text = null;
            }

            nameTexts[i].transform.localScale = Vector3.one;
        }

        shakeTimer = shakeTime;
        shake = true;
    }
    
    [Server]
    public void ServerStartQuiz()
    {
        QuizNetworkManager.current.allowJoining = false;

        state = QuizState.Question;
        RpcStartQuiz(quiz.questions[currentQuestion].question);

        timer = readQuestionTime;
    }

    [ClientRpc]
    public void RpcStartQuiz(string _question)
    {
        waitPanel.SetActive(false);
        questionPanel.SetActive(true);

        timer = readQuestionTime;
        state = QuizState.Question;

        questionText.text = _question;
    }

    [ClientRpc]
    public void RpcContinueQuiz(string _question)
    {
        skipButtonText.text = "Skip";

        scoreShowcasePanel.SetActive(false);
        questionPanel.SetActive(true);

        timer = readQuestionTime;
        state = QuizState.Question;

        questionText.text = _question;

        skipButton.SetActive(false);
    }

    [Server]
    public void ServerStartAnswering()
    {
        state = QuizState.Answering;

        timer = quiz.questions[currentQuestion].time;
        ogQuestionTime = timer;

        RpcStartAnswering(quiz.questions[currentQuestion].answers.ToArray(),timer, quiz.questions[currentQuestion].image);

        totalAnsersSent = 0;

        foreach (var _player in QuizNetworkManager.current.playerList)
        {
            _player.currentChoice = -1;
        }
    }

    [ClientRpc]
    public void RpcStartAnswering(string[] _alternetives,float _time, QuizImage _image)
    {
        if(_image == null)
        {
            questionImage.gameObject.SetActive(false);
        }
        else
        {
            questionImage.gameObject.SetActive(true);

            Texture2D _texture = new Texture2D(_image.width, _image.height, _image.format, false);
            _texture.LoadRawTextureData(_image.imageData);
            _texture.Apply();


            int _width = _texture.width;
            int _height = _texture.height;
            if (_width > maxImageWidth)
            {
                _width = maxImageWidth;
                _height = _width * _texture.height / _texture.width;
            }

            if (_height > maxImageHeight)
            {
                _height = maxImageHeight;
                _width = _height * _texture.width / _texture.height;
            }

            questionImage.texture = _texture;
            questionImage.rectTransform.sizeDelta = new Vector2(_width, _height);
        }

        answerBorder.gameObject.SetActive(false);

        if (QuizNetworkManager.myPlayer.isLeader) skipButton.SetActive(true);

        state = QuizState.Answering;
        timer = _time;
        ogQuestionTime = timer;

        questionPanel.SetActive(false);
        answeringPanel.SetActive(true);

        answeringBar.gameObject.SetActive(true);

        for (int i = 0; i < alternetiveButtons.Length; i++)
        {
            if(i < _alternetives.Length)
            {
                alternetiveButtons[i].SetActive(true);

                alternetives[i].text = _alternetives[i];

                alternetiveImages[i].color = alternetiveColors[i];
            }
            else
            {
                alternetiveButtons[i].SetActive(false);
            }
        }
    }

    [Server]
    public void RecivedAnswer()
    {
        totalAnsersSent++;

        if(totalAnsersSent == playerList.Count)
        {
            ServerRevealAnswer();
        }
    }

    [Client]
    public void AnswerMade()
    {
        for (int i = 0; i < alternetiveButtons.Length; i++)
        {
            alternetiveButtons[i].SetActive(false);
            answerBorder.transform.localPosition = alternetiveButtons[QuizNetworkManager.myPlayer.currentChoice].transform.localPosition;
        }
    }

    [Server]
    public void QuizSkip()
    {
        switch (state)
        {
            case QuizState.Answering:
                ServerRevealAnswer();
                break;
            case QuizState.Reveal:
                ServerScoreShowcase();
                break;
            case QuizState.ScoreShowcase:
                ServerContinueQuiz();
                break;
        }
    }

    [Server]
    public void ServerRevealAnswer()
    {
        bool[] _corrections = quiz.questions[currentQuestion].corrections.ToArray();

        foreach (var _player in QuizNetworkManager.current.playerList)
        {
            _player.ValidateScore(_corrections);
        }

        RpcRevealAnswer(_corrections);

        

        state = QuizState.Reveal;
    }

    [ClientRpc]
    public void RpcRevealAnswer(bool[] _corrections)
    {
        for (int i = 0; i < _corrections.Length; i++)
        {
            alternetiveButtons[i].SetActive(true);
            alternetiveImages[i].color = _corrections[i] ? Color.green : Color.red;
        }

        skipButtonText.text = "Continue";

        answerBorder.gameObject.SetActive(true);

        answeringBar.gameObject.SetActive(false);

        state = QuizState.Reveal;
    }

    [Server]
    public void ServerScoreShowcase()
    {
        playerList.Sort(QuizNetworkManager.SortPlayersByScore);

        int _top5 = playerList.Count < 5 ? playerList.Count : 5;

        string[] _names = new string[_top5];
        int[] _scores = new int[_top5];

        for (int i = 0; i < _top5; i++)
        {
            _names[i] = playerList[i].playerName;
            _scores[i] = playerList[i].score;
        }

        state = QuizState.ScoreShowcase;

        RpcScoreShowcase(_names,_scores);
    }

    [ClientRpc]
    public void RpcScoreShowcase(string[] _names,int[] _scores)
    {
        answeringPanel.SetActive(false);
        scoreShowcasePanel.SetActive(true);

        scoreGainText.gameObject.SetActive(false);

        for (int i = 0; i < 5; i++)
        {
            if (i < _names.Length)
            {
                top5Names[i].text = _names[i];
                top5Scores[i].text = _scores[i].ToString();
            }
            else
            {
                top5Names[i].text = null;
                top5Scores[i].text = null;
            }
        }
    }

    [Server]
    public void ServerContinueQuiz()
    {
        currentQuestion++;
        if(currentQuestion == quiz.questions.Count)
        {
            ServerEndQuiz();
        }
        else
        {
            RpcContinueQuiz(quiz.questions[currentQuestion].question);
        }
    }

    [Server]
    public void ServerEndQuiz()
    {
        state = QuizState.End;

        playerList.Sort(QuizNetworkManager.SortPlayersByScore);

        int _top5 = playerList.Count < 5 ? playerList.Count : 5;

        string[] _names = new string[_top5];
        //int[] _scores = new int[_top5];

        for (int i = 0; i < _top5; i++)
        {
            _names[i] = playerList[i].playerName;
            //_scores[i] = playerList[i].score;
        }

        RpcEndQuiz(_names);
    }

    [ClientRpc]
    public void RpcEndQuiz(string[] _names)
    {
        scoreShowcasePanel.SetActive(false);
        endPanel.SetActive(true);

        for (int i = 0; i < 5; i++)
        {
            if (i < _names.Length)
            {
                top5NamesEnd[i].text = _names[i];
            }
            else
            {
                top5NamesEnd[i].text = null;
            }
        }

        skipButton.SetActive(false);
    }
}


public enum QuizState
{
    Waiting,
    Question,
    Answering,
    Reveal,
    ScoreShowcase,
    End,
}