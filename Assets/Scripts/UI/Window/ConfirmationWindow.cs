﻿using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
namespace Assets.Scripts.UI.Window
{
    internal class ConfirmationWindow
    {
        public static WindowUI Create(VisualElement Parent, System.Action OkCallback, System.Action NoCallback = null, string Message = null,bool canConfirm = true)
        {
            var elements = new List<VisualElement>();
            var label = new Label("Are you sure you want to proceed?");
            if(Message != null)
            {
                label.text = Message;
            }
            elements.Add(label);

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
            }) { text = "Yes" };
            yesButton.SetEnabled(canConfirm);

            var noButton = new Button(() => {
                NoCallback?.Invoke();
                DestroyWindow();
            }) { text = "No" };
            yesButton.focusable = true;
            noButton.focusable = true;

            if (yesButton.enabledSelf)
            {
                yesButton.schedule.Execute(() => yesButton.Focus()).ExecuteLater(1);
            }
            else
            {
                noButton.schedule.Execute(() => noButton.Focus()).ExecuteLater(1);
            }

            var buttonContainer = new VisualElement();
            buttonContainer.Add(yesButton);
            if(NoCallback != null)
            {
                buttonContainer.Add(noButton);
            }
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.Center;
            buttonContainer.style.marginTop = 10;
            buttonContainer.style.marginBottom = 10;
            buttonContainer.style.marginLeft = 10;
            buttonContainer.style.marginRight = 10;

            elements.Add(buttonContainer);

            window = new WindowUI("Confirmation", elements, new Vector2(0, 0), icon: UIManager.CreateFAIcon("circle-question"), false, false);
            
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
            Parent.Add(window);

            return window;
        }
    }
}
