using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private Button btnNewGame;
    public VisualElement navbar;
    public VisualElement content;
    public VisualElement messagebox;
    Game Game;

    VisualElement Root;
    void OnEnable()
    {
        if (Game != null) return; // Avoid creating multiple game instances or duplicate handlers
        var uiDocument = GetComponent<UIDocument>();
        Root = uiDocument.rootVisualElement;
        navbar = Root.Q<VisualElement>("navbar");
        content = Root.Q<VisualElement>("content");
        messagebox = Root.Q<VisualElement>("messagebox"); // Access the messagebox by name
        btnNewGame = Root.Q<Button>("btnNewGame"); // Access the button by name
        btnNewGame.clicked += () =>
        {
            Debug.Log("newgame Button clicked!");
            Game.OnNewProject += NewProjectUI;
            Game.OnNewWorker += NewWorkerUI;
            Game.OnNewTask += NewTaskUI;
            Game.textPop.OnNewTextPop += NewMessageBox;
            var project = new Project(Game, "Starting project", "Your first project.", 1, 3000);
            Game.Projects.Add(project);
            NewProjectUI(project);
        };    // Add click event handler

        Game = new Game();
        CreateNavBar();

    }
    
    private void CreateNavBar()
    {
        var btn = new Button()
        {
            text = "NavBar Button"
        };
        navbar.Add(btn); // Access the button by name
        btn.clicked += ()=>
        {
            Debug.Log("NavBar Button clicked!");
            if(Game != null) Game.textPop.New("NavBar Button clicked!", Vector2.zero, new Color(0.5f, 0.5f, 0.0f));
        };    // Add click event handler

        // available projects
        List<string> options = Game.AvailableProjects
          .Select(p => $"Project: {p.Name} Difficulty: {p.Difficulty} Pay: {p.Pay}")
          .ToList();

        DropdownField dropdown = new DropdownField("Projects", options, 0);
        dropdown.style.alignSelf = Align.Center;
        dropdown.RegisterValueChangedCallback(evt =>
        {
            int index = options.IndexOf(evt.newValue);
            if (index >= 0)
            {
                Game.AddProject(Game.AvailableProjects[index]);
                Game.AvailableProjects.RemoveAt(index);
            }
        });

        navbar.Add(dropdown);
        
        
        // available workers
        List<string> woptions = Game.WorkersForHire
          .Select(p => $"Name: {p.Name} Skill: {p.Skill} Speciality: {p.Specialty}")
          .ToList();

        DropdownField wdropdown = new DropdownField("Workers", woptions, 0);
        wdropdown.style.alignSelf = Align.Center;
        wdropdown.RegisterValueChangedCallback(evt =>
        {
            int index = woptions.IndexOf(evt.newValue);
            if (index >= 0)
            {
                Game.AddWorker(Game.WorkersForHire[index]);
                Game.WorkersForHire.RemoveAt(index);
            }
        });

        navbar.Add(wdropdown);
    }


    private void NewProjectUI(Project project)
    {
        //var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/UI/WindowUI.uxml");
        //Root.Q<VisualElement>("main").Add(uxml.CloneTree());

        var elements = new List<VisualElement>();
        
        ProjectWrapper pr = ScriptableObject.CreateInstance<ProjectWrapper>();
        pr.project = project;
        SerializedObject so = new SerializedObject(pr);
        
        ProgressBar progressBar = new ProgressBar()
        {
            value = project.Progress,
            lowValue = 0,
            highValue = 100
        };
        progressBar.style.backgroundColor = Color.green;

        progressBar.bindingPath = "Progress";
        progressBar.Bind(so);


        ProgressBar deadlineBar = new ProgressBar()
        {
            value = project.Duration,
            lowValue = 0,
            highValue = 100,
        };
        deadlineBar.style.color = Color.red;
        
        deadlineBar.bindingPath = "Duration";
        deadlineBar.Bind(so);

        elements.Add(new Label($"TaskID: {project.Id}"));
        elements.Add(new Label($"Description: {project.Description}"));
        elements.Add(new Label($"Difficulty: {project.Difficulty}"));
        elements.Add(new Label($"Duration: {project.StartDuration}"));
        elements.Add(new Label($"Pay: {project.Pay}"));
        elements.Add(new Label($"Status: {project.Status}"));
        elements.Add(new Label($"Tasks left: {project.Tasks.Count}"));
        elements.Add(deadlineBar);
        elements.Add(progressBar);
        Root.Q<VisualElement>("content").Add(new WindowUI(project.Name,elements));
    }
    private void NewTaskUI(Task task)
    {
        var elements = new List<VisualElement>();

        TaskWrapper pr = ScriptableObject.CreateInstance<TaskWrapper>();
        pr.task = task;
        SerializedObject so = new SerializedObject(pr);
        elements.Add(new Label($"TaskID: {task.Id}"));
        elements.Add(new Label($"Description: {task.Description}"));
        elements.Add(new Label($"Difficulty: {task.Difficulty}"));
        elements.Add(new Label($"Priority: {task.Priority}"));
        elements.Add(new Label($"Speciality: {task.Specialty}"));
        elements.Add(new Label($"Status: {task.Status}"));
        elements.Add(new Label($"Assigned workers: {string.Join(", ", task.Workers.Select(w => w.Name))}"));

        ProgressBar pBar = new ProgressBar()
        {
            value = task.Progress,
            lowValue = 0,
            highValue = 100
        };
        pBar.style.backgroundColor = Color.green;
        pBar.bindingPath = "Progress";
        pBar.Bind(so);

        elements.Add(pBar);

        Root.Q<VisualElement>("content").Add(new WindowUI(task.Name, elements));
    }
    private void NewWorkerUI(Worker worker)
    {
        var elements = new List<VisualElement>();

        WorkerWrapper pr = ScriptableObject.CreateInstance<WorkerWrapper>();
        pr.worker = worker;
        SerializedObject so = new SerializedObject(pr);
        elements.Add(new Label($"WorkerID: {worker.Id}"));
        elements.Add(new Label($"Level: {worker.Level}"));
        elements.Add(new Label($"Current xp: {worker.Xp}"));
        elements.Add(new Label($"Next xp: {worker.NextXp}"));
        elements.Add(new Label($"Skill: {worker.Skill}"));
        elements.Add(new Label($"Speciality: {worker.Specialty}"));
        elements.Add(new Label($"Status: {worker.Status}"));
        elements.Add(new Label($"Task: {worker.Task}"));

        ProgressBar hBar = new ProgressBar()
        {
            value = worker.Health,
            lowValue = 0,
            highValue = 100
        };
        hBar.style.backgroundColor = Color.green;
        
        hBar.bindingPath = "Health";
        hBar.Bind(so);
        elements.Add(hBar);

        ProgressBar sBar = new ProgressBar()
        {
            value = worker.Stress,
            lowValue = 0,
            highValue = 100
        };
        sBar.style.backgroundColor = Color.cyan;

        sBar.bindingPath = "Stress";
        sBar.Bind(so);
        elements.Add(sBar);
        
        ProgressBar eBar = new ProgressBar()
        {
            value = worker.Efficiency,
            lowValue = 0,
            highValue = 100
        };
        eBar.style.backgroundColor = Color.cyan;

        eBar.bindingPath = "Efficiency";
        eBar.Bind(so);
        elements.Add(eBar);

        Root.Q<VisualElement>("content").Add(new WindowUI(worker.Name, elements));
    }

    private void NewMessageBox(string message, Color color)
    {
        var label = new Label(message);
        label.style.color = new StyleColor(color);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.marginBottom = 5;
        messagebox.Add(label);
    }
}
