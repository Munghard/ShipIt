using UnityEngine;

public class WorkerWrapper : ScriptableObject
{
    public Worker worker { get; set; }
    public WorkerWrapper(Worker worker) 
    {
        this.worker = worker;
    }
}
