using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Xml.Linq;

public class BattleUnit : MonoBehaviour
{

    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public List<GameObject> vfxPrefabs;
    public List<GameObject> vfxDashPrefabs;
    private List<GameObject> vfxObjects;
    private List<GameObject> vfxDashObjects;
    private int vfxIndex = 0;
    private bool[] vfxInstantiated;
    private bool[] vfxDashInstantiated;
    [SerializeField] Vector3 vfxRotation;
    [SerializeField] Vector3 vfxDashRotation;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud
    {
        get { return hud; }
    }
    public Fakemon Fakemon { get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    public void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }
    private void Start()
    {
        vfxInstantiated = new bool[vfxPrefabs.Count];

        // Initialize vfxObjects list
        vfxObjects = new List<GameObject>();
        for (int i = 0; i < vfxPrefabs.Count; i++)
        {
            vfxObjects.Add(null);
        }

        vfxDashInstantiated = new bool[vfxDashPrefabs.Count];

        // Initialize vfxObjects list
        vfxDashObjects = new List<GameObject>();
        for (int i = 0; i < vfxDashPrefabs.Count; i++)
        {
            vfxDashObjects.Add(null);
        }
    }

    public void Setup(Fakemon fakemon)
    {
        Fakemon = fakemon;
        if (isPlayerUnit)
        {
            image.sprite = Fakemon.Base.BackSprite;
        } else
        {
            image.sprite = Fakemon.Base.FrontSprite;
        }

        hud.gameObject.SetActive(true);
        hud.SetData(fakemon);

        transform.localScale = new Vector3(1f, 1f, 1f);
        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-1050f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(1050f, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f);

    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        var currentPos = new Vector3(0f,0f,0f);

        sequence.AppendCallback(() => ToggleDashVFX(vfxIndex));

        if (isPlayerUnit)
        {
            //sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
            
            sequence.Join(image.transform.DOLocalMove(new Vector3(300f, 100f, 0f), 0.35f, snapping: true));
            
            currentPos = new Vector3(300f, 100f, 0f);
        }
        else
        {
            //sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
            sequence.Join(image.transform.DOLocalMove(new Vector3(-250f, -300f, 0f), 0.35f, snapping: true));
            currentPos = new Vector3(-250f, -300f, 0f);
        }

        

        sequence.AppendCallback(() => ToggleVFX(vfxIndex));
        if (isPlayerUnit)
        {
            sequence.Join(image.transform.DOLocalMoveX(currentPos.x + 50f, 0.25f, snapping: true));
        }
        else
        {
            sequence.Join(image.transform.DOLocalMoveX(currentPos.x - 50f, 0.25f, snapping: true));
        }
        sequence.Append(image.transform.DOLocalMove(currentPos, 0.25f));

        sequence.AppendInterval(0.7f);
        sequence.AppendCallback(() => DisableVFX());
        sequence.AppendCallback(() => PlayMoveToOriginalPosition());
        sequence.AppendCallback(() => DisableDashVFX());
        //sequence.Append(image.transform.DOLocalMove(originalPos, 0.25f));
    }

    public void PlayMoveToOriginalPosition()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMove(originalPos, 0.25f));

    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.red, 0.1f));
        if (isPlayerUnit)
        {
            sequence.Join(image.transform.DOLocalMoveX(originalPos.x - 200f, 0.1f));
        }
        else
        {
            sequence.Join(image.transform.DOLocalMoveX(originalPos.x + 200f, 0.1f));
        }
        sequence.Append(image.transform.DOLocalMove(originalPos, 0.25f));
        sequence.Join(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    void ToggleVFX(int vfxIndex)
    {
        if (vfxObjects == null || vfxInstantiated == null || vfxObjects.Count != vfxPrefabs.Count || vfxInstantiated.Length != vfxPrefabs.Count)
        {
            Debug.LogError("VFX arrays not properly initialized.");
            return;
        }
        // If the VFX hasn't been instantiated yet, instantiate it
        if (!vfxInstantiated[vfxIndex])
        {
            vfxObjects[vfxIndex] = Instantiate(vfxPrefabs[vfxIndex], originalPos, Quaternion.identity, transform);
            vfxObjects[vfxIndex].transform.SetLocalPositionAndRotation(new Vector3(1f, 1f, 1f), Quaternion.Euler(vfxRotation));
            vfxInstantiated[vfxIndex] = true; // Set the flag to indicate that the VFX has been instantiated
        }
        else
        {
            vfxObjects[vfxIndex].SetActive(true);
        }
    }

    void ToggleDashVFX(int vfxIndex)
    {
        if (vfxDashObjects == null || vfxDashInstantiated == null || vfxDashObjects.Count != vfxDashPrefabs.Count || vfxDashInstantiated.Length != vfxDashPrefabs.Count)
        {
            Debug.LogError("VFX arrays not properly initialized.");
            return;
        }
        // If the VFX hasn't been instantiated yet, instantiate it
        if (!vfxDashInstantiated[vfxIndex])
        {
            vfxDashObjects[vfxIndex] = Instantiate(vfxDashPrefabs[vfxIndex], originalPos, Quaternion.identity, transform);
            if (isPlayerUnit)
            {
                vfxDashObjects[vfxIndex].transform.SetLocalPositionAndRotation(new Vector3(1f, 1f, -1f), Quaternion.Euler(vfxDashRotation));
            }
            else
            {
                vfxDashObjects[vfxIndex].transform.SetLocalPositionAndRotation(new Vector3(-1f, -1f, 1f), Quaternion.Euler(vfxDashRotation));
            }
            
            vfxDashInstantiated[vfxIndex] = true; // Set the flag to indicate that the VFX has been instantiated
        }
        else
        {
            vfxDashObjects[vfxIndex].SetActive(true);
        }
    }

    void DisableVFX()
    {
        for (int i = 0; i < vfxObjects.Count; i++)
        {
            if (vfxObjects[i] != null && vfxObjects[i].activeSelf)
            {
                vfxObjects[i].SetActive(false);
            }
        }
    }

    void DisableDashVFX()
    {
        for (int i = 0; i < vfxDashObjects.Count; i++)
        {
            if (vfxDashObjects[i] != null && vfxDashObjects[i].activeSelf)
            {
                vfxDashObjects[i].SetActive(false);
            }
        }
    }

    public static int AttackType(Move attackType)
    {
        if (attackType.Base.Type.ToString() == FakemonType.Normal.ToString())
        {
            return 0;
        }
        else if (attackType.Base.Type.ToString() == FakemonType.Fire.ToString())
        {
            return 1;
        }
        else if (attackType.Base.Type.ToString() == FakemonType.Water.ToString())
        {
            return 2;
        }
        else if (attackType.Base.Type.ToString() == FakemonType.Electric.ToString())
        {
            return 3;
        }
        else if (attackType.Base.Type.ToString() == FakemonType.Grass.ToString())
        {
            return 4;
        }
        else if (attackType.Base.Type.ToString() == FakemonType.Ice.ToString())
        {
            return 5;
        }
        else if (attackType.Base.Type.ToString() == FakemonType.Poison.ToString())
        {
            return 6;
        }
        else
        {
            return 0;
        }

    }

    public void SetVFXIndex(int index)
    {
        vfxIndex = index;
    }


}
