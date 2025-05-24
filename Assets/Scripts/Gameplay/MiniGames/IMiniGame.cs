using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Assets.Scripts.Gameplay.MiniGames
{
    public interface IMiniGame
    {
        string Title { get; }
        string Description { get; }
        List<VisualElement> CreateUIElements();
        IEnumerator Run(Game game, UIManager uiManager, int difficulty, Action<float> onSuccess, Action onCancel);
    }
}
