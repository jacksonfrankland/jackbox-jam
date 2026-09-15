using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Hand
{
    public const int HandSize = 5;

    private static readonly CardType[] Pool =
    {
        CardType.MoveForward1,
        CardType.MoveForward2,
        CardType.MoveForward3,
        CardType.MoveBackward,
        CardType.RotateClockwise,
        CardType.RotateAnticlockwise
    };

    public CardType[] Cards = new CardType[HandSize];
    public bool HasMulliganed;

    public void Deal()
    {
        for (var i = 0; i < HandSize; i++)
        {
            Cards[i] = DrawRandomCard();
        }
        HasMulliganed = false;
    }

    public void Mulligan(IEnumerable<int> cardIndicesToRedraw)
    {
        if (HasMulliganed) return;
        foreach (var index in cardIndicesToRedraw)
        {
            if (index < 0 || index >= HandSize) continue;
            Cards[index] = DrawRandomCard();
        }
        HasMulliganed = true;
    }

    private static CardType DrawRandomCard()
    {
        return Pool[Random.Range(0, Pool.Length)];
    }
}
