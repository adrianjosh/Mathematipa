using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialog(dialog, choices: new List<string>() { "Yes", "No" }, onChoiceSelected:
        (choiceIndex) => selectedChoice = choiceIndex );

        if (selectedChoice == 0)
        {
            // Yes
            StartCoroutine(Fader.i.FadeIn(0.5f));
            //yield return Fader.i.FadeIn(0.5f);
            AudioManager.i.PlaySfx(AudioId.FakemonHeal, pauseMusic: true);

            yield return DialogManager.Instance.ShowDialogText($"......................................................................................................", waitForInput: false);
            

            var playerParty = player.GetComponent<FakemonParty>();
            playerParty.Fakemons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            StartCoroutine(Fader.i.FadeOut(0.5f));
            //yield return Fader.i.FadeOut(0.5f);
            yield return DialogManager.Instance.ShowDialogText($"Your monsters should be fully healed now.");
            yield return DialogManager.Instance.ShowDialogText($"You can come back again anytime!");
        }
        else if (selectedChoice == 1)
        {
            // No
            yield return DialogManager.Instance.ShowDialogText($"Okay! Come back if you change your mind.");
        }
    }
}
