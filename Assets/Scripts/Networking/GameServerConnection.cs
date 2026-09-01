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

        Debug.Log(
            $"GameServerConnection Awake on: {gameObject.name}");
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
    int objectId)
    {
        try
        {
            if (_client == null || !_client.Connected)
                return;

            string data =
                objectId.ToString();

            byte[] packet =
                BuildPacket(
                    21,
                    data);

            await _client.GetStream().WriteAsync(
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