using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountSuggestionUIController : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI TagText;
    public Image ProfileImage;
    public Image VerifiedTick;

    SocialAccountData accountData;

    public void SetValues(SocialAccountData accountData)
    {
        this.accountData = accountData;

        ProfileImage.sprite = accountData.Icon;
        VerifiedTick.gameObject.SetActive(accountData.IsVerified);

        ProgramManager.WriteText(NameText, accountData.Name, 1f);
        ProgramManager.WriteText(TagText, accountData.Tag, 1f);
    }
}

public class SocialAccountData
{
    public string Name;
    public string Tag;

    public Sprite Icon;
    public bool IsVerified;
}
