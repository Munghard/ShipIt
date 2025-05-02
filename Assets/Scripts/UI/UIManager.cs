using Assets.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private Button btnNewGame;
    public VisualElement navbar;
    public VisualElement workContent;
    public VisualElement messagebox;
    Game Game;

    VisualElement Root;

    List<Sprite> Icons;

    private void Awake()
    {
        // load icons
        Icons = Resources.LoadAll<Sprite>("textures/IconsShadow-32").ToList();
    }

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
        workContent = Root.Q<VisualElement>("workContent");
        messagebox = Root.Q<VisualElement>("msgBoxScrollView"); // Access the messagebox by name
        btnNewGame = Root.Q<Button>("btnNewGame"); // Access the button by name
        btnNewGame.clicked += () =>
        {
            Game.NewGame();
        };    // Add click event handler

        CreateNavBar();

    }
    private void Update()
    {
        Game?.UpdateGame();
    }
    private void CreateNavBar()
    {
        // Initial options
        List<string> options = Game.AvailableProjects
            .Select(p => $"{p.Name} Diff: {p.Difficulty} Pay: {p.Pay}$ Rep: {p.ReputationNeeded} {(p.ReputationNeeded > Game.Reputation ? "[LOCKED]" : "")}")
            .ToList();

        DropdownField dropdown = new DropdownField("Projects", options, 0);
        dropdown.AddToClassList("truncate");
        //dropdown.style.alignSelf = Align.Center;

        dropdown.RegisterValueChangedCallback(evt =>
        {
            int index = options.IndexOf(evt.newValue);
            if (index >= 0 && Game.AvailableProjects[index].ReputationNeeded <= Game.Reputation)
            {
                Game.AddProject(Game.AvailableProjects[index]);
                Game.RemoveProjectFromAvailableProjects(Game.AvailableProjects[index]);
            }
        });

        Game.OnAvailableProjectsChanged += (projects) =>
        {
            options = Game.AvailableProjects
                .Select(p => $"{p.Name} Diff: {p.Difficulty} Pay: {p.Pay}$ Rep: {p.ReputationNeeded} {(p.ReputationNeeded > Game.Reputation ? "[LOCKED]" : "")}")
                .ToList();

            dropdown.choices = options;

            if (options.Count > 0)
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
        wdropdown.AddToClassList("truncate");
        //wdropdown.style.alignSelf = Align.Center;
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
        fundsLabel.AddToClassList("navbar-label");

        navbar.Add(fundsLabel);

        Label repLabel = new Label($"Reputation: {Game.Reputation}");
        Game.OnReputationChanged += (rep) => repLabel.text = $"Reputation: {Game.Reputation}";
        repLabel.AddToClassList("navbar-label");

        navbar.Add(repLabel);


        VisualElement vTimeContainer = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Column
            }
        };

        VisualElement hTimeContainer = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Row
            }
        };



        navbar.Add(vTimeContainer);

        Label timeScaleLabel = new Label($"Time scale: {Game.TimeScale}")
        {
            style =
            {
                fontSize = 25
            }
        };
        Game.OnTimeScaleChanged += (time) => timeScaleLabel.text = $"Time scale: {time}x";
        timeScaleLabel.AddToClassList("navbar-label");
        
        vTimeContainer.Add(timeScaleLabel);

        Button btnDown = new Button()
        {
            text = "0.5x",
        };
        btnDown.clicked += () => Game.SetTimeScale(Game.TimeScale * 0.5f);

        hTimeContainer.Add(btnDown);

        Button btnUp = new Button()
        {
            text = "2x",
        };
        btnUp.clicked += () => Game.SetTimeScale(Game.TimeScale * 2);

        hTimeContainer.Add(btnUp);
        
        Button btnPause = new Button()
        {
            text = "Pause",
        };
        btnPause.clicked += () => Game.TogglePaused();

        hTimeContainer.Add(btnPause);

        vTimeContainer.Add(hTimeContainer);
    }


    private void NewProjectUI(Project project)
    {
        var projectIndex = Game.Projects.IndexOf(project);

        var elements = new List<VisualElement>();

        ProgressBar pBar = new ProgressBar()
        {
            value = project.Progress,
            lowValue = 0,
            highValue = 100,
            tooltip = "Progress"
        };
        var pBarFill = pBar.Q(className: "unity-progress-bar__progress");
        pBarFill.style.backgroundColor = ProgressBarColors.Green;

        project.OnProgressChanged += (value) => pBar.value = value;
    

        ProgressBar dlBar = new ProgressBar()
        {
            value = project.Duration,
            lowValue = 0,
            highValue = project.StartDuration,
            tooltip = "Deadline"
        };

        var dlBarFill = dlBar.Q(className: "unity-progress-bar__progress");
        dlBarFill.style.backgroundColor = ProgressBarColors.Red;

        project.OnDurationChanged += (value) => dlBar.value = value;

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

        elements.Add(dlBar);
        elements.Add(pBar);

        Button btnSubmit = new()
        {
            text = "Ship it!",
        };
        btnSubmit.clicked += () =>
        {
            project.TurnInProject();
        };

        elements.Add(btnSubmit);
        
        workContent.Add(new WindowUI(project.Name, Icons[36], elements, new Vector2(projectIndex * 100, projectIndex * 100)));
    }
    private void NewTaskUI(Task task)
    {
        int taskIndex = 0;

        var project = Game.Projects.FirstOrDefault(p => p.Tasks.Contains(task));
        if (project != null)
        {
            taskIndex = project.Tasks.IndexOf(task);
            // Use project and taskIndex as needed
        }

        var elements = new List<VisualElement>();

        Label idLabel = new Label($"TaskID: {task.Id}");
        Label descLabel = new Label($"Description: {TextWrap.WrapText(task.Description,30)}");
        Label diffLabel = new Label($"Difficulty: {task.Difficulty}");
        Label priorityLabel = new Label($"Priority: {task.Priority}");
        Label specLabel = new Label($"Speciality: {task.Specialty.Name}");
        Label statusLabel = new Label($"Status: {task.Status}");
        Label workersLabel = new Label($"Assigned workers: {string.Join(", ", task.Workers.Select(w => w.Name))}");

        elements.Add(idLabel);
        elements.Add(descLabel);
        elements.Add(diffLabel);
        elements.Add(priorityLabel);
        elements.Add(specLabel);
        elements.Add(statusLabel);
        elements.Add(workersLabel);

        // Keep actual worker references
        List<Worker> availableWorkers = Game.Workers
            .Where(w => w.Task == null)
            .ToList();

        // available workers
        List<string> woptions = availableWorkers
            .Where(w => w.Task == null)
            .Select(p => $"{p.Name}, {p.Skill}, {p.Specialty.Name}")
            .ToList();

        DropdownField wdropdown = new ("Assign worker", woptions, -1);


        wdropdown.RegisterValueChangedCallback(evt =>
        {
            int index = woptions.IndexOf(evt.newValue);
            if (index >= 0 && index < availableWorkers.Count)
            {
                Worker selectedWorker = availableWorkers[index];
                task.AssignWorker(selectedWorker);
            }
        });

        Game.OnNewWorker += (worker)=> SetupWorkerDropdown(wdropdown,task);
        Game.OnWorkerAssigned += (worker)=> SetupWorkerDropdown(wdropdown,task);
        Game.OnWorkerFreed += (worker)=> SetupWorkerDropdown(wdropdown,task);

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
            highValue = 100,
            tooltip = "Progress"
        };
        var pBarFill = pBar.Q(className: "unity-progress-bar__progress");
        pBarFill.style.backgroundColor = ProgressBarColors.Green;

        task.OnProgressChanged += (progress) => pBar.value = progress;

        elements.Add(pBar);
        workContent.Add(new WindowUI(task.Name, Icons[42], elements, new Vector2((taskIndex * 100) + 400, taskIndex * 100)));
    }
    private Dictionary<DropdownField, EventCallback<ChangeEvent<string>>> dropdownCallbacks = new();

    private void SetupWorkerDropdown(DropdownField dropdown, Task task)
    {
        // Get available workers
        List<Worker> availableWorkers = Game.Workers
            .Where(w => w.Task == null)
            .ToList();

        List<string> options = availableWorkers
            .Select(w => $"{w.Name}, {w.Skill}, {w.Specialty}")
            .ToList();

        // Remove existing callback if one is registered
        if (dropdownCallbacks.TryGetValue(dropdown, out var oldCallback))
        {
            dropdown.UnregisterValueChangedCallback(oldCallback);
            dropdownCallbacks.Remove(dropdown);
        }

        // Update dropdown
        dropdown.choices = options;
        dropdown.index = -1;

        // Create new callback
        EventCallback<ChangeEvent<string>> callback = evt =>
        {
            int index = options.IndexOf(evt.newValue);
            if (index >= 0 && index < availableWorkers.Count)
            {
                task.AssignWorker(availableWorkers[index]);
            }
        };

        dropdown.RegisterValueChangedCallback(callback);
        dropdownCallbacks[dropdown] = callback;
    }



    private void NewWorkerUI(Worker worker)
    {
        var workerIndex = Game.Workers.IndexOf(worker);

        var elements = new List<VisualElement>();

        Image imgPortrait = new()
        {
            sprite = worker.Portrait,
        };
        imgPortrait.AddToClassList("portrait");

        elements.Add (imgPortrait);

        Label idLabel = new Label($"WorkerID: {worker.Id}");
        Label levelLabel = new Label($"Level: {worker.Level}");
        Label xpLabel = new Label($"Current xp: {worker.Xp}");
        Label nextXpLabel = new Label($"Next xp: {worker.NextXp}");
        Label skillLabel = new Label($"Skill: {worker.Skill}");
        Label specialtyLabel = new Label($"Speciality: {worker.Specialty.Name}");
        Label statusLabel = new Label($"Status: {worker.Status}");
        Label taskLabel = new Label($"Task: {worker.Task?.Name}");

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
        worker.OnTaskChanged += val => taskLabel.text = $"Task: {val?.Name}";


        ProgressBar hBar = new ProgressBar()
        {
            value = worker.Health,
            lowValue = 0,
            highValue = 100,
            tooltip = "Health"
        };

        var hBarFill = hBar.Q(className: "unity-progress-bar__progress");
        hBarFill.style.backgroundColor = ProgressBarColors.Red;

        elements.Add(hBar);

        worker.OnHealthChanged += (value) => hBar.value = value;

        ProgressBar sBar = new ProgressBar()
        {
            value = worker.Stress,
            lowValue = 0,
            highValue = 100,
            tooltip = "Stress"
        };

        var sBarFill = sBar.Q(className: "unity-progress-bar__progress");
        sBarFill.style.backgroundColor = ProgressBarColors.Yellow;

        elements.Add(sBar);
        worker.OnStressChanged += (value) => sBar.value = value;
        
        ProgressBar eBar = new ProgressBar()
        {
            value = worker.Efficiency,
            lowValue = 0,
            highValue = 100,
            tooltip = "Efficiency"
        };
        var eBarFill = eBar.Q(className: "unity-progress-bar__progress");
        eBarFill.style.backgroundColor = ProgressBarColors.Cyan;

        worker.OnEfficiencyChanged += (value) => eBar.value = value;

        elements.Add(eBar);

        workContent.Add(new WindowUI(worker.Name, Icons[20], elements, new Vector2(workerIndex * 100, (workerIndex * 100) + 600)));
    }

    private void NewMessageBox(string message, Color color)
    {
        Debug.Log("New message: " +  message);
        var label = new Label()
        {
            text = message,
            style =
            {
                color = color,
                marginBottom = 5,
            }
        };
        messagebox.Insert(0,label);
    }
}
