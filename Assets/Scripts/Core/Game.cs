using Assets.Scripts.Utils;
using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Data;
using System.Linq;
using UnityEngine.VFX;
using UnityEngine.UIElements;
using Assets.Scripts.UI;

public class Game
{
    public float SimulationTime { get; set; } = 3600;

    public List<Project> Projects = new();
    public List<Project> AvailableProjects = new();
    public List<Worker> Workers = new();
    public List<Worker> WorkersForHire = new();

    public List<Buyable> buyables = new();
    public List<Buyable> acquiredBuyables = new();


    public float Money = 0f;

    public float Reputation = 0f;

    public float TimeScale = 1f;
    public float ScaledDeltaTime = 1f;
    public bool Paused = false;

    public TextPop textPop;
    public IconManager iconManager;
    public WorkerGenerator workerGenerator;

    public System.Action<Project> OnNewProject;
    
    public System.Action<Worker> OnNewWorker;
    public System.Action<Worker> OnWorkerAssigned;
    public System.Action<Worker> OnWorkerFreed;
    public System.Action<List<Worker>> OnWorkersChanged;
    
    public System.Action<float> OnMoneyChanged;

    public System.Action<float> OnReputationChanged;

    public System.Action<float> OnTimeScaleChanged;

    public System.Action<List<Project>> OnProjectsChanged;

    public System.Action<List<Buyable>> OnAcquiredBuyablesChanged;

    public System.Action<List<Project>> OnAvailableProjectsChanged;

    public System.Action<List<Worker>> OnAvailableWorkersChanged;

    PlayerInput PlayerInput;
    UIManager UIManager;
    public Game(UIManager uIManager)
    {
        PlayerInput = new(this);
        textPop = new TextPop();
        iconManager = new IconManager();
        workerGenerator = new WorkerGenerator();
        Load();
        Debug.Log("New game instance created");
        UIManager = uIManager;
    }
    public void NewGame()
    {
        Projects = new List<Project>();
        Workers = new List<Worker>();
        AvailableProjects = new List<Project>();
        WorkersForHire = new List<Worker>();

        OnProjectsChanged?.Invoke(Projects);
        OnWorkersChanged?.Invoke(Workers);
        OnAvailableProjectsChanged?.Invoke(AvailableProjects);
        OnAvailableWorkersChanged?.Invoke(WorkersForHire);
        
        ScaledDeltaTime = Time.deltaTime * TimeScale;

        SetMoney(0);
        SetReputation(0);

        RollWorkersForHire(3);
        RollProjects(3);
        RollBuyables(3);

        var project = new Project(this, "First project", "Your entry into the working world.", 1, 1000, 100);
        var worker = new Worker("Willy Workman",workerGenerator.GetPortrait(133), Specialty.Get("General"), 1, 100, project, this);
        AddWorker(worker);
        AddProject(project);

        textPop.New("New game started!", Vector2.zero, Color.white);
    }

    private void RollBuyables(int num)
    {
        buyables = BuyableLibrary.GetBuyables();
    }

    public void SetReputation(float amount)
    {
        Reputation = amount;
        OnReputationChanged?.Invoke(Reputation);
    }
    public void ReduceReputation(float amount)
    {
        Reputation -= amount;
        OnReputationChanged?.Invoke(Reputation);
    }
    public void GainReputation(float amount)
    {
        Reputation += amount;
        OnReputationChanged?.Invoke(Reputation);
    }
    public bool HasReputation(float amount)
    {
        return Reputation >= amount;
    }

    public void SetMoney(float amount)
    {
        Money = amount;
        OnMoneyChanged?.Invoke(Money);
    }
    public void SpendMoney(float amount)
    {
        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
    }
    public void GainMoney(float amount)
    {
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }
    public bool HasMoney(float amount)
    {
        return Money >= amount;
    }
    public void Load()
    {
        iconManager.Load();
        workerGenerator.Load();
        textPop.New("Welcome to the game!", new Vector2(Screen.width / 2, Screen.height / 2), Color.white);
    }

    public void RollProjects(int num)
    {
        for (int i = 0; i < num; i++)
        {
            float difficulty = Random.Range(1, Project.DifficultyFromReputation(Reputation)); // should only roll projects you have rep for
            float duration = Random.Range(10, 20) * 60 * difficulty; // multiply time with difficulty
            float pay = (difficulty * 500) + Random.Range(50, 200);
            var project = new Project(this, "Project" + i, "Project description placeholder", difficulty, duration, pay);
            AvailableProjects.Add(project);
        }
        OnAvailableProjectsChanged?.Invoke(AvailableProjects);
    }

