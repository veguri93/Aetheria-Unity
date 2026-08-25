public class ClientPacket
{
    public ushort Type { get; }
    public byte[] Data { get; }

    public ClientPacket(ushort type, byte[] data)
    {
        Type = type;
        Data = data;
    }
}