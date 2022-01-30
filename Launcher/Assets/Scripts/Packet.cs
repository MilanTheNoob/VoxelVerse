using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
    public Packet(byte[] data)
    {
        SetBytes(data);
        pos = 3;
    }

    public void AddChunk(byte[] nb, int pos = 3) { for (int i = pos; i < nb.Length; i++) { Write(nb[i]); } rb = buffer.ToArray(); }

    public void Send(ClientPackets enumId) 
    {
        byte id = (byte)enumId;

        if (buffer.Count < 4090)
        {
            byte[] sendBuffer = new byte[4096];

            sendBuffer[0] = id;
            sendBuffer[1] = 1;
            sendBuffer[2] = 0;

            buffer.CopyTo(0, sendBuffer, 3, buffer.Count);
            ProgramManager.instance.tcp.SendData(sendBuffer);
        }
        else
        {
            ProgramManager.instance.StartCoroutine(IMultipleSend(enumId));
        }
    }

    public IEnumerator IMultipleSend(ClientPackets enumId)
    {
        byte id = (byte)enumId;

        byte[] firstBuffer = new byte[4096];
        int unwritten_bytes = buffer.Count - 4093;
        int b = 1;

        firstBuffer[0] = id;
        firstBuffer[1] = 0;
        firstBuffer[2] = 0;

        buffer.CopyTo(0, firstBuffer, 3, 4093);
        ProgramManager.instance.tcp.SendData(firstBuffer);

        yield return new WaitForSeconds(0.01f);

        while (unwritten_bytes > 4093)
        {
            byte[] midBuffer = new byte[4096];

            midBuffer[0] = id;
            midBuffer[1] = 0;
            midBuffer[2] = 1;

            buffer.CopyTo(b * 4093, midBuffer, 3, 4093);
            ProgramManager.instance.tcp.SendData(midBuffer);

            yield return new WaitForSeconds(0.01f);

            unwritten_bytes -= 4093;
            b++;
        }

        byte[] endBuffer = new byte[4096];

        endBuffer[0] = id;
        endBuffer[1] = 0;
        endBuffer[2] = 2;

        buffer.CopyTo(buffer.Count - unwritten_bytes, endBuffer, 3, unwritten_bytes);
        ProgramManager.instance.tcp.SendData(endBuffer);
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

    public void Write(Vector2 value) { Write(value.x); Write(value.y); }
    public void Write(Vector3 value) { Write(value.x); Write(value.y); Write(value.z); }
    public void Write(Quaternion value) { Write(value.x); Write(value.y); Write(value.z); Write(value.w); }

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

    public byte[] ReadBytes(int _length = 0) { if (_length == 0) { _length = ReadInt(); } pos += _length; return buffer.GetRange(pos - _length, _length).ToArray(); }
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
