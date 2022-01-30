using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocialPostUIController : MonoBehaviour
{
    public TextMeshProUGUI PublisherText;
    public TextMeshProUGUI DateText;
    public Image PublisherImage;

    [Space]

    public TextMeshProUGUI ContentText;
    public Image AttachedImage;

    [Space]

    public Button DownloadItemButton;
    public TextMeshProUGUI DownloadNameText;

    [Space]

    public Button LikeButton;
    public TextMeshProUGUI LikeCount;
    public Button CommentButton;
    public TextMeshProUGUI CommentCount;
    public Button ReshareButton;
    public TextMeshProUGUI ReshareCount;

    SocialPostData postData;

    public void SetValues(SocialPostData postData)
    {
        this.postData = postData;

        PublisherImage.sprite = postData.PublisherIcon;
        ProgramManager.WriteText(PublisherText, postData.Publisher, 1f);
        ProgramManager.WriteText(DateText, postData.Date, 1f);

        ProgramManager.WriteText(ContentText, postData.Content, 1f);
        if (postData.UseImage) AttachedImage.sprite = postData.AttachedImage;

        DownloadItemButton.gameObject.SetActive(postData.UseDownload);
        DownloadNameText.text = postData.DownloadName;

        LikeCount.text = postData.LikeCount.ToString();
        CommentCount.text = postData.CommentCount.ToString();
        ReshareCount.text = postData.ReshareCount.ToString();
    }
}

public class SocialPostData
{
    public string Publisher;
    public string Date;
    public Sprite PublisherIcon;

    public string Content;

    public bool UseImage;
    public Sprite AttachedImage;

    public bool UseDownload;
    public string DownloadName;

    public int LikeCount;
    public int CommentCount;
    public int ReshareCount;
}
