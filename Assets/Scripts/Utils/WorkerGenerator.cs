using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkerGenerator
{
    private readonly string[] firstNames = {
    "John", "Jane", "Alex", "Chris", "Sam",
    "Taylor", "Jordan", "Morgan", "Dylan", "Riley",
    "Emily", "Ryan", "Lily", "Ethan", "Sophie",
    "Ben", "Avery", "Max", "Charlie", "Olivia",
    "Sophia", "Zoe", "Lucas", "Isaac", "Maya",
    "Levi", "Chloe", "Wyatt", "Madison", "Aidan",
    "Grace", "Henry", "Ella", "Jack", "Mason",
    "Nina", "Leo", "Liam", "Scarlett", "Isaiah",
    "Mila", "James", "Amelia", "Eli", "Luca"
    };
    private readonly string[] lastNames = {
    "Smith", "Johnson", "Williams", "Brown", "Jones",
    "Miller", "Davis", "García", "Rodriguez", "Martínez",
    "Taylor", "Anderson", "Thomas", "Jackson", "White",
    "Harris", "Clark", "Lewis", "Walker", "Young",
    "Allen", "King", "Scott", "Green", "Baker",
    "Adams", "Nelson", "Hill", "Ramirez", "Carter",
    "González", "Lopez", "Wilson", "Perez", "Roberts",
    "Morris", "King", "Nguyen", "Mitchell", "Campbell",
    "Parker", "Evans", "Edwards", "Collins", "Stewart"
    };


    List<Sprite> AllSprites = new List<Sprite>();
    List<Sprite> Sprites = new List<Sprite>();


    public void Load()
    {
        AllSprites = Resources.LoadAll<Sprite>("textures/Portraits").ToList();
        Sprites.AddRange(AllSprites); 
        Debug.Log("Portraits loaded: " + AllSprites.Count);
    }
    public Sprite GetPortrait(int index)
    {
        return AllSprites[index];
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

    public int GetPortraitIndex(Sprite portrait)
    {
        return AllSprites.IndexOf(portrait);
    }
}
