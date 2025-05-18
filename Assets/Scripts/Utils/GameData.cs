using Assets.Scripts.Core.Enums;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Utils
{
    [Serializable]
    public class GameData
    {
        public float SimulationTime;
        public int Money;
        public int Reputation;

        public List<WorkerData> Workers;
        public List<WorkerData> AvailableWorkers;
        public List<ProjectData> Projects;
        public List<ProjectData> AvailableProjects;
        public List<BuyableData> Shop;
        public List<BuyableData> BoughtBuyables;

        public GameData() { }
    }

    [Serializable]
    public class WorkerData
    {
        public long Id;
        public string Name;
        public int PortraitIndex;
        public string Specialty;
        public int Level;
        public int Skill;
        public float Health;
        public float Happiness;
        public float Stress;
        public float Xp;
        public Location Location;
        public WorkerData() { }

    }

    [Serializable]
    public class ProjectData
    {
        public int Id;
        public string Name;
        public string Description;
        public float Duration;
        public float StartDuration;
        public int Difficulty;
        public int Pay;
        public List<TaskData> Tasks;
        public ProjectData() { } 
    }

    [Serializable]
    public class TaskData
    {
        public string Name;
        public float Progress;
        public int Difficulty;
        public int Priority;
        public string Description;
        public string Specialty;
        public float TimeToComplete;
        public TaskData() { }
    }
    [Serializable]
    public class BuyableData
    {
        public int Id;
    }
}
