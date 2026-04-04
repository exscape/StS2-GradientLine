using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace GradientLine.GradientLineCode;

public class GradientTypeMessage : INetMessage, IPacketSerializable
{
    public ulong PlayerId;
    public GradientUtil.GradientType GradientType;

    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Info;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(PlayerId);
        writer.WriteUShort((ushort)GradientType);
    }

    public void Deserialize(PacketReader reader)
    {
        PlayerId = reader.ReadULong();
        GradientType = (GradientUtil.GradientType)reader.ReadShort();
    }
}