using UnityEngine;
using Core.Traits;

[CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Trait")]
public abstract class Trait : ScriptableObject
{
    public string TraitName;
    public string Description;
    public TraitType TraitType { get; set; }

    public abstract void Apply(Worker worker);
    public virtual void Remove(Worker worker) { }

}
