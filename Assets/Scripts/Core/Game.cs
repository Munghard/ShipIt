using Assets.Scripts.Utils;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    public List<Project> Projects = new();
    public List<Project> AvailableProjects = new();
    public List<Worker> Workers = new();
    public List<Worker> WorkersForHire = new();
    public float Money = 1000f;

    public float Reputation = 50f;

    public float TimeScale = 1f;
    public float ScaledDeltaTime;
    public bool Paused = false;

    public TextPop textPop;
    public IconManager iconManager;
    public WorkerGenerator workerGenerator;

    public System.Action<Project> OnNewProject;
    
    public System.Action<Worker> OnNewWorker;
    public System.Action<Worker> OnWorkerAssigned;
    public System.Action<Worker> OnWorkerFreed;
    
    public System.Action<float> OnMoneyChanged;

    public System.Action<float> OnReputationChanged;

    public System.Action<float> OnTimeScaleChanged;

    public System.Action<List<Project>> OnProjectsChanged;

    public System.Action<List<Project>> OnAvailableProjectsChanged;
    public System.Action<List<Worker>> OnAvailableWorkersChanged;

    PlayerInput PlayerInput;
    public Game()
    {
        PlayerInput = new(this);
        textPop = new TextPop();
        iconManager = new IconManager();
        workerGenerator = new WorkerGenerator();
        Load();
        Debug.Log("New game instance created");
    }
    public void NewGame()
    {
        Projects = new List<Project>();
        Workers = new List<Worker>();
        AvailableProjects = new List<Project>();
        WorkersForHire = new List<Worker>();
        SetMoney(1000);
        SetReputation(50);

        RollWorkersForHire(3);
        RollProjects(3);

        var project = new Project(this, "Starting project", "Your first project.", 1, 3000);
        var worker = new Worker("Willy Worker",workerGenerator.GetRandomPortrait(), Specialty.Get("General"), 1, 100, project, this);
        AddWorker(worker);
        AddProject(project);

        textPop.New("New game started!", Vector2.zero, Color.white);
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
            float difficulty = Random.Range(1, 5);
            float duration = Random.Range(6, 20) * 60;
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
    public void SetTimeScale(float scale)
    {
        TimeScale = Mathf.Clamp(scale, 0.125f, 16f);
        OnTimeScaleChanged?.Invoke(TimeScale);
    }

    public void TogglePaused()
    {
        Paused = !Paused;
        if (Paused)
        {
            Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
            textPop.New("Paused!", center, Color.white);
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
    }

    public void UpdateGame()
    {
        float dt = Time.deltaTime;
        
        ScaledDeltaTime = dt * TimeScale;

        Time.timeScale = TimeScale * 0.1f;
        PlayerInput.UpdateInput();

        if (Paused) return;

        foreach (var project in Projects)
        {
            project.UpdateProject();
            foreach (var task in project.Tasks)
                task.UpdateTask();
        }

        foreach (var worker in Workers)
            worker.UpdateWorker();
    }

}
