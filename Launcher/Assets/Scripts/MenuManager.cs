using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class MenuManager : MonoBehaviour
{
    public MenuClass[] Menus;

    void Awake()
    {
        for (int i = 0; i < Menus.Length; i++)
        {
            if (i == 0)
            {
                Menus[i].MenuTransform.gameObject.SetActive(true);
                LeanTween.alpha(Menus[i].MenuTransform, 1f, 0f);
            }
            else
            {
                Menus[i].MenuTransform.gameObject.SetActive(false);
                LeanTween.alpha(Menus[i].MenuTransform, 0f, 0f);
            }
        }
    }

    public void OpenMenu(int index) 
    {
        for (int i = 0; i < Menus.Length; i++)
        {
            if (i == index)
            {
                Menus[i].MenuChildren = Menus[i].MenuTransform.GetComponentsInChildren<TextMeshProUGUI>();
                Menus[i].MenuTransform.gameObject.SetActive(true);

                LeanTween.alpha(Menus[i].MenuTransform, 0f, 0f);
                LeanTween.alpha(Menus[i].MenuTransform, 1f, Menus[i].MoveSpeed).setDelay(Menus[i].MoveSpeed / 2f);

                LeanTween.moveLocalY(Menus[i].MenuTransform.gameObject, Menus[i].MenuTransform.anchoredPosition.y + Menus[i].MoveAmount, 0f);
                LeanTween.moveLocalY(Menus[i].MenuTransform.gameObject, Menus[i].MenuTransform.anchoredPosition.y - Menus[i].MoveAmount, Menus[i].MoveSpeed).setEaseInExpo();

                for (int j = 0; j < Menus[i].MenuChildren.Length; j++)
                {
                    TweenIn(Menus[i].MenuChildren[j], Menus[i].MoveSpeed);
                }

                for (int j = 0; j < Menus[i].WritingTexts.Length; j++) { ProgramManager.WriteText(Menus[i].WritingTexts[j].WritingText, 
                    Menus[i].WritingTexts[j].PossibleValues[Random.Range(0, Menus[i].WritingTexts[j].PossibleValues.Length)], Menus[i].WritingTexts[j].Time); }
            }
            else if (Menus[i].MenuTransform.gameObject.activeSelf)
            {
                int num = i;
                LeanTween.alpha(Menus[i].MenuTransform, 0f, 0.2f).setOnComplete(() => { Menus[num].MenuTransform.gameObject.SetActive(false); });

                LeanTween.moveLocalY(Menus[i].MenuTransform.gameObject, Menus[i].MenuTransform.anchoredPosition.y - Menus[i].MoveAmount, Menus[i].MoveSpeed).setEaseInExpo().
                    setOnComplete(() => { LeanTween.moveLocalY(Menus[num].MenuTransform.gameObject, Menus[num].MenuTransform.anchoredPosition.y + Menus[num].MoveAmount, 0f); });

                for (int j = 0; j < Menus[i].MenuChildren.Length; j++)
                {
                    try { TweenOut(Menus[i].MenuChildren[j], Menus[i].MoveSpeed); } catch { }
                }

                for (int j = 0; j < Menus[i].WritingTexts.Length; j++) { ProgramManager.WriteText(Menus[i].WritingTexts[j].WritingText, "", Menus[i].WritingTexts[j].Time); }
            }
        }
    }

    void TweenIn(TextMeshProUGUI fadingText, float time)
    {
        var _color = fadingText.color;
        _color.a = 0;
        fadingText.color = new Color32(255, 255, 255, 0);

        LeanTween.value(fadingText.gameObject, _color.a, 1f, time).setOnUpdate((float _value) =>
        { _color.a = _value; fadingText.color = _color; }).setEaseInExpo();
    }

    void TweenOut(TextMeshProUGUI fadingText, float time)
    {
        var _color = fadingText.color;
        _color.a = 1;
        fadingText.color = new Color32(255, 255, 255, 0);

        LeanTween.value(fadingText.gameObject, _color.a, 0f, time).setOnUpdate((float _value) =>
        { _color.a = _value; fadingText.color = _color; }).setEaseInExpo();
    }

    [System.Serializable]
    public class MenuClass
    {
        public RectTransform MenuTransform;
        [HideInInspector] public TextMeshProUGUI[] MenuChildren;

        [Space]

        public MovingObjectClass[] MovingObjects;
        public WritingTextClass[] WritingTexts;

        [Space]

        public float MoveAmount;
        public float MoveSpeed;

        [System.Serializable]
        public class MovingObjectClass
        {
            public RectTransform MovingObject;

            [Space]

            public float MoveAmount = 100;
            public float Time = 0.5f;

            [Space]

            public MoveTypeEnum MoveType;
            public enum MoveTypeEnum { MoveFromLeft, MoveFromRight, MoveFromTop, MoveFromBottom, RotateFromLeft, RotateFromRight }
        }

        [System.Serializable]
        public class WritingTextClass
        {
            public TextMeshProUGUI WritingText;
            public string[] PossibleValues;

            [Space]

            public float Time = 0.7f;
        }
    }
}
