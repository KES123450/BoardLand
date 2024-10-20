using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    private int _cardNumber;
    private bool _isVolatile;
    public Card(int number, bool isVolatile)
    {
        _cardNumber = number;
        _isVolatile = isVolatile;
    }

    public bool IsVolatile => _isVolatile;
    public int CardNumber => _cardNumber;
    

}
