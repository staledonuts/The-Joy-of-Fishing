using Ami.BroAudio;
using UnityEngine;

public class MoneyEffect : MonoBehaviour
{
    [SerializeField] private SoundID rarefishGet;
    [SerializeField] private SoundID commonfishGet;
    [SerializeField] private SoundID legendaryfishGet;
    [SerializeField] private SoundID lvl0coins;
    [SerializeField] private SoundID lvl1coins;
    [SerializeField] private SoundID lvl2coins;
    [SerializeField] private SoundID lvl3coins;

    [SerializeField] private ParticleSystem[] coinParticle;

    private GameObject FishCollector;

    public uint totalMoney = 0;
    public uint fractionMoney = 0;
    public bool gainedNow;

    private bool hookedFish = false;

    public static event System.Action DeleteFish;
    public static event System.Func<uint> TheMoney;

    BankAccountScript callBankAccountScript;

    private void Awake()
    {
        //FMODUnity.RuntimeManager.AttachInstanceToGameObject(fishGetInstance, gameObject.transform);
        //FMODUnity.RuntimeManager.AttachInstanceToGameObject(coinsInstnace, gameObject.transform);
        //coinsInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        //fishGetInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
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
            b[i] = FishCollector.transform.GetChild(i).GetComponent<FishStats>().fishStats.value;
            if (FishCollector.transform.GetChild(i).GetComponent<FishStats>().fishStats.baitLevel > a)
            {
                a = FishCollector.transform.GetChild(i).GetComponent<FishStats>().fishStats.baitLevel;
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
        //Debug.Log(totalMoney);


        coinParticle[a].Play();
        PlaySound(a);

        DeleteFish?.Invoke();
        TheMoney?.Invoke();
    }

    private void PlaySound(int level)
    {
        // Levels 0 1 2 3 small to big.
        switch (level)
        {
            default:
            case 0:
            {
                lvl0coins.Play(transform);
                commonfishGet.Play(transform);
                break;
            }
            case 1:
            {
                lvl1coins.Play(transform);
                commonfishGet.Play(transform);
                break;
            }
            case 2:
            {
                lvl2coins.Play(transform);
                rarefishGet.Play(transform);
                break;
            }
            case 3:
            {
                lvl3coins.Play(transform);
                legendaryfishGet.Play(transform);

                break;
            }
        }

    }


}