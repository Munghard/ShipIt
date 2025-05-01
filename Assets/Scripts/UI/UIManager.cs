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
    void Start()
    {
        Game = new Game();

        Game.OnNewProject += NewProjectUI;
        Game.OnNewWorker += NewWorkerUI;
        Game.OnNewTask += NewTaskUI;
        Game.textPop.OnNewTextPop += NewMessageBox;

        var uiDocument = GetComponent<UIDocument>();
        Root = uiDocument.rootVisualElement;
        navbar = Root.Q<VisualElement>("navbar");
        content = Root.Q<VisualElement>("content");
        messagebox = Root.Q<VisualElement>("msgBoxScrollView"); // Access the messagebox by name
        btnNewGame = Root.Q<Button>("btnNewGame"); // Access the button by name
        btnNewGame.clicked += () =>
        {
            Game.textPop.New("New game started!",Vector2.zero, Color.white);
            var project = new Project(Game, "Starting project", "Your first project.", 1, 3000);
            var worker = new Worker("Willy Worker",Specialty.Get("General"),1,100,project,Game);
            Game.AddWorker(worker);
            Game.AddProject(project);
        };    // Add click event handler

        CreateNavBar();

    }
    private void Update()
    {
        Game?.UpdateGame();
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
            Game?.textPop.New("NavBar Button clicked!", Vector2.zero, new Color(0.5f, 0.5f, 0.0f));
        };    // Add click event handler

        // available projects
        List<string> options = Game.AvailableProjects
          .Select(p => $"{p.Name} Diff: {p.Difficulty} Pay: {p.Pay}$")
          .ToList();



        DropdownField dropdown = new DropdownField("Projects", options, 0);
        dropdown.style.alignSelf = Align.Center;
        dropdown.RegisterValueChangedCallback(evt => 
        {
            int index = options.IndexOf(evt.newValue);
            if (index >= 0)
            {
                Game.AddProject(Game.AvailableProjects[index]);
                Game.RemoveProjectFromAvailableProjects(Game.AvailableProjects[index]);
            }
        });

        Game.OnAvailableProjectsChanged += (projects) =>
        {
            List<string> options = Game.AvailableProjects
                .Select(p => $"{p.Name} Diff: {p.Difficulty} Pay: {p.Pay}$")
                .ToList();

            if (dropdown.choices.Count > 0)
                dropdown.index = 0;
            else
                dropdown.value = null;
        };

        navbar.Add(dropdown);
        

        // available workers
        List<string> woptions = Game.WorkersForHire
          .Select(p => $"{p.Name}, {p.Skill.ToString()}, {p.Specialty}")
          .ToList();

        DropdownField wdropdown = new DropdownField("Workers", woptions, 0);
        wdropdown.style.alignSelf = Align.Center;
        wdropdown.RegisterValueChangedCallback(evt =>
        {
            int index = woptions.IndexOf(evt.newValue);
            if (index >= 0)
            {
                Game.AddWorker(Game.WorkersForHire[index]);
                Game.RemoveWorkerFromHirePool(Game.WorkersForHire[index]);
            }
        });

        Game.OnAvailableWorkersChanged += (workers) =>
        {
            wdropdown.choices = workers
               .Select(p => $"{p.Name}, {p.Skill}, {p.Specialty}")
               .ToList();

            if (wdropdown.choices.Count > 0)
                wdropdown.index = 0;
            else
                wdropdown.value = null;
        };

        navbar.Add(wdropdown);


        Label fundsLabel = new Label($"Funds: {Game.Money}$");
        Game.OnMoneyChanged += (money) => fundsLabel.text = $"Funds: {Game.Money}$";
        
        navbar.Add(fundsLabel);


        Label timeScaleLabel = new Label($"Time scale: {Game.TimeScale}");
        Game.OnTimeScaleChanged += (time) => timeScaleLabel.text = $"Time scale: {time}x";
        
        navbar.Add(timeScaleLabel);

        Button btnUp = new Button()
        {
            text = "2x",
        };
        btnUp.clicked += () => Game.SetTimeScale(Game.TimeScale * 2);

        navbar.Add(btnUp);
        
        Button btnDown = new Button()
        {
            text = "0.5x",
        };
        btnDown.clicked += () => Game.SetTimeScale(Game.TimeScale * 0.5f);

        navbar.Add(btnDown);
    }


    private void NewProjectUI(Project project)
    {
        var elements = new List<VisualElement>();

        ProgressBar progressBar = new ProgressBar()
        {
            value = project.Progress,
            lowValue = 0,
            highValue = 100
        };
        progressBar.style.backgroundColor = Color.green;

        project.OnProgressChanged += (value) => progressBar.value = value;
    

        ProgressBar deadlineBar = new ProgressBar()
        {
            value = project.Duration,
            lowValue = 0,
            highValue = project.StartDuration,
        };

        deadlineBar.style.color = Color.red;

        project.OnDurationChanged += (value) => deadlineBar.value = value;

        Label idLabel = new Label($"ProjectID: {project.Id}");
        Label descLabel = new Label($"Description: {project.Description}");
        Label diffLabel = new Label($"Difficulty: {project.Difficulty}");
        Label durationLabel = new Label($"Duration: {project.StartDuration:F1}");
        Label payLabel = new Label($"Pay: {project.Pay}");
        Label statusLabel = new Label($"Status: {project.Status}");
        Label tasksLeftLabel = new Label($"Tasks left: {project.Tasks.Count}");

        elements.Add(idLabel);
        elements.Add(descLabel);
        elements.Add(diffLabel);
        elements.Add(durationLabel);
        elements.Add(payLabel);
        elements.Add(statusLabel);
        elements.Add(tasksLeftLabel);

        // Example event subscriptions (you need to define these in your Project class)
        //project.OnDescriptionChanged += val => descLabel.text = $"Description: {val}";
        //project.OnDifficultyChanged += val => diffLabel.text = $"Difficulty: {val}";
        project.OnStatusChanged += val => statusLabel.text = $"Status: {val}";
        project.OnTasksChanged += (tasks) => tasksLeftLabel.text = $"Tasks left: {tasks.Count}";
        project.OnDurationChanged += (value) => durationLabel.text = $"Duration: {project.Duration}";

        elements.Add(deadlineBar);
        elements.Add(progressBar);

        Root.Q<VisualElement>("content").Add(new WindowUI(project.Name,elements));
    }
    private void NewTaskUI(Task task)
    {
        var elements = new List<VisualElement>();

        Label idLabel = new Label($"TaskID: {task.Id}");
        Label descLabel = new Label($"Description: {task.Description}");
        Label diffLabel = new Label($"Difficulty: {task.Difficulty}");
        Label priorityLabel = new Label($"Priority: {task.Priority}");
        Label specLabel = new Label($"Speciality: {task.Specialty}");
        Label statusLabel = new Label($"Status: {task.Status}");
        Label workersLabel = new Label($"Assigned workers: {string.Join(", ", task.Workers.Select(w => w.Name))}");

        elements.Add(idLabel);
        elements.Add(descLabel);
        elements.Add(diffLabel);
        elements.Add(priorityLabel);
        elements.Add(specLabel);
        elements.Add(statusLabel);
        elements.Add(workersLabel);

        // available workers
        List<string> woptions = Game.Workers
            .Where(w => w.Task == null)
            .Select(p => $"{p.Name}, {p.Skill}, {p.Specialty}")
            .ToList();

        DropdownField wdropdown = new ("Assign worker", woptions, -1);


        wdropdown.RegisterValueChangedCallback(evt =>
        {
            int index = woptions.IndexOf(evt.newValue);
            if (index >= 0 && index < Game.Workers.Count && Game.Workers[index] != null)
            {
                task.AssignWorker(Game.Workers[index]);

                //something wrong here, workers not exist?

            }
        });

        Game.OnNewWorker += (worker)=> UpdateWorkerDropDown(wdropdown,task);
        Game.OnWorkerAssigned += (worker)=> UpdateWorkerDropDown(wdropdown,task);
        Game.OnWorkerFreed += (worker)=> UpdateWorkerDropDown(wdropdown,task);

        elements.Add(wdropdown);
        // Bind update events
        //task.OnDescriptionChanged += val => descLabel.text = $"Description: {val}";
        //task.OnDifficultyChanged += val => diffLabel.text = $"Difficulty: {val}";
        //task.OnPriorityChanged += val => priorityLabel.text = $"Priority: {val}";
        //task.OnSpecialtyChanged += val => specLabel.text = $"Speciality: {val}";
        task.OnStatusChanged += val => statusLabel.text = $"Status: {val}";
        task.OnWorkersChanged += (workers) =>
        {
            workersLabel.text = $"Assigned workers: {string.Join(", ", workers.Select(w => w.Name))}";
        };


        ProgressBar pBar = new ProgressBar()
        {
            value = task.Progress,
            lowValue = 0,
            highValue = 100
        };
        pBar.style.backgroundColor = Color.green;

        task.OnProgressChanged += (progress) => pBar.value = progress;

        elements.Add(pBar);

        Root.Q<VisualElement>("content").Add(new WindowUI(task.Name, elements));
    }

    private void UpdateWorkerDropDown(DropdownField wdropdown,Task task)
    {
        Debug.Log("Workers was updated so dropdown was updated");
        List<string> woptions = Game.Workers
            .Where(w => w.Task == null)
            .Select(p => $"{p.Name}, {p.Skill.ToString()}, {p.Specialty}")
            .ToList();


        wdropdown.RegisterValueChangedCallback(evt =>
        {
            int index = woptions.IndexOf(evt.newValue);
            if (index >= 0 && index < Game.Workers.Count && Game.Workers[index] != null)
            {
                task.AssignWorker(Game.Workers[index]);
            }
        });
    }

    private void NewWorkerUI(Worker worker)
    {
        var elements = new List<VisualElement>();
        Label idLabel = new Label($"WorkerID: {worker.Id}");
        Label levelLabel = new Label($"Level: {worker.Level}");
        Label xpLabel = new Label($"Current xp: {worker.Xp}");
        Label nextXpLabel = new Label($"Next xp: {worker.NextXp}");
        Label skillLabel = new Label($"Skill: {worker.Skill}");
        Label specialtyLabel = new Label($"Speciality: {worker.Specialty}");
        Label statusLabel = new Label($"Status: {worker.Status}");
        Label taskLabel = new Label($"Task: {worker.Task}");

        elements.Add(idLabel);
        elements.Add(levelLabel);
        elements.Add(xpLabel);
        elements.Add(nextXpLabel);
        elements.Add(skillLabel);
        elements.Add(specialtyLabel);
        elements.Add(statusLabel);
        elements.Add(taskLabel);

        // Bind update events
        worker.OnLevelChanged += val => levelLabel.text = $"Level: {val}";
        worker.OnXpChanged += val => xpLabel.text = $"Current xp: {val}";
        worker.OnNextXpChanged += val => nextXpLabel.text = $"Next xp: {val}";
        worker.OnSkillChanged += val => skillLabel.text = $"Skill: {val}";
        //worker.OnSpecialtyChanged += val => specialtyLabel.text = $"Speciality: {val}";
        worker.OnStatusChanged += val => statusLabel.text = $"Status: {val}";
        worker.OnTaskChanged += val => taskLabel.text = $"Task: {val}";


        ProgressBar hBar = new ProgressBar()
        {
            value = worker.Health,
            lowValue = 0,
            highValue = 100
        };
        hBar.style.backgroundColor = Color.green;
        
        elements.Add(hBar);

        worker.OnHealthChanged += (value) => hBar.value = value;

        ProgressBar sBar = new ProgressBar()
        {
            value = worker.Stress,
            lowValue = 0,
            highValue = 100
        };
        sBar.style.backgroundColor = Color.cyan;

        elements.Add(sBar);
        worker.OnStressChanged += (value) => sBar.value = value;
        
        ProgressBar eBar = new ProgressBar()
        {
            value = worker.Efficiency,
            lowValue = 0,
            highValue = 100
        };
        eBar.style.backgroundColor = Color.cyan;

        worker.OnEfficiencyChanged += (value) => eBar.value = value;

        elements.Add(eBar);

        Root.Q<VisualElement>("content").Add(new WindowUI(worker.Name, elements));
    }

    private void NewMessageBox(string message, Color color)
    {
        var label = new Label()
        {
            text = message,
            style =
            {
                color = color,
                marginBottom = 5,
            }
        };
        messagebox.Add(label);
    }
}
