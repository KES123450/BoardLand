using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardinals.UI
{
    public class UICardDeck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Action<bool> _onMouseHover;

        public void Init(Action<bool> onMouseHover)
        {
            _onMouseHover = onMouseHover;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onMouseHover?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onMouseHover?.Invoke(false);
        }
        
    }
}
