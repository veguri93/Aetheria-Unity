using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_camera == null)
            return;

        transform.rotation =
            _camera.transform.rotation;
    }

    public void SetHealth(
        int currentHp,
        int maxHp)
    {


        if (_slider == null)
        {


            return;
        }

        _slider.maxValue = maxHp;
        _slider.value = currentHp;

    }
}