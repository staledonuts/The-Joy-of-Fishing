using System;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIManager : MonoBehaviour
{
    private static UIManager instance = null;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Find singleton of this type in the scene
                instance = FindFirstObjectByType<UIManager>();

                // If there is no singleton object in the scene, create one
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("UIManager_Singleton");
                    instance = singletonObject.AddComponent<UIManager>();
                }
            }
            return instance;
        }
    }

    [SerializeField] private SoundID buyInst;

    [SerializeField] private Image _GameStartLogo;
    [SerializeField] private Button _mindcontrol; 
    [SerializeField] private Button _lvl1Line; 
    [SerializeField] private Button _lvl2Line; 
    [SerializeField] private Button _lvl3Line;
    [SerializeField] private uint _mindControlCost; 
    [SerializeField] private uint _lvl1Cost; 
    [SerializeField] private uint _lvl2Cost; 
    [SerializeField] private uint _lvl3Cost;
    [SerializeField] private int _lvl1Length; 
    [SerializeField] private int _lvl2Length;
    [SerializeField] private int _lvl3Lenght;

    private void Awake()
    {   
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning("Another instance of UIManager found, destroying this new one.");
            Destroy(gameObject);
        }
    }

    

    private void Start() 
    {

    }

    public void BuyMindControlLure()
    {
        if (Inventory.Instance.SpendMoney(_mindControlCost))
        {
            _mindcontrol.interactable = false;
            GameManager.Instance.MindcontrolActive = true;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL1()
    {
        if (Inventory.Instance.SpendMoney(_lvl1Cost))
        {
            _lvl1Line.interactable = false;
            CustomRopeSolver.Instance.maxNodes = _lvl1Length;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL2()
    {
        if (Inventory.Instance.SpendMoney(_lvl2Cost))
        {
            _lvl1Line.interactable = false;
            _lvl2Line.interactable = false;
            CustomRopeSolver.Instance.maxNodes = _lvl2Length;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL3()
    {
        if (Inventory.Instance.SpendMoney(_lvl3Cost))
        {
            _lvl1Line.interactable = false;
            _lvl2Line.interactable = false;
            _lvl3Line.interactable = false;
            CustomRopeSolver.Instance.maxNodes = _lvl3Lenght;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void TweenLogo()
    {
        if(_GameStartLogo != null)
        {
            if(_GameStartLogo.material.GetInstanceID() == _GameStartLogo.materialForRendering.GetInstanceID())
            {
                Debug.Log("Creating a new material instance");
                _GameStartLogo.material = Instantiate(_GameStartLogo.material);
            }
            _GameStartLogo.material.TweenMaterialFloat("_Dissolve", 0f, 2f, () => {
                _GameStartLogo.gameObject.SetActive(false);
            }, Tween.Easing.EaseInOutQuad);
        }
    }
}
