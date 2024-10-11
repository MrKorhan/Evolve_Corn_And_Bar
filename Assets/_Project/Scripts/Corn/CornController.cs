using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CornController : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private List<CornStruct> cornsList;

    [Header("LAYER SETTINGS")]
    [SerializeField] private int layerIndex = 0;
    [SerializeField] private float layerChangePositionDuration = 0.25f;
    [SerializeField] private float layerChangeRotationDuration = 0.25f;
    [SerializeField] private float floorTargetLocalYValue = 2.75f;
    [SerializeField] private float floorTargetLocalMoveDuration = 0.15f;

    [Header("CORN PART SETTINGS")]
    [SerializeField] private Vector2 randomForceUpAmount;
    [SerializeField] private Vector2 randomForceForwardAmount;
    [SerializeField] private float torqueAmount = 10f;
    [SerializeField] private Vector3 cornPartLocalScale; //0.3f;
    [SerializeField] private Vector3 punchScaleValue; //0.15f
    [SerializeField] private float punchScaleDuration = 0.25f;
    [SerializeField] private float moveToSliderDuration = 0.75f;

    [Header("INPUT SETTINGS")]
    [SerializeField] private KeyCode keyOne;
    [SerializeField] private int keyOneAmount;
    [SerializeField] private KeyCode keyTwo;
    [SerializeField] private int keyTwoAmount;
    [SerializeField] private KeyCode keyThree;
    [SerializeField] private int keyThreeAmount;


    [Space()]
    [Header("EDITOR")]
    public GameObject[] cubes;  // Yerlestirilecek kupler
    public float radius = 1f;   // Cemberin Yaricapi
    public Vector3 center;      // Merkez noktasi


    private UIManager uiManager;
    private Transform sliderHandleTransform;
    private Camera m_camera;
    private Sequence layerChangeSequence;
    private List<Tween> cornPartTweenList = new List<Tween>();
    private int layerDifferentIndex = -1;
    private WaitForSeconds moveToSliderWait;
    private Tween punchScaleTween = null;

    private void Start()
    {
        DOTween.Init();
        uiManager = GameManager.Instance.uiManager;
        sliderHandleTransform = GameManager.Instance.sliderHandleTransform;
        m_camera = GameManager.Instance.m_camera;
        moveToSliderWait = new WaitForSeconds(moveToSliderDuration);
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyOne))
        {
            for (int i = 0; i < keyOneAmount; i++)
            {
                GetCornPart(i);
            }
        }
        if (Input.GetKeyDown(keyTwo))
        {
            for (int i = 0; i < keyTwoAmount; i++)
            {
                GetCornPart(i);
            }
        }
        if (Input.GetKeyDown(keyThree))
        {
            for (int i = 0; i < keyThreeAmount; i++)
            {
                GetCornPart(i);
            }
        }
    }

    /// <summary>
    /// Siradan corn part al
    /// </summary>
    /// <param name="callIndex"></param>
    private void GetCornPart(int callIndex)
    {
        var _cornPart = cornsList[layerIndex].CornPartStructList[cornsList[layerIndex].CornPartCounter];
        _cornPart.Transform.parent = null;
        _cornPart.Rb.isKinematic = false;
        //_cornPart.Rb.velocity = Vector3.zero;
        _cornPart.Transform.localScale = cornPartLocalScale;
        ApplyForceAndTorque(_cornPart.Rb, _cornPart.Transform);
        cornsList[layerIndex].CornPartCounter++;

        if (layerDifferentIndex != layerIndex)
        {
            layerDifferentIndex = layerIndex;
            Vector3 layerPos = cornsList[layerIndex].FloorsList[0].parent.localPosition;
            layerPos.y -= 0.02f;
            cornsList[layerIndex].FloorsList[0].parent.localPosition = layerPos;
        }

        //Yeni layer'a gec
        if (cornsList[layerIndex].CornPartCounter > cornsList[layerIndex].MaxCornPart - 1)
        {
            cornsList[layerIndex].CornPartCounter = 0;
            layerIndex++;
            layerIndex %= cornsList.Count;
            LayerChange();
        }

        Vector3 _localPos = cornsList[cornsList.Count - 1].CornPartStructList[cornsList[layerIndex].CornPartCounter].LocalPosition;
        Vector3 _localRot = cornsList[cornsList.Count - 1].CornPartStructList[cornsList[layerIndex].CornPartCounter].Rotation;
        StartCoroutine(MoveToSliderDelayCall(_cornPart, _localPos, _localRot, callIndex));

        if (punchScaleTween != null && punchScaleTween.IsPlaying())
        {
            punchScaleTween.Complete();
            punchScaleTween = null;
        }
        punchScaleTween = transform.DOPunchScale(punchScaleValue, punchScaleDuration);
    }


    /// <summary>
    /// Her bir parcacigi delayli bir sekilde cagiriyoruz.
    /// </summary>
    /// <param name="_cornPart"></param>
    /// <param name="_localPosition"></param>
    /// <param name="_localRotation"></param>
    /// <param name="_callIndex"></param>
    /// <returns></returns>
    IEnumerator MoveToSliderDelayCall(CornPartStruct _cornPart, Vector3 _localPosition, Vector3 _localRotation, int _callIndex)
    {
        yield return moveToSliderWait;
        MoveToSlider(
                    _cornPart.Parent,
                    _cornPart.Transform,
                    _cornPart.Rb,
                    _cornPart.Renderer,
                    _localPosition,
                    _localRotation,
                    _callIndex
                    );
    }

    /// <summary>
    /// Layer degisimi
    /// </summary>
    private void LayerChange()
    {
        layerChangeSequence = DOTween.Sequence();

        int stayIndex = GetPreviousIndex(layerIndex, cornsList.Count);
        int previousCounter = 0;

        //cornPartTweenList.ForEach(x => x.Complete());
        //cornPartTweenList.Clear();

        for (int i = 0; i < cornsList.Count; i++)
        {
            if (i == stayIndex)
            {
                Vector3 layerPos = cornsList[stayIndex].FloorsList[0].parent.localPosition;
                layerPos.y = 0;
                cornsList[stayIndex].FloorsList[0].parent.localPosition = layerPos;

                var stayIndexMaterial = cornsList[stayIndex].CornPartStructList[0].Renderer.materials;
                int previousLayerIndex = GetPreviousIndex(stayIndex, cornsList.Count);
                cornsList[previousLayerIndex].CornPartStructList.ForEach(x => x.Renderer.materials = stayIndexMaterial);
                continue;
            }

            int nextIndex = (layerIndex + previousCounter) % cornsList.Count;
            for (int j = 0; j < cornsList[i].CornPartStructList.Count; j++)
            {
                int _j = j;
                cornsList[nextIndex].CornPartStructList[_j].Rb.isKinematic = true;
                var _cornTransform = cornsList[nextIndex].CornPartStructList[_j].Transform;
                _cornTransform.parent = cornsList[nextIndex].CornPartStructList[_j].Parent;
                _cornTransform.localPosition = cornsList[previousCounter].CornPartStructList[_j].LocalPosition;
                _cornTransform.eulerAngles = cornsList[previousCounter].CornPartStructList[_j].Rotation;
                //_cornTransform.DOLocalMove(target, layerChangePositionDuration);
                //layerChangeSequence.Join(_cornTransform.DORotate(cornsList[previousCounter].CornPartStructList[_j].Rotation, layerChangeRotationDuration));
            }

            if (previousCounter == 1)
            {
                cornsList[nextIndex].FloorsList[0].DOLocalMoveY(floorTargetLocalYValue, floorTargetLocalMoveDuration).SetLoops(2, LoopType.Yoyo);
            }
            previousCounter++;
        }
    }


    public int GetPreviousIndex(int index, int count)
    {
        // Modüler aritmetik ile bir önceki indeksi döndür
        return (index - 1 + count) % count;
    }


    /// <summary>
    /// Corn partlari slider'a yonlendir.
    /// </summary>
    /// <param name="_parent"></param>
    /// <param name="_transform"></param>
    /// <param name="_rb"></param>
    /// <param name="_renderer"></param>
    /// <param name="_localPosition"></param>
    /// <param name="_localRotation"></param>
    /// <param name="_callIndex"></param>
    private void MoveToSlider(Transform _parent, Transform _transform, Rigidbody _rb, Renderer _renderer, Vector3 _localPosition, Vector3 _localRotation, int _callIndex)
    {
        DOVirtual.DelayedCall(_callIndex * 0.01f, () =>
        {
            _rb.isKinematic = true;
            Vector3 targetPos = GetSliderHandlePosition(_transform.position);
            Tween _cornPartTween = _transform.DOMove(targetPos, .4f);
            _cornPartTween.OnComplete(() =>
            {
                uiManager.UpdateSlider(1);
                _transform.parent = _parent;
                _transform.localPosition = _localPosition;
                _transform.eulerAngles = _localRotation;
            });
            cornPartTweenList.Add(_cornPartTween);
        });
    }


    private Vector3 GetSliderHandlePosition(Vector3 target)
    {
        Vector3 uiPos = sliderHandleTransform.position;
        uiPos.z = (target - m_camera.transform.position).z;
        Vector3 result = m_camera.ScreenToWorldPoint(uiPos);
        return result;
    }


    /// <summary>
    /// Nesneye kuvvet uygular
    /// </summary>
    /// <param name="rb"></param>
    /// <param name="_transform"></param>
    public void ApplyForceAndTorque(Rigidbody rb, Transform _transform)
    {
        Vector3 forceDirection = (_transform.up * Random.Range(randomForceUpAmount.x, randomForceUpAmount.y)) + (_transform.forward * Random.Range(randomForceForwardAmount.x, randomForceForwardAmount.y));
        rb.AddForce(forceDirection, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f) * torqueAmount,
            Random.Range(-1f, 1f) * torqueAmount,
            Random.Range(-1f, 1f) * torqueAmount
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }

    #region EDITOR
