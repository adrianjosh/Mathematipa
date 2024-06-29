using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy}

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] TMP_Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    Action<ItemBase> onItemUsed;

    int selectedItem = 0;
    int selectedCategory = 0;

    MoveBase moveToLearn;

    InventoryUIState state;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;

    Inventory inventory;
    RectTransform itemListRect;
    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;
    }
    void UpdateItemList()
    {
        //clear all the existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int previSelection = selectedItem;
            int previCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ++selectedCategory;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                --selectedCategory;
            }

            if (selectedCategory > Inventory.ItemCategories.Count - 1)
            {
                selectedCategory = 0;
            }
            else if (selectedCategory < 0)
            {
                selectedCategory = Inventory.ItemCategories.Count - 1;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if (previCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
            else if (previSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                onBack?.Invoke();
            }

        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if (GameController.Instance.State == GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;

        }

        if (GameController.Instance.State == GameState.Battle)
        {
            // in battle
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item can't be used in battle");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            // outside battle
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item can't be used outside the battle");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        if (selectedCategory == (int)ItemCategory.MonsterBaits)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
            if (item is ManualItem)
            {
                //show if manual is usable
                partyScreen.ShowIfManualIsUsable(item as ManualItem);
            }
        }

    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleManualItems();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var fakemon = partyScreen.SelectedMember;

        //Handle Evolution
        if (item is EvolutionItems)
        {
            var evolution = fakemon.CheckForEvolution(item);
            if (evolution != null)
            {
                yield return EvolutionManager.i.Evolve(fakemon, evolution);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialogText($"It won't have any effect!");
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem =  inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (usedItem != null)
        {
            if (usedItem is RecoveryItem)
            {
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");
            }
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (selectedCategory == (int)ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText($"It won't have any effect!");
        }

        ClosePartyScreen();
    }

    IEnumerator HandleManualItems()
    {
        var manualItem = inventory.GetItem(selectedItem, selectedCategory) as ManualItem;
        if (manualItem == null)
        {
            yield break;
        }

        var fakemon = partyScreen.SelectedMember;

        if (fakemon.HasMove(manualItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{fakemon.Base.Name} already knows {manualItem.Move.Name}");
            yield break;
        }

        if (!manualItem.CanBeTaught(fakemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{fakemon.Base.Name} can't learn {manualItem.Move.Name}");
            yield break;
        }

        if (fakemon.Moves.Count < FakemonBase.MaxNumberOfMoves)
        {
            fakemon.LearnMove(manualItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{fakemon.Base.Name} learned {manualItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{fakemon.Base.Name} is trying to learn {manualItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {FakemonBase.MaxNumberOfMoves} moves!");
            yield return ChooseMoveToForget(fakemon, manualItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }
    }

    IEnumerator ChooseMoveToForget(Fakemon fakemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Choose a move that you want to forget", true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(fakemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                slotUIList[i].NameText.color = Color.black;
            }
        }

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport)
        {
            return;
        }
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelection()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;

        partyScreen.ClearMemberSlotMessages();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {   
        var fakemon = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();

        moveSelectionUI.gameObject.SetActive(false);
        if (moveIndex == FakemonBase.MaxNumberOfMoves)
        {
            // Dont learn a new move
            yield return DialogManager.Instance.ShowDialogText($"{fakemon.Base.Name} did not learn {moveToLearn.Name}");
        }
        else
        {
            //forget a move
            var selectedMove = fakemon.Moves[moveIndex].Base;

            yield return DialogManager.Instance.ShowDialogText($"{fakemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}");
            fakemon.Moves[moveIndex] = new Move(moveToLearn);
        }
        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
