using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TextPop
{
    UIManager UIManager;
    List<Label> activePops = new List<Label>();

    public System.Action<string, Color> OnNewTextPop;

    public TextPop(UIManager uIManager)
    {
        UIManager = uIManager;
    }

    public void New(string text, Vector2 position, Color color)
    {
        //CreateOnScreenText(text, ref position, color);

        OnNewTextPop?.Invoke(text, color); // sends to messagebox
    }

    private void CreateOnScreenText(string text, ref Vector2 position, Color color)
    {
        var root = UIManager.Root;
        if (root == null) return;

        position = new Vector2(Screen.width / 2, Screen.height / 2);

        float yOffset = activePops.Count * 60f; // 40 pixels per pop (adjust as needed)

        Label pop = new Label();
        pop.text = text;
        pop.style.position = Position.Absolute;
        pop.AddToClassList("TextPop");
        pop.style.left = position.x;
        pop.style.top = position.y + yOffset;
        pop.style.color = color;
        pop.style.scale = new StyleScale(new Vector2(1, 1));

        root.Add(pop);
        activePops.Add(pop);

        UIManager.StartCoroutine(Animate(pop, position, root));
    }

    private IEnumerator Animate(Label pop, Vector2 pos, VisualElement root)
    {
        float entryDuration = 0.3f;    // Time to move from left to middle
        float holdDuration = 2f;       // Time to hold at the center
        float exitDuration = 1f;       // Time to move from middle to right
        float totalDuration = entryDuration + holdDuration + exitDuration;
        float timeElapsed = 0f;

        float maxRotation = 2.5f;
        float randomSeed = Random.value;
        Vector3 initialRotation = pop.transform.rotation.eulerAngles;

        float startX = -200f;  // off-screen left
        float middleX = pos.x; // target position
        float endX = Screen.width + 200f;  // off-screen right

        // Initially place the label off-screen to the left
        pop.style.left = startX;

        while (timeElapsed < totalDuration)
        {
            float t = timeElapsed;
            float currentX;

            // Scale up quickly during entry
            float currentScale = 1f;
            float opacity = 1f;

            // Entry phase: From left to center
            if (t < entryDuration)
            {
                float p = t / entryDuration;
                currentX = Mathf.Lerp(startX, middleX, p); // Move from left to center
                currentScale = Mathf.Lerp(0f, 1f, p);     // Scale up during entry
                opacity = 1f; // Full opacity during entry
            }
            // Hold phase: Stay at center
            else if (t < entryDuration + holdDuration)
            {
                currentX = middleX;
                opacity = 1f; // Full opacity during hold
            }
            // Exit phase: Move from center to off-screen right
            else
            {
                float p = (t - entryDuration - holdDuration) / exitDuration;
                currentX = Mathf.Lerp(middleX, endX, p); // Move from center to right
                opacity = Mathf.Lerp(1f, 0f, (t - entryDuration - holdDuration) / exitDuration);  // Fade out during exit
            }

            // Wiggle effect during all phases
            float wiggleRotation = Mathf.Sin((timeElapsed + randomSeed) * 5f) * maxRotation;
            pop.transform.rotation = Quaternion.Euler(initialRotation.x, initialRotation.y, wiggleRotation);

            // Apply position, scale, and opacity
            pop.style.left = currentX;
            pop.style.scale = new StyleScale(Vector2.one * currentScale);
            pop.style.opacity = opacity;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // After animation, remove the label
        root.Remove(pop);
        activePops.Remove(pop);
    }

}
