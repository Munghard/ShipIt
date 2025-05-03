using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Project
{
    public string Name;
    public string Description;
    public float Difficulty;
    public float Duration;
    public float Progress;
    public float ReputationNeeded;
    public float ReputationGain;
    public List<Task> Tasks = new();
    public int StartingTasks;
    public string Status;
    public int Id;
    public float LowestPriority;
    public float StartDuration;
    public float Pay;

    public float WindowX, WindowY, WindowW, WindowH;
    public Texture2D Icon;

    public Game Game;

    public System.Action<float> OnProgressChanged;
    public System.Action<float> OnDurationChanged;
    public System.Action<List<Task>> OnTasksChanged;
    public System.Action<Task> OnNewTask;
    public System.Action<string> OnStatusChanged;
    public System.Action<Project> OnCompleted;
    public System.Action<Project> OnFailed;

    public Project(Game game, string name, string description, float difficulty, float duration, float? pay = null)
    {
        this.Game = game;
        Id = UnityEngine.Random.Range(1, 10000);
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Duration = duration;
        ReputationNeeded = (difficulty - 1) * 500;
        ReputationGain = (difficulty) * 100;
        StartDuration = duration;
        Pay = pay ?? difficulty * 1000;
        Status = "pending";
        Icon = game.iconManager.GetIcon(32).SheetTexture;
        CreateTasks();
    }
    private void SetStatus(string status)
    {
        Status = status.ToLower();
        OnStatusChanged?.Invoke(status);
    }
    private void CreateTasks()
    {
        List<Task> allTasks = TaskLibrary.GetMainTasks().ToList();

        // Set max task count proportional to Difficulty
        int maxTasks = Mathf.Clamp((int)Difficulty * 2, 1, allTasks.Count); // e.g. 2–10 tasks

        int tasksCreated = 0;

        foreach (var def in allTasks.OrderBy(_ => UnityEngine.Random.value))
        {
            if (tasksCreated >= maxTasks)
                break;

            var task = new Task(def.Name, def.Description, def.Difficulty, def.Specialty, def.TimeToComplete, this, "pending", Tasks.Count + 1);
            AddTask(task);
            tasksCreated++;
        }
    }


    public void AddTask(Task task)
    {
        Tasks.Add(task);
        StartingTasks++;
        TasksChanged();
        OnNewTask?.Invoke(task);
        OnTasksChanged?.Invoke(Tasks);
    }

    public void RemoveTask(Task task)
    {
        Tasks.Remove(task);
        TasksChanged();
        OnTasksChanged?.Invoke(Tasks);
    }

    public void TasksChanged()
    {
        float minPriority = float.MaxValue;
        foreach (var task in Tasks)
        {
            if (task.Status != "completed" && task.Priority < minPriority)
            {
                minPriority = task.Priority;
                task.CanStart = true;
            }
            else
            {
                task.CanStart = false;
            }
        }
        LowestPriority = minPriority;
    }

    public void CompleteTask(Task task)
    {
        //Game.textPop.New("Task completed!", GetWindowCenter(), Color.yellow);

        foreach (var worker in Game.Workers)
        {
            int workerCount = Mathf.Max(1, task.Workers.Count);
            worker.Stress -= task.Difficulty / workerCount;
            worker.Stress = Mathf.Max(0, worker.Stress);
        }

        if (task.Status == "completed")
        {
            Progress += (task.Difficulty / Tasks.Count) * 100f;
        }

        TasksChanged();
        OnTasksChanged?.Invoke(Tasks);
    }

    public void FailProject()
    {
        SetStatus("failed");
        Game.textPop.New("Project failed...", GetWindowCenter(), Color.red);

        foreach (var task in Tasks)
        {
            task.Fail();
            Game.textPop.New("Task failed!", task.GetWindowCenter(), Color.red);

            foreach (var worker in task.Workers)
            {
                worker.RemoveFromTask();
            }
        }

        foreach (var worker in Game.Workers)
        {
            worker.IncreaseStress(50);
        }


        Game.SpendMoney(Pay / 4);
        Game.ReduceReputation(ReputationGain / 4);

        OnFailed?.Invoke(this);
    }
    public void TurnInProject()
    {
        // logic for randomizing submission success
        var roll = UnityEngine.Random.value * 100;
        if (Progress >= roll)
        {
            // get paid
            // get rep
            CompleteProject();
        }
        else
        {
            // dont get paid // get a fine 25%
            // suffer rep damage 25%
            FailProject();
        }
    }
    public void CompleteProject()
    {
        Game.textPop.New("Project completed!", GetWindowCenter(), Color.green);

        foreach (var worker in Game.Workers)
        {
            worker.DecreaseStress(50);
        }

        Game.GainMoney(Pay);
        Game.GainReputation(ReputationGain);
        
        SetStatus("paid out");

        OnCompleted?.Invoke(this);
    }
    public void UpdateProject()
    {
        if (Status != "completed" && Status != "failed" && Status != "paid out")
        {
            Duration -= Time.deltaTime;
            OnDurationChanged?.Invoke(Duration);
            if (Duration <= 0)
                FailProject();
        }

        if (Status == "pending")
            SetStatus("in progress");

        float totalProgress = 0f;
        foreach (var task in Tasks)
        {
            totalProgress += task.Progress;
        }

        Progress = Tasks.Count > 0 ? (totalProgress / (Tasks.Count * 100f)) * 100f : 0f;
        OnProgressChanged?.Invoke(Progress);

        if (Progress >= 100f && Status != "completed" && Status != "paid out")
        {
            SetStatus("completed");
            Game.CompleteProject(this);
        }
    }

    public Vector2 GetWindowCenter()
    {
        return new Vector2(WindowX + WindowW / 2f, WindowY + WindowH / 2f);
    }
}
