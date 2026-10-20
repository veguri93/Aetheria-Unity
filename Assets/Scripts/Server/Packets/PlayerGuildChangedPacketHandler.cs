using System;
using System.Text;

/// <summary>86: session id | URI-escaped authoritative guild name (empty means no guild).</summary>
public static class PlayerGuildChangedPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        string data = Encoding.UTF8.GetString(packet.Data);
        int separator = data.IndexOf('|');
        if (separator < 1 || !int.TryParse(data.Substring(0, separator), out int id)) return;
        string guild;
        try { guild = Uri.UnescapeDataString(data.Substring(separator + 1)); }
        catch (UriFormatException) { return; }
        if (LocalPlayer.Instance && LocalPlayer.Instance.PlayerId == id) LocalPlayer.Instance.SetGuildName(guild);
        else PlayerManager.Instance?.UpdatePlayerGuild(id, guild);
    }
}
