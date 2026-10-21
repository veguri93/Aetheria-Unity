using TMPro;
using UnityEngine;

public class GuildCreateWindow : MonoBehaviour
{
    public static GuildCreateWindow Instance { get; private set; }

    [SerializeField]
    private TMP_InputField guildNameInput;

    [SerializeField] private TMP_Text validationText;

    private int _npcObjectId;

    private void Awake()
    {
        Instance =
            this;

        guildNameInput.onValueChanged.AddListener(ClearValidation);

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

        ClearValidation(string.Empty);
        GetComponent<FantasyPopupPresentation>()?.Present();

        guildNameInput.Select();
        guildNameInput.ActivateInputField();
    }

    public void Hide()
    {
        ClearValidation(string.Empty);
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
            SetValidation("Enter a guild name to continue.");

            Debug.LogWarning(
                "Guild name is empty.");

            return;
        }

        GameServerConnection connection =
            GameServerConnection.Instance;

        if (connection == null)
        {
            SetValidation("Connection is not available.");

            Debug.LogError(
                "GameServerConnection is not available.");

            return;
        }

        ClearValidation(string.Empty);

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
    private void ClearValidation(string value) => SetValidation(string.Empty);
    private void SetValidation(string message)
    {
        if (validationText) validationText.text = message;
    }
    private void OnDestroy()
    {
        if (guildNameInput) guildNameInput.onValueChanged.RemoveListener(ClearValidation);
    }
}
