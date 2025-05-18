using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ami.BroAudio;

public class BuyStuff : MonoBehaviour
{
    public Button mindcontrol, lvl1Line, lvl2Line, lvl3Line;
    public uint mindControlCost, lvl1Cost, lvl2Cost, lvl3Cost;
    public int lvl1Length, lvl2Length, lvl3Lenght;
    private MoneyEffect callMoneyEffectScript;
    private BoatScript callBoatScript;
    [SerializeField] private SoundID buyInst;

    private void Start() 
    {
    callMoneyEffectScript = FindAnyObjectByType<MoneyEffect>();   
    callBoatScript = FindAnyObjectByType<BoatScript>(); 
    }

    public void BuyMindControlLure()
    {
        if (callMoneyEffectScript.totalMoney >= mindControlCost)
        {
            mindcontrol.interactable = false;
            GameManager.Instance.MindcontrolActive = true;
            callMoneyEffectScript.totalMoney = callMoneyEffectScript.totalMoney - mindControlCost;
            FinishBuySound(true);
        }
        else
        {
            Debug.Log("not enough money!");
            FinishBuySound(false);
        }
    }


    public void BuyLineLVL1()
    {
        if (callMoneyEffectScript.totalMoney >= lvl1Cost)
        {
            lvl1Line.interactable = false;
            callBoatScript.maxLineLength = lvl1Length;
            callMoneyEffectScript.totalMoney = callMoneyEffectScript.totalMoney - lvl1Cost;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL2()
    {
        if (callMoneyEffectScript.totalMoney >= lvl2Cost)
        {
            lvl1Line.interactable = false;
            lvl2Line.interactable = false;
            callBoatScript.maxLineLength = lvl2Length;
            callMoneyEffectScript.totalMoney = callMoneyEffectScript.totalMoney - lvl1Cost;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL3()
    {
        if (callMoneyEffectScript.totalMoney >= lvl3Cost)
        {
            lvl1Line.interactable = false;
            lvl2Line.interactable = false;
            lvl3Line.interactable = false;
            callBoatScript.maxLineLength = lvl3Lenght;
            callMoneyEffectScript.totalMoney = callMoneyEffectScript.totalMoney - lvl1Cost;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void StartBuySound() // Public for event component
    {
       //Reset Parameters for event
       
       //buyInst.setParameterByName("gotMoney", 0);
       //buyInst.setParameterByName("mouseReleased", 0);
       //buyInst.start();
    }

    private void FinishBuySound(bool money)
    {
        //buyInst.setParameterByName("gotMoney", money ? 1 : 0);
        // ? Operator = condition ? consequent : alternative
        // if true consequent expression is chose.

        //buyInst.setParameterByName("mouseReleased", 1);
    }

}
