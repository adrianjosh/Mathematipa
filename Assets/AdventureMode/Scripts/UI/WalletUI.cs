using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WalletUI : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText;

    private void Start()
    {
        Wallet.i.OnMoneyChanged += SetMoneyText;
    }
    public void Show()
    {
        gameObject.SetActive(true);
        SetMoneyText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void SetMoneyText()
    {
        moneyText.text = "$ " + Wallet.i.Money;
    }
}
