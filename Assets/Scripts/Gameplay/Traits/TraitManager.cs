using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TraitManager
{
    List<Trait> _traits;
    public TraitManager() {
        LoadTraits();
    }

    private void LoadTraits()
    {
        _traits = Resources.LoadAll<Trait>("Traits").ToList();
        Debug.Log($"TraitManager loaded {_traits.Count} traits");
    }

    public Trait GetRandomTrait()
    {
        if(_traits == null || _traits.Count == 0)
            return null;
        
        return _traits[UnityEngine.Random.Range(0,_traits.Count)];
    }

    public Trait GetTrait(string traitName)
    {
        return _traits.FirstOrDefault(t => t.TraitName == traitName);
    }
}