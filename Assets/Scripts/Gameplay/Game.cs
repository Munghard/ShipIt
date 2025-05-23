using Assets.Scripts.Utils;
using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Data;
using System.Linq;
using UnityEngine.UIElements;
using Assets.Scripts.UI.Window;
using System.Collections;
using Assets.Scripts.Core.Enums;
using Assets.Scripts.Core;
public class Game
{
    // TIME
    public float SimulationTime { get; set; } = 360; // in minutes
    public int SimulationDay { get; set; } = 0;
    public static readonly int secondsPerDay = 1440;
    public float GetCurrentHour => (SimulationTime / 60f) % 24f;
    private bool _lastBusinessOpen = true;
    public Vector2 BusinessHours;
    public bool BusinessOpen => IsWorkHours();
    public float TimeScale = 1f;
    public float ScaledDeltaTime = 1f;
    public bool Paused = false;
    // TIME

    public List<Project> Projects = new();
    public List<Project> AvailableProjects = new();
    public List<Worker> Workers = new();
    public List<Worker> WorkersForHire = new();

    public int MaximumAmountOfWorkers => GameConfig? Mathf.FloorToInt(GameConfig.MaximumAmountOfWorkers.Evaluate(Reputation)) : 1;

    public List<Buyable> buyables = new();
    public List<Buyable> acquiredBuyables = new();

    

    public int Money = 0;

    public int Reputation = 0;


    public TextPop textPop;
    public IconManager iconManager;
    public WorkerGenerator workerGenerator;

    public System.Action<Project> OnNewProject;
    
    public System.Action<Worker> OnNewWorker;
    public System.Action<Worker> OnWorkerAssigned;
    public System.Action<Worker> OnWorkerFreed;
    public System.Action<Worker> OnWorkerQuit;
    public System.Action<Worker> OnWorkerDied;

    public System.Action<List<Worker>> OnWorkersChanged;
    
    public System.Action<bool> OnBusinessOpenChanged;

    public System.Action<int> OnMoneyChanged;

    public System.Action<int> OnReputationChanged;

    public System.Action<float> OnTimeScaleChanged;
    public System.Action<int> OnNewDay;

    public System.Action<List<Project>> OnProjectsChanged;

    public System.Action<List<Buyable>> OnAcquiredBuyablesChanged;

    public System.Action<List<Project>> OnAvailableProjectsChanged;

    public System.Action<List<Worker>> OnAvailableWorkersChanged;

    public GameConfig GameConfig;

    public TraitManager TraitManager;

    PlayerInput PlayerInput;
    public UIManager UIManager;
    
    public Game(UIManager uIManager, GameConfig gameConfig)
    {
        GameConfig = gameConfig;
        PlayerInput = new(this);
        textPop = new TextPop(uIManager);
        iconManager = new IconManager();
        workerGenerator = new WorkerGenerator();
        Debug.Log("New game instance created");
        UIManager = uIManager;
        GameConfig = gameConfig;
        TraitManager = new TraitManager();
        Load();

        // gets set in setsimulationtime on load
        //SimulationTime = 1800; // 6h
        //currentDay = Mathf.FloorToInt(SimulationTime / secondsPerDay);

        BusinessHours = GameConfig.BusinessHours;

        ScaledDeltaTime = Time.deltaTime * TimeScale;

        OnBusinessOpenChanged +=(value)=> UpdateWorkersLocation(value?Location.WORK:Location.HOME);
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
        

        SetMoney(GameConfig.StartingMoney);
        SetReputation(GameConfig.StartingRep);

        RollWorkersForHire(GameConfig.RolledWorkersAmount);
        RollProjects(GameConfig.RolledProjectsAmount);
        RollBuyables(GameConfig.RolledBuyablesAmount);

        var project = new Project(this, "First project", "Your entry into the working world.", 1, 1000, 100);
        
        var _traits = GameConfig.StartingWorker.Traits.Select(t => TraitManager.GetTrait(t)).ToList(); 
        var worker = new Worker(GameConfig.StartingWorker.Name, workerGenerator.GetPortrait(GameConfig.StartingWorker.PortraitIndex), Specialty.Get(GameConfig.StartingWorker.Specialty), GameConfig.StartingWorker.Skill, GameConfig.StartingWorker.Level, null, this, traits: _traits);
        AddWorker(worker);
        AddProject(project);

        textPop.New("New game started!", Vector2.zero, Color.white);

    }
    public void UpdateBusinessStatus()
    {
        bool current = BusinessOpen;
        if (current != _lastBusinessOpen)
        {
            _lastBusinessOpen = current;
            ChangeBusinessStatus(current);
        }
    }
    private void ChangeBusinessStatus(bool isOpen)
    {
        if (isOpen)
        {
            AudioManager.Play("DoorBell");
            OnBusinessOpenChanged?.Invoke(true);
            Debug.Log("Business is now OPEN");
        }
        else
        {
            AudioManager.Play("Bell");
            OnBusinessOpenChanged?.Invoke(false);
            Debug.Log("Business is now CLOSED");
        }
    }

