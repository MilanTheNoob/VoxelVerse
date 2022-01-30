using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkUIController : MonoBehaviour
{
    public TextMeshProUGUI PublisherText;
    public Image PublisherImage;
    public TextMeshProUGUI ContentText;

    BookmarkData postData;

    public void SetValues(BookmarkData postData)
    {
        this.postData = postData;

        PublisherImage.sprite = postData.PublisherImage;
        ProgramManager.WriteText(PublisherText, postData.Publisher, 1f);
        ProgramManager.WriteText(ContentText, postData.Content, 1f);
    }
}

public class BookmarkData
{
    public string Publisher;
    public string Content;
    public Sprite PublisherImage;
}
