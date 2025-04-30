using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPop
{

    public System.Action<string,Color> OnNewTextPop;

    public void New(string text, Vector2 position, Color color)
    {
        OnNewTextPop?.Invoke(text, color);
    }

}
