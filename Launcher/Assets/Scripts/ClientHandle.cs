using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using UnityEngine.UI;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        int _myId = _packet.ReadInt();
        ClientSend.WelcomeReceived();

        ProgramManager.instance.myId = _myId;

        if (File.Exists(Application.persistentDataPath + "/Settings.dat"))
        {
            Packet serPacket = new Packet();
            serPacket.rb = File.ReadAllBytes(Application.persistentDataPath + "/Settings.dat");
            serPacket.pos = 0;

            ProgramManager.Email = serPacket.ReadString();
            ProgramManager.Username = serPacket.ReadString();
            ProgramManager.Password = serPacket.ReadString();

            ClientSend.Login(ProgramManager.Email, ProgramManager.Password);
        }
        else
        {
            ProgramManager.instance.MainMenuM.OpenMenu(1);
            ProgramManager.instance.SignupMenuM.OpenMenu(0);
        }
    }

    public static void LoginResponse(Packet packet)
    {
        bool success = packet.ReadBool();

        if (success)
        {
            GameObject.Find("Canvas").GetComponent<MenuManager>().OpenMenu(2);
            new Packet().Send(ClientPackets.launcherDataRequest);

            ProgramManager.Email = packet.ReadString();
            ProgramManager.Username = packet.ReadString();

            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(packet.ReadBytes());
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            ProgramManager.Icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));

            ProgramManager.GUID = new System.Guid(packet.ReadBytes(16));

            ProgramManager.WriteText(ProgramManager.instance.ProfileName, ProgramManager.Username, 1f);
            ProgramManager.instance.ProfileIcon.sprite = ProgramManager.Icon;
        }
        else
        {
            ProgramManager.WriteText(ProgramManager.instance.LoginError, packet.ReadString(), 1.5f);

            ProgramManager.instance.MainMenuM.OpenMenu(1);
            ProgramManager.instance.SignupMenuM.OpenMenu(1);
        }
    }

    public static void SignupResponse(Packet packet)
    {
        bool success = packet.ReadBool();

        if (success)
        {
            ProgramManager.instance.MainMenuM.OpenMenu(2);
            new Packet().Send(ClientPackets.launcherDataRequest);
            print(ProgramManager.Username);
            ProgramManager.WriteText(ProgramManager.instance.ProfileName, ProgramManager.Username, 1f);
        }
        else
        {
            ProgramManager.WriteText(ProgramManager.instance.SignupError, "An account with the provided email already exists", 1.5f);

            if (ProgramManager.Saved)
            {
                ProgramManager.instance.MainMenuM.OpenMenu(1);
                ProgramManager.instance.SignupMenuM.OpenMenu(0);
            }
        }
    }

    public static void LauncherDataResponse(Packet packet)
    {
        // Downloadable Game Clients

        int versionCount = packet.ReadInt();
        List<TMPro.TMP_Dropdown.OptionData> dropDownOptions = new List<TMPro.TMP_Dropdown.OptionData>();

        ProgramManager.DownloadableVersions.Clear();

        for (int i = 0; i < versionCount; i++)
        {
            ProgramManager.DownloadableVersions.Add(VersionInfoClass.Deserialize(ref packet));
            dropDownOptions.Add(new TMPro.TMP_Dropdown.OptionData() { text = ProgramManager.DownloadableVersions[i].ToString() });
        }

        ProgramManager.instance.LatestVerText.text = ProgramManager.DownloadableVersions[ProgramManager.DownloadableVersions.Count - 1].ToString();

        ProgramManager.instance.InstallVersionDropdown.ClearOptions();
        ProgramManager.instance.InstallVersionDropdown.AddOptions(dropDownOptions);

        // Devlogs

        int devlogCount = packet.ReadInt();
        ProgramManager.Devlogs.Clear();

        for (int i = 0; i < ProgramManager.instance.DevlogListHolder.transform.childCount; i++) { Destroy(ProgramManager.instance.DevlogListHolder.transform.GetChild(0)); }
        for (int i = 0; i < devlogCount; i++)
        {
            ProgramManager.Devlogs.Add(ArticleData.Deserialize(ref packet));

            ArticleItemUIController devlogItem = Instantiate(ProgramManager.instance.ArticleItemUIObject, ProgramManager.instance.DevlogListHolder.transform).GetComponent<ArticleItemUIController>();
            devlogItem.transform.localScale = Vector3.one;
            devlogItem.SetValues(ProgramManager.Devlogs[i], ActiveCategoryEnum.Devlog, i);
        }

        // News

        int newsCount = packet.ReadInt();
        ProgramManager.News.Clear();

        for (int i = 0; i < ProgramManager.instance.NewsListHolder.transform.childCount; i++) { Destroy(ProgramManager.instance.NewsListHolder.transform.GetChild(0)); }
        for (int i = 0; i < newsCount; i++)
        {
            ProgramManager.News.Add(ArticleData.Deserialize(ref packet));

            ArticleItemUIController newsItem = Instantiate(ProgramManager.instance.ArticleItemUIObject, ProgramManager.instance.NewsListHolder.transform).GetComponent<ArticleItemUIController>();
            newsItem.transform.localScale = Vector3.one;
            newsItem.SetValues(ProgramManager.News[i], ActiveCategoryEnum.News, i);
        }

        // Events

        int eventsCount = packet.ReadInt();
        ProgramManager.Events.Clear();

        for (int i = 0; i < ProgramManager.instance.EventsListHolder.transform.childCount; i++) { Destroy(ProgramManager.instance.EventsListHolder.transform.GetChild(0)); }
        for (int i = 0; i < newsCount; i++)
        {
            ProgramManager.Events.Add(ArticleData.Deserialize(ref packet));

            ArticleItemUIController eventsItem = Instantiate(ProgramManager.instance.ArticleItemUIObject, ProgramManager.instance.EventsListHolder.transform).GetComponent<ArticleItemUIController>();
            eventsItem.transform.localScale = Vector3.one;
            eventsItem.SetValues(ProgramManager.Events[i], ActiveCategoryEnum.News, i);
        }
    }

    public static void DownloadVersion(Packet packet)
    {
        VersionInfoClass versionInfo = VersionInfoClass.Deserialize(ref packet);
        int count = packet.ReadInt();

        byte[] versionDownload = new byte[count];
        for (int i = 0; i < count; i++) { versionDownload[i] = packet.ReadByte(); }

        if (!Directory.Exists(Application.persistentDataPath + "/Temp/")) Directory.CreateDirectory(Application.persistentDataPath + "/Temp/");
        File.WriteAllBytes(Application.persistentDataPath + "/Temp/" + versionInfo.ToString() + ".zip", versionDownload);

        UnpackInstall(versionInfo.ToString(), versionDownload);

        ProgramManager.instance.LauncherMenuM.OpenMenu(1);
        packet.Dispose();

        ProgramManager.DownloadedVersions.Add(versionInfo);
        ProgramManager.instance.RenderVersion(versionInfo);

        if (ProgramManager.PlayOnDownload)
        {
            ProgramManager.PlayOnDownload = false;
            Process.Start(Application.persistentDataPath + "/Installs/" + versionInfo.ToString() + "/VoxelVerse.exe");
        }
    }

    public static void UnpackInstall(string name, byte[] data)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Installs")) Directory.CreateDirectory(Application.persistentDataPath + "/Installs");
        Directory.CreateDirectory(Application.persistentDataPath + "/Installs/" + name);
        int numFiles = 0;

        using (ZipInputStream s = new ZipInputStream(new MemoryStream(data)))
        {
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (directoryName.Length > 0)
                {
                    var dirPath = Path.Combine(Application.persistentDataPath + "/Installs/" + name + "/", directoryName);
                    Directory.CreateDirectory(dirPath);
                }

                if (fileName != string.Empty)
                {
                    var entryFilePath = Path.Combine(Application.persistentDataPath + "/Installs/" + name + "/", theEntry.Name);
                    using (FileStream streamWriter = File.Create(entryFilePath))
                    {
                        int size = 2048;
                        byte[] fdata = new byte[size];
                        while (true)
                        {
                            size = s.Read(fdata, 0, fdata.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(fdata, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public static void RequestThreadResponse(Packet packet)
    {
        int itemCount = packet.ReadInt();
        UnityEngine.Debug.Log(itemCount);
        ProgramManager.CurrentForumThread.Clear();

        for (int i = 0; i < itemCount; i++)
        {
            ProgramManager.CurrentForumThread.Add(PostData.Deserialize(ref packet));
            GameObject itemObject = Instantiate(ProgramManager.instance.PostUIItem, ProgramManager.instance.PostsHolder.transform);
            itemObject.transform.localScale = Vector3.one;
            itemObject.GetComponent<PostUIController>().SetValues(ProgramManager.CurrentForumThread[i]);
            itemObject.GetComponent<Button>().onClick.AddListener(delegate { ProgramManager.instance.OpenPost(itemObject.transform.GetSiblingIndex()); });
        }
    }

    public static void ChangeUsernameResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            ProgramManager.Username = packet.ReadString();
            ProgramManager.instance.ProfileName.text = ProgramManager.Username;

            ProgramManager.instance.LauncherMenuM.OpenMenu(16);
        }
        else
        {
            ProgramManager.WriteText(ProgramManager.instance.ChangeUsernameError, packet.ReadString(), 0.5f);
        }
    }

    public static void ChangeEmailResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            ProgramManager.Email = packet.ReadString();
            ProgramManager.instance.LauncherMenuM.OpenMenu(16);
        }
        else
        {
            ProgramManager.WriteText(ProgramManager.instance.ChangeUsernameError, packet.ReadString(), 0.5f);
        }
    }

    public static void RequestSocialDataResponse(Packet packet)
    {
        ProgramManager.instance.SocialPeopleOnline.text = packet.ReadInt() + " People Online";
        ProgramManager.instance.SocialFriendsOnline.text = packet.ReadInt() + " Friends Online";

        ProgramManager.instance.SocialNotifications.text = packet.ReadInt() + " New Notifications";
        ProgramManager.instance.SocialNewMessages.text = packet.ReadInt() + " New Messages";
        ProgramManager.instance.SocialNewFollowers.text = packet.ReadInt() + " More Followers";

        int feedPosts = packet.ReadInt();
        foreach (Transform child in ProgramManager.instance.SocialPostsHolder.transform) { Destroy(child.gameObject); }

        for (int i = 0; i < feedPosts; i++)
        {
            SocialPostData postData = new SocialPostData()
            {
                Publisher = packet.ReadString(),
                Date = packet.ReadString(),
                Content = packet.ReadString(),
                UseImage = packet.ReadBool(),
                UseDownload = packet.ReadBool(),
                LikeCount = packet.ReadInt(),
                CommentCount = packet.ReadInt(),
                ReshareCount = packet.ReadInt()
            };

            Texture2D publisherTex = new Texture2D(1, 1);
            publisherTex.LoadImage(packet.ReadBytes());
            publisherTex.Apply();
            publisherTex.filterMode = FilterMode.Point;
            postData.PublisherIcon = Sprite.Create(publisherTex, new Rect(0, 0, publisherTex.width, publisherTex.height), new Vector2(publisherTex.width / 2, publisherTex.height / 2));

            if (postData.UseImage)
            {
                Texture2D attTex = new Texture2D(1, 1);
                attTex.LoadImage(packet.ReadBytes());
                attTex.Apply();
                attTex.filterMode = FilterMode.Point;
                postData.AttachedImage = Sprite.Create(attTex, new Rect(0, 0, attTex.width, attTex.height), new Vector2(attTex.width / 2, attTex.height / 2));
            }

            Instantiate(ProgramManager.instance.SocialPostUI, ProgramManager.instance.SocialPostsHolder.transform).GetComponent<SocialPostUIController>().SetValues(postData);
        }

        int recommendedPeople = packet.ReadInt();
        foreach (Transform child in ProgramManager.instance.SocialAccountSuggestionsHolder.transform) { Destroy(child.gameObject); }

        for (int i = 0; i < recommendedPeople; i++)
        {
            SocialAccountData accountData = new SocialAccountData()
            {
                Name = packet.ReadString(),
                Tag = packet.ReadString(),

                IsVerified = packet.ReadBool()
            };

            Texture2D iconTex = new Texture2D(1, 1);
            iconTex.LoadImage(packet.ReadBytes());
            iconTex.Apply();
            iconTex.filterMode = FilterMode.Point;
            accountData.Icon = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(iconTex.width / 2, iconTex.height / 2));

            Instantiate(ProgramManager.instance.SocialAccountSuggestionUI, ProgramManager.instance.SocialAccountSuggestionsHolder.transform).GetComponent<AccountSuggestionUIController>().SetValues(accountData);
        }

        int bookmarks = packet.ReadInt();
        foreach (Transform child in ProgramManager.instance.SocialBookmarksHolder.transform) { Destroy(child.gameObject); }

        for (int i = 0; i < bookmarks; i++)
        {
            BookmarkData bookmarkData = new BookmarkData()
            {
                Publisher = packet.ReadString(),
                Content = packet.ReadString()
            };

            Texture2D iconTex = new Texture2D(1, 1);
            iconTex.LoadImage(packet.ReadBytes());
            iconTex.Apply();
            iconTex.filterMode = FilterMode.Point;
            bookmarkData.PublisherImage = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(iconTex.width / 2, iconTex.height / 2));

            Instantiate(ProgramManager.instance.SocialBookmarkUI, ProgramManager.instance.SocialBookmarksHolder.transform).GetComponent<BookmarkUIController>().SetValues(bookmarkData);
        }
    }
}