using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cardinals.Enums;
using DG.Tweening;
using TMPro;
using Util;

namespace Cardinals
{
    public class CardUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private int _cardIndex;
        private bool _canAction;
        private bool _canMove;
        private bool _isDiscard=false; // discard�� ī���� true
        private Vector2 _CardUIPos;
        [SerializeField] private GameObject _actionMark;
        [SerializeField] private GameObject _moveMark;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private GameObject _volatileText;
        [SerializeField] private bool _isSelect;
        
        ComponentGetter<Image> _image 
            = new ComponentGetter<Image>(TypeOfGetter.This);

        private bool _isSelectable;
        private Card _card;
        private CardManager _cardManager;
        private CardAnimation _cardAnimation;

        public CardAnimation CardAnim => _cardAnimation;
        public bool IsSelect
        {
            set
            {
                _isSelect = value;
            }
        }

        public bool IsDiscard
        {
            set => _isDiscard = value;
        }
        public bool IsSelectable
        {
            get => _isSelectable;
            set {
                _isSelectable = value;
                if (_isSelectable)
                {
                    _image.Get(gameObject).color = Color.white;
                }
                else
                {
                    _image.Get(gameObject).color = Color.gray;
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
                    if (GameManager.I.Player.OnTile.Type == TileType.Start || GameManager.I.Player.OnTile.Type == TileType.Blank)
                    {
                        _canAction = false;
                    }
                }

                SetActionMark(_canAction);
            }
        }
      
        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
                _moveMark.SetActive(_canMove);
            }
        }
        public Card Card => _card;
        public int CardIndex
        {
            get => _cardIndex;
            set => _cardIndex = value;
            
        }

        public void Init(Card card, int index, CardManager cardManager)
        {
            _card = card;
            _cardIndex = index;
            _cardManager = cardManager;
            _isSelectable = true;
            Image image = GetComponent<Image>();
            _numberText.text = card.CardNumber.ToString();
            _volatileText.SetActive(card.IsVolatile);
            _CardUIPos = (transform as RectTransform).anchoredPosition;
            _cardAnimation = GetComponent<CardAnimation>();

            switch (card.CardNumber)
            {
                case 1:
                    image.sprite = ResourceLoader.LoadSprite(Constants.FilePath.Resources.Sprites_Card_Basic1);
                    break;
                case 2:
                    image.sprite = ResourceLoader.LoadSprite(Constants.FilePath.Resources.Sprites_Card_Basic2);
                    break;
                case 3:
                    image.sprite = ResourceLoader.LoadSprite(Constants.FilePath.Resources.Sprites_Card_Basic3);
                    break;
                case 4:
                    image.sprite = ResourceLoader.LoadSprite(Constants.FilePath.Resources.Sprites_Card_Basic4);
                    break;
                case 5:
                    image.sprite = ResourceLoader.LoadSprite(Constants.FilePath.Resources.Sprites_Card_Basic5);
                    break;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_cardManager.State != CardState.Idle)
                return;
            if (!_isSelectable)
                return;

            transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.1f);
            _cardManager.SelectCardIndex = _cardIndex;
            _isSelect = true;
            _cardManager.State = CardState.Select;
            
            GameManager.I.Sound.CardClick();

            StartCoroutine(_cardManager.Dragging());
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            SetCardUIHovered();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetCardUIRestore();
        }

        public void StartDraggingState() {
            _image.Get(gameObject).raycastTarget = false;
        }

        public void ClickDismiss() {
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
            if (!_isDiscard&&_isSelectable)
            {
                (transform as RectTransform).DOAnchorPosY(_CardUIPos.y + 30, 0.1f);
            }
            
        }

        private void SetCardUIRestore()
        {
            if (!_isDiscard && _isSelectable)
            {
                (transform as RectTransform).DOAnchorPosY(_CardUIPos.y, 0.1f);
            }
                
        }

        private void SetActionMark(bool isActive)
        {
            //_actionMark.SetActive(isActive);

            // TODO: 타일 마법에 따라 마크 변경 필요
            if (GameManager.I.Player.OnTile.Type == TileType.Attack)
            {
                _actionMark.GetComponent<Image>().sprite =
                    ResourceLoader.LoadSprite(Constants.FilePath.Resources.Sprites_Card_Attack_Mark);
            }

            if (GameManager.I.Player.OnTile.Type == TileType.Defence)
            {
                _actionMark.GetComponent<Image>().sprite =
                    ResourceLoader.LoadSprite(Constants.FilePath.Resources.Sprites_Card_Defense_Mark);
            }

            float targetPosY = isActive ? -17f : -44f;
            _actionMark.GetComponent<RectTransform>().DOAnchorPosY(targetPosY, 0.3f).SetEase(Ease.InOutBack);
        }

        private void Update()
        {
            MoveCardUI();
        }

    }
}

