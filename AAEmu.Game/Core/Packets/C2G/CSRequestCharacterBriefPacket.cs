﻿using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSRequestCharacterBriefPacket : GamePacket
    {
        public CSRequestCharacterBriefPacket() : base(CSOffsets.CSRequestCharacterBriefPacket, 5)
        {
        }

        public override void Read(PacketStream stream)
        {
            _log.Debug("CSRequestCharacterBriefPacket");
        }
    }
}