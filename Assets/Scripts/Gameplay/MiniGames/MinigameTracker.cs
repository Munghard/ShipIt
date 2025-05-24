using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.MiniGames
{
    public class MinigameTracker
    {
        private Dictionary<int, Dictionary<int, float>> results
            = new Dictionary<int, Dictionary<int, float>>();

        public bool HasPlayed(int workerId, int taskId)
        {
            return results.ContainsKey(workerId) && results[workerId].ContainsKey(taskId);
        }

        public void RecordResult(int workerId, int taskId, float result)
        {
            if (!results.ContainsKey(workerId))
                results[workerId] = new Dictionary<int, float>();

            results[workerId][taskId] = result;
        }

        public float GetResult(int workerId, int taskId)
        {
            return results[workerId][taskId];
        }
    }
}