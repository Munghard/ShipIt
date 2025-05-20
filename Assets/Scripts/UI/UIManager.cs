using Assets.Scripts.Core.Config;
using Assets.Scripts.Data;
using Assets.Scripts.Models;
using Assets.Scripts.UI;
using Assets.Scripts.UI.HUD;
using Assets.Scripts.UI.Tooltip;
using Assets.Scripts.UI.Window;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public VisualElement configContent;

    //GAME UI
    Label moneyLabel;
    Label repLabel;
    Label workersLabel;

    TimeControlsUI TimeControlsUI;
    //GAME UI

    Game Game;
    public GameConfig GameConfig;

    public VisualElement Root;

    public static List<Sprite> Icons;

    private void Awake()
    {
        // load icons
        Icons = Resources.LoadAll<Sprite>("textures/IconsShadow-32").ToList();
    
        // Get root
        var uiDocument = GetComponent<UIDocument>();
        Root = uiDocument.rootVisualElement;

        // Get elements
        navbar = Root.Q<VisualElement>("navbar");
        workersContent = Root.Q<VisualElement>("workersContent");
        staffingContent = Root.Q<VisualElement>("staffingContent");
        availableProjectsContent = Root.Q<VisualElement>("availableProjectsContent");
        projectsContent = Root.Q<VisualElement>("projectsContent");
        tasksContent = Root.Q<VisualElement>("tasksContent");
        shopContent = Root.Q<VisualElement>("shopContent");
        messagebox = Root.Q<VisualElement>("msgBoxScrollView");
        gameButtons = Root.Q<VisualElement>("gameButtons");
    }

    void Start()
    {
        Game = new Game(this, GameConfig);

        TimeControlsUI = new TimeControlsUI(this, navbar, Game);
        ConfigUI configUI = new ConfigUI(this);

        SetupGameButtons();

        SetGameEventListeners(Game);
        Game.NewGame();
        BindGameToUI(Game);
    }

    private void SetupGameButtons()
    {
        Button btnNewGame = new();
        btnNewGame.clicked += () =>
        {
            ConfirmationWindow.Create(Root, () =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }, () => { });
        };
        SetButtonIcon(btnNewGame, "circle-play", "New game");

        Button btnLoad = new();
        btnLoad.clicked += () =>
        {
            ConfirmationWindow.Create(Root, () => LoadGame(), () => { });
        };
        SetButtonIcon(btnLoad, "folder-open", "Load game");

        Button btnSave = new();
        btnSave.clicked += () =>
        {
            ConfirmationWindow.Create(Root, () => SaveGame(), () => { });
        };
        SetButtonIcon(btnSave, "floppy-disk", "Save game");

        gameButtons.Add(btnNewGame);
        gameButtons.Add(btnLoad);
        gameButtons.Add(btnSave);
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
        Game = new Game(this, GameConfig);

        ConfigUI configUI = new ConfigUI(this);
        
        SetGameEventListeners(Game);

        ClearContainers();

        BindGameToUI(Game);

        GameData loadedData = SaveManager.LoadGame();
        if (loadedData != null)
        {
            // Set loaded data to game state
            
            Game.SetSimulationTime(loadedData.SimulationTime);
            Game.SetMoney(loadedData.Money);
            Game.SetReputation(loadedData.Reputation);

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
                    happiness: loadedWorker.Happiness,
                    project: null,
                    game: Game,
                    health: loadedWorker.Health,
                    stress: loadedWorker.Stress,
                    level: loadedWorker.Level,
                    xp: loadedWorker.Xp,
                    id: loadedWorker.Id,
                    location: loadedWorker.Location,
                    traits: new List<Trait>(loadedWorker.Traits.Select(t => Game.TraitManager.GetTrait(t)))
                ));

            }
            foreach (var loadedWorker in loadedData.AvailableWorkers)
            {
                Game.WorkersForHire.Add(new Worker(
                    name: loadedWorker.Name,
                    portrait: Game.workerGenerator.GetPortrait(loadedWorker.PortraitIndex),
                    specialty: Specialty.Get(loadedWorker.Specialty),
                    skill: loadedWorker.Skill,
                    project: null,
                    game: Game,
                    health: loadedWorker.Health,
                    stress: loadedWorker.Stress,
                    level: loadedWorker.Level,
                    xp: loadedWorker.Xp,
                    id: loadedWorker.Id,
                    location: loadedWorker.Location,
                    traits: new List<Trait>(loadedWorker.Traits.Select(t => Game.TraitManager.GetTrait(t)))
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
            foreach (var loadedProject in loadedData.AvailableProjects)
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
                foreach (var task in project.Tasks)
                {
                    task.Project = project;
                }
                Game.AvailableProjects.Add(project);
                Game.OnAvailableProjectsChanged?.Invoke(Game.AvailableProjects);
            }
        }
        Game.textPop.New("Game loaded!", Vector2.zero, Color.magenta);
        //update everything

    }

    private void SetGameEventListeners(Game game)
    {
        game.OnNewProject += NewProjectUI;

        game.OnAvailableProjectsChanged += AvailableProjectsChanged;

        game.OnProjectsChanged += ProjectsChanged;

        //foreach (var project in Game.Projects)
        //{
        //    project.OnTasksChanged += TasksChanged;
        //}

        game.OnAvailableWorkersChanged += AvailableWorkersChanged;

        game.OnNewWorker += NewWorkerUI;

        game.OnWorkersChanged += WorkersChanged;

        game.textPop.OnNewTextPop += NewMessageBox;

        game.OnProjectsChanged += UpdateShop;

        game.OnWorkerQuit +=(worker)=> ConfirmationWindow.Create(Root,null,null,$"{worker.Name} had enough and quit.");

        //game.OnBusinessOpenChanged += (val) => ProjectsChanged(game.Projects);
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
                Happiness = worker.Happiness,
                Stress = worker.Stress,
                PortraitIndex = Game.workerGenerator.GetPortraitIndex(worker.Portrait),
                Specialty = worker.Specialty.Name,
                Xp = worker.Xp,
                Location = worker.Location,
                Traits = worker.Traits.Select(t => t.TraitName).ToList()
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
                Happiness = worker.Happiness,
                Stress = worker.Stress,
                PortraitIndex = Game.workerGenerator.GetPortraitIndex(worker.Portrait),
                Specialty = worker.Specialty.Name,
                Xp = worker.Xp,
                Location = worker.Location,
                Traits = worker.Traits.Select(t => t.TraitName).ToList()
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
        if(list.Count == 0)
        {
            var btn = new Button();
            btn.text = "Quick add project";
            btn.clicked += () => 
            {
                var project = Game.AvailableProjects.FirstOrDefault();
                Game.RemoveProjectFromAvailableProjects(project);
                Game.AddProject(project);
            };
            projectsContent.Add(btn);
        }
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

        TimeControlsUI = new(this,navbar,Game);

        CreateStatsContainer();
        //CreateThemeButton();

    }
    public void BindGameToUI(Game game)
    {
        CreateNavBar();

        // Unhook previous bindings (if any)

        game.OnMoneyChanged -= OnMoneyChangedHandler;
        game.OnReputationChanged -= OnReputationChangedHandler;

        // Hook new bindings

        game.OnMoneyChanged += OnMoneyChangedHandler;
        game.OnReputationChanged += OnReputationChangedHandler;



        void OnMoneyChangedHandler(int money)
        {
            if (moneyLabel != null)
                moneyLabel.text = $"{money}$";
        }

        void OnReputationChangedHandler(int rep)
        {
            if (repLabel != null)
                repLabel.text = $"{rep} Rep";
        }


        
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
        var oldSC = navbar.Q<VisualElement>("stats-container");
        if (oldSC != null) navbar.Remove(oldSC);

        VisualElement statsContainer = new VisualElement();
        statsContainer.AddToClassList("container");
        statsContainer.name = "stats-container";

        navbar.Add(statsContainer);

        Label statsLabel = new Label("Stats");
        statsLabel.AddToClassList("navbar-label-header");

        statsContainer.Add(statsLabel);

        moneyLabel = new Label($"{Game.Money}$");
        Game.OnMoneyChanged += (money) => moneyLabel.text = $"{Game.Money}$";
        moneyLabel.style.color = Color.green;
        moneyLabel.AddToClassList("navbar-label");

        statsContainer.Add(moneyLabel);

        repLabel = new Label($"{Game.Reputation}Rep");
        Game.OnReputationChanged += (rep) => repLabel.text = $"{Game.Reputation}Rep";
        repLabel.style.color = Color.yellow;
        repLabel.AddToClassList("navbar-label");

        statsContainer.Add(repLabel);
        
        workersLabel = new Label($"Workers: {Game.Workers.Count}/{Game.MaximumAmountOfWorkers}");
        Game.OnWorkersChanged += (workers) => workersLabel.text = $"Workers: {Game.Workers.Count}/{Game.MaximumAmountOfWorkers}";
        workersLabel.style.color = Color.magenta;
        workersLabel.AddToClassList("navbar-label");

        statsContainer.Add(workersLabel);
    }

    

    
    public void SetButtonIcon(Button btn, string name, string text)
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
        Label permanenceLabel = new Label($"Permanent: {buyable.SingleBuy}");

        elements.Add(nameLabel);
        elements.Add(descriptionLabel);
        elements.Add(permanenceLabel);
        elements.Add(costLabel);
        elements.Add(reputationLabel);

        Button btnSubmit = new() { text = "Buy!" };
        Label purchasedLabel = new() { text = "Purchased!" };
        bool hasBuyable = Game.acquiredBuyables.Contains(buyable) && buyable.SingleBuy;

        void UpdateBuyableUI()
        {
            bool acquired = Game.acquiredBuyables.Contains(buyable) && buyable.SingleBuy;
            
            bool canBuy = buyable.CanPurchase(Game.Money, Game.Reputation);
            bool hasRep = buyable.HasRep(Game.Reputation);
            bool hasMoney = buyable.HasMoney(Game.Money);

            costLabel.style.color = hasMoney ? Color.green : Color.red;

            reputationLabel.style.color = hasRep ? Color.green : Color.red;

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
                elements.Add(new Label("Already acquired."));
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

        var window = new WindowUI(buyable.Name, elements, Vector2.zero, CreateFAIcon("cart-shopping", 32, 32), false);
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

            var timeString = $"Time left: {totalHours}h {duration.Minutes}m / {totalStartHours}h {startDuration.Minutes}m";


            durationLabel.text = timeString;
        };

        elements.Add(dlBar);
        elements.Add(pBar);

        elements.Add(btnContainer);

        UpdateProjectButtons(project, btnContainer);
        
        project.OnStatusChanged += val => UpdateProjectButtons(project, btnContainer);

        var window = new WindowUI(project.Name, elements, Vector2.zero, CreateFAIcon("folder-closed", 32, 32), false);

        window.AddToClassList("project-window");

        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        window.Q<VisualElement>("window").style.position = Position.Relative;
        
        project.OnDurationChanged += (value) => {
            dlBar.value = value;
            if (value < 60)
            {
                WindowAlerts.DoAlert(this, window, "alert-red", 3, "Alarm");
            }
        };

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
                    ConfirmationWindow.Create(Message: $"Chance to get paid: {chance * 100f}%, are you sure?",Parent:Root, OkCallback:() =>
                    {
                        project.TurnInProject();
                    }, NoCallback: () => { });

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

        var window = new WindowUI(project.Name, elements, Vector2.zero, CreateFAIcon("folder-closed", 32, 32),false);
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
        //if (task == null || task.Project == null || task.Project.Tasks.Count <= 0) return;
        int taskIndex = task.Project.Tasks.IndexOf(task);

        var elements = new List<VisualElement>();

        Label idLabel = new Label($"TaskID: {task.Id}");
        Label projectLabel = new Label($"Project: {task.Project.Name}");
        Label descLabel = new Label($"Description: {TextWrap.WrapText(task.Description,30)}");
        Label diffLabel = new Label($"Difficulty: {task.Difficulty}");
        Label priorityLabel = new Label($"Priority: {task.Priority}");

        Label specLabel = new Label($"Speciality: {task.Specialty.Name}");
        specLabel.style.color = Color.yellow;

        Label statusLabel = new Label($"Status: {task.Status}");
        Label workersLabel = new Label($"Workers: {string.Join(", ", task.Workers.Select(w => w.Name))}");

        
        VisualElement workersContainer = new VisualElement();
        workersContainer.name = "WorkersContainer";
        workersContainer.style.flexDirection = FlexDirection.Row;
        workersContainer.style.display = DisplayStyle.Flex;


        //elements.Add(idLabel);
        elements.Add(projectLabel);
        elements.Add(descLabel);
        elements.Add(diffLabel);
        elements.Add(priorityLabel);
        elements.Add(specLabel);
        elements.Add(statusLabel);

        elements.Add(workersContainer);

        
        
        UpdateTaskWorkers(task, workersContainer, Game.Workers);


        Game.OnNewWorker += (_worker) => UpdateTaskWorkers(task, workersContainer, Game.Workers);
        Game.OnWorkerAssigned += (_worker) => UpdateTaskWorkers(task, workersContainer, Game.Workers);
        Game.OnWorkerFreed += (_worker) => UpdateTaskWorkers(task, workersContainer, Game.Workers);
        Game.OnWorkerDied += (_worker) => UpdateTaskWorkers(task, workersContainer, Game.Workers);
        Game.OnBusinessOpenChanged += (_val) => UpdateWorkerContainer(task.Status);

        void UpdateWorkerContainer(string status)
        {
            //bool show = task.CanStart; // override can start
            bool show = task.Status != "completed" && task.Status != "failed" && Game.BusinessOpen; // override can start

            workersContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            workersLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
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
            workersLabel = new Label($"Workers: {string.Join(", ", task.Workers.Select(w => w.Name))}");
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

        var window = new WindowUI($"{task.Priority}. {task.Name}", elements, new Vector2(0, 0), CreateFAIcon("file", 32, 32),false);

        window.AddToClassList("task-window");
        
        window.style.position = Position.Relative;
        window.style.paddingRight = 16;
        window.style.paddingLeft = 16;
        window.style.paddingTop = 16;
        window.style.paddingBottom = 16;
        window.Q<VisualElement>("window").style.position = Position.Relative;

        tasksContent.Add(window);

        task.OnStatusChanged += (status) => WindowAlerts.DoAlert(this, window, "alert-green",1, "TurningPage");

    }

    private static void UpdateTaskWorkers(Task task, VisualElement workersContainer, List<Worker> workers)
    {
        workersContainer.Clear();
        foreach (Worker worker in workers.Where(w=>w.CanWork()))
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

            if(worker.Task == null)
            {
                if (worker.Specialty.Name == task.Specialty.Name ||worker.Specialty.Name == "General")
                {
                    assignButton.AddToClassList("assign-worker");
                }
                else
                {
                    assignButton.AddToClassList("assign-worker-untrained");
                }
                assignButton.clicked += () =>
                {
                    task.AssignWorker(worker);
                };

                workersContainer.Add(assignButton);

            }
            else if(worker.Task == task)
            {
                assignButton.AddToClassList("un-assign-worker");
                assignButton.clicked += () =>
                {
                    task.RemoveWorker(worker);
                };

                workersContainer.Add(assignButton);
            }
            else
            {
                // dont add it anywhere
            }


        }
    }



    private void NewWorkerUI(Worker worker)
    {
        var workerIndex = Game.Workers.IndexOf(worker);

        var elements = new List<VisualElement>();

        // containers+
        VisualElement mainContainer = new VisualElement(); 
        mainContainer.style.flexDirection = FlexDirection.Row;



        VisualElement leftContainer = new VisualElement();
        leftContainer.style.flexDirection = FlexDirection.Column;


        VisualElement rightContainer = new VisualElement();
        rightContainer.style.flexDirection = FlexDirection.Column;

        rightContainer.style.paddingLeft = 16;

        rightContainer.style.display = DisplayStyle.None; // default to hidden


        var btnInfo = new Button(() =>
        {
            rightContainer.style.display = rightContainer.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        });
        btnInfo.text = "i";
        
        btnInfo.style.position = Position.Absolute;
        btnInfo.style.bottom = 0;
        btnInfo.style.right = 0;
        btnInfo.AddToClassList("button-small");
        


        Label lblSpecialty = new Label(new string(worker.Specialty.Name.Take(2).ToArray()).ToUpper());
        lblSpecialty.style.top = 0;
        lblSpecialty.style.left = 0;
        lblSpecialty.style.position = Position.Absolute;
        lblSpecialty.style.color = Color.yellow;

        lblSpecialty.AddToClassList("info-label");

        Label lblLevel = new Label(new string(worker.Level.ToString()));
        lblLevel.style.top = 0;
        lblLevel.style.right = 0;
        lblLevel.style.position = Position.Absolute;
        lblLevel.style.color = Color.cyan;
        lblLevel.style.backgroundColor = new Color(0.2f,0.2f,0.2f);
        lblLevel.tooltip = "Level:" + worker.Level.ToString();

        lblLevel.AddToClassList("info-label");


        elements.Add(mainContainer);

        mainContainer.Add(leftContainer);
        
        mainContainer.Add(rightContainer);

        // containers


        Image imgPortrait = new()
        {
            sprite = worker.Portrait,
        };
        imgPortrait.AddToClassList("portrait");

        imgPortrait.Add(btnInfo);
        imgPortrait.Add(lblSpecialty);
        imgPortrait.Add(lblLevel);

        leftContainer.Add(imgPortrait);

        var ttElements = new List<VisualElement>()
        {
            new Image()
            {
                sprite = worker.Portrait,
            },
            new Label()
            {
                text = $"" +
                $"{worker.Name}\n" +
                $"Traits: {string.Join(", ", worker.Traits.Select(t=>t.TraitName))}\n" +
                $"Tasks completed: {worker.TasksCompleted}\n"
            }
        };
        Tooltip.RegisterTooltip(Root, imgPortrait, ttElements);


        Button btnFire = new Button()
        {
            text = "Fire",
        };

        btnFire.clicked += () =>
        {
            ConfirmationWindow.Create(Parent: Root, Message: $"Are you sure you want to fire {worker.Name}?", OkCallback: () =>
            {
                Game.RemoveWorker(worker);
                Game.textPop.New("Fired: " + worker.Name, Vector2.zero, Color.red);
            }, NoCallback: () => { });
        };

        Label idLabel = new Label($"WorkerID: {worker.Id}");
        Label levelLabel = new Label($"Level: {worker.Level}");
        Label xpLabel = new Label($"Current xp: {worker.Xp}");
        Label nextXpLabel = new Label($"Next xp: {worker.NextXp}");
        Label skillLabel = new Label($"Skill: {worker.Skill}");
        Label specialtyLabel = new Label($"Speciality: {worker.Specialty.Name}");
        Label statusLabel = new Label($"Status: {worker.Status}");
        Label taskLabel = new Label($"Task: {worker.Task?.Name}");
        Label locationLabel = new Label($"Location: {worker.Location}");
        Label traitsLabel = new Label($"Traits: {string.Join(", ",worker.Traits.Select(t=>t.TraitName))}"); // needs a ontraitschanged event to update

        worker.OnTraitsChanged += (traits) =>
        {
            traitsLabel.text = $"Traits: {string.Join(", ", worker.Traits.Select(t => t.TraitName))}";
        };


        rightContainer.Add(levelLabel);
        rightContainer.Add(xpLabel);
        rightContainer.Add(nextXpLabel);
        rightContainer.Add(skillLabel);
        rightContainer.Add(specialtyLabel);
        rightContainer.Add(statusLabel);
        rightContainer.Add(locationLabel);
        rightContainer.Add(taskLabel);
        rightContainer.Add(traitsLabel);

        rightContainer.Add(btnFire);

        // Bind update events
        worker.OnLevelChanged += val => levelLabel.text = $"Level: {val}";
        worker.OnXpChanged += val => xpLabel.text = $"Current xp: {val}";
        worker.OnNextXpChanged += val => nextXpLabel.text = $"Next xp: {val}";
        worker.OnSkillChanged += val => skillLabel.text = $"Skill: {val}";
        //worker.OnSpecialtyChanged += val => specialtyLabel.text = $"Speciality: {val}";
        worker.OnStatusChanged += val => statusLabel.text = $"Status: {val}";
        worker.OnTaskChanged += val => taskLabel.text = $"Task: {val?.Name}";
        worker.OnLocationChanged += val => locationLabel.text = $"Location: {val}";


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
        sBarFill.style.backgroundColor = ProgressBarColors.Orange;

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

        ProgressBar xpBar = new ProgressBar()
        {
            value = worker.Xp,
            lowValue = 0,
            highValue = worker.NextXp,
            tooltip = "Experience"
        };
        xpBar.Q<Label>(className: "unity-progress-bar__title").text = "Experience";

        var xpBarFill = xpBar.Q(className: "unity-progress-bar__progress");
        xpBarFill.style.backgroundColor = ProgressBarColors.Yellow;

        worker.OnXpChanged += (value) => xpBar.value = value;
        worker.OnNextXpChanged += (value) => xpBar.highValue = value;
        
        ProgressBar haBar = new ProgressBar()
        {
            value = worker.Happiness,
            lowValue = 0,
            highValue = worker.MaxHappiness,
            tooltip = "Happiness"
        };
        haBar.Q<Label>(className: "unity-progress-bar__title").text = "Happiness";

        var haBarFill = haBar.Q(className: "unity-progress-bar__progress");
        haBarFill.style.backgroundColor = ProgressBarColors.Green;

        worker.OnHappinessChanged += (value) => haBar.value = value;

        if(worker.Status != "dead")
        {
            leftContainer.Add(sBar);
            leftContainer.Add(hBar);
            leftContainer.Add(eBar);
            leftContainer.Add(xpBar);
            leftContainer.Add(haBar);
        }

        var window = new WindowUI(worker.Name, elements, new Vector2(0, 0),null /*CreateFAIcon("user", 32, 32)*/,false,true,false);

        worker.OnStressChanged += (stress) => {
        if (stress >= 100)
            {
                WindowAlerts.DoAlert(this, window, "alert-red", 3, "Alert"); 
            }
        };
        worker.OnXpChanged += (xp) =>  WindowAlerts.DoAlert(this, window, "alert-yellow", 1, "Writing") ;
        worker.OnLevelChanged+= (xp) =>  WindowAlerts.DoAlert(this, window, "alert-yellow", 1, "Stamping") ;

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
        Label traitsLabel = new Label($"Traits: {string.Join(", ", worker.Traits?.Where(t => t != null && t.TraitName != null).Select(t => t.TraitName) ?? new List<string>())}");


        //elements.Add(idLabel);
        elements.Add(levelLabel);
        elements.Add(xpLabel);
        elements.Add(nextXpLabel);
        elements.Add(skillLabel);
        elements.Add(specialtyLabel);
        elements.Add(hireCostLabel);
        elements.Add(traitsLabel);


        bool canHire = Game.HasMoney(worker.HiringCost) && Game.HasSpaceForWorker();
        hireCostLabel.style.color = canHire ? new StyleColor(Color.green) : new StyleColor(Color.red);

        Button btnSubmit = new()
        {
            text = "Hire!",
        };
        btnSubmit.clicked += () =>
        {
            int currentCost = worker.HiringCost; // Or a method like worker.GetCurrentHiringCost()
            Game.SpendMoney(currentCost);
            Game.AddWorker(worker);
            Game.RemoveWorkerFromHirePool(worker);
            
        };
        btnSubmit.SetEnabled(canHire);

        elements.Add(btnSubmit);

        Game.OnMoneyChanged += (money) =>
        {
            bool _canHire = Game.HasMoney(worker.HiringCost);
            hireCostLabel.style.color = _canHire ? new StyleColor(Color.green) : new StyleColor(Color.red);
            btnSubmit.SetEnabled(_canHire);
        };


        var window = new WindowUI(worker.Name, elements, Vector2.zero, CreateFAIcon("person", 32, 32), false);

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
