using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

public class GradientMessage : INetMessage, IPacketSerializable
{
    public Color[]? Colors;
    public float[]? Offsets;
    public ulong PlayerId;

    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Info;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(Colors.Length);

        for (int i = 0; i < Colors.Length; i++)
        {
            writer.WriteFloat(Colors[i].R);
            writer.WriteFloat(Colors[i].G);
            writer.WriteFloat(Colors[i].B);
            writer.WriteFloat(Colors[i].A);
        }

        writer.WriteInt(Offsets.Length);

        for (int i = 0; i < Offsets.Length; i++)
            writer.WriteFloat(Offsets[i]);

        writer.WriteULong(PlayerId);
    }

    public void Deserialize(PacketReader reader)
    {
        int colorCount = reader.ReadInt();
        Colors = new Color[colorCount];

        for (int i = 0; i < colorCount; i++)
        {
            float r = reader.ReadFloat();
            float g = reader.ReadFloat();
            float b = reader.ReadFloat();
            float a = reader.ReadFloat();

            Colors[i] = new Color(r, g, b, a);
        }

        int offsetCount = reader.ReadInt();
        Offsets = new float[offsetCount];

        for (int i = 0; i < offsetCount; i++)
            Offsets[i] = reader.ReadFloat();

        PlayerId = reader.ReadULong();
    }

    public Gradient ToGradient()
    {
        return new Gradient
        {
            Colors = Colors,
            Offsets = Offsets
        };
    }
}