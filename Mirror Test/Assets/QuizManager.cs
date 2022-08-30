using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuizManager : NetworkBehaviour
{
    public static QuizManager current;
    public static Quiz quiz;
    public static PlayMode mode;

    public List<PlayerQuizData> playerList
    {
        get
        {
            return QuizNetworkManager.current.playerList;
        }
    }

    public Question CurrentQuestion
    {
        get
        {
            return quiz.questions[currentQuestion];
        }
    }
    public WriteQuestion CurrentWriteQuestion
    {
        get
        {
            return quiz.questions[currentQuestion] as WriteQuestion;
        }
    }
    public AudioQuestion CurrentAudioQuestion
    {
        get
        {
            return quiz.questions[currentQuestion] as AudioQuestion;
        }
    }

    public bool HostCheck
    {
        get
        {
            return mode != PlayMode.Default && !isClientOnly;
        }
    }
    public bool ClientCheck
    {
        get
        {
            return mode == PlayMode.BigScreen && isClientOnly;
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
    [SerializeField] GraphicRaycaster raycaster;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] int maxBytesPerSend;
    [SerializeField] Animator anim;

    [Header("--Waiting--")]
    [SerializeField] GameObject waitPanel;
    [SerializeField] NameText[] nameTexts;
    [SerializeField] Button startButton;
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
    [SerializeField] TextMeshProUGUI typeText;
    [SerializeField] TextMeshProUGUI typeDescText;

    [Header("--Answering--")]
    [SerializeField] GameObject answeringPanel;
    [SerializeField] GameObject skipButton;
    [SerializeField] TextMeshProUGUI questionAnsweringText;
    [SerializeField] TextMeshProUGUI skipButtonText;
    [SerializeField] TextMeshProUGUI[] alternetives;
    [SerializeField] Button[] alternetiveButtons;
    [SerializeField] Image[] alternetiveImages;
    [SerializeField] Color[] alternetiveColors;
    [SerializeField] Image answerBorder;
    [SerializeField] RawImage questionImage;
    [SerializeField] int maxImageWidth;
    [SerializeField] int maxImageHeight;
    [SerializeField] Image timerImage;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject normalGroup;
    [Header("    <<--Write--->>")]
    [SerializeField] GameObject writeGroup;
    public TMP_InputField answerField;
    [SerializeField] Button confirmButton;
    [SerializeField] TextMeshProUGUI listOfAnswers;
    [SerializeField] GameObject hereAreTheAnswers;
    [Header("       <<--AnswerResult--->>")]
    public TextMeshProUGUI scoreGainText;
    [SerializeField] Animator scoreAnim;
    [SerializeField] TextMeshProUGUI placementText;
    [SerializeField] Image[] diagramResults;
    [SerializeField] TextMeshProUGUI[] answersCountText;
    [SerializeField] Image[] resultSymbols;
    [SerializeField] Sprite[] symbolImages;
    [Header("       <<--Audio--->>")]
    [SerializeField] GameObject questionAudio;
    [SerializeField] RectTransform[] audioVisualizers;
    [SerializeField] AudioSource questionAudioSource;
    [SerializeField] float heightMultiplier;
    [SerializeField] int numberOfSamples;
    [SerializeField] FFTWindow fftWindow;
    [SerializeField] float lerpTime;

    [Header("--ScoreShowcase--")]
    [SerializeField] GameObject scoreShowcasePanel;
    [SerializeField] TextMeshProUGUI[] top5Names;
    [SerializeField] TextMeshProUGUI[] top5Scores;

    [Header("--End--")]
    [SerializeField] GameObject endPanel;
    [SerializeField] TextMeshProUGUI[] top5NamesEnd;
    [SerializeField] SupriseCircle endCircle;
    [SerializeField] GameObject endScreenCinematic;

    [Header("--SmallScreenPanel--")]
    [SerializeField] GameObject smallScreenPanel;
    [SerializeField] TextMeshProUGUI nameText_SCP;
    [SerializeField] TextMeshProUGUI scoreText_SCP;
    [SerializeField] GameObject waiting_SCP;
    [SerializeField] GameObject question_SCP;
    [SerializeField] GameObject answer_SCP;
    [SerializeField] GameObject result_SCP;
    [SerializeField] TextMeshProUGUI questionText_SCP;
    [SerializeField] Animator scoreAnim_SCP;
    public TextMeshProUGUI scoreGainText_SCP;
    [SerializeField] TextMeshProUGUI placementText_SCP;
    [SerializeField] Button[] alternetiveButtons_SCP;
    [SerializeField] Image[] alternetiveImages_SCP;
    public TMP_InputField answerField_SCP;
    [SerializeField] Button confirmButton_SCP;
    [SerializeField] TextMeshProUGUI waitingForPlayers_SCP;

    QuizState state;

    IHoverable lastHover;

    bool shake;

    float timer;
    float shakeTimer;

    float ogQuestionTime;

    [HideInInspector] public int currentQuestion;

    int totalAnswersSent;
    int bytesToExpect;

    int mogus;

    byte[] currentImageData;
    byte[] currentAudioData;
    QuizImage newLoadedImage;

    AudioClip currentAudio;

    [SerializeField] AudioClip testetst;

    private void Awake()
    {
        current = this;
        anim = GetComponent<Animator>();

        
    }

    private void Start()
    {
        if(QuizNetworkManager.myPlayer.isLeader)
        {
            startButton.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (isServer)
        {
            ServerUpdate();

            if (Input.GetKeyDown(KeyCode.P))
            {
                float[] _data = new float[testetst.samples * testetst.channels];
                testetst.GetData(_data, 0);
                QuizAudio au = new QuizAudio
                {
                    samples = testetst.samples,
                    frequency = testetst.frequency,
                    channels = testetst.channels,
                    audioData = _data,
                };


                StartCoroutine(AudioTransfer(au));
            }
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
                #region
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

                PointerEventData _pointerEventData = new PointerEventData(eventSystem);
                _pointerEventData.position = Input.mousePosition;


                List<RaycastResult> _results = new List<RaycastResult>();

                raycaster.Raycast(_pointerEventData, _results);

                bool _foundHoverable = false;
                if (_results.Count > 0)
                {
                    foreach (var _item in _results)
                    {
                        Component _component = _item.gameObject.GetComponent(typeof(IHoverable));
                        if(_component != null)
                        {
                            _foundHoverable = true;

                            IHoverable _newHover = _component as IHoverable;
                            if(_newHover != lastHover)
                            {
                                if(lastHover != null)lastHover.EndHover();
                                _newHover.StartHover();
                                lastHover = _newHover;
                                
                            }
                        }

                        if (!Input.GetMouseButtonDown(0)) continue;

                        _component = _item.gameObject.GetComponent(typeof(IClickable));
                        if (_component != null)
                        {
                            (_component as IClickable).Click();
                        }
                    }

                }

                if (!_foundHoverable && lastHover != null)
                {
                    
                    lastHover.EndHover();
                    lastHover = null;
                }

                break;
                #endregion
            case QuizState.Question:
                #region
                timer -= Time.deltaTime;

                if(timer <= 0f)
                {
                    ServerStartAnswering();
                }

                questionBar.value = timer / readQuestionTime;
                break;
            #endregion
            case QuizState.Answering:
                #region
                timer -= Time.deltaTime;

                timerText.text = Mathf.Ceil(timer).ToString();
                timerImage.fillAmount = timer / ogQuestionTime;

                if(timer <= 0f)
                {
                    ServerRevealAnswer();
                }

                if (questionAudioSource.isPlaying)
                {
                    float[] _samples = new float[numberOfSamples];
                    questionAudioSource.GetSpectrumData(_samples, 0, fftWindow);

                    for (int i = 0; i < audioVisualizers.Length; i++)
                    {
                        RectTransform _visualizer = audioVisualizers[i];

                        float _intesity = _samples[i] * heightMultiplier;

                        float _height = Mathf.Lerp(_visualizer.sizeDelta.y, _intesity, lerpTime * Time.deltaTime);

                        _visualizer.sizeDelta = new Vector2(_visualizer.sizeDelta.x, _height);
                    }

                }
                break;
                #endregion
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

                timerText.text = Mathf.Ceil(timer).ToString();
                timerImage.fillAmount = timer / ogQuestionTime;

                if (questionAudioSource.isPlaying)
                {
                    float[] _samples = new float[numberOfSamples];
                    questionAudioSource.GetSpectrumData(_samples, 0, fftWindow);

                    for (int i = 0; i < audioVisualizers.Length; i++)
                    {
                        RectTransform _visualizer = audioVisualizers[i];

                        float _intesity = _samples[i] * heightMultiplier;

                        float _height = Mathf.Lerp(_visualizer.sizeDelta.y, _intesity, lerpTime * Time.deltaTime);

                        _visualizer.sizeDelta = new Vector2(_visualizer.sizeDelta.x, _height);
                    }

                }

                break;
        }
    }

    #region --Server--
    [Server]
    public void SetNames()
    {
        List<PlayerQuizData> _data = QuizNetworkManager.current.playerList;
        string[] _names = new string[_data.Count];

        for (int i = 0; i < _data.Count; i++)
        {
            _names[i] = _data[i].playerName;

        }

        RpcSetNames(_names, quiz.name);
    }
    [Server]
    public void KickPlayer(int _id)
    {
        playerList[_id].DisconnectMe(playerList[_id].connectionToClient);

        playerList.RemoveAt(_id);

        SetNames();
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
                if (currentQuestion == quiz.questions.Count - 1)
                {
                    ServerEndQuiz();
                }
                else
                {
                    ServerScoreShowcase();
                }
                break;
            case QuizState.ScoreShowcase:
                ServerContinueQuiz();
                break;
        }
    }
    [Server]
    public void ServerStartQuiz()
    {
        QuizNetworkManager.current.allowJoining = false;

        state = QuizState.Question;
        RpcStartQuiz(quiz.questions[currentQuestion].question,CurrentQuestion.type);

        timer = readQuestionTime;
        if (CurrentQuestion.type == QuestionType.Audio)
        {
            QuizAudio _audio = CurrentAudioQuestion.audio;
            StartCoroutine(AudioTransfer(_audio));
        }
        else
        {
            QuizImage _image = quiz.questions[currentQuestion].image;
            if (_image != null && _image.imageData.Length != 0) StartCoroutine(ImageTransfering(_image));
        }

    }
    [Server]
    public void ServerContinueQuiz()
    {
        state = QuizState.Question;
        currentQuestion++;
        RpcContinueQuiz(quiz.questions[currentQuestion].question, CurrentQuestion.type);
        timer = readQuestionTime;

        if(CurrentQuestion.type == QuestionType.Audio)
        {
            QuizAudio _audio = CurrentAudioQuestion.audio;
            StartCoroutine(AudioTransfer(_audio));
        }
        else
        {
            QuizImage _image = quiz.questions[currentQuestion].image;
            if (_image != null && _image.imageData.Length != 0) StartCoroutine(ImageTransfering(_image));
        }
    }
    [Server]
    public void ServerStartAnswering()
    {
        answerBorder.transform.localPosition = new Vector3(0f, 10000f, 0f);

        state = QuizState.Answering;

        timer = quiz.questions[currentQuestion].time;
        ogQuestionTime = timer;

        string[] _info = null;

        switch(CurrentQuestion.type)
        {
            case QuestionType.Normal:
            case QuestionType.Audio:
                _info = CurrentQuestion.answers.ToArray();
                break;
        }

        RpcStartAnswering(_info, timer,CurrentQuestion.type);

        totalAnswersSent = 0;

        foreach (var _player in QuizNetworkManager.current.playerList)
        {
            _player.currentChoice = -1;
        }
    }
    [Server]
    public void RecivedAnswer()
    {
        totalAnswersSent++;

        if (totalAnswersSent == playerList.Count)
        {
            ServerRevealAnswer();
        }
    }
    [Server]
    public void ServerRevealAnswer()
    {
        bool[] _corrections = quiz.questions[currentQuestion].corrections.ToArray();

        int[] _answers = new int[_corrections.Length];
        foreach (var _player in playerList)
        {
            if (_player.currentChoice == -1) continue;

            _answers[_player.currentChoice]++;

        }

        foreach (var _player in playerList)
        {
            if (CurrentQuestion.type == QuestionType.Normal || CurrentQuestion.type == QuestionType.Audio) _player.ValidateScore(_corrections);
            else if (CurrentQuestion.type == QuestionType.Write) _player.ValidateScore(CurrentWriteQuestion.acceptedAnswers);
        }

        playerList.Sort(QuizNetworkManager.SortPlayersByScore);
        state = QuizState.Reveal;

        string[] _nameOrder = new string[playerList.Count];
        int[] _pointsOrder = new int[playerList.Count];
        for (int i = 0; i < _nameOrder.Length; i++)
        {
            _nameOrder[i] = playerList[i].playerName;
            _pointsOrder[i] = playerList[i].score;
        }

        string[] _otherInfo = null;
        if(CurrentQuestion.type == QuestionType.Write)
        {
            _otherInfo = CurrentWriteQuestion.acceptedAnswers.ToArray();
        }

        RpcRevealAnswer(_corrections, _answers, _nameOrder, _pointsOrder,CurrentQuestion.type, _otherInfo);
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

        RpcScoreShowcase(_names, _scores);
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
    #endregion

    #region --Client--

    [ClientRpc]
    public void RpcSetNames(string[] _names,string _quizName)
    {
        if (ClientCheck)
        {
            nameText_SCP.text = QuizMainMenu.DisplayName;
            scoreText_SCP.text = "0";

            waitPanel.SetActive(false);
            smallScreenPanel.SetActive(true);
            waiting_SCP.SetActive(true);

            return;
        }

        if (_names.Length > 0) startButton.interactable = true;
        else startButton.interactable = false;

        quizNameText.text = _quizName;

        float _border = 10f;

        quizNameText.rectTransform.sizeDelta = new Vector2(quizNameText.preferredWidth + _border, quizNameText.preferredHeight + _border);

        quizNameBackground.rectTransform.sizeDelta = quizNameText.rectTransform.sizeDelta;

        for (int i = 0; i < nameTexts.Length; i++)
        {
            if(i < _names.Length)
            {
                nameTexts[i].SetText(_names[i]);
                nameTexts[i].id = i;
            }
            else
            {
                nameTexts[i].SetText(null);
            }

            nameTexts[i].transform.localScale = Vector3.one;
        }

        shakeTimer = shakeTime;
        shake = true;
    }

    [ClientRpc]
    public void RpcStartQuiz(string _question, QuestionType _type)
    {
        timer = readQuestionTime;
        state = QuizState.off;
        
        if (ClientCheck)
        { 
            waiting_SCP.SetActive(false);
            question_SCP.SetActive(true);

            mogus++;
            questionText_SCP.text = "Question " + mogus.ToString();
            questionText_SCP.gameObject.SetActive(true);
            return;
        }

        waitPanel.SetActive(false);
        questionPanel.SetActive(true);

        questionText.text = _question;

        anim.Play("Quiz_Question");

        switch (_type)
        {
            case QuestionType.Normal:
                typeText.text = "Normal Quiz";
                typeDescText.text = "Choose the right answer from up to 4 options";
                break;
            case QuestionType.Write:
                typeText.text = "Type Quiz";
                typeDescText.text = "Type the right answer with words";
                break;
            case QuestionType.Audio:
                typeText.text = "Audio Quiz";
                typeDescText.text = "Listen to a audio clip and choose the right answer from up to 4 options";
                break;
        }
    }

    [ClientRpc]
    public void RpcContinueQuiz(string _question,QuestionType _type)
    {
        timer = readQuestionTime;
        state = QuizState.off;

        if (ClientCheck)
        {
            answer_SCP.SetActive(false);
            question_SCP.SetActive(true);

            mogus++;
            questionText_SCP.text = "Question " + mogus.ToString();
            questionText_SCP.gameObject.SetActive(true);
            return;
        }

        skipButtonText.text = "Skip";

        scoreShowcasePanel.SetActive(false);
        questionPanel.SetActive(true);

        questionText.text = _question;

        skipButton.SetActive(false);

        anim.Play("Quiz_Question");

        normalGroup.SetActive(false);
        writeGroup.SetActive(false);

        switch (_type)
        {
            case QuestionType.Normal:
                typeText.text = "Normal Quiz";
                typeDescText.text = "Choose the right answer from 4 options";
                normalGroup.SetActive(true);
                break;
            case QuestionType.Write:
                typeText.text = "Type Quiz";
                typeDescText.text = "Type the right answer with words";
                writeGroup.SetActive(true);
                break;
        }
    }

    [ClientRpc]
    public void RpcStartAnswering(string[] _arrayInfo, float _time,QuestionType _type)
    {
        if (ClientCheck)
        {
            question_SCP.SetActive(false);
            answer_SCP.SetActive(true);
            switch (_type)
            {
                case QuestionType.Normal:
                case QuestionType.Audio:
                    #region
                    for (int i = 0; i < alternetiveButtons_SCP.Length; i++)
                    {
                        if (i < _arrayInfo.Length)
                        {
                            alternetiveButtons_SCP[i].gameObject.SetActive(true);
                            alternetiveImages_SCP[i].color = alternetiveColors[i];

                            alternetiveButtons[i].interactable = true;
                        }
                        else
                        {
                            alternetiveButtons_SCP[i].gameObject.SetActive(false);
                        }
                    }
                    return;
                #endregion
                case QuestionType.Write:
                    #region
                    answerField_SCP.gameObject.SetActive(true);
                    confirmButton_SCP.gameObject.SetActive(true);

                    answerField_SCP.text = null;
                    confirmButton_SCP.interactable = false;
                    return;
                    #endregion
            }
        }

        anim.Play("New State");

        questionAnsweringText.text = questionText.text;

        answerBorder.gameObject.SetActive(false);

        if (QuizNetworkManager.myPlayer.isLeader) skipButton.SetActive(true);

        state = QuizState.Answering;
        timer = _time;
        ogQuestionTime = timer;

        questionPanel.SetActive(false);
        answeringPanel.SetActive(true);

        timerImage.transform.parent.gameObject.SetActive(true);

        switch (_type)
        {
            case QuestionType.Normal:
            case QuestionType.Audio:
                #region
                for (int i = 0; i < alternetiveButtons.Length; i++)
                {
                    if (i < _arrayInfo.Length)
                    {
                        alternetiveButtons[i].gameObject.SetActive(true);
                        alternetives[i].text = _arrayInfo[i];
                        alternetiveImages[i].color = alternetiveColors[i];

                        if (HostCheck)
                        {
                            alternetiveButtons[i].interactable = false;
                        }
                        else
                        {
                            alternetiveButtons[i].interactable = true;
                        }
                        
                    }
                    else
                    {
                        alternetiveButtons[i].gameObject.SetActive(false);
                    }
                }
                normalGroup.SetActive(true);
                break;
            #endregion
            case QuestionType.Write:
                #region

                listOfAnswers.gameObject.SetActive(false);
                hereAreTheAnswers.SetActive(false);

                if (HostCheck)
                {
                    answerField.gameObject.SetActive(false);
                    confirmButton.gameObject.SetActive(false);
                }
                else
                {
                    answerField.gameObject.SetActive(true);
                    confirmButton.gameObject.SetActive(true);
                }                

                answerField.text = null;
                confirmButton.interactable = false;

                writeGroup.SetActive(true);
                break;
                #endregion
        }

        if (_type == QuestionType.Audio)
        {
            questionAudioSource.clip = currentAudio;
            questionAudioSource.Play();
            questionAudio.SetActive(true);
        }

        //diagram
        for (int i = 0; i < diagramResults.Length; i++)
        {
            diagramResults[i].gameObject.SetActive(false);
            answersCountText[i].gameObject.SetActive(false);
            resultSymbols[i].enabled = false;
        }
    }

    [Client]
    public void AnswerMade(QuestionType _type)
    {
        if (ClientCheck)
        {
            for (int i = 0; i < alternetiveButtons_SCP.Length; i++)
            {
                alternetiveButtons_SCP[i].gameObject.SetActive(false);
            }
            answerField_SCP.gameObject.SetActive(false);
            confirmButton_SCP.gameObject.SetActive(false);
        }

        switch (_type)
        {
            case QuestionType.Normal:
            case QuestionType.Audio:
                for (int i = 0; i < alternetiveButtons.Length; i++)
                {
                    alternetiveButtons[i].gameObject.SetActive(false);
                    answerBorder.transform.localPosition = alternetiveButtons[QuizNetworkManager.myPlayer.currentChoice].transform.localPosition;
                }
                break;
            case QuestionType.Write:
                answerField.gameObject.SetActive(false);
                confirmButton.gameObject.SetActive(false);
                break;
        }
        
    }

    [ClientRpc]
    public void RpcRevealAnswer(bool[] _corrections, int[] _answers,string[] _nameOrder,int[] _pointsOrder, QuestionType _type, string[] _otherInfo)
    {
        if (!HostCheck)
        {
            string _placeText = null;
            for (int i = 0; i < _nameOrder.Length; i++)
            {
                if (QuizNetworkManager.myPlayer.playerName == _nameOrder[i])
                {
                    scoreText_SCP.text = _pointsOrder[i].ToString();
                    if (i != 0)
                    {
                        string _raebilov;
                        if (i == 1)
                        {
                            _raebilov = "nd";
                        }
                        else if (i == 2)
                        {
                            _raebilov = "rd";
                        }
                        else
                        {
                            _raebilov = "th";
                        }

                        int _dif = _pointsOrder[i - 1] - _pointsOrder[i];

                        _placeText = "You're " + i.ToString() + _raebilov + "!\n" + _dif.ToString() + " points behind \n" + _nameOrder[i - 1];
                    }
                    else
                    {
                        _placeText = "You're first!";
                    }
                }
            }

            if (ClientCheck)
            {
                scoreAnim_SCP.Play("Result_Transition");
                placementText_SCP.text = _placeText;
                return;
            }

            placementText.text = _placeText;
            scoreAnim.Play("Result_Transition");
        }

        questionImage.enabled = false;

        switch (_type)
        {
            case QuestionType.Normal:
            case QuestionType.Audio:
                #region
                for (int i = 0; i < _corrections.Length; i++)
                {
                    alternetiveButtons[i].gameObject.SetActive(true);
                    float _alpha = _corrections[i] ? 1f : 0.4f;
                    alternetiveImages[i].color = new Color(alternetiveImages[i].color.r, alternetiveImages[i].color.g, alternetiveImages[i].color.b, _alpha);
                    alternetiveButtons[i].interactable = false;

                    resultSymbols[i].enabled = true;
                    resultSymbols[i].sprite = _corrections[i] ? symbolImages[1] : symbolImages[0];
                }

                answerBorder.gameObject.SetActive(true);

                //diagram
                int _totalAnswers = 0;
                for (int i = 0; i < _answers.Length; i++)
                {
                    _totalAnswers += _answers[i];
                }

                for (int i = 0; i < _answers.Length; i++)
                {
                    diagramResults[i].gameObject.SetActive(true);
                    answersCountText[i].gameObject.SetActive(true);

                    diagramResults[i].color = alternetiveColors[i];

                    float _percentege;
                    if (_totalAnswers != 0) _percentege = _answers[i] / (float)_totalAnswers;
                    else _percentege = 0f;

                    diagramResults[i].rectTransform.sizeDelta = new Vector2(50f, _percentege * 100f + 5f);

                    answersCountText[i].text = _answers[i].ToString();
                }
                break;
            #endregion
            case QuestionType.Write:
                #region
                listOfAnswers.text = null;
                foreach (var _answer in _otherInfo)
                {
                    listOfAnswers.text += "   " + _answer;
                }

                listOfAnswers.gameObject.SetActive(true);
                hereAreTheAnswers.SetActive(true);
                break;
                #endregion
        }

        if(_type == QuestionType.Audio)
        {
            questionAudio.SetActive(false);
            questionAudioSource.Stop();
        }

        skipButtonText.text = "Continue";

        timerImage.transform.parent.gameObject.SetActive(false);

        state = QuizState.Reveal;
    }

    [ClientRpc]
    public void RpcScoreShowcase(string[] _names,int[] _scores)
    {
        if (ClientCheck) return;

        answeringPanel.SetActive(false);
        scoreShowcasePanel.SetActive(true);

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

    [ClientRpc]
    public void RpcEndQuiz(string[] _names)
    {
        answer_SCP.SetActive(false);
        result_SCP.SetActive(true);
        

        answeringPanel.SetActive(false);
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

        endScreenCinematic.SetActive(true);
        endCircle.StartMoving();

        skipButton.SetActive(false);
    }
    #endregion

    #region other

    //image loading
    IEnumerator ImageTransfering(QuizImage _imageToLoad)
    {
        byte[] _bytes = _imageToLoad.imageData;

        int _bytesToSend = _bytes.Length;
        int _bytesSent = 0;

        RpcStartRecivingImageData(_bytesToSend,_imageToLoad.width, _imageToLoad.height,_imageToLoad.format);

        while (_bytesSent < _bytesToSend)
        {
            int _buffer = maxBytesPerSend;
            if (_bytesToSend - _bytesSent < _buffer) _buffer = _bytesToSend - _bytesSent;

            byte[] _dataToSend = new byte[_buffer];

            Array.Copy(_bytes, _bytesSent, _dataToSend, 0, _buffer);

            RpcReciveImageData(_dataToSend);

            _bytesSent += _buffer;
            yield return null;
        }
    }

    [ClientRpc]
    public void RpcStartRecivingImageData(int _expectedBytesAmount,int _width,int _heigt,TextureFormat _format)
    {
        currentImageData = new byte[0];
        bytesToExpect = _expectedBytesAmount;

        newLoadedImage = new QuizImage
        {
            width = _width,
            height = _heigt,
            format = _format
        };
    }

    [ClientRpc]
    public void RpcReciveImageData(byte[] _bytesSent)
    {
        byte[] _newData = new byte[currentImageData.Length + _bytesSent.Length];

        Array.Copy(currentImageData, 0, _newData, 0, currentImageData.Length);
        Array.Copy(_bytesSent, 0, _newData, currentImageData.Length, _bytesSent.Length);

        currentImageData = _newData;

        if (currentImageData.Length == bytesToExpect)
        {
            print(currentImageData[120021]);
            print("imposter");

            newLoadedImage.imageData = currentImageData;
            LoadImage();
        }
    }

    [Client]
    void LoadImage()
    {
        QuizImage _image = newLoadedImage;

        questionImage.enabled = true;

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

    //Audio loading
    IEnumerator AudioTransfer(QuizAudio _audioToLoad)
    {
        float[] _audioData = _audioToLoad.audioData;
        byte[] _bytes = new byte[_audioData.Length * sizeof(float)];

        Buffer.BlockCopy(_audioData, 0, _bytes, 0, _bytes.Length);

        int _bytesToSend = _bytes.Length;
        int _bytesSent = 0;

        RpcStartReciveAudioData(_bytesToSend, _audioToLoad.samples, _audioToLoad.frequency, _audioToLoad.channels);

        while (_bytesSent < _bytesToSend)
        {
            int _buffer = maxBytesPerSend;
            if (_bytesToSend - _bytesSent < _buffer) _buffer = _bytesToSend - _bytesSent;

            byte[] _dataToSend = new byte[_buffer];

            Array.Copy(_bytes, _bytesSent, _dataToSend, 0, _buffer);

            RpcReciveAudioData(_dataToSend);

            _bytesSent += _buffer;
            yield return null;
        }
    }

    [ClientRpc]
    public void RpcStartReciveAudioData(int _expectedBytesAmount,int _samples,int _frequency,int _channels)
    {
        currentAudioData = new byte[0];
        currentAudio = AudioClip.Create("myAudio", _samples, _channels, _frequency, false);

        bytesToExpect = _expectedBytesAmount;
    }

    [ClientRpc]
    public void RpcReciveAudioData(byte[] _bytesSent)
    {
        byte[] _newData = new byte[currentAudioData.Length + _bytesSent.Length];

        Array.Copy(currentAudioData, 0, _newData, 0, currentAudioData.Length);
        Array.Copy(_bytesSent, 0, _newData, currentAudioData.Length, _bytesSent.Length);

        currentAudioData = _newData;

        if (currentAudioData.Length == bytesToExpect)
        {

            LoadAudio();
        }
    }

    [Client]
    public void LoadAudio()
    {
        float[] _audioData = new float[currentAudioData.Length / 4];
        Buffer.BlockCopy(currentAudioData, 0, _audioData, 0, currentAudioData.Length);

        currentAudio.SetData(_audioData, 0);

        if (!currentAudio.LoadAudioData())
        {
            Debug.LogError("Audio couldn't be loaded");
        }
    }

    //animation event

    void QuestionStart()
    {
        state = QuizState.Question;
    }

    public void AnswerFieldUpdate()
    {
        confirmButton.interactable = !string.IsNullOrWhiteSpace(answerField.text);
        confirmButton_SCP.interactable = !string.IsNullOrWhiteSpace(answerField_SCP.text);
        print("as");
    }
    #endregion
}

public enum QuizState
{
    Waiting,
    Question,
    Answering,
    Reveal,
    ScoreShowcase,
    End,
    off,
}