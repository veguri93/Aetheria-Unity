using TMPro;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    [SerializeField]
    private TMP_InputField usernameInput;

    [SerializeField]
    private TMP_InputField passwordInput;



    private void Awake()
    {
        Instance = this;
    }

    public void Login()
    {
        GameServerConnection connection =
            GameServerConnection.Instance;

        if (connection == null)
        {
            Debug.LogError(
                "GameServerConnection is not available.");

            return;
        }

        string username =
            usernameInput.text.Trim();

        string password =
            passwordInput.text;

        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogWarning(
                "Username is empty.");

            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Debug.LogWarning(
                "Password is empty.");

            return;
        }



        connection.SendLogin(
            username,
            password);
    }

    public void OnLoginSuccessful()
    {


        gameObject.SetActive(false);
    }
}