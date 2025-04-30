using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WindowUI : VisualElement
{
    private Vector2 dragOffset;  // Offset between mouse position and element position
    private bool isDragging = false;

    public WindowUI(string header, List<VisualElement> elements)
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

        this.Q<Label>("lblHeader").text = header; // Set the header text

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

        Draggable.MakeDraggable(this);
        //Debug.Log("Draggable: "+draggable);
    }

}
