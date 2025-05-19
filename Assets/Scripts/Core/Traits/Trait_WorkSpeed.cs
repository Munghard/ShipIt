using UnityEngine;

[CreateAssetMenu(fileName = "Trait_WorkSpeed", menuName = "Scriptable Objects/Trait_WorkSpeed")]
public class Trait_WorkSpeed : Trait
{
    public float workspeedMod = 1f;
    public override void Apply(Worker worker)
    {
        worker.baseWorkSpeed = workspeedMod;
    }
    public override void Remove(Worker worker)
    {
        base.Remove(worker);
        worker.baseWorkSpeed = 1f;
    }
}
