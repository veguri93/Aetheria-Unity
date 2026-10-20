using TMPro;
using UnityEngine;

public class PlayerNameplate : MonoBehaviour
{
    [SerializeField] private WorldNameplate worldNameplate;
    [SerializeField] private Sprite guildCrest;

    public void SetGuildCrest(Sprite crest)
    {
        guildCrest = crest;
        if (worldNameplate && worldNameplate.TargetIndicator)
            worldNameplate.TargetIndicator.SetGuildCrest(crest);
    }
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text titleText;

    public void SetPlayerInfo(
        string playerName,
        string title)
    {
        if (worldNameplate)
        {
            worldNameplate.SetInfo(playerName, title);
            SetGuildCrest(guildCrest);
            return;
        }
        nameText.text = playerName;

        if (string.IsNullOrWhiteSpace(title))
        {
            titleText.gameObject.SetActive(false);
        }
        else
        {
            titleText.gameObject.SetActive(true);
            titleText.text = title;
        }
    }
}
