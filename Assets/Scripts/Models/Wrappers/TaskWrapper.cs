using UnityEngine;

public class TaskWrapper : ScriptableObject
{
    public Task task { get; set; }
    public TaskWrapper(Task task)
    {
        this.task = task;
    }
}
