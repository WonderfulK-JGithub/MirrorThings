using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] NetworkManager network;

    [Header("UI")]
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;

    public static string DisplayName { get; private set; }

    private const string PlayerPrefsKey = "PlayerName";

    void Start()
    {
        SetUpInstanceField();
    }

    void SetUpInstanceField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            hostButton.interactable = false;
            joinButton.interactable = false;
            return;
        }

        string defaultName = PlayerPrefs.GetString(PlayerPrefsKey);

        nameInputField.text = defaultName;
        DisplayName = defaultName;

        CheckPlayerName(defaultName);
    }

    public void CheckPlayerName(string name)//if the name is valid
    {
        hostButton.interactable = !string.IsNullOrEmpty(name);
        joinButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;

        PlayerPrefs.SetString(PlayerPrefsKey, nameInputField.text);

        CheckPlayerName(DisplayName);
    }

    public void Host()
    {
        network.StartHost();
    }
    public void Join()
    {
        network.StartClient();
    }
}


public interface IInteractable
{
    void Interact();
}