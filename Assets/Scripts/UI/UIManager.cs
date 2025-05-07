using Assets.Scripts.Data;
using Assets.Scripts.Models;
using Assets.Scripts.UI;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private Button btnNewGame;
    public VisualElement navbar;
    public VisualElement workersContent;
    public VisualElement availableProjectsContent;
    public VisualElement projectsContent;
    public VisualElement tasksContent;
    public VisualElement staffingContent;
    public VisualElement shopContent;
    public VisualElement messagebox;
    public VisualElement gameButtons;


    //GAME UI
    Label simulationTimeLabel;
    Label timeScaleLabel;
    Label moneyLabel;
    Label repLabel;

    Button btnPassTime;
    Button btnPause;
    Button btnUp;
    Button btnDown;
    //GAME UI

    Game Game;

    VisualElement Root;

    public static List<Sprite> Icons;

    private void Awake()
    {
        // load icons
        Icons = Resources.LoadAll<Sprite>("textures/IconsShadow-32").ToList();
    }

    void Start()
    {
        Game = new Game();

        var uiDocument = GetComponent<UIDocument>();
        Root = uiDocument.rootVisualElement;
        navbar = Root.Q<VisualElement>("navbar");
        workersContent = Root.Q<VisualElement>("workersContent");
        staffingContent = Root.Q<VisualElement>("staffingContent");
        availableProjectsContent = Root.Q<VisualElement>("availableProjectsContent");
        projectsContent = Root.Q<VisualElement>("projectsContent");
        tasksContent = Root.Q<VisualElement>("tasksContent");
        shopContent = Root.Q<VisualElement>("shopContent");

        messagebox = Root.Q<VisualElement>("msgBoxScrollView"); // Access the messagebox by name
        gameButtons = Root.Q<VisualElement>("gameButtons");

        Button btnNewGame = new();
        btnNewGame.clicked += () =>
        {
            ConfirmationWindow.Create(Root,() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        };    // Add click event handler
        SetButtonIcon(btnNewGame, "circle-play", "New game");

        Button btnLoad = new();
        btnLoad.clicked += () =>
        {
            ConfirmationWindow.Create(Root, () =>
            {
                LoadGame();
            });
        };
        SetButtonIcon(btnLoad, "folder-open", "Load game");
        
        Button btnSave = new();
        
        btnSave.clicked += () =>
        {
            ConfirmationWindow.Create(Root, () =>
            {
                SaveGame();
            });
        };
        SetButtonIcon(btnSave, "floppy-disk", "Save game");
        gameButtons.Add(btnNewGame);
        gameButtons.Add(btnLoad);
        gameButtons.Add(btnSave);


        Game.OnNewProject += NewProjectUI;

        Game.OnAvailableProjectsChanged += AvailableProjectsChanged;

        Game.OnProjectsChanged += ProjectsChanged;

        Game.OnAvailableWorkersChanged += AvailableWorkersChanged;

        Game.OnNewWorker += NewWorkerUI;

        Game.OnWorkersChanged += WorkersChanged;

        Game.textPop.OnNewTextPop += NewMessageBox;

        Game.OnProjectsChanged += UpdateShop;

        Game.NewGame();

        CreateNavBar();


    }
    void ClearContainers()
    {
        //Root
        //navbar.Clear();
        workersContent.Clear();
        staffingContent.Clear();
        availableProjectsContent.Clear();
        projectsContent.Clear();
        tasksContent.Clear();
        shopContent.Clear();

        //messagebox.Clear();
    }

    private void LoadGame()
    {
        Game = new Game();

        Game.OnNewProject += NewProjectUI;

        Game.OnAvailableProjectsChanged += AvailableProjectsChanged;

        Game.OnProjectsChanged += ProjectsChanged;

        //foreach (var project in Game.Projects)
        //{
        //    project.OnTasksChanged += TasksChanged;
        //}

        Game.OnAvailableWorkersChanged += AvailableWorkersChanged;

        Game.OnNewWorker += NewWorkerUI;

        Game.OnWorkersChanged += WorkersChanged;

        Game.textPop.OnNewTextPop += NewMessageBox;

        Game.OnProjectsChanged += UpdateShop;

        ClearContainers();

        BindGameToUI(Game);

        GameData loadedData = SaveManager.LoadGame();
        if (loadedData != null)
        {
            // Set loaded data to game state
            Game.SimulationTime = loadedData.SimulationTime;
            Game.Money = loadedData.Money;
            Game.Reputation = loadedData.Reputation;

            // Clear current workers and projects in the game
            Game.Workers = new List<Worker>();  // Clear workers list
            Game.WorkersForHire = new List<Worker>();  // Clear workers list
            Game.Projects = new List<Project>(); // Clear projects list
            Game.AvailableProjects = new List<Project>(); // Clear projects list
            Game.buyables = new List<Buyable>();
            Game.acquiredBuyables = new List<Buyable>();

            // Reconstruct Workers from loaded data
            foreach (var buyableData in loadedData.Shop)
            {
                Game.buyables.Add(BuyableLibrary.GetBuyable(buyableData.Id));
            }
            foreach (var buyableData in loadedData.BoughtBuyables)
            {
                Game.acquiredBuyables.Add(BuyableLibrary.GetBuyable(buyableData.Id));
            }
            UpdateShop(Game.Projects);

            Game.OnAcquiredBuyablesChanged?.Invoke(Game.acquiredBuyables);
            
            foreach (var loadedWorker in loadedData.Workers)
            {
                Game.AddWorker(new Worker(
                    name: loadedWorker.Name,
                    portrait: Game.workerGenerator.GetPortrait(loadedWorker.PortraitIndex),
                    specialty: Specialty.Get(loadedWorker.Specialty),
                    skill: loadedWorker.Skill,
                    efficiency: loadedWorker.Efficiency,
                    project: null,
                    game: Game,
                    health: loadedWorker.Health,
                    stress: loadedWorker.Stress,
                    level: loadedWorker.Level,
                    xp: loadedWorker.Xp,
                    id: loadedWorker.Id
                ));
            }
            foreach (var loadedWorker in loadedData.AvailableWorkers)
            {
                Game.WorkersForHire.Add(new Worker(
                    name: loadedWorker.Name,
                    portrait: Game.workerGenerator.GetPortrait(loadedWorker.PortraitIndex),
                    specialty: Specialty.Get(loadedWorker.Specialty),
                    skill: loadedWorker.Skill,
                    efficiency: loadedWorker.Efficiency,
                    project: null,
                    game: Game,
                    health: loadedWorker.Health,
                    stress: loadedWorker.Stress,
                    level: loadedWorker.Level,
                    xp: loadedWorker.Xp,
                    id: loadedWorker.Id
                ));
            }
            Game.OnAvailableWorkersChanged?.Invoke(Game.WorkersForHire);

            // Reconstruct Projects and their tasks
            foreach (var loadedProject in loadedData.Projects)
            {
                var tasks = new List<Task>();
                foreach (var loadedTask in loadedProject.Tasks)
                {

                    tasks.Add(new Task(
                        name: loadedTask.Name,
                        description: loadedTask.Description,
                        difficulty: loadedTask.Difficulty,
                        specialty: Specialty.Get(loadedTask.Specialty),
                        timeToComplete: loadedTask.TimeToComplete,
                        project: null,
                        game: Game,
                        status: "pending",
                        priority: loadedTask.Priority,
                        progress: loadedTask.Progress
                    ));
                }
                // Add project to game
                var project = new Project(
                    game: Game,
                    name: loadedProject.Name,
                    description: loadedProject.Description,
                    difficulty: loadedProject.Difficulty,
                    duration: loadedProject.Duration,
                    pay: loadedProject.Pay,
                    startDuration: loadedProject.StartDuration,
                    id: loadedProject.Id,
                    tasks: tasks

                );
                foreach (var task in tasks)
                {
                    task.Project = project;
                }

                Game.AddProject(project);

                // Reconstruct tasks for each project
            }
            //load availableProjects
            foreach (var loadedProject in loadedData.AvailableProjects )
            {
                var tasks = new List<Task>();
                foreach (var loadedTask in loadedProject.Tasks)
                {

                    tasks.Add(new Task(
                        name: loadedTask.Name,
                        description: loadedTask.Description,
                        difficulty: loadedTask.Difficulty,
                        specialty: Specialty.Get(loadedTask.Specialty),
                        timeToComplete: loadedTask.TimeToComplete,
                        project: null,
                        game: Game,
                        status: "pending",
                        priority: loadedTask.Priority,
                        progress: loadedTask.Progress
                    ));
                }
                var project = new Project(
                    game: Game,
                    name: loadedProject.Name,
                    description: loadedProject.Description,
                    difficulty: loadedProject.Difficulty,
                    duration: loadedProject.Duration,
                    pay: loadedProject.Pay,
                    startDuration: loadedProject.StartDuration,
                    id: loadedProject.Id,
                    tasks: tasks

                );
                Game.AvailableProjects.Add(project);
                Game.OnAvailableProjectsChanged?.Invoke(Game.AvailableProjects);
            }
        }
        Game.textPop.New("Game loaded!", Vector2.zero, Color.magenta);
        //update everything

    }


    private void SaveGame()
    {
        GameData data = new GameData()
        {
            SimulationTime = Game.SimulationTime,
            Money = Game.Money,
            Reputation = Game.Reputation,
            Workers = new List<WorkerData>(),
            Projects = new List<ProjectData>(),

            AvailableProjects = new List<ProjectData>(),
            AvailableWorkers = new List<WorkerData>(),
            BoughtBuyables = new List<BuyableData>(),
            Shop = new List<BuyableData>(),

        };
        foreach (var buyable in Game.buyables)
        {
            data.Shop.Add(new BuyableData()
            {
                Id = buyable.Id,
            });
        }
        foreach (var buyable in Game.acquiredBuyables)
        {
            data.BoughtBuyables.Add(new BuyableData()
            {
                Id = buyable.Id,
            });
        }
        foreach (var worker in Game.Workers)
        {
            data.Workers.Add(new WorkerData()
            {
                Id = worker.Id,
                Name = worker.Name,
                Level = worker.Level,
                Skill = worker.Skill,
                Health = worker.Health,
                Stress = worker.Stress,
                PortraitIndex = Game.workerGenerator.GetPortraitIndex(worker.Portrait),
                Specialty = worker.Specialty.Name,
                Efficiency = worker.Efficiency,
                Xp = worker.Xp,
            });
        }
        foreach (var worker in Game.WorkersForHire)
        {
            data.AvailableWorkers.Add(new WorkerData()
            {
                Id = worker.Id,
                Name = worker.Name,
                Level = worker.Level,
                Skill = worker.Skill,
                Health = worker.Health,
                Stress = worker.Stress,
                PortraitIndex = Game.workerGenerator.GetPortraitIndex(worker.Portrait),
                Specialty = worker.Specialty.Name,
                Efficiency = worker.Efficiency,
                Xp = worker.Xp,
            });
        }
        foreach (var project in Game.Projects)
        {
            var pData = new ProjectData()
            {
                Name = project.Name,
                Difficulty = project.Difficulty,
                Description = project.Description,
                Duration = project.Duration,
                Id = project.Id,
                Pay = project.Pay,
                StartDuration = project.StartDuration,
                Tasks = new List<TaskData>(),
            };
            data.Projects.Add(pData);
            foreach (var task in project.Tasks)
            {
                pData.Tasks.Add(new TaskData()
                {
                    Name = task.Name,
                    Progress = task.Progress,
                    Specialty = task.Specialty.Name,
                    Difficulty = task.Difficulty,
                    Priority = task.Priority,
                    Description = task.Description,
                    TimeToComplete = task.TimeToComplete,
                });
            }
        }
        foreach (var project in Game.AvailableProjects)
        {
            var pData = new ProjectData()
            {
                Name = project.Name,
                Difficulty = project.Difficulty,
                Description = project.Description,
                Duration = project.Duration,
                Id = project.Id,
                Pay = project.Pay,
                StartDuration = project.StartDuration,
                Tasks = new List<TaskData>(),
            };
            data.AvailableProjects.Add(pData);
            foreach (var task in project.Tasks)
            {
                pData.Tasks.Add(new TaskData()
                {
                    Name = task.Name,
                    Progress = task.Progress,
                    Specialty = task.Specialty.Name,
                    Difficulty = task.Difficulty,
                    Priority = task.Priority,
                    Description = task.Description,
                    TimeToComplete = task.TimeToComplete,
                });
            }
        }
        // fill in data.Workers, data.Projects, etc.
        SaveManager.SaveGame(data);
        Game.textPop.New("Game saved!", Vector2.zero, Color.magenta);
    }

    public static VisualElement CreateFAIcon(string name,int width = 32,int height = 32)
    {
        VisualElement icon = new();

        var vec = Resources.Load<VectorImage>($"Fontawesome/svgs/solid/{name}");
        icon.style.backgroundImage = new StyleBackground(vec);
        //test.style.unityBackgroundImageTintColor = Color.cyan;
        icon.style.width = width;
        icon.style.height = height;
        return icon;
    }

    private void WorkersChanged(List<Worker> list)
    {
        workersContent.Clear();
        foreach (var worker in list)
        {
            NewWorkerUI(worker);
        }
    }

    private void UpdateShop(List<Project> list)
    {
        shopContent.Clear();
        foreach (var buyable in Game.buyables)
        {
            NewBuyableUI(buyable);
        }
    }



    private void AvailableWorkersChanged(List<Worker> list)
    {
        staffingContent.Clear();
        foreach (Worker worker in list)
        {
            NewAvailableWorkerUI(worker);
        }
    }

    private void AvailableProjectsChanged(List<Project> list)
    {
        availableProjectsContent.Clear();
        foreach (Project project in list) 
        {
            NewAvailableProjectUI(project);
        }
    }
    private void ProjectsChanged(List<Project> list)
    {
        projectsContent.Clear();
        tasksContent.Clear();
        foreach (Project project in list) 
        {
            NewProjectUI(project);
            foreach(Task task in project.Tasks)
            {
                NewTaskUI(task);
            }
        }
    }

    private void Update()
    {
        Game?.UpdateGame();
        var simulationTimeLabel = Root.Q<Label>("simulationTimeLabel");
        if (simulationTimeLabel != null && Game != null)
        {
            // Format the SimulationTime to display as a clock (HH:mm:ss)
            TimeSpan timeSpan = TimeSpan.FromSeconds(Game.SimulationTime * 60);
            simulationTimeLabel.text = $"Time: {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
    private void CreateNavBar()
    {
        // Initial options

        CreateStatsContainer();

        CreateTimeContainer();

        CreateThemeButton();

    }
    public void BindGameToUI(Game game)
    {
        // Optional: unhook previous if needed
        game.OnTimeScaleChanged += time => {
            if (timeScaleLabel != null)
                timeScaleLabel.text = $"Time scale: {time}x";
        };
        game.OnMoneyChanged += money => {
            if (moneyLabel != null)
                moneyLabel.text = $"{money}$";
        };
        game.OnReputationChanged += rep => {
            if (repLabel != null)
                repLabel.text = $"{rep} Rep";
        };

        btnPassTime.clicked += Game.PassTime; // pass 6 hours?
        
        btnPause.clicked += game.TogglePaused;
        btnUp.clicked += game.DoubleTimeScale; 
        btnDown.clicked += game.HalveTimeScale;

    }

    private void CreateThemeButton()
    {
        Button btnTheme = new Button();
        SetButtonIcon(btnTheme, "Brush", "Theme");

        btnTheme.clicked += () =>
        {
            Root.ToggleInClassList("light-theme");
            //availableProjectsContent.ToggleInClassList("light-theme");
            ////workContent.ToggleInClassList("light-theme");
            //workersContent.ToggleInClassList("light-theme");
            //staffingContent.ToggleInClassList("light-theme");
            //availableProjectsContent.ToggleInClassList("light-theme");
            //projectsContent.ToggleInClassList("light-theme");
            //tasksContent.ToggleInClassList("light-theme");
            //shopContent.ToggleInClassList("light-theme");
        };

        navbar.Add(btnTheme);
    }

    private void CreateStatsContainer()
    {
        VisualElement statsContainer = new VisualElement();
        statsContainer.AddToClassList("container");

        navbar.Add(statsContainer);

        Label statsLabel = new Label("Stats");
        statsLabel.AddToClassList("navbar-label-header");

        statsContainer.Add(statsLabel);

        moneyLabel = new Label($"{Game.Money}$");
        Game.OnMoneyChanged += (money) => moneyLabel.text = $"{Game.Money}$";
        moneyLabel.AddToClassList("navbar-label");

        statsContainer.Add(moneyLabel);

        repLabel = new Label($"{Game.Reputation}Rep");
        Game.OnReputationChanged += (rep) => repLabel.text = $"{Game.Reputation}Rep";
        repLabel.AddToClassList("navbar-label");

        statsContainer.Add(repLabel);
    }

    private void CreateTimeContainer()
    {
        VisualElement vTimeContainer = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Column
            }
        };
        vTimeContainer.AddToClassList("container");

        VisualElement hTimeContainer = new VisualElement()
        {
            style =
            {
                flexDirection = FlexDirection.Row
            }
        };


        vTimeContainer.AddToClassList("container");

        navbar.Add(vTimeContainer);

        simulationTimeLabel = new Label($"Time: {Game.SimulationTime}")
        {
            style =
            {
                fontSize = 25
            }
        };
        simulationTimeLabel.name = "simulationTimeLabel";
        vTimeContainer.Add(simulationTimeLabel);

        timeScaleLabel = new Label($"Time scale: {Game.TimeScale}")
        {
            style =
            {
                fontSize = 25
            }
        };
        Game.OnTimeScaleChanged += (time) => timeScaleLabel.text = $"Time scale: {time}x";

        timeScaleLabel.AddToClassList("navbar-label");


        vTimeContainer.Add(timeScaleLabel);

        btnDown = new Button();
        SetButtonIcon(btnDown, "backward", "");

        btnDown.clicked += () => Game.HalveTimeScale();

        hTimeContainer.Add(btnDown);

        btnUp = new Button();
        btnUp.clicked += () => Game.DoubleTimeScale();
        SetButtonIcon(btnUp, "forward", "");

        hTimeContainer.Add(btnUp);

        btnPause = new Button();
        SetButtonIcon(btnPause, "pause", "");

        btnPause.clicked += () => Game.TogglePaused();

        hTimeContainer.Add(btnPause);
        vTimeContainer.Add(hTimeContainer);

        btnPassTime = new Button();
        SetButtonIcon(btnPassTime, "circle-plus", "Pass time");

        btnPassTime.clicked += Game.PassTime; // pass an hour?


        vTimeContainer.Add(btnPassTime);

    }

    
    private void SetButtonIcon(Button btn, string name, string text)
    {
        btn.text = string.Empty;
        // Create the container for the button content
        VisualElement buttonContent = new VisualElement();
        buttonContent.style.flexDirection = FlexDirection.Row; // Align items horizontally
        buttonContent.style.alignItems = Align.Center; // Center the icon and label vertically

        // Create the Image element for the icon
        
        
        // Create the Label for the button's text
        Label buttonLabel = new Label(text);

        // Add the icon and label to the container
        buttonContent.Add(CreateFAIcon(name, 24, 24));
        buttonLabel.style.marginLeft = 16;

        if(!string.IsNullOrEmpty(text))buttonContent.Add(buttonLabel);

        // Set the content of the button to be the container with the icon and label
        btn.Clear();  // Remove any existing content
        btn.Add(buttonContent);  // Add the new container with both icon and label
    }
    private void SetButtonIconOld(Button btn, Sprite sprite, string text)
    {
        btn.text = string.Empty;
        // Create the container for the button content
        VisualElement buttonContent = new VisualElement();
        buttonContent.style.flexDirection = FlexDirection.Row; // Align items horizontally
        buttonContent.style.alignItems = Align.Center; // Center the icon and label vertically

        // Create the Image element for the icon
        Image iconImage = new Image();
        iconImage.sprite = sprite;
        iconImage.style.width = 24;  // Set the width of the icon (adjust as needed)
        iconImage.style.height = 24; // Set the height of the icon (adjust as needed)

        // Create the Label for the button's text
        
        Label buttonLabel = new Label(text);

        // Add the icon and label to the container
        buttonContent.Add(iconImage);
        if(!string.IsNullOrEmpty(text))buttonContent.Add(buttonLabel);

        // Set the content of the button to be the container with the icon and label
        btn.Clear();  // Remove any existing content
        btn.Add(buttonContent);  // Add the new container with both icon and label
    }




    private void NewBuyableUI(Buyable buyable)
    {
        var elements = new List<VisualElement>();


        VisualElement imgPortrait = CreateFAIcon(buyable.IconName, 128, 128);
        imgPortrait.style.paddingLeft = 16;
        imgPortrait.style.paddingRight = 16;
        imgPortrait.style.paddingBottom = 16;
        imgPortrait.style.paddingTop = 16;

        imgPortrait.AddToClassList("portrait");
        elements.Add(imgPortrait);

        Label nameLabel = new Label($"Name: {buyable.Name}");
        Label descriptionLabel = new Label($"Description: {TextWrap.WrapText(buyable.Description, 30)}");
        Label costLabel = new Label($"Cost: {buyable.Cost}$");
        Label reputationLabel = new Label($"Reputation needed: {buyable.ReputationNeeded}");

        elements.Add(nameLabel);
        elements.Add(descriptionLabel);
        elements.Add(costLabel);
        elements.Add(reputationLabel);

        Button btnSubmit = new() { text = "Buy!" };
        Label purchasedLabel = new() { text = "Purchased!" };
        bool hasBuyable = Game.acquiredBuyables.Contains(buyable) && buyable.SingleBuy;

        void UpdateBuyableUI()
        {
            bool acquired = Game.acquiredBuyables.Contains(buyable) && buyable.SingleBuy;
            bool canBuy = buyable.CanPurchase((int)Game.Money, (int)Game.Reputation);

            costLabel.style.color = canBuy ? Color.green : Color.red;
            reputationLabel.style.color = canBuy ? Color.green : Color.red;

            if (!acquired)
            {
                if (!elements.Contains(btnSubmit))
                {
                    elements.Add(btnSubmit);
                }
                btnSubmit.SetEnabled(canBuy);
                elements.Remove(purchasedLabel);
            }
            else
            {
                elements.Remove(btnSubmit);
                if (!elements.Contains(purchasedLabel))
                    elements.Add(purchasedLabel);
            }
        }

        btnSubmit.clicked += () =>
        {
            buyable.OnPurchased(buyable, Game);
            Game.AddBuyableToAcquired(buyable);
            Game.textPop.New("Purchased: " + buyable.Name, Vector2.zero, Color.cyan);
            UpdateBuyableUI();
        };

        UpdateBuyableUI();
        Game.OnMoneyChanged += (_) => UpdateBuyableUI();
        Game.OnReputationChanged += (_) => UpdateBuyableUI();
        Game.OnAcquiredBuyablesChanged += (_) => UpdateBuyableUI();

        var window = new WindowUI(buyable.Name, CreateFAIcon("cart-shopping", 32, 32), elements, Vector2.zero, false);
        window.AddToClassList("buyable-window");

        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        window.Q<VisualElement>("window").style.position = Position.Relative;

        shopContent.Add(window);
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
        pBar.Q<Label>(className: "unity-progress-bar__title").text = "Progress";
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
        dlBar.Q<Label>(className: "unity-progress-bar__title").text = "Deadline";
        var dlBarFill = dlBar.Q(className: "unity-progress-bar__progress");
        dlBarFill.style.backgroundColor = ProgressBarColors.Red;

        project.OnDurationChanged += (value) => dlBar.value = value;

        Label idLabel = new Label($"ProjectID: {project.Id}");
        Label lowestPriorityLabel = new Label($"Lowest priority: {project.LowestPriority}");
        Label descLabel = new Label($"Description: {TextWrap.WrapText(project.Description, 30)}");
        Label diffLabel = new Label($"Difficulty: {project.Difficulty}");

        TimeSpan duration = TimeSpan.FromSeconds(project.Duration * 60);
        TimeSpan startDuration = TimeSpan.FromSeconds(project.StartDuration * 60);

        int totalHours = (int)duration.TotalHours;
        int totalStartHours = (int)startDuration.TotalHours;

        var timeString = $"Time left: {totalHours}h {duration.Minutes}m {duration.Seconds}s / {totalStartHours}h {startDuration.Minutes}m {startDuration.Seconds}s";


        Label durationLabel = new Label(timeString);
        Label payLabel = new Label($"Pay: {project.Pay}");
        Label statusLabel = new Label($"Status: {project.Status}");
        Label tasksLeftLabel = new Label($"Tasks left: {project.Tasks.Count}");

        VisualElement btnContainer = new VisualElement();


        //elements.Add(idLabel);
        elements.Add(descLabel);
        elements.Add(lowestPriorityLabel);
        elements.Add(diffLabel);
        elements.Add(durationLabel);
        elements.Add(payLabel);
        elements.Add(statusLabel);
        elements.Add(tasksLeftLabel);

        // Example event subscriptions (you need to define these in your Project class)
        //project.OnDescriptionChanged += val => descLabel.text = $"Description: {val}";
        //project.OnDifficultyChanged += val => diffLabel.text = $"Difficulty: {val}";
        project.OnStatusChanged += val => statusLabel.text = $"Status: {val}";
        project.OnTasksChanged += (tasks) =>
        {
            tasksLeftLabel.text = $"Tasks left: {tasks.Count}";
            lowestPriorityLabel.text = $"Lowest priority: {project.LowestPriority}";
        };


        project.OnDurationChanged += (value) =>
        {
            TimeSpan duration = TimeSpan.FromSeconds(project.Duration * 60);
            TimeSpan startDuration = TimeSpan.FromSeconds(project.StartDuration * 60);

            int totalHours = (int)duration.TotalHours;
            int totalStartHours = (int)startDuration.TotalHours;

            var timeString = $"Time left: {totalHours}h {duration.Minutes}m {duration.Seconds}s / {totalStartHours}h {startDuration.Minutes}m {startDuration.Seconds}s";


            durationLabel.text = timeString;
        };

        elements.Add(dlBar);
        elements.Add(pBar);

        elements.Add(btnContainer);

        UpdateProjectButtons(project, btnContainer);
        
        project.OnStatusChanged += val => UpdateProjectButtons(project, btnContainer);

        var window = new WindowUI(project.Name, CreateFAIcon("folder-closed", 32, 32), elements, new Vector2((projectIndex * 100), projectIndex * 100),false);
        window.AddToClassList("project-window");

        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        window.Q<VisualElement>("window").style.position = Position.Relative;

        projectsContent.Add(window);

        void UpdateProjectButtons(Project project, VisualElement container)
        {
            container.Clear();

            Button btn = new()
            {
                text = project.Status == "failed" || project.Status == "paid out" ? "Remove!" : "Ship it!"
            };

            btn.clicked += () =>
            {
                if (project.Status == "failed" || project.Status == "paid out")
                    Game.RemoveProject(project);
                else
                {
                    var chance = project.Progress / 100f;
                    ConfirmationWindow.Create(Message: $"Chance to get paid: {chance}x, are you sure?",Parent:Root, OkCallback:() =>
                    {
                        project.TurnInProject();
                    });

                }
            };

            container.Add(btn);
        }
    }

    

    private void NewAvailableProjectUI(Project project)
    {
        var projectIndex = Game.Projects.IndexOf(project);

        var elements = new List<VisualElement>();


        Label idLabel = new Label($"ProjectID: {project.Id}");
        Label descLabel = new Label($"Description: {TextWrap.WrapText(project.Description,30)}");
        Label diffLabel = new Label($"Difficulty: {project.Difficulty}");
        Label durationLabel = new Label($"Duration: {project.StartDuration:F1}");
        Label payLabel = new Label($"Pay: {project.Pay}");
        Label statusLabel = new Label($"Status: {project.Status}");
        Label tasksLeftLabel = new Label($"Tasks left: {project.Tasks.Count}");
        Label repGainLabel = new Label($"Reputation gain: {project.ReputationGain}");
        Label repNeededLabel = new Label($"Reputation needed: {project.ReputationNeeded}");

        //elements.Add(idLabel);
        elements.Add(descLabel);
        elements.Add(diffLabel);
        elements.Add(durationLabel);
        elements.Add(payLabel);
        elements.Add(tasksLeftLabel);
        elements.Add(repGainLabel);
        elements.Add(repNeededLabel);


        bool canTakeOn = Game.Reputation >= project.ReputationNeeded;
        repNeededLabel.style.color = canTakeOn? new StyleColor(Color.green): new StyleColor(Color.red);
        
        Button btnSubmit = new()
        {
            text = "Take on!",
        };
        btnSubmit.clicked += () =>
        {
            Game.AddProject(project);
            Game.RemoveProjectFromAvailableProjects(project);
        };
        btnSubmit.SetEnabled(canTakeOn);

        elements.Add(btnSubmit);

        var window = new WindowUI(project.Name, CreateFAIcon("folder-closed", 32, 32), elements, Vector2.zero,false);
        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        
        window.Q<VisualElement>("window").style.position = Position.Relative;

        window.AddToClassList("available-project-window");

        availableProjectsContent.Add(window);
    }
    private void NewTaskUI(Task task)
    {
        int taskIndex = task.Project.Tasks.IndexOf(task);

        var elements = new List<VisualElement>();

        Label idLabel = new Label($"TaskID: {task.Id}");
        Label projectLabel = new Label($"Project: {task.Project.Name}");
        Label descLabel = new Label($"Description: {TextWrap.WrapText(task.Description,30)}");
        Label diffLabel = new Label($"Difficulty: {task.Difficulty}");
        Label priorityLabel = new Label($"Priority: {task.Priority}");
        Label specLabel = new Label($"Speciality: {task.Specialty.Name}");
        Label statusLabel = new Label($"Status: {task.Status}");
        Label assignedWorkersLabel = new Label($"Assigned workers: {string.Join(", ", task.Workers.Select(w => w.Name))}");
        Label assignWorkersLabel = new Label($"Assign workers: {string.Join(", ", task.Workers.Select(w => w.Name))}");

        VisualElement assignableWorkerContainer = new VisualElement();
        assignableWorkerContainer.name = "workerContainer";
        assignableWorkerContainer.style.flexDirection = FlexDirection.Row;
        
        VisualElement assignedWorkersContainer = new VisualElement();
        assignedWorkersContainer.name = "assignedWorkersContainer";
        assignedWorkersContainer.style.flexDirection = FlexDirection.Row;


        //elements.Add(idLabel);
        elements.Add(projectLabel);
        elements.Add(descLabel);
        elements.Add(diffLabel);
        elements.Add(priorityLabel);
        elements.Add(specLabel);
        elements.Add(statusLabel);

        elements.Add(assignedWorkersLabel);
        elements.Add(assignedWorkersContainer);

        elements.Add(assignWorkersLabel);
        elements.Add(assignableWorkerContainer);


        List<Worker> availableWorkers = Game.Workers.Where(w => w.Task == null && w.Health > 0).ToList();
        List<Worker> assignedWorkers = task.Workers.ToList();
        UpdateAssignableWorkers(task, assignableWorkerContainer, availableWorkers);
        Game.OnNewWorker += (_worker) =>
        {
            List<Worker> availableWorkers = Game.Workers.Where(w => w.Task == null).ToList();
            UpdateAssignableWorkers(task, assignableWorkerContainer, availableWorkers);
            List<Worker> assignedWorkers = task.Workers.ToList();
            UpdateAssignedWorkers(task, assignedWorkersContainer, task.Workers);
        };
        Game.OnWorkerAssigned += (_worker) =>
        {
            List<Worker> availableWorkers = Game.Workers.Where(w => w.Task == null).ToList();
            UpdateAssignableWorkers(task, assignableWorkerContainer, availableWorkers);
            List<Worker> assignedWorkers = task.Workers.ToList();
            UpdateAssignedWorkers(task, assignedWorkersContainer, task.Workers);
        };

        Game.OnWorkerFreed += (_worker) =>
        {
            List<Worker> availableWorkers = Game.Workers.Where(w => w.Task == null).ToList();
            UpdateAssignableWorkers(task, assignableWorkerContainer, availableWorkers);
            List<Worker> assignedWorkers = task.Workers.ToList();
            UpdateAssignedWorkers(task, assignedWorkersContainer, task.Workers);
        };

        void UpdateWorkerContainer(string status)
        {
            bool show = task.CanStart;

            assignableWorkerContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            assignedWorkersLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            assignedWorkersContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            assignWorkersLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }


        UpdateWorkerContainer(task.Status);

        task.OnStatusChanged += (status) =>
        {
            statusLabel.text = $"Status: {status}";
            UpdateWorkerContainer(status);
        };

        task.Project.OnTasksChanged += (tasks) =>
        {
            UpdateWorkerContainer(task.Status);
        };

        task.OnWorkersChanged += (workers) =>
        {
            assignedWorkersLabel.text = $"Assigned workers: {string.Join(", ", workers.Select(w => w.Name))}";
        };


        ProgressBar pBar = new ProgressBar()
        {
            value = task.Progress,
            lowValue = 0,
            highValue = 100,
            tooltip = "Progress",
        };
        pBar.Q<Label>(className: "unity-progress-bar__title").text = "Progress";

        var pBarFill = pBar.Q(className: "unity-progress-bar__progress");
        pBarFill.style.backgroundColor = ProgressBarColors.Green;

        task.OnProgressChanged += (progress) => pBar.value = progress;

        elements.Add(pBar);

        var window = new WindowUI($"{task.Priority}. {task.Name}", CreateFAIcon("file", 32, 32), elements, new Vector2(0, /*taskIndex * 100*/ 0),false);

        window.AddToClassList("task-window");
        
        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        window.Q<VisualElement>("window").style.position = Position.Relative;

        tasksContent.Add(window);
    }

    private static void UpdateAssignableWorkers(Task task, VisualElement assignableWorkersContainer, List<Worker> availableWorkers)
    {
        assignableWorkersContainer.Clear();
        foreach (Worker worker in availableWorkers)
        {
            Button assignButton = new Button
            {
                style =
                {
                    width = 50,
                    height = 50,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                }
            };

            Image portrait = new Image
            {
                sprite = worker.Portrait,
                scaleMode = ScaleMode.ScaleToFit,
                style = 
                    {
                        flexGrow = 1,
                        width = 50,
                        height = 50,
                    }
            };

            assignButton.Add(portrait);
            assignButton.AddToClassList("assign-worker");

            assignButton.clicked += () =>
            {
                task.AssignWorker(worker);
            };

            assignableWorkersContainer.Add(assignButton);
        }
    }
    private static void UpdateAssignedWorkers(Task task, VisualElement assignableWorkersContainer, List<Worker> assignedWorkers)
    {
        assignableWorkersContainer.Clear();
        foreach (Worker worker in assignedWorkers)
        {
            Button assignButton = new Button
            {
                style =
                {
                    width = 50,
                    height = 50,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                }
            };

            Image portrait = new Image
            {
                sprite = worker.Portrait,
                scaleMode = ScaleMode.ScaleToFit,
                style = 
                {
                    flexGrow = 1,
                    width = 50,
                    height = 50,
                }
            };

            assignButton.Add(portrait);
            assignButton.AddToClassList("un-assign-worker");

            assignButton.clicked += () =>
            {
                task.RemoveWorker(worker);
            };

            assignableWorkersContainer.Add(assignButton);
        }
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

        //elements.Add(idLabel);
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
        var hbl = hBar.Q<Label>(className: "unity-progress-bar__title").text = "Health"; ;


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
        sBar.Q<Label>(className: "unity-progress-bar__title").text = "Stress";

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
        eBar.Q<Label>(className: "unity-progress-bar__title").text = "Efficiency";

        var eBarFill = eBar.Q(className: "unity-progress-bar__progress");
        eBarFill.style.backgroundColor = ProgressBarColors.Cyan;

        worker.OnEfficiencyChanged += (value) => eBar.value = value;

        elements.Add(eBar);

        var window = new WindowUI(worker.Name, CreateFAIcon("user",32,32), elements, new Vector2(0, 0),false,true);

        window.OnHeaderChanged += (value) =>  worker.Name = value;

        window.AddToClassList("worker-window");
        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        window.Q<VisualElement>("window").style.position = Position.Relative;

        workersContent.Add(window);
    }
    private void NewAvailableWorkerUI(Worker worker)
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
        
        Label hireCostLabel = new Label($"Cost: {worker.HiringCost}$");

        //elements.Add(idLabel);
        elements.Add(levelLabel);
        elements.Add(xpLabel);
        elements.Add(nextXpLabel);
        elements.Add(skillLabel);
        elements.Add(specialtyLabel);
        elements.Add(hireCostLabel);


        bool canHire = Game.Money >= worker.HiringCost;
        hireCostLabel.style.color = canHire ? new StyleColor(Color.green) : new StyleColor(Color.red);

        Button btnSubmit = new()
        {
            text = "Hire!",
        };
        btnSubmit.clicked += () =>
        {
            Game.AddWorker(worker);
            Game.RemoveWorkerFromHirePool(worker);
        };
        btnSubmit.SetEnabled(canHire);

        elements.Add(btnSubmit);



        var window = new WindowUI(worker.Name, CreateFAIcon("person", 32, 32), elements, Vector2.zero, false);

        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        window.Q<VisualElement>("window").style.position = Position.Relative;

        window.AddToClassList("available-worker-window");

        staffingContent.Add(window);
    }

    private void NewMessageBox(string message, Color color)
    {
        Debug.Log("New message: " +  message);
        string time = DateTime.Now.ToString("HH:mm:ss");
        var label = new Label()
        {
            text = $"{time} - {message}",
            style =
            {
                color = color,
                marginBottom = 5,
            }
        };
        messagebox.Insert(0,label);
    }
}
