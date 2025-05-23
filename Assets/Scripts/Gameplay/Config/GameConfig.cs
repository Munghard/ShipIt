using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Scriptable Objects/Config")]
public class GameConfig : ScriptableObject
{
    [Header("Currency")]
    public int StartingMoney;
    public int StartingRep;
    public WorkerInfo StartingWorker;

    [Header("Game")]
    public int RolledWorkersAmount;//3
    public int RolledProjectsAmount;//3
    public int RolledBuyablesAmount;//3

    public int ReputationToHiresLevelRatio; // 500
    public float RepLoseFrequencySeconds; // 5
    public int RepLostPerTick; // 1

    public Vector2 BusinessHours; // time of day range when workers are in

    public AnimationCurve MaximumAmountOfWorkers; // 1

    [Header("Tasks")]
    public float TaskProgressMultiplier; //0.05f

    [Header("Projects")]


    public int ProjectBasePay; // 100 * project.difficulty 
    public Vector2 ProjectBasePayRandomAdd; // 50 - 200
    public float TasksAmountPowerCurve;
    public int GetProjectPay(int difficulty)
    {
        return  (ProjectBasePay * difficulty) + (int)Random.Range(ProjectBasePayRandomAdd.x,ProjectBasePayRandomAdd.y);
    }

    public int ProjectDurationMultiplier; // 600 
    public Vector2 ProjectBaseDurationRandomAdd; // 10-20
    public int GetProjectDuration(int difficulty)
    {
        return (int)Random.Range(ProjectBaseDurationRandomAdd.x, ProjectBaseDurationRandomAdd.y) + ProjectDurationMultiplier * difficulty;
    }
    public int ProjectRepGainBase; // 100

    public Vector2 ProjectRepGainRandomAdd; // 

    public int ReputationGainOnProjectComplete(int difficulty)
    {
        return (int)Random.Range(ProjectRepGainRandomAdd.x, ProjectRepGainRandomAdd.y) + (ProjectRepGainBase * difficulty);
    }

    public int RepToLevelConversion; // 500
    public int ReputationNeededFromLevelFormula(int difficulty)
    {
        return (difficulty - 1) * RepToLevelConversion;
    }

    public int DifficultyFromReputation(int reputation)
    {
        return Mathf.FloorToInt(((float)reputation / RepToLevelConversion) + 1);
    }

    public int StressDecreaseProjectCompletion;
    public int StressDecreaseProjectComplete(int difficulty)
    {
        return StressDecreaseProjectCompletion * difficulty;
    }

    [Header("Workers")]

    public float XpCurvePower;
    public float EvalWorkerXpCurve(int level)
    {
        return Mathf.Floor(100f * Mathf.Pow(level, XpCurvePower));
    }

    public float HireCostPerLevel;
    public int GetHiringCost(int level)
    {
        return Mathf.FloorToInt((level + Random.value) * HireCostPerLevel);
    }

    public float StressDecreaseDifficultyMultiplier;
    internal float GetStressDecreaseOnTaskComplete(int difficulty)
    {
        return difficulty * StressDecreaseDifficultyMultiplier;
    }

    public float XpGainFromTaskMultiplier;
    internal float GetXpGainFromTask(int difficulty, float timeToComplete)
    {
        return difficulty * timeToComplete * XpGainFromTaskMultiplier;
    }


    public float SpecialtySameMultiplier;
    public float SpecialtyDifferentMultiplier;

    
    [Header("Stress")]

    public float StressDecreasePerSecondSkillMultiplier;
    
    
    public float GetStressDecreasePerSecond(int skill)
    {
        return skill * StressDecreasePerSecondSkillMultiplier * StressDecreaseMultiplier;
    }


    public float StressIncreaseMultiplier;
    public float StressDecreaseMultiplier;

    [Header("Health")]
    public float HealthLossMultiplier;
    public float HealthGainMultiplier;

    public float DeathChance;

    public float HomeHealMultiplier;
    public float HospitalHealMultiplier;

    [Header("Happiness")]

    public float HappinessGainMultiplier;
    public float HappinessLossMultiplier;


}
[System.Serializable]
public class WorkerInfo
{

    public string Name;
    public int Level;
    public int Skill;
    public string Specialty;
    public int PortraitIndex;
    public List<string> Traits;
}