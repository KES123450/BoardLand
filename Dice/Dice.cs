using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cardinals.Enums;

namespace Cardinals
{
    public class Dice
    {
        private List<int> _diceNumbers;
        private DiceType _diceType;
        private BuffType _diceBuffType;
        private int _rollResultNumber;
        private int _rollResultIndex;
        public Dice(List<int> numbers, DiceType type)
        {
            _diceNumbers = numbers.ToList();
            _diceType = type;
            switch (type)
            {
                case DiceType.Normal:
                    _diceBuffType = BuffType.Empty;
                    break;

                case DiceType.Fire:
                    _diceBuffType = BuffType.Burn;
                    break;

                case DiceType.Water:
                    _diceBuffType = BuffType.Weak;
                    break;

                case DiceType.Earth:
                    _diceBuffType = BuffType.Powerless;
                    break;
            }
        }

        public List<int> DiceNumbers => _diceNumbers;
        public DiceType DiceType => _diceType;
        public BuffType DiceBuffType => _diceBuffType;
        public int RollResultNumber { get; set; }
        public int RollResultIndex { get; set; }

    }
}
