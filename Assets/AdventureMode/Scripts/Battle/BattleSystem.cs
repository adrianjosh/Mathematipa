using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using DG.Tweening;
using Unity.VisualScripting;

public enum BattleState { Start, ActionSelection, MoveSelection, PlayerAnswer, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver }

public enum BattleAction { Move, SwitchFakemon, UseItem, Run }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject monsterBaitSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] public TMP_InputField inputField;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip bossBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;


    public event Action<bool> OnBattleOver;

    public BattleQuestion[] questions;
    public BattleQuestion[] questionsW2;
    public BattleQuestion[] questionsW3;
    public BattleQuestion[] questionsW4;
    public BattleQuestion[] questionsW5;
    private static List<BattleQuestion> unansweredBattleQuestions;

    private BattleQuestion currentBattleQuestion;
    private BattleReadInput battleReadInput;

    public SceneDetails sceneForQuestions;

    [SerializeField] GameObject questionHud;
    [SerializeField] GameObject inputtedAnswerHud;

    [SerializeField] private TextMeshProUGUI tanongText;
    [SerializeField] private TextMeshProUGUI hintText;

    private IncorrectAnswerLogger answerLogger; // Reference to the IncorrectAnswerLogger script

    BattleState state;
    int currentAction;
    int currentMove;
    bool correctAnswer = true;
    bool aboutToUseChoice = true;
    public int worldKO;
    public static float answerLength = 1;
    public static bool playerTurnOrder;
    public static float playerAnswerInterval;
    public static bool isTrainerBattle2 = false;

    FakemonParty playerParty;
    FakemonParty trainerParty;
    Fakemon wildFakemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;


    public void StartBattle(FakemonParty playerParty, Fakemon wildFakemon)
    {
        //unansweredBattleQuestions = questions.ToList<BattleQuestion>();
        

        this.playerParty = playerParty;
        this.wildFakemon = wildFakemon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        AudioManager.i.PlayMusicWithLoopPoints(wildBattleMusic, loopStartPoint: BattleSystemMusicTiming.i.loopStartPointWild, loopEndPoint: BattleSystemMusicTiming.i.loopEndPointWild);
        StartCoroutine(SetupBattle());

    }


    public void StartTrainerBattle(FakemonParty playerParty, FakemonParty trainerParty)
    {
        if (worldKO == 1)
        {
            unansweredBattleQuestions = questions.ToList<BattleQuestion>();
        }
        else if (worldKO == 2)
        {
            unansweredBattleQuestions = questionsW2.ToList<BattleQuestion>();
        }
        else if (worldKO == 3)
        {
            unansweredBattleQuestions = questionsW3.ToList<BattleQuestion>();
        }
        else if (worldKO == 4)
        {
            unansweredBattleQuestions = questionsW4.ToList<BattleQuestion>();
        }
        else if (worldKO == 5)
        {
            unansweredBattleQuestions = questionsW5.ToList<BattleQuestion>();
        }

        answerLogger = GetComponent<IncorrectAnswerLogger>(); // Get reference to IncorrectAnswerLogger script


        Debug.Log(worldKO);
        

        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        isTrainerBattle2 = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        if (trainer.isBoss)
        {
            AudioManager.i.PlayMusicWithLoopPoints(bossBattleMusic, loopStartPoint: BattleSystemMusicTiming.i.loopStartPointBoss, loopEndPoint: BattleSystemMusicTiming.i.loopEndPointBoss);
        }
        else
        {
            AudioManager.i.PlayMusicWithLoopPoints(trainerBattleMusic, loopStartPoint: BattleSystemMusicTiming.i.loopStartPointTrainer, loopEndPoint: BattleSystemMusicTiming.i.loopEndPointTrainer);
        }

        

        StartCoroutine(SetupBattle());
        SetCurrentQuestion();
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            // Wild Monster Battle
            playerUnit.Setup(playerParty.GetHealthyFakemon());
            enemyUnit.Setup(wildFakemon);

            dialogBox.SetMoveNames(playerUnit.Fakemon.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Fakemon.Base.Name} appeared.");
        }
        else
        {
            //Trainer Battle

            //Show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            //send out first fakemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyFakemon = trainerParty.GetHealthyFakemon();
            enemyUnit.Setup(enemyFakemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyFakemon.Base.Name}");

            //send out first fakemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerFakemon = playerParty.GetHealthyFakemon();
            playerUnit.Setup(playerFakemon);
            yield return dialogBox.TypeDialog($"Go {playerFakemon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Fakemon.Moves);
        }


        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }


    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Fakemons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }
    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(enabled);
    }

    IEnumerator AboutToUse(Fakemon newFakemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newFakemon.Base.Name}. Do you want to change monster?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Fakemon fakemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move that you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(fakemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    void PlayerAnswer()
    {
        state = BattleState.PlayerAnswer;
        EnableQuestionHud(true);
        //SetCurrentQuestion();
        Debug.Log($"{currentBattleQuestion.tanong} is {currentBattleQuestion.sagot}");
        BattleTimer.i.StartTimer();
    }
    void SetCurrentQuestion()
    {
        int randomQuestionIndex = UnityEngine.Random.Range(0, unansweredBattleQuestions.Count);
        currentBattleQuestion = unansweredBattleQuestions[randomQuestionIndex];

        tanongText.text = currentBattleQuestion.tanong;

        var shuffled = new string(currentBattleQuestion.sagot.OrderBy(x => Guid.NewGuid()).ToArray());
        var _rndAnsChars = ($"{shuffled}{ShuffleHintCharacters()}");
        var finalRndChars = new string(_rndAnsChars.OrderBy(x => Guid.NewGuid()).ToArray());

        DisplayHintText.instance.DisplayHint($"{finalRndChars.ToUpper()}");
        

        //hintText.text = ($"{finalRndChars.ToUpper()}");

        answerLength = currentBattleQuestion.sagot.Length;
    }

    void TransitionToNextQuestion()
    {

        unansweredBattleQuestions.Remove(currentBattleQuestion);


        if (unansweredBattleQuestions == null || unansweredBattleQuestions.Count == 0)
        {
            Debug.Log("0 alr");
            if (worldKO == 1)
            {
                unansweredBattleQuestions = questions.ToList<BattleQuestion>();
            }
            else if (worldKO == 2)
            {
                unansweredBattleQuestions = questionsW2.ToList<BattleQuestion>();
            }
            else if (worldKO == 3)
            {
                unansweredBattleQuestions = questionsW3.ToList<BattleQuestion>();
            }
            else if (worldKO == 4)
            {
                unansweredBattleQuestions = questionsW4.ToList<BattleQuestion>();
            }
            else if (worldKO == 5)
            {
                unansweredBattleQuestions = questionsW5.ToList<BattleQuestion>();
            }
        }

        SetCurrentQuestion();

    }

    public void InputedBattleAnswer(string _answer)
    {
        if (BattleTimer.i.currentTime <= 25.1)
        {
            
        }
        if (_answer == currentBattleQuestion.sagot)
        {
            Debug.Log("Correct!!!");
            //CheckForCorrectAnswer(true);
            if (answerLogger.IsQuestionLogged(currentBattleQuestion.tanong))
            {
                // Remove the incorrect answer from the log
                answerLogger.RemoveIncorrectAnswer(currentBattleQuestion.tanong);
            }

            correctAnswer = true;
        }
        else
        {
            Debug.Log("ENGENGGGG!!!");
            //CheckForCorrectAnswer(false);
            answerLogger.LogIncorrectAnswer(currentBattleQuestion.tanong, _answer,currentBattleQuestion.sagot);
            correctAnswer = false;

        }

        BattleTimer.i.ResetTimer();

        EnableQuestionHud(false);
        dialogBox.EnableDialogText(true);
        StartCoroutine(AnswerCorrection(correctAnswer));
        StartCoroutine(RunTurns(BattleAction.Move));
    }

    public String ShuffleHintCharacters()
    {
        var rndChars = "abcdefghijklmnopqrstuvwxyz";
        var length = 2;

        var _rndChars = new char[length];

        for (var i = 0; i < length; i++)
        {
            _rndChars[i] = rndChars[UnityEngine.Random.Range(0, rndChars.Length)];
        }

        return new string(_rndChars);
    }

    IEnumerator AnswerCorrection(bool facts)
    {
        state = BattleState.PlayerAnswer;

        if (facts)
        {
            yield return dialogBox.TypeDialog("Correct Answer!");
            
        }
        else
        {
            yield return dialogBox.TypeDialog("Incorrect Answer!");
        }

    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        //hack implementation of enemy accuracy
        bool _correctAnswer = correctAnswer;

        state = BattleState.RunningTurn;
        
 

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Fakemon.CurrentMove = playerUnit.Fakemon.Moves[currentMove];
            enemyUnit.Fakemon.CurrentMove = enemyUnit.Fakemon.GetRandomMove();

            int playerMovePriority = playerUnit.Fakemon.CurrentMove.Base.Priority;
            int enemyMovePriority = playerUnit.Fakemon.CurrentMove.Base.Priority;

            if (isTrainerBattle)
            {
                yield return new WaitForSeconds(1.5f);
            }

            //check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Fakemon.Speed >= enemyUnit.Fakemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondFakemon = secondUnit.Fakemon;

            //hack implementation of enemy accuracy
            if (!playerGoesFirst)
            {
                correctAnswer = true;
                playerTurnOrder = false;
            }
            else
            {
                playerTurnOrder = true;
            }

            // first turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Fakemon.CurrentMove);
            
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }

            if (secondFakemon.HP > 0)
            {
                //hack implementation of enemy accuracy
                if (!playerGoesFirst)
                {
                    correctAnswer = _correctAnswer;
                    playerTurnOrder = true;
                }
                if (playerGoesFirst)
                {
                    correctAnswer = true;
                    playerTurnOrder = false;
                }

                // second turn

                yield return RunMove(secondUnit, firstUnit, secondUnit.Fakemon.CurrentMove);
                
                yield return RunAfterTurn(secondUnit);

                answerLength = 1;

                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchFakemon)
            {
                var selectedFakemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                dialogBox.EnableActionSelector(false);
                yield return SwitchFakemon(selectedFakemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // this is handled from item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
                //yield return ThrowMonsterBait();
            }
            else if (playerAction == BattleAction.Run)
            {
                dialogBox.EnableActionSelector(false);
                yield return TryToEscape();
            }

            // enemy turn

            //hack implementation of enemy accuracy
            correctAnswer = true;
            answerLength = 1;

            var enemyMove = enemyUnit.Fakemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }
        if (state != BattleState.BattleOver)
        {
            if(isTrainerBattle)
            {
                TransitionToNextQuestion();
            }
            
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Fakemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Fakemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Fakemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Fakemon.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Fakemon, targetUnit.Fakemon))
        {
            int vfxIndex = BattleUnit.AttackType(move);
            sourceUnit.SetVFXIndex(vfxIndex);

            sourceUnit.PlayAttackAnimation();
            
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(0.5f);
            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            if (move.Base.Category == MoveCategory.Status)
            {

                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Fakemon, targetUnit.Fakemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Fakemon.TakeDamage(move, sourceUnit.Fakemon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Fakemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Fakemon, targetUnit.Fakemon, secondary.Target);
                    }
                }
            }

            if (targetUnit.Fakemon.HP <= 0)
            {
                yield return HandleFakemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Fakemon.Base.Name}'s attack missed");
        }

        
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Fakemon source, Fakemon target, MoveTarget moveTarget)
    {
        //Stat boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        //Stat condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        //Volatile Stat condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver)
        {
            yield break;
        }
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        //statuses like burn or poison will hurt the fakemon after the turn
        sourceUnit.Fakemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Fakemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Fakemon.HP <= 0)
        {
            yield return HandleFakemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Fakemon source, Fakemon target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }
        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = source.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (correctAnswer)
        {
            if (accuracy > 0)
            {
                moveAccuracy *= boostValues[accuracy];
            }
            else
            {
                moveAccuracy /= boostValues[-accuracy];
            }

            if (evasion > 0)
            {
                moveAccuracy /= boostValues[evasion];
            }
            else
            {
                moveAccuracy *= boostValues[-evasion];
            }
            

            return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
        }
        else
        {
            
            return false;
        }
        


    }

    IEnumerator ShowStatusChanges(Fakemon fakemon)
    {
        while (fakemon.StatusChanges.Count > 0)
        {
            var message = fakemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleFakemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Fakemon.Base.Name} Fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
            {
                battleWon = trainerParty.GetHealthyFakemon() == null;
            }

            if (battleWon)
            {
                AudioManager.i.PlayMusicWithLoopPoints(battleVictoryMusic);
            }

            // EXP Gain
            int expYield = faintedUnit.Fakemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Fakemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Fakemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Fakemon.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            //Check level up
            while (playerUnit.Fakemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                playerUnit.Fakemon.BoostStatsAfterLevelUp();
                yield return dialogBox.TypeDialog($"{playerUnit.Fakemon.Base.Name} grew to level {playerUnit.Fakemon.Level}");

                // try to learn a new move
                var newMove = playerUnit.Fakemon.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Fakemon.Moves.Count < FakemonBase.MaxNumberOfMoves)
                    {
                        playerUnit.Fakemon.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.Fakemon.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Fakemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Fakemon.Base.Name} is trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {FakemonBase.MaxNumberOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.Fakemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }
        

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextFakemon = playerParty.GetHealthyFakemon();
            if (nextFakemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                Debug.Log("talo");
                
                BattleOver(false);
               
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                
                BattleOver(true);
            }
            else
            {
                var nextFakemon = trainerParty.GetHealthyFakemon();
                if (nextFakemon != null)
                {
                    // Send out next fakemon
                    StartCoroutine(AboutToUse(nextFakemon));
                }
                else
                {
                    
                    BattleOver(true);
                }
            }
            
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("Got a critical hit!");
        }
        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog("Powerful attack!");
        }
        else if (damageDetails.TypeEffectiveness < 1)
        {
            yield return dialogBox.TypeDialog("Weak attack!");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelector();
        } else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };
            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
                dialogBox.EnableActionSelector(false);
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
            

        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.PlayerAnswer)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == FakemonBase.MaxNumberOfMoves)
                {
                    // Dont learn a new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Fakemon.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    //forget a move
                    var selectedMove = playerUnit.Fakemon.Moves[moveIndex].Base;

                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Fakemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                    playerUnit.Fakemon.Moves[moveIndex] = new Move(moveToLearn);
                }
                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelector()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction +=2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Inv
                OpenBag();
                // TODO for now: StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 2)
            {
                //monster
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //run
                StartCoroutine(RunTurns(BattleAction.Run));
            }

            currentAction = 0;
        }
        
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Fakemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Fakemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Fakemon.Moves[currentMove];
            if (move.PP == 0)
            {
                return;
            }
            dialogBox.EnableMoveSelector(false);
            //maybe start dito yung question??

            if (isTrainerBattle)
            {
                PlayerAnswer();
            }
            else
            {
                StartCoroutine(RunTurns(BattleAction.Move));
                dialogBox.EnableDialogText(true);
            }
            
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    public void EnableQuestionHud(bool enabled)
    {
        questionHud.SetActive(enabled);
        inputtedAnswerHud.SetActive(enabled);
        inputField.enabled = enabled;

    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a weakened monster");
                return;
            }
            if (selectedMember == playerUnit.Fakemon)
            {
                partyScreen.SetMessageText("You can't switch with the same monster");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchFakemon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchFakemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Fakemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster to continue");
            }
            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerFakemon());
            }
            else
            {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        };
        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                //yes
                OpenPartyScreen();

            }
            else
            {
                //no
                StartCoroutine(SendNextTrainerFakemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerFakemon());
        }
    }

    IEnumerator SwitchFakemon(Fakemon newFakemon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Fakemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Fakemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newFakemon);
        dialogBox.SetMoveNames(newFakemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newFakemon.Base.Name}!");

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerFakemon());
        }
        else
        {
            state = BattleState.RunningTurn;
        }
        
    }

    IEnumerator SendNextTrainerFakemon()
    {
        state = BattleState.Busy;

        var nextFakemon = trainerParty.GetHealthyFakemon();
        enemyUnit.Setup(nextFakemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextFakemon.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is MonsterBaitItem)
        {
            yield return ThrowMonsterBait((MonsterBaitItem)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowMonsterBait(MonsterBaitItem monsterBaitItem)
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't tame the other monster taimers monster!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} threw the {monsterBaitItem.Name.ToUpper()} !");

        var monsterBaitObj = Instantiate(monsterBaitSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var monsterBait = monsterBaitObj.GetComponent<SpriteRenderer>();
        monsterBait.sprite = monsterBaitItem.Icon;

        //Animations
        yield return monsterBait.transform.DOJump(enemyUnit.transform.position + new Vector3(-2, 0), 2f, 1, 1f).WaitForCompletion();
        /*
         Line for capture anim like pokeball
        yield return enemyUnit.PlayCaptureAnimation();
         */
        //yield return monsterBait.transform.DOMoveY(enemyUnit.transform.position.y, 0.5f).WaitForCompletion();
        int shakeCount = TryToCatchFakemon(enemyUnit.Fakemon, monsterBaitItem);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return monsterBait.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }
        yield return monsterBait.DOFade(0, 0.5f).WaitForCompletion();

        if (shakeCount == 4)
        {
            //Fakemon is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Fakemon.Base.Name} was tamed!");
            yield return enemyUnit.PlayCaptureAnimation();

            playerParty.AddFakemon(enemyUnit.Fakemon);
            yield return dialogBox.TypeDialog($"Remember that {enemyUnit.Fakemon.Base.Name} will only be added to your party if you have less than 6 monsters!");

            Destroy(monsterBait);
            BattleOver(true);
        }
        else
        {
            //Fakemon broke out
            yield return new WaitForSeconds(1f);
            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Fakemon.Base.Name} wasn't tamed!");
            }
            else
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Fakemon.Base.Name} was almost tamed!");
            }

            Destroy(monsterBait);
            state = BattleState.RunningTurn;
        }
        //yield return enemyUnit.PlayCaptureAnimation();
        
    }

    int TryToCatchFakemon(Fakemon fakemon, MonsterBaitItem monsterBaitItem)
    {
        float a = (3 * fakemon.MaxHp - 2 * fakemon.HP) * fakemon.Base.CatchRate * monsterBaitItem.CatchRateModifier * ConditionsDB.GetStatusBonus(fakemon.Status) / (3 * fakemon.MaxHp);
        if (a >= 255)
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));
        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }
            ++shakeCount;
        }
        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from monster tamer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Fakemon.Speed;
        int enemySpeed = enemyUnit.Fakemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Escaped succesfully!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Escaped succesfully!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
