using System.Collections.Generic;

public class Specialty
{
    public string Name;
    public string Description;

    public Specialty(string name, string description)
    {
        Name = name;
        Description = description;
    }
    public static Specialty Get(string name)
    {
        foreach (var specialty in specialties)
        {
            if (specialty.Name == name)
            {
                return specialty;
            }
        }
        return null;
    }
    public static List<Specialty> specialties = new()
    {
        new Specialty("General", "Generalist with a wide range of skills."),
        new Specialty("Management", "Expert in project management and leadership."),
        new Specialty("Coding", "Skilled in programming and software development."),
        new Specialty("UI", "Specialist in user interface design."),
        new Specialty("Testing", "Expert in software testing and quality assurance."),
        new Specialty("Deployment", "Specialist in software deployment and release management.")
    };
    public static Specialty GenerateRandomSpecialty()
    {
        return specialties[UnityEngine.Random.Range(0, specialties.Count)];
    }
}