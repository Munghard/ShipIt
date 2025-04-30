using System.Collections.Generic;
using UnityEngine;

public class Game
{
    public List<Project> Projects = new();
    public List<Project> AvailableProjects = new();
    public List<Worker> Workers = new();
    public List<Worker> WorkersForHire = new();
    public float Money = 1000f;
    public float TimeScale = 1f;
    public float ScaledDeltaTime;
    public bool Paused = false;

    public TextPop textPop;
    public IconManager iconManager;
    public WorkerGenerator workerGenerator;

    public System.Action<Project> OnNewProject;
    public System.Action<Task> OnNewTask;
    public System.Action<Worker> OnNewWorker;

    public Game()
    {
        textPop = new TextPop();
        iconManager = new IconManager();
        workerGenerator = new WorkerGenerator();
        Load();
        Debug.Log("New game instance created");
    }

    public void Load()
    {
        iconManager.Load();
        workerGenerator.Load();
        RollWorkersForHire(3);
        RollProjects(3);
        textPop.New("Welcome to the game!", new Vector2(Screen.width / 2, Screen.height / 2), Color.white);
    }

    public void RollProjects(int num)
    {
        for (int i = 0; i < num; i++)
        {
            float difficulty = Random.Range(1, 5);
            float duration = Random.Range(6, 20) * 60;
            float pay = (difficulty * 500) + Random.Range(50, 200);
            var project = new Project(this, "ProjectName_placeholder", "Project description placeholder", difficulty, duration, pay);
            AvailableProjects.Add(project);
        }
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
                Specialty.GenerateRandomSpecialty(),
                skill,
                100f,
                null,
                this
            );

            WorkersForHire.Add(newWorker);
        }
    }

    public void SetTimeScale(float scale)
    {
        TimeScale = Mathf.Clamp(scale, 0.125f, 16f);
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
        RollWorkersForHire(3);
        RollProjects(3);
    }

    public void RemoveProject(Project project)
    {
        Projects.Remove(project);
    }

    public void AddProject(Project project)
    {
        Projects.Add(project);
        OnNewProject?.Invoke(project);
        Debug.Log($"{project.Name} added to active projects.");
    }

    public void AddWorker(Worker worker)
    {
        Workers.Add(worker);
        Debug.Log($"{worker.Name} added to team.");
        OnNewWorker?.Invoke(worker);
    }

    public void Update()
    {
        float dt = Time.deltaTime;
        
        ScaledDeltaTime = dt * TimeScale;

        Time.timeScale = TimeScale;

        foreach (var project in Projects)
        {
            project.Update();
            foreach (var task in project.Tasks)
                task.Update();
        }

        foreach (var worker in Workers)
            worker.Update();
    }

    public void Draw()
    {
        // Placeholder: integrate with UI later
    }
}
