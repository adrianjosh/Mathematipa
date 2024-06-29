using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog, Menu, PartyScreen, Bag, Cutscene, Paused, Evolution, Shop, Dead, AdventureMenu, Map, WrongAnswers }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI InventoryUI;
    [SerializeField] GameObject GameOverScreen;
    [SerializeField] GameObject AdventureMenuScreen;
    [SerializeField] UnityEngine.UI.Button button;
    [SerializeField] GameObject WrongAnswerUI;

    [SerializeField] GameObject essentialObjectsPrefab;

    [SerializeField] MapController mapController;

    [SerializeField] IncorrectAnswerLogger incorrectAnswerLogger;

    [SerializeField] AudioClip introMusic;
    [SerializeField] public float loopStartPoint = 0f;
    [SerializeField] public float loopEndPoint = 0f;

    GameState state;

    GameState previState;
    GameState stateBeforeEvolution;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviScene { get; private set; }
    MenuController menuController;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        FakemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        StartAdventureMenuState();
        incorrectAnswerLogger.UpdateLogDisplay();

        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            previState = state;
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnDialogFinished += () =>
        {
            if (state == GameState.Dialog)
            {
                state = previState;
            }
        };
        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };
        menuController.onMenuSelected += OnMenuSelected;

        EvolutionManager.i.OnStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };
        EvolutionManager.i.OnCompleteEvolution += () =>
        {
            partyScreen.SetPartyData();
            state = stateBeforeEvolution;

            AudioManager.i.PlayMusicWithLoopPoints(CurrentScene.SceneMusic, fade: true, loopStartPoint: CurrentScene.loopStartPoint, loopEndPoint: CurrentScene.loopEndPoint);
        };

        ShopController.i.OnStart += () => state = GameState.Shop;
        ShopController.i.OnFinish += () => state = GameState.FreeRoam;

    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            previState = state;
            state = GameState.Paused;
        }
        else
        {
            state = previState;
        }
    }

    public void StartCutsceneState()
    {
        state = GameState.Cutscene;
    }

    public void StartFreeRoamState()
    {
        state = GameState.FreeRoam;
    }
    public void StartAdventureMenuState()
    {
        state = GameState.AdventureMenu;
    }
    public void StartFreeRoamStateInAdventureMenu()
    {
        state = GameState.FreeRoam;
        
        //StartCoroutine(AudioManager.i.UnPauseMusic(0));
    }
    public void CloseLargeMap()
    {
        MapManager.instance.CloselargeMap();
    }


    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        mapController.mapUIPrefab.SetActive(false);

        var playerParty = playerController.GetComponent<FakemonParty>();
        var wildFakemon = CurrentScene.GetComponent<MapArea>().GetRandomWildFakemon();

        var wildFakemonCopy = new Fakemon(wildFakemon.Base, wildFakemon.Level);

        
        battleSystem.StartBattle(playerParty, wildFakemonCopy);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        mapController.mapUIPrefab.SetActive(false);

        this.trainer = trainer;

        var playerParty = playerController.GetComponent<FakemonParty>();
        var trainerParty = trainer.GetComponent<FakemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.triggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
        else if (won == false)
        {
            state = GameState.Dead;
            
            StartCoroutine(Fader.i.FadeIn(0.5f));
            GameOverScreen.SetActive(true);
            StartCoroutine(Fader.i.FadeOut(0.5f));
            StartCoroutine(TransitionDelay());
            AudioManager.i.PlaySfx(AudioId.GameOver);
            return;
            
        }
        else
        {

        }


        partyScreen.SetPartyData();

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<FakemonParty>();
        bool hasEvolutions = playerParty.CheckForEvolutions();

        if (hasEvolutions)
        {
            StartCoroutine(playerParty.RunEvolutions());
        }
        else
        {
            if (!(state == GameState.Dead))
            {
                AudioManager.i.PlayMusicWithLoopPoints(CurrentScene.SceneMusic, fade: true, loopStartPoint: CurrentScene.loopStartPoint, loopEndPoint: CurrentScene.loopEndPoint);
            }
        }
        
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
                //music menuopen
                AudioManager.i.PlaySfx(AudioId.MenuOpen, pauseMusic: false);
            }
            SavingSystem.i.saveData.SetActive(false);
        }
        else if (state == GameState.AdventureMenu)
        {
            AudioManager.i.PlayMusicWithLoopPoints(introMusic, fade: true, loopStartPoint: loopStartPoint, loopEndPoint: loopEndPoint);
        }
        else if (state == GameState.Cutscene)
        {
            playerController.Character.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //todo: go to summary screen
            };
            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Action onBack = () =>
            {
                InventoryUI.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                state = GameState.FreeRoam;
            };

            InventoryUI.HandleUpdate(onBack);
        }
        else if (state == GameState.Shop)
        {
            ShopController.i.HandleUpdate();
        }
        else if (state == GameState.Map)
        {
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                if (!MapManager.instance.IsLargeMapOpen)
                {
                    MapManager.instance.OpenlargeMap();
                }
                else
                {
                    CloseLargeMap();
                }
            }
        }
        else if (state == GameState.WrongAnswers)
        {
            WrongAnswerUI.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                WrongAnswerUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            }
        }


    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PreviScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //Fakemon
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            //Bag
            InventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            //Map
            state = GameState.Map;
            MapManager.instance.OpenlargeMap();
        }
        else if (selectedItem == 3)
        {
            //Wrong Answer UI
            state = GameState.WrongAnswers;
        }
        else if (selectedItem == 4)
        {
            //Save
            SavingSystem.i.Save("saveSlot1");
            SavingSystem.i.saveData.SetActive(false);
            SavingSystem.i.haveASaveFile = true;

            PlayerPrefs.SetString("WrongAnswer", incorrectAnswerLogger.logText.text);
            SavingSystem.i.Save("saveSlot1");

            state = GameState.FreeRoam;
        }
        else if (selectedItem == 5)
        {
            //Load
            SavingSystem.i.Load("saveSlot1");
            SavingSystem.i.Load("saveSlot1");

            incorrectAnswerLogger.logText.text = PlayerPrefs.GetString("WrongAnswer");

            state = GameState.FreeRoam;
        }
        else if (selectedItem == 6)
        {
            //Quit
            StartCoroutine(MainMenuSelect());
        }

    }

    IEnumerator MainMenuSelect()
    {
        state = GameState.Paused;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("Do you want to go back to the Main Menu?",
            waitForInput: false,
            choices: new List<string>() { "Yes", "No"},
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Yes
            yield return Fader.i.FadeIn(0.5f);
            yield return TransitionDelay();
            DestroyObjectsPrefab();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 3);
        }
        else if (selectedChoice == 1)
        {
            state = GameState.FreeRoam;
        }
    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut=false)
    {
        yield return Fader.i.FadeIn(0.5f);

        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        if (waitForFadeOut)
        {
            yield return Fader.i.FadeOut(0.5f);
        }
        else
        {
            StartCoroutine(Fader.i.FadeOut(0.5f));
        }
        
    }

    public void ReloadScene()
    {
        //SceneManager.LoadScene("MainMenu");

        GameOverScreen.SetActive(false);
        DestroyObjectsPrefab();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public void DestroyObjectsPrefab()
    {
        Destroy(essentialObjectsPrefab);
    }

    public void GoBackToMainMenu()
    {
        Destroy(essentialObjectsPrefab);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 3);
    }

    public void LoadGameOnDeath()
    {
        if (!SavingSystem.i.haveASaveFile)
        {
            SavingSystem.i.Load("saveSlot1");
        }
        if (SavingSystem.i.haveASaveFile)
        {
            GameOverScreen.SetActive(false);
            state = GameState.FreeRoam;
            battleSystem.gameObject.SetActive(false);
            worldCamera.gameObject.SetActive(true);

            PlayCurrentSceneMusic();

            SavingSystem.i.Load("saveSlot1");
            SavingSystem.i.Load("saveSlot1");
            //StartCoroutine(AudioManager.i.UnPauseMusic(2));
        }
        incorrectAnswerLogger.logText.text = PlayerPrefs.GetString("WrongAnswer");
    }

    public void LoadGameOnAdventureMenu()
    {
        if (!SavingSystem.i.haveASaveFile)
        {
            SavingSystem.i.Load("saveSlot1");
            
        }

        if (SavingSystem.i.haveASaveFile)
        {
            AdventureMenuScreen.SetActive(false);
            state = GameState.FreeRoam;

            PlayCurrentSceneMusic();

            SavingSystem.i.Load("saveSlot1");
            SavingSystem.i.Load("saveSlot1");
            //StartCoroutine(AudioManager.i.UnPauseMusic(1));
        }
        incorrectAnswerLogger.logText.text = PlayerPrefs.GetString("WrongAnswer");
    }

    public void PlayCurrentSceneMusic()
    {
        AudioManager.i.PlayMusicWithLoopPoints(CurrentScene.SceneMusic, fade: true, loopStartPoint: CurrentScene.loopStartPoint, loopEndPoint: CurrentScene.loopEndPoint);
    }
    public void StartNewGame()
    {
        //StartCoroutine();
        StartCoroutine(TransitionDelay());

        PlayCurrentSceneMusic();
    }

    IEnumerator TransitionDelay()
    {
        yield return AudioManager.i.FadeMusic();
        yield return new WaitForSeconds(0.75f);
    }


    public GameState State => state;
}
