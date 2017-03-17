using UnityEngine;
using Frontiers;

/// Component manager class.
///
/// Component of the main camera of the scene.

[ExecuteInEditMode]
public class TOD_Components : MonoBehaviour
{
    /// Sun child game object reference.
    public GameObject Sun = null;

    /// Moon child game object reference.
    public GameObject Moon = null;

    /// Atmosphere child game object reference.
    public GameObject Atmosphere = null;

    /// Clear child game object reference.
    public GameObject Clear = null;

    /// Clouds child game object reference.
    public GameObject Clouds = null;

    /// Space child game object reference.
    public GameObject Space = null;

    /// Light child game object reference.
    public GameObject Light = null;

    /// Projector child game object reference.
    public GameObject Projector = null;

	/// Transform component of the light source game object.
	public Transform LightTransform;

    /// Transform component of the sky dome game object.
    internal Transform DomeTransform;

    /// Transform component of the sun game object.
    internal Transform SunTransform;

    /// Transform component of the moon game object.
    internal Transform MoonTransform;

    /// Transform component of the main camera game object.
    internal Transform CameraTransform;

    /// Renderer component of the space game object.
    internal Renderer SpaceRenderer;

    /// Renderer component of the atmosphere game object.
    internal Renderer AtmosphereRenderer;

    /// Renderer component of the clear game object.
    internal Renderer ClearRenderer;

    /// Renderer component of the cloud game object.
    internal Renderer CloudRenderer;

    /// Renderer component of the sun game object.
    internal Renderer SunRenderer;

    /// Renderer component of the moon game object.
    internal Renderer MoonRenderer;

    /// MeshFilter component of the space game object.
    internal MeshFilter SpaceMeshFilter;

    /// MeshFilter component of the atmosphere game object.
    internal MeshFilter AtmosphereMeshFilter;

    /// MeshFilter component of the clear game object.
    internal MeshFilter ClearMeshFilter;

    /// MeshFilter component of the cloud game object.
    internal MeshFilter CloudMeshFilter;

    /// MeshFilter component of the sun game object.
    internal MeshFilter SunMeshFilter;

    /// MeshFilter component of the moon game object.
    internal MeshFilter MoonMeshFilter;

    /// Main material of the space game object.
    internal Material SpaceShader;

    /// Main material of the atmosphere game object.
    internal Material AtmosphereShader;

	/// Main material of the fog.
	public Material FogShader;

    /// Main material of the clear game object.
    internal Material ClearShader;

    /// Main material of the cloud game object.
    internal Material CloudShader;

    /// Main material of the sun game object.
    internal Material SunShader;

    /// Main material of the moon game object.
    internal Material MoonShader;

    /// Main material of the projector game object.
    internal Material ShadowShader;

    /// Light component of the light source game object.
    internal Light LightSource;

    /// Projector component of the shadow projector game object.
    internal Projector ShadowProjector;

    /// Sky component of the sky dome game object.
    internal TOD_Sky Sky;

    /// Animation component of the sky dome game object.
    internal TOD_Animation Animation;

    /// Time component of the sky dome game object.
    internal TOD_Time Time;

    /// Weather component of the sky dome game object.
    internal TOD_Weather Weather;

    /// Resource container component of the sky dome game object.
    internal TOD_Resources Resources;

    /// Sun shaft component of the camera game object if available.
    internal TOD_SunShafts SunShafts;

    protected void OnEnable()
    {
		DomeTransform = transform;

        if (Camera.main != null)
        {
            CameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("Main camera does not exist or is not tagged 'MainCamera'.");
        }

        Sky       = GetComponent<TOD_Sky>();
        Animation = GetComponent<TOD_Animation>();
        Time      = GetComponent<TOD_Time>();
        Weather   = GetComponent<TOD_Weather>();
        Resources = GetComponent<TOD_Resources>();

        if (Space)
        {
            SpaceRenderer   = Space.GetComponent<Renderer>();
            SpaceShader     = SpaceRenderer.sharedMaterial;
            SpaceMeshFilter = Space.GetComponent<MeshFilter>();
        }
        else
        {
            Debug.LogError("Space reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

        if (Atmosphere)
        {
            AtmosphereRenderer   = Atmosphere.GetComponent<Renderer>();
            AtmosphereShader     = AtmosphereRenderer.sharedMaterial;
            AtmosphereMeshFilter = Atmosphere.GetComponent<MeshFilter>();
        }
        else
        {
            Debug.LogError("Atmosphere reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

        if (Clear)
        {
            ClearRenderer   = Clear.GetComponent<Renderer>();
            ClearShader     = ClearRenderer.sharedMaterial;
            ClearMeshFilter = Clear.GetComponent<MeshFilter>();
        }
        else
        {
            Debug.LogError("Clear reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

        if (Clouds)
        {
            CloudRenderer   = Clouds.GetComponent<Renderer>();
            CloudShader     = CloudRenderer.sharedMaterial;
            CloudMeshFilter = Clouds.GetComponent<MeshFilter>();
        }
        else
        {
            Debug.LogError("Clouds reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

        if (Projector)
        {
            ShadowProjector = Projector.GetComponent<Projector>();
            ShadowShader    = ShadowProjector.material;
        }
        else
        {
            Debug.LogError("Projector reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

        if (Light)
        {
			//LightTransform = gameObject.FindOrCreateChild ("LightTransform");//Light.transform;
			LightSource    = Light.GetComponent<Light>();
        }
        else
        {
            Debug.LogError("Light reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

        if (Sun)
        {
            SunTransform  = Sun.transform;
            SunRenderer   = Sun.GetComponent<Renderer>();
            SunShader     = SunRenderer.sharedMaterial;
            SunMeshFilter = Sun.GetComponent<MeshFilter>();
        }
        else
        {
            Debug.LogError("Sun reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

        if (Moon)
        {
            MoonTransform  = Moon.transform;
            MoonRenderer   = Moon.GetComponent<Renderer>();
            MoonShader     = MoonRenderer.sharedMaterial;
            MoonMeshFilter = Moon.GetComponent<MeshFilter>();
        }
        else
        {
            Debug.LogError("Moon reference not set. Disabling TOD_Sky script.");
            Sky.enabled = false;
            return;
        }

		if (Application.isPlaying) {
			DontDestroyOnLoad (transform);
			DontDestroyOnLoad (LightTransform);
			DontDestroyOnLoad (LightSource);
		}
    }
}
