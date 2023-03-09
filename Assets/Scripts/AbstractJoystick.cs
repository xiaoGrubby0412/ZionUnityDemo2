using UnityEngine;

public abstract class AbstractJoystick: MonoBehaviour
{
    public abstract float Horizontal { get; }
    public abstract float Vertical { get; }
}
