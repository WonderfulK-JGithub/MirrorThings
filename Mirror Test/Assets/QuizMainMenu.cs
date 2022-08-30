using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

public class QuizMainMenu : MonoBehaviour
{
    public static PlayMode UsedMode { get; private set; }
    public static QuizMainMenu current;
    public static string DisplayName { get; private set; }
    public static Quiz UsedQuiz { get; private set; }

    [SerializeField] GameObject firstPanel;
    [SerializeField] GameObject namePanel;
    [SerializeField] GameObject joinPanel;
    [SerializeField] GameObject fileSelectPanel;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] Button nameButton;
    [SerializeField] GameObject nameBackButton;

    [Header("File")]
    [SerializeField] Transform fileContents;
    [SerializeField] GameObject quizBoxPrefab;
    [SerializeField] float quizBoxDistance;
    [SerializeField] Transform quizBoxBorder;
    [SerializeField] Button selectButton;

    [SerializeField] GraphicRaycaster raycaster;
    [SerializeField] EventSystem eventSystem;

    [Header("PlaySettings")]
    [SerializeField] Toggle[] toggles;

    [Header("Other")]
    [SerializeField] GameObject popUpPrefab;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] Button joinWithIpButton;

    string path;

    int selectedQuiz;
    int tabOffset;

    int sceneToLoad;

    bool hoverOverTab;
    bool fileSelected;
    bool fuckYou;

    List<QuizBoxBehavior> quizBoxes = new List<QuizBoxBehavior>();

    IHoverable lastHover;

    private void Awake()
    {
        current = this;
        path = Path.Combine(Application.persistentDataPath, "Saves");

        print(GetLocalIPAddress());
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
        PlayerPrefs.SetString("Name", DisplayName);
        nameText.text = DisplayName;

        namePanel.SetActive(false);
        joinPanel.SetActive(true);
        nameBackButton.SetActive(true);
    }

    public void IpEdited()
    {
        joinWithIpButton.interactable = !string.IsNullOrEmpty(ipInputField.text);
    }

    public void Join()
    {
        QuizNetworkManager.current.networkAddress = GetLocalIPAddress();

        SceneTransition.OnTransitionExit += StartJoin;
        SceneTransition.current.Exit();
        ReverseDaButtons();
    }
    public void JoinWithIP()
    {
        QuizNetworkManager.current.networkAddress = ipInputField.text;

        SceneTransition.OnTransitionExit += StartJoin;
        SceneTransition.current.Exit();
        ReverseDaButtons();
    }
    public void StartJoin()
    {
        QuizNetworkManager.current.StartClient();
        SceneTransition.OnTransitionExit -= StartJoin;
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

        SceneTransition.OnTransitionExit += StartHost;
    }

    public void Back()
    {
        foreach (var item in quizBoxes)
        {
            Destroy(item.gameObject);
        }

        quizBoxes.Clear();

        SceneTransition.OnTransitionExit -= StartHost;

        joinPanel.SetActive(true);
        fileSelectPanel.SetActive(false);
    }
    public void SelectQuiz(int _id)
    {
        selectedQuiz = _id;
        quizBoxBorder.localPosition = quizBoxes[_id].transform.localPosition;

        selectButton.interactable = true;
        fileSelected = true;
    }
    public void StartHost()
    {
        

        var _info = new DirectoryInfo(path);
        var _fileInfo = _info.GetFiles();

        for (int i = 0; i < _fileInfo.Length; i++)
        {
            if (i == selectedQuiz)
            {
                //string _jsonString = File.ReadAllText(_fileInfo[i].FullName);
                //print(_fileInfo[i].Name);

                //UsedQuiz = JsonUtility.FromJson<Quiz>(_jsonString);

                BinaryFormatter _formatter = new BinaryFormatter();
                FileStream _stream = new FileStream(_fileInfo[i].FullName, FileMode.Open);

                UsedQuiz = _formatter.Deserialize(_stream) as Quiz;
                _stream.Close();
                break;
            }
        }

        foreach (var item in quizBoxes)
        {
            Destroy(item.gameObject);
        }

        quizBoxes.Clear();

        QuizNetworkManager.current.StartHost();

        SceneTransition.OnTransitionExit -= StartHost;
    }

    public void Play()
    {
        DisplayName = PlayerPrefs.GetString("Name", null);
        firstPanel.SetActive(false);
        if (!string.IsNullOrWhiteSpace(DisplayName))
        {
            joinPanel.SetActive(true);
            nameText.text = DisplayName;
        }
        else
        {
            nameBackButton.SetActive(false);
            namePanel.SetActive(true);
        }

        
    }
    public void Create()
    {
        sceneToLoad = 2;
        SceneTransition.OnTransitionExit += LoadSceneToLoad;
        SceneTransition.current.Exit("Transition2_Exit");
        Destroy(QuizNetworkManager.current.gameObject);

        ReverseDaButtons();
    }
    public void LoadSceneToLoad()
    {
        SceneTransition.OnTransitionExit -= LoadSceneToLoad;
        SceneManager.LoadScene(sceneToLoad);
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
                if (_component != null)
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

    public void ToggleUpdate(GameObject _o)
    {
        if (fuckYou) return;

        fuckYou = true;

        for (int i = 0; i < toggles.Length; i++)
        {

            if (toggles[i].gameObject != _o)
            {
                toggles[i].isOn = false;
                toggles[i].interactable = true;
            }
            else
            {
                toggles[i].interactable = false;
                UsedMode = (PlayMode)i;
            }
        }

        fuckYou = false;
    }

    private void OnEnable()
    {
        QuizNetworkManager.OnClientDisconnected += ConnectionFailed;
    }
    private void OnDisable()
    {
        QuizNetworkManager.OnClientDisconnected -= ConnectionFailed;
    }
    void ConnectionFailed()
    {
        SceneTransition.current.ReEnter();
        ReverseDaButtons();

        PopUpText a = Instantiate(popUpPrefab).GetComponent<PopUpText>();

        a.transform.SetParent(firstPanel.transform.parent);
        a.transform.localPosition = Vector3.zero;
        a.transform.localScale = Vector3.one;

        a.SetText("No server to connect to");
    }

    public void ReverseDaButtons()
    {
        foreach (var item in FindObjectsOfType<Button>())
        {
            item.interactable = !item.interactable;
        }
    }


    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        int i = 0;

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                
                return ip.ToString();
            }
            i++;
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }


}

public enum PlayMode
{
    Default,
    HostOnly,
    BigScreen,
}