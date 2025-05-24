using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using Assets.Scripts.Utils;


namespace Assets.Scripts.Gameplay.MiniGames
{
    public class TimingMiniGame : BaseMiniGame
    {
        private bool triggered;

        private Slider slider;
        private Label lblDescription, lblResult;
        private bool CanConfirm = false;
        private float Result = 0f;

        List<float> Stops = new List<float>() { 2f, 1f, 0.5f, 0.25f, 0.125f, 0f };

        public override string Title => "Timing";
        public override string Description => "Stop the slider at the right time for the best stressreduction";
        public override List<VisualElement> CreateUIElements()
        {
            var elements = new List<VisualElement>();

            slider = new Slider
            {
                name = "slider",
                lowValue = 0,
                highValue = 1
            };
            slider.style.width = 600;
            slider.style.height = 50;

            lblDescription = new Label
            {
                name = "lblDescription",
                text = TextWrap.WrapText(Description, 50)
            };

            lblResult = new Label
            {
                name = "lblResult",
                text = "Result: "
            };

            elements.Add(lblDescription);
            elements.Add(slider);
            elements.Add(lblResult);

            for (int i = 0; i < Stops.Count; i++)
            {
                var stop = Stops[i];
                var lbl = new Label { text = $"{stop}x" };
                lbl.style.position = Position.Absolute;
                float t = i / (float)(Stops.Count - 1);
                lbl.style.left = t * 600 - 10;
                lbl.style.top = 25; // halfway down the slider
                slider.Add(lbl);
            }

            return elements;
        }

        protected override float GetResult()
        {
            return Result;
        }




        //public WindowUI Create(UIManager uIManager,Game game,int difficulty,Action<float> OkCallback, Action NoCallback) {

        //    WindowUI window = null;
        //    VisualElement blocker = null;


        //    Debug.Log("Creating minigame window");
        //    var elements = new List<VisualElement>();
        //    var sliderWidth = 600;
        //    var sliderHeight = 50;
        //    //Create elements for minigame
        //    slider = new Slider();
        //    slider.name = "slider";
        //    slider.style.width = sliderWidth;
        //    slider.style.height = sliderHeight;
        //    slider.lowValue = 0;
        //    slider.highValue = 1;

        //    elements.Add(slider);


        //    lblDescription = new Label();
        //    lblDescription.name = "lblDescription";
        //    lblDescription.text = TextWrap.WrapText("Press space when at the end for best performance, you can only roll once per task per worker", 50);

        //    lblResult = new Label();
        //    lblResult.name = "lblResult";
        //    lblResult.text = "Result: ";



        //    elements.Add(lblDescription);
        //    elements.Add(slider);
        //    elements.Add(lblResult);


        //    void DestroyWindow()
        //    {
        //        window?.RemoveFromHierarchy();
        //        blocker?.RemoveFromHierarchy();
        //        game.Paused = false;
        //        if (Coroutine != null) uIManager.StopCoroutine(Coroutine);
        //    }

        //    yesButton = new Button(() => {
        //        OkCallback?.Invoke(Result);
        //        DestroyWindow();
        //    })
        //    { text = "Yes" };
        //    yesButton.SetEnabled(CanConfirm);

        //    noButton = new Button(() => {
        //        NoCallback?.Invoke();
        //        DestroyWindow();
        //    })
        //    { text = "No" };

        //    yesButton.focusable = true;

        //    noButton.focusable = true;

        //    if (yesButton.enabledSelf)
        //    {
        //        yesButton.schedule.Execute(() => yesButton.Focus()).ExecuteLater(1);
        //    }
        //    else
        //    {
        //        noButton.schedule.Execute(() => noButton.Focus()).ExecuteLater(1);
        //    }

        //    var buttonContainer = new VisualElement();
        //    buttonContainer.Add(yesButton);
        //    if (NoCallback != null)
        //    {
        //        buttonContainer.Add(noButton);
        //    }
        //    buttonContainer.style.flexDirection = FlexDirection.Row;
        //    buttonContainer.style.justifyContent = Justify.Center;
        //    buttonContainer.style.marginTop = 10;
        //    buttonContainer.style.marginBottom = 10;
        //    buttonContainer.style.marginLeft = 10;
        //    buttonContainer.style.marginRight = 10;

        //    elements.Add(buttonContainer);

        //    blocker = new VisualElement();
        //    blocker.style.position = Position.Absolute;
        //    blocker.style.top = 0;
        //    blocker.style.left = 0;
        //    blocker.style.right = 0;
        //    blocker.style.bottom = 0;
        //    blocker.style.backgroundColor = new Color(0, 0, 0, 0.8f); // transparent
        //    blocker.pickingMode = PickingMode.Position; // blocks mouse input


        //    window = new WindowUI("Timing minigame",elements,Vector2.zero,null,false,false,false);
        //    window.style.position = Position.Absolute;
        //    window.style.position = Position.Absolute;
        //    window.style.top = Length.Percent(50);
        //    window.style.left = Length.Percent(50);
        //    window.style.transformOrigin = new TransformOrigin(50, 50); // center point
        //    window.style.translate = new Translate(-50, -50, 0); // offset back by half



        //    uIManager.Root.Add(blocker); // Add the window to the root element
        //    uIManager.Root.Add(window); // Add the window to the root element



        //    // slider stops after window creation
        //    // add stops after slider added

        //    for (int i = 0; i < Stops.Count; i++)
        //    {
        //        var stop = Stops[i];
        //        var lbl = new Label() { text = $"{stop}x" };
        //        lbl.style.position = Position.Absolute;

        //        // Normalize position across slider width
        //        float t = i / (float)(Stops.Count - 1); // normalized 0 to 1
        //        lbl.style.left = t * sliderWidth - 10;

        //        lbl.style.top = sliderHeight / 2;
        //        slider.Add(lbl);
        //    }

        //    Coroutine = uIManager.StartCoroutine(MiniGameCoroutine(difficulty));

        //    return window;
        //}



        protected override IEnumerator MiniGameCoroutine(int difficulty)
        {
            Debug.Log("Started minigame coroutine");
            triggered = true;

            while (true)
            {
                if (triggered)
                {
                    // Minigame running
                    slider.value = Mathf.Repeat(Time.time * difficulty, 1f);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        triggered = false;
                        Debug.Log("Minigame stopped!");
                        
                        AnimationCurve animationCurve = new AnimationCurve();
                        for (int i = 0; i < Stops.Count; i++)
                        {
                            float t = i / (float)(Stops.Count - 1); // normalized position from 0 to 1
                            animationCurve.AddKey(new Keyframe(t, Stops[i]));
                        }
                        float res = animationCurve.Evaluate(slider.value);

                        lblResult.text = $"Result: {res:F1}";
                        CanConfirm = true;
                        yesButton.SetEnabled(CanConfirm);
                        yesButton.Focus();
                        noButton.SetEnabled(!CanConfirm);
                        Result = res;
                    }
                }
                else
                {
                    //// Waiting for retrigger
                    //if (Input.GetKeyDown(KeyCode.Space))
                    //{
                    //    triggered = true;
                    //    Debug.Log("Minigame restarted!");
                    //}
                }
                yield return null;
            }
        }

        // pause everything but the minigame,
        // create new window,
        // create some kind of slider ui that goes from left to right and back,
        // write description of the minigame in the window,
        // start coroutine that moves the slider back and forth,
        // waits for player to press space or click when the slider is in the middle,
        // the closer to the middle the better,
        // gives efficiency bonus or something,
        // then closes the window and unpauses the game
        // slider speed is based on the difficulty of the task
    }
}
