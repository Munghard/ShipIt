using System.Collections.Generic;
using UnityEngine;

public class Task
{
    public int Id;
    public string Name;
    public string Description;
    public float Difficulty;
    public float TimeToComplete;
    public Specialty Specialty;
    public float Progress;
    public Project Project;
    public Game Game;
    public string Status;
    public int Priority;
    public List<Worker> Workers = new();
    public bool CanStart;

    public float WindowX, WindowY, WindowW, WindowH;

    public Task(string name, string description, float difficulty, Specialty specialty, float timeToComplete, Project project, string status, int priority)
    {
        Id = Random.Range(1, 10000);
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Specialty = specialty;
        TimeToComplete = timeToComplete;
        Project = project;
        Game = project?.Game;
        Status = status;
        Priority = priority;
        Progress = 0;
    }

    public Vector2 GetWindowCenter()
    {
        return new Vector2(WindowX + WindowW / 2f, WindowY + WindowH / 2f);
    }

    public void Update()
    {
        float deltaTime = Time.deltaTime;
        if (Status == "completed" || Status == "failed") return;

        foreach (var worker in Workers)
        {
            float multiplier = 1f;

            if (worker.Specialty.Name == Specialty.Name || worker.Specialty.Name == "General")
                multiplier = 1.5f;
            else if (worker.Specialty.Name == "Management" && Specialty.Name != "General")
                multiplier = 0.2f;

            Progress += (multiplier * worker.Efficiency * deltaTime) / TimeToComplete;
        }

        if (Status == "pending" && Workers.Count > 0)
            Status = "in progress";

        if (Progress >= 100f && Status != "completed")
            Complete();
    }

    public void AssignWorker(Worker worker)
    {
        if (!Workers.Contains(worker))
        {
            Workers.Add(worker);
            worker.AssignTask(this);
        }
    }

    public void RemoveWorker(Worker worker)
    {
        if (Workers.Contains(worker))
        {
            Workers.Remove(worker);
            worker.RemoveFromTask();
        }
    }

    public void Complete()
    {
        Status = "completed";
        Game.textPop.New("Task completed!", GetWindowCenter(), Color.yellow);

        for (int i = Workers.Count - 1; i >= 0; i--)
        {
            var worker = Workers[i];
            worker.TaskCompleted(this);
            RemoveWorker(worker);
        }

        Project.CompleteTask(this);
    }
}
