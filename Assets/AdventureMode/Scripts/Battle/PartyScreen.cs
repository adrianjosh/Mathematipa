using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    PartyMemberUI[] memberSlots;
    List<Fakemon> fakemons;
    FakemonParty party;
    int selection = 0;
    public Fakemon SelectedMember => fakemons[selection];

    // So party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }
    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = FakemonParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        fakemons = party.Fakemons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < fakemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(fakemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Monster";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var previSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++selection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --selection;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selection -= 2;
        }

        selection = Mathf.Clamp(selection, 0, fakemons.Count - 1);

        if (selection != previSelection)
        {
            UpdateMemberSelection(selection);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < fakemons.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void ShowIfManualIsUsable(ManualItem manualItem)
    {
        for (int i = 0; i < fakemons.Count; i++)
        {
            string message = manualItem.CanBeTaught(fakemons[i]) ? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < fakemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
