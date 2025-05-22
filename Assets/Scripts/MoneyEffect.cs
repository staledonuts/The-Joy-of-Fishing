using System;
using Ami.BroAudio;
using UnityEngine;

public class MoneyEffect : MonoBehaviour
{
    [SerializeField] private FishRanking[] fishRanking;

    private GameObject FishCollector;

    public uint totalMoney = 0;
    public uint fractionMoney = 0;
    public bool gainedNow;

    private bool hookedFish = false;

    public static event Action DeleteFish;
    public static event Func<uint> TheMoney;

    BankAccountScript callBankAccountScript;

    private void Awake()
    {
        callBankAccountScript = FindAnyObjectByType<BankAccountScript>();
    }

    private void OnEnable()
    {
        BoatScript.DoneCollecting += EarnMoney;
        BaitScript.BaitIsOut += FindFishCollector;
        BaitScript.FishOnHook += IsFishOnHook;
        TheMoney = delegate () { return totalMoney; };
    }

    private void OnDisable()
    {
        BoatScript.DoneCollecting -= EarnMoney;
        BaitScript.BaitIsOut -= FindFishCollector;
        BaitScript.FishOnHook -= IsFishOnHook;
    }

    private void IsFishOnHook()
    {
        hookedFish = true;
    }

    private void FindFishCollector(bool bait)
    {
        if (!hookedFish) { return; }

        FishCollector = GameObject.FindGameObjectWithTag("FishInventory");
    }

    private void EarnMoney()
    {
        if (FishCollector.transform.childCount == 0) { return; }

        int a = 0;
        uint[] b = new uint[FishCollector.transform.childCount];

        for (int i = 0; i < FishCollector.transform.childCount; i++)
        {
            b[i] = FishCollector.transform.GetChild(i).GetComponent<FishStats>().Value;
            if (FishCollector.transform.GetChild(i).GetComponent<FishStats>().BaitLevel > a)
            {
                a = FishCollector.transform.GetChild(i).GetComponent<FishStats>().BaitLevel;
            }
        }

        foreach(uint value in b)
        {
            totalMoney += value;
            fractionMoney = value;
            if (callBankAccountScript.thisIs2)
            {
                callBankAccountScript.elapsed = 0;
            }
            if (gainedNow == false)
            {
                gainedNow = true;
            }

        }
        fishRanking[a].Play(transform);

        DeleteFish?.Invoke();
        TheMoney?.Invoke();
    }
}