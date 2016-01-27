using UnityEngine;

/// Material and mesh wrapper class.
///
/// Component of the sky dome parent game object.

public class TOD_Resources : MonoBehaviour
{
    public Mesh Quad;

    public Mesh SphereHigh;
    public Mesh SphereMedium;
    public Mesh SphereLow;

    public Mesh IcosphereHigh;
    public Mesh IcosphereMedium;
    public Mesh IcosphereLow;

    public Mesh HalfIcosphereHigh;
    public Mesh HalfIcosphereMedium;
    public Mesh HalfIcosphereLow;

    public Material CloudMaterialBumped;
    public Material CloudMaterialDensity;
    public Material CloudMaterialFastest;

    public Material ShadowMaterialBumped;
    public Material ShadowMaterialDensity;
    public Material ShadowMaterialFastest;

    public Material SpaceMaterial;
    public Material AtmosphereMaterial;
    public Material SunMaterial;
    public Material MoonMaterial;
    public Material ClearMaterial;
}
