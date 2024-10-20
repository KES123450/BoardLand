using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cardinals
{
    [CreateAssetMenu(fileName = "Reward Dice Data", menuName = "Cardinals/Reward Dice Data")]
    public class RewardDiceSO : ScriptableObject
    {
        public string[] numbers;
    }
}