    private void UpdateWorkersLocation(Location location) // check traits, workaholics might not leave when closed
    {
        foreach (var worker in Workers.Where(w => w.Location != Location.HOSPITAL))
        {
            worker.SetWorkerLocation(location); // check traits, ie druggies might not come in at all and alcoholics might be late
        }
    }

    public bool HasSpaceForWorker()
    {
        return Workers.Count < MaximumAmountOfWorkers;
    }

    private void RollBuyables(int num)
    {
        buyables = BuyableLibrary.GetBuyables();
    }

    public void SetReputation(int amount)
    {
        Reputation = amount;
        OnReputationChanged?.Invoke(Reputation);
    }
    public void ReduceReputation(int amount)
    {
        Reputation -= amount;
        OnReputationChanged?.Invoke(Reputation);
        if (amount >= 5)
        {
            textPop.New($"-{amount}Rep", new Vector2(Screen.width / 2, Screen.height / 2), Color.magenta);
        }
    }
    public void GainReputation(int amount)
    {
        Reputation += amount;
        OnReputationChanged?.Invoke(Reputation);
        textPop.New($"+{amount}Rep", new Vector2(Screen.width / 2, Screen.height / 2), Color.magenta);
    }
    public bool HasReputation(int amount)
    {
        return Reputation >= amount;
    }

