using TMPro;
using UnityEngine;

public class GuildTitleWindow : MonoBehaviour
{
    public static GuildTitleWindow Instance { get; private set; }

    [SerializeField]
    private TMP_InputField titleInput;

    private void Awake()
    {
        Instance = this;

        Hide();
    }

    public void Show()
    {
        titleInput.text = "";

        gameObject.SetActive(
            true);

        GetComponent<FantasyPopupPresentation>()?.Present();
        titleInput.Select();
        titleInput.ActivateInputField();
    }

    public void Confirm()
    {
        string title =
            titleInput.text.Trim();

        GameServerConnection.Instance?
            .SendGuildTitleChangeRequest(
                title);

        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(
            false);
    }
}