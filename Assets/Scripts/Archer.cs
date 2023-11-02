using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class Archer : MonoBehaviour
{
    private UnityAction pointerDown;
    private UnityAction pointerMove;
    private UnityAction pointerUp;
    private Vector2 tapPosition = new();
    private bool onRotation = false;

    [SerializeField]
    private SkeletonUtilityBone bow;
    [SerializeField]
    private GameObject tap;
    [SerializeField]
    private Transform tapPoint;
    [SerializeField]
    private RectTransform tension;
    [SerializeField]
    private SkeletonAnimation archer;
    [SerializeField] [Range(-180f, 180f)]
    private float minAngle, maxAngle;
    [SerializeField]
    private float resetRotationSpeed = 20f;
    [SerializeField]
    private StatsData stats;
    [SerializeField]
    private GameObject arrow;
    [SerializeField]
    private Transform arrowSpawn;
    [SerializeField]
    private float minTension = 100f;
    [SerializeField]
    private float maxTension = 400f;
    [SerializeField]
    private Transform path;

    void OnEnable()
    {
        pointerDown += PointerDown;
    }

    private void OnDisable()
    {
        pointerDown -= PointerDown;
        pointerMove -= PointerMove;
        pointerUp -= PointerUp;
    }

    private void Start()
    {
        for (int i = 0; i < path.childCount; i++)
        {
            path.GetChild(i).localScale = Vector3.one * Mathf.Pow(0.9f, i);
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (pointerDown != null)
            {
                pointerDown.Invoke();
            }
        }
        if (Input.GetButton("Fire1"))
        {
            if (pointerMove != null)
            {
                pointerMove.Invoke();
            }
        }
        if (Input.GetButtonUp("Fire1"))
        {
            if (pointerUp != null)
            {
                pointerUp.Invoke();
            }
        }
        if (onRotation)
        {
            float currentAngle = bow.transform.eulerAngles.z;
            if (Mathf.Abs(currentAngle) > resetRotationSpeed * Time.deltaTime * 2)
            {
                bow.transform.rotation = Quaternion.RotateTowards(
                    bow.transform.rotation,
                    Quaternion.Euler(0f, 0f, 0f),
                    resetRotationSpeed * Time.deltaTime
                    );
            }
            else
            {
                bow.transform.eulerAngles = Vector3.zero;
                onRotation = false;
            }
        }
    }

    private void PointerDown()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new(mousePos.x, mousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider == null)
        {
            return;
        }
        if (hit.collider.CompareTag("Player"))
        {
            archer.AnimationState.SetAnimation(0, "attack_start", false);
            archer.AnimationState.AddAnimation(0, "attack_target", false, 0f);
            tap.transform.position = Camera.main.WorldToScreenPoint(tapPoint.position);
            tapPosition = new(tap.transform.position.x, tap.transform.position.y);
            tension.sizeDelta = new(5f, 5f);
            tension.eulerAngles = Vector3.zero;
            tap.SetActive(true);
            path.gameObject.SetActive(true);
            pointerDown -= PointerDown;
            pointerMove += PointerMove;
            pointerUp += PointerUp;
        }
    }

    private void PointerMove()
    {
        Vector2 pointerPosition = Input.mousePosition;
        Vector2 aimVector = tapPosition - pointerPosition;
        float tensionValue = Mathf.Clamp(aimVector.magnitude * 1920 / Screen.width, minTension, maxTension);
        tension.sizeDelta = new Vector2(tensionValue, 5f);
        Vector2 aimVectorNorm = aimVector.normalized;
        float aimAngle = Mathf.Clamp(Mathf.Rad2Deg * (Mathf.Asin(aimVectorNorm.y) > 0 ? 1 : -1) * Mathf.Acos(aimVectorNorm.x), minAngle, maxAngle);
        tension.eulerAngles = new(0f, 0f, aimAngle);
        bow.transform.eulerAngles = new(0f, 0f, aimAngle);
        stats.startVelocity = new Vector2(Mathf.Cos(aimAngle * Mathf.Deg2Rad), Mathf.Sin(aimAngle * Mathf.Deg2Rad)) * tensionValue;
        
        for (int i = 0; i < path.childCount; i++)
        {
            path.GetChild(i).localPosition = new Vector3(
                i * tensionValue / maxTension * 0.8f + 1,
                0f,
                0f
                );
            path.GetChild(i).position += new Vector3(
                0f,
                Mathf.Sqrt(path.childCount - i) - Mathf.Sqrt(path.childCount),
                0f
                );
        }
    }

    private void PointerUp()
    {
        archer.AnimationState.AddAnimation(0, "attack_finish", false, 0f);
        archer.AnimationState.AddAnimation(0, "idle", true, 0f);
        archer.AnimationState.Complete += ResetRotate;
        archer.AnimationState.Event += OnShoot;
        pointerDown += PointerDown;
        pointerMove -= PointerMove;
        pointerUp -= PointerUp;
        tap.SetActive(false);
        path.gameObject.SetActive(false);
    }

    private void ResetRotate(TrackEntry trackEntry)
    {
        archer.AnimationState.Complete -= ResetRotate;
        onRotation = true;
    }

    private void OnShoot(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "shoot")
        {
            archer.AnimationState.Event -= OnShoot;
            Instantiate(arrow, arrowSpawn.position, arrowSpawn.rotation);
        }
    }
}
