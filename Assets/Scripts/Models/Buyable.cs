using System;
using UnityEngine;

namespace Assets.Scripts.Models
{
    internal class Buyable
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Cost { get; private set; }
        public int ReputationNeeded { get; private set; }
        public Sprite Icon { get; private set; }
        public Action<Buyable,Game> OnPurchased { get; set; }

        public Buyable(int id, string name, string description, int cost, int reputationNeeded, Sprite icon, Action<Buyable, Game> onPurchased = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Cost = cost;
            ReputationNeeded = reputationNeeded;
            Icon = icon;
            OnPurchased = onPurchased;
        }

        public bool CanPurchase(int money, int reputation)
        {
            return money >= Cost && reputation >= ReputationNeeded;
        }

        public void Purchase(Game game)
        {
            OnPurchased?.Invoke(this, game);
        }
    }
}
