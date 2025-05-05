using UnityEngine;

public class Worker
{
    public long Id;
    public string Name;
    public float Stress;
    public float Health;
    public float MaxHealth = 100f;
    public float MaxStress = 100f;
    public int Level;
    public float Xp;
    public float NextXp;
    public int Skill;
    public Specialty Specialty;
    public float Efficiency;
    public bool Occupied;
    public string Status;
    public int TasksCompleted;
    public int TasksFailed;
    public int HiringCost => Level * 100;

    public Sprite Portrait;

    public float WindowX, WindowY, WindowW, WindowH;

    public Project Project;
    public Task Task;
    public Game Game;

    public System.Action<float> OnHealthChanged;
    public System.Action<float> OnStressChanged;
    public System.Action<float> OnEfficiencyChanged;
    public System.Action<float> OnXpChanged;
    public System.Action<float> OnNextXpChanged;
    public System.Action<float> OnLevelChanged;
    public System.Action<Task> OnTaskChanged;
    public System.Action<string> OnStatusChanged;
    public System.Action<int> OnSkillChanged;

    public Worker(string name,Sprite portrait, Specialty specialty, float skill, float efficiency, Project project, Game game,float? health = null,float? stress = null,int? level = null, float? xp = null, long? id = null)
    {
        Id = id ?? GenerateId();
        Name = name;
        Portrait = portrait;
        Specialty = specialty;
        Skill = (int)skill;
        Efficiency = efficiency;
        Project = project;
        Game = game;
        Health = health ?? MaxHealth; // if health null set to max
        Stress = stress ?? 0;
        Level = level ?? 1; // if level null set to 1
        Xp = xp ?? 0;
        NextXp = Mathf.Floor(100f * Mathf.Pow(Level, 1.5f));
        Status = "idle";
    }

    private long GenerateId()
    {
        return long.Parse(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() + Random.Range(0, 999).ToString());
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
    public void AssignTask(Task task)
    {
        Game.textPop.New(Name + " assigned!", GetWindowCenter(), Color.green);
        Project = task.Project;
        Task = task;
        SetStatus("working");
        Occupied = true;
        OnTaskChanged?.Invoke(task);
        Game.OnWorkerAssigned?.Invoke(this);
    }

    public void RemoveFromTask()
    {
        Task = null;
        SetStatus("idle");
        Occupied = false;
        Game.textPop.New(Name + " freed!", GetWindowCenter(), Color.green);
        OnTaskChanged?.Invoke(null);
        Game.OnWorkerFreed?.Invoke(this);
    }

    public void TaskCompleted(Task task)
    {
        TasksCompleted++;
        DecreaseStress(task.Difficulty * 5);
        GainXp(task.Difficulty * task.TimeToComplete);
        //RemoveFromTask(); // already called in task
    }

    public void GainXp(float amount)
    {
        Xp += amount;
        Game.textPop.New($"Xp + {amount}", GetWindowCenter(), Color.yellow);
        while (Xp >= NextXp)
        {
            Xp -= NextXp;
            LevelUp();
        }
        OnXpChanged?.Invoke(Xp);
    }

    private void LevelUp()
    {
        Skill += 1;
        Level += 1;
        NextXp = Mathf.Floor(100f * Mathf.Pow(Level, 1.5f));
        Game.textPop.New($"Level up! {Level}", GetWindowCenter(), Color.yellow);
        OnLevelChanged?.Invoke(Level);
        OnNextXpChanged?.Invoke(NextXp);
    }

    public void IncreaseStress(float amount)
    {
        Stress = Mathf.Min(MaxStress, Stress + amount);
        if (amount > 5)
        {
            Game.textPop.New($"{Name} Stress + {amount}", GetWindowCenter(), Color.red);
        }
        OnStressChanged?.Invoke(Stress);
    }

    public void DecreaseStress(float amount)
    {
        Stress = Mathf.Max(0, Stress - amount);
        if (amount > 5)
        {
            Game.textPop.New($"{Name} Stress - {amount}", GetWindowCenter(), Color.green);
        }
        OnStressChanged?.Invoke(Stress);
    }

    public void IncreaseEfficiency(float amount)
    {
        Efficiency = Mathf.Min(100, Efficiency + amount);
        OnEfficiencyChanged?.Invoke(Efficiency);
    }

    public void DecreaseEfficiency()
    {
        Efficiency = Mathf.Max(1, 99 - Stress);
        OnEfficiencyChanged?.Invoke(Efficiency);
    }

    public void IncreaseHealth(float amount)
    {
        Health = Mathf.Min(MaxHealth, Health + (amount / 100f));
        if (amount > 5)
        {
            Game.textPop.New($"{Name} Health + {amount}", GetWindowCenter(), Color.green);
        }
        OnHealthChanged?.Invoke(Health);
    }

    public void DecreaseHealth(float amount)
    {
        Health -= amount;
        if (amount > 5)
        {
            Game.textPop.New($"{Name} Health - {amount}", GetWindowCenter(), Color.red);
        }
        OnHealthChanged?.Invoke(Health);
    }

    public void Death()
    {
        if (Status == "dead") return;

        Task?.RemoveWorker(this);
        RemoveFromTask();
        Health = 0;
        SetStatus("dead");
        Game.textPop.New("Died!", GetWindowCenter(), Color.white);
    }

    public void UpdateWorker(float simulationTime)
    {
        float deltaTime = Game.ScaledDeltaTime;
        if (Status == "dead") return;

        Occupied = Task != null;
        Status = Occupied ? "working" : "idle";

        if (Occupied && Task.Status != "completed")
        {
            float stressIncrease = (Task.Difficulty / Skill) * deltaTime * 0.5f;
            IncreaseStress(stressIncrease);
            DecreaseEfficiency();
        }
        else
        {
            DecreaseStress(Skill * deltaTime * 0.5f);
        }

        if (Stress >= MaxStress)
        {
            DecreaseHealth(deltaTime);
            if (Health <= 0) Death();
        }

        if (Stress <= 0 && Health < MaxHealth)
        {
            IncreaseHealth(deltaTime / 10f);
        }
    }

    internal void IncreaseSkill(int amount)
    {
        Skill += amount;
        OnSkillChanged(Skill);
    }
    internal void DecreaseSkill(int amount)
    {
        Skill -= amount;
        OnSkillChanged(Skill);
    }
}
