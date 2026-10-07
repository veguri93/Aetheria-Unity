using TMPro;
using UnityEngine;

public class GuildCreateWindow : MonoBehaviour
{
    public static GuildCreateWindow Instance { get; private set; }

    [SerializeField]
    private TMP_InputField guildNameInput;

    private int _npcObjectId;

    private void Awake()
    {
        Instance =
            this;

        gameObject.SetActive(
            false);
    }

    public void Show(
        int npcObjectId)
    {
        _npcObjectId =
            npcObjectId;

        guildNameInput.text =
            "";

        gameObject.SetActive(
            true);

        guildNameInput.Select();
        guildNameInput.ActivateInputField();
    }

    public void Hide()
    {
        gameObject.SetActive(
            false);

        _npcObjectId =
            0;
    }

    public void CreateGuild()
    {
        string guildName =
            guildNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(
                guildName))
        {
            Debug.LogWarning(
                "Guild name is empty.");

            return;
        }

        GameServerConnection connection =
            GameServerConnection.Instance;

        if (connection == null)
        {
            Debug.LogError(
                "GameServerConnection is not available.");

            return;
        }

        connection.SendGuildCreateRequest(
            _npcObjectId,
            guildName);
    }
    public void OnCreateSuccess(
       string guildName)
    {
        Hide();
    }

    public void Cancel()
    {
        Hide();
    }
}