using Gameplay.Mechanics.Collectables;
using Gameplay.Models;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Gameplay.Components.View
{
    public sealed class SessionWalletTextView : MonoBehaviour
    {
        [Inject] private CoinCollectionHandler _coinCollectionHandler;
        [SerializeField] private TMP_Text _text;

        private Wallet _wallet;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            _coinCollectionHandler.OnWalletInitialized += Bind;
            Bind(_coinCollectionHandler.SessionWallet);
        }

        private void OnDisable()
        {
            if (_coinCollectionHandler != null)
                _coinCollectionHandler.OnWalletInitialized -= Bind;

            Unbind();
        }

        private void Bind(Wallet wallet)
        {
            if (!ReferenceEquals(_wallet, wallet))
            {
                Unbind();
                _wallet = wallet;

                if (_wallet != null)
                    _wallet.OnBalanceChanged += HandleBalanceChanged;
            }

            UpdateText(_wallet?.Balance ?? 0);
        }

        private void Unbind()
        {
            if (_wallet != null)
                _wallet.OnBalanceChanged -= HandleBalanceChanged;

            _wallet = null;
        }

        private void HandleBalanceChanged(long previousBalance, long currentBalance)
        {
            UpdateText(currentBalance);
        }

        private void UpdateText(long value)
        {
            if (_text != null)
                _text.text = value.ToString();
        }
    }
}