    public void RollWorkersForHire(int num)
    {
        for (int i = 0; i < num; i++)
        {
            float totalSkill = 0;
            foreach (var worker in Workers)
                totalSkill += worker.Skill;

            float averageSkill = Workers.Count > 0 ? totalSkill / Workers.Count : 0;
            float skill = Random.Range(1, averageSkill + 1);

            var newWorker = new Worker(
                workerGenerator.GenerateRandomName(),
                workerGenerator.GetRandomPortrait(),
                Specialty.GenerateRandomSpecialty(),
                skill,
                100f,
                null,
                this
            );

            WorkersForHire.Add(newWorker);
        }
        OnAvailableWorkersChanged?.Invoke(WorkersForHire);
    }
    public void RemoveWorkerFromHirePool(Worker worker)
    {
        WorkersForHire.Remove(worker);
        OnAvailableWorkersChanged?.Invoke(WorkersForHire);
    }
    public void RemoveProjectFromAvailableProjects(Project project)
    {
        AvailableProjects.Remove(project);
        OnAvailableProjectsChanged?.Invoke(AvailableProjects);
    }
    public void HalveTimeScale()
    {
        SetTimeScale(TimeScale * 0.5f);
    }
    public void DoubleTimeScale()
    {
        SetTimeScale(TimeScale * 2);
    }
        
    public void SetTimeScale(float scale)
    {
        TimeScale = Mathf.Clamp(scale, 0.125f, 16f);
        OnTimeScaleChanged?.Invoke(TimeScale);
    }
    public void PassTime(float timeToPass)
    {
        SimulationTime += timeToPass; // 6 h
        foreach (var project in Projects)
        {
            project.Duration -= timeToPass;
            project.OnDurationChanged?.Invoke(project.Duration);
        }
        foreach (var worker in Workers)
        {
            worker.DecreaseStress(timeToPass * worker.StressDecreasePerSecond);
        }
    }

    VisualElement PauseWindow;
    public void TogglePaused()
    {
        
        Paused = !Paused;
        if (Paused)
        {
            PauseWindow = Assets.Scripts.UI.PauseWindow.Create(UIManager.Root,()=> Paused = !Paused);
            //textPop.New("Paused!", Vector2.zero, Color.white);
        }
        else
        {
            if(PauseWindow != null) PauseWindow.RemoveFromHierarchy();
        }
    }

    public void CompleteProject(Project project)
    {
        Debug.Log($"Project {project.Name} completed!");
        textPop.New("Project completed!", project.GetWindowCenter(), Color.green);
        GainReputation(project.Difficulty * 100);
        RollWorkersForHire(3);
        RollProjects(3);
    }

    public void RemoveProject(Project project)
    {
        foreach (var task in project.Tasks.ToList())
        {
            project.RemoveTask(task);
        }
        Projects.Remove(project);
        OnProjectsChanged.Invoke(Projects);
    }

    public void AddProject(Project project)
    {
        Projects.Add(project);
        OnNewProject?.Invoke(project);
        OnProjectsChanged?.Invoke(Projects);
        Debug.Log($"{project.Name} added to active projects.");
    }

    public void AddWorker(Worker worker)
    {
        Workers.Add(worker);
        Debug.Log($"{worker.Name} added to team.");
        OnNewWorker?.Invoke(worker);
        OnWorkersChanged?.Invoke(Workers);
    }

    public void UpdateGame()
    {
        PlayerInput.UpdateInput();

        if (Paused) return;

        // Use deltaTime and TimeScale for your scaled time calculations
        ScaledDeltaTime = Time.deltaTime * TimeScale;

        // Update SimulationTime based on custom TimeScale
        SimulationTime += ScaledDeltaTime;  // Adjust time flow with TimeScale




        foreach (var project in Projects)
        {
            project.UpdateProject(SimulationTime);
            foreach (var task in project.Tasks)
                task.UpdateTask(SimulationTime);
        }

        foreach (var worker in Workers)
            worker.UpdateWorker(SimulationTime);
    }

    internal void AddBuyableToAcquired(Buyable buyable)
    {
        acquiredBuyables.Add(buyable);
        OnAcquiredBuyablesChanged?.Invoke(acquiredBuyables);
    }
}
