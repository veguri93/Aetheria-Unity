using System.Collections.Generic;
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

            case 20:
                HandleCharacterStats(packet);
                break;

            case 23:
                HandleEquipmentChanged(packet);
                break;

            case 24:
                HandleInventorySnapshot(packet);
                break;

            case 25:
                HandleSystemMessage(packet);
                break;

            case 26:
                HandleCharacterProgress(packet);
                break;

            case 28:
                HandleGroundItemSpawn(packet);
                break;

            case 29:
                HandleGroundItemDespawn(packet);
                break;

            case 31:
                HandlePlayerHealthChanged(packet);
                break;

            case 32:
                HandleMonsterHealthChanged(packet);
                break;

            case 34:
                HandlePlayerManaChanged(packet);
                break;

            case 36:
                HandleShortcutSnapshot(packet);
                break;

            case 37:
                HandleSkillReuseStarted(packet);
                break;

            case 38:
                HandleMonsterMove(packet);
                break;

            case 39:
                HandleSkillToggleChanged(packet);
                break;

            case 40:
                HandleSkillSnapshot(packet);
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

            if (ChatManager.Instance != null)
            {
                SystemMessage message =
                    SystemMessageTable.Get(3);

                if (message != null)
                {
                    string coloredDamage =
                        ChatManager.Instance.ColorText(
                            GameMessageType.DamageDealt,
                            damage.ToString());

                    string text =
                        message.Text
                            .Replace(
                                "$1",
                                coloredDamage)
                            .Replace(
                                "$2",
                                npcName);

                    ChatManager.Instance.AddCombatMessage(
                        GameMessageType.Normal,
                        text);
                }
            }

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
            4,
            System.StringSplitOptions.None);

        if (parts.Length != 4)
            return;

        string chatType = parts[0];
        string chatScope = parts[1];
        string senderName = parts[2];
        string message = parts[3];

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
                senderName,
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

        PlayerProgressUI.Instance?.SetName(
    playerName);
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

        PlayerStatsUI statsUI =
    Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetHealth(
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
        PlayerStatsUI statsUI =
    Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetHealth(
            currentHp,
            maxHp);
        localPlayer.SetSpawnPosition(
            new Vector3(x, y, z),
            0f);

        RespawnWindow.Instance?.Hide();
    }
    private static void HandleCharacterStats(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 7)
            return;

        if (!int.TryParse(parts[0], out int maxHp) ||
            !int.TryParse(parts[1], out int physicalAttack) ||
            !int.TryParse(parts[2], out int physicalDefense) ||
            !int.TryParse(parts[3], out int magicAttack) ||
            !int.TryParse(parts[4], out int magicDefense) ||
            !int.TryParse(parts[5], out int attackSpeed) ||
            !int.TryParse(parts[6], out int castingSpeed))
        {
            return;
        }

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        if (statsUI == null)
            return;

        statsUI.SetStats(
            maxHp,
            physicalAttack,
            physicalDefense,
            magicAttack,
            magicDefense,
            attackSpeed,
            castingSpeed);
    }

    private static void HandleEquipmentChanged(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 4)
            return;

        string action =
            parts[0];

        if (!int.TryParse(
                parts[1],
                out int objectId))
        {
            return;
        }

        if (!int.TryParse(
                parts[2],
                out int itemId))
        {
            return;
        }

        if (!System.Enum.TryParse(
                parts[3],
                out EquipmentSlot slot))
        {
            Debug.LogWarning(
                $"Unknown equipment slot: {parts[3]}");

            return;
        }



        if (ClientInventory.TryGetItem(
                objectId,
                out ClientItemInstance item))
        {
            if (action == "EQUIP")
            {
                item.SetEquippedSlot(
                    slot.ToString());
            }
            else if (action == "UNEQUIP")
            {
                item.SetEquippedSlot(
                    "None");
            }
        }

        if (action == "EQUIP")
        {
            EquipmentUI.Instance?.EquipItem(
                objectId,
                itemId,
                slot);
        }
        else if (action == "UNEQUIP")
        {
            EquipmentUI.Instance?.UnequipItem(
                slot);
        }

        InventoryUI inventoryUI =
            Object.FindAnyObjectByType<InventoryUI>();

        inventoryUI?.Refresh();
    }

    private static void HandleInventorySnapshot(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        ClientInventory.Clear();

        if (string.IsNullOrWhiteSpace(data))
        {
            Debug.Log(
                "Inventory loaded: 0 items.");

            return;
        }

        string[] entries =
            data.Split(
                ';',
                System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string entry in entries)
        {
            string[] parts =
                entry.Split(',');

            if (parts.Length != 4)
                continue;

            if (!int.TryParse(
                    parts[0],
                    out int objectId) ||
                !int.TryParse(
                    parts[1],
                    out int itemId) ||
                !int.TryParse(
                    parts[2],
                    out int quantity))
            {
                continue;
            }

            string equippedSlot =
                parts[3];

            ClientItemInstance item =
                new(
                    objectId,
                    itemId,
                    quantity,
                    equippedSlot);

            ClientInventory.Add(
                item);
        }



        InventoryUI inventoryUI =
    Object.FindAnyObjectByType<InventoryUI>();

        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }
    }
    private static void HandleSystemMessage(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (!int.TryParse(
                parts[0],
                out int messageId))
        {
            return;
        }

        SystemMessage message =
            SystemMessageTable.Get(
                messageId);

        if (message == null)
        {
            Debug.LogWarning(
                $"Unknown system message ID: {messageId}");

            return;
        }

        string text =
            message.Text;

        for (int i = 1;
             i < parts.Length;
             i++)
        {
            text =
                text.Replace(
                    $"${i}",
                    parts[i]);
        }

        ChatManager.Instance?.AddCombatMessage(
            GameMessageType.System,
            text);
    }
    private static void HandleCharacterProgress(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 4)
            return;

        if (!int.TryParse(
                parts[0],
                out int level) ||
            !long.TryParse(
                parts[1],
                out long experience) ||
            !long.TryParse(
                parts[2],
                out long currentLevelExperience) ||
            !long.TryParse(
                parts[3],
                out long nextLevelExperience))
        {
            return;
        }



        PlayerProgressUI.Instance?.SetProgress(
    level,
    experience,
    currentLevelExperience,
    nextLevelExperience);
    }

    private static void HandleGroundItemSpawn(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 6)
        {
            Debug.LogWarning(
                $"[GROUND ITEM] Invalid spawn packet: {data}");

            return;
        }

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !int.TryParse(
                parts[1],
                out int itemId) ||
            !int.TryParse(
                parts[2],
                out int quantity) ||
            !float.TryParse(
                parts[3],
                out float x) ||
            !float.TryParse(
                parts[4],
                out float y) ||
            !float.TryParse(
                parts[5],
                out float z))
        {
            Debug.LogWarning(
                $"[GROUND ITEM] Could not parse spawn packet: {data}");

            return;
        }

        GroundItemManager.Instance?.Spawn(
            objectId,
            itemId,
            quantity,
            new Vector3(
                x,
                y,
                z));
    }
    private static void HandleGroundItemDespawn(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        if (!int.TryParse(
                data,
                out int objectId))
        {
            Debug.LogWarning(
                $"[GROUND ITEM] Invalid despawn packet: {data}");

            return;
        }

        GroundItemManager.Instance?.Despawn(
            objectId);
    }
    private static void HandlePlayerHealthChanged(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(
                parts[0],
                out int currentHp) ||
            !int.TryParse(
                parts[1],
                out int maxHp))
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

        PlayerStatsUI statsUI =
    Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetHealth(
            currentHp,
            maxHp);
    }
    private static void HandleMonsterHealthChanged(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 3)
            return;

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !int.TryParse(
                parts[1],
                out int currentHp) ||
            !int.TryParse(
                parts[2],
                out int maxHp))
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

        monster.SetServerHealth(
            currentHp,
            maxHp);
    }

    private static void HandlePlayerManaChanged(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(
                parts[0],
                out int currentMp) ||
            !int.TryParse(
                parts[1],
                out int maxMp))
        {
            return;
        }

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetMana(
            currentMp,
            maxMp);
    }

    private static void HandleShortcutSnapshot(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        SkillShortcutBarUI shortcutBar =
            Object.FindAnyObjectByType<SkillShortcutBarUI>();

        if (shortcutBar == null)
        {
            Debug.LogWarning(
                "[SHORTCUT] Shortcut bar was not found.");

            return;
        }

        shortcutBar.ClearAllShortcuts();

        if (string.IsNullOrWhiteSpace(data))
            return;

        string[] entries =
            data.Split(
                ';',
                System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string entry in entries)
        {
            string[] parts =
                entry.Split(',');

            if (parts.Length != 4)
                continue;

            if (!int.TryParse(
                    parts[0],
                    out int page) ||
                !int.TryParse(
                    parts[1],
                    out int slot) ||
                !int.TryParse(
                    parts[2],
                    out int typeValue) ||
                !int.TryParse(
                    parts[3],
                    out int referenceId))
            {
                continue;
            }

            if (page != 0)
                continue;

            if (slot < 1 ||
                slot > 12)
            {
                continue;
            }

            ShortcutType shortcutType =
                (ShortcutType)typeValue;

            if (shortcutType != ShortcutType.Skill &&
                shortcutType != ShortcutType.Item)
            {
                continue;
            }

            if (referenceId <= 0)
                continue;

            shortcutBar.LoadShortcut(
                slot,
                shortcutType,
                referenceId);
        }
    }

    private static void HandleSkillReuseStarted(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(
                parts[0],
                out int skillId) ||
            !int.TryParse(
                parts[1],
                out int durationMilliseconds))
        {
            return;
        }

        if (skillId <= 0 ||
            durationMilliseconds <= 0)
        {
            return;
        }

        SkillReuseTracker.StartReuse(
            skillId,
            durationMilliseconds);
    }
    private static void HandleMonsterMove(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 5)
            return;

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !float.TryParse(
                parts[1],
                out float x) ||
            !float.TryParse(
                parts[2],
                out float y) ||
            !float.TryParse(
                parts[3],
                out float z) ||
            !float.TryParse(
                parts[4],
                out float rotation))
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

        monster.SetServerMovement(
            new Vector3(
                x,
                y,
                z),
            rotation);
    }
    private static void HandleSkillToggleChanged(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(
                parts[0],
                out int skillId) ||
            !bool.TryParse(
                parts[1],
                out bool isActive))
        {
            return;
        }

        if (skillId <= 0)
            return;

        SkillToggleTracker.SetActive(
            skillId,
            isActive);
    }

    private static void HandleSkillSnapshot(
    ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        List<int> skillIds =
            new();

        if (!string.IsNullOrWhiteSpace(
                data))
        {
            string[] parts =
                data.Split(
                    '|',
                    System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (!int.TryParse(
                        part,
                        out int skillId))
                {
                    continue;
                }

                if (skillId <= 0)
                    continue;

                skillIds.Add(
                    skillId);
            }
        }

        ClientSkillBook.SetSkills(
            skillIds);

        Debug.Log(
            $"Skills loaded: {skillIds.Count}");
    }
}