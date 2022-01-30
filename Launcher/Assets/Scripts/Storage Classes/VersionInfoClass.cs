using System;

public class VersionInfoClass
{
    public VersionTypeEnum VersionType;

    public byte MajorRevision;
    public byte MidRevision;
    public byte MinorRevision;

    public override string ToString() { return VersionType.ToString() + "V" + MajorRevision + "." + MidRevision + "." + MinorRevision; }

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

    public static VersionInfoClass GetFromString(string name)
    {
        const string V = "/Installs/";
        string[] arrZero = name.Split(V.ToCharArray());
        string[] arrOne = arrZero[1].Split('V');
        string[] arrTwo = arrOne[1].Split('.');

        VersionTypeEnum vt = (VersionTypeEnum)Enum.Parse(typeof(VersionTypeEnum), arrOne[0]);

        return new VersionInfoClass()
        {
            VersionType = vt,
            MajorRevision = byte.Parse(arrTwo[0]),
            MidRevision = byte.Parse(arrTwo[1]),
            MinorRevision = byte.Parse(arrTwo[2])
        };
    }
}