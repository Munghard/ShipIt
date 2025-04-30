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

    public float WindowX, WindowY, WindowW, WindowH;

    public Project Project;
    public Task Task;
    public Game Game;

    public Worker(string name, Specialty specialty, float skill, float efficiency, Project project, Game game)
    {
        Id = GenerateId();
        Name = name;
        Specialty = specialty;
        Skill = (int)skill;
        Efficiency = efficiency;
        Project = project;
        Game = game;
        Health = MaxHealth;
        Stress = 0;
        Level = 1;
        Xp = 0;
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

    public void AssignTask(Task task)
    {
        Game.textPop.New("Assigned!", GetWindowCenter(), Color.green);
        Project = task.Project;
        Task = task;
        Status = "working";
        Occupied = true;
    }

    public void RemoveFromTask()
    {
        Task = null;
        Status = "idle";
        Occupied = false;
        Game.textPop.New("Freed!", GetWindowCenter(), Color.green);
    }

    public void TaskCompleted(Task task)
    {
        TasksCompleted++;
        DecreaseStress(task.Difficulty * 5);
        GainXp(task.Difficulty * task.TimeToComplete);
        RemoveFromTask();
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
    }

    private void LevelUp()
    {
        Skill += 1;
        Level += 1;
        NextXp = Mathf.Floor(100f * Mathf.Pow(Level, 1.5f));
        Game.textPop.New($"Level up! {Level}", GetWindowCenter(), Color.yellow);
    }

    public void IncreaseStress(float amount)
    {
        Stress = Mathf.Min(MaxStress, Stress + amount);
        if (amount > 5)
            Game.textPop.New($"Stress + {amount}", GetWindowCenter(), Color.red);
    }

    public void DecreaseStress(float amount)
    {
        Stress = Mathf.Max(0, Stress - amount);
        if (amount > 5)
            Game.textPop.New($"Stress - {amount}", GetWindowCenter(), Color.green);
    }

    public void IncreaseEfficiency(float amount)
    {
        Efficiency = Mathf.Min(100, Efficiency + amount);
    }

    public void DecreaseEfficiency()
    {
        Efficiency = Mathf.Max(1, 99 - Stress);
    }

    public void IncreaseHealth(float amount)
    {
        Health = Mathf.Min(MaxHealth, Health + (amount / 100f));
        if (amount > 5)
            Game.textPop.New($"Health + {amount}", GetWindowCenter(), Color.green);
    }

    public void DecreaseHealth(float amount)
    {
        Health -= amount;
        if (amount > 5)
            Game.textPop.New($"Health - {amount}", GetWindowCenter(), Color.red);
    }

    public void Death()
    {
        if (Status == "dead") return;

        Task?.RemoveWorker(this);
        RemoveFromTask();
        Health = 0;
        Status = "dead";
        Game.textPop.New("Died!", GetWindowCenter(), Color.white);
    }

    public void Update()
    {
        float deltaTime = Time.deltaTime;
        if (Status == "dead") return;

        Occupied = Task != null;
        Status = Occupied ? "working" : "idle";

        if (Occupied && Task.Status != "completed")
        {
            float stressIncrease = (Task.Difficulty / Skill) * deltaTime;
            IncreaseStress(stressIncrease);
            DecreaseEfficiency();
        }
        else
        {
            DecreaseStress(Skill * deltaTime);
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
}
