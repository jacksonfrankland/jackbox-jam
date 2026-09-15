using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class HandUIBuilder
{
    [MenuItem("GameObject/UI/Hand UI (Cards)", false, 10)]
    public static void CreateHandUI()
    {
        EnsureEventSystem();

        var canvasGO = new GameObject("HandCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Hand UI");
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var handUIGO = new GameObject("HandUI", typeof(HandUI));
        handUIGO.transform.SetParent(canvasGO.transform, false);
        var handUI = handUIGO.GetComponent<HandUI>();

        var panelGO = new GameObject("HandPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = (RectTransform)panelGO.transform;
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0, 40);
        panelRect.sizeDelta = new Vector2(700, 180);

        var layout = panelGO.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        var cardSlots = new CardUI[Hand.HandSize];
        for (var i = 0; i < Hand.HandSize; i++)
        {
            cardSlots[i] = CreateCardSlot(panelGO.transform, i);
        }
        handUI.CardSlots = cardSlots;

        var dealButton = CreateButton("DealButton", "Deal", canvasGO.transform, new Vector2(-260, 240));
        UnityEventTools.AddPersistentListener(dealButton.onClick, handUI.DealHand);

        var mulliganButton = CreateButton("MulliganButton", "Confirm Mulligan", canvasGO.transform, new Vector2(260, 240));
        UnityEventTools.AddPersistentListener(mulliganButton.onClick, handUI.ConfirmMulligan);

        var selectedForklift = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Forklift>() : null;
        if (selectedForklift)
        {
            handUI.TargetForklift = selectedForklift;
        }

        Selection.activeGameObject = canvasGO;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>()) return;
        var eventSystemGO = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create Event System");
    }

    private static CardUI CreateCardSlot(Transform parent, int index)
    {
        var go = new GameObject($"CardSlot_{index}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(CardUI));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(120, 160);

        var image = go.GetComponent<Image>();
        image.color = Color.white;

        var label = CreateStretchedLabel(go.transform, "");
        label.color = Color.black;

        var cardUI = go.GetComponent<CardUI>();
        cardUI.Label = label;
        cardUI.Background = image;

        Undo.RegisterCreatedObjectUndo(go, "Create Card Slot");
        return cardUI;
    }

    private static Button CreateButton(string name, string label, Transform parent, Vector2 anchoredPosition)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(180, 50);

        var text = CreateStretchedLabel(go.transform, label);
        text.color = Color.black;

        Undo.RegisterCreatedObjectUndo(go, "Create Button");
        return go.GetComponent<Button>();
    }

    private static TextMeshProUGUI CreateStretchedLabel(Transform parent, string text)
    {
        var textGO = new GameObject("Label", typeof(RectTransform));
        textGO.transform.SetParent(parent, false);
        var textRect = (RectTransform)textGO.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var label = textGO.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 20;
        return label;
    }
}
