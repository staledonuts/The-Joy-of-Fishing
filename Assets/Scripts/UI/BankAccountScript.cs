using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BankAccountScript : MonoBehaviour
{
    MoneyEffect callMoneyEffectScript;
    TextMeshProUGUI BankText;
    public bool thisIs1;
    public bool thisIs2;
    Rigidbody2D thisIs2rb;
    RectTransform thisIs2transform;
    public float elapsed;
    RectTransform TextMeshParentTransform;
    void Start()
    {
        callMoneyEffectScript = FindAnyObjectByType<MoneyEffect>();
        BankText = GetComponent<TextMeshProUGUI>();
        if (thisIs2)
        {
            thisIs2rb = GetComponent<Rigidbody2D>();
            thisIs2transform = GetComponent<RectTransform>();
        }
        BankText.text = "";
        TextMeshParentTransform = GameObject.Find("MoneyGainedCanvas").GetComponent<RectTransform>();

    }
    
    void Update()
    {

        if (GameManager.Instance.moveCam != 3 && thisIs1)
        {
            BankText.text = "Savings: " + callMoneyEffectScript.totalMoney.ToString() + "c";
        }

        if (thisIs2 && callMoneyEffectScript.totalMoney > 0 && callMoneyEffectScript.gainedNow == true)
        {
            
            elapsed += Time.deltaTime;
            BankText.text = "Gains: " + callMoneyEffectScript.fractionMoney + "c";
            thisIs2rb.linearVelocity = new Vector2(0, 1);
            if(elapsed <= 0.2)
            {
                BankText.color = new Color(1, 1, 1, 1);
            }
            if (elapsed >= 3 && elapsed <= 4f)
            {
                BankText.color = new Color(1, 1, 1, 0);
                thisIs2rb.linearVelocity = new Vector2(0, -3);
            }
            if (elapsed > 4f)
            {
                thisIs2rb.linearVelocity = Vector2.zero;
                callMoneyEffectScript.gainedNow = false;
            }
           
            
        }
    }

    IEnumerator MoneyGainedTextVector()
    {

        yield return new WaitForSeconds(3);
        thisIs2rb.linearVelocity = Vector2.zero;
    }
}
