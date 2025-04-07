using TMPro;
using UnityEngine;
using Utilities;

namespace Game.UI
{
    public class CoinsUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinsAmountText;

        private int _coinAmount;

        private void Awake()
        {
            coinsAmountText.SetText(_coinAmount.ToString());
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvents.CoinAmountEvent>(IncreaseCoin);
        }

        private void OnDisable()
        {
            EventBus.UnSubscribe<GameEvents.CoinAmountEvent>(IncreaseCoin);
        }

        private void IncreaseCoin()
        {
            _coinAmount++;
            AnimationUtils.AnimateTextPop(coinsAmountText);
            coinsAmountText.SetText(_coinAmount.ToString());
        }
    }
}