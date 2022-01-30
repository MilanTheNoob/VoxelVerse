using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CommentUIController : MonoBehaviour
{
    public TextMeshProUGUI ContentText;
    public TextMeshProUGUI PublisherText;
    public TextMeshProUGUI DateText;

    public void SetValues(CommentData commentData)
    {
        ProgramManager.WriteText(ContentText, commentData.Content, 1f);

        ProgramManager.WriteText(PublisherText, commentData.Publisher, 1f);
        ProgramManager.WriteText(DateText, commentData.Date, 1f);
    }
}
