using Assets.Scripts.UI;
using Assets.Scripts.Utils;
using Assets.Scripts.Core.Enums;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI.Window;

public class Worker
{
    public long Id;
    public string Name;
    public float Stress;
    public float Health;
    public float MaxHealth = 100f;
    public float MaxStress = 100f;
    public float Happiness;
    public float MaxHappiness = 100f;
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
    public int HiringCost;

    public Location Location;

    public Sprite Portrait;

    public float WindowX, WindowY, WindowW, WindowH;

    public Project Project;
    public Task Task;
    public Game Game;

    public System.Action<float> OnHealthChanged;
    public System.Action<float> OnStressChanged;
    public System.Action<float> OnHappinessChanged;
    public System.Action<float> OnEfficiencyChanged;
    public System.Action<float> OnXpChanged;
    public System.Action<float> OnNextXpChanged;
    public System.Action<float> OnLevelChanged;
    public System.Action<Task> OnTaskChanged;
    public System.Action<string> OnStatusChanged;
    public System.Action<int> OnSkillChanged;
    public System.Action<Location> OnLocationChanged;
    
    public System.Action<List<Trait>> OnTraitsChanged;

    public List<Trait> Traits = new List<Trait>();

    public float baseWorkSpeed = 1f;
    public float baseLearnSpeed = 1f;
    public float baseHealSpeed = 1f;

    public Worker(string name,Sprite portrait, Specialty specialty, float skill, int level, Project project, Game game,float? health = null,float? stress = null, float? xp = null, long? id = null,float? happiness = null, Location? location = null,List<Trait> traits = null)
    {
        Id = id ?? GenerateId();
        Name = name;
        Portrait = portrait;
        Specialty = specialty;
        Skill = (int)skill;
        Efficiency = 100f - stress ?? 0;
        Happiness = happiness ?? 100f;
        Project = project;
        Game = game;
        Health = health ?? MaxHealth; // if health null set to max
        Stress = stress ?? 0;
        Level = level; // if level null set to 1
        Xp = xp ?? 0;
        NextXp = Game.GameConfig.EvalWorkerXpCurve(Level);
        Status = "idle";
        HiringCost = Game.GameConfig.GetHiringCost(Level);
        location = location??= Location.WORK;
        traits = traits ?? new List<Trait>();
    }

    private long GenerateId()
    {
        return long.Parse(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() + Random.Range(0, 999).ToString());
    }
    public void SetWorkerLocation(Location location)
    {
        Location = location;
        OnLocationChanged?.Invoke(location);
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
        DecreaseStress(Game.GameConfig.GetStressDecreaseOnTaskComplete(task.Difficulty));
        GainXp(Game.GameConfig.GetXpGainFromTask(task.Difficulty,task.TimeToComplete));
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
        NextXp = Game.GameConfig.EvalWorkerXpCurve(Level);
        OnLevelChanged?.Invoke(Level);
        OnNextXpChanged?.Invoke(NextXp);
        var trait = AddRandomTrait(); // add one trait every level
        Game.textPop.New($"Level up! {Level}", GetWindowCenter(), Color.yellow);
        
        Game.Paused = true;
        ConfirmationWindow.Create(
            Game.UIManager.Root,
            () => Game.Paused = false,
            Message: $"{Name} has been promoted to level:{Level}\n" +
            $"New trait: {trait.TraitName}\n" +
            $"{trait.Description}"
        );
        
        //AudioManager.Play("LevelUp");
    }

