using System;

public class AccountData
{
    public Guid GUID;

    public string Email;
    public string Username;
    public string Pass;

    public byte[] Icon;

    public static AccountData Deserialize(ref Packet packet)
    {
        return new AccountData()
        {
            Email = packet.ReadString(),
            Username = packet.ReadString(),
            Pass = packet.ReadString(),
            //Icon = packet.ReadBytes()
        };
    }

    public void Serialize(ref Packet packet)
    {
        packet.Write(Email);
        packet.Write(Username);
        packet.Write(Pass);
        //packet.Write(Icon);
    }
}