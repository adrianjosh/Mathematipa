using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomEndgame : MonoBehaviour
{
    [SerializeField] Transform teleportDestination;

    public IEnumerator TeleportToBeginning(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialog(dialog, choices: new List<string>() { "Yes", "No" }, onChoiceSelected:
        (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // Yes
            yield return Fader.i.FadeIn(0.5f);
            AudioManager.i.PlaySfx(AudioId.EnterDoor);
            player.position = teleportDestination.position;
            yield return Fader.i.FadeOut(0.5f);
            SavingSystem.i.Save("saveSlot1");
        }
        else if (selectedChoice == 1)
        {
            // No
            yield return DialogManager.Instance.ShowDialogText($"Alright! I'll be here waiting for you if you decide to go back.");
        }
    }
}
