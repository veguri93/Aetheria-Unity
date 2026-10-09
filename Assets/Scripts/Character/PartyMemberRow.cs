using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberRow : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private Slider hpSlider;

    [SerializeField]
    private Slider mpSlider;

    public int PlayerId { get; private set; }

    public void SetData(
        int playerId,
        string playerName)
    {
        PlayerId =
            playerId;

        nameText.text =
            playerName;

        // Temporary until we connect
        // real party HP/MP updates.
        hpSlider.value =
            1f;

        mpSlider.value =
            1f;
    }
    public void SetVitals(
    int currentHp,
    int maxHp,
    int currentMp,
    int maxMp)
    {
        if (maxHp > 0)
        {
            hpSlider.value =
                (float)currentHp /
                maxHp;
        }

        if (maxMp > 0)
        {
            mpSlider.value =
                (float)currentMp /
                maxMp;
        }
    }
}