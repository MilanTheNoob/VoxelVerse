using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public enum ServerPackets
{
    welcome = 1,
    loginResponse,
    signupResponse,
    launcherDataResponse,
    versionDownload,
    sendCommentResponse,
    requestThreadResponse,
    addPostResponse,
    changeUsernameResponse,
    changeEmailResponse,
    changeIconResponse,
    requestSocialDataResponse
}

public enum ClientPackets
{
    welcomeReceived = 1,
    loginRequest,
    signupRequest,
    launcherDataRequest,
    requestVersionDownload,
    sendComment,
    addView,
    requestThread,
    addPost,
    addReplyPost,
    changeUsername,
    changeEmail,
    changeIcon,
    requestSocialData,
}

public class Packet : IDisposable
{
    public List<byte> buffer = new List<byte>();
    public byte[] rb;
    public int pos = 0;

    public Packet() { }
    public Packet(byte[] data, int clientId)
    {
        SetBytes(data);
        rb = buffer.ToArray();

        pos = 3;
        if (data[1] == 1) { try { Program.packetHandlers[rb[0]](clientId, this); Dispose(); } catch { } }
    }

    public void AddChunk(byte[] nb) { for (int i = 3; i < 4096; i++) { Write(nb[i]); } rb = buffer.ToArray(); }

    public void Send(ServerPackets enumId, int clientId)
    {
        byte id = (byte)enumId;

        if (buffer.Count < 255997)
        {
            byte[] sendBuffer = new byte[256000];

            sendBuffer[0] = id;
            sendBuffer[1] = 1;
            sendBuffer[2] = 0;

            buffer.CopyTo(0, sendBuffer, 3, buffer.Count);
            Program.clients[clientId].tcp.SendData(sendBuffer);
        }
        else
        {
            Task.Factory.StartNew(() =>
            {
                SendMultipleI(enumId, clientId);
            });
        }
    }

    public void SendMultipleI(ServerPackets enumId, int clientId)
    {
        byte id = (byte)enumId;

        byte[] firstBuffer = new byte[256000];
        int unwritten_bytes = buffer.Count - 255997;
        int b = 1;

        firstBuffer[0] = id;
        firstBuffer[1] = 0;
        firstBuffer[2] = 0;

        buffer.CopyTo(0, firstBuffer, 3, 255997);
        Program.clients[clientId].tcp.SendData(firstBuffer);

        Thread.Sleep(25);

        while (unwritten_bytes > 255997)
        {
            byte[] midBuffer = new byte[256000];

            midBuffer[0] = id;
            midBuffer[1] = 0;
            midBuffer[2] = 1;

            buffer.CopyTo(b * 255997, midBuffer, 3, 255997);
            Program.clients[clientId].tcp.SendData(midBuffer);

            Thread.Sleep(25);

            unwritten_bytes -= 255997;
            b++;
        }

        byte[] endBuffer = new byte[256000];

        endBuffer[0] = id;
        endBuffer[1] = 0;
        endBuffer[2] = 2;

        buffer.CopyTo(buffer.Count - unwritten_bytes, endBuffer, 3, unwritten_bytes);
        Program.clients[clientId].tcp.SendData(endBuffer);

        Console.WriteLine((b + 1).ToString());
    }

    #region Functions

    public void SetBytes(byte[] _data) { Write(_data, false); rb = buffer.ToArray(); }
    public void WriteLength() { buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); }
    public void InsertInt(int value) { buffer.InsertRange(0, BitConverter.GetBytes(value)); }
    public byte[] ToArray() { rb = buffer.ToArray(); return rb; }
    public int Length() { return buffer.Count; }
    public int UnreadLength() { return Length() - pos; }

    public void Reset(bool _shouldReset = true)
    {
        if (_shouldReset)
        {
            buffer.Clear();
            rb = null;
            pos = 0;
        }
        else { pos -= 4; }
    }

    #endregion

    #region Write Data

    public void Write(byte value) { buffer.Add(value); }
    public void Write(sbyte value) { Write((byte)value); }

    public void Write(short value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(ushort value) { buffer.AddRange(BitConverter.GetBytes(value)); }

    public void Write(int value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(uint value) { buffer.AddRange(BitConverter.GetBytes(value)); }

    public void Write(float value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(bool value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(string value) { Write(value.Length); buffer.AddRange(Encoding.ASCII.GetBytes(value)); }

    public void Write(Vector2 value) { Write(value.X); Write(value.Y); }
    public void Write(Vector3 value) { Write(value.X); Write(value.Y); Write(value.Z); }
    public void Write(Quaternion value) { Write(value.X); Write(value.Y); Write(value.Z); Write(value.W); }

    public void Write(byte[] value, bool writeValues = true) { if (writeValues) { Write(value.Length); } buffer.AddRange(value); }
    public void Write(Guid guid) { Write(16); Write(guid.ToByteArray()); }

    public void Write(List<Vector3> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    public void Write(List<Quaternion> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    public void Write(List<int> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    public void Write(List<string> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    #endregion
    #region Read Data

    public sbyte ReadSbyte() { pos += 1; return (sbyte)rb[pos - 1]; }
    public byte ReadByte() { pos += 1; return rb[pos - 1]; }

    public short ReadShort() { pos += 2; return BitConverter.ToInt16(rb, pos - 2); }
    public ushort ReadUshort() { pos += 2; return BitConverter.ToUInt16(rb, pos - 2); }

    public int ReadInt() { pos += 4; return BitConverter.ToInt32(rb, pos - 4); }
    public uint ReadUint() { pos += 4; return BitConverter.ToUInt32(rb, pos - 4); }

    public float ReadFloat() { pos += 4; return BitConverter.ToSingle(rb, pos - 4); }
    public bool ReadBool() { pos += 1; return BitConverter.ToBoolean(rb, pos - 1); }

    public string ReadString()
    {
        try
        {
            int _length = ReadInt();
            string _value = Encoding.ASCII.GetString(rb, pos, _length);

            pos += _length;
            return _value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    public Vector2 ReadVector2() { return new Vector2(ReadFloat(), ReadFloat()); }
    public Vector3 ReadVector3() { return new Vector3(ReadFloat(), ReadFloat(), ReadFloat()); }
    public Quaternion ReadQuaternion() { return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat()); }

    public List<Vector3> ReadVector3List()
    {
        List<Vector3> list = new List<Vector3>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadVector3()); }
        return list;
    }
    public List<Quaternion> ReadQuaternionList()
    {
        List<Quaternion> list = new List<Quaternion>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadQuaternion()); }
        return list;
    }
    public List<int> ReadIntList()
    {
        List<int> list = new List<int>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadInt()); }
        return list;
    }
    public List<string> ReadStringList()
    {
        List<string> list = new List<string>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadString()); }
        return list;
    }

    public byte[] ReadBytes() { int _length = ReadInt(); pos += _length; return buffer.GetRange(pos - _length, _length).ToArray(); }
    public Guid ReadGuid() { return new Guid(ReadBytes()); }

    #endregion

    #region GC Code

    private bool disposed = false;

    void Dispose(bool _disposing)
    {
        if (!disposed)
        {
            if (_disposing)
            {
                buffer = null;
                rb = null;
                pos = 0;
            }

            disposed = true;
        }
    }
    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

    #endregion
}
