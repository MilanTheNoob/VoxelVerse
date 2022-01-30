public class VersionInfoClass
{
    public VersionTypeEnum VersionType;

    public byte MajorRevision;
    public byte MidRevision;
    public byte MinorRevision;

    public override string ToString() { return VersionType.ToString() + " v" + MajorRevision + "." + MidRevision + "." + MinorRevision; }

    public static bool operator ==(VersionInfoClass one, VersionInfoClass two)
    {
        return (one.VersionType == two.VersionType && one.MajorRevision == two.MajorRevision &&
            one.MidRevision == two.MidRevision && one.MinorRevision == two.MinorRevision);
    }
    public static bool operator !=(VersionInfoClass one, VersionInfoClass two)
    {
        return (one.VersionType != two.VersionType || one.MajorRevision != two.MajorRevision ||
            one.MidRevision != two.MidRevision || one.MinorRevision != two.MinorRevision);
    }

    public override int GetHashCode()
    {
        return VersionType.GetHashCode() ^ (MajorRevision.GetHashCode() << 2) ^
            (MidRevision.GetHashCode() >> 2) ^ (MinorRevision.GetHashCode() >> 1);
    }

    public bool Equals(VersionInfoClass other)
    {
        return (VersionType != other.VersionType || MajorRevision != other.MajorRevision ||
            MidRevision != other.MidRevision || MinorRevision != other.MinorRevision);
    }

    public override bool Equals(object other)
    {
        if (!(other is VersionInfoClass)) return false;

        return Equals((VersionInfoClass)other);
    }

    public void SerializeVersion(ref Packet packet)
    {
        packet.Write((byte)VersionType);

        packet.Write(MajorRevision);
        packet.Write(MidRevision);
        packet.Write(MinorRevision);
    }

    public static VersionInfoClass Deserialize(ref Packet packet)
    {
        return new VersionInfoClass()
        {
            VersionType = (VersionTypeEnum)packet.ReadByte(),
            MajorRevision = packet.ReadByte(),
            MidRevision = packet.ReadByte(),
            MinorRevision = packet.ReadByte()
        };
    }
}


public enum VersionTypeEnum { Prototype = 1, Beta, Alpha, Release }