using System;
using System.Numerics;
using AAEmu.Commons.Network;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Skills.Effects;
using AAEmu.Game.Models.Game.Skills.Templates;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Game.Units.Movements;
using AAEmu.Game.Models.Game.World;
using AAEmu.Game.Utils;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSMoveUnitPacket : GamePacket
    {
        public CSMoveUnitPacket() : base(0x089, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            var objId = stream.ReadBc();
            var myObjId = Connection.ActiveChar.ObjId;
            var type = (UnitMovementType)stream.ReadByte();
            var moveType = UnitMovement.GetType(type);
            stream.Read(moveType);

            // ---- test Ai ----
            var movementAction = new MovementAction(
                new Point(moveType.X, moveType.Y, moveType.Z, (sbyte)moveType.Rot.X, (sbyte)moveType.Rot.Y, (sbyte)moveType.Rot.Z),
                new Point(0, 0, 0),
                (sbyte)moveType.Rot.Z,
                3,
                UnitMovementType.Actor
                );
            Connection.ActiveChar.VisibleAi.OwnerMoved(movementAction);
            // ---- test Ai ----

            if (objId != myObjId) // Can be mate
            {
                if (moveType is ShipInput shipRequestMoveType)
                {
                    var slave = SlaveManager.Instance.GetActiveSlaveByOwnerObjId(myObjId);
                    if (slave != null)
                    {
                        slave.RequestThrottle = shipRequestMoveType.Throttle;
                        slave.RequestSteering = shipRequestMoveType.Steering;
                    }
                }
                if (moveType is Vehicle VehicleMoveType)
                {
                    //var quatX = VehicleMoveType.RotationX * 0.00003052f;
                    //var quatY = VehicleMoveType.RotationY * 0.00003052f;
                    //var quatZ = VehicleMoveType.RotationZ * 0.00003052f;

                    var quatX = VehicleMoveType.Rot.X;
                    var quatY = VehicleMoveType.Rot.Y;
                    var quatZ = VehicleMoveType.Rot.Z;

                    var quatNorm = quatX * quatX + quatY * quatY + quatZ * quatZ;

                    var quatW = 0.0f;
                    if (quatNorm < 0.99750)
                    {
                        quatW = (float)Math.Sqrt(1.0 - quatNorm);
                    }

                    var quat = new Quaternion(quatX, quatY, quatZ, quatW);

                    var roll = (float)Math.Atan2(2 * (quat.W * quat.X + quat.Y * quat.Z), 1 - 2 * (quat.X * quat.X + quat.Y * quat.Y));
                    var sinp = 2 * (quat.W * quat.Y - quat.Z * quat.X);
                    var pitch = 0.0f;
                    if (Math.Abs(sinp) >= 1)
                    {
                        pitch = (float)MathUtil.CopySign(Math.PI / 2, sinp);
                    }
                    else
                    {
                        pitch = (float)Math.Asin(sinp);
                    }

                    var yaw = (float)Math.Atan2(2 * (quat.W * quat.Z + quat.X * quat.Y), 1 - 2 * (quat.Y * quat.Y + quat.Z * quat.Z));

                    var reverseQuat = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                    var reverseZ = reverseQuat.Y / 0.00003052f;

                    Connection.ActiveChar.SendMessage("Client: " + VehicleMoveType.RotationZ + ". Yaw (deg): " + (yaw * 180 / Math.PI) + ". Reverse: " + reverseZ);
                }

                var mateInfo = MateManager.Instance.GetActiveMateByMateObjId(objId);
                if (mateInfo == null)
                {
                    return;
                }

                RemoveEffects(mateInfo, moveType);
                //mateInfo.SetPosition(moveType.X, moveType.Y, moveType.Z, moveType.RotationX, moveType.RotationY, moveType.RotationZ);
                mateInfo.SetPosition(moveType.X, moveType.Y, moveType.Z, (sbyte)moveType.Rot.X, (sbyte)moveType.Rot.Y, (sbyte)moveType.Rot.Z);
                mateInfo.BroadcastPacket(new SCOneUnitMovementPacket(objId, moveType), myObjId);

                if (mateInfo.Attached1 > 0)
                {

                    var owner = WorldManager.Instance.GetCharacterByObjId(mateInfo.Attached1);
                    if (owner != null)
                    {
                        RemoveEffects(owner, moveType);
                        owner.SetPosition(moveType.X, moveType.Y, moveType.Z, (sbyte)moveType.Rot.X, (sbyte)moveType.Rot.Y, (sbyte)moveType.Rot.Z);
                        owner.BroadcastPacket(new SCOneUnitMovementPacket(owner.ObjId, moveType), false);
                    }
                }

                if (mateInfo.Attached2 > 0)
                {
                    var passenger = WorldManager.Instance.GetCharacterByObjId(mateInfo.Attached2);
                    if (passenger != null)
                    {
                        RemoveEffects(passenger, moveType);
                        passenger.SetPosition(moveType.X, moveType.Y, moveType.Z, (sbyte)moveType.Rot.X, (sbyte)moveType.Rot.Y, (sbyte)moveType.Rot.Z);
                        passenger.BroadcastPacket(new SCOneUnitMovementPacket(passenger.ObjId, moveType), false);
                    }
                }
            }
            else
            {
                RemoveEffects(Connection.ActiveChar, moveType);
                // This will allow you to walk on a boat
                if (moveType is ActorData mType && (mType.actorFlags & 0x20) != 0)
                {
                    Connection
                        .ActiveChar
                        //.SetPosition(mType.GcWorldPos.X + mType.X, mType.GcWorldPos.Y + mType.Y, mType.GcWorldPos.Z + mType.Z, (sbyte)mType.Rot.X, (sbyte)mType.Rot.Y, (sbyte)mType.Rot.Z);
                        .SetPosition(mType.X2 + mType.X, mType.Y2 + mType.Y, mType.Z2 + mType.Z, (sbyte)mType.Rot.X, (sbyte)mType.Rot.Y, (sbyte)mType.Rot.Z);
                }
                else
                {
                    Connection
                        .ActiveChar
                        .SetPosition(moveType.X, moveType.Y, moveType.Z, (sbyte)moveType.Rot.X, (sbyte)moveType.Rot.Y, (sbyte)moveType.Rot.Z);
                }
                Connection.ActiveChar.BroadcastPacket(new SCOneUnitMovementPacket(objId, moveType), false);
            }
        }

        private static void RemoveEffects(BaseUnit unit, UnitMovement unitMovement)
        {
            // снять эффекты при начале движения персонажа
            if (Math.Abs(unitMovement.Velocity.X) > 0 || Math.Abs(unitMovement.Velocity.Y) > 0 || Math.Abs(unitMovement.Velocity.Z) > 0)
            {
                var effects = unit.Effects.GetEffectsByType(typeof(BuffTemplate));
                foreach (var effect in effects)
                {
                    if (((BuffTemplate)effect.Template).RemoveOnMove)
                    {
                        effect.Exit();
                    }
                }

                effects = unit.Effects.GetEffectsByType(typeof(BuffEffect));
                foreach (var effect in effects)
                {
                    if (((BuffEffect)effect.Template).Buff.RemoveOnMove)
                    {
                        effect.Exit();
                    }
                }
            }
        }
    }
}
