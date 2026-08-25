using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider _slider;

    public void SetHealth(
        int currentHp,
        int maxHp)
    {
        if (_slider == null)
            return;

        _slider.maxValue = maxHp;
        _slider.value = currentHp;
    }
}