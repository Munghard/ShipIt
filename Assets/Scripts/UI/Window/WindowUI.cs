using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.Window
{

    public class WindowUI : VisualElement
    {
        public Coroutine CurrentAlertCoroutine;
        public System.Action<string> OnHeaderChanged;

        private VisualElement _iconElement;           // the icon element that was added to the header

        public WindowUI(string headerText, List<VisualElement> elements, Vector2 position, VisualElement icon = null, bool draggable = true,bool renameable= false, bool canMinimize = true)
        {

            // Load the UXML file and clone it
            var visualTree = Resources.Load<VisualTreeAsset>("UI/WindowUI");
            if (visualTree != null)
            {
                visualTree.CloneTree(this);  // Clone the UXML content into this VisualElement
            }
            else
            {
                Debug.LogError("UXML file not found!");
            }


            this.style.left = position.x;
            this.style.top = position.y;

            var lblHeader = this.Q<Label>("lblHeader");
            lblHeader.style.marginLeft = 30;
            lblHeader.style.marginRight = 30;
            lblHeader.style.alignSelf = Align.Center;
            lblHeader.text = headerText; // Set the header text


            // icon
            var header = this.Q<VisualElement>("header");

            _iconElement = icon;

            if(_iconElement != null)
            {
                _iconElement.style.marginBottom = 8;
                _iconElement.style.marginLeft = 8;
                _iconElement.style.marginRight = 8;
                _iconElement.style.marginTop = 8;
                header.Insert(0, _iconElement);
            }

            if (renameable)
            {
                this.Q<Label>("lblHeader").RemoveFromHierarchy(); // Set the header text
                TextField textField = new TextField();
                textField.name = "headerText";
                textField.value = headerText;
                textField.style.marginLeft = 5;
                textField.style.marginRight = 5;
                textField.style.alignSelf = Align.Center;
                header.Insert(1, textField);
                textField.RegisterValueChangedCallback(evt =>
                {
                    OnHeaderChanged?.Invoke(evt.newValue);
                });

            }

        

            // Optionally, get references to specific elements in the UXML and add logic
            var closeButton = this.Q<Button>("closeButton");
            if (closeButton != null)
            {
                closeButton.clicked += () => Debug.Log("Close Button Clicked");
            }

        

            var container = this.Q<VisualElement>("content"); // Set the header text

            if (container != null)
            {
                foreach (var element in elements)
                {
                    container.Add(element); // Add each element to the container
                }
            }
            else
            {
                Debug.LogError("Container not found!");
            }
            var minimize = this.Q<Button>("btnMin"); // Set the header text

            if(canMinimize)
            {
                if (minimize != null)
                {
                    minimize.clicked += () =>
                    {
                        container.style.display = container.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
                        minimize.text = minimize.text == "-" ? "+" : "-";
                    };
                }
            }
            else
            {
                minimize.RemoveFromHierarchy();
            }
            if (header.childCount == 1) header.style.justifyContent = Justify.Center; // center if only one element in header

            if (draggable) Draggable.MakeDraggable(this);
        }

        public void SetIcon(VisualElement newIcon)
        {
            var header = this.Q<VisualElement>("header");
            if (_iconElement != null)
            {
                header.Remove(_iconElement);
            }
            _iconElement = newIcon;
            header.Insert(0, _iconElement);
        }
    }
}