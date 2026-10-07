using System.Text;
using UnityEngine;

public static class LoginResponsePacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        if (data != "SUCCESS")
            return;

        if (LoginManager.Instance != null)
        {
            LoginManager.Instance
                .OnLoginSuccessful();
        }
        else
        {
            Debug.LogError(
                "LoginManager instance not found.");
        }

        if (GameServerConnection.Instance == null)
        {
            Debug.LogError(
                "GameServerConnection instance not found.");

            return;
        }

        GameServerConnection.Instance
            .SendHello();
    }
}