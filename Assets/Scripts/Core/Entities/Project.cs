using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Project
{
    public string Name;
    public string Description;
    public int Difficulty;
    public float Duration;
    public float Progress;
    public int ReputationNeeded;
    public int ReputationGain;
    public List<Task> Tasks = new();
    public int StartingTasks;
    public string Status;
    public int Id;
    public float LowestPriority;
    public float StartDuration;
    public int Pay;

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

    public Project(Game game, string name, string description, int difficulty, float duration, int? pay = null,float? startDuration = null, int? id = null,List<Task> tasks = null)
    {
        this.Game = game;
        Id = id?? UnityEngine.Random.Range(1, 10000);
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Duration = duration;
        ReputationNeeded = (int)game.GameConfig.ReputationNeededFromLevelFormula(difficulty);
        ReputationGain = game.GameConfig.ReputationGainOnProjectComplete(difficulty);
        StartDuration = startDuration ?? duration;
        Pay = pay ?? game.GameConfig.GetProjectPay(difficulty);
        Status = "pending";
        Icon = game.iconManager.GetIcon(32).SheetTexture;
        Tasks = new List<Task>(); // Initialize Tasks first
        var newTasks = tasks ?? CreateTasks();
        foreach (var task in newTasks)
        {
            AddTask(task);
        } 
    }

    

    private void SetStatus(string status)
    {
        Status = status.ToLower();
        OnStatusChanged?.Invoke(status);
    }
    private List<Task> CreateTasks()
    {
        //Debug.Log("Creating tasks");
        List<Task> allTasks = TaskLibrary.GetMainTasks().ToList();
        List<Task> createdTasks = new();

        int baseTasks = Mathf.RoundToInt(Mathf.Pow(Difficulty, Game.GameConfig.TasksAmountPowerCurve));
        int randomOffset = UnityEngine.Random.Range(-1, 2); // -1, 0, or +1
        int maxTasks = Mathf.Clamp(baseTasks + randomOffset, 1, allTasks.Count);


        foreach (var def in allTasks.Where(t=>t.Difficulty <= Difficulty).OrderBy(_ => UnityEngine.Random.value).Take(maxTasks))
        {
            var task = new Task(
                name: def.Name,
                description: def.Description,
                difficulty: def.Difficulty,
                specialty: def.Specialty,
                timeToComplete: def.TimeToComplete,
                project: this, // Safe if no logic in Task depends on a fully constructed Project
                game:Game,
                status: "pending",
                priority: createdTasks.Count + 1, // use local count instead
                progress: 0f
            );
            createdTasks.Add(task);
        }

        return createdTasks;
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
            if (task.Status != "completed" && task.Status != "failed" && task.Priority < minPriority)
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
        if (task.Priority == LowestPriority) Game.GainMoney(task.Difficulty * 100,"Bonus"); // Give BONUS
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
            worker.DecreaseStress(Game.GameConfig.StressDecreaseProjectComplete(Difficulty));
        }

        Game.GainMoney(Pay,"Project completion");
        Game.GainReputation(ReputationGain);
        
        SetStatus("paid out");

        OnCompleted?.Invoke(this);
        Game.RemoveProject(this);
    }
    public void UpdateProject(float simulatedTime)
    {
        if (Status != "completed" && Status != "failed" && Status != "paid out")
        {
            Duration -= Game.ScaledDeltaTime;
            OnDurationChanged?.Invoke(Duration);
            if (Duration <= 0)
                FailProject();
        }

        if (Status == "pending")
            SetStatus("in progress");

        //float totalProgress = 0f;
        //foreach (var task in Tasks)
        //{
        //    totalProgress += task.Progress;
        //}

        //Progress = Tasks.Count > 0 ? (totalProgress / (Tasks.Count * 100f)) * 100f : 0f;

        Progress = Tasks.Count > 0 ? (Tasks.Where(t => t.Progress >= 100f).Count() / (float)Tasks.Count) * 100f : 0f;

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
