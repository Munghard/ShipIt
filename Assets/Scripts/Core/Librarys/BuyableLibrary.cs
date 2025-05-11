using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Models;
using System.Linq;

namespace Assets.Scripts.Data
{
    internal static class BuyableLibrary
    {
        static string stressRelief = "baseball-bat-ball";
        static string health = "bowl-food";
        static string skill = "star";

        public static Buyable GetBuyable(int id)
        {
            return GetBuyables().FirstOrDefault(b=>b.Id == id);
        }
        public static List<Buyable> GetBuyables()
        {
            return new List<Buyable>
            {
                new Buyable(
                    id: 1,
                    name: "Coffee Break",
                    description: "Boosts worker morale by reducing stress for all workers. Reduces stress by 10 for each worker.",
                    cost: 250,
                    reputationNeeded: 0,
                    singleBuy: false,
                    iconName: stressRelief,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Coffee Break purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.DecreaseStress(10);
                        }
                    }
                ),
                new Buyable(
                    id: 2,
                    name: "Fun Activities",
                    description: "Reduces stress for all workers by organizing fun activities. Reduces stress by 20 for each worker.",
                    cost: 400,
                    reputationNeeded: 300,
                    singleBuy: false,
                    iconName: stressRelief,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Fun Activities purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.DecreaseStress(20);
                        }
                    }
                ),
                new Buyable(
                    id: 3,
                    name: "Pizza Time",
                    description: "Boosts productivity and worker health for a short period by offering pizza. Increases efficiency by 10 and health by 20 for each worker.",
                    cost: 650,
                    reputationNeeded: 0,
                    singleBuy: false,
                    iconName: health,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Pizza Time purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.DecreaseStress(10);
                            worker.IncreaseHealth(20);
                        }
                    }
                ),
                new Buyable(
                    id: 4,
                    name: "Team Lunch",
                    description: "Improves teamwork and boosts task speed by organizing a team lunch. Increases efficiency by 20 and health by 30 for each worker.",
                    cost: 1000,
                    reputationNeeded: 500,
                    singleBuy: false,
                    iconName: health,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Team Lunch purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.DecreaseStress(20);
                            worker.IncreaseHealth(30);
                        }
                    }
                ),
                new Buyable(
                    id: 5,
                    name: "Office Plants",
                    description: "Adds a calming atmosphere, reducing worker stress and improving their skills over time. Reduces stress by 100 once and increases skill by 1 for each worker.",
                    cost: 400,
                    reputationNeeded: 100,
                    singleBuy: true,
                    iconName: skill,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Office Plants purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.IncreaseSkill(1);
                            worker.DecreaseStress(100);
                        }
                    }
                ),
                new Buyable(
                    id: 6,
                    name: "New Coffee Machine",
                    description: "Improves worker focus and boosts their skills by providing a new coffee machine. Increases health by 50 once Increases skill by 1 for each worker.",
                    cost: 1500,
                    reputationNeeded: 1000,
                    singleBuy: true,
                    iconName: skill,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("New Coffee Machine purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.IncreaseSkill(1);
                            worker.IncreaseHealth(50);
                        }
                    }
                ),
                new Buyable(
                    id: 7,
                    name: "Casual Friday",
                    description: "Increases worker satisfaction and reduces stress by allowing casual attire on Fridays. Reduces stress by 20 for each worker.",
                    cost: 500,
                    reputationNeeded: 500,
                    singleBuy: false,
                    iconName: stressRelief,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Casual Friday purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.DecreaseStress(70);
                        }
                    }
                ),
                new Buyable(
                    id: 8,
                    name: "Ergonomic Chairs",
                    description: "Reduces worker fatigue and increases productivity by providing ergonomic chairs. Increases skill by 1 for each worker.",
                    cost: 2000,
                    reputationNeeded: 2000,
                    singleBuy: true,
                    iconName: skill,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Ergonomic Chairs purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.IncreaseSkill(1); // Assuming ReduceFatigue method exists
                        }
                    }
                ),
                new Buyable(
                    id: 9,
                    name: "Healthcare",
                    description: "Increases worker health by providing healthcare.",
                    cost: 500,
                    reputationNeeded: 0,
                    singleBuy: false,
                    iconName: health,
                    onPurchased: (Buyable buyable, Game game) =>
                    {
                        Debug.Log("Healthcare purchased!");
                        game.SpendMoney(buyable.Cost);
                        foreach (var worker in game.Workers)
                        {
                            worker.IncreaseHealth(50); // Assuming ReduceFatigue method exists
                        }
                    }
                )
            };
        }
    }
}
