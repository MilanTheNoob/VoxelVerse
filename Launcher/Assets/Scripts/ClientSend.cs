using UnityEngine;

public class ClientSend : MonoBehaviour
{
    public static void WelcomeReceived()
    {
        Packet packet = new Packet();
        packet.Write(ProgramManager.instance.myId);
        packet.Send(ClientPackets.welcomeReceived);
    }

    public static void Login(string email, string password)
    {
        Packet packet = new Packet();

        packet.Write(email);
        packet.Write(password);

        packet.Send(ClientPackets.loginRequest);
    }
}