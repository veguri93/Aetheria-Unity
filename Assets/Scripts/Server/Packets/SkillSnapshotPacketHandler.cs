using System.Collections.Generic;
using System.Text;

public static class SkillSnapshotPacketHandler
{
    public static void Handle(
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
    }
}