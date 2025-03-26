using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject kart;
    [SerializeField] Image driftDurationImg;
    [SerializeField] Transform[] itemInventImg;
    [SerializeField] Image itemImgSlot;
    // ���Կ� �� ������ �̹���
    [SerializeField] Sprite itemImg;

    TestCHMKart kartCtrl;

    public int itemCount { get; set; }

    bool isItemUseNotYet;
    float blinkSpeed = 0.5f;

    void Start()
    {
        itemCount = 0;
    }

    void Update()
    {
        if (kartCtrl != null)
        {
            driftDurationImg.fillAmount = (kartCtrl.boostGauge / kartCtrl.maxBoostGauge);     
        }

        // ���߿� �ν�Ʈ �Ӹ� �ƴ϶� �������� ������ ���� �ʿ�
        if(kartCtrl != null && kartCtrl.isBoostCreate && itemCount < 2)
        {
            MakeInventoryItemSlot();
            kartCtrl.isBoostCreate = false;
            StopCoroutine(ItemUseNotYet());
        }

        // �ӽ÷�
        if(Input.GetKeyDown(KeyCode.H))
        {
            itemCount--;
            // �� �� ���Կ� ����ִ� �̹��� ���ֱ�
            // ���� ���Կ� �ִ� �̹��� �� �� ���Կ� �ֱ�
            if(itemCount == 0)
            {
                itemInventImg[0].GetComponent<Image>().color = new Color(0, 0, 0, 0.6156863f);
                itemInventImg[itemCount].GetChild(0).gameObject.SetActive(false);
                isItemUseNotYet = false;
            }
            else
            {
                itemInventImg[itemCount].GetChild(0).gameObject.SetActive(false);
                itemInventImg[itemCount - 1].GetChild(0).GetComponent<Image>().sprite = itemImg;
            }
        }
    }

    void MakeInventoryItemSlot()
    {
        Image slot = null;
        if (itemInventImg[itemCount].childCount != 0)
        {
            itemInventImg[itemCount].GetChild(0).gameObject.SetActive(true);
            slot = itemInventImg[itemCount].GetChild(0).GetComponent<Image>();
        }
        else
        {
            slot = Instantiate(itemImgSlot, itemInventImg[itemCount]);
        }

        slot.sprite = itemImg;
        // ��Ʈ�� Ű�� ������ itemCount--;
        itemCount++;

        isItemUseNotYet = true;

        if(itemCount == 1)
        {
            itemInventImg[0].GetComponent<Image>().color = new Color(1, 1, 1, 0.3686275f);
            StartCoroutine(ItemUseNotYet());
        }
    }

    IEnumerator ItemUseNotYet()
    {
        while(isItemUseNotYet)
        {
            Color color = itemInventImg[0].GetComponent<Image>().color;
            color = (color.r == 1) ? new Color(0.6f, 0.6f, 0.6f, 0.3686275f) : new Color(1, 1, 1, 0.3686275f);
            itemInventImg[0].GetComponent<Image>().color = color;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
    
    public void SetKart(GameObject instance)
    {
        kart = instance;
        kartCtrl = kart.GetComponent<TestCHMKart>();
    }    
}
