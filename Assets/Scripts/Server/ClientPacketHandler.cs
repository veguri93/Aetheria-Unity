using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static class ClientPacketHandler
{

    public static void Handle(ClientPacket packet)
    {
        switch (packet.Type)
        {
            case 3:
                HelloAckPacketHandler.Handle(
                    packet);
                break;

            case 4:
                PlayerEnterPacketHandler.Handle(
                    packet);
                break;

            case 5:
                PlayerLeavePacketHandler.Handle(
                    packet);
                break;

            case 6:
                PlayerMovePacketHandler.Handle(
                    packet);
                break;

            case 7:
                PlayerSpawnPacketHandler.Handle(
                    packet);
                break;

            case 8:
                ChatMessagePacketHandler.Handle(
                    packet);
                break;

            case 10:
                LoginResponsePacketHandler.Handle(
                    packet);
                break;

            case 12:
                MonsterDamagePacketHandler.Handle(
                    packet);
                break;

            case 13:
                MonsterSpawnPacketHandler.Handle(
                    packet);
                break;

            case 14:
                MonsterRespawnPacketHandler.Handle(
                    packet);
                break;

            case 15:
                MonsterDespawnPacketHandler.Handle(
                    packet);
                break;

            case 16:
                PlayerDamagePacketHandler.Handle(
                    packet);
                break;

            case 18:
                PlayerRespawnPromptPacketHandler.Handle(
                    packet);
                break;

            case 19:
                PlayerRespawnPacketHandler.Handle(
                    packet);
                break;

            case 20:
                CharacterStatsPacketHandler.Handle(
                    packet);
                break;

            case 23:
                EquipmentChangedPacketHandler.Handle(
                    packet);
                break;

            case 24:
                InventorySnapshotPacketHandler.Handle(
                    packet);
                break;

            case 25:
                SystemMessagePacketHandler.Handle(
                    packet);
                break;

            case 26:
                CharacterProgressPacketHandler.Handle(
                    packet);
                break;

            case 28:
                GroundItemSpawnPacketHandler.Handle(
                    packet);
                break;

            case 29:
                GroundItemDespawnPacketHandler.Handle(
                    packet);
                break;

            case 31:
                PlayerHealthChangedPacketHandler.Handle(
                    packet);
                break;

            case 32:
                MonsterHealthChangedPacketHandler.Handle(
                    packet);
                break;

            case 34:
                PlayerManaChangedPacketHandler.Handle(
                    packet);
                break;

            case 36:
                ShortcutSnapshotPacketHandler.Handle(
                    packet);
                break;

            case 37:
                SkillReuseStartedPacketHandler.Handle(
                    packet);
                break;

            case 38:
                MonsterMovePacketHandler.Handle(
                    packet);
                break;

            case 39:
                SkillToggleChangedPacketHandler.Handle(
                    packet);
                break;

            case 40:
                SkillSnapshotPacketHandler.Handle(
                    packet);
                break;

            case 41:
                NpcSpawnPacketHandler.Handle(
                    packet);
                break;

            case 43:
                NpcHtmlPacketHandler.Handle(
                    packet);
                break;

            case 45:
                MultiSellListPacketHandler.Handle(
                    packet);
                break;

            case 48:
                NpcDespawnPacketHandler.Handle(
                    packet);
                break;

            case 51:
                PlayerTeleportPacketHandler.Handle(
                    packet);
                break;

            case 52:
                BuffAddedPacketHandler.Handle(
                    packet);
                break;

            case 53:
                BuffRemovedPacketHandler.Handle(
                    packet);
                break;

            case 54:
                NegativeEffectAddedPacketHandler.Handle(
                    packet);
                break;

            case 55:
                NegativeEffectRemovedPacketHandler.Handle(
                    packet);
                break;

            case 59:
                GuildCreateSuccessPacketHandler.Handle(
                    packet);
                break;

            case 61:
                GuildInviteReceivedPacketHandler.Handle(
                    packet);
                break;

            case 65:
                GuildWindowSnapshotPacketHandler.Handle(
                    packet);
                break;

            case 68:
                GuildLeaveSuccessPacketHandler.Handle(
                    packet);
                break;

            case 70:
                GuildDismissSuccessPacketHandler.Handle(
                    packet);
                break;

            case 72:
                PlayerTitleChangedPacketHandler.Handle(
                    packet);
                break;

            case 74:
                PartyInviteReceivedPacketHandler.Handle(
                    packet);
                break;

            case 77:
                PartySnapshotPacketHandler.Handle(
                    packet);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown packet type: {packet.Type}");
                break;
        }

    }
}