using System.Text;
using UnityEngine;

public static class PartySnapshotPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        Debug.Log(
            $"[PARTY SNAPSHOT] {data}");
    }
}