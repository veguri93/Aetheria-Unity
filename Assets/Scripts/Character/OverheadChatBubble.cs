using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class OverheadChatBubble : MonoBehaviour
{
    private const int MaxCharacters = 15;
    private const float DisplayDuration = 5f;
    private const float FadeDuration = 1f;

    private WorldNameplate nameplate;
    private TextMeshPro chatLabel;
    private float hideTime;

    public void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (nameplate == null)
            nameplate = GetComponentInChildren<WorldNameplate>(true);

        if (nameplate == null || nameplate.Label == null)
            return;

        if (chatLabel == null)
        {
            // Reuse the existing nameplate font and rendering setup.
            chatLabel = Instantiate(
                nameplate.Label,
                transform,
                false);

            chatLabel.name = "OverheadChatText";
            chatLabel.richText = false;
            chatLabel.raycastTarget = false;
            chatLabel.alignment = TextAlignmentOptions.Center;
            chatLabel.enableWordWrapping = false;

            chatLabel.fontSize =
                nameplate.Label.fontSize * 0.9f;

            chatLabel.enabled = false;
        }

        string preview = message
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Trim();

        if (preview.Length > MaxCharacters)
        {
            preview = preview
                .Substring(0, MaxCharacters - 3)
                .TrimEnd();
        }

        // Always end the preview with ...
        chatLabel.text = preview.TrimEnd('.') + "...";

        hideTime = Time.time + DisplayDuration;

        chatLabel.enabled = true;
    }

    private void LateUpdate()
    {
        if (chatLabel == null || !chatLabel.enabled)
            return;

        float remaining = hideTime - Time.time;

        if (remaining <= 0f)
        {
            chatLabel.enabled = false;
            return;
        }

        Camera camera = Camera.main;

        if (camera == null || nameplate == null)
            return;

        // Above the existing player name and title.
        float titleOffset = nameplate.Style != null
            ? nameplate.Style.titleVerticalSeparation
            : 0.2f;

        Vector3 position =
            nameplate.AnchorPosition +
            Vector3.up * (titleOffset + 0.6f);

        chatLabel.transform.SetPositionAndRotation(
            position,
            camera.transform.rotation);

        Vector3 scale = transform.lossyScale;

        chatLabel.transform.localScale = new Vector3(
            1f / Mathf.Max(Mathf.Abs(scale.x), 0.001f),
            1f / Mathf.Max(Mathf.Abs(scale.y), 0.001f),
            1f / Mathf.Max(Mathf.Abs(scale.z), 0.001f));

        Color color = new Color(1f, 0.95f, 0.8f);

        color.a = Mathf.Clamp01(
            remaining / FadeDuration);

        chatLabel.color = color;
    }
}