using System;
using System.Text;
using UnityEngine;

public static class QuestSnapshotPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        QuestWindow questWindow =
            QuestWindow.Instance;

        if (questWindow == null)
        {
            Debug.LogWarning(
                "QuestWindow instance was not found.");

            return;
        }

        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        questWindow.ClearQuests();

        if (string.IsNullOrWhiteSpace(
                data))
        {
            return;
        }

        string[] entries =
            data.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries);

        foreach (string entry
            in entries)
        {
            string[] parts =
                entry.Split(
                    '|');

            if (parts.Length != 5)
                continue;

            if (!int.TryParse(
                    parts[0],
                    out int questId))
            {
                continue;
            }

            string questName =
                DecodeText(
                    parts[1]);

            string description =
                DecodeText(
                    parts[2]);

            string objective =
                DecodeText(
                    parts[3]);

            if (!bool.TryParse(
                    parts[4],
                    out bool canAbandon))
            {
                canAbandon =
                    false;
            }

            questWindow.AddOrUpdateQuest(
                questId,
                questName,
                description,
                objective,
                canAbandon);
        }
    }

    private static string DecodeText(
        string encodedText)
    {
        try
        {
            byte[] bytes =
                Convert.FromBase64String(
                    encodedText);

            return Encoding.UTF8.GetString(
                bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}