    public void SetMoney(int amount)
    {
        Money = amount;
        OnMoneyChanged?.Invoke(Money);
    }
    public void SpendMoney(int amount)
    {
        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
        textPop.New($"-{amount}$", new Vector2(Screen.width / 2, Screen.height / 2), Color.cyan);
        AudioManager.Play("SpendMoney");
    }
    public void GainMoney(int amount, string reason)
    {
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
        textPop.New($"{reason} +{amount}$", new Vector2(Screen.width / 2, Screen.height / 2), Color.cyan);
        AudioManager.Play("GetMoney");
    }
    public bool HasMoney(int amount)
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
        AvailableProjects = new List<Project>(); // clear old projects
        for (int i = 0; i < num; i++)
        {
            int difficulty = Random.Range(1, GameConfig.DifficultyFromReputation(Reputation)); // should only roll projects you have rep for
            float duration = GameConfig.GetProjectDuration(difficulty); // multiply time with difficulty
            int pay = GameConfig.GetProjectPay(difficulty);
            var project = new Project(this, "Project" + i, "Project description placeholder", difficulty, duration, pay);
            AvailableProjects.Add(project);
        }
        OnAvailableProjectsChanged?.Invoke(AvailableProjects);
    }

    public void RollWorkersForHire(int num)
    {
        WorkersForHire = new List<Worker>(); // clear old workers
        for (int i = 0; i < num; i++)
        {
            float totalSkill = 0;
            foreach (var worker in Workers)
                totalSkill += worker.Skill;

            float averageSkill = Workers.Count > 0 ? totalSkill / Workers.Count : 0;
            float skill = Random.Range(1, averageSkill + 1);
            int level = Mathf.Max(Reputation / GameConfig.ReputationToHiresLevelRatio, 1);

            var newWorker = new Worker(
                workerGenerator.GenerateRandomName(),
                workerGenerator.GetRandomPortrait(),
                Specialty.GenerateRandomSpecialty(),
                skill,
                level,
                null,
                this
            );
            for (int ii = 0; ii < level; ii++)
            {
                newWorker.Traits.Add(TraitManager.GetRandomTrait()); // add one trait every level
            }
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
        TimeScale = Mathf.Clamp(scale, 0.125f, 32f);
        OnTimeScaleChanged?.Invoke(TimeScale);
    }
    public void SetSimulationTime(float simulationTime)
    {
        SimulationTime = simulationTime;
        currentDay = Mathf.FloorToInt(SimulationTime / secondsPerDay);
        _lastBusinessOpen = !IsWorkHours();
        UpdateBusinessStatus(); // calls workers update location based off of time

        //var timeToPass = simulationTime - SimulationTime;
        //PassTime(timeToPass);
    }
    public void PassTime(float timeToPass)
    {
        UIManager.StartCoroutine(PassTimeCR(timeToPass));

        //SimulationTime += timeToPass; // 6 h
        //foreach (var project in Projects)
        //{
        //    project.Duration -= timeToPass;
        //    project.OnDurationChanged?.Invoke(project.Duration);
        //}
        //foreach (var worker in Workers)
        //{
        //    worker.DecreaseStress(timeToPass * GameConfig.GetStressDecreasePerSecond(worker.Skill));
        //}
    }
    IEnumerator PassTimeCR (float timeToPass){
        // should prbably disable input or something we'll see
        TimeScale = timeToPass;
        OnTimeScaleChanged?.Invoke(TimeScale);
        float targetTime = SimulationTime + timeToPass - 1;

        while (SimulationTime < targetTime)
        {
            yield return null;
        }
        Paused = true;
        ConfirmationWindow.Create(
            UIManager.Root,
            () => Paused = false,
            Message: $"{(timeToPass / 60f).ToString("F1")}h has passed.."
        );
        SetTimeScale(1);

    }
    VisualElement PauseWindow;
    public void TogglePaused()
    {
        Paused = !Paused;
        if (Paused)
        {
            PauseWindow = Assets.Scripts.UI.Window.PauseWindow.Create(UIManager.Root,()=> Paused = !Paused);
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
        GainReputation(GameConfig.ReputationGainOnProjectComplete(project.Difficulty));
        RollWorkersForHire(GameConfig.RolledWorkersAmount);
        RollProjects(GameConfig.RolledProjectsAmount);
    }

    public void RemoveProject(Project project)
    {
        foreach (var task in project.Tasks.ToList())
        {
            foreach (var worker in task.Workers.ToList())
            {
                task.RemoveWorker(worker); // remove all workers
            }
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
    internal void AddBuyableToAcquired(Buyable buyable)
    {
        acquiredBuyables.Add(buyable);
        OnAcquiredBuyablesChanged?.Invoke(acquiredBuyables);
    }

    internal void RemoveWorker(Worker worker)
    {
        Workers.Remove(worker);
        OnWorkersChanged?.Invoke(Workers);
    }

    public bool IsWorkHours()
    {
        return GetCurrentHour >= BusinessHours.x && GetCurrentHour <= BusinessHours.y;
    }
    // timer stuff
    private float repTimer = 0f;
    private float businessCheckTimer = 0f;
    private float businessCheckInterval = 1f; // Check every n second
    private int currentDay = 0;
    public void UpdateGame()
    {
        // INPUT
        PlayerInput.UpdateInput();

        if (Paused) return;


        // TIME

        // Use deltaTime and TimeScale for your scaled time calculations
        ScaledDeltaTime = Time.deltaTime * TimeScale;

        // Update SimulationTime based on custom TimeScale
        SimulationTime += ScaledDeltaTime;  // Adjust time flow with TimeScale

        
        SimulationDay = Mathf.FloorToInt(SimulationTime / secondsPerDay);
        //Debug.Log($"simulation time: {SimulationTime} Day: {SimulationDay} / {currentDay}");
        if (SimulationDay > currentDay)
        {
            currentDay = SimulationDay;
            OnNewDay?.Invoke(currentDay);
            Debug.Log($"New day: {currentDay}");
        }

        // Throttle business status checks to not update every frame
        businessCheckTimer += ScaledDeltaTime;
        if (businessCheckTimer >= businessCheckInterval)
        {
            businessCheckTimer = 0f;
            UpdateBusinessStatus();
        }

        //REP LOSS
        repTimer += ScaledDeltaTime;
        while (repTimer >= GameConfig.RepLoseFrequencySeconds) // lose 1 rep every 5 seconds
        {
            repTimer = 0f;
            if(Reputation > 0) ReduceReputation(GameConfig.RepLostPerTick);
        }


        //UPDATE
        foreach (var project in Projects)
        {
            project.UpdateProject(SimulationTime);
            foreach (var task in project.Tasks)
                task.UpdateTask(SimulationTime);
        }

        foreach (var worker in Workers.ToList())
            worker.UpdateWorker(SimulationTime);
    }


}
