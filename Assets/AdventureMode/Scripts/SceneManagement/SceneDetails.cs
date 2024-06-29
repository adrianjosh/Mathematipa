using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI sceneNameText;
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    [SerializeField] public float loopStartPoint = 0f;
    [SerializeField] public float loopEndPoint = 0f;
    public bool IsLoaded { get; private set; }

    [SerializeField] BattleSystem battleSys;

    [SerializeField] public int worldNum;
    [SerializeField] string sceneName;

    List<SavableEntity> savableEntities;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");

            if (sceneNameText != null)
            {
                sceneNameText.text = sceneName;
            }

            Debug.Log(worldNum);
            battleSys.worldKO = worldNum;

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if (sceneMusic != null)
            {
                //AudioManager.i.PlayMusic(sceneMusic, fade: true);
                AudioManager.i.PlayMusicWithLoopPoints(sceneMusic, fade: true, loopStartPoint: loopStartPoint, loopEndPoint: loopEndPoint);
            }

            // Load Scene
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //unload the scenes that are no longer connected
            var previScene = GameController.Instance.PreviScene;

            if (GameController.Instance.PreviScene != null)
            {
                var previoslyLoadedScenes = GameController.Instance.PreviScene.connectedScenes;
                foreach (var scene in previoslyLoadedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                    }

                    if (!connectedScenes.Contains(previScene))
                    {
                        previScene.UnloadScene();
                    }
                }
            }
            
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };

        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
