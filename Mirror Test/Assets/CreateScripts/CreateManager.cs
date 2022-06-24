using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using AnotherFileBrowser.Windows;
using UnityEngine.Networking;


public class CreateManager : MonoBehaviour
{
    string savePath;

    const int maxFiles = 100;

    public static CreateManager current;

    [SerializeField] GameObject firstPanel;
    [SerializeField] GameObject createPanel;
    [SerializeField] GameObject savePanel;
    [SerializeField] GameObject fileListPanel;

    [SerializeField] TMP_InputField questionInputField;
    [SerializeField] TMP_InputField[] answerInputFields;
    [SerializeField] Toggle[] trueFalseToggles;
    [SerializeField] Image[] trueFalseImages;

    [SerializeField] Transform questionTabContents;
    [SerializeField] GameObject questionBoxPrefab;
    [SerializeField] Transform addButton;
    [SerializeField] Transform boxBorder;
    [SerializeField] float boxDistance;

    [SerializeField] GraphicRaycaster raycaster;
    [SerializeField] EventSystem eventSystem;

    [SerializeField] TMP_InputField quizInputField;

    [SerializeField] Transform fileContents;
    [SerializeField] GameObject quizBoxPrefab;
    [SerializeField] float quizBoxDistance;
    [SerializeField] Transform quizBoxBorder;
    [SerializeField] Button selectButton;

    Quiz createdQuiz;

    public int currentQuestionIndex;

    [Header("Image")]
    [SerializeField] RawImage image;
    [SerializeField] int maxWidth;
    [SerializeField] int maxHeight;
    [SerializeField] GameObject noImageText;
    [SerializeField] TextMeshProUGUI addImageText;

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

    List<QuestionBoxBehavior> boxes = new List<QuestionBoxBehavior>();
    List<QuizBoxBehavior> quizBoxes = new List<QuizBoxBehavior>();

    int tabOffset;
    int selectedQuiz;

    bool fuckYou;
    bool hoverOverTab;
    bool isCreating;

    void Awake()
    {
        current = this;

        savePath = Application.persistentDataPath + "/Saves";
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
            if (hoverOverTab && Input.mouseScrollDelta != Vector2.zero)
            {
                tabOffset -= (int)Input.mouseScrollDelta.y;

                if (tabOffset > boxes.Count - 3) tabOffset = boxes.Count - 3;
                if (tabOffset < 0) tabOffset = 0;
            }

            questionTabContents.localPosition = Vector3.Lerp(questionTabContents.localPosition, new Vector3(0f, tabOffset * boxDistance, 0f), Time.deltaTime * 30f);
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

        AddNewQuestion();

        isCreating = true;
    }

    public void AddNewQuestion()
    {
        Question _newQuestion = new Question();

        Questions.Add(_newQuestion);

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

        questionInputField.text = Questions[currentQuestionIndex].question;

        for (int i = 0; i < 4; i++)
        {
            answerInputFields[i].text = Questions[currentQuestionIndex].answers[i];
        }

        for (int i = 0; i < trueFalseToggles.Length; i++)
        {
            trueFalseImages[i].color = CurrentQuestion.corrections[i] ? Color.green : Color.blue;
            trueFalseToggles[i].isOn = CurrentQuestion.corrections[i];
        }

        fuckYou = false;

        boxBorder.localPosition = boxes[currentQuestionIndex].transform.localPosition;

        LoadImage();

        HandleAnswerFields();
    }

    void HandleAnswerFields()
    {
        if(string.IsNullOrEmpty(CurrentQuestion.answers[0]) || string.IsNullOrEmpty(CurrentQuestion.answers[1]))
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
            if(string.IsNullOrEmpty(answerInputFields[i].text))
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
            foreach (var _item in _results)
            {
                if (_item.gameObject.CompareTag("Tab"))
                {
                    hoverOverTab = true;

                    return;
                }
            }

        }
    }

    public void MoveToSave()
    {
        foreach (var _question in Questions)
        {
            if (string.IsNullOrEmpty(_question.question)) return;
            if (string.IsNullOrEmpty(_question.answers[0]) || string.IsNullOrEmpty(_question.answers[1])) return;
            int trueCount = 0;
            for (int i = 0; i < _question.answers.Count; i++)
            {
                if (!string.IsNullOrEmpty(_question.answers[i]) && _question.corrections[i]) trueCount++;
            }
            if (trueCount == 0) return;
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
        if (string.IsNullOrEmpty(createdQuiz.name)) return;

        savePanel.SetActive(false);
        firstPanel.SetActive(true);

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
                if(string.IsNullOrEmpty(_question.answers[i]))
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
        
        string _jsonString = JsonUtility.ToJson(createdQuiz);

        File.WriteAllText(savePath + _saveName, _jsonString);
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
        var _info = new DirectoryInfo(savePath);
        var _fileInfo = _info.GetFiles();

        for (int i = 0; i < _fileInfo.Length; i++)
        {
            if(i == selectedQuiz)
            {
                string _jsonString = File.ReadAllText(_fileInfo[i].FullName);
                print(_fileInfo[i].Name);

                createdQuiz = JsonUtility.FromJson<Quiz>(_jsonString);
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
        if(CurrentQuestion.image == null)
        {
            image.color = Color.black;
            noImageText.SetActive(true);
            addImageText.text = "Add image";
            return;
        }
        else
        {
            image.color = Color.white;
            addImageText.text = "Change image";
            noImageText.SetActive(false);
        }


        Texture2D _texture = new Texture2D(CurrentQuestion.image.width, CurrentQuestion.image.height, CurrentQuestion.image.format,false);
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

public interface IClickable
{
    void Click();
}