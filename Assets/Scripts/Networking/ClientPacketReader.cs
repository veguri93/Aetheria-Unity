using System.Net.Sockets;
using System.Threading.Tasks;

public class ClientPacketReader
{
    private readonly NetworkStream _stream;

    public ClientPacketReader(NetworkStream stream)
    {
        _stream = stream;
    }

    public async Task<ClientPacket> ReadPacketAsync()
    {
        byte[] lengthBuffer = new byte[2];

        await ReadExactlyAsync(lengthBuffer, 2);

        ushort length = System.BitConverter.ToUInt16(
            lengthBuffer,
            0);

        byte[] packetBuffer = new byte[length];

        await ReadExactlyAsync(
            packetBuffer,
            length);

        ushort packetType = System.BitConverter.ToUInt16(
            packetBuffer,
            0);

        byte[] data = new byte[length - 2];

        System.Array.Copy(
            packetBuffer,
            2,
            data,
            0,
            length - 2);

        return new ClientPacket(
            packetType,
            data);
    }

    private async Task ReadExactlyAsync(
        byte[] buffer,
        int length)
    {
        int totalRead = 0;

        while (totalRead < length)
        {
            int bytesRead = await _stream.ReadAsync(
                buffer,
                totalRead,
                length - totalRead);

            if (bytesRead == 0)
            {
                throw new System.Exception(
                    "Server disconnected.");
            }

            totalRead += bytesRead;
        }
    }
}