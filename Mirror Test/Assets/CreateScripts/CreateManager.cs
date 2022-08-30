using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AnotherFileBrowser.Windows;
using UnityEngine.Networking;


public class CreateManager : MonoBehaviour
{
    string savePath;

    const int maxFiles = 100;

    public static CreateManager current;

    [Header("General")]
    [SerializeField] GameObject firstPanel;
    [SerializeField] GameObject createPanel;
    [SerializeField] GameObject savePanel;
    [SerializeField] GameObject fileListPanel;
    [SerializeField] GameObject areYouSurePanel;
    [SerializeField] GameObject popUpPrefab;
    [SerializeField] AudioClip soundTest;

    [Header("Normal type")]
    [SerializeField] GameObject normalGroup;
    [SerializeField] TMP_InputField questionInputField;
    [SerializeField] TMP_InputField[] answerInputFields;
    [SerializeField] Toggle[] trueFalseToggles;
    [SerializeField] Image[] trueFalseImages;

    [SerializeField] Transform questionTabContents;
    [SerializeField] GameObject questionBoxPrefab;
    [SerializeField] Transform addButton;
    [SerializeField] Transform boxBorder;
    public float boxDistance;

    [SerializeField] GraphicRaycaster raycaster;
    [SerializeField] EventSystem eventSystem;

    [SerializeField] TMP_InputField quizInputField;

    [SerializeField] Transform fileContents;
    [SerializeField] GameObject quizBoxPrefab;
    [SerializeField] float quizBoxDistance;
    [SerializeField] Transform quizBoxBorder;
    [SerializeField] Button selectButton;

    [Header("WriteQuestions")]
    [SerializeField] GameObject writeGroup;
    [SerializeField] Transform answerContents;
    [SerializeField] GameObject inputFeildPrefab;
    [SerializeField] float writeBufferWidth;
    [SerializeField] float writeBufferHeight;
    [SerializeField] Button addAnswerButton;

    List<AnswerBox> writeAnswerFields = new List<AnswerBox>();

    Quiz createdQuiz;

    public int currentQuestionIndex;

    [Header("OtherTab")]
    [SerializeField] TMP_InputField timeLimitField;
    [SerializeField] Slider timeLimitSlider;
    [SerializeField] TMP_InputField pointsField;
    [SerializeField] Slider pointsSlider;

    [Header("Image")]
    [SerializeField] RawImage image;
    [SerializeField] int maxWidth;
    [SerializeField] int maxHeight;
    [SerializeField] GameObject noImageText;
    [SerializeField] TextMeshProUGUI addImageText;

    [Header("Audio")]
    [SerializeField] GameObject quizAudio;
    [SerializeField] GameObject playButton;
    [SerializeField] GameObject stopButton;
    [SerializeField] TextMeshProUGUI addAudioText;
    [SerializeField] RectTransform[] audioVisualizers;
    [SerializeField] float heightMultiplier;
    [SerializeField] int numberOfSamples;
    [SerializeField] FFTWindow fftWindow;
    [SerializeField] float lerpTime;

    public List<Question> Questions
    {
        get
        {
            return createdQuiz.questions;
        }
    }
    public Question CurrentQuestion
    {
        get
        {
            return Questions[currentQuestionIndex];
        }
    }
    public WriteQuestion CurrentWriteQuestion
    {
        get
        {
            return Questions[currentQuestionIndex] as WriteQuestion;
        }
    }
    public AudioQuestion CurrentAudioQuestion
    {
        get
        {
            return Questions[currentQuestionIndex] as AudioQuestion;
        }
    }

    List<QuestionBoxBehavior> boxes = new List<QuestionBoxBehavior>();
    List<QuizBoxBehavior> quizBoxes = new List<QuizBoxBehavior>();

    int tabOffset;
    int selectedQuiz;

    bool fuckYou;
    bool hoverOverTab;
    bool isCreating;

    string lastTime;
    string lastPoint;

    float lastTimeSlider;
    float lastPointSlider;

    IHoverable lastHover;

    Transform canvas;

    AudioClip currentClip;
    AudioSource source;

    void Awake()
    {
        current = this;
        canvas = createPanel.transform.parent;
        //savePath = Application.persistentDataPath + "/Saves";
        savePath = Path.Combine(Application.persistentDataPath, "Saves");

        source = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseCheck();
        }

        HoverCheck();

