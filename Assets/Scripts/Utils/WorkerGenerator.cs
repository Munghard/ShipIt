using System.Collections.Generic;
using System.Linq;
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


    List<Sprite> AllSprites = new List<Sprite>();
    List<Sprite> Sprites = new List<Sprite>();


    public void Load()
    {
        AllSprites = Resources.LoadAll<Sprite>("textures/Portraits").ToList();
        Sprites.AddRange(AllSprites); 
        Debug.Log("Portraits loaded: " + AllSprites.Count);
    }

    public Sprite GetRandomPortrait()
    {
        int index = Random.Range(0, Sprites.Count);
        Sprite selected = Sprites[index];
        Sprites.RemoveAt(index);
        return selected;
    }

    public string GenerateRandomName()
    {
        string first = firstNames[Random.Range(0, firstNames.Length)];
        string last = lastNames[Random.Range(0, lastNames.Length)];
        return first + " " + last;
    }

    
}
