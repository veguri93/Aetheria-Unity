using System.Text;

public static class SkillToggleChangedPacketHandler
{
    public static void Handle(
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
}