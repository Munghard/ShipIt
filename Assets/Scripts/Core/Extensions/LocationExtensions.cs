using UnityEngine;

namespace Assets.Scripts.Core.Enums
{
    public static class LocationExtensions
    {
        public static Color GetColor(this Location location)
        {
            return location switch
            {
                Location.WORK => new Color(0.2f, 0.8f, 0.4f),     // green
                Location.HOME => new Color(0.9f, 0.7f, 0.2f),     // yellowish
                Location.HOSPITAL => new Color(0.9f, 0.2f, 0.2f), // red
                _ => Color.white
            };
        }
    }
}
