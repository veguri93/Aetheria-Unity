using System.Text;

public static class CharacterProgressPacketHandler
{
    public static void Handle(
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
}