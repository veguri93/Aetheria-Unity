using UnityEngine;
using UnityEngine.UI;

public class BuffSlotUI : MonoBehaviour
{
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private Image _durationOverlay;

    private int _skillId;

    private float _expiresAt;
    private float _durationSeconds;

    public int SkillId =>
        _skillId;

    public float RemainingSeconds =>
        Mathf.Max(
            0f,
            _expiresAt -
            Time.unscaledTime);

    private void Awake()
    {
        if (_durationOverlay == null)
            return;

        _durationOverlay.type =
            Image.Type.Filled;

        _durationOverlay.fillMethod =
            Image.FillMethod.Radial360;

        _durationOverlay.fillOrigin =
            (int)Image.Origin360.Top;

        _durationOverlay.fillClockwise =
            true;

        _durationOverlay.fillAmount =
            0f;

        _durationOverlay.raycastTarget =
            false;
    }

    private void Update()
    {
        UpdateDurationVisual();
    }

    public void Initialize(
        int skillId,
        Sprite icon,
        int durationMilliseconds)
    {
        _skillId =
            skillId;

        if (_icon != null)
        {
            _icon.sprite =
                icon;
        }

        if (_durationOverlay != null)
        {
            _durationOverlay.sprite =
                icon;

            _durationOverlay.preserveAspect =
                true;
        }

        Refresh(
            durationMilliseconds);
    }

    public void Refresh(
        int durationMilliseconds)
    {
        _durationSeconds =
            durationMilliseconds /
            1000f;

        _expiresAt =
            Time.unscaledTime +
            _durationSeconds;

        UpdateDurationVisual();
    }

    private void UpdateDurationVisual()
    {
        if (_durationOverlay == null)
            return;

        if (_durationSeconds <= 0f)
        {
            _durationOverlay.fillAmount =
                0f;

            return;
        }

        float normalizedRemaining =
            RemainingSeconds /
            _durationSeconds;

        _durationOverlay.fillAmount =
            Mathf.Clamp01(
                normalizedRemaining);
    }
}