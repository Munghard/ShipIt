using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WindowUI : VisualElement
{

    public WindowUI(string headerText,Sprite sprite, List<VisualElement> elements, Vector2 position)
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
        Image icon = new()
        {
            sprite = sprite,
            style = 
            {
                width = 32,
                height = 32,
                marginLeft = 9,
                marginRight = 9,
                marginTop = 9,
                marginBottom = 9,
            },
        };

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

        Draggable.MakeDraggable(this);
    }

}