    private Trait AddRandomTrait()
    {
        var trait = Game.TraitManager.GetRandomTrait();
        Traits.Add(trait);
        OnTraitsChanged?.Invoke(Traits);
        return trait;
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
    public void IncreaseHappiness(float amount)
    {
        Happiness = Mathf.Min(MaxHappiness, Happiness + amount);
        if (amount > 5)
        {
            Game.textPop.New($"{Name} Happiness + {amount}", GetWindowCenter(), Color.red);
        }
        OnHappinessChanged?.Invoke(Happiness);
    }

    public void DecreaseHappiness(float amount)
    {
        Happiness = Mathf.Max(0, Happiness - amount);
        if (amount > 5)
        {
            Game.textPop.New($"{Name} Happiness - {amount}", GetWindowCenter(), Color.green);
        }
        OnHappinessChanged?.Invoke(Happiness);
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
        Health = Mathf.Min(MaxHealth, Health + amount);
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
    private void GoToHospital()
    {
        var hospitalBill = Random.Range(100, 1000); // roll bill

        SetWorkerLocation(Location.HOSPITAL);
        ConfirmationWindow.Create(
            Game.UIManager.Root,
            () =>
            {
                Game.Paused = false;

                if(Game.HasMoney(hospitalBill))
                {
                    Game.SpendMoney(hospitalBill);
                }
            },
            () =>
            {
                var deathRoll = Random.value;
                if(deathRoll < Game.GameConfig.DeathChance)
                {
                    Death();
                }
            },
            Message: $"{Name} has collapsed from stress and was taken to a hospital.\n" +
            $"Pay the hospital bill or {Name} might die.\n",
            canConfirm:Game.HasMoney(hospitalBill)
        );

    }
    public void Death()
    {
        if (Status == "dead") return;

        Task?.RemoveWorker(this);
        RemoveFromTask();
        Health = 0;
        SetStatus("dead");
        Game.textPop.New("Died!", GetWindowCenter(), Color.white);
        Portrait = Game.workerGenerator.GetPortrait(98);
        Game.OnWorkerDied?.Invoke(this);
        Game.OnWorkersChanged?.Invoke(Game.Workers);
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
    public bool CanWork()
    {
        return Status != "dead" && Location == Location.WORK; // not dead and at work
    }
    public bool IsWorkHours()
    {
        return Game.GetCurrentHour >= Game.BusinessHours.x && Game.GetCurrentHour <= Game.BusinessHours.y;
    }

    public void UpdateWorker(float simulationTime)
    {
        float deltaTime = Game.ScaledDeltaTime;
        if (Status == "dead") return;

        Occupied = Task != null;
        Status = Occupied ? "working" : "idle";

        if (Task != null && !IsWorkHours())
        {
            Task.RemoveWorker(this);
        }

        if (Occupied && Task.Status != "completed")
        {
            float effectiveSkill = Mathf.Max(Skill / 2f, 1f); // Prevent division by <1
            float specialtyStressModifier = Task.Specialty.Name == Specialty.Name ? 1f : Task.Difficulty * 2f;

            float stressIncrease = (Task.Difficulty / effectiveSkill) * deltaTime * Game.GameConfig.StressIncreaseMultiplier * specialtyStressModifier;

            IncreaseStress(stressIncrease);
            DecreaseEfficiency();
        }
        else
        {
            DecreaseStress(Game.GameConfig.GetStressDecreasePerSecond(Skill) * deltaTime);
        }

        if (Stress >= MaxStress)
        {
            DecreaseHappiness(deltaTime * Game.GameConfig.HappinessLossMultiplier);
            DecreaseHealth(deltaTime * Game.GameConfig.HealthLossMultiplier);
            if (Health <= 0) GoToHospital();
        }
        else
        {
            IncreaseHappiness(deltaTime * Game.GameConfig.HappinessGainMultiplier);
        }


        if (Happiness <= 0)
        {
            RemoveFromTask();
            Game.RemoveWorker(this);
            Game.textPop.New($"{Name} left the company!", GetWindowCenter(), Color.red);
            Game.OnWorkerQuit(this);
        }
        

        if (Stress <= 0 && Health < MaxHealth)
        {
            IncreaseHealth(deltaTime / Game.GameConfig.HealthGainMultiplier * baseHealSpeed); // regenerate health
        }

    }


}
