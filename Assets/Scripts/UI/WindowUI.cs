using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WindowUI : VisualElement
{

    public WindowUI(string headerText,VisualElement icon, List<VisualElement> elements, Vector2 position, bool draggable = true)
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

        this.Q<Label>("lblHeader").text = headerText; // Set the header text

        // icon
        var header = this.Q<VisualElement>("header");

        icon.style.marginBottom = 8;
        icon.style.marginLeft = 8;
        icon.style.marginRight = 8;
        icon.style.marginTop = 8;
        header.Insert(0,icon);

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
        if (minimize != null)
        {
            minimize.clicked += () =>
            {
                container.style.display = container.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
                minimize.text = minimize.text == "-" ? "+" : "-";
            };
        }

        if(draggable) Draggable.MakeDraggable(this);
    }

}
