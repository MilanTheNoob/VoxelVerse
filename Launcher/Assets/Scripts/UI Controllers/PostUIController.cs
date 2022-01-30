using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PostUIController : MonoBehaviour
{
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI PublisherText;
    public TextMeshProUGUI DateText;

    [Space]

    public TextMeshProUGUI RepliesText;
    public TextMeshProUGUI ViewsText;

    [Space]

    public Button ParentButton;

    public void SetValues(PostData postData)
    {
        ProgramManager.WriteText(TitleText, postData.Title, 1f);
        ProgramManager.WriteText(PublisherText, postData.Items[0].Publisher, 1f);
        ProgramManager.WriteText(DateText, postData.Items[0].Date, 1f);

        ProgramManager.WriteText(RepliesText, (postData.Items.Count - 1).ToString(), 1f);
        ProgramManager.WriteText(ViewsText, postData.Views.ToString(), 1f);
    }

    public void OnClick()
    {
        Packet serPacket = new Packet();
        serPacket.Write((byte)ProgramManager.ActiveThread);
        serPacket.Write(transform.GetSiblingIndex());
        serPacket.Send(ClientPackets.addView);

        
    }
}
