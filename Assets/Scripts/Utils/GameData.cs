using System;
using System.Collections.Generic;

namespace Assets.Scripts.Utils
{
    [Serializable]
    public class GameData
    {
        public float SimulationTime;
        public float Money;
        public float Reputation;

        public List<WorkerData> Workers;
        public List<ProjectData> Projects;

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
        public float Stress;
        public float Efficiency;
        public float Xp;
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
        public float Difficulty;
        public float Pay;
        public List<TaskData> Tasks;
        public ProjectData() { } 
    }

    [Serializable]
    public class TaskData
    {
        public string Name;
        public float Progress;
        public float Difficulty;
        public int Priority;
        public string Description;
        public string Specialty;
        public float TimeToComplete;
        public TaskData() { }
    }
}
