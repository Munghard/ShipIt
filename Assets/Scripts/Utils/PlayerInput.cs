using UnityEngine;

namespace Assets.Scripts.Utils
{
    internal class PlayerInput
    {

        Game game;
        public PlayerInput(Game _game)
        {
            game = _game;
        }
        public void UpdateInput() {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                game.TogglePaused();
            }
        }
    }
}
