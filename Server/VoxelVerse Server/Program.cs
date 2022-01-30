using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    public static int Port = 26951;
    public static TcpListener tcpListener;

    public static List<string> Usernames = new List<string>();
    public static List<string> Emails = new List<string>();

    public static Dictionary<Guid, AccountData> Accounts = new Dictionary<Guid, AccountData>(); // Reference by using GUIDs
    public static Dictionary<VersionInfoClass, byte[]> GameVersions = new Dictionary<VersionInfoClass, byte[]>();

    public static List<ArticleData> DevlogsArticles = new List<ArticleData>();
    public static List<ArticleData> NewsArticles = new List<ArticleData>();
    public static List<ArticleData> EventsArticles = new List<ArticleData>();

    public static List<PostData> GameDiscussionThread = new List<PostData>();
    public static List<PostData> MultiplayerThread = new List<PostData>();
    public static List<PostData> GuidesThread = new List<PostData>();
    public static List<PostData> ShowcaseThread = new List<PostData>(); 
    public static List<PostData> FanmadeThread = new List<PostData>();
    public static List<PostData> MerchandiseThread = new List<PostData>();
    public static List<PostData> ModsShowcaseThread = new List<PostData>();
    public static List<PostData> ModsHelpThread = new List<PostData>();
    public static List<PostData> ModsIdeasThread = new List<PostData>();
    public static List<PostData> ModsDiscussionThread = new List<PostData>();
    public static List<PostData> HelpThread = new List<PostData>();
    public static List<PostData> BugsThread = new List<PostData>();
    public static List<PostData> SuggestionsThread = new List<PostData>();
    public static List<PostData> FeedbackThread = new List<PostData>();
    public static List<PostData> IntroductionsThread = new List<PostData>();
    public static List<PostData> GeneralThread = new List<PostData>();
    public static List<PostData> OpinionThread = new List<PostData>();

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public static List<int> CachedClients = new List<int>();

    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<byte, PacketHandler> packetHandlers;

    public static Dictionary<byte, Packet> Recving = new Dictionary<byte, Packet>();

    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);
    static EventHandler _handler;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello world\nServer is starting...\n");

        _handler += new EventHandler(Handler);
        SetConsoleCtrlHandler(_handler, true);

        Console.WriteLine("Added quiting handler");

        packetHandlers = new Dictionary<byte, PacketHandler>()
        {
            { (byte)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (byte)ClientPackets.loginRequest, ServerHandle.Login },
            { (byte)ClientPackets.signupRequest, ServerHandle.Signup },
            { (byte)ClientPackets.launcherDataRequest, ServerHandle.LauncherData },
            { (byte)ClientPackets.requestVersionDownload, ServerHandle.DownloadVersion },
            { (byte)ClientPackets.sendComment, ServerHandle.AddComment },
            { (byte)ClientPackets.addView, ServerHandle.AddView },
            { (byte)ClientPackets.requestThread, ServerHandle.RequestThread },
            { (byte)ClientPackets.addPost, ServerHandle.AddPost },
            { (byte)ClientPackets.addReplyPost, ServerHandle.AddReplyPost },
            { (byte)ClientPackets.changeUsername, ServerHandle.ChangeUsername },
            { (byte)ClientPackets.changeEmail, ServerHandle.ChangeEmail },
            { (byte)ClientPackets.changeIcon, ServerHandle.ChangeIcon }
        };

        Console.WriteLine("Setup packet handlers");

        GameVersions.Clear();
        LoadAccounts();

        if (DoesDirectoryExist("Installations"))
        {
            StreamReader InfoFileReader = new StreamReader(Directory.GetCurrentDirectory() + "/Installations/Installations.txt");

            while (!InfoFileReader.EndOfStream)
            {
                string[] versionValues = InfoFileReader.ReadLine().Split(",");
                GameVersions.Add(new VersionInfoClass()
                {
                    VersionType = (VersionTypeEnum)Enum.Parse(typeof(VersionTypeEnum), versionValues[0]),
                    MajorRevision = byte.Parse(versionValues[1]),
                    MidRevision = byte.Parse(versionValues[2]),
                    MinorRevision = byte.Parse(versionValues[3])
                }, File.ReadAllBytes(Directory.GetCurrentDirectory() + "/Installations/" + versionValues[4]));
            }

            InfoFileReader.Close();
        }

        Console.WriteLine("Loaded installations");

        LoadArticleContainer(ref DevlogsArticles, "Devlogs", "dev", false);
        LoadArticleContainer(ref NewsArticles, "News", "news", false);
        LoadArticleContainer(ref EventsArticles, "Events", "event", false);

        Console.WriteLine("Loaded articles");

        LoadThread(ref GameDiscussionThread, "GameDiscussion");
        LoadThread(ref MultiplayerThread, "Multiplayer");
        LoadThread(ref GuidesThread, "Guides");
        LoadThread(ref ShowcaseThread, "Showcase");
        LoadThread(ref FanmadeThread, "Fanmade");
        LoadThread(ref MerchandiseThread, "Merchandise");
        LoadThread(ref ModsShowcaseThread, "ModsShowcase");
        LoadThread(ref ModsHelpThread, "ModsHelp");
        LoadThread(ref ModsIdeasThread, "ModsIdeas");
        LoadThread(ref ModsDiscussionThread, "ModsDiscussion");
        LoadThread(ref HelpThread, "Help");
        LoadThread(ref BugsThread, "Bugs");
        LoadThread(ref SuggestionsThread, "Suggestions");
        LoadThread(ref FeedbackThread, "Feedback");
        LoadThread(ref IntroductionsThread, "Introductions");
        LoadThread(ref GeneralThread, "General");
        LoadThread(ref OpinionThread, "Opinion");

        Console.WriteLine("Loaded threads");

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        Console.WriteLine("Started tcp listener\n\nServer setup finished, ready to accept connections");

        Thread mainThread = new Thread(new ThreadStart(MainThread));
        mainThread.Start();
    }

    static bool Handler(CtrlType sig)
    {
        List<byte> buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes(Accounts.Count));

        for (int i = 0; i < Accounts.Count; i++)
        {
            AccountData ac = Accounts.ElementAt(i).Value;
            buffer.AddRange(ac.GUID.ToByteArray());

            buffer.AddRange(BitConverter.GetBytes(ac.Email.Length));
            buffer.AddRange(Encoding.ASCII.GetBytes(ac.Email));

            buffer.AddRange(BitConverter.GetBytes(ac.Username.Length));
            buffer.AddRange(Encoding.ASCII.GetBytes(ac.Username));

            buffer.AddRange(BitConverter.GetBytes(ac.Pass.Length));
            buffer.AddRange(Encoding.ASCII.GetBytes(ac.Pass));

            buffer.AddRange(BitConverter.GetBytes(ac.Icon.Length));
            buffer.AddRange(ac.Icon);
        }

        File.WriteAllBytes(Directory.GetCurrentDirectory() + "/Accounts.dat", buffer.ToArray());

        SaveArticleComments(ref DevlogsArticles, "Devlogs", "dev");
        SaveArticleComments(ref NewsArticles, "News", "news");
        SaveArticleComments(ref DevlogsArticles, "Devlogs", "dev");

        SaveThread(ref GameDiscussionThread, "GameDiscussion");
        SaveThread(ref MultiplayerThread, "Multiplayer");
        SaveThread(ref GuidesThread, "Guides");
        SaveThread(ref ShowcaseThread, "Showcase");
        SaveThread(ref FanmadeThread, "Fanmade");
        SaveThread(ref MerchandiseThread, "Merchandise");
        SaveThread(ref ModsShowcaseThread, "ModsShowcase");
        SaveThread(ref ModsHelpThread, "ModsHelp");
        SaveThread(ref ModsIdeasThread, "ModsIdeas");
        SaveThread(ref ModsDiscussionThread, "ModsDiscussion");
        SaveThread(ref HelpThread, "Help");
        SaveThread(ref BugsThread, "Bugs");
        SaveThread(ref SuggestionsThread, "Suggestions");
        SaveThread(ref FeedbackThread, "Feedback");
        SaveThread(ref IntroductionsThread, "Introductions");
        SaveThread(ref GeneralThread, "General");
        SaveThread(ref OpinionThread, "Opinion");

        return false;
    }

    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient clientSocket = tcpListener.EndAcceptTcpClient(result);
        int clientId = 0;

        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Console.WriteLine($"Incoming connection from {clientSocket.Client.RemoteEndPoint}...");

        if (CachedClients.Count > 0) { clientId = CachedClients[0]; CachedClients.RemoveAt(0); }
        else { clientId = clients.Count; clients.Add(clientId, new Client()); }

        clients[clientId].Connect(clientSocket, clientId);
    }

    static void MainThread()
    {
        DateTime _nextLoop = DateTime.Now;

        while (true)
        {
            while (_nextLoop < DateTime.Now)
            {
                ThreadManager.UpdateMain();

                _nextLoop = _nextLoop.AddMilliseconds(33.3333333333);
                if (_nextLoop > DateTime.Now) Thread.Sleep(_nextLoop - DateTime.Now);
            }
        }
    }

    static void UpdateData()
    {
        GameVersions.Clear();

        if (DoesDirectoryExist("Installations"))
        {
            StreamReader InfoFileReader = new StreamReader(Directory.GetCurrentDirectory() + "/Installations/Installations.txt");

            while (!InfoFileReader.EndOfStream)
            {
                string[] versionValues = InfoFileReader.ReadLine().Split(",");
                GameVersions.Add(new VersionInfoClass()
                {
                    VersionType = (VersionTypeEnum)Enum.Parse(typeof(VersionTypeEnum), versionValues[0]),
                    MajorRevision = byte.Parse(versionValues[1]),
                    MidRevision = byte.Parse(versionValues[2]),
                    MinorRevision = byte.Parse(versionValues[3])
                }, File.ReadAllBytes(Directory.GetCurrentDirectory() + "/Installations/" + versionValues[4]));
            }

            InfoFileReader.Close();
        }

        LoadArticleContainer(ref DevlogsArticles, "Devlogs", "dev", false);
        LoadArticleContainer(ref NewsArticles, "News", "news", false);
        LoadArticleContainer(ref EventsArticles, "Events", "event", false);
    }

    static void LoadAccounts()
    {
        string saveDirectory = Directory.GetCurrentDirectory() + "/Accounts.dat";

        if (File.Exists(saveDirectory))
        {
            byte[] data = File.ReadAllBytes(saveDirectory);
            List<byte> listData = data.ToList();
            int pos = 0;

            int saveLength = BitConverter.ToInt32(data, pos); pos += 4;
            for (int i = 0; i < saveLength; i++)
            {
                AccountData accData = new AccountData();

                byte[] guidDat = listData.GetRange(pos, 16).ToArray(); pos += 16;
                accData.GUID = new Guid(guidDat);

                int emailLength = BitConverter.ToInt32(data, pos); pos += 4;
                accData.Email = Encoding.ASCII.GetString(data, pos, emailLength); pos += emailLength;

                int usernameLength = BitConverter.ToInt32(data, pos); pos += 4;
                accData.Username = Encoding.ASCII.GetString(data, pos, usernameLength); pos += usernameLength;

                int passLength = BitConverter.ToInt32(data, pos); pos += 4;
                accData.Pass = Encoding.ASCII.GetString(data, pos, passLength); pos += passLength;

                int iconLength = BitConverter.ToInt32(data, pos); pos += 4;
                accData.Icon = data.ToList().GetRange(pos, iconLength).ToArray(); pos += iconLength;

                Accounts.Add(accData.GUID, accData);

                Emails.Add(accData.Email);
                Usernames.Add(accData.Username);
            }
        }
    }

    static bool DoesDirectoryExist(string dir)
    {
        string fullDir = Directory.GetCurrentDirectory() + "/" + dir;

        if (!Directory.Exists(fullDir))
        {
            Directory.CreateDirectory(fullDir);
            File.Create(fullDir + "/" + dir + ".txt");

            return false;
        }
        else
        {
            return true;
        }
    }

    static void LoadArticleContainer(ref List<ArticleData> articlesContainer, string name, string smallName, bool loadComments = true)
    {
        articlesContainer.Clear();

        if (DoesDirectoryExist(name))
        {
            StreamReader InfoFileReader = new StreamReader(Directory.GetCurrentDirectory() + "/" + name + "/" + name + ".txt");
            int newsCount = int.Parse(InfoFileReader.ReadLine());

            InfoFileReader.Close();

            for (int i = 1; i < newsCount + 1; i++)
            {
                string newsLoc = Directory.GetCurrentDirectory() + "/" + name + "/" + smallName + i + "/";

                articlesContainer.Add(new ArticleData()
                {
                    Title = File.ReadAllText(newsLoc + "title.txt"),
                    Publisher = File.ReadAllText(newsLoc + "author.txt"),
                    Date = File.ReadAllText(newsLoc + "date.txt"),

                    Content = File.ReadAllText(newsLoc + "content.txt")
                });

                if (File.Exists(newsLoc + "comments.dat") && loadComments)
                {
                    Packet serPacket = new Packet();
                    serPacket.pos = 0;
                    serPacket.rb = File.ReadAllBytes(newsLoc + "comments.dat");

                    int commentsCount = serPacket.ReadInt();
                    for (int j = 0; j < commentsCount; j++)
                    {
                        articlesContainer[i - 1].Comments.Add(new CommentData()
                        {
                            Date = serPacket.ReadString(),
                            Publisher = serPacket.ReadString(),
                            Content = serPacket.ReadString()
                        });
                    }
                }
            }
        }
    }

    static void SaveThread(ref List<PostData> threadList, string name)
    {
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Forums/")) Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Forums/");

        string articleLoc = Directory.GetCurrentDirectory() + "/Forums/" + name + ".dat";
        Packet packet = new Packet();

        packet.Write(threadList.Count);
        for (int i = 0; i < threadList.Count; i++)
        {
            threadList[i].Serialize(ref packet, true);
        }

        File.WriteAllBytes(articleLoc, packet.buffer.ToArray());
    }

    static void LoadThread(ref List<PostData> threadList, string name)
    {
        string articleLoc = Directory.GetCurrentDirectory() + "/Forums/" + name + ".dat";
        if (File.Exists(articleLoc))
        {
            Packet packet = new Packet();
            packet.pos = 0;
            packet.rb = File.ReadAllBytes(articleLoc);

            int itemLength = packet.ReadInt();
            threadList.Clear();

            for (int i = 0; i < itemLength; i++)
            {
                threadList.Add(PostData.Deserialize(ref packet));
            }
        }
    }

    static void SaveArticleComments(ref List<ArticleData> articlesContainer, string name, string smallName)
    {
        string articleLoc = Directory.GetCurrentDirectory() + "/" + name + "/";

        for (int i = 0; i < articlesContainer.Count; i++)
        {
            Packet serPacket = new Packet();
            serPacket.Write(articlesContainer[i].Comments.Count);

            for (int j = 0; j < articlesContainer[i].Comments.Count; j++)
            {
                serPacket.Write(articlesContainer[i].Comments[j].Date);
                serPacket.Write(articlesContainer[i].Comments[j].Publisher);

                serPacket.Write(articlesContainer[i].Comments[j].Content);
            }

            File.WriteAllBytes(articleLoc + smallName + (i + 1) + "/comments.dat", serPacket.buffer.ToArray());
        }
    }
}

public enum ActiveCategoryEnum { Devlog, News, Event }
public enum CtrlType { CTRL_C_EVENT = 0, CTRL_BREAK_EVENT = 1, CTRL_CLOSE_EVENT = 2, CTRL_LOGOFF_EVENT = 5, CTRL_SHUTDOWN_EVENT = 6 }
public enum ForumTypeEnum { GameDiscussion, Multiplayer, Guides, Showcase, Fanmade, Merchandise, ModsShowcase, ModsHelp, ModsIdeas, ModsDiscussion, 
    Help, Bugs, Suggestions, Feedback, Introductions, General, Opinion }