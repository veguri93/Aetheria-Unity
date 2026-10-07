using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcHtmlWindow : MonoBehaviour
{
    public static NpcHtmlWindow Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI _contentText;

    [SerializeField]
    private Transform _buttonContainer;

    [SerializeField]
    private Button _buttonTemplate;

    private int _npcObjectId;
    private int _npcTemplateId;

    public int NpcObjectId =>
        _npcObjectId;

    public int NpcTemplateId =>
        _npcTemplateId;

    private void Awake()
    {
        Instance =
            this;

        if (_buttonTemplate != null)
        {
            _buttonTemplate.gameObject.SetActive(
                false);
        }

        gameObject.SetActive(
            false);
    }

    public void Show(
        int npcObjectId,
        int npcTemplateId,
        string html)
    {
        _npcObjectId =
            npcObjectId;

        _npcTemplateId =
            npcTemplateId;

        ClearButtons();

        CreateButtons(
            html);

        string displayText =
            ConvertBasicHtml(
                html);

        if (_contentText != null)
        {
            _contentText.text =
                displayText;
        }

        gameObject.SetActive(
            true);
    }

    public void Hide()
    {
        gameObject.SetActive(
            false);

        _npcObjectId =
            0;

        _npcTemplateId =
            0;

        ClearButtons();
    }

    private void CreateButtons(
        string html)
    {
        if (_buttonContainer == null ||
            _buttonTemplate == null)
        {
            return;
        }

        MatchCollection matches =
            Regex.Matches(
                html,
                "<button\\s+value=\"([^\"]+)\"\\s+action=\"([^\"]+)\"\\s*>",
                RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            string buttonText =
                match.Groups[1].Value;

            string action =
                match.Groups[2].Value;

            Button button =
                Instantiate(
                    _buttonTemplate,
                    _buttonContainer);

            button.gameObject.SetActive(
                true);

            TextMeshProUGUI label =
                button.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
            {
                label.text =
                    buttonText;
            }

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                () => HandleAction(
                    action));
        }
    }

    private void HandleAction(
        string action)
    {
        if (string.IsNullOrWhiteSpace(
                action))
        {
            return;
        }

        string[] parts =
            action.Split(
                ' ',
                System.StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return;

        if (!int.TryParse(
                parts[1],
                out int actionId) ||
            actionId < 0)
        {
            Debug.LogWarning(
                $"Invalid NPC HTML action: {action}");

            return;
        }

        string actionType =
            parts[0].ToLowerInvariant();

        switch (actionType)
        {
            case "multisell":
                {
                    if (actionId <= 0)
                        return;

                    GameServerConnection.Instance?
                        .SendMultiSellOpenRequest(
                            _npcObjectId,
                            actionId);

                    break;
                }

            case "html":
                {
                    GameServerConnection.Instance?
                        .SendNpcHtmlPageRequest(
                            _npcObjectId,
                            actionId);

                    break;
                }

            case "teleport":
                {
                    if (actionId <= 0)
                        return;

                    GameServerConnection.Instance?
                        .SendTeleportRequest(
                            _npcObjectId,
                            actionId);

                    break;
                }

            case "classchange":
                {
                    GameServerConnection.Instance?
                        .SendClassChangeRequest(
                            _npcObjectId,
                            actionId);

                    break;
                }

            case "learnskill":
                {
                    GameServerConnection.Instance?
                        .SendSkillLearnRequest(
                            _npcObjectId,
                            actionId);

                    break;
                }

            case "createguild":
                {
                    GuildCreateWindow.Instance?
                        .Show(
                            _npcObjectId);

                    Hide();

                    break;
                }

            default:
                {
                    Debug.LogWarning(
                        $"Unknown NPC HTML action: {action}");

                    break;
                }
        }
    }

    private void ClearButtons()
    {
        if (_buttonContainer == null ||
            _buttonTemplate == null)
        {
            return;
        }

        for (int i =
                 _buttonContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                _buttonContainer.GetChild(
                    i);

            if (child.gameObject ==
                _buttonTemplate.gameObject)
            {
                continue;
            }

            Destroy(
                child.gameObject);
        }
    }

    private static string ConvertBasicHtml(
        string html)
    {
        string text =
            Regex.Replace(
                html,
                "<button[^>]*>",
                "",
                RegexOptions.IgnoreCase);

        return text
            .Replace(
                "<html>",
                "")
            .Replace(
                "</html>",
                "")
            .Replace(
                "<body>",
                "")
            .Replace(
                "</body>",
                "")
            .Replace(
                "<br>",
                "\n")
            .Trim();
    }
}