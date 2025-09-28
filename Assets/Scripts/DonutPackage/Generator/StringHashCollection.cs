
using UnityEngine;
using System.Collections.Generic;

namespace DonutPackage.Generator
{
    [CreateAssetMenu(fileName = "StringHashCollection", menuName = "DonutPackage/Generate/String Hash Collection")]
    public class StringHashCollection : ScriptableObject
    {
        public List<string> TimerTags = new List<string>
        {
            "Cooldown",
            "Wait",
            "Duration",
            "Stagger",
            "Invincibility",
            "ComboReset",
            "AttackCooldown"
        };
        
        public List<string> AnimationHashes = new List<string>();
    }
}
