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
            playerObject.GetComponent<RemotePlayer>();

        if (remotePlayer == null)
        {
            Debug.LogError(
                $"Player prefab does not have a RemotePlayer component.");

            Destroy(
                playerObject);

            return;
        }

        remotePlayer.enabled =
    true;

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

    public void UpdatePlayerGuild(int playerId, string guildName)
    {
        if (_remotePlayers.TryGetValue(playerId, out RemotePlayer player))
            player.SetGuildName(guildName);
    }

    public void UpdatePlayerTitle(
    int playerId,
    string title)
    {
        if (!_remotePlayers.TryGetValue(
                playerId,
                out RemotePlayer remotePlayer))
        {
            return;
        }

        remotePlayer.SetTitle(
            title);
    }

    public void RemovePlayer(int playerId)
    {
        if (!_remotePlayers.TryGetValue(
                playerId,
                out RemotePlayer remotePlayer))
        {
            return;
        }


        if (TargetManager.Instance && ReferenceEquals(TargetManager.Instance.CurrentTarget, remotePlayer))
            TargetManager.Instance.ClearTarget();
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

    public bool TryGetPlayerByName(
    string playerName,
    out RemotePlayer player)
    {
        foreach (RemotePlayer remote in _remotePlayers.Values)
        {
            if (remote == null)
                continue;

            if (string.Equals(
                    remote.PlayerName,
                    playerName,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                player = remote;
                return true;
            }
        }

        player = null;
        return false;
    }

}