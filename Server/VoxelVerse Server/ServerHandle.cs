using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class ServerHandle
{
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        Console.WriteLine($"{Program.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now user {fromClient}.");
    }

    public static void Login(int fromClient, Packet packet)
    {
        string email = packet.ReadString();
        string password = packet.ReadString();

        Packet sendPacket = new Packet();
        if (Program.clients[fromClient].signinAttempts <= 5)
        {
            Program.clients[fromClient].signinAttempts++;

            if (Program.Emails.Contains(email))
            {
                AccountData ac = Program.Accounts.ElementAt(Program.Emails.IndexOf(email)).Value;

                if (ac.Pass == password)
                {
                    sendPacket.Write(true);

                    sendPacket.Write(ac.Email);
                    sendPacket.Write(ac.Username);
                    sendPacket.Write(ac.Icon);

                    sendPacket.Write(ac.GUID.ToByteArray());

                    Program.clients[fromClient].accountData = ac.GUID;
                }
                else
                {
                    sendPacket.Write(false);
                    sendPacket.Write("You did not provide the correct details");
                }
            }
            else
            {
                sendPacket.Write(false);
                sendPacket.Write("You did not provide the correct details, fuck, " + email);
            }
        }
        else
        {
            sendPacket.Write(false);
            sendPacket.Write("You took more than five attempts to login, you can no longer attempt to login/signup");
        }

        sendPacket.Send(ServerPackets.loginResponse, fromClient);
    }

    public static void Signup(int fromClient, Packet packet)
    {
        string email = packet.ReadString();
        string username = packet.ReadString();
        string password = packet.ReadString();

        Packet sendPacket = new Packet();

        if (!Program.Usernames.Contains(username))
        {
            Guid guid = Guid.NewGuid();

            Program.Accounts.Add(guid, new AccountData()
            {
                GUID = guid,
                Email = email,
                Username = username,
                Pass = password,
                Icon = packet.ReadBytes()
            });

            Program.clients[fromClient].accountData = guid;
            Program.Usernames.Add(username);
            Program.Emails.Add(email);

            sendPacket.Write(true);
        }
        else sendPacket.Write(false);
        sendPacket.Send(ServerPackets.signupResponse, fromClient);
    }

    public static void LauncherData(int fromClient, Packet packet)
    {
        Packet sendPacket = new Packet();

        // Downloadable Versions
        sendPacket.Write(Program.GameVersions.Count);
        for (int i = 0; i < Program.GameVersions.Count; i++) Program.GameVersions.ElementAt(i).Key.SerializeVersion(ref sendPacket);

        // Devlogs
        sendPacket.Write(Program.DevlogsArticles.Count);
        for (int i = 0; i < Program.DevlogsArticles.Count; i++) { Program.DevlogsArticles[i].Serialize(ref sendPacket); }

        // News
        sendPacket.Write(Program.NewsArticles.Count);
        for (int i = 0; i < Program.NewsArticles.Count; i++) { Program.NewsArticles[i].Serialize(ref sendPacket); }

        // Events
        sendPacket.Write(Program.EventsArticles.Count);
        for (int i = 0; i < Program.EventsArticles.Count; i++) { Program.EventsArticles[i].Serialize(ref sendPacket); }

        Console.WriteLine(Program.DevlogsArticles.Count + ", " + Program.NewsArticles.Count);
        sendPacket.Send(ServerPackets.launcherDataResponse, fromClient);
    }

    public static void DownloadVersion(int fromClient, Packet packet)
    {
        if (Program.clients[fromClient].downloadingData) return;
        Program.clients[fromClient].downloadingData = true;

        VersionInfoClass downloadVer = VersionInfoClass.Deserialize(ref packet);
        string downloadId = downloadVer.ToString();
        byte[] gameDownloadData = null;

        for (int i = 0; i < Program.GameVersions.Count; i++)
        {
            if (Program.GameVersions.ElementAt(i).Key.ToString() == downloadId) gameDownloadData = Program.GameVersions.ElementAt(i).Value;
        }

        if (gameDownloadData != null)
        {
            Packet sendPacket = new Packet();

            downloadVer.SerializeVersion(ref sendPacket);
            sendPacket.Write(gameDownloadData.Length);

            for (int i = 0; i < gameDownloadData.Length; i++) sendPacket.Write(gameDownloadData[i]);
            sendPacket.Send(ServerPackets.versionDownload, fromClient);
        }

        Task.Factory.StartNew(() =>
        {
            Thread.Sleep(1000);
            Program.clients[fromClient].downloadingData = false;
        });
    }

    public static void AddComment(int fromClient, Packet packet)
    {
        try
        {
            ActiveCategoryEnum category = (ActiveCategoryEnum)packet.ReadByte();
            int pos = packet.ReadInt();

            ref List<ArticleData> articleList = ref Program.DevlogsArticles;

            switch (category)
            {
                case ActiveCategoryEnum.News: articleList = ref Program.NewsArticles; break;
                case ActiveCategoryEnum.Event: articleList = ref Program.EventsArticles; break;
                default: articleList = ref Program.DevlogsArticles; break;
            }

            if (articleList.Count > pos)
            {
                articleList[pos].Comments.Add(new CommentData()
                {
                    Publisher = packet.ReadString(),
                    Date = packet.ReadString(),
                    Content = packet.ReadString()
                });
            }
        }
        catch { }
    }

    public static void AddView(int fromClient, Packet packet)
    {
        ForumTypeEnum requestedForum = (ForumTypeEnum)packet.ReadInt();

        switch (requestedForum)
        {
            case ForumTypeEnum.GameDiscussion: AddView(ref Program.GameDiscussionThread, ref packet); break;
            case ForumTypeEnum.Multiplayer: AddView(ref Program.MultiplayerThread, ref packet); break;
            case ForumTypeEnum.Guides: AddView(ref Program.GuidesThread, ref packet); break;
            case ForumTypeEnum.Showcase: AddView(ref Program.ShowcaseThread, ref packet); break;
            case ForumTypeEnum.Fanmade: AddView(ref Program.FanmadeThread, ref packet); break;
            case ForumTypeEnum.Merchandise: AddView(ref Program.MerchandiseThread, ref packet); break;
            case ForumTypeEnum.ModsShowcase: AddView(ref Program.ModsShowcaseThread, ref packet); break;
            case ForumTypeEnum.ModsHelp: AddView(ref Program.ModsHelpThread, ref packet); break;
            case ForumTypeEnum.ModsIdeas: AddView(ref Program.ModsIdeasThread, ref packet); break;
            case ForumTypeEnum.ModsDiscussion: AddView(ref Program.ModsDiscussionThread, ref packet); break;
            case ForumTypeEnum.Help: AddView(ref Program.HelpThread, ref packet); break;
            case ForumTypeEnum.Bugs: AddView(ref Program.BugsThread, ref packet); break;
            case ForumTypeEnum.Suggestions: AddView(ref Program.SuggestionsThread, ref packet); break;
            case ForumTypeEnum.Feedback: AddView(ref Program.FeedbackThread, ref packet); break;
            case ForumTypeEnum.Introductions: AddView(ref Program.IntroductionsThread, ref packet); break;
            case ForumTypeEnum.General: AddView(ref Program.GeneralThread, ref packet); break;
            case ForumTypeEnum.Opinion: AddView(ref Program.OpinionThread, ref packet); break;
        }
    }

    static void AddView(ref List<PostData> threadList, ref Packet packet)
    {
        int postId = packet.ReadInt();
        if (threadList.Count < postId || postId < 0) return;

        threadList[postId].Views++;
    }

    public static void RequestThread(int fromClient, Packet packet)
    {
        ForumTypeEnum requestedForum = (ForumTypeEnum)packet.ReadByte();
        Console.WriteLine(requestedForum.ToString());

        Packet sendPacket = new Packet();

        switch (requestedForum)
        {
            case ForumTypeEnum.GameDiscussion: SerializeThread(ref Program.GameDiscussionThread, ref sendPacket); break;
            case ForumTypeEnum.Multiplayer: SerializeThread(ref Program.MultiplayerThread, ref sendPacket); break;
            case ForumTypeEnum.Guides: SerializeThread(ref Program.GuidesThread, ref sendPacket); break;
            case ForumTypeEnum.Showcase: SerializeThread(ref Program.ShowcaseThread, ref sendPacket); break;
            case ForumTypeEnum.Fanmade: SerializeThread(ref Program.FanmadeThread, ref sendPacket); break;
            case ForumTypeEnum.Merchandise: SerializeThread(ref Program.MerchandiseThread, ref sendPacket); break;
            case ForumTypeEnum.ModsShowcase: SerializeThread(ref Program.ModsShowcaseThread, ref sendPacket); break;
            case ForumTypeEnum.ModsHelp: SerializeThread(ref Program.ModsHelpThread, ref sendPacket); break;
            case ForumTypeEnum.ModsIdeas: SerializeThread(ref Program.ModsIdeasThread, ref sendPacket); break;
            case ForumTypeEnum.ModsDiscussion: SerializeThread(ref Program.ModsDiscussionThread, ref sendPacket); break;
            case ForumTypeEnum.Help: SerializeThread(ref Program.HelpThread, ref sendPacket); break;
            case ForumTypeEnum.Bugs: SerializeThread(ref Program.BugsThread, ref sendPacket); break;
            case ForumTypeEnum.Suggestions: SerializeThread(ref Program.SuggestionsThread, ref sendPacket); break;
            case ForumTypeEnum.Feedback: SerializeThread(ref Program.FeedbackThread, ref sendPacket); break;
            case ForumTypeEnum.Introductions: SerializeThread(ref Program.IntroductionsThread, ref sendPacket); break;
            case ForumTypeEnum.General: SerializeThread(ref Program.GeneralThread, ref sendPacket); break;
            case ForumTypeEnum.Opinion: SerializeThread(ref Program.OpinionThread, ref sendPacket); break;
        }

        sendPacket.Send(ServerPackets.requestThreadResponse, fromClient);
    }

    static void SerializeThread(ref List<PostData> threadList, ref Packet packet)
    {
        packet.Write(threadList.Count);
        for (int i = 0; i < threadList.Count; i++)
        {
            threadList[i].Serialize(ref packet, false);
        }
    }

    public static void AddPost(int fromClient, Packet packet)
    {
        ForumTypeEnum requestedForum = (ForumTypeEnum)packet.ReadInt();

        switch (requestedForum)
        {
            case ForumTypeEnum.GameDiscussion: AddToThread(ref Program.GameDiscussionThread, ref packet); break;
            case ForumTypeEnum.Multiplayer: AddToThread(ref Program.MultiplayerThread, ref packet); break;
            case ForumTypeEnum.Guides: AddToThread(ref Program.GuidesThread, ref packet); break;
            case ForumTypeEnum.Showcase: AddToThread(ref Program.ShowcaseThread, ref packet); break;
            case ForumTypeEnum.Fanmade: AddToThread(ref Program.FanmadeThread, ref packet); break;
            case ForumTypeEnum.Merchandise: AddToThread(ref Program.MerchandiseThread, ref packet); break;
            case ForumTypeEnum.ModsShowcase: AddToThread(ref Program.ModsShowcaseThread, ref packet); break;
            case ForumTypeEnum.ModsHelp: AddToThread(ref Program.ModsHelpThread, ref packet); break;
            case ForumTypeEnum.ModsIdeas: AddToThread(ref Program.ModsIdeasThread, ref packet); break;
            case ForumTypeEnum.ModsDiscussion: AddToThread(ref Program.ModsDiscussionThread, ref packet); break;
            case ForumTypeEnum.Help: AddToThread(ref Program.HelpThread, ref packet); break;
            case ForumTypeEnum.Bugs: AddToThread(ref Program.BugsThread, ref packet); break;
            case ForumTypeEnum.Suggestions: AddToThread(ref Program.SuggestionsThread, ref packet); break;
            case ForumTypeEnum.Feedback: AddToThread(ref Program.FeedbackThread, ref packet); break;
            case ForumTypeEnum.Introductions: AddToThread(ref Program.IntroductionsThread, ref packet); break;
            case ForumTypeEnum.General: AddToThread(ref Program.GeneralThread, ref packet); break;
            case ForumTypeEnum.Opinion: AddToThread(ref Program.OpinionThread, ref packet); break;
        }
    }

    static void AddToThread(ref List<PostData> threadList, ref Packet packet)
    {
        PostData postData = new PostData();
        postData.Title = packet.ReadString();
        postData.Views = 1;

        postData.Items = new List<PostData.PostItemData> { new PostData.PostItemData
        {
            Content = packet.ReadString(),
            Publisher = packet.ReadGuid(),
            Date = packet.ReadString()
        }
        };

        threadList.Add(postData);
    }

    public static void AddReplyPost(int fromClient, Packet packet)
    {
        ForumTypeEnum requestedForum = (ForumTypeEnum)packet.ReadInt();

        switch (requestedForum)
        {
            case ForumTypeEnum.GameDiscussion: AddToPost(ref Program.GameDiscussionThread, ref packet); break;
            case ForumTypeEnum.Multiplayer: AddToPost(ref Program.MultiplayerThread, ref packet); break;
            case ForumTypeEnum.Guides: AddToPost(ref Program.GuidesThread, ref packet); break;
            case ForumTypeEnum.Showcase: AddToPost(ref Program.ShowcaseThread, ref packet); break;
            case ForumTypeEnum.Fanmade: AddToPost(ref Program.FanmadeThread, ref packet); break;
            case ForumTypeEnum.Merchandise: AddToPost(ref Program.MerchandiseThread, ref packet); break;
            case ForumTypeEnum.ModsShowcase: AddToPost(ref Program.ModsShowcaseThread, ref packet); break;
            case ForumTypeEnum.ModsHelp: AddToPost(ref Program.ModsHelpThread, ref packet); break;
            case ForumTypeEnum.ModsIdeas: AddToPost(ref Program.ModsIdeasThread, ref packet); break;
            case ForumTypeEnum.ModsDiscussion: AddToPost(ref Program.ModsDiscussionThread, ref packet); break;
            case ForumTypeEnum.Help: AddToPost(ref Program.HelpThread, ref packet); break;
            case ForumTypeEnum.Bugs: AddToPost(ref Program.BugsThread, ref packet); break;
            case ForumTypeEnum.Suggestions: AddToPost(ref Program.SuggestionsThread, ref packet); break;
            case ForumTypeEnum.Feedback: AddToPost(ref Program.FeedbackThread, ref packet); break;
            case ForumTypeEnum.Introductions: AddToPost(ref Program.IntroductionsThread, ref packet); break;
            case ForumTypeEnum.General: AddToPost(ref Program.GeneralThread, ref packet); break;
            case ForumTypeEnum.Opinion: AddToPost(ref Program.OpinionThread, ref packet); break;
        }
    }

    static void AddToPost(ref List<PostData> threadList, ref Packet packet)
    {
        int postId = packet.ReadInt();
        if (threadList.Count < postId || postId < 0) return;

        threadList[postId].Items.Add(new PostData.PostItemData()
        { 
            Content = packet.ReadString(),
            Date = packet.ReadString(),
            Publisher = packet.ReadGuid()
        });
    }

    public static void ChangeUsername(int fromClient, Packet packet)
    {
        string newUsername = packet.ReadString();
        Packet sendPacket = new Packet();

        string password = packet.ReadString();
        string username = packet.ReadString();
        string email = packet.ReadString();

        if (newUsername == username) { sendPacket.Write(false); sendPacket.Write("You need to provide a new username"); }
        else if (username == "") { sendPacket.Write(false); sendPacket.Write("Sorry mate but im not giving you a blank username"); }
        else if (Program.Usernames.Contains(newUsername)) { sendPacket.Write(false); sendPacket.Write("This username is already being used"); }
        else if (!Program.Usernames.Contains(username)) { sendPacket.Write(false); sendPacket.Write("You did not provide the correct original username"); }
        else if (Program.Accounts[Program.clients[fromClient].accountData].Email != email || Program.Accounts[Program.clients[fromClient].accountData].Pass != password) 
        { sendPacket.Write(false); sendPacket.Write("You provided incorrect account details"); }
        else
        {
            Program.Accounts[Program.clients[fromClient].accountData].Username = newUsername;

            Program.Usernames.Remove(username);
            Program.Usernames.Add(newUsername);

            sendPacket.Write(true);
            sendPacket.Write(newUsername);
        }

        sendPacket.Send(ServerPackets.changeUsernameResponse, fromClient);
    }

    public static void ChangeEmail(int fromClient, Packet packet)
    {
        string newEmail = packet.ReadString();
        Packet sendPacket = new Packet();

        string password = packet.ReadString();
        string username = packet.ReadString();
        string email = packet.ReadString();

        if (newEmail == email) { sendPacket.Write(false); sendPacket.Write("You need to provide a new email"); }
        else if (email == "") { sendPacket.Write(false); sendPacket.Write("You did not provide a valid email"); }
        else if (!Program.Emails.Contains(username)) { sendPacket.Write(false); sendPacket.Write("The account associated does not exist"); }
        else if (Program.Accounts[Program.clients[fromClient].accountData].Email != email || Program.Accounts[Program.clients[fromClient].accountData].Pass != password) 
        { sendPacket.Write(false); sendPacket.Write("You provided incorrect account details"); }
        else
        {
            Program.Accounts[Program.clients[fromClient].accountData].Email = newEmail;

            sendPacket.Write(true);
            sendPacket.Write(newEmail);
        }

        sendPacket.Send(ServerPackets.changeEmailResponse, fromClient);
    }

    public static void ChangeIcon(int fromClient, Packet packet)
    {
        Program.Accounts[Program.clients[fromClient].accountData].Icon = packet.ReadBytes();
    }
}