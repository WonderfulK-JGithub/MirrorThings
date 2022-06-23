using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.EventSystems;

public class QuizMainMenu : MonoBehaviour
{

    public static QuizMainMenu current;
    public static string DisplayName { get; private set; }
    public static Quiz UsedQuiz { get; private set; }

    [SerializeField] GameObject firstPanel;
    [SerializeField] GameObject namePanel;
    [SerializeField] GameObject joinPanel;
    [SerializeField] GameObject fileSelectPanel;
    [SerializeField] TMP_InputField nameInputField;

    [SerializeField] Button nameButton;

    [Header("File")]
    [SerializeField] Transform fileContents;
    [SerializeField] GameObject quizBoxPrefab;
    [SerializeField] float quizBoxDistance;
    [SerializeField] Transform quizBoxBorder;
    [SerializeField] Button selectButton;

    [SerializeField] GraphicRaycaster raycaster;
    [SerializeField] EventSystem eventSystem;

    string path;

    int selectedQuiz;
    int tabOffset;

    bool hoverOverTab;

    List<QuizBoxBehavior> quizBoxes = new List<QuizBoxBehavior>();

    private void Awake()
    {
        current = this;
        path = Application.persistentDataPath + "/Saves";
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseCheck();
        }

        HoverCheck();

        if (hoverOverTab && Input.mouseScrollDelta != Vector2.zero)
        {
            tabOffset -= (int)Input.mouseScrollDelta.y;

            if (tabOffset > quizBoxes.Count - 6) tabOffset = quizBoxes.Count - 6;
            if (tabOffset < 0) tabOffset = 0;
        }

        fileContents.localPosition = Vector3.Lerp(fileContents.localPosition, new Vector3(0f, tabOffset * quizBoxDistance, 0f), Time.deltaTime * 30f);
    }

    public void NameEdited()
    {
        nameButton.interactable = !string.IsNullOrEmpty(nameInputField.text);
    }

    public void NameSelected()
    {
        DisplayName = nameInputField.text;

        namePanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    public void Join()
    {
        QuizNetworkManager.current.StartClient();
    }

    public void Host()
    {
        if (!Directory.Exists(path)) return;

        var _info = new DirectoryInfo(path);
        var _fileInfo = _info.GetFiles();

        if (_fileInfo.Length == 0) return;

        int i = 0;
        foreach (var _file in _fileInfo)
        {
            QuizBoxBehavior _quizBox = Instantiate(quizBoxPrefab).GetComponent<QuizBoxBehavior>();

            _quizBox.transform.SetParent(fileContents);
            _quizBox.transform.localPosition = new Vector3(0f, quizBoxes.Count * -quizBoxDistance);
            _quizBox.transform.localScale = Vector3.one;

            _quizBox.SetText(_file.Name.Substring(0, _file.Name.Length - 4), i);

            quizBoxes.Add(_quizBox);

            i++;
        }

        joinPanel.SetActive(false);
        fileSelectPanel.SetActive(true);
    }

    public void Back()
    {
        foreach (var item in quizBoxes)
        {
            Destroy(item.gameObject);
        }

        quizBoxes.Clear();


        joinPanel.SetActive(true);
        fileSelectPanel.SetActive(false);
    }
    public void SelectQuiz(int _id)
    {
        selectedQuiz = _id;
        quizBoxBorder.localPosition = quizBoxes[_id].transform.localPosition;

        selectButton.interactable = true;
    }
    public void StartHost()
    {
        var _info = new DirectoryInfo(path);
        var _fileInfo = _info.GetFiles();

        for (int i = 0; i < _fileInfo.Length; i++)
        {
            if (i == selectedQuiz)
            {
                string _jsonString = File.ReadAllText(_fileInfo[i].FullName);
                print(_fileInfo[i].Name);

                UsedQuiz = JsonUtility.FromJson<Quiz>(_jsonString);
                break;
            }
        }

        foreach (var item in quizBoxes)
        {
            Destroy(item.gameObject);
        }

        quizBoxes.Clear();

        QuizNetworkManager.current.StartHost();
    }

    public void Play()
    {
        firstPanel.SetActive(false);
        namePanel.SetActive(true);
    }

    public void Create()
    {
        SceneManager.LoadScene(2);
    }


    void MouseCheck()
    {
        PointerEventData _pointerEventData = new PointerEventData(eventSystem);
        _pointerEventData.position = Input.mousePosition;

        List<RaycastResult> _results = new List<RaycastResult>();

        raycaster.Raycast(_pointerEventData, _results);

        if (_results.Count > 0)
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
}