#if UNITY_EDITOR
    /// <summary>
    /// Kupleri daire seklinde yerlestir.
    /// </summary>
    [ContextMenu("Corn Part Placement")]
    private void PlaceCubesInCircle()
    {
        float angleStep = -360f / cubes.Length;

        for (int i = 0; i < cubes.Length; i++)
        {
            float angleInRadians = Mathf.Deg2Rad * (i * angleStep);

            float xPos = center.x + Mathf.Cos(angleInRadians) * radius;
            float zPos = center.z + Mathf.Sin(angleInRadians) * radius;

            cubes[i].transform.position = new Vector3(xPos, cubes[i].transform.position.y, zPos);

            Vector3 directionFromCenter = cubes[i].transform.position - center;
            float angleY = Mathf.Atan2(directionFromCenter.x, directionFromCenter.z) * Mathf.Rad2Deg;

            cubes[i].transform.rotation = Quaternion.Euler(0, angleY, 0);
        }

        EditorUtility.SetDirty(this);

    }


    /// <summary>
    /// Corns listesini ayarla
    /// </summary>
    [ContextMenu("Populate List Variables")]
    private void PopulateCornStructs()
    {
        cornsList.Clear();

        // Layer'lari bul
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform layer = transform.GetChild(i);
            CornStruct newCornStruct = new CornStruct
            {
                LayerStr = layer.name,
                CornPartCounter = 0,
                MaxCornPart = 0,
                FloorsList = new List<Transform>(),
                CornPartStructList = new List<CornPartStruct>()
            };

            // Her Layer altinda bulunan Floors
            for (int j = 0; j < layer.childCount; j++)
            {
                Transform floor = layer.GetChild(j);
                newCornStruct.FloorsList.Add(floor);  // Floors'lari listeye ekliyoruz

                // Her Floor altinda bulunan CornPart'lar
                for (int k = 0; k < floor.childCount; k++)
                {
                    Transform cornPart = floor.GetChild(k);
                    Rigidbody rb = cornPart.GetComponent<Rigidbody>();
                    Renderer renderer = cornPart.GetComponent<Renderer>();

                    // CornPartStruct olustur ve listeye ekle
                    CornPartStruct newCornPartStruct = new CornPartStruct
                    {
                        PartStr = cornPart.name,
                        Parent = cornPart.parent,
                        Transform = cornPart,
                        Rb = rb,
                        Renderer = renderer,
                        LocalPosition = cornPart.localPosition,
                        Rotation = cornPart.eulerAngles
                    };

                    newCornStruct.CornPartStructList.Add(newCornPartStruct);
                    newCornStruct.MaxCornPart++;  // MaxCornPart sayisini artiriyoruz
                }
            }

            cornsList.Add(newCornStruct);  // Her layer için oluþturduðumuz CornStruct'ý listeye ekliyoruz


            EditorUtility.SetDirty(this);

        }
    }
#endif
    #endregion
}
