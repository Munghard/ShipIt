using Assets.Scripts.UI.Window;
using System;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.HUD
{
    public class TimeControlsUI
    {
        public Label businessOpenLabel, dayLabel, simulationTimeLabel, timeScaleLabel;
        private Button btnUp, btnDown, btnPause, btnPassTime, btnPlay, btnPassTimeTillMorning;

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
            
            dayLabel = new Label($"Day: {game.SimulationDay}")
            {
                name = "dayLabel",
                style = { fontSize = 25 }
            };
            vTimeContainer.Add(dayLabel);

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

            var hpTimeContainer = new VisualElement();
            hpTimeContainer.style.flexDirection = FlexDirection.Row;

            btnPassTime = new Button();
            uiManager.SetButtonIcon(btnPassTime, "forward-fast", "Pass time");

            btnPassTimeTillMorning = new Button();
            uiManager.SetButtonIcon(btnPassTimeTillMorning, "forward-fast", "Pass day");

            hpTimeContainer.Add(btnPassTime);
            hpTimeContainer.Add(btnPassTimeTillMorning);

            vTimeContainer.Add(hTimeContainer);
            vTimeContainer.Add(hpTimeContainer);
        }

        private void BindEvents()
        {
            game.OnBusinessOpenChanged -= OnBusinessOpenedChanged;
            game.OnBusinessOpenChanged += OnBusinessOpenedChanged;

            game.OnTimeScaleChanged -= OnTimeScaleChanged;
            game.OnTimeScaleChanged += OnTimeScaleChanged;

            game.OnNewDay -= OnNewDay;
            game.OnNewDay += OnNewDay;

            btnPassTime.clicked -= OnPassTimeClicked;
            btnPassTime.clicked += OnPassTimeClicked;
            
            btnPassTimeTillMorning.clicked -= OnPassTimeTillMorningClicked;
            btnPassTimeTillMorning.clicked += OnPassTimeTillMorningClicked;

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

        private void OnNewDay(int day)
        {
            dayLabel.text = $"Day: {day}";
        }

        private void OnPassTimeTillMorningClicked()
        {
            var timeTillMorning = 0f;
            if(game.GetCurrentHour > 0 && game.GetCurrentHour < 6)
            {
                timeTillMorning =  6f - game.GetCurrentHour;
            }
            else
            {
                timeTillMorning = (24 - game.GetCurrentHour) + 6f;
            }

            ConfirmationWindow.Create(
                Message: $"passing {timeTillMorning.ToString("F1")}h, are you sure?", // add projects with deadline calcs here later
                Parent: uiManager.Root,
                OkCallback: () => game.PassTime(60 * timeTillMorning),
                NoCallback: () => { }
            );
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