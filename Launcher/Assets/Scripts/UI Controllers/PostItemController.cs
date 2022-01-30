using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PostItemController : MonoBehaviour
{
    public TextMeshProUGUI PublisherText;
    public TextMeshProUGUI DateText;
    public TextMeshProUGUI ContentText;

    public void SetValues(PostData.PostItemData postData)
    {
        ProgramManager.WriteText(PublisherText, postData.Publisher, 1f);
        ProgramManager.WriteText(DateText, postData.Date, 1f);
        ProgramManager.WriteText(ContentText, postData.Content, 1f);
    }
}
