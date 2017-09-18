using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditorLayout : MonoBehaviour
{
    public const float WidthCheck = 1000f;

    public Vector2 DeckButtonsLandscapePosition {
        get { return new Vector2(-377.5f, 0f); }
    }

    public Vector2 DeckButtonsPortraitPosition {
        get { return new Vector2(-50f, -(GetComponent<RectTransform>().rect.height + 110f)); }
    }

    public RectTransform deckButtons;

    #if UNITY_ANDROID && !UNITY_EDITOR
    void Start()
    {
        if (GetComponent<RectTransform>().rect.width < WidthCheck)
            deckButtons.anchoredPosition = DeckButtonsPortraitPosition;
    }

    void OnRectTransformDimensionsChange()
    {
        if (GetComponent<RectTransform>().rect.width < WidthCheck)
            deckButtons.anchoredPosition = DeckButtonsPortraitPosition;
        else
            deckButtons.anchoredPosition = DeckButtonsLandscapePosition;
    }
    #endif

}
