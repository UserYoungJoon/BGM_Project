using TMPro;
using UnityEngine;
using YoungJoon.L1.Text;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle.View
{
    public class CardInfoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _hp;
        [SerializeField] private TMP_Text _cate;
        [SerializeField] private TMP_Text _cost;
        [SerializeField] private TMP_Text _tooltip;

        public void Show(CardBase card)
        {
            var data = card.Data;
            _name.text = data.CardName;
            _hp.text = card.CurrentHp + " / " + card.MaxHp;
            _cate.text = card.Type.GetCategory().ToString();
            _cost.text = "Cost " + card.Cost;
            _tooltip.text = TooltipHelper.Build(data.Tooltip, card.TooltipArgs());
        }

        public void Clear()
        {
            _name.text = string.Empty;
            _hp.text = string.Empty;
            _cate.text = string.Empty;
            _cost.text = string.Empty;
            _tooltip.text = string.Empty;
        }
    }
}
