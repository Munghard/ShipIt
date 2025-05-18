using Assets.Scripts.UI.Window;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.HUD
{
    public class TimeControlsUI
    {
        public Label businessOpenLabel;
        public Label simulationTimeLabel;
        public Label timeScaleLabel;
        private Button btnUp, btnDown, btnPause, btnPassTime, btnPlay;

        private readonly UIManager uiManager;
        private readonly Game game;

        public TimeControlsUI(UIManager uiManager, VisualElement navbar, Game game)
        {
            this.uiManager = uiManager;
            this.game = game;

            CreateUI(navbar);
            BindEvents();
        }

        private void CreateUI(VisualElement navbar)
        {
            var existing = navbar.Q<VisualElement>("TimeControls");
            if (existing != null) navbar.Remove(existing);

            var vTimeContainer = new VisualElement { name = "TimeControls" };
            vTimeContainer.style.flexDirection = FlexDirection.Column;
            vTimeContainer.AddToClassList("container");

            navbar.Add(vTimeContainer);

            businessOpenLabel = new Label($"Business {(game.BusinessOpen ? "open" : "closed")}")
            {
                name = "businessOpenLabel",
                style = { fontSize = 25 }
            };
            vTimeContainer.Add(businessOpenLabel);
            
            simulationTimeLabel = new Label($"Time: {game.SimulationTime}")
            {
                name = "simulationTimeLabel",
                style = { fontSize = 25 }
            };
            vTimeContainer.Add(simulationTimeLabel);

            timeScaleLabel = new Label($"Time scale: {game.TimeScale}")
            {
                style = { fontSize = 25 }
            };
            timeScaleLabel.AddToClassList("navbar-label");
            vTimeContainer.Add(timeScaleLabel);

            var hTimeContainer = new VisualElement();
            hTimeContainer.style.flexDirection = FlexDirection.Row;

            btnDown = new Button();
            uiManager.SetButtonIcon(btnDown, "backward", "");
            hTimeContainer.Add(btnDown);

            btnPlay = new Button();
            uiManager.SetButtonIcon(btnPlay, "play", "");
            hTimeContainer.Add(btnPlay);

            btnUp = new Button();
            uiManager.SetButtonIcon(btnUp, "forward", "");
            hTimeContainer.Add(btnUp);

            btnPause = new Button();
            uiManager.SetButtonIcon(btnPause, "pause", "");
            hTimeContainer.Add(btnPause);

            btnPassTime = new Button();
            uiManager.SetButtonIcon(btnPassTime, "forward-fast", "Pass time");

            vTimeContainer.Add(hTimeContainer);
            vTimeContainer.Add(btnPassTime);
        }

        private void BindEvents()
        {
            game.OnBusinessOpenChanged -= OnBusinessOpenedChanged;
            game.OnBusinessOpenChanged += OnBusinessOpenedChanged;

            game.OnTimeScaleChanged -= OnTimeScaleChanged;
            game.OnTimeScaleChanged += OnTimeScaleChanged;

            btnPassTime.clicked -= OnPassTimeClicked;
            btnPassTime.clicked += OnPassTimeClicked;

            btnPause.clicked -= game.TogglePaused;
            btnPause.clicked += game.TogglePaused;

            btnUp.clicked -= game.DoubleTimeScale;
            btnUp.clicked += game.DoubleTimeScale;

            btnDown.clicked -= game.HalveTimeScale;
            btnDown.clicked += game.HalveTimeScale;

            System.Action playAction = () => game.SetTimeScale(1f);

            btnPlay.clicked -= playAction;
            btnPlay.clicked += playAction;
        }

        private void OnTimeScaleChanged(float time)
        {
            if (timeScaleLabel != null)
                timeScaleLabel.text = $"Time scale: {time}x";
        }
        private void OnBusinessOpenedChanged(bool open)
        {
            if (businessOpenLabel != null)
                businessOpenLabel.text = $"Business {(open ? "open" : "closed")}";
        }
        

        private void OnPassTimeClicked()
        {
            ConfirmationWindow.Create(
                Message: "passing 4 hours, are you sure?",
                Parent: uiManager.Root,
                OkCallback: () => game.PassTime(60 * 4),
                NoCallback: () => { }
                );
        }
    }
}