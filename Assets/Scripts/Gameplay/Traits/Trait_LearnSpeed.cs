using UnityEngine;

[CreateAssetMenu(fileName = "Trait_LearnSpeed", menuName = "Scriptable Objects/Trait_LearnSpeed")]
public class Trait_LearnSpeed : Trait
{
    public float learnspeedMod = 1f;
    public override void Apply(Worker worker)
    {
        worker.baseLearnSpeed = learnspeedMod;
    }
    public override void Remove(Worker worker)
    {
        base.Remove(worker);
        worker.baseLearnSpeed = 1f;
    }
}
