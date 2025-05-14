using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Window
{
    public class PauseWindow
    {
        public static VisualElement Create(VisualElement Parent, System.Action OkCallback)
        {
            var elements = new List<VisualElement>();
            
            WindowUI window = null;
            VisualElement blocker = null;

            void DestroyWindow()
            {
                window?.RemoveFromHierarchy();
                blocker?.RemoveFromHierarchy();
            }
            var yesButton = new Button(() => {
                OkCallback?.Invoke();
                DestroyWindow();
            }) { text = "Continue" };
            

            var buttonContainer = new VisualElement();
            buttonContainer.Add(yesButton);
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.Center;
            buttonContainer.style.marginTop = 10;
            buttonContainer.style.marginBottom = 10;
            buttonContainer.style.marginLeft = 10;
            buttonContainer.style.marginRight = 10;

            elements.Add(buttonContainer);

            window = new WindowUI("Paused", elements, new Vector2(0, 0), null, false, false, false);
            
            window.style.position = Position.Absolute;
            window.style.top = Length.Percent(50);
            window.style.left = Length.Percent(50);
            window.style.transformOrigin = new TransformOrigin(50, 50); // center point
            window.style.translate = new Translate(-50, -50, 0); // offset back by half

            blocker = new VisualElement();
            blocker.style.position = Position.Absolute;
            blocker.style.top = 0;
            blocker.style.left = 0;
            blocker.style.right = 0;
            blocker.style.bottom = 0;
            blocker.style.backgroundColor = new Color(0, 0, 0, 0.8f); // transparent
            blocker.pickingMode = PickingMode.Position; // blocks mouse input

            Parent.Add(blocker);
            blocker.Add(window);


            return blocker;
        }
    }
}
