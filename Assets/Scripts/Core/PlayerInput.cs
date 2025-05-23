using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Assets.Scripts.Core
{
    internal class PlayerInput
    {
        Game game;

        public PlayerInput(Game _game)
        {
            game = _game;
        }

        public void UpdateInput()
        {

            if (Input.GetKeyDown(KeyCode.E))
            {
                game.TogglePaused();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                game.HalveTimeScale();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                game.SetTimeScale(32);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                game.DoubleTimeScale();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                game.SetTimeScale(1);
            }
        }
    }
}
