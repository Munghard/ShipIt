using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.Tooltip
{
    public class Tooltip : VisualElement
    {
        public Tooltip(List<VisualElement> _visualElements) 
        {
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
                this.Add(visualElement);
            }
        }
    }
}
