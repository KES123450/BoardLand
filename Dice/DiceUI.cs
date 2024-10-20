using Cardinals.Enums;
using Cardinals.Game;
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
    public class DiceUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private int _diceIndex;
        private bool _canAction;
        private bool _canMove;
        private bool _isSelect;
        private bool _isSelectable;
        private bool _isDiscard = false;

        private Vector2 _diceUIPos;
        private Dice _dice;
        private DiceManager _diceManager;
        ComponentGetter<Image> _image
                = new ComponentGetter<Image>(TypeOfGetter.This);
        [SerializeField] private Image _diceUIRenderer;
        [SerializeField] private DiceAnimation _diceAnimation;
        private DiceDescription _diceDescription;
        [SerializeField] private Animator _diceAnimator;

        [SerializeField] private bool isJustDisplay; 

        public int Index
        {
            get => _diceIndex;
            set => _diceIndex = value;
        }
        public bool IsSelect
        { 
            get => _isSelect;
            set => _isSelect = value;
        }

        public bool IsDiscard
        {
            get => _isDiscard;
            set => _isDiscard = value;
        }

        public bool IsSelectable
        {
            get => _isSelectable;
            set
            {
                _isSelectable = value;
                if (_isSelectable)
                {
                    //_image.Get(gameObject).color = Color.white;
                }
                else
                {
                    //_image.Get(gameObject).color = Color.gray;
                }
            }
        }

        public bool CanAction
        {
            get => _canAction;
            set
            {
                _canAction = value;
                if (GameManager.I.Stage.Board.IsBoardSquare) {
                    if (GameManager.I.Player.OnTile.Type == TileType.Start || GameManager.I.Player.OnTile.Type == TileType.Blank) {
                        _canAction = false;
                    }
                }
                
            }
        }

        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
            }
        }
        public Dice Dice => _dice;
        public DiceAnimation DiceAnimation => _diceAnimation;
        public DiceDescription DiceDescription => _diceDescription;
        public void Init(Dice dice, int index, DiceManager diceManager)
        {
            _dice = dice;
            Index = index;
            _diceManager = diceManager;
            _isSelectable = true;
            _diceUIPos = (transform as RectTransform).anchoredPosition;
            _diceDescription = GetComponent<DiceDescription>();
            
            string path = "Dice/Dice_" + _dice.DiceType.ToString() + "_" + "1";
            _diceUIRenderer.sprite = ResourceLoader.LoadSprite(path);

        }

        public void SortingDiceUI(int index, Vector2 diceUIPos)
        {
            GameManager.I.Sound.DiceMove();
            Index = index;
            (transform as RectTransform).SetUILeftBottom();
            (transform as RectTransform).DOAnchorPos(diceUIPos, 0.2f).SetEase(Ease.OutCubic);
            _diceUIPos = diceUIPos;
        }

        public void UpdateDiceUI(Dice dice)
        {
            _diceUIRenderer.color = new Color(1, 1, 1, 1);
            _dice = dice;
            string path = "Dice/Dice_" + _dice.DiceType.ToString() + "_" + _dice.RollResultNumber.ToString();
            Sprite sprite = ResourceLoader.LoadSprite(path);
            _diceUIRenderer.sprite = sprite;
            //_image.Get(gameObject).sprite = ResourceLoader.LoadSprite(path);
        }

        public IEnumerator RollDiceUI(int number, Action onCompleted=null)
        {
            GameManager.I.Sound.CardReroll();
            _diceUIRenderer.color = new Color(1, 1, 1, 1);
            _diceAnimator.runtimeAnimatorController = ResourceLoader.LoadAnimatorController(_dice.DiceType.ToString() + "DiceAnimator");
            _diceAnimator.enabled = true;
            _diceAnimator.Play("Roll");
            yield return new WaitForSeconds(_diceAnimator.GetCurrentAnimatorStateInfo(0).length-0.3f);
            _diceAnimator.enabled = false;

            string path = "Dice/Dice_" + _dice.DiceType.ToString() + "_" + number.ToString();
            _diceUIRenderer.sprite = ResourceLoader.LoadSprite(path);
            GameManager.I.Sound.DiceDetermine();
            _diceUIRenderer.transform.DOScale(1f, 0.3f).From(1.3f).SetEase(Ease.InBack);
            onCompleted?.Invoke();
        }

        public void EnableCardUI()
        {
            IsDiscard = false;
            GetComponent<RectTransform>().anchoredPosition = _diceUIPos;
            _diceUIRenderer.color = new Color(1, 1, 1, 0);
            transform.localScale = new Vector3(1, 1, 1);
            gameObject.SetActive(true);
        }
   
        public void DismissDiceUI()
        {
            IsSelect = false;
            transform.localScale = new Vector3(1, 1, 1);
            GetComponent<RectTransform>().anchoredPosition = _diceUIPos;
            DiceDescription.SetDescriptionUIRestored();

        }

        private IEnumerator Reroll()
        {
            SetCardUIRestore();
            //GameManager.I.Sound.CardReroll();
            IsSelectable = false;
            if (GameManager.I.Player.PlayerInfo.Gold <= 0)
            {
                GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_REROLL));
                IsSelectable = true;
                yield break;
            }
            GameManager.I.Player.PlayerInfo.UseGold(1);
            bool isComplete = false;
            _diceManager.Roll(Index, () => { isComplete = true; });
            yield return new WaitUntil(() => isComplete);
            yield return new WaitForSeconds(0.3f);
            IsSelectable = true;

        }

        public void InitRenderer()
        {
            _diceUIRenderer.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsSelect) return;
            if (!CheckCanMove()) return;
            if (!_isSelectable) {
                if (GameManager.I.Stage.CurEvent is TutorialEvent && GameManager.I.IsWaitingForNext) {
                    GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_TUTORIAL));
                }
                return;
            }
            if (GameManager.I.Stage.Board.IsTileDoingLevelUp) return;

            if (eventData.button== PointerEventData.InputButton.Left)
            {
                if (_diceManager.State != CardState.Idle) return;


                transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.1f);
                _diceManager.SelectCardIndex = Index;
                _isSelect = true;
                _diceManager.State = CardState.Select;

                GameManager.I.Sound.CardClick();
                DiceDescription.SetDescriptionUIRestored();
                StartCoroutine(_diceManager.Dragging());
            }

            if(eventData.button == PointerEventData.InputButton.Right)
            {
                if (GameManager.I.Stage.CurEvent is TutorialEvent)
                {
                    GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_TUTORIAL1));
                    return;
                }
                StartCoroutine(Reroll());
            }
            
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CheckCanMove()) return;
            SetCardUIHovered();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!CheckCanMove()) return;
            SetCardUIRestore();
        }

        public void StartDraggingState()
        {
            _image.Get(gameObject).raycastTarget = false;
        }

        public void ClickDismiss()
        {
            _image.Get(gameObject).raycastTarget = true;
        }

        private void MoveCardUI()
        {
            if (_isSelect)
            {
                transform.position = Input.mousePosition;
            }
        }

        private void SetCardUIHovered()
        {
            if (!_isDiscard && _isSelectable&&!_isSelect)
            {
                GameManager.I.Sound.DiceHover();
                _diceUIRenderer.GetComponent<RectTransform>().DOAnchorPosY(15f, 0.1f);
                DiceDescription.SetDescriptionUIHovered(Dice.RollResultIndex,_dice.DiceBuffType);
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

        private bool CheckCanMove()
        {
            bool result = true;
            result &= _diceManager != null;
            
            return result;
        }
        private void Update()
        {
            if (CheckCanMove())
            {
                MoveCardUI();
            }
        }
    }
}

