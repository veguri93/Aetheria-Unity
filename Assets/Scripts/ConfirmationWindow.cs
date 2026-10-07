using System;
using UnityEngine;

public class ConfirmationWindow : MonoBehaviour
{
    public static ConfirmationWindow Instance { get; private set; }

    private Action _confirmAction;

    private void Awake()
    {
        Instance =
            this;

        Hide();
    }

    public void Show(
        Action confirmAction)
    {
        _confirmAction =
            confirmAction;

        gameObject.SetActive(
            true);
    }

    public void Confirm()
    {
        Action action =
            _confirmAction;

        Hide();

        action?.Invoke();
    }

    public void Cancel()
    {
        Hide();
    }

    public void Hide()
    {
        _confirmAction =
            null;

        gameObject.SetActive(
            false);
    }
}