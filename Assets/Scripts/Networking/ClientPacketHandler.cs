using System.Text;
using UnityEngine;

public static class ClientPacketHandler
{

    public static void Handle(ClientPacket packet)
    {
        switch (packet.Type)
        {
            case 3:
                HandleHelloAck(packet);
                break;

            case 4:
                HandlePlayerEnter(packet);
                break;

            case 5:
                HandlePlayerLeave(packet);
                break;

            case 6:
                HandlePlayerMove(packet);
                break;

            case 7:
                HandlePlayerSpawn(packet);
                break;

            case 8:
                HandleChatMessage(packet);
                break;

            case 10:
                HandleLoginResponse(packet);
                break;

            case 12:
                HandleMonsterDamage(packet);
                break;

            case 13:
                HandleMonsterSpawn(packet);
                break;

            case 14:
                HandleMonsterRespawn(packet);
                break;

            case 15:
                HandleMonsterDespawn(packet);
                break;

            case 16:
                HandlePlayerDamage(packet);
                break;

            case 18:
                HandlePlayerRespawnPrompt(packet);
                break;

            case 19:
                HandlePlayerRespawn(packet);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown packet type: {packet.Type}");
                break;
        }
    }

    private static void HandleMonsterDamage(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 5)
            return;

        if (!int.TryParse(parts[0], out int objectId) ||
            !int.TryParse(parts[1], out int templateId) ||
            !int.TryParse(parts[2], out int damage) ||
            !int.TryParse(parts[3], out int currentHp) ||
            !int.TryParse(parts[4], out int maxHp))
        {
            return;
        }

        Monster[] monsters =
            Object.FindObjectsByType<Monster>();