        if (isCreating)
        {
            boxBorder.localPosition = boxes[currentQuestionIndex].transform.localPosition;

            if (hoverOverTab && Input.mouseScrollDelta != Vector2.zero)
            {
                tabOffset -= (int)Input.mouseScrollDelta.y;

                if (tabOffset > boxes.Count - 3) tabOffset = boxes.Count - 3;
                if (tabOffset < 0) tabOffset = 0;
            }

            questionTabContents.localPosition = Vector3.Lerp(questionTabContents.localPosition, new Vector3(0f, tabOffset * boxDistance, 0f), Time.deltaTime * 30f);

            if (CurrentQuestion.type != QuestionType.Audio) return;

            if (source.isPlaying)
            {
                float[] _samples = new float[numberOfSamples];
                source.GetSpectrumData(_samples, 0, fftWindow);

                for (int i = 0; i < audioVisualizers.Length; i++)
                {
                    RectTransform _visualizer = audioVisualizers[i];

                    float _intesity = _samples[i] * heightMultiplier;

                    float _height = Mathf.Lerp(_visualizer.sizeDelta.y, _intesity, lerpTime * Time.deltaTime);

                    _visualizer.sizeDelta = new Vector2(_visualizer.sizeDelta.x, _height);
                }

                stopButton.SetActive(true);
                playButton.SetActive(false);
            }
            else
            {
                stopButton.SetActive(false);
                for (int i = 0; i < audioVisualizers.Length; i++)
                {
                    RectTransform _visualizer = audioVisualizers[i];

                    _visualizer.sizeDelta = new Vector2(_visualizer.sizeDelta.x, 5f);
                }

                AudioQuestion _audioQuestion = CurrentQuestion as AudioQuestion;

                if (_audioQuestion.audio != null && _audioQuestion.audio.audioData.Length != 0)
                {
                    playButton.SetActive(true);
                }
            }
        }
        else
        {
            if (hoverOverTab && Input.mouseScrollDelta != Vector2.zero)
            {
                tabOffset -= (int)Input.mouseScrollDelta.y;

                if (tabOffset > quizBoxes.Count - 6) tabOffset = quizBoxes.Count - 6;
                if (tabOffset < 0) tabOffset = 0;
            }

            fileContents.localPosition = Vector3.Lerp(fileContents.localPosition, new Vector3(0f, tabOffset * quizBoxDistance, 0f), Time.deltaTime * 30f);
        }

        


    }

    public void NewQuiz()
    {
        firstPanel.SetActive(false);
        createPanel.SetActive(true);

        createdQuiz = new Quiz();

        AddNewQuestion((int)QuestionType.Normal);

        isCreating = true;
    }

    public void AddNewQuestion(int _type)
    {
        
        switch ((QuestionType)_type)
        {
            case QuestionType.Normal:
                #region
                Question _newQuestion = new Question();

                Questions.Add(_newQuestion);
                break;
                #endregion
            case QuestionType.Write:
                #region
                WriteQuestion _writeQuestion = new WriteQuestion();

                Questions.Add(_writeQuestion);
                break;
                #endregion
            case QuestionType.Audio:
                #region
                AudioQuestion _audioQuestion = new AudioQuestion();

                Questions.Add(_audioQuestion);
                break;
                #endregion
        }

        currentQuestionIndex = Questions.Count - 1;

        QuestionBoxBehavior _questionBox = Instantiate(questionBoxPrefab).GetComponent<QuestionBoxBehavior>();

        _questionBox.transform.SetParent(questionTabContents);
        _questionBox.transform.localScale = Vector3.one;
        _questionBox.transform.localPosition = new Vector3(0f, (Questions.Count - 1) * -boxDistance, 0f);

        boxes.Add(_questionBox);

        NumberBoxes();

        addButton.localPosition = new Vector3(0f, (Questions.Count - 1) * -boxDistance - 80f, 0f);

        LoadQuestion();
    }

    void NumberBoxes()
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].SetText((i + 1));
        }
    }

    public void LoadQuestion()
    {
        fuckYou = true;

        normalGroup.SetActive(false);
        writeGroup.SetActive(false);

        questionInputField.text = CurrentQuestion.question;

        switch (CurrentQuestion.type)
        {
            case QuestionType.Normal:
            case QuestionType.Audio:
             #region
                normalGroup.SetActive(true);

                for (int i = 0; i < 4; i++)
                {
                    answerInputFields[i].text = Questions[currentQuestionIndex].answers[i];
                }

                for (int i = 0; i < trueFalseToggles.Length; i++)
                {
                    trueFalseImages[i].color = CurrentQuestion.corrections[i] ? Color.green : Color.blue;
                    trueFalseToggles[i].isOn = CurrentQuestion.corrections[i];
                }
                break;
            #endregion
            case QuestionType.Write:
            #region
                writeGroup.SetActive(true);

                WriteQuestion _writeQuestion = CurrentQuestion as WriteQuestion;

                if (_writeQuestion == null) Debug.LogError("Question type is wrong");

                foreach (var item in writeAnswerFields)
                {
                    Destroy(item.gameObject);
                }

                writeAnswerFields.Clear();

                bool _beOn = true;

                for (int i = 0; i < _writeQuestion.acceptedAnswers.Count; i++)
                {
                    TMP_InputField _field = Instantiate(inputFeildPrefab).GetComponent<TMP_InputField>();

                    _field.transform.SetParent(answerContents);
                    _field.transform.localScale = Vector3.one;

                    _field.text = _writeQuestion.acceptedAnswers[i];

                    if (string.IsNullOrWhiteSpace(_writeQuestion.acceptedAnswers[i])) _beOn = false;

                    float _a = i % 2;

                    _field.onValueChanged.AddListener(WriteAnswerUpdate);

                    _field.transform.localPosition = new Vector3((i - _a) * writeBufferWidth / 2, -writeBufferHeight * _a, 0f);

                    writeAnswerFields.Add(_field.gameObject.GetComponent<AnswerBox>());
                    _field.gameObject.GetComponent<AnswerBox>().id = i;
                }

                float _b = _writeQuestion.acceptedAnswers.Count % 2;

                addAnswerButton.transform.localPosition = new Vector3((_writeQuestion.acceptedAnswers.Count - _b) * writeBufferWidth / 2, -writeBufferHeight * _b, 0f);
                addAnswerButton.interactable = _beOn;

                if (writeAnswerFields.Count == 1) writeAnswerFields[0].button.interactable = false;
                break;
                #endregion
        }

        source.Stop();

        if(CurrentQuestion.type == QuestionType.Audio)
        {
            image.gameObject.SetActive(false);
            quizAudio.SetActive(true);

            AudioQuestion _audioQuestion = CurrentQuestion as AudioQuestion;

            if(_audioQuestion.audio != null && _audioQuestion.audio.audioData.Length != 0)
            {
                QuizAudio _au = _audioQuestion.audio;

                currentClip = AudioClip.Create("mogus", _au.samples, _au.channels, _au.frequency, false);
                currentClip.SetData(_au.audioData, 0);
                currentClip.LoadAudioData();

                addAudioText.text = "change audio";
                playButton.SetActive(true);
            }
            else
            {

                addAudioText.text = "add audio";
                playButton.SetActive(false);
            }
            stopButton.SetActive(false);
        }
        else
        {
            image.gameObject.SetActive(true);
            quizAudio.SetActive(false);
        }

        fuckYou = false;

        boxBorder.localPosition = boxes[currentQuestionIndex].transform.localPosition;

        LoadImage();

        HandleAnswerFields();

        boxBorder.SetAsLastSibling();
        boxes[currentQuestionIndex].transform.SetAsLastSibling();
        
    }

    void HandleAnswerFields()
    {
        if(string.IsNullOrWhiteSpace(CurrentQuestion.answers[0]) || string.IsNullOrWhiteSpace(CurrentQuestion.answers[1]))
        {
            answerInputFields[2].interactable = false;
            answerInputFields[3].interactable = false;
        }
        else
        {
            answerInputFields[2].interactable = true;
            answerInputFields[3].interactable = true;
        }

        for (int i = 0; i < trueFalseToggles.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(answerInputFields[i].text))
            {
                trueFalseToggles[i].interactable = false;
            }
            else
            {
                trueFalseToggles[i].interactable = true;
            }
        }
    }

    public void QuestionUpdate()
    {
        CurrentQuestion.question = questionInputField.text;
    }
    public void AnswerUpdate()
    {
        if (fuckYou) return;

        for (int i = 0; i < answerInputFields.Length; i++)
        {
            CurrentQuestion.answers[i] = answerInputFields[i].text;
        }

        HandleAnswerFields();
    }
    public void ToggleUpdate()
    {
        if (fuckYou) return;

        for (int i = 0; i < trueFalseToggles.Length; i++)
        {
            trueFalseImages[i].color = trueFalseToggles[i].isOn ? Color.green : Color.blue;
            CurrentQuestion.corrections[i] = trueFalseToggles[i].isOn;
        }
    }
    public void QuizUpdate()
    {
        createdQuiz.name = quizInputField.text;
    }
    public void WriteAnswerUpdate(string _text)
    {
        if (fuckYou) return;

        bool _beOn = true;

        for (int i = 0; i < writeAnswerFields.Count; i++)
        {
            CurrentWriteQuestion.acceptedAnswers[i] = writeAnswerFields[i].field.text;
            if (string.IsNullOrWhiteSpace(writeAnswerFields[i].field.text)) _beOn = false;
        }

        addAnswerButton.interactable = _beOn;
    }

    public void AddWriteAnswer()
    {
        TMP_InputField _field = Instantiate(inputFeildPrefab).GetComponent<TMP_InputField>();

        _field.transform.SetParent(answerContents);
        _field.transform.localScale = Vector3.one;

        _field.text = null;

        float i = CurrentWriteQuestion.acceptedAnswers.Count;

        float _a = i % 2;

        _field.onValueChanged.AddListener(WriteAnswerUpdate);

        _field.transform.localPosition = new Vector3((i - _a) * writeBufferWidth / 2, -writeBufferHeight * _a, 0f);

        writeAnswerFields.Add(_field.gameObject.GetComponent<AnswerBox>());
        _field.gameObject.GetComponent<AnswerBox>().id = (int)i;

        float _b = (i + 1) % 2;

        addAnswerButton.transform.localPosition = new Vector3(((i + 1) - _b) * writeBufferWidth / 2, -writeBufferHeight * _b, 0f);
        addAnswerButton.interactable = false;

        CurrentWriteQuestion.acceptedAnswers.Add(null);

        writeAnswerFields[0].button.interactable = true;
    }
    public void RemoveWriteAnswer(int _answerId)
    {
        CurrentWriteQuestion.acceptedAnswers.RemoveAt(_answerId);
        Destroy(writeAnswerFields[_answerId].gameObject);
        writeAnswerFields.RemoveAt(_answerId);

        bool _beOn = true;
        for (int i = 0; i < writeAnswerFields.Count; i++)
        {
            float _a = i % 2;

            writeAnswerFields[i].transform.localPosition = new Vector3((i - _a) * writeBufferWidth / 2, -writeBufferHeight * _a, 0f);
            writeAnswerFields[i].id = i;
            if (string.IsNullOrWhiteSpace(writeAnswerFields[i].field.text)) _beOn = false;
        }
        float _b = CurrentWriteQuestion.acceptedAnswers.Count % 2;
        addAnswerButton.transform.localPosition = new Vector3((CurrentWriteQuestion.acceptedAnswers.Count - _b) * writeBufferWidth / 2, -writeBufferHeight * _b, 0f);
        addAnswerButton.interactable = _beOn;

        if (writeAnswerFields.Count == 1) writeAnswerFields[0].button.interactable = false;
    }

    void MouseCheck()
    {
        PointerEventData _pointerEventData = new PointerEventData(eventSystem);
        _pointerEventData.position = Input.mousePosition;

        List<RaycastResult> _results = new List<RaycastResult>();

        raycaster.Raycast(_pointerEventData, _results);

        if(_results.Count > 0)
        {
            foreach (var _item in _results)
            {

                Component _click = _item.gameObject.GetComponent(typeof(IClickable));

                if (_click != null)
                {
                    (_click as IClickable).Click();

                    break;
                }
            }
            
        }
    }
    void HoverCheck()
    {
        PointerEventData _pointerEventData = new PointerEventData(eventSystem);
        _pointerEventData.position = Input.mousePosition;
        

        List<RaycastResult> _results = new List<RaycastResult>();

        raycaster.Raycast(_pointerEventData, _results);

        hoverOverTab = false;
        
        if (_results.Count > 0)
        {
            bool mogus = false;
            foreach (var _item in _results)
            {
                
                if (_item.gameObject.CompareTag("Tab"))
                {
                    hoverOverTab = true;

                    //return;
                    continue;
                }
                Component _component = _item.gameObject.GetComponent(typeof(IHoverable));
                if(_component != null)
                {
                    mogus = true;
                    IHoverable _hover = _component as IHoverable;
                    if (_hover == lastHover)
                    {
                        continue;
                    }
                    if (lastHover != null) lastHover.EndHover();
                    _hover.StartHover();
                    lastHover = _hover;
                } 
            }

            if (!mogus && lastHover != null)
            {
                lastHover.EndHover();
                lastHover = null;
            }

        }
        else if (lastHover != null)
        {
            lastHover.EndHover();
            lastHover = null;
        }

        
    }

    public void MoveToSave()//saveCheck
    {
        foreach (var _question in Questions)
        {
            if (string.IsNullOrWhiteSpace(_question.question))
            {
                CreatePopUp("Your question is empty");
                return;
            }
            switch (_question.type)
            {
                case QuestionType.Normal:
                case QuestionType.Audio:
                    if (string.IsNullOrWhiteSpace(_question.answers[0]) || string.IsNullOrWhiteSpace(_question.answers[1]))
                    {
                        CreatePopUp("Your regular questions needs atleast 2 alternetives");
                        return;
                    }
                    int trueCount = 0;
                    for (int i = 0; i < _question.answers.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(_question.answers[i]) && _question.corrections[i]) trueCount++;
                    }
                    if (trueCount == 0)
                    {
                        CreatePopUp("Your question needs atleast 1 correct answer");
                        return;
                    }
                    break;
                case QuestionType.Write:
                    foreach (var item in (_question as WriteQuestion).acceptedAnswers)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                        {
                            if ((_question as WriteQuestion).acceptedAnswers.Count > 1) CreatePopUp("Your write questions can't have an empty answer");
                            else CreatePopUp("Your write questions need atleast 1 answer");
                            return;
                        }
                    }
                    break;
            }

            if(_question.type == QuestionType.Audio)
            {
                if((_question as AudioQuestion).audio == null || (_question as AudioQuestion).audio.audioData == null)
                {
                    CreatePopUp("Your audio question doesn't have any audio");
                    return;
                }
            }
        }

        createPanel.SetActive(false);
        savePanel.SetActive(true);

        quizInputField.text = createdQuiz.name;
    }
    public void BackToEdit()
    {
        createPanel.SetActive(true);
        savePanel.SetActive(false);
    }
    public void SaveQuiz()
    {
        if (string.IsNullOrWhiteSpace(createdQuiz.name))
        {
            CreatePopUp("Your quiz can't have an empty name");
            return;
        }

        if (Directory.Exists(savePath))
        {
            var _info = new DirectoryInfo(savePath);
            var _fileInfo = _info.GetFiles();

            foreach (var _file in _fileInfo)
            {
                if(_file.Name.Substring(0, _file.Name.Length - 4) == createdQuiz.name)
                {
                    areYouSurePanel.SetActive(true);
                    return;
                }

                print(_file.Name.Length - 4 + " vs " + createdQuiz.name);
            }
        }

        ConfirmedSave();
    }
    public void ConfirmedSave()
    {
        savePanel.SetActive(false);
        firstPanel.SetActive(true);
        areYouSurePanel.SetActive(false);

        isCreating = false;

        foreach (var item in boxes)
        {
            Destroy(item.gameObject);
        }

        boxes.Clear();
        currentQuestionIndex = 0;

        foreach (var _question in Questions)
        {
            for (int i = 0; i < _question.answers.Count; i++)
            {
                if (string.IsNullOrEmpty(_question.answers[i]))
                {
                    _question.answers.RemoveAt(i);
                    _question.corrections.RemoveAt(i);
                    i--;
                }
            }
        }

        string _saveName = createdQuiz.name + ".fun";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            //new DirectoryInfo(Application.persistentDataPath).CreateSubdirectory(path);
        }

        //string _jsonString = JsonUtility.ToJson(createdQuiz);

        //File.WriteAllText(Path.Combine(savePath ,_saveName), _jsonString);

        BinaryFormatter _formatter = new BinaryFormatter();
        FileStream _stream = new FileStream(Path.Combine(savePath, _saveName), FileMode.Create);

        _formatter.Serialize(_stream, createdQuiz);
        _stream.Close();
    }

    public void EditQuiz()
    {
        if (!Directory.Exists(savePath)) return;

        var _info = new DirectoryInfo(savePath);
        var _fileInfo = _info.GetFiles();

        if (_fileInfo.Length == 0) return;

        int i = 0;
        foreach (var _file in _fileInfo)
        {
            QuizBoxBehavior _quizBox = Instantiate(quizBoxPrefab).GetComponent<QuizBoxBehavior>();

            _quizBox.transform.SetParent(fileContents);
            _quizBox.transform.localPosition = new Vector3(0f, quizBoxes.Count * -quizBoxDistance);
            _quizBox.transform.localScale = Vector3.one;

            _quizBox.SetText(_file.Name.Substring(0,_file.Name.Length - 4),i);

            quizBoxes.Add(_quizBox);
            
            i++;
        }


        firstPanel.SetActive(false);
        fileListPanel.SetActive(true);
    }
    public void SelectQuiz(int _id)
    {
        selectedQuiz = _id;
        quizBoxBorder.localPosition = quizBoxes[_id].transform.localPosition;

        selectButton.interactable = true;
    }
    public void LoadQuiz()
    {
        currentQuestionIndex = 0;

        var _info = new DirectoryInfo(savePath);
        var _fileInfo = _info.GetFiles();

        for (int i = 0; i < _fileInfo.Length; i++)
        {
            if(i == selectedQuiz)
            {
                //string _jsonString = File.ReadAllText(_fileInfo[i].FullName);
                //print(_fileInfo[i].Name);
                
                //createdQuiz = JsonUtility.FromJson<Quiz>(_jsonString);

                BinaryFormatter _formatter = new BinaryFormatter();
                FileStream _stream = new FileStream(_fileInfo[i].FullName, FileMode.Open);

                createdQuiz = _formatter.Deserialize(_stream) as Quiz;
                _stream.Close();
                break;
            }
        }

        foreach (var item in quizBoxes)
        {
            Destroy(item.gameObject);
        }

        quizBoxes.Clear();

        foreach (var _question in Questions)
        {
            while(_question.answers.Count < 4)
            {
                _question.answers.Add(null);
                _question.corrections.Add(false);
            }
            QuestionBoxBehavior _questionBox = Instantiate(questionBoxPrefab).GetComponent<QuestionBoxBehavior>();

            _questionBox.transform.SetParent(questionTabContents);
            _questionBox.transform.localScale = Vector3.one;
            _questionBox.transform.localPosition = new Vector3(0f, (Questions.Count - 1) * -boxDistance, 0f);

            boxes.Add(_questionBox);
        }

        NumberBoxes();

        addButton.localPosition = new Vector3(0f, (Questions.Count - 1) * -boxDistance - 80f, 0f);

        fileListPanel.SetActive(false);
        createPanel.SetActive(true);

        isCreating = true;

        LoadQuestion();
    }

    public void AddImage()
    {
        var bp = new BrowserProperties();
        bp.filter = "Image files (*.jpg, *.png) | *.jpg; *.png";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            StartCoroutine(GetTexture(path));
        });
    }
    IEnumerator GetTexture(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if(uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                var maTexture = DownloadHandlerTexture.GetContent(uwr);
                
                QuizImage _newImage = new QuizImage
                {
                    imageData = maTexture.GetRawTextureData(),
                    width = maTexture.width,
                    height = maTexture.height,
                    format = maTexture.format
                };

                CurrentQuestion.image = _newImage;

                LoadImage();
            }
        }


    }
    void LoadImage()
    {

        if (CurrentQuestion.image == null || CurrentQuestion.image.imageData.Length == 0)
        {
            image.color = Color.black;
            noImageText.SetActive(true);
            addImageText.text = "Add image";
            image.rectTransform.sizeDelta = new Vector2(100, 100);
            return;
        }
        else
        {
            image.color = Color.white;
            addImageText.text = "Change image";
            noImageText.SetActive(false);

            Texture2D _texture = new Texture2D(CurrentQuestion.image.width, CurrentQuestion.image.height, CurrentQuestion.image.format, false);
            _texture.LoadRawTextureData(CurrentQuestion.image.imageData);
            _texture.Apply();


            int _width = _texture.width;
            int _height = _texture.height;
            if (_width > maxWidth)
            {
                _width = maxWidth;
                _height = _width * _texture.height / _texture.width;
            }

            if (_height > maxHeight)
            {
                _height = maxHeight;
                _width = _height * _texture.width / _texture.height;
            }

            image.texture = _texture;
            image.rectTransform.sizeDelta = new Vector2(_width, _height);
        }
    }

    public void AddAudio()
    {
        var bp = new BrowserProperties();
        bp.filter = "Audio files (*.mp3,) | *.mp3;";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            StartCoroutine(GetAudio(path));
        });
    }
    IEnumerator GetAudio(string path)
    {
        
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path,AudioType.MPEG))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                var _maAudio = DownloadHandlerAudioClip.GetContent(uwr);

                float[] _data = new float[_maAudio.channels * _maAudio.samples];
                _maAudio.GetData(_data, 0);

                currentClip = AudioClip.Create("mogus", _maAudio.samples, _maAudio.channels, _maAudio.frequency, false);
                currentClip.SetData(_data, 0);
                currentClip.LoadAudioData();

                QuizAudio _quizAudio = new QuizAudio()
                {
                    samples = _maAudio.samples,
                    channels = _maAudio.channels,
                    frequency = _maAudio.frequency,
                    audioData = _data,

                };

                (CurrentQuestion as AudioQuestion).audio = _quizAudio;

                addAudioText.text = "change audio";
                playButton.SetActive(true);

                print("adasdsadas");
            }
        }


    }
    public void PlayAudio()
    {
        source.clip = currentClip;
        source.Play();
    }
    public void StopAudio()
    {
        source.Stop();
    }

    public void MainMenu()
    {
        SceneTransition.current.ExitToScene(0,"Transition2_Exit");

        ReverseDaButtons();
    } 

    public void SwitchQuestionPlace(int _newPlace,int _oldPlace)
    {

        Question _currentQuestion = CurrentQuestion;
        Question _switchWith = Questions[_newPlace];

        Questions[_newPlace] = _currentQuestion;
        Questions[_oldPlace] = _switchWith;

        QuestionBoxBehavior _currentBox = boxes[_oldPlace];
        QuestionBoxBehavior _switchBox = boxes[_newPlace];

        _currentBox.SetText(_newPlace + 1);
        _switchBox.SetText(_oldPlace + 1);

        boxes[_newPlace] = _currentBox;
        boxes[_oldPlace] = _switchBox;

        currentQuestionIndex = _newPlace;
    }
    public void DeleteQuestion(int _questionIndex)
    {
        Questions.RemoveAt(_questionIndex);

        Destroy(boxes[_questionIndex].gameObject);
        boxes.RemoveAt(_questionIndex);

        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].SetText(i + 1);
        }

        if (currentQuestionIndex > Questions.Count - 1) currentQuestionIndex--;
        addButton.localPosition = new Vector3(0f, (Questions.Count - 1) * -boxDistance - 80f, 0f);
        LoadQuestion();

        if (tabOffset > 0) tabOffset--;
    }

    public void TimeFieldUpdate()
    {
        if (lastTime == timeLimitField.text) return;

        if (float.TryParse(timeLimitField.text, out float _result))
        {
            float _value = Mathf.Clamp(Mathf.Round(_result), timeLimitSlider.minValue, timeLimitSlider.maxValue);
            lastTime = _value.ToString();
            timeLimitSlider.value = _value;
        }
        timeLimitField.text = lastTime;
    }
    public void TimeSliderUpdate()
    {
        if (lastTimeSlider == timeLimitSlider.value) return;
        lastTimeSlider = timeLimitSlider.value;
        timeLimitField.text = lastTimeSlider.ToString();

        CurrentQuestion.time = lastTimeSlider;
    }
    public void PointsFieldUpdate()
    {
        if (lastPoint == pointsField.text) return;

        if(float.TryParse(pointsField.text,out float _result))
        {
            float _value = Mathf.Clamp(Mathf.Round(_result), pointsSlider.minValue, pointsSlider.maxValue);
            lastPoint = _value.ToString();
            pointsSlider.value = _value;
        }
        pointsField.text = lastPoint;
    }
    public void PointsSliderUpdate()
    {
        if (lastPointSlider == pointsSlider.value) return;
        lastPointSlider = pointsSlider.value;
        pointsField.text = lastPointSlider.ToString();

        CurrentQuestion.points = (int)lastPointSlider;
    }

    void ReverseDaButtons()
    {
        foreach (var item in FindObjectsOfType<Button>())
        {
            item.interactable = !item.interactable;
        }
    }

    void CreatePopUp(string _notice)
    {
        PopUpText a = Instantiate(popUpPrefab).GetComponent<PopUpText>();

        a.transform.SetParent(canvas);
        a.transform.localPosition = Vector3.zero;
        a.transform.localScale = Vector3.one;

        a.SetText(_notice);
    }
}

