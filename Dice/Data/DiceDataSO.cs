using Cardinals.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Cardinals{
    public class DiceDataSO : ScriptableObject
    {
        public string title;
        public string information;
        public DiceType diceType;
        public Color elementColor;

        public static DiceDataSO Data(DiceType type)
        {
            return ResourceLoader.LoadSO<DiceDataSO>(
                Constants.FilePath.Resources.SO_DiceData + type
            ); ;
        }
    }
}
