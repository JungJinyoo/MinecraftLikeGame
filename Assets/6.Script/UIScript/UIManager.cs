using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static bool CraftOpen;
    public static bool FurnaceOpen;
    public static bool InvenOpen;

    public GameObject testInven;
    public GameObject testFurnace;
    public GameObject testCraft;

    public Chat chat;

    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (CraftOpen == false && FurnaceOpen == false)
            {
                InvenOpen = !InvenOpen;
                return;
            }
            CraftOpen = false;
            FurnaceOpen = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CraftOpen = false;
            FurnaceOpen = false;
            InvenOpen = false;
        }
        if (CraftOpen)
        { testCraft.SetActive(true); }
        else
        {testCraft.SetActive(false);}
        if (FurnaceOpen)
        { testFurnace.SetActive(true); }
        else
        {testFurnace.SetActive(false);}
        if (InvenOpen)
        { testInven.SetActive(true); }
        else
        {testInven.SetActive(false);}
    }
}
