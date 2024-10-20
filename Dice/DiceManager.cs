using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cardinals.Enums;
using Sirenix.OdinInspector;
using Cardinals.Board;
using Cardinals.Tutorial;
using Cardinals.Buff;
using UnityEngine.UI;
using Util;
using System.Linq;
using Steamworks.Data;

namespace Cardinals
{
    public class DiceManager : MonoBehaviour
    {
        public List<Dice> Dices => _dices;
        public List<DiceUI> DiceUis => _dicesUI;

        private int _prevDiceNumber = -1;
        private int _selectDiceIndex;
        private bool _canActionUse;
        private bool _lastDiceUsedForAction;
        private bool _isMouseOnDiceDeck;
        private int _continuousUseCount;
        private int _continuousConfusionCount;
        private int _DiceUsedForMoveCountOnThisTurn;
        private int _diceUsedCountOnThisTurn;
        #region Tutorial
        public bool IsTutorial => _isTutorial;
        private bool _isTutorial;
        #endregion

        private CardState _state;
        private MouseState _mouseState = MouseState.Cancel;

        [SerializeField] private bool _newDiceUseMod;
        [ShowInInspector] private List<Dice> _dices;
        private List<DiceUI> _dicesUI;

        //public IEnumerable<Card> HandCards => _handCards;

        [SerializeField] private Transform _diceDeckUIParent;

        public bool IsElectricShock { get; set; }

        private int _selectedNumber;

        public int SelectCardIndex
        {
            get => _selectDiceIndex;
            set => _selectDiceIndex = value;
        }

        public CardState State
        {
            get => _state;
            set
            {
                _state = value;
            }
        }
        public MouseState MouseState
        {
            set
            {
                _mouseState = value;
            }
        }

        [Button]
        public void Init(List<(int[], DiceType)> initialDiceList = null)
        {
            _dices = new();
            _dicesUI = new();
            _newDiceUseMod = true;
            
            if (initialDiceList != null) {
                foreach ((int[], DiceType) dice in initialDiceList) {
                    AddDice(dice.Item1.ToList(), dice.Item2);
                }
            } else {
                AddDice(new List<int>() { 1,1,2,2,3,3 }, DiceType.Normal);
                AddDice(new List<int>() { 1,1,2,2,3,3 }, DiceType.Normal);
                AddDice(new List<int>() { 1,1,2,2,3,3 }, DiceType.Normal);
                AddDice(new List<int>() { 3,3,4,4,5,5 }, DiceType.Normal);
                AddDice(new List<int>() { 3,3,4,4,5,5 }, DiceType.Normal);
            }
            
            foreach(DiceUI d in _dicesUI)
            {
                d.gameObject.SetActive(false);
            }
        }

        public void SetDiceDeckUIParent(Transform parent)
        {
            _diceDeckUIParent = parent;
        }

        [Button]
        public IEnumerator OnTurn()
        {
            foreach(DiceUI d in _dicesUI)
            {
                d.EnableCardUI();
                d.InitRenderer();
            }
            SetDiceSelectable(true);

            _canActionUse = false;
            if (!_lastDiceUsedForAction && _newDiceUseMod)
            {
                _canActionUse = true;
            }
            _DiceUsedForMoveCountOnThisTurn = 0;
            _continuousConfusionCount = 0;
            _diceUsedCountOnThisTurn = 0;
            _continuousUseCount = 0;
            _state = CardState.Idle;
            UpdateDiceState(-1, true);

            yield return RollAllDice();
            
            yield break;
        }

        public IEnumerator OnTutorialTurn(int[] diceNumbers) {
            foreach(DiceUI d in _dicesUI)
            {
                d.EnableCardUI();
                d.InitRenderer();
            }
            SetDiceSelectable(true);

            _canActionUse = false;
            if (!_lastDiceUsedForAction && _newDiceUseMod)
            {
                _canActionUse = true;
            }

            _diceUsedCountOnThisTurn = 0;
            _continuousUseCount = 0;
            _state = CardState.Idle;
            UpdateDiceState(-1, true);

            yield return TutorialRoll(diceNumbers);
            SetDiceSelectable(true);
            
            yield break;
        }

