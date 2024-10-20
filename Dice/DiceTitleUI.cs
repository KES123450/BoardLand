using Cardinals.Enums;
using Cardinals.Tutorial;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;

namespace Cardinals
{
    public class DiceTitleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _isSelect;
        private bool _isSelectable;
        private bool _isDiscard = false;

        private Dice _dice;
        [SerializeField] private Image _diceUIRenderer;
        private DiceTitleDescription _diceDescription;

        public DiceTitleDescription DiceDescription => _diceDescription;

        public void UpdateDiceUIinTitle(Dice dice)
        {
            _isSelectable = true;
            _diceDescription = GetComponent<DiceTitleDescription>();
            _diceUIRenderer.color = new Color(1, 1, 1, 1);
            _dice = dice;
            string path = "Dice/Dice_" + _dice.DiceType.ToString() + "_" + _dice.DiceNumbers[5].ToString();
            Sprite sprite = ResourceLoader.LoadSprite(path);
            _diceUIRenderer.sprite = sprite;
            DiceDescription.Init(dice.DiceNumbers, dice.DiceType, dice);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetCardUIHovered();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetCardUIRestore();
        }

        private void SetCardUIHovered()
        {
            if (!_isDiscard && _isSelectable && !_isSelect)
            {
                _diceUIRenderer.GetComponent<RectTransform>().DOAnchorPosY(15f, 0.1f);
                DiceDescription.SetDescriptionUIHovered(-1, _dice.DiceBuffType);
            }

        }

        private void SetCardUIRestore()
        {
            if (!_isDiscard && _isSelectable)
            {

                _diceUIRenderer.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                DiceDescription.SetDescriptionUIRestored();
            }

        }
    }
}

