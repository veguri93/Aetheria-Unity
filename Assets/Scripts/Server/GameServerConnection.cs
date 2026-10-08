using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class GameServerConnection : MonoBehaviour
{
    public static GameServerConnection Instance { get; private set; }

    private TcpClient _client;

    private void Awake()
    {
        Instance = this;

    }

    private async void Start()
    {
        try
        {
            _client = new TcpClient();



            await _client.ConnectAsync("127.0.0.1", 7777);



            NetworkStream stream = _client.GetStream();

            ClientPacketReader packetReader = new(stream);

            // Receive WELCOME
            ClientPacket packet = await packetReader.ReadPacketAsync();



            // Continuously receive packets
            while (true)
            {
                ClientPacket incomingPacket =
                    await packetReader.ReadPacketAsync();

                ClientPacketHandler.Handle(incomingPacket);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to connect to GameServer: {ex.Message}");
        }
    }

    public async void SendPlayerMove(
    Vector3 position,
    float rotationY)
{
    try
    {
        if (_client == null || !_client.Connected)
            return;

        string data = string.Join(
            "|",
            position.x,
            position.y,
            position.z,
            rotationY);

        byte[] packet = BuildPacket(
            6,
            data);

        await _client.GetStream().WriteAsync(packet);
    }
    catch (System.Exception ex)
    {
        Debug.LogError(
            $"Failed to send player movement: {ex.Message}");
    }
}
    public async void SendChatMessage(
        ChatType type,
        string message)
    {
        try
        {
            if (_client == null || !_client.Connected)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            string data =
                type.ToString() + "|Local|" + message.Trim();

            byte[] packet = BuildPacket(
                8,
                data);

            await _client.GetStream().WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send chat message: {ex.Message}");
        }
    }

    public async void SendLogin(
    string username,
    string password)
    {
        try
        {
            if (_client == null || !_client.Connected)
            {
                Debug.LogWarning(
                    "Cannot login: not connected to GameServer.");

                return;
            }

            string data =
                username.Trim() + "|" + password;

            byte[] packet = BuildPacket(
                9,
                data);

            await _client.GetStream().WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send login: {ex.Message}");
        }
    }

    public async void SendHello()
    {
        try
        {
            if (_client == null || !_client.Connected)
                return;

            byte[] packet = BuildPacket(
                2,
                "HELLO");

            await _client.GetStream().WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send hello: {ex.Message}");
        }
    }
    public async void SendAttackRequest(int monsterId)
    {
        try
        {
            if (_client == null || !_client.Connected)
                return;

            string data =
                monsterId.ToString();

            byte[] packet =
                BuildPacket(
                    11,
                    data);

            await _client.GetStream().WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send attack request: {ex.Message}");
        }
    }

    public async void SendRespawnRequest(
    string destination)
    {
        try
        {
            if (_client == null || !_client.Connected)
                return;

            if (string.IsNullOrWhiteSpace(destination))
                return;

            byte[] packet =
                BuildPacket(
                    17,
                    destination);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send respawn request: {ex.Message}");
        }
    }
    public async void SendItemActionRequest(
        int objectId,
        EquipmentSlot targetSlot = EquipmentSlot.None)
    {
        try
        {
            if (_client == null || !_client.Connected)
                return;

            string data;

            if (targetSlot == EquipmentSlot.None)
            {
                // Normal double-click / shortcut use.
                data =
                    objectId.ToString();
            }
            else
            {
                // Dragged directly onto an equipment slot.
                data =
                    string.Join(
                        "|",
                        objectId,
                        targetSlot);
            }

            byte[] packet =
                BuildPacket(
                    21,
                    data);

            await _client
                .GetStream()
                .WriteAsync(
                    packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send item action request: {ex.Message}");
        }
    }
    public async void SendUnequipItemRequest(
    int objectId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            string data =
                objectId.ToString();

            byte[] packet =
                BuildPacket(
                    22,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send unequip item request: {ex.Message}");
        }
    }

    public async void SendDropItemRequest(
    int objectId,
    int quantity)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (objectId <= 0 ||
                quantity <= 0)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    objectId,
                    quantity);

            byte[] packet =
                BuildPacket(
                    27,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send drop item request: {ex.Message}");
        }
    }
    public async void SendPickupItemRequest(
    int objectId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (objectId <= 0)
                return;

            string data =
                objectId.ToString();



            byte[] packet =
                BuildPacket(
                    30,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send pickup item request: {ex.Message}");
        }
    }

    public async void SendSkillUseRequest(
    int skillId,
    string targetEntityType,
    int targetObjectId,
    bool forceUse)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (skillId <= 0 ||
                string.IsNullOrWhiteSpace(
                    targetEntityType))
            {
                return;
            }

            bool isSelf =
                targetEntityType.Equals(
                    "Self",
                    System.StringComparison.OrdinalIgnoreCase);

            if (isSelf)
            {
                if (targetObjectId != 0)
                    return;
            }
            else
            {
                if (targetObjectId <= 0)
                    return;
            }

            string data =
                string.Join(
                    "|",
                    skillId,
                    targetEntityType,
                    targetObjectId,
                    forceUse);


            byte[] packet =
                BuildPacket(
                    33,
                    data);

            await _client
                .GetStream()
                .WriteAsync(
                    packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send skill use request: " +
                $"{ex.Message}");
        }
    }
    public async void SendShortcutUpdateRequest(
    int page,
    int slot,
    ShortcutType shortcutType,
    int referenceId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    page,
                    slot,
                    (int)shortcutType,
                    referenceId);


            byte[] packet =
                BuildPacket(
                    35,
                    data);

            await _client
                .GetStream()
                .WriteAsync(
                    packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send shortcut update: " +
                $"{ex.Message}");
        }
    }

    public async void SendMultiSellBuyRequest(
    int npcObjectId,
    int multiSellId,
    int entryId,
    int quantity)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (npcObjectId <= 0 ||
                multiSellId <= 0 ||
                entryId <= 0 ||
                quantity <= 0)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    npcObjectId,
                    multiSellId,
                    entryId,
                    quantity);

            byte[] packet =
                BuildPacket(
                    46,
                    data);

            await _client
                .GetStream()
                .WriteAsync(
                    packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send multisell buy request: " +
                $"{ex.Message}");
        }
    }

    public async void SendMultiSellOpenRequest(
    int npcObjectId,
    int multiSellId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (npcObjectId <= 0 ||
                multiSellId <= 0)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    npcObjectId,
                    multiSellId);

            byte[] packet =
                BuildPacket(
                    44,
                    data);

            await _client
                .GetStream()
                .WriteAsync(
                    packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send multisell open request: " +
                $"{ex.Message}");
        }
    }
    public async void SendNpcInteractRequest(
    int objectId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (objectId <= 0)
                return;

            string data =
                objectId.ToString();

            byte[] packet =
                BuildPacket(
                    42,
                    data);

            await _client
                .GetStream()
                .WriteAsync(
                    packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send NPC interact request: " +
                $"{ex.Message}");
        }
    }
    public async void SendTargetSelect(
       int objectId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (objectId < 0)
                return;

            byte[] packet =
                BuildPacket(
                    47,
                    objectId.ToString());

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send target selection: " +
                $"{ex.Message}");
        }
    }
    public async void SendNpcHtmlPageRequest(
    int npcObjectId,
    int pageId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (npcObjectId <= 0 ||
                pageId < 0)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    npcObjectId,
                    pageId);

            byte[] packet =
                BuildPacket(
                    49,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send NPC HTML page request: " +
                $"{ex.Message}");
        }
    }
    public async void SendTeleportRequest(
    int npcObjectId,
    int destinationId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (npcObjectId <= 0 ||
                destinationId <= 0)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    npcObjectId,
                    destinationId);

            byte[] packet =
                BuildPacket(
                    50,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send teleport request: " +
                $"{ex.Message}");
        }
    }

    public async void SendClassChangeRequest(
    int npcObjectId,
    int classId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (npcObjectId <= 0 ||
                classId < 0)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    npcObjectId,
                    classId);

            byte[] packet =
                BuildPacket(
                    56,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send class change request: " +
                $"{ex.Message}");
        }
    }

    public async void SendSkillLearnRequest(
    int npcObjectId,
    int skillId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (npcObjectId <= 0 ||
                skillId <= 0)
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    npcObjectId,
                    skillId);

            byte[] packet =
                BuildPacket(
                    57,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send skill learn request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildCreateRequest(
    int npcObjectId,
    string guildName)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (npcObjectId <= 0 ||
                string.IsNullOrWhiteSpace(
                    guildName))
            {
                return;
            }

            string data =
                string.Join(
                    "|",
                    npcObjectId,
                    guildName.Trim());

            byte[] packet =
                BuildPacket(
                    58,
                    data);

            await _client
                .GetStream()
                .WriteAsync(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild create request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildInviteRequest(
        int targetPlayerId,
        string message)
    {

        try
        {
            if (_client == null ||
                !_client.Connected)
                return;

            if (targetPlayerId <= 0)
                return;

            string payload =
                $"{targetPlayerId}|{message}";

            byte[] packet =
                BuildPacket(
                    60,
                    payload);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild invite request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildInviteRejectRequest()
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            byte[] packet =
                BuildPacket(
                    62,
                    string.Empty);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild invite reject request: " +
                $"{ex.Message}");
        }
    }

    public async void SendGuildInviteAcceptRequest()
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            byte[] packet =
                BuildPacket(
                    63,
                    string.Empty);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild invite accept request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildWindowRequest()
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            byte[] packet =
                BuildPacket(
                    64,
                    string.Empty);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild window request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildMemberRoleChangeRequest(
    int targetCharacterId,
    int role)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (targetCharacterId <= 0)
                return;

            string payload =
                $"{targetCharacterId}|{role}";

            byte[] packet =
                BuildPacket(
                    66,
                    payload);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild member role change request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildLeaveRequest()
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            byte[] packet =
                BuildPacket(
                    67,
                    string.Empty);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild leave request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildDismissRequest(
    int targetCharacterId)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (targetCharacterId <= 0)
                return;

            byte[] packet =
                BuildPacket(
                    69,
                    targetCharacterId.ToString());

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild dismiss request: " +
                $"{ex.Message}");
        }
    }
    public async void SendGuildTitleChangeRequest(
    string title)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            string payload =
                title.Trim();

            byte[] packet =
                BuildPacket(
                    71,
                    payload);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send guild title change request: " +
                $"{ex.Message}");
        }
    }
    public async void SendPartyInviteRequest(
    int targetPlayerId,
    int lootMode)
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            if (targetPlayerId <= 0)
                return;

            string payload =
                string.Join(
                    "|",
                    targetPlayerId,
                    lootMode);

            byte[] packet =
                BuildPacket(
                    73,
                    payload);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send party invite request: " +
                $"{ex.Message}");
        }
    }
    public async void SendPartyInviteAcceptRequest()
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            byte[] packet =
                BuildPacket(
                    75,
                    string.Empty);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send party invite accept request: " +
                $"{ex.Message}");
        }
    }

    public async void SendPartyInviteRejectRequest()
    {
        try
        {
            if (_client == null ||
                !_client.Connected)
            {
                return;
            }

            byte[] packet =
                BuildPacket(
                    76,
                    string.Empty);

            await _client.GetStream().WriteAsync(
                packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"Failed to send party invite reject request: " +
                $"{ex.Message}");
        }
    }
    private byte[] BuildPacket(ushort packetType, string data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        ushort length = (ushort)(2 + dataBytes.Length);

        byte[] packet = new byte[2 + length];

        System.BitConverter.GetBytes(length).CopyTo(packet, 0);
        System.BitConverter.GetBytes(packetType).CopyTo(packet, 2);

        dataBytes.CopyTo(packet, 4);

        return packet;
    }
}