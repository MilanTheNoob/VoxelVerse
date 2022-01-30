using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArticleItemUIController : MonoBehaviour
{
    public Button ArticleButton;

    [Space]

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI PublisherText;
    public TextMeshProUGUI DateText;

    ArticleData currentData;

    ActiveCategoryEnum itemCategory;
    int itemId;

    void Start()
    {
        ArticleButton.onClick.AddListener(OpenArticle);
    }

    public void SetValues(ArticleData articleData, ActiveCategoryEnum activeCategory, int activeItem)
    {
        currentData = articleData;

        itemCategory = activeCategory;
        itemId = activeItem;

        TitleText.text = articleData.Title;
        PublisherText.text = articleData.Publisher;
        DateText.text = articleData.Date;

        ProgramManager.instance.ActiveCategory = itemCategory;
        ProgramManager.instance.ActiveItem = itemId;
        ProgramManager.instance.ActiveArticle = articleData;
    }

    public void OpenArticle()
    {
        ProgramManager.instance.LauncherMenuM.OpenMenu(5);

        ProgramManager.WriteText(ProgramManager.instance.ArticleName, currentData.Title, 1f);
        ProgramManager.WriteText(ProgramManager.instance.ArticlePublisher, currentData.Publisher, 1f);
        ProgramManager.WriteText(ProgramManager.instance.ArticleDate, currentData.Date, 1f);

        ProgramManager.WriteText(ProgramManager.instance.ArticleContent, currentData.Content, 1f);

        ProgramManager.instance.ActiveCategory = itemCategory;
        ProgramManager.instance.ActiveItem = itemId;
        ProgramManager.instance.ActiveArticle = currentData;
    }
}

public class ArticleData
{
    public string Title;
    public string Publisher;
    public string Date;

    public string Content;
    public byte[] Images;

    public List<CommentData> Comments = new List<CommentData>();

    public void Serialize(ref Packet packet)
    {
        packet.Write(Title);
        packet.Write(Publisher);
        packet.Write(Date);

        packet.Write(Content);
        // Images here
    }

    public static ArticleData Deserialize(ref Packet packet)
    {
        ArticleData data = new ArticleData();

        data.Title = packet.ReadString();
        data.Publisher = packet.ReadString();
        data.Date = packet.ReadString();

        data.Content = packet.ReadString();

        int commentCount = packet.ReadInt();
        for (int i = 0; i < commentCount; i++)
        {
            data.Comments.Add(new CommentData()
            {
                Date = packet.ReadString(),
                Publisher = packet.ReadString(),

                Content = packet.ReadString()
            });
        }

        return data;
    }
}

public class CommentData
{
    public string Date;
    public string Publisher;

    public string Content;
}