using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cardinals.Enums;
using Util;
using TMPro;

namespace Cardinals
{
    public class DiceTitleDescription : MonoBehaviour
    {
        [SerializeField] private GameObject _diceDescription;

        [SerializeField] private Image _infoPanel;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _info;

        [SerializeField] private GameObject _buffInfoPanel;
        [SerializeField] private TextMeshProUGUI _buffTitle;
        [SerializeField] private TextMeshProUGUI _buffInfo;
        public void Init(List<int> numbers, DiceType type, Dice dice)
        {
            for (int i = 0; i < numbers.Count; i++)
            {
                GameObject surface = Instantiate(ResourceLoader.LoadPrefab(Constants.FilePath.Resources.Prefabs_UI_DiceSurface), _diceDescription.transform);
                surface.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = numbers[i].ToString();

                string path = "Dice/Dice_" + type.ToString() + "_" + numbers[i].ToString();
                surface.GetComponent<Image>().sprite = ResourceLoader.LoadSprite(path);

            }
            DiceDataSO data = DiceDataSO.Data(type);
            _title.text = TMPUtils.CustomParse(data.title, true);
            _title.color = data.elementColor;
            _info.text = TMPUtils.CustomParse(data.information, true);
            _infoPanel.color = data.elementColor;

            BuffDataSO buffData = BuffDataSO.Data(dice.DiceBuffType);
            if (buffData == null) return;

            string buffIcon = $"<debuff={dice.DiceBuffType.ToString()}> ";
            _buffTitle.text = TMPUtils.CustomParse(buffData.buffName, true);
            _buffInfo.text = buffData.Description;
        }

        public void UpdateDiceDescription(Dice dice)
        {
            List<int> numbers = dice.DiceNumbers;
            DiceType type = dice.DiceType;

            for (int i = 0; i < _diceDescription.transform.childCount; i++)
            {
                Image image = _diceDescription.transform.GetChild(i).GetComponent<Image>();
                string path = "Dice/Dice_" + type.ToString() + "_" + numbers[i].ToString();
                image.sprite = ResourceLoader.LoadSprite(path);

            }

            DiceDataSO data = DiceDataSO.Data(dice.DiceType);
            _title.text = TMPUtils.CustomParse(data.title, true);
            _title.color = data.elementColor;
            _info.text = TMPUtils.CustomParse(data.information, true);
            _infoPanel.color = data.elementColor;

            BuffDataSO buffData = BuffDataSO.Data(dice.DiceBuffType);
            string buffIcon = $"<debuff={dice.DiceBuffType.ToString()}> ";
            _buffTitle.text = TMPUtils.CustomParse(buffData.buffName, true);
            _buffInfo.text = buffData.Description;
        }

        public void SetDescriptionUIHovered(int index, BuffType buffType)
        {
            ResetOutline();
            if (index != -1)
            {
                _diceDescription.transform.GetChild(index).GetComponent<Outline>().enabled = true;
            }
            _diceDescription.SetActive(true);
            _infoPanel.gameObject.SetActive(true);

            if (buffType == BuffType.Empty) return;
            _buffInfoPanel.SetActive(true);
        }

        public void SetDescriptionUIRestored()
        {
            _diceDescription.SetActive(false);
            _infoPanel.gameObject.SetActive(false);
            _buffInfoPanel.SetActive(false);
        }

        private void ResetOutline()
        {
            foreach (Transform t in _diceDescription.transform)
            {
                t.GetComponent<Outline>().enabled = false;
            }
        }
    }
}
