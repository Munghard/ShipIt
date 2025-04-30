using System.Collections.Generic;
using UnityEngine;

public class WorkerGenerator
{
    private readonly string[] firstNames = {
        "John", "Jane", "Alex", "Chris", "Sam",
        "Taylor", "Jordan", "Morgan", "Dylan", "Riley"
    };

    private readonly string[] lastNames = {
        "Smith", "Johnson", "Williams", "Brown", "Jones",
        "Miller", "Davis", "García", "Rodriguez", "Martínez"
    };

    private readonly List<Rect> portraitRects = new();
    private Texture2D texture;
    private const int PortraitWidth = 70;
    private const int PortraitHeight = 70;

    

    public void Load()
    {
        texture = Resources.Load<Texture2D>("textures/Portraits Packed (5x)");
        if (texture == null)
        {
            Debug.LogError("Portrait texture not found!");
            return;
        }

        for (int j = 0; j < 10; j++)
        {
            for (int i = 0; i < 20; i++)
            {
                var rect = new Rect(i * PortraitWidth, j * PortraitHeight, PortraitWidth, PortraitHeight);
                portraitRects.Add(rect);
            }
        }

        Debug.Log("Portraits loaded: " + portraitRects.Count);
    }

    public PortraitData GetRandomPortrait()
    {
        int index = Random.Range(0, portraitRects.Count);
        var rect = portraitRects[index];

        return new PortraitData
        {
            X = rect.x,
            Y = rect.y,
            Width = rect.width,
            Height = rect.height,
            Texture = texture
        };
    }

    public string GenerateRandomName()
    {
        string first = firstNames[Random.Range(0, firstNames.Length)];
        string last = lastNames[Random.Range(0, lastNames.Length)];
        return first + " " + last;
    }

    
}
