using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataUI : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] private TMP_Text playerNameText, coinsText;

    void Start()
    {
        if (dataContainer != null)
        {
            playerNameText.text = dataContainer.username;
            coinsText.text = dataContainer.coin.ToString();
        }
    }
}
