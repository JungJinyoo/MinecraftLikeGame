using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class StoveUI : MonoBehaviour
{
    public static StoveUI Instance;

    public GameObject slotPrefab;
    public GameObject ResoultslotPrefab;

    public Transform gridProduction;       // slot 0
    public Transform gridProductionBurn;   // slot 1
    public Transform gridProductionResult; // slot 2

    public Image fireImg;   
    public Image cookImg;

    public Stove stoveScript;
    private List<Button> slotButtons;
    private InventoryData data => stoveScript?.inventoryData;

    void Awake()
    {
        Instance = this;
    }

    public void Open(Stove stove)
    {
        BindStove(stove);

        if (slotButtons == null)
            BuildSlots();

        Refresh();
        RefreshBars(); 
        SetProgress(0f);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        stoveScript = null;
        gameObject.SetActive(false);
    }

    public void BindStove(Stove stove)
    {
        stoveScript = stove;
    }

    public bool IsOpenedAt(Vector3Int pos)
    {
        if (stoveScript == null)
            return false;

        return stoveScript.GetPos() == pos;
    }

    public void SetProgress(float v)
    {
        if (cookImg != null)
            cookImg.fillAmount = Mathf.Clamp01(v);
    }

    public void RefreshBars()
    {
        if (stoveScript == null) return;

        // 요리 진행률 (cookTimer / cookTimeTotal)
        float cookRate = (stoveScript.cookTimeTotal > 0)
            ? stoveScript.cookTimer / stoveScript.cookTimeTotal
            : 0f;

        if (cookImg != null)
            cookImg.fillAmount = Mathf.Clamp01(cookRate);

        // 연료 남은 시간 (heatRemaining / maxFuelTime)
        float fireRate = (stoveScript.maxFuelTime > 0)
            ? stoveScript.heatRemaining / stoveScript.maxFuelTime
            : 0f;

        if (fireImg != null)
            fireImg.fillAmount = Mathf.Clamp01(fireRate);
    }

    void BuildSlots()
    {
        slotButtons = new List<Button>();

        CreateSlot(0, gridProduction);              // ingredient
        CreateSlot(1, gridProductionBurn);          // fuel
        CreateResultSlot(2, gridProductionResult);  // result
    }

    void CreateSlot(int index, Transform parent)
    {
        GameObject go = Instantiate(slotPrefab, parent);
        Button btn = go.GetComponent<Button>();
        slotButtons.Add(btn);

        btn.onClick.AddListener(() =>
        {
            InventoryManager.Instance.OnSSlotClicked(index, false, stoveScript.inventoryData);
            Refresh();
            RequestSyncToServer();
        });

        EventTrigger trigger = go.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) =>
        {
            var p = (PointerEventData)data;
            if (p.button == PointerEventData.InputButton.Right)
            {
                InventoryManager.Instance.OnSSlotClicked(index, true, stoveScript.inventoryData);
                Refresh();
                RequestSyncToServer();
            }
        });
        trigger.triggers.Add(entry);
    }

    void CreateResultSlot(int index, Transform parent)
    {
        GameObject go = Instantiate(ResoultslotPrefab, parent);
        Button btn = go.GetComponent<Button>();
        slotButtons.Add(btn);

        btn.onClick.AddListener(() =>
        {
            InventoryManager.Instance.OnSSlotClicked(index, false, stoveScript.inventoryData);
            Refresh();
            RequestSyncToServer();
        });

        EventTrigger trigger = go.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) =>
        {
            var p = (PointerEventData)data;
            if (p.button == PointerEventData.InputButton.Right)
            {
                InventoryManager.Instance.OnSSlotClicked(index, true, stoveScript.inventoryData);
                Refresh();
                RequestSyncToServer();
            }
        });
        trigger.triggers.Add(entry);
    }

    public void Refresh()
    {
        if (stoveScript == null || data == null || slotButtons == null) return;

        for (int i = 0; i < 3; i++)
        {
            var slot = data.SGetSlot(i);
            var icon = slotButtons[i].transform.GetChild(0).GetComponent<Image>();
            var countText = slotButtons[i].transform.GetChild(1).GetComponent<Text>();
            var countShadow = slotButtons[i].transform.GetChild(2).GetComponent<Text>();

            if (slot.IsEmpty)
            {
                icon.enabled = false;
                countText.text = "";
                countShadow.text = "";
            }
            else
            {
                icon.enabled = true;
                icon.sprite = slot.item.icon;

                string txt = slot.count > 1 ? slot.count.ToString() : "";
                countText.text = txt;
                countShadow.text = txt;
            }
        }
    }

    public void RequestSyncToServer()
    {
        if (stoveScript == null) return;

        Vector3Int pos = stoveScript.GetPos();
        int[] types = new int[3];
        int[] counts = new int[3];

        for (int i = 0; i < 3; i++)
        {
            var s = data.SGetSlot(i);
            types[i] = s.IsEmpty ? (int)ItemType.NONE : (int)s.item.itemType;
            counts[i] = s.IsEmpty ? 0 : s.count;
        }

        PhotonView worldPV = World.Instance.GetComponent<PhotonView>();

        worldPV.RPC("RPC_RequestSetStoveSlots", RpcTarget.MasterClient,
            pos.x, pos.y, pos.z, types, counts);

        worldPV.RPC("RPC_StartStoveCook", RpcTarget.MasterClient,
            pos.x, pos.y, pos.z);
    }
}
