using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField]
    private GameObject playerPrefab;

    private readonly Dictionary<int, RemotePlayer> _remotePlayers = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddPlayer(
        int playerId,
        string name,
        string title,
        Vector3 position)
    {
        if (_remotePlayers.ContainsKey(playerId))
            return;

        GameObject playerObject =
            Instantiate(
                playerPrefab,
                position,
                Quaternion.identity);

        playerObject.name =
            $"RemotePlayer_{playerId}";

        RemotePlayer remotePlayer =
            playerObject.AddComponent<RemotePlayer>();

        remotePlayer.Initialize(
            playerId,
            name,
            title);

        _remotePlayers.Add(
            playerId,
            remotePlayer);

        PlayerNameplate playerNameplate =
            playerObject.GetComponentInChildren<PlayerNameplate>();

        if (playerNameplate != null)
        {
            playerNameplate.SetPlayerInfo(
                name,
                title);
        }
    

}

    public void RemovePlayer(int playerId)
    {
        if (!_remotePlayers.TryGetValue(
                playerId,
                out RemotePlayer remotePlayer))
        {
            return;
        }


        Destroy(remotePlayer.gameObject);

        _remotePlayers.Remove(playerId);
    }

    public void UpdatePlayerPosition(
    int playerId,
    Vector3 position,
    float rotationY)
    {
        if (!_remotePlayers.TryGetValue(
                playerId,
                out RemotePlayer remotePlayer))
        {
            return;
        }

        remotePlayer.SetTargetPosition(
            position,
            rotationY);
    }

}