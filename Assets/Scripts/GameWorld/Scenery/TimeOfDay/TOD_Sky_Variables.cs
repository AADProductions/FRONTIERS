using UnityEngine;

public partial class TOD_Sky : MonoBehaviour
{
    /// Available methods to detect the Unity color space.
    public enum ColorSpaceDetection
    {
        Auto,
        Linear,
        Gamma
    }

    /// Available methods to render the clouds.
    public enum CloudQualityType
    {
        Fastest,
        Density,
        Bumped
    }

    /// Available vertex count levels for the meshes.
    public enum MeshQualityType
    {
        Low,
        Medium,
        High
    }

    private const float pi = Mathf.PI;
    private const float pi2 = pi*pi;
    private const float pi3 = pi*pi2;
    private const float pi4 = pi2*pi2;

    private Vector2 opticalDepth;
    private Vector3 oneOverBeta;
    private Vector3 betaRayleigh;
    private Vector3 betaRayleighTheta;
    private Vector3 betaMie;
    private Vector3 betaMieTheta;
    private Vector2 betaMiePhase;
    private Vector3 betaNight;

    /// Inspector variable to adjust the color space.
    /// Should stay at ColorSpaceDetection.Auto in most cases.
    public ColorSpaceDetection UnityColorSpace = ColorSpaceDetection.Auto;

    /// Inspector variable to adjust the cloud quality.
    public CloudQualityType CloudQuality = CloudQualityType.Bumped;

    /// Inspector variable to adjust the mesh quality.
    public MeshQualityType MeshQuality = MeshQualityType.High;

    /// Inspector variable containing parameters of the day and night cycle.
    public TOD_CycleParameters Cycle;

    /// Inspector variable containing parameters of the atmosphere.
    public TOD_AtmosphereParameters Atmosphere;

    /// Inspector variable containing parameters of the day.
    public TOD_DayParameters Day;

    /// Inspector variable containing parameters of the night.
    public TOD_NightParameters Night;

    /// Inspector variable containing parameters of the light source.
    public TOD_LightParameters Light;

    /// Inspector variable containing parameters of the stars.
    public TOD_StarParameters Stars;

    /// Inspector variable containing parameters of the cloud layers.
    public TOD_CloudParameters Clouds;

    /// Inspector variable containing parameters of the world.
    public TOD_WorldParameters World;

    /// Containins references to all components.
    internal TOD_Components Components
    {
        get; private set;
    }

    /// Boolean to check if it is day.
    internal bool IsDay
    {
        get { return LerpValue > 0; }
    }

    /// Boolean to check if it is night.
    internal bool IsNight
    {
        get { return LerpValue == 0; }
    }

    /// Radius of the sky dome.
    internal float Radius
    {
        get { return Components.DomeTransform.localScale.x; }
    }

    /// Gamma value that is being used in the shaders.
    internal float Gamma
    {
        get { return (UnityColorSpace == ColorSpaceDetection.Auto && QualitySettings.activeColorSpace == ColorSpace.Linear || UnityColorSpace == ColorSpaceDetection.Linear) ? 1.0f : 2.2f; }
    }

	/// Inverse of the gamma value (1 / Gamma) that is being used in the shaders.
    internal float OneOverGamma
    {
        get { return (UnityColorSpace == ColorSpaceDetection.Auto && QualitySettings.activeColorSpace == ColorSpace.Linear || UnityColorSpace == ColorSpaceDetection.Linear) ? 1.0f/1.0f : 1.0f/2.2f; }
    }

    /// Falls off the darker the sunlight gets.
    /// Can for example be used to lerp between day and night values in shaders.
    /// \n = +1 at day
    /// \n = 0 at night
    internal float LerpValue
    {
        get; private set;
    }

    /// Sun zenith angle in degrees.
    /// \n = 0   if the sun is exactly at zenith.
    /// \n = 180 if the sun is exactly below the ground.
    internal float SunZenith
    {
        get; private set;
    }

    /// Moon zenith angle in degrees.
    /// \n = 0   if the moon is exactly at zenith.
    /// \n = 180 if the moon is exactly below the ground.
    internal float MoonZenith
    {
        get; private set;
    }

    /// Currently active light source zenith angle in degrees.
    /// \n = 0  if the currently active light source (sun or moon) is exactly at zenith.
    /// \n = 90 if the currently active light source (sun or moon) is exactly at the horizon.
    internal float LightZenith
    {
        get { return Mathf.Min(SunZenith, MoonZenith); }
    }

    /// Current light intensity.
    /// Returns the intensity of TOD_Sky.LightSource.
    internal float LightIntensity
    {
        get { return Components.LightSource.intensity; }
    }

    /// Moon direction vector in world space.
    /// Returns the forward vector of TOD_Sky.MoonTransform.
    internal Vector3 MoonDirection
    {
        get { return Components.MoonTransform.forward; }
    }

    /// Sun direction vector in world space.
    /// Returns the forward vector of TOD_Sky.SunTransform.
    internal Vector3 SunDirection
    {
        get { return Components.SunTransform.forward; }
    }

    /// Current directional light vector in world space.
    /// Lerps between TOD_Sky.SunDirection and TOD_Sky.MoonDirection at dusk and dawn.
    internal Vector3 LightDirection
    {
        get { return Vector3.Lerp(MoonDirection, SunDirection, LerpValue*LerpValue); }
    }

    /// Current light color.
    internal Color LightColor
    {
        get { return Components.LightSource.color; }
    }

    /// Current sun shaft color.
    internal Color SunShaftColor
    {
        get; private set;
    }

    /// Current sun color.
    internal Color SunColor
    {
        get; private set;
    }

    /// Current moon color.
    internal Color MoonColor
    {
        get; private set;
    }

    /// Current moon halo color.
    internal Color MoonHaloColor
    {
        get; private set;
    }

    /// Current cloud color.
    internal Color CloudColor
    {
        get; private set;
    }

    /// Current additive color.
    internal Color AdditiveColor
    {
        get; private set;
    }

    /// Current ambient color.
    internal Color AmbientColor
    {
        get; private set;
    }

    /// The fog color sampled from the physical model.
    /// Depends on camera view direction.
    /// This property is O(1) if TOD_WorldParameters.SetFogColor is enabled as its value will be calculated anyhow.
    internal Color FogColor
    {
        get { return World.SetFogColor ? RenderSettings.fogColor : SampleFogColor(); }
    }
}
