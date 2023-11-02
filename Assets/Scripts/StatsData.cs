using UnityEngine;

[CreateAssetMenu(fileName = "StatsData", menuName = "Scrobjects/Stats", order = 0)]
public class StatsData : ScriptableObject
{
    public Vector2 startVelocity = new();
    public float velocityCoef = 0.01f;
}
