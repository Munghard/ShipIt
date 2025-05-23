using System.Collections.Generic;
using UnityEngine;

public class Task
{
    public int Id;
    public string Name;
    public string Description;
    public int Difficulty;
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

    public System.Action<float> OnProgressChanged;
    public System.Action<string> OnStatusChanged;
    public System.Action<List<Worker>> OnWorkersChanged;
    public System.Action<Task> OnCompleted;

    public Task(string name, string description, int difficulty, Specialty specialty, float timeToComplete, Project project,Game game, string status, int priority, float progress)
    {
        Id = Random.Range(1, 10000);
        Name = name;
        Progress = progress;
        Description = description;
        Difficulty = difficulty;
        Specialty = specialty;
        TimeToComplete = timeToComplete;
        Project = project;
        Game = game;
        Status = status;
        Priority = priority;
        Progress = progress;
    }

    public Vector2 GetWindowCenter()
    {
        return new Vector2(WindowX + WindowW / 2f, WindowY + WindowH / 2f);
    }

    
    private void SetStatus(string status)
    {
        Status = status.ToLower();
        OnStatusChanged?.Invoke(status);
    }

    public void AssignWorker(Worker worker)
    {

        if (!Workers.Contains(worker))
        {
            Workers.Add(worker);
            worker.AssignTask(this);
        }
        OnWorkersChanged?.Invoke(Workers);
    }

    public void RemoveWorker(Worker worker)
    {
        if (Workers.Contains(worker))
        {
            Workers.Remove(worker);
            worker.RemoveFromTask();
        }
        OnWorkersChanged?.Invoke(Workers);
    }

    public void Complete()
    {
        SetStatus("completed");
        Game.textPop.New("Task completed!", GetWindowCenter(), Color.yellow);

        for (int i = Workers.Count - 1; i >= 0; i--)
        {
            var worker = Workers[i];
            worker.TaskCompleted(this);
            RemoveWorker(worker);
        }
        OnCompleted?.Invoke(this);
        Project.CompleteTask(this);
    }
    public void Fail()
    {
        SetStatus("failed");
        Game.textPop.New("Task failed..", GetWindowCenter(), Color.red);

        for (int i = Workers.Count - 1; i >= 0; i--)
        {
            var worker = Workers[i];
            RemoveWorker(worker);
        }
    }
    public void UpdateTask(float simulationTime)
    {
        float deltaTime = Game.ScaledDeltaTime;
        if (Status == "completed" || Status == "failed") return;

        foreach (var worker in Workers)
        {
            float multiplier = 1f;

            if (worker.Specialty.Name == Specialty.Name || worker.Specialty.Name == "General")
                multiplier = Game.GameConfig.SpecialtySameMultiplier;
            else if (worker.Specialty.Name == "Management" && Specialty.Name != "General")
                multiplier = Game.GameConfig.SpecialtyDifferentMultiplier;

            Progress += (worker.baseWorkSpeed * multiplier * worker.Efficiency * deltaTime * Game.GameConfig.TaskProgressMultiplier) / TimeToComplete;
            OnProgressChanged?.Invoke(Progress);
        }

        if (Status == "pending" && Workers.Count > 0)
            SetStatus("in progress");

        if (Progress >= 100f && Status != "completed")
            Complete();
    }
}