        foreach (Monster monster in monsters)
        {
            if (monster.ObjectId != objectId)
                continue;

            string npcName = "Unknown NPC";

            if (NpcTemplateManager.Instance != null &&
                NpcTemplateManager.Instance.TryGetTemplate(
                    templateId,
                    out NpcTemplate template))
            {
                npcName = template.Name;
            }

            monster.SetServerHealth(
                currentHp,
                maxHp);

            ChatManager.Instance?.AddCombatMessage(
                GameMessageType.DamageDealt,
                $"You dealt {damage} damage to {npcName}.");

            return;
        }
    }

    private static void HandleHelloAck(ClientPacket packet)
    {
    }
    private static void HandleLoginResponse(ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);



        if (data != "SUCCESS")
            return;



        if (LoginManager.Instance != null)
        {
            LoginManager.Instance.OnLoginSuccessful();
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

        GameServerConnection.Instance.SendHello();
    }


    private static void HandleMonsterSpawn(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 6)
            return;

        if (!int.TryParse(parts[0], out int objectId) ||
            !int.TryParse(parts[1], out int templateId) ||
            !float.TryParse(parts[2], out float x) ||
            !float.TryParse(parts[3], out float y) ||
            !float.TryParse(parts[4], out float z) ||
            !float.TryParse(parts[5], out float rotation))
        {
            return;
        }

        NpcTemplateManager manager =
            NpcTemplateManager.Instance;

        if (manager == null)
            return;

        if (!manager.TryGetTemplate(
                templateId,
                out NpcTemplate template))
        {
            Debug.LogWarning(
                $"Unknown NPC template: {templateId}");

            return;
        }

        GameObject monsterObject =
            Object.Instantiate(
                template.Prefab,
                new Vector3(x, y, z),
                Quaternion.Euler(
                    0f,
                    rotation,
                    0f));

        Monster monster =
            monsterObject.GetComponent<Monster>();

        if (monster == null)
        {


            Object.Destroy(monsterObject);
            monster.SetNpcName(template.Name);
            return;
        }

        monster.SetObjectId(objectId);
        MonsterManager.Instance.AddMonster(monster);


    }

    private static void HandleChatMessage(ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);


        string[] parts = data.Split(
            '|',
            3,
            System.StringSplitOptions.None);

        if (parts.Length != 3)
            return;

        string chatType = parts[0];
        string chatScope = parts[1];
        string message = parts[2];

        if (ChatManager.Instance == null)
            return;

        if (System.Enum.TryParse(
                chatType,
                out ChatType type) &&
            System.Enum.TryParse(
                chatScope,
                out ChatScope scope))
        {
            ChatManager.Instance.AddMessage(
                type,
                scope,
                message);
        }
    }

    private static void HandlePlayerSpawn(ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split(
                '|',
                6,
                System.StringSplitOptions.None);

        if (parts.Length != 6)
            return;

        string playerName = parts[0];
        string title = parts[1];

        if (!float.TryParse(
                parts[2],
                out float x) ||
            !float.TryParse(
                parts[3],
                out float y) ||
            !float.TryParse(
                parts[4],
                out float z) ||
            !float.TryParse(
                parts[5],
                out float rotationY))
        {
            return;
        }

        Vector3 position =
            new Vector3(x, y, z);

        LocalPlayer localPlayer =
            Object.FindAnyObjectByType<LocalPlayer>();

        if (localPlayer == null)
            return;

        localPlayer.SetSpawnPosition(
            position,
            rotationY);

        localPlayer.SetPlayerInfo(
            playerName,
            title);
    }

    private static void HandlePlayerEnter(ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split(
                '|',
                6,
                System.StringSplitOptions.None);

        if (parts.Length != 6)
            return;

        if (!int.TryParse(
                parts[0],
                out int playerId))
        {
            return;
        }

        string name = parts[1];
        string title = parts[2];

        if (!float.TryParse(
                parts[3],
                out float x) ||
            !float.TryParse(
                parts[4],
                out float y) ||
            !float.TryParse(
                parts[5],
                out float z))
        {
            return;
        }

        Vector3 position =
            new Vector3(x, y, z);

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.AddPlayer(
                playerId,
                name,
                title,
                position);
        }


    }

    private static void HandlePlayerLeave(ClientPacket packet)
    {
        string data = Encoding.UTF8.GetString(packet.Data);

        if (!int.TryParse(data, out int playerId))
        {

            return;
        }



        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.RemovePlayer(playerId);
        }
    }

    private static void HandlePlayerMove(ClientPacket packet)
    {
        string data = Encoding.UTF8.GetString(packet.Data);

        string[] parts = data.Split('|');

        if (parts.Length != 5)
        {


            return;
        }

        if (!int.TryParse(parts[0], out int playerId) ||
            !float.TryParse(parts[1], out float x) ||
            !float.TryParse(parts[2], out float y) ||
            !float.TryParse(parts[3], out float z) ||
            !float.TryParse(parts[4], out float rotationY))
        {


            return;
        }

        Vector3 position = new Vector3(x, y, z);



        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.UpdatePlayerPosition(
                playerId,
                position,
                rotationY);
        }
    }


    private static void HandleMonsterRespawn(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 6)
            return;

        if (!int.TryParse(parts[0], out int objectId) ||
            !int.TryParse(parts[1], out int templateId) ||
            !float.TryParse(parts[2], out float x) ||
            !float.TryParse(parts[3], out float y) ||
            !float.TryParse(parts[4], out float z) ||
            !float.TryParse(parts[5], out float rotation))
        {
            return;
        }

        if (MonsterManager.Instance == null)
            return;

        if (!MonsterManager.Instance.TryGetMonster(
                objectId,
                out Monster monster))
        {
            Debug.LogWarning(
                $"Monster {objectId} not found for respawn.");

            return;
        }

        monster.RespawnFromServer(
            new Vector3(x, y, z),
            rotation);
    }
    private static void HandleMonsterDespawn(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        if (!int.TryParse(
                data,
                out int objectId))
        {
            return;
        }

        if (MonsterManager.Instance == null)
            return;

        if (!MonsterManager.Instance.TryGetMonster(
                objectId,
                out Monster monster))
        {
            return;
        }

        monster.DespawnFromServer();
    }

    private static void HandlePlayerDamage(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 5)
            return;

        if (!int.TryParse(parts[0], out int objectId) ||
            !int.TryParse(parts[1], out int templateId) ||
            !int.TryParse(parts[2], out int damage) ||
            !int.TryParse(parts[3], out int currentHp) ||
            !int.TryParse(parts[4], out int maxHp))
        {
            return;
        }

        PlayerHealth health =
            Object.FindAnyObjectByType<PlayerHealth>();

        if (health == null)
            return;

        health.SetServerHealth(
            currentHp,
            maxHp);

        string npcName = "Unknown NPC";

        if (NpcTemplateManager.Instance != null &&
            NpcTemplateManager.Instance.TryGetTemplate(
                templateId,
                out NpcTemplate template))
        {
            npcName = template.Name;
        }

        ChatManager.Instance?.AddCombatMessage(
            GameMessageType.DamageTaken,
            $"{npcName} dealt {damage} damage to you.");
    }

    private static void HandlePlayerRespawnPrompt(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        if (data == "Village")
        {
            RespawnWindow.Instance?.Show();
        }
    }

    private static void HandlePlayerRespawn(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 5)
            return;

        if (!float.TryParse(parts[0], out float x) ||
            !float.TryParse(parts[1], out float y) ||
            !float.TryParse(parts[2], out float z) ||
            !int.TryParse(parts[3], out int currentHp) ||
            !int.TryParse(parts[4], out int maxHp))
        {
            return;
        }

        LocalPlayer localPlayer =
            Object.FindAnyObjectByType<LocalPlayer>();

        if (localPlayer == null)
            return;

        PlayerHealth health =
            localPlayer.GetComponent<PlayerHealth>();

        if (health == null)
            return;

        health.Respawn();

        localPlayer.SetSpawnPosition(
            new Vector3(x, y, z),
            0f);

        RespawnWindow.Instance?.Hide();
    }

}