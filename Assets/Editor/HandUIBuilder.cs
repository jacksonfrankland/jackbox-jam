using TMPro;
using UnityEditor;
using UnityEngine;

public static class HandUIBuilder
{
    private const float CardWidth = 1f;
    private const float CardHeight = 1.4f;
    private const float CardSpacing = 0.2f;
    private const float ButtonWidth = 1.6f;
    private const float ButtonHeight = 0.6f;
    private const float ButtonGap = 0.3f;

    [MenuItem("GameObject/UI/Hand UI (Cards)", false, 10)]
    public static void CreateHandUI()
    {
        var handUIGO = new GameObject("HandUI", typeof(HandUI));
        Undo.RegisterCreatedObjectUndo(handUIGO, "Create Hand UI");
        var handUI = handUIGO.GetComponent<HandUI>();

        var fitToBounds = Object.FindFirstObjectByType<CamaraFitToBounds>();
        if (fitToBounds && fitToBounds.Target && fitToBounds.Target.Value)
        {
            handUI.BoardRoot = fitToBounds.Target.Value;
        }

        var totalCardsWidth = Hand.HandSize * CardWidth + (Hand.HandSize - 1) * CardSpacing;
        var halfCardsWidth = totalCardsWidth / 2f;

        var cardSlots = new CardUI[Hand.HandSize];
        for (var i = 0; i < Hand.HandSize; i++)
        {
            var x = -halfCardsWidth + CardWidth / 2f + i * (CardWidth + CardSpacing);
            cardSlots[i] = CreateCardSlot(handUIGO.transform, i, new Vector3(x, 0f, 0f));
        }
        handUI.CardSlots = cardSlots;

        var dealX = -halfCardsWidth - ButtonWidth / 2f - ButtonGap;
        handUI.DealButtonCollider = CreateWorldButton("DealButton", "Deal", handUIGO.transform, new Vector3(dealX, 0f, 0f));

        var confirmX = halfCardsWidth + ButtonWidth / 2f + ButtonGap;
        handUI.ConfirmMulliganButtonCollider = CreateWorldButton("ConfirmMulliganButton", "Confirm Mulligan", handUIGO.transform, new Vector3(confirmX, 0f, 0f));

        var selectedForklift = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Forklift>() : null;
        if (selectedForklift)
        {
            handUI.TargetForklift = selectedForklift;
        }

        Selection.activeGameObject = handUIGO;
    }

    private static CardUI CreateCardSlot(Transform parent, int index, Vector3 localPosition)
    {
        var go = new GameObject($"CardSlot_{index}", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(CardUI));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;

        var spriteRenderer = go.GetComponent<SpriteRenderer>();
        ConfigureBackgroundSprite(spriteRenderer, CardWidth, CardHeight, 10);

        var collider = go.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(CardWidth, CardHeight);

        var label = CreateCenteredLabel(go.transform, "", CardWidth, CardHeight);
        label.color = Color.black;

        var cardUI = go.GetComponent<CardUI>();
        cardUI.Background = spriteRenderer;
        cardUI.Label = label;
        cardUI.Collider = collider;

        Undo.RegisterCreatedObjectUndo(go, "Create Card Slot");
        return cardUI;
    }

    private static Collider2D CreateWorldButton(string name, string label, Transform parent, Vector3 localPosition)
    {
        var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;

        var spriteRenderer = go.GetComponent<SpriteRenderer>();
        ConfigureBackgroundSprite(spriteRenderer, ButtonWidth, ButtonHeight, 10);

        var collider = go.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(ButtonWidth, ButtonHeight);

        var text = CreateCenteredLabel(go.transform, label, ButtonWidth, ButtonHeight);
        text.color = Color.black;

        Undo.RegisterCreatedObjectUndo(go, "Create World Button");
        return collider;
    }

    private static void ConfigureBackgroundSprite(SpriteRenderer spriteRenderer, float width, float height, int sortingOrder)
    {
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = new Vector2(width, height);
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private static TextMeshPro CreateCenteredLabel(Transform parent, string text, float width, float height)
    {
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(parent, false);
        textGO.transform.localPosition = Vector3.zero;

        var label = textGO.AddComponent<TextMeshPro>();
        var rectTransform = (RectTransform)label.transform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(width, height);

        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 0.5f;
        label.fontSizeMax = 10f;
        label.sortingOrder = 11;
        return label;
    }
}
