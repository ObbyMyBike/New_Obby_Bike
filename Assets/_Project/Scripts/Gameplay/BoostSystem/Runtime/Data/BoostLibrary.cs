using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Boost/Boost Library", fileName = "New Boost Library")]
public class BoostLibrary : ScriptableObject
{
    public List<BoostZonePreset> Presets = new List<BoostZonePreset>();
}