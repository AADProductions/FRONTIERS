using UnityEngine;

/// Cloud animation class.
///
/// Component of the sky dome parent game object.

public class TOD_Animation : MonoBehaviour
{
    /// Wind direction in degrees.
    /// = 0 for wind blowing in northern direction.
    /// = 90 for wind blowing in eastern direction.
    /// = 180 for wind blowing in southern direction.
    /// = 270 for wind blowing in western direction.
    public float WindDegrees = 0.0f;

    /// Speed of the wind that is acting upon the clouds.
    public float WindSpeed = 3.0f;

    /// Current cloud UV coordinates.
    /// Can be synchronized between multiple game clients to guarantee identical cloud positions.
    internal Vector4 CloudUV
    {
        get; set;
    }

    /// Current offset UV coordinates.
    /// Is being calculated from the sky dome world position.
    internal Vector4 OffsetUV
    {
        get
        {
            Vector3 pos = transform.position;
            Vector3 scale = transform.lossyScale;
            Vector3 offset = new Vector3(pos.x / scale.x, 0, pos.z / scale.z);
            offset = -transform.TransformDirection(offset);
            return new Vector4(offset.x, offset.z, offset.x, offset.z);
        }
    }

    private TOD_Sky sky;

    protected void Start()
    {
        sky = GetComponent<TOD_Sky>();
    }

    protected void Update()
    {
        // Wind direction and speed calculation
        Vector2 v1 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (WindDegrees + 15)),
                                 Mathf.Sin(Mathf.Deg2Rad * (WindDegrees + 15)));
        Vector2 v2 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (WindDegrees - 15)),
                                 Mathf.Sin(Mathf.Deg2Rad * (WindDegrees - 15)));
        Vector4 wind = WindSpeed / 100f * new Vector4(v1.x, v1.y, v2.x, v2.y);

        // Update cloud UV coordinates
		CloudUV += (float) Frontiers.WorldClock.ARTDeltaTime * wind;
        CloudUV = new Vector4(CloudUV.x % sky.Clouds.Scale1.x,
                              CloudUV.y % sky.Clouds.Scale1.y,
                              CloudUV.z % sky.Clouds.Scale2.x,
                              CloudUV.w % sky.Clouds.Scale2.y);
    }
}
