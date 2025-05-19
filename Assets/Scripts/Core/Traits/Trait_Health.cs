using UnityEngine;

[CreateAssetMenu(fileName = "Trait_Health", menuName = "Scriptable Objects/Trait_Health")]
public class Trait_Health : Trait
{
    public float healSpeedMod = 1f;
    public override void Apply(Worker worker)
    {
        worker.baseHealSpeed = healSpeedMod;
    }
    public override void Remove(Worker worker)
    {
        base.Remove(worker);
        worker.baseHealSpeed = 1f;
    }
}
