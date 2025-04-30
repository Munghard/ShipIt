using UnityEngine;

public class ProjectWrapper : ScriptableObject
{
    public Project project { get; set; }
    public ProjectWrapper(Project project)
    {
        this.project = project;
    }
}