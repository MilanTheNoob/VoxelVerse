using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using UnityEngine.Audio;

public class ProgramManager : MonoBehaviour
{
    public static ProgramManager instance;
    public static Dictionary<byte, Packet> Recving = new Dictionary<byte, Packet>();

    public delegate void PacketHandler(Packet _packet);
    public static Dictionary<byte, PacketHandler> packetHandlers;

    public static List<VersionInfoClass> DownloadableVersions = new List<VersionInfoClass>();
    public static List<VersionInfoClass> DownloadedVersions = new List<VersionInfoClass>();

    public static List<ArticleData> Devlogs = new List<ArticleData>();
    public static List<ArticleData> News = new List<ArticleData>();
    public static List<ArticleData> Events = new List<ArticleData>();

    public static List<PostData> CurrentForumThread = new List<PostData>();

    public static bool PlayOnDownload = false;
    public static bool Saved = false;

    public static Guid GUID;
    public static string Email = null;
    public static string Username = null;
    public static string Password = null;

    public static Sprite Icon;

    public static Process ActiveGame;
    public static int ActiveThread;
    public static int ActivePost;

    public MenuManager MainMenuM;
    public MenuManager SignupMenuM;
    public MenuManager LauncherMenuM;

    [Space]

    public TMP_InputField LoginEmail;
    public TMP_InputField LoginPass;
    public TextMeshProUGUI LoginError;
    public Button LoginEnter;

    [Space]

    public TMP_InputField SignupEmail;
    public TMP_InputField SignupUsername;
    public TMP_InputField SignupPass;
    public TMP_InputField SignupPassConfirm;
    public TextMeshProUGUI SignupError;
    public Button SignupEnter;

    [Space]

    public TMP_Dropdown InstallVersionDropdown;
    public TMP_InputField InstallNameInput;
    public Button InstallButton;

    [Space]

    public GameObject InstallUIHolder;
    public GameObject VersionUIObject;

    [Space]

    public TextMeshProUGUI LatestVerText;
    public Button LatestVerPlay;

    [Space]

    public Image ProfileIcon;
    public TextMeshProUGUI ProfileName;

    [Space]

    public GameObject ArticleItemUIObject;
    public GameObject DevlogListHolder;
    public GameObject NewsListHolder;
    public GameObject EventsListHolder;

    [Space]

    public TextMeshProUGUI ArticleName;
    public TextMeshProUGUI ArticleDate;
    public TextMeshProUGUI ArticlePublisher;
    public TextMeshProUGUI ArticleContent;

    [Space]

    public Button ArticleViewComments;
    public Button ArticlePostComment;
    public TMP_InputField ArticleComment;

    [Space]

    public GameObject CommentsHolder;
    public GameObject CommentPrefab;

    [Space]

    public GameObject PostUIItem;
    public GameObject PostsHolder;
    public TextMeshProUGUI ThreadNameText;
    public Button NewPostButton;

    [Space]

    public TMP_InputField NewPostTitleInput;
    public TMP_InputField NewPostContentInput;
    public Toggle NewPostNotificationToggle;
    public Toggle NewPostEmailToggle;
    public Button NewPostSubmit;
    public TextMeshProUGUI NewPostContentLength;
    public TextMeshProUGUI NewPostErrorText;

    [Space]

    public GameObject PostUI;
    public GameObject PostViewerHolder;
    public TextMeshProUGUI PostNameText;
    public TextMeshProUGUI PostSecondaryText;
    public Button PostReplyButton;

    [Space]

    public TMP_InputField PostReplyContent;
    public TextMeshProUGUI PostReplyContentLength;
    public Button PostReplySubmit;
    public TextMeshProUGUI PostReplyError;

    [Space]

    public TMP_InputField ChangeUsernameOld;
    public TMP_InputField ChangeUsernameNew;
    public Button ChangeUsernameSubmit;
    public TextMeshProUGUI ChangeUsernameError;

    [Space]

    public TMP_InputField ChangeEmailOld;
    public TMP_InputField ChangeEmailNew;
    public Button ChangeEmailSubmit;
    public TextMeshProUGUI ChangeEmailError;

    [Space]

    public GameObject FileButtonPrefab;
    public GameObject DirectoryButtonPrefab;

    [Space]

    public RectTransform BrowseFileContent;
    public RectTransform BrowseDirContent;
    public InputField BrowseCurrentPathField;
    public InputField BrowseSearchField;
    public Button BrowseSearchCancelButton;
    public Button BrowseCancelButton;
    public Button BrowseSelectButton;

    [Space]

    public Sprite BrowseFolderIcon;
    public Sprite BrowseDefaultIcon;
    public List<FileIcon> BrowseFileIcons = new List<FileIcon>();

    [Space]

    public Button IconChangeBrowse;
    public TMP_InputField IconChangeLoc;
    public Button IconChangeSubmit;
    public TextMeshProUGUI IconChangeError;

    [Space]

    public Button SocialPost;
    public Button SocialUploadPhoto;
    public Button SocialUploadFile;
    public TMP_InputField SocialPostContent;
    public Image SocialPostIcon;

    [Space]

    public TMP_InputField SocialSearch;
    public TextMeshProUGUI SocialPeopleOnline;
    public TextMeshProUGUI SocialFriendsOnline;
    public TextMeshProUGUI SocialNotifications;
    public TextMeshProUGUI SocialNewMessages;
    public TextMeshProUGUI SocialNewFollowers;
    public TextMeshProUGUI SocialMiscText;

    [Space]

    public GameObject SocialPostUI;
    public GameObject SocialAccountSuggestionUI;
    public GameObject SocialBookmarkUI;

    [Space]

    public GameObject SocialPostsHolder;
    public GameObject SocialAccountSuggestionsHolder;
    public GameObject SocialBookmarksHolder;

    [Space]

    public string ip = "127.0.0.1";
    public int port = 26950;

    [Space]

    public Sprite DefaultIcon;
    
    [HideInInspector] public int myId = 0;
    [HideInInspector] public TCP tcp;

    [HideInInspector] public ActiveCategoryEnum ActiveCategory;
    [HideInInspector] public int ActiveItem;
    [HideInInspector] public ArticleData ActiveArticle;

    bool isConnected = false;
    int requestedDownload = 0;

    string browseCurrentPath;
    string browseSearch;
    string browseSlash;

    List<string> browseDrives;
    List<FileButton> browseFileButtons;
    List<DirectoryButton> browseDirButtons;
    int browseSelected = -1;

    string[] browseFileFormats;
    Action<string> browseActionOnEnd;
    Action<string> browseActionCancel;

    #region Unity Funcs

    void Awake()
    {
        //MainMenuM.OpenMenu(0);

        packetHandlers = new Dictionary<byte, PacketHandler>()
        {
            { (byte)ServerPackets.welcome, ClientHandle.Welcome },
            { (byte)ServerPackets.loginResponse, ClientHandle.LoginResponse },
            { (byte)ServerPackets.signupResponse, ClientHandle.SignupResponse },
            { (byte)ServerPackets.launcherDataResponse, ClientHandle.LauncherDataResponse },
            { (byte)ServerPackets.versionDownload, ClientHandle.DownloadVersion },
            { (byte)ServerPackets.requestThreadResponse, ClientHandle.RequestThreadResponse },
            { (byte)ServerPackets.changeUsernameResponse, ClientHandle.ChangeUsernameResponse },
            { (byte)ServerPackets.changeEmailResponse, ClientHandle.ChangeEmailResponse },
            { (byte)ServerPackets.requestSocialDataResponse, ClientHandle.RequestSocialDataResponse }
        };

        instance = this;
        tcp = new TCP();

        isConnected = true;
        tcp.Connect();

        LoginEnter.onClick.AddListener(Login);
        SignupEnter.onClick.AddListener(Signup);
        InstallButton.onClick.AddListener(DownloadVersion);

        LoginError.text = "";
        SignupError.text = "";

        InstallVersionDropdown.onValueChanged.AddListener((int ver) => { requestedDownload = ver; });
        LatestVerPlay.onClick.AddListener(PlayLatestVersion);

        ArticlePostComment.onClick.AddListener(SendComment);
        ArticleViewComments.onClick.AddListener(ListComments);

        if (Directory.Exists(Application.persistentDataPath + "/Installs/"))
        {
            string[] paths = Directory.GetDirectories(Application.persistentDataPath + "/Installs/");
            for (int i = 0; i < paths.Length; i++) { DownloadedVersions.Add(VersionInfoClass.GetFromString(paths[i])); }
        }

        for (int i = 0; i < InstallUIHolder.transform.childCount; i++) Destroy(InstallUIHolder.transform.GetChild(0).gameObject);
        for (int i = 0; i < DownloadedVersions.Count; i++) { RenderVersion(DownloadedVersions[i]); }

        NewPostErrorText.text = "";
        PostReplyError.text = "";
        ChangeUsernameError.text = "";
        ChangeEmailError.text = "";
        IconChangeError.text = "";

        NewPostContentInput.onValueChanged.AddListener(UpdateContentLength);
        PostReplyContent.onValueChanged.AddListener(UpdateReplyContentLength);

        NewPostSubmit.onClick.AddListener(SubmitPost);
        PostReplySubmit.onClick.AddListener(SubmitReplyPost);

        ChangeUsernameSubmit.onClick.AddListener(SubmitNewUsername);
        ChangeEmailSubmit.onClick.AddListener(SubmitNewEmail);

        browseDrives = new List<string>(Directory.GetLogicalDrives());
        browseSlash = Path.DirectorySeparatorChar.ToString();

        browseCurrentPath = "";
        browseSelected = -1;
        browseSearch = "";

        BrowseSearchCancelButton.onClick.AddListener(SearchCancelClick);
        BrowseCancelButton.onClick.AddListener(CancelButtonClicked);
        BrowseSelectButton.onClick.AddListener(SelectButtonClicked);

        IconChangeBrowse.onClick.AddListener(ProfileIconBrowse);
        IconChangeSubmit.onClick.AddListener(ProfileIconLocSend);
    }

    void OnApplicationQuit() { Disconnect(); }

    void Disconnect()
    {
        if (Email != null && Password != null)
        {
            Packet serPacket = new Packet();

            serPacket.Write(Email);
            serPacket.Write(Username);
            serPacket.Write(Password);

            File.WriteAllBytes(Application.persistentDataPath + "/Settings.dat", serPacket.buffer.ToArray());
        }

        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
        }
    }

    #endregion

    #region Login & Signup

    public void Signup()
    {
        if (SignupEmail.text.Length == 0 || SignupPass.text.Length == 0 || SignupPassConfirm.text.Length == 0 ||
            SignupUsername.text.Length == 0) { WriteText(SignupError, "One or more fields are empty", 1.5f); return; }
        if (SignupPass.text != SignupPassConfirm.text) { WriteText(SignupError, "The password fields do not match", 1.5f); ; return; }
        if (SignupPass.text.Length <= 4) { WriteText(SignupError, "Please provide a longer password", 1.5f); return; }
        if (!SignupEmail.text.Contains("@") || !SignupEmail.text.Contains(".")) { WriteText(SignupError, "You did not provide a valid email :-(", 1.5f); return; }

        Packet packet = new Packet();

        packet.Write(SignupEmail.text);
        packet.Write(SignupUsername.text);
        packet.Write(SignupPass.text);
        packet.Write(DefaultIcon.texture.EncodeToPNG());

        Email = SignupEmail.text;
        Password = SignupPass.text;
        Username = SignupUsername.text;

        packet.Send(ClientPackets.signupRequest);
    }

    public void Login()
    {
        if (LoginEmail.text.Length == 0 || LoginPass.text.Length == 0) { WriteText(LoginError, "One or more fields are empty", 1.5f); return; }

        Email = LoginEmail.text;
        Password = LoginPass.text;

        ClientSend.Login(LoginEmail.text, LoginPass.text);
    }

    #endregion
    #region Misc

    public void DownloadVersion()
    {
        LauncherMenuM.OpenMenu(3);

        Packet packet = new Packet();
        DownloadableVersions[requestedDownload].SerializeVersion(ref packet);
        packet.Send(ClientPackets.requestVersionDownload);
    }

    public void RenderVersion(VersionInfoClass verData)
    {
        GameObject installUI = Instantiate(VersionUIObject);
        installUI.transform.parent = InstallUIHolder.transform;
        installUI.transform.localScale = Vector3.one;

        installUI.GetComponent<VersionUIController>().SetVersion(verData);
    }

    public void SendComment()
    {
        if (ArticleComment.text.Length > 0)
        {
            Packet packet = new Packet();

            packet.Write((byte)ActiveCategory);
            packet.Write(ActiveItem);

            packet.Write(DateTime.Now.ToString("dd/MM/yy"));
            packet.Write(Username);
            packet.Write(ArticleComment.text);

            CommentUIController commentUI = Instantiate(CommentPrefab, CommentsHolder.transform).GetComponent<CommentUIController>();
            commentUI.transform.localScale = Vector3.one;
            commentUI.SetValues(new CommentData() { Content = ArticleComment.text, Date = DateTime.Now.ToString("dd/MM/yy"), Publisher = Username });

            packet.Send(ClientPackets.sendComment);
        }
    }

    public void ListComments()
    {
        LauncherMenuM.OpenMenu(8);

        for (int i = 0; i < CommentsHolder.transform.childCount; i++) { CommentsHolder.transform.GetChild(0); }
        for (int i = 0; i < ActiveArticle.Comments.Count; i++)
        {
            CommentUIController commentUI = Instantiate(CommentPrefab, CommentsHolder.transform).GetComponent<CommentUIController>();
            commentUI.transform.localScale = Vector3.one;
            commentUI.SetValues(ActiveArticle.Comments[i]);
        }
    }

    public void PlayLatestVersion()
    {
        VersionInfoClass latestVer = DownloadableVersions[DownloadableVersions.Count - 1];

        if (DownloadedVersions.Contains(latestVer))
        {
            Process.Start(Application.persistentDataPath + "/Installs/" + latestVer.ToString() + "/VoxelVerse.exe");
        }
        else
        {
            LauncherMenuM.OpenMenu(3);
            PlayOnDownload = true;

            Packet packet = new Packet();
            DownloadableVersions[requestedDownload].SerializeVersion(ref packet);
            packet.Send(ClientPackets.requestVersionDownload);
        }
    }

    public static void WriteText(TextMeshProUGUI textHolder, string text, float duration) { instance.StartCoroutine(IWriteText(textHolder, text, duration)); }
    public static IEnumerator IWriteText(TextMeshProUGUI textHolder, string text, float duration)
    {
        textHolder.text = "";
        float timePerChar = duration / text.Length;

        for (int i = 0; i < text.Length; i++)
        {
            textHolder.text = textHolder.text + text[i];
            yield return new WaitForSeconds(timePerChar);
        }
    }

    #endregion
    #region Networking

    public class TCP
    {
        public TcpClient socket;

        public NetworkStream stream;
        public byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient { ReceiveBufferSize = 256000, SendBufferSize = 4096 };

            receiveBuffer = new byte[256000];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);
            if (!socket.Connected) return;

            stream = socket.GetStream();
            stream.BeginRead(receiveBuffer, 0, 256000, ReceiveCallback, null);
        }

        public void SendData(byte[] data)
        {
            try { stream.BeginWrite(data, 0, 4096, null, null); }
            catch (Exception _ex) { UnityEngine.Debug.Log($"Error sending data to server via TCP: {_ex}"); }
        }

        void ReceiveCallback(IAsyncResult _result)
        {
            if (stream.EndRead(_result) <= 0) { Disconnect(); return; }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                if (receiveBuffer[1] == 1) { packetHandlers[receiveBuffer[0]](new Packet(receiveBuffer)); }
                else
                {
                    switch (receiveBuffer[2])
                    {
                        case 0: Recving.Add(receiveBuffer[0], new Packet(receiveBuffer)); break;
                        case 1: Recving[receiveBuffer[0]].AddChunk(receiveBuffer); break;
                        case 2: Recving[receiveBuffer[0]].AddChunk(receiveBuffer); packetHandlers[receiveBuffer[0]](Recving[receiveBuffer[0]]); Recving.Remove(receiveBuffer[0]); break;
                    }
                }
            });

            stream.BeginRead(receiveBuffer, 0, 256000, ReceiveCallback, null);
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    #endregion
    #region Forums

    public void OpenThread(int id)
    {
        LauncherMenuM.OpenMenu(13);
        ThreadNameText.text = Utils.AddSpacesToSentence(((ForumTypeEnum)id).ToString(), false);

        foreach (Transform child in PostsHolder.transform) { Destroy(child.gameObject); }

        Packet serPacket = new Packet();
        serPacket.Write(id);
        serPacket.Send(ClientPackets.requestThread);

        ActiveThread = id;
    }

    public void OpenPost(int id)
    {
        LauncherMenuM.OpenMenu(15);
        PostNameText.text = CurrentForumThread[id].Title;
        PostSecondaryText.text = "By " + CurrentForumThread[id].Items[0].Publisher + ", Created " + CurrentForumThread[id].Items[0].Date +
            ", Latest Reply " + CurrentForumThread[id].Items[CurrentForumThread[id].Items.Count - 1].Date;

        ActivePost = id;

        foreach (Transform child in PostViewerHolder.transform) { Destroy(child.gameObject); }
        for (int i = 0; i < CurrentForumThread[id].Items.Count; i++)
        {
            GameObject itemObject = Instantiate(PostUI);
            itemObject.transform.parent = PostViewerHolder.transform;
            itemObject.GetComponent<RectTransform>().localScale = Vector3.one;
            itemObject.GetComponent<PostItemController>().SetValues(CurrentForumThread[id].Items[i]);
        }
    }

    public void UpdateContentLength(string text) { NewPostContentLength.text = text.Length + "/4000"; }
    public void UpdateReplyContentLength(string text) { PostReplyContentLength.text = text.Length + "/4000"; }

    public void SubmitPost()
    {
        if (NewPostContentInput.text.Length == 0 || NewPostTitleInput.text.Length == 0) { WriteText(NewPostErrorText, "One or more fields are empty", 0.5f); }
        Packet packet = new Packet();

        PostData postData = new PostData();
        postData.Title = NewPostTitleInput.text;
        postData.Views = 1;
        postData.Items.Add(new PostData.PostItemData() { Content = NewPostContentInput.text, Date = DateTime.Now.ToString("dd/MM/yy"), Publisher = Username });

        packet.Write(ActiveThread);
        packet.Write(postData.Title);
        packet.Write(postData.Items[0].Content);
        packet.Write(Username);
        packet.Write(postData.Items[0].Date);

        CurrentForumThread.Add(postData);
        LauncherMenuM.OpenMenu(13);

        GameObject itemObject = Instantiate(PostUIItem);
        itemObject.transform.parent = PostsHolder.transform;
        itemObject.GetComponent<RectTransform>().localScale = Vector3.one;
        itemObject.GetComponent<PostUIController>().SetValues(postData);

        packet.Send(ClientPackets.addPost);
    }

    public void SubmitReplyPost()
    {
        if (PostReplyContent.text.Length == 0) { WriteText(NewPostErrorText, "One or more fields are empty", 0.5f); }

        LauncherMenuM.OpenMenu(15);
        Packet packet = new Packet();

        packet.Write(ActiveThread);
        packet.Write(ActivePost);

        PostData.PostItemData postItem = new PostData.PostItemData()
        {
            Content = PostReplyContent.text,
            Date = DateTime.Now.ToString("dd/MM/yy"),
            Publisher = Username
        };
        CurrentForumThread[ActivePost].Items.Add(postItem);

        GameObject itemObject = Instantiate(PostUI);
        itemObject.transform.parent = PostViewerHolder.transform;
        itemObject.GetComponent<RectTransform>().localScale = Vector3.one;
        itemObject.GetComponent<PostItemController>().SetValues(postItem);

        packet.Write(postItem.Content);
        packet.Write(postItem.Date);
        packet.Write(postItem.Publisher);

        packet.Send(ClientPackets.addReplyPost);
    }

    #endregion

    public void SubmitNewUsername()
    {
        Packet packet = new Packet();
        packet.Write(ChangeUsernameNew.text);

        packet.Write(Password);
        packet.Write(Username);
        packet.Write(Email);

        packet.Send(ClientPackets.changeUsername);
    }

    public void SubmitNewEmail()
    {
        Packet packet = new Packet();
        packet.Write(ChangeEmailNew.text);

        packet.Write(Password);
        packet.Write(Username);
        packet.Write(Email);

        packet.Send(ClientPackets.changeUsername);
    }

    public void ProfileIconBrowse() { BrowseForFile(SendNewProfileIcon, (string r) => { WriteText(IconChangeError, "You need to provide a valid square png", 0.5f); }, new string[] { "png" }); }
    public void ProfileIconLocSend() { SendNewProfileIcon(IconChangeLoc.text); }

    public void SendNewProfileIcon(string loc)
    {
        if (File.Exists(loc))
        {
            byte[] imgData = File.ReadAllBytes(loc);
            Texture2D tex = new Texture2D(1, 1);

            tex.LoadImage(imgData);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            Icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
            ProfileIcon.sprite = Icon;

            Packet packet = new Packet();
            packet.Write(imgData);
            packet.Send(ClientPackets.changeIcon);

            LauncherMenuM.OpenMenu(0);
        }
        else
        {
            WriteText(IconChangeError, "Could not find a png file at the specified location man.", 0.5f);
            LauncherMenuM.OpenMenu(19);
        }
    }

    #region File Browsing

    public static void BrowseForFile(Action<string> actionOnEnd, Action<string> actionOnCancel, string[] acceptableFileTypes, string defaultDirectory = "")
    {
        instance.LauncherMenuM.OpenMenu(20);

        instance.browseFileFormats = acceptableFileTypes;
        instance.browseActionOnEnd = actionOnEnd;
        instance.browseActionCancel = actionOnCancel;

        instance.BrowseCurrentPathField.text = instance.browseCurrentPath;
        instance.BrowseSearchField.text = instance.browseSearch;

        instance.UpdateDirectoryList();
        instance.UpdateFileList();
    }

    public void OnFileClick(int i)
    {
        if (i >= browseFileButtons.Count) return;

        if (browseFileButtons[i].isDir) GotoDirectory(browseFileButtons[i].fullPath);
        else SelectFile(i);
    }

    void SelectFile(int i)
    {
        try { browseFileButtons[browseSelected].Unselect(); } catch { }

        browseSelected = i;
        browseFileButtons[i].Select();
    }

    void UpdateFileList()
    {
        if (browseFileButtons == null) browseFileButtons = new List<FileButton>();
        else
        {
            for (int i = 0; i < browseFileButtons.Count; i++) { Destroy(browseFileButtons[i].gameObject); }
            browseFileButtons.Clear();
        }

        if (string.IsNullOrEmpty(browseCurrentPath))
        {
            for (int i = 0; i < browseDrives.Count; i++) CreateFileButton(browseDrives[i], browseDrives[i], true, i);
            return;
        }

        List<string> files = new List<string>();
        files = new List<string>(Directory.GetFiles(browseCurrentPath));
        FilterFormat(files);

        List<string> dirs = new List<string>();
        dirs = new List<string>(Directory.GetDirectories(browseCurrentPath));

        for (int i = 0; i < dirs.Count; i++)
        {
            string name = dirs[i].Substring(dirs[i].LastIndexOf(browseSlash) + 1);
            CreateFileButton(name, dirs[i], true, browseFileButtons.Count);
        }
        for (int i = 0; i < files.Count; i++)
        {
            string name = files[i].Substring(files[i].LastIndexOf(browseSlash) + 1);
            CreateFileButton(name, files[i], false, browseFileButtons.Count);
        }
    }

    void UpdateDirectoryList()
    {
        if (browseDirButtons == null) browseDirButtons = new List<DirectoryButton>();
        else
        {
            for (int i = 0; i < browseDirButtons.Count; i++) Destroy(browseDirButtons[i].gameObject);
            browseDirButtons.Clear();
        }

        CreateDirectoryButton("This PC", "", 0);

        if (!string.IsNullOrEmpty(browseCurrentPath))
        {
            string[] dirs = browseCurrentPath.Split(browseSlash[0]);
            for (int i = 0; i < dirs.Length; i++)
            {
                if (!string.IsNullOrEmpty(dirs[i]))
                {
                    string path = browseCurrentPath.Substring(0, browseCurrentPath.LastIndexOf(dirs[i]));
                    CreateDirectoryButton(dirs[i] + browseSlash, path + dirs[i] + browseSlash, browseDirButtons.Count);
                }
            }
        }
    }

    void CreateFileButton(string text, string path, bool dir, int i)
    {
        GameObject obj = Instantiate(FileButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        obj.GetComponent<RectTransform>().SetParent(BrowseFileContent, false);
        FileButton fb = obj.GetComponent<FileButton>();
        fb.Set(text, path, dir, i);
        browseFileButtons.Add(fb);
    }
    void CreateDirectoryButton(string text, string path, int i)
    {
        GameObject obj = Instantiate(DirectoryButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        obj.GetComponent<RectTransform>().SetParent(BrowseDirContent, false);
        DirectoryButton db = obj.GetComponent<DirectoryButton>();
        db.Set(text, path, i);
        browseDirButtons.Add(db);
    }

    void FilterFormat(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            bool remove = true;
            string extension = "";

            if (files[i].Contains(".")) extension = files[i].Substring(files[i].LastIndexOf('.') + 1).ToLowerInvariant();
            for (int j = 0; j < browseFileFormats.Length; j++) { if (extension == browseFileFormats[j].Trim().ToLowerInvariant()) { remove = false; break; } }

            if (remove) { files.RemoveAt(i); i--; }
        }
    }

    public void OnDirectoryClick(int i)
    {
        if (i >= browseDirButtons.Count) return;
        GotoDirectory(browseDirButtons[i].fullPath);
    }

    void FilterList()
    {
        if (string.IsNullOrEmpty(browseSearch))
        {
            for (int i = 0; i < browseFileButtons.Count; i++) browseFileButtons[i].gameObject.SetActive(true);
            return;
        }
        for (int i = 0; i < browseFileButtons.Count; i++)
        {
            browseFileButtons[i].gameObject.SetActive(browseFileButtons[i].text.Contains(browseSearch));
        }
    }

    void GotoDirectory(string path)
    {
        if (path == browseCurrentPath && path != string.Empty) return;

        if (string.IsNullOrEmpty(path)) browseCurrentPath = "";
        else
        {
            if (!Directory.Exists(path)) return;
            else browseCurrentPath = path;
        }

        if (BrowseCurrentPathField) BrowseCurrentPathField.text = browseCurrentPath;
        browseSelected = -1;

        UpdateFileList();
        UpdateDirectoryList();
    }

    public void PathFieldEndEdit()
    {
        if (Directory.Exists(BrowseCurrentPathField.text)) GotoDirectory(BrowseCurrentPathField.text);
        else BrowseCurrentPathField.text = browseCurrentPath;
    }

    public void SearchChanged() { browseSearch = BrowseSearchField.text.Trim(); FilterList(); }
    public void SearchCancelClick() { browseSearch = ""; BrowseSearchField.text = ""; FilterList(); }

    public void SelectButtonClicked()
    {
        if (browseSelected > -1 && !browseFileButtons[browseSelected].isDir) { browseActionOnEnd(browseFileButtons[browseSelected].fullPath); }
    }
    public void CancelButtonClicked() { browseActionCancel(""); }

    public Sprite GetFileIcon(string path)
    {
        string extension = "";

        if (path.Contains(".")) extension = path.Substring(path.LastIndexOf('.') + 1);
        else return BrowseDefaultIcon;

        for (int i = 0; i < BrowseFileIcons.Count; i++)
        {
            if (BrowseFileIcons[i].extension == extension) { return BrowseFileIcons[i].icon; }
        }
        return BrowseDefaultIcon;
    }

    #endregion
    #region Social

    public void SetupSocial() { new Packet().Send(ClientPackets.requestSocialData); }

    public void SocialSendPost()
    {
        if (SocialPostContent.text.Length == 0) { WriteText(SocialMiscText, "You ... don't have a anything in the contents of the post? Heh?", 0.5f); return; }
    }

    #endregion
}

public class PostData
{
    public string Title;
    public int Views;

    public List<PostItemData> Items = new List<PostItemData>();
    
    public class PostItemData
    {
        public string Content;
        public string Publisher;
        public string Date;
    }

    public static PostData Deserialize(ref Packet packet)
    {
        PostData postData = new PostData()
        {
            Title = packet.ReadString(),
            Views = packet.ReadInt()
        };

        int postItems = packet.ReadInt();
        for (int i = 0; i < postItems; i++)
        {
            postData.Items.Add(new PostItemData()
            {
                Content = packet.ReadString(),
                Publisher = packet.ReadString(),
                Date = packet.ReadString()
            });
        }

        return postData;
    }
}

public enum VersionTypeEnum { Prototype = 1, Beta, Alpha, Release }
public enum ActiveCategoryEnum { Devlog, News, Event }
public enum ForumTypeEnum { GameDiscussion, Multiplayer, Guides, Showcase, Fanmade, Merchandise, ModsShowcase, ModsHelp, ModsIdeas, ModsDiscussion, 
    Help, Bugs, Suggestions, Feedback, Introductions, General, Opinion }