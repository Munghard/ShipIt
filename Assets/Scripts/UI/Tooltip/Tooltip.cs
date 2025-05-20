using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.Tooltip
{
    public class Tooltip : VisualElement
    {
        private static Tooltip _activeTooltip;

        public static void RegisterTooltip(VisualElement root, VisualElement parent, List<VisualElement> visualElements)
        {
            parent.RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (_activeTooltip != null)
                {
                    _activeTooltip.RemoveFromHierarchy();
                    Debug.Log("Deleting previous tooltip");
                }

                _activeTooltip = new Tooltip();
                _activeTooltip.Create(root, parent, visualElements);
            });

            parent.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (_activeTooltip != null)
                {
                    _activeTooltip.RemoveFromHierarchy();
                    _activeTooltip = null;
                    Debug.Log("Deleting tooltip");
                }
            });
        }

        private VisualElement Create(VisualElement _Root, VisualElement parent, List<VisualElement> _visualElements)
        {
            Debug.Log($"Creating tooltip");
            var visualTree = Resources.Load<VisualTreeAsset>("UI/Tooltip");
            if (visualTree != null)
            {
                visualTree.CloneTree(this);  // Clone the UXML content into this VisualElement
            }
            else
            {
                Debug.LogError("UXML file not found!");
            }

            foreach (var visualElement in _visualElements)
            {
                this.Q<VisualElement>("tooltip").Add(visualElement);
            }

            _Root.Add(this);
            

            Vector2 worldPos = parent.worldBound.position;
            float width = parent.worldBound.width;
            float heigth = parent.worldBound.height;
            
            Vector2 localPos = this.parent.WorldToLocal(worldPos);

            this.style.position = Position.Absolute;
            this.style.left = localPos.x + width + 10f;
            this.style.top = localPos.y + 10f;
          


            return this;
        }
    }
}
