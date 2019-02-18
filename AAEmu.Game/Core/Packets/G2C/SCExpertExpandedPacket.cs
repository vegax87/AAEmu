using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.G2C
{
    public class SCExpertExpandedPacket : GamePacket
    {
        private readonly byte _next;

        public SCExpertExpandedPacket(byte next) : base(0x1bf, 1)
        {
            _next = next;
        }

        public override PacketStream Write(PacketStream stream)
        {
            stream.Write(_next);
            return stream;
        }
    }
}
