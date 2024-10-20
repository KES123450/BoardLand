using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Cardinals.UI;
using UnityEngine;
using Util;

namespace Cardinals.Game {

	public class UICardSystem : MonoBehaviour {

		ComponentGetter<RectTransform> _diceDeck
			= new ComponentGetter<RectTransform>(TypeOfGetter.ChildByName, "DiceDeck");

		ComponentGetter<UICardUseSlot> _cardMoveSlot
			= new ComponentGetter<UICardUseSlot>(TypeOfGetter.ChildByName, "CardMoveSlot");
		ComponentGetter<UICardUseSlot> _cardActionSlot
			= new ComponentGetter<UICardUseSlot>(TypeOfGetter.ChildByName, "CardActionSlot");

		public void Init() {
			GameManager.I.Stage.DiceManager.SetDiceDeckUIParent(_diceDeck.Get(gameObject).transform);
			//_sortButton.Get(gameObject).onClick.AddListener(() => GameManager.I.Stage.DiceManager.SortDices());
		}
	}

}