        [Button]
        public IEnumerator EndTurn()
        {
            SetDiceSelectable(false);
            //StartCoroutine(DiscardAll(0, CardAnimationType.TurnEnd));
            bool[] discarded = new bool[_dicesUI.Count];
            for (int i = 0; i < _dicesUI.Count; i++)
            {
                discarded[i] = false;
            }
            
            for(int i = 0; i < _dicesUI.Count; i++)
            {
                if (_dicesUI[i].IsDiscard) {
                    discarded[i] = true;
                    continue;
                }
                
                int target = i;
                StartCoroutine(Discard(i, DiceAnimationType.UseMove, () => { }, () => {
                    discarded[target] = true;
                }));
            }
            yield return new WaitUntil(() => discarded.All(x => x == true));

            yield return new WaitForSeconds(0.2f);
            yield break;
        }


        public void OnBattle(bool isTutorial = false)
        {
            _lastDiceUsedForAction = false;
            _prevDiceNumber = -1;

            _isTutorial = isTutorial;
        }

        public IEnumerator EndBattle()
        {
            yield return EndTurn();
        }

        [Button]
        public IEnumerator TutorialRoll(int[] diceNumbers)
        {
            bool[] rollCompleted = new bool[_dicesUI.Count];
            for (int i = 0; i < _dicesUI.Count; i++)
            {
                rollCompleted[i] = false;
            }

            for(int i = 0; i < diceNumbers.Length; i++)
            {
                if (diceNumbers[i] == -1)
                {
                    StartCoroutine(Discard(i, DiceAnimationType.Empty, () => { }));
                    rollCompleted[i] = true;
                }

                else
                {
                    int resultIndex = _dices[i].DiceNumbers.FindIndex(n => n==diceNumbers[i]);
                    int rollResult = _dices[i].DiceNumbers[resultIndex];
                    _dices[i].RollResultIndex = resultIndex;
                    _dices[i].RollResultNumber = rollResult;

                    int target = i;
                    StartCoroutine(_dicesUI[i].RollDiceUI(rollResult, () => {
                        rollCompleted[target] = true;
                    }));
                    _dicesUI[i].DiceDescription.SetDescriptionUIRestored();
                }
            }

            yield return new WaitUntil(() => rollCompleted.All(x => x == true));
        }

        public void SetDiceSelectable(bool isSelectable)
        {
            if (isSelectable == true && _isTutorial)
            {
                var cardSequenceCheck = (GameManager.I.Stage.CurEvent as TutorialEvent).CheckIfHasDiceSequence();
                if (cardSequenceCheck.hasSequence)
                {
                    SetCardUnselectableExcept(cardSequenceCheck.targetSequence.CardNumber);
                    return;
                }
            }

            foreach (DiceUI d in _dicesUI)
            {
                d.IsSelectable = isSelectable;
            }
        }
        public void SetDiceMouseState(bool isOnDiceDeck)
        {
            _isMouseOnDiceDeck = isOnDiceDeck;
        }

        [Button]
        public void AddDice(List<int> numbers, DiceType type)
        {
            Dice dice = new Dice(numbers, type);
            _dices.Add(dice);

            GameObject diceUI = Instantiate(ResourceLoader.LoadPrefab(Constants.FilePath.Resources.Prefabs_UI_Dice),_diceDeckUIParent);
            Vector3 UIPos = Vector3.zero;
            if (_dicesUI.Count == 0)
            {
                UIPos = new Vector3(100f, 100f, 0);
            }
            else
            {
                UIPos = _dicesUI[_dicesUI.Count - 1].GetComponent<RectTransform>().anchoredPosition;
                UIPos.x += 140f;
            }

            diceUI.GetComponent<RectTransform>().anchoredPosition = UIPos;
            diceUI.GetComponent<DiceUI>().Init(dice, _dicesUI.Count,this);
            diceUI.GetComponent<DiceUI>().DiceDescription.Init(numbers, type,dice);
            _dicesUI.Add(diceUI.GetComponent<DiceUI>());
        }

       
        public void ChangeDice(int index, Dice dice)
        {
            bool isDiscard = _dicesUI[index].IsDiscard;
            int resultIndex = UnityEngine.Random.Range(0, dice.DiceNumbers.Count);
            int rollResult = dice.DiceNumbers[resultIndex];
            dice.RollResultIndex = resultIndex;
            dice.RollResultNumber = rollResult;

            _dices.RemoveAt(index);
            _dices.Insert(index,dice);
            _dicesUI[index].UpdateDiceUI(dice);
            _dicesUI[index].DiceDescription.UpdateDiceDescription(dice);

            if (isDiscard)
            {
                _dicesUI[index].IsDiscard = true;
                _dicesUI[index].IsSelect = false;
                _dicesUI[index].gameObject.SetActive(false);
            }
        }

        [Button]
        public void Roll(int index,Action onCompleted=null)
        {
            int resultIndex = UnityEngine.Random.Range(0, _dices[index].DiceNumbers.Count);
            int rollResult = _dices[index].DiceNumbers[resultIndex];
            _dices[index].RollResultIndex = resultIndex;
            _dices[index].RollResultNumber = rollResult;
            StartCoroutine(_dicesUI[index].RollDiceUI(rollResult,onCompleted));
            _dicesUI[index].DiceDescription.SetDescriptionUIRestored();
        }

        [Button]
        public IEnumerator SortDices()
        {
            _dices.Sort((p1, p2) => p1.RollResultNumber.CompareTo(p2.RollResultNumber));
            for(int i = 0; i < _dices.Count; i++)
            {
                for(int j = 0; j < _dicesUI.Count; j++)
                {
                    if (_dices[i].Equals(_dicesUI[j].Dice))
                    {
                        Vector2 diceUIPos = new Vector2(100 + 140f * i, 100f);
                        _dicesUI[j].SortingDiceUI(i, diceUIPos);
                        break;
                    }
                }
                yield return new WaitForSeconds(0.15f);
            }
            _dicesUI.Sort((p1, p2) => p1.Index.CompareTo(p2.Index));

            for (int i = 0; i < _dicesUI.Count; i++)
            {
                _dicesUI[i].UpdateDiceUI(_dicesUI[i].Dice);
            }
        }

        [Button]
        public IEnumerator RollAllDice()
        {
            SetDiceSelectable(false);
            // 여기서 DiceUI anchoredPosition 변경
            float startPos = -((((float)(_dicesUI.Count - 1)) / 2f) * 130 + ((int)_dicesUI.Count / 2) * 10f);

            for (int i=0;i< _dicesUI.Count; i++)
            {
                (_dicesUI[i].transform as RectTransform).SetUICenter();

                //Vector2 center = _dicesUI[i].GetComponent<RectTransform>().anchoredPosition;
                Vector2 center = new Vector2(startPos + i * 140f, 0);
                _dicesUI[i].GetComponent<RectTransform>().anchoredPosition = center;
            }

            for(int i = 0; i < _dices.Count; i++)
            {
                Roll(i);
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.8f);
            yield return SortDices();
            SetDiceSelectable(true);
        }

        private IEnumerator Discard(int index, DiceAnimationType animationType, Action changeDiscardState, Action onDiscarded=null)
        {
            _dicesUI[index].IsDiscard = true;
            _dicesUI[index].IsSelect = false;

            GameManager.I.Sound.CardUse();

            yield return _dicesUI[index].DiceAnimation.Play(animationType);
            _dicesUI[index].gameObject.SetActive(false);

            changeDiscardState?.Invoke();
            onDiscarded?.Invoke();
        }


        public IEnumerator Dragging()
        {
            foreach (DiceUI c in _dicesUI)
            {
                if (c == null)
                    continue;
                c.StartDraggingState();
            }

            bool initFirstSelect = false;
            while (_state == CardState.Select)
            {
                if (!initFirstSelect)
                {
                    GameManager.I.Player.MotionThinking();
                    initFirstSelect = true;
                }
                
                if (Input.GetMouseButtonUp(0))
                {
                    GameManager.I.Player.MotionIdle();
                    UpdateMarkedNextTile();
                    IBoardInputHandler boardInputHandler = GameManager.I.Stage.Board.BoardInputHandler;
                    if (boardInputHandler.IsMouseHoverUI)
                    {
                        if (boardInputHandler.HoveredMouseDetectorType == UIMouseDetectorType.CardPile)
                        {
                            _mouseState = MouseState.Cancel;
                        }
                        else
                        {
                            _mouseState = MouseState.CardEvent;
                        }
                    }
                    else
                    {
                        if (boardInputHandler.IsMouseHover)
                        {
                            if (boardInputHandler.HoveredIdx >= 0)
                            {
                                _mouseState = MouseState.Action;
                            }
                            else
                            {
                                _mouseState = MouseState.Move;
                            }
                        }
                        else
                        {
                            _mouseState = MouseState.Cancel;
                        }
                    }

                    int useNumber = _dices[_selectDiceIndex].RollResultNumber;
                    _selectedNumber = useNumber;
                    if (_isTutorial)
                    {
                        var cardValidCheck = (GameManager.I.Stage.CurEvent as TutorialEvent).CheckIfHasDiceSequence();
                        if (cardValidCheck.hasSequence && cardValidCheck.targetSequence.CardNumber != useNumber)
                        {
                            GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_TUTORIAL));
                            goto DismissCards;
                        }

                        if (cardValidCheck.hasSequence && cardValidCheck.targetSequence.HowToUse != _mouseState)
                        {
                            GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_TUTORIAL));
                            goto DismissCards;
                        }
                    }

                    switch (_mouseState)
                    {
                        case MouseState.Action:
                            var target = GameManager.I.Stage.Enemies[boardInputHandler.HoveredIdx];

                            if (!CheckUseDiceOnAction())
                            {
                                break;
                            }

                            if(IsWind1BlessUsable())
                            {
                                GameManager.I.Player.PlayerInfo.BlessEventDict[BlessType.BlessWind1]?.Invoke();
                                StartCoroutine(DiceUseAction(useNumber, _dices[_selectDiceIndex].DiceType, target));
                                yield break;
                            }

                            if (IsNumberSequence())
                            {
                                break;
                            }
                            StartCoroutine(DiceUseAction(useNumber, _dices[_selectDiceIndex].DiceType,target));
                            
                            if (_isTutorial)
                            {
                                CheckTutorialStateForCard(useNumber, MouseState.Action);
                            }
                            yield break;

                        case MouseState.Move:

                            if (!CheckUseDiceOnMove())
                            {
                                break;
                            }

                            StartCoroutine(DiceUseMove(useNumber));
                            
                            if (_isTutorial)
                            {
                                CheckTutorialStateForCard(useNumber, MouseState.Move);
                            }
                            yield break;

                        case MouseState.CardEvent:
                            yield return Discard(_selectDiceIndex, DiceAnimationType.UseMove, 
                                () =>
                                {
                                    GameManager.I.UI.UIDiceEvent.SelectedCard(useNumber);
                                    _state = CardState.Idle;
                                    UpdateDiceState(useNumber, false);
                                    DismissAllCards();
                            
                                    _diceUsedCountOnThisTurn++;
                                    if (_isTutorial)
                                    {
                                        CheckTutorialStateForCard(useNumber, MouseState.CardEvent);
                                    }
                                });
                            
                            yield break;
                    }

                DismissCards:
                    _dicesUI[_selectDiceIndex].DismissDiceUI();
                    DismissAllCards();

                    _state = CardState.Idle;
                    yield break;
                }
                yield return null;
            }
        }

        private void DismissAllCards()
        {
            foreach (DiceUI d in _dicesUI)
            {
                if (d == null)
                    continue;
                d.ClickDismiss();
            }
        }

        private void SetCardUnselectableExcept(int diceNumber)
        {
            foreach (DiceUI d in _dicesUI)
            {
                if (d.Dice.RollResultNumber == diceNumber)
                {
                    d.IsSelectable = true;
                }
                else
                {
                    d.IsSelectable = false;
                }
            }
        }

        private void CheckTutorialStateForCard(int useNumber, MouseState mouseState)
        {
            (GameManager.I.Stage.CurEvent as TutorialEvent).CheckCardQuest(useNumber, mouseState);
            var cardSequenceCheck = (GameManager.I.Stage.CurEvent as TutorialEvent).CheckIfHasDiceSequence();

            if (cardSequenceCheck.hasSequence)
            {
                SetCardUnselectableExcept(cardSequenceCheck.targetSequence.CardNumber);
            }
        }

        public bool CheckUseDiceOnMove(bool printMsg = true)
        {
            if (GameManager.I.Player.CheckBuffExist(BuffType.Slow) && _DiceUsedForMoveCountOnThisTurn >= 2)
            {
                if (printMsg)
                {
                    GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_SLOW));
                }
                return false;
            }
            return true;
        }
        public void UpdateMarkedNextTile(PlayerActionType type = PlayerActionType.None)
        {
            if (GameManager.I.Player.IsPlayerMove)
                return;
            if (type == PlayerActionType.Move)
            {
                int onTileIndex = GameManager.I.Stage.Board.GetTileIndex(GameManager.I.Player.OnTile);
                GameManager.I.Stage.Board[onTileIndex + _dices[SelectCardIndex].RollResultNumber].MarkAsTarget();
            }
            else
            {
                int onTileIndex = GameManager.I.Stage.Board.GetTileIndex(GameManager.I.Player.OnTile);
                GameManager.I.Stage.Board[onTileIndex + _dices[SelectCardIndex].RollResultNumber].UnMark();
            }
        }

        public IEnumerator DiceUseMove(int num)
        {
            _diceUsedCountOnThisTurn++;
            _DiceUsedForMoveCountOnThisTurn++;
            SetDiceSelectable(false);
            StartCoroutine(Discard(_selectDiceIndex, DiceAnimationType.UseMove, () => { }));

            //디버프 혼란
            if (GameManager.I.Player.CheckBuffExist(BuffType.Confusion)&&UnityEngine.Random.Range(0,2)==1)
            {
                _continuousConfusionCount++;
                if (_continuousConfusionCount >= 3)
                {
                    GameManager.I.SteamHandler.TriggerAchievement("ConFused_Confused");
                }
                GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_CONFUSE));
                yield return GameManager.I.Player.PrevMoveTo(num, 0.4f);
            }
            else
            {
                _continuousConfusionCount = 0;
                yield return GameManager.I.Player.MoveTo(num, 0.4f);
            }

            GameManager.I.DiceRollingCount++;

            _state = CardState.Idle;
            _prevDiceNumber = -1;
            _continuousUseCount = 0;
            _canActionUse = true;
            _lastDiceUsedForAction = false;
            DismissAllCards();
            SetDiceSelectable(true);
        }

        public bool PotionUseMove(int num)
        {
            if (!CheckUseDiceOnMove()) return false;
            _DiceUsedForMoveCountOnThisTurn++;
            SetDiceSelectable(false);
            StartCoroutine(GameManager.I.Player.MoveTo(num, 0.4f, ()=> { SetDiceSelectable(true); }));
            _prevDiceNumber = -1;
            _continuousUseCount = 0;
            _canActionUse = true;
            _lastDiceUsedForAction = false;
            DismissAllCards();
            return true;
        }

        public bool PotionUsePrevMove(int num)
        {
            if (!CheckUseDiceOnMove()) return false;
            _DiceUsedForMoveCountOnThisTurn++;
            SetDiceSelectable(false);
            StartCoroutine(GameManager.I.Player.PrevMoveTo(num, 0.4f, () => { SetDiceSelectable(true); }));
            _prevDiceNumber = -1;
            _continuousUseCount = 0;
            _canActionUse = true;
            _lastDiceUsedForAction = false;
            DismissAllCards();
            return true;
        }

        public void PotionUseAction(int num)
        {
            StartCoroutine(GameManager.I.Player.CardAction(num, GameManager.I.Stage.Enemies[UnityEngine.Random.Range(0, GameManager.I.Stage.Enemies.Count)]));
            _state = CardState.Idle;
            DismissAllCards();
            if (GameManager.I.Stage.Enemies.Count == 0)
            {
                StartCoroutine(EndBattle());
            }
        }

        public void WarpArtifact()
        {
            StartCoroutine(GameManager.I.Player.MoveTo(1, 0.4f));
            _state = CardState.Idle;
            _prevDiceNumber = -1;
            _continuousUseCount = 0;
            _canActionUse = true;
            _lastDiceUsedForAction = false;
            DismissAllCards();
        }

        public bool IsWind1BlessUsable()
        {
            if(GameManager.I.Player.PlayerInfo.CheckBlessExist(BlessType.BlessWind1)
                                && _dices[_selectDiceIndex].RollResultNumber == 1 && (_prevDiceNumber == 5 || _prevDiceNumber == 6))
            {
                return true;
            }

            return false;
        }

        public bool IsNumberSequence()
        {
            if(_prevDiceNumber != -1 && _prevDiceNumber + 1 != _dices[_selectDiceIndex].RollResultNumber)
            {
                return true;
            }

            return false;
        }

        public bool CheckUseDiceOnAction(bool printMsg = true)
        {
            bool result = true;

            if (GameManager.I.Stage.Board.IsBoardSquare) {
                if (GameManager.I.Player.OnTile.Type == TileType.Start ||
                GameManager.I.Player.OnTile.Type == TileType.Blank)
                {
                    result = false;
                }
            }
            
            if (!_canActionUse)
            {
                result = false;
            }

            // [�����] ����
            if (GameManager.I.Player.CheckBuffExist(BuffType.ElectricShock) && _continuousUseCount >= 2)
            {
                if (printMsg)
                {
                    Debug.Log("뭐지? 감전당했나?");
                    GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_ELECTRICSHOCK));
                }
                result = false;
            }

            if (GameManager.I.Player.OnTile.IsSealed) {
                if (printMsg)
                {
                    Debug.Log("뭐지? 봉인당했나?");
                    GameManager.I.Player.Bubble.SetBubble(GameManager.I.Localization.Get(LocalizationEnum.PLAYER_SCRIPT_LOCK));
                }
                result = false;
            }

            return result;
        }

        private IEnumerator DiceUseAction(int num, DiceType type, BaseEntity target = null)
        {
            _diceUsedCountOnThisTurn++;
            SetDiceSelectable(false);
            _prevDiceNumber = num;

            bool hasDiscard = false;
            void ChangeDiscard()
            {
                hasDiscard = true;
            }

            switch (GameManager.I.Player.OnTile.TileMagic.Type)
            {
                case TileMagicType.Defence:
                case TileMagicType.Earth:
                case TileMagicType.Water:
                    StartCoroutine(Discard(_selectDiceIndex, DiceAnimationType.UseDefense, ChangeDiscard));
                    break;
                default:
                    StartCoroutine(Discard(_selectDiceIndex, DiceAnimationType.UseAttack, ChangeDiscard));
                    break;
            }

            //[축복] 태풍 : 3번째 행동마다 적에게 3의 데미지를 추가로 부여
            if (GameManager.I.Player.PlayerInfo.CheckBlessExist(BlessType.BlessWind2)&& _continuousUseCount == 2)
            {
                GameManager.I.Player.PlayerInfo.BlessEventDict[BlessType.BlessWind2]?.Invoke();
                target.Hit(3, TileMagicType.Wind);
            }

            yield return GameManager.I.Player.CardAction(num, target);
            DiceBuffByType(num, type, target);
            GameManager.I.DiceRollingCount++;

            yield return new WaitUntil(() => hasDiscard);

            _continuousUseCount++;
            _state = CardState.Idle;
            _lastDiceUsedForAction = true;
            UpdateDiceState(num, false);
            DismissAllCards();
            if (GameManager.I.CurrentEnemies.Count() == 0)
            {
                yield return EndBattle();
            }
            SetDiceSelectable(true);

        }

        private void DiceBuffByType(int num, DiceType type, BaseEntity target = null)
        {
            if(type==DiceType.Fire 
                && GameManager.I.Player.OnTile.TileMagic.Type == TileMagicType.Fire)
            {
                target.AddBuff(new Burn(num));
            }
            else if(type == DiceType.Water
                && GameManager.I.Player.OnTile.TileMagic.Type == TileMagicType.Water)
            {
                target.AddBuff(new Weak(num));
            }

            else if(type == DiceType.Earth
                && GameManager.I.Player.OnTile.TileMagic.Type == TileMagicType.Earth)
            {
                target.AddBuff(new Powerless(num));
            }
        }

        public void UpdateDiceState(int usedDiceNumber, bool isMove)
        {
            if (!_canActionUse)
            {
                foreach (DiceUI dice in _dicesUI)
                {
                    dice.CanMove = true;
                    dice.CanAction = false;

                }
                return;
            }

            if (isMove)
            {
                for (int i = 0; i < _dicesUI.Count; i++)
                {
                    _dicesUI[i].CanMove = true;
                    _dicesUI[i].CanAction = true;
                }
                return;
            }

            foreach (DiceUI dice in _dicesUI)
            {
                dice.CanAction = false;
                dice.CanMove = true;
            }

            int prevNum = usedDiceNumber + 1;
            for (int i = 0; i < _dicesUI.Count; i++)
            {
                // [�����] ����
                if (GameManager.I.Player.CheckBuffExist(BuffType.ElectricShock) && _continuousUseCount >= 2)
                {
                    return;
                }

                if (_dicesUI[i].Dice.RollResultNumber == prevNum)
                {
                    _dicesUI[i].CanAction = true;
                }
            }


        }
    }


}