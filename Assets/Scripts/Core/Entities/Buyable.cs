using System;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class Buyable
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Cost { get; private set; }
        public int ReputationNeeded { get; private set; }
        public bool SingleBuy { get; private set; }
        public string IconName { get; private set; }
        public Action<Buyable,Game> OnPurchased { get; set; }

        public Buyable(int id, string name, string description, int cost, int reputationNeeded, bool singleBuy, string iconName, Action<Buyable, Game> onPurchased = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Cost = cost;
            ReputationNeeded = reputationNeeded;
            SingleBuy = singleBuy;
            IconName = iconName;
            OnPurchased = onPurchased;
        }

        public bool CanPurchase(int money, int reputation)
        {
            return HasMoney(money) && HasRep(reputation);
        }
        public bool HasMoney(int money)
        {
            return money >= Cost;
        }
        public bool HasRep(int reputation)
        {
            return reputation >= ReputationNeeded;
        }

        public void Purchase(Game game)
        {
            OnPurchased?.Invoke(this, game);
        }
    }
}
