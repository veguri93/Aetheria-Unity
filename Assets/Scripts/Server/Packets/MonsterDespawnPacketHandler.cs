using System.Text;

public static class MonsterDespawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

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
}