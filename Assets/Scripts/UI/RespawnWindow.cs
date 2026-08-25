using System.Net.Sockets;
using UnityEngine;

public class RespawnWindow : MonoBehaviour
{
    public static RespawnWindow Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void RespawnVillage()
    {
        Debug.Log("RESPAWN VILLAGE BUTTON CLICKED");

        GameServerConnection connection =
            GameServerConnection.Instance;

        if (connection == null)
        {
            Debug.LogError(
                "GameServerConnection not found.");

            return;
        }

        connection.SendRespawnRequest(
            "Village");
    }
}