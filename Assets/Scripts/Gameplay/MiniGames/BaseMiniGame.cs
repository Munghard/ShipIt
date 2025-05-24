using Assets.Scripts.UI.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Gameplay.MiniGames
{
    public abstract class BaseMiniGame : IMiniGame
    {
        public abstract string Title { get; }

        public abstract string Description { get; }

        public virtual List<VisualElement> CreateUIElements()
        {
            return new List<VisualElement>();
        }
        protected Button yesButton, noButton;
        protected Coroutine gameCoroutine;
        protected VisualElement window, blocker;
        protected abstract float GetResult();

        protected virtual void Cleanup(UIManager uiManager, Game game)
        {
            if (gameCoroutine != null)
                uiManager.StopCoroutine(gameCoroutine);

            window.RemoveFromHierarchy();
            blocker.RemoveFromHierarchy();
            game.Paused = false; // unpause
        }
        public virtual IEnumerator Run(Game game, UIManager uiManager,int difficulty, Action<float> onSuccess, Action onCancel)
        {
            game.Paused = true;

            blocker = CreateBlocker();
            uiManager.Root.Add(blocker);

            var elements = CreateUIElements();

            yesButton = new Button(() => {
                onSuccess?.Invoke(GetResult());
                Cleanup(uiManager,game);
            })
            { text = "Yes" };
            yesButton.SetEnabled(false); // Initially disabled

            noButton = new Button(() => {
                onCancel?.Invoke();
                Cleanup(uiManager,game);
            })
            { text = "No" };
            noButton.SetEnabled(true);

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.Center;
            buttonContainer.Add(yesButton);
            buttonContainer.Add(noButton);
            elements.Add(buttonContainer);

            window = new WindowUI(Title, elements, Vector2.zero, null, false, false, false);
            window.style.position = Position.Absolute;
            window.style.top = Length.Percent(50);
            window.style.left = Length.Percent(50);
            window.style.transformOrigin = new TransformOrigin(50, 50);
            window.style.translate = new Translate(-50, -50, 0);

            uiManager.Root.Add(window);

            gameCoroutine = uiManager.StartCoroutine(MiniGameCoroutine(difficulty));

            yield return gameCoroutine;
        }

        protected virtual VisualElement CreateBlocker()
        {
            var blocker = new VisualElement();
            blocker.style.position = Position.Absolute;
            blocker.style.top = 0;
            blocker.style.left = 0;
            blocker.style.right = 0;
            blocker.style.bottom = 0;
            blocker.style.backgroundColor = new Color(0, 0, 0, 0.8f);
            blocker.pickingMode = PickingMode.Position;
            return blocker;
        }
        protected void EnableConfirmButton()
        {
            yesButton.SetEnabled(true);
            yesButton.Focus();
            noButton.SetEnabled(false);
        }

        protected abstract IEnumerator MiniGameCoroutine(int difficulty);
    }
}
