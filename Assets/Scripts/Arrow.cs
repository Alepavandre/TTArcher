using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField]
    private StatsData stats;
    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = stats.startVelocity * stats.velocityCoef;
    }

    private void Update()
    {
        Vector2 rbNorm = rb.velocity.normalized;
        transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * (Mathf.Asin(-rbNorm.y) > 0 ? 1 : -1) * Mathf.Acos(rbNorm.x), Vector3.back);
    }
}
