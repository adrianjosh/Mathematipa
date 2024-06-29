using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;


    [SerializeField] QuestBase questToStartTrainer;
    [SerializeField] QuestBase questToCompleteTrainer;

    [SerializeField] public float loopStartPoint = 0f;
    [SerializeField] public float loopEndPoint = 0f;

    Quest activeQuestTrainer;


    [SerializeField] AudioClip trainerAppearsClip;


    //state
    bool battleLost = false;
    
    public bool isBoss = false;

    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovDirection(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            AudioManager.i.PlayMusicWithLoopPoints(trainerAppearsClip, loopStartPoint: loopStartPoint, loopEndPoint: loopEndPoint);

            yield return DialogManager.Instance.ShowDialog(dialog);

            GameController.Instance.StartTrainerBattle(this);

            //Activate quest
            if (questToCompleteTrainer != null)
            {
                var quest = new Quest(questToCompleteTrainer);
                yield return quest.CompleteQuest(initiator);
                questToCompleteTrainer = null;

                Debug.Log($"{quest.Base.Name} completed");
            }

            if (questToStartTrainer != null)
            {
                activeQuestTrainer = new Quest(questToStartTrainer);
                yield return activeQuestTrainer.StartQuest();
                questToStartTrainer = null;

                if (activeQuestTrainer.CanBeCompleted())
                {
                    yield return activeQuestTrainer.CompleteQuest(initiator);
                    activeQuestTrainer = null;
                }

            }
            else if (activeQuestTrainer != null)
            {
                if (activeQuestTrainer.CanBeCompleted())
                {
                    yield return activeQuestTrainer.CompleteQuest(initiator);
                    activeQuestTrainer = null;
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialog(activeQuestTrainer.Base.InProgressDialogue);
                }
            }
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
            
        }
        
    }

    public IEnumerator triggerTrainerBattle(PlayerController player, Transform initiator=null)
    {
        AudioManager.i.PlayMusicWithLoopPoints(trainerAppearsClip, loopStartPoint: loopStartPoint, loopEndPoint: loopEndPoint);

        //show exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //walk towards the player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        // show dialog
        yield return DialogManager.Instance.ShowDialog(dialog);
        GameController.Instance.StartTrainerBattle(this);

        //Activate quest
        if (questToCompleteTrainer != null)
        {
            var quest = new Quest(questToCompleteTrainer);
            yield return quest.CompleteQuest(initiator);
            questToCompleteTrainer = null;

            Debug.Log($"{quest.Base.Name} completed");
        }

        if (questToStartTrainer != null)
        {
            activeQuestTrainer = new Quest(questToStartTrainer);
            yield return activeQuestTrainer.StartQuest();
            questToStartTrainer = null;

            if (activeQuestTrainer.CanBeCompleted())
            {
                yield return activeQuestTrainer.CompleteQuest(initiator);
                activeQuestTrainer = null;
            }

        }
        else if (activeQuestTrainer != null)
        {
            if (activeQuestTrainer.CanBeCompleted())
            {
                yield return activeQuestTrainer.CompleteQuest(initiator);
                activeQuestTrainer = null;
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(activeQuestTrainer.Base.InProgressDialogue);
            }
        }
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public void SetFovDirection(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if (dir == FacingDirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        //return battleLost;

        var saveData = new NPCQuestSaveDataTrainer();
        saveData.activeQuestTrainer = activeQuestTrainer?.GetSaveData();
        saveData.battleLost = battleLost;
        if (questToStartTrainer != null)
        {
            saveData.questToStartTrainer = (new Quest(questToStartTrainer)).GetSaveData();
        }
        if (questToCompleteTrainer != null)
        {
            saveData.questToCompleteTrainer = (new Quest(questToCompleteTrainer)).GetSaveData();
        }

        return saveData;

    }

    public void RestoreState(object state)
    {
        

        var saveData = state as NPCQuestSaveDataTrainer;

        battleLost = saveData.battleLost;

        if (saveData != null)
        {
            activeQuestTrainer = (saveData.activeQuestTrainer != null) ? new Quest(saveData.activeQuestTrainer) : null;

            questToStartTrainer = (saveData.questToStartTrainer != null) ? new Quest(saveData.questToStartTrainer).Base : null;
            questToCompleteTrainer = (saveData.questToCompleteTrainer != null) ? new Quest(saveData.questToCompleteTrainer).Base : null;
        }

        if (battleLost)
        {
            fov.gameObject.SetActive(false);
        }

    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}

[System.Serializable]
public class NPCQuestSaveDataTrainer
{
    public QuestSaveData activeQuestTrainer;
    public QuestSaveData questToStartTrainer;
    public QuestSaveData questToCompleteTrainer;

    public bool battleLost;
}

