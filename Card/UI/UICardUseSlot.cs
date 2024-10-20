using Cardinals.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardinals
{
    public class UICardUseSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private MouseState _slotState;
        public void OnPointerEnter(PointerEventData eventData)
        {
            //GameManager.I.Stage.CardManager.MouseState = _slotState;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //GameManager.I.Stage.CardManager.MouseState = MouseState.Cancel;
        }
    }

}
