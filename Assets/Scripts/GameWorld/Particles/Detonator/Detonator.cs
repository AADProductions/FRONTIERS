using UnityEngine;
using System.Collections;

/*

	Detonator - A parametric explosion system for Unity
	Created by Ben Throop in August 2009 for the Unity Summer of Code
	
	Simplest use case:
	
	1) Use a prefab
	
	OR
	
	1) Attach a Detonator to a GameObject, either through code or the Unity UI
	2) Either set the Detonator's ExplodeOnStart = true or call Explode() yourself when the time is right
	3) View explosion :)
	
	Medium Complexity Use Case:
	
	1) Attach a Detonator as above 
	2) Change parameters, add your own materials, etc
	3) Explode()
	4) View Explosion
	
	Super Fun Use Case:
	
	1) Attach a Detonator as above
	2) Drag one or more DetonatorComponents to that same GameObject
	3) Tweak those parameters
	4) Explode()
	5) View Explosion
	
	Better documentation is included as a PDF with this package, or is available online. Check the Unity site for a link
	or visit my site, listed below.
	
	Ben Throop
	http://variancetheory.com
	@benimaru

*/

/*
	All pieces of Detonator inherit from this. 
*/
public abstract class DetonatorComponent : MonoBehaviour
{
	public bool on = true;
	public bool detonatorControlled = true;
	
	 [HideInInspector] 
	public  float startSize = 1f;
    public  float size = 1f;
	
    public  float explodeDelayMin = 0f;
    public  float explodeDelayMax = 0f;

	[HideInInspector]  
	public  float startDuration = 2f;
	public  float duration = 2f;
		
	[HideInInspector]
	public  float timeScale = 1f;
	
	 [HideInInspector] 
	public  float startDetail = 1f;
	public  float detail = 1f;
	
	 [HideInInspector] 
	public  Color startColor = Color.white;
	public  Color color = Color.white;
	
	[HideInInspector]
	public  Vector3 startLocalPosition = Vector3.zero;
	public  Vector3 localPosition = Vector3.zero;
	
	[HideInInspector]
	public  Vector3 startForce = Vector3.zero;
	public  Vector3 force = Vector3.zero;
	
	[HideInInspector]
	public  Vector3 startVelocity = Vector3.zero;
	public  Vector3 velocity = Vector3.zero;
	
    public abstract void Explode();
	
	//The main Detonator calls this instead of using Awake() or Start() on subcomponents
	//which ensures it happens when we want.
	public abstract void Init();
	
	public  float detailThreshold;

	/*
		This exists because Detonator makes relative changes
		to set values once the game is running, so we need to store their beginning
		values somewhere to calculate against. An improved design could probably
		avoid this.
	*/
	public void SetStartValues()
	{
		startSize = size;
		startForce = force;
		startVelocity = velocity;
		startDuration = duration;
		startDetail = detail;
		startColor = color;
		startLocalPosition = localPosition;
	}
	
	//implement functions to find the Detonator on this GO and get materials if they are defined
	public Detonator MyDetonator()
	{
		Detonator _myDetonator = GetComponent("Detonator") as Detonator;
		return _myDetonator;
	}
}

[AddComponentMenu("Detonator/Detonator")]
public class Detonator : MonoBehaviour {

	private static float _baseSize = 30f;
	private static Color _baseColor = new Color(1f, .423f, 0f, .5f);
	private static float _baseDuration = 3f;
	
	/*
		_baseSize reflects the size that DetonatorComponents at size 1 match. Yes, this is really big (30m)
		size below is the default Detonator size, which is more reasonable for typical useage. 
		It wasn't my intention for them to be different, and I may change that, but for now, that's how it is.
	*/
	public float size = 10f; 
	public Color color = Detonator._baseColor;
	public bool explodeOnStart = true;
	public float duration = Detonator._baseDuration;
	public float detail = 1f; 
	public float upwardsBias = 0f;
	
	public float destroyTime = 7f; //sorry this is not auto calculated... yet.
	public bool useWorldSpace = true;
	public Vector3 direction = Vector3.zero;
	
	public Material fireballAMaterial;
	public Material fireballBMaterial;
	public Material smokeAMaterial;
	public Material smokeBMaterial;
	public Material shockwaveMaterial;
	public Material sparksMaterial;
	public Material glowMaterial;
	public Material heatwaveMaterial;
		
    private Component[] components;

	private DetonatorFireball _fireball;
	private DetonatorSparks _sparks;
	private DetonatorShockwave _shockwave;
	private DetonatorSmoke _smoke;
	private DetonatorGlow _glow;
	private DetonatorLight _light;
	private DetonatorForce _force;
	private DetonatorHeatwave _heatwave;
	public bool autoCreateFireball = true;
	public bool autoCreateSparks = true;
	public bool autoCreateShockwave = true;
	public bool autoCreateSmoke = true;
	public bool autoCreateGlow = true;
	public bool autoCreateLight = true;
	public bool autoCreateForce = true;
	public bool autoCreateHeatwave = false;
	
	void Awake() 
	{
		gameObject.layer = Globals.LayerNumScenery;
		
		FillDefaultMaterials();
		
        components = this.GetComponents(typeof(DetonatorComponent));
		foreach (DetonatorComponent dc in components)
		{
			if (dc is DetonatorFireball)
			{
				_fireball = dc as DetonatorFireball;
			}
			if (dc is DetonatorSparks)
			{
				_sparks = dc as DetonatorSparks;
			}
			if (dc is DetonatorShockwave)
			{
				_shockwave = dc as DetonatorShockwave;
			}
			if (dc is DetonatorSmoke)
			{
				_smoke = dc as DetonatorSmoke;
			}
			if (dc is DetonatorGlow)
			{
				_glow = dc as DetonatorGlow;
			}
			if (dc is DetonatorLight)
			{
				_light = dc as DetonatorLight;
			}
			if (dc is DetonatorForce)
			{
				_force = dc as DetonatorForce;
			}
			if (dc is DetonatorHeatwave)
			{
				_heatwave = dc as DetonatorHeatwave;
			}
		}
		
		if (!_fireball && autoCreateFireball)
		{
			_fireball = gameObject.AddComponent("DetonatorFireball") as DetonatorFireball;
			_fireball.Reset();
		}
		
		if (!_smoke && autoCreateSmoke)
		{
			_smoke = gameObject.AddComponent("DetonatorSmoke") as DetonatorSmoke;
			_smoke.Reset();
		}
		
		if (!_sparks && autoCreateSparks)
		{
			_sparks = gameObject.AddComponent("DetonatorSparks") as DetonatorSparks;
			_sparks.Reset();
		}
		
		if (!_shockwave && autoCreateShockwave)
		{
			_shockwave = gameObject.AddComponent("DetonatorShockwave") as DetonatorShockwave;
			_shockwave.Reset();
		}
		
		if (!_glow && autoCreateGlow)
		{
			_glow = gameObject.AddComponent("DetonatorGlow") as DetonatorGlow;
			_glow.Reset();
		}
		
		if (!_light && autoCreateLight)
		{
			_light = gameObject.AddComponent("DetonatorLight") as DetonatorLight;
			_light.Reset();
		}
		
		if (!_force && autoCreateForce)
		{
			_force = gameObject.AddComponent("DetonatorForce") as DetonatorForce;
			_force.Reset();
		}

        if (!_heatwave && autoCreateHeatwave && SystemInfo.supportsImageEffects)
		{
			_heatwave = gameObject.AddComponent("DetonatorHeatwave") as DetonatorHeatwave;
			_heatwave.Reset();
		}
		
		components = this.GetComponents(typeof(DetonatorComponent));
	}
	
	void FillDefaultMaterials()
	{
		if (!fireballAMaterial) fireballAMaterial = DefaultFireballAMaterial();
		if (!fireballBMaterial) fireballBMaterial = DefaultFireballBMaterial();
		if (!smokeAMaterial) smokeAMaterial = DefaultSmokeAMaterial();
		if (!smokeBMaterial) smokeBMaterial = DefaultSmokeBMaterial();
		if (!shockwaveMaterial) shockwaveMaterial = DefaultShockwaveMaterial();
		if (!sparksMaterial) sparksMaterial = DefaultSparksMaterial();
		if (!glowMaterial) glowMaterial = DefaultGlowMaterial();
		if (!heatwaveMaterial) heatwaveMaterial = DefaultHeatwaveMaterial();
	}
	
	void Start()
	{
		if (explodeOnStart)
		{
			UpdateComponents();
			this.Explode();
		}
	}
	
	private float _lastExplosionTime = 1000f;
	void Update () 
    {
		if (destroyTime > 0f)
		{
			if (_lastExplosionTime + destroyTime <= Time.time)
			{
				Destroy(gameObject);
			}
		}
	}
	
	private bool _firstComponentUpdate = true;
	void UpdateComponents()
	{
		if (_firstComponentUpdate)
		{
			foreach (DetonatorComponent component in components)
			{
				component.Init();
				component.SetStartValues();
			}
			_firstComponentUpdate = false;
		}
			
		if (!_firstComponentUpdate)
		{
			foreach (DetonatorComponent component in components)
			{
				if (component.detonatorControlled)
				{
					component.size = component.startSize * (size / _baseSize);
					component.timeScale = (duration / _baseDuration);
					component.detail = component.startDetail * detail;
					component.force = (component.startForce *  (size /_baseSize)) + (direction * (size /_baseSize));
					component.velocity = (component.startVelocity *  (size /_baseSize)) + (direction * (size /_baseSize));
				
					//take the alpha of detonator color and consider it a weight - 1=use all detonator, 0=use all components
					component.color = Color.Lerp(component.startColor, color, color.a);
				}
			}
		}
	}
	
	private Component[] _subDetonators;
	
	 public void Explode() 
	{
		_lastExplosionTime = Time.time;
	
		foreach (DetonatorComponent component in components)
		{
			UpdateComponents();
			component.Explode();
		}
	}
	
	public void Reset() 
	{
		size = 10f; //this is hardcoded because _baseSize up top is not really the default as much as what we match to
		color = _baseColor;
		duration = _baseDuration;
		FillDefaultMaterials();
	}
	

	//Default Materials
	//The statics are so that even if there are multiple Detonators in the world, they
	//don't each create their own default materials. Theoretically this will reduce draw calls, but I haven't really
	//tested that.
	public static Material defaultFireballAMaterial;
	public static Material defaultFireballBMaterial;
	public static Material defaultSmokeAMaterial;
	public static Material defaultSmokeBMaterial;
	public static Material defaultShockwaveMaterial;
	public static Material defaultSparksMaterial;
	public static Material defaultGlowMaterial;
	public static Material defaultHeatwaveMaterial;
	
	public static Material DefaultFireballAMaterial()
	{
		if (defaultFireballAMaterial != null) return defaultFireballAMaterial;
        defaultFireballAMaterial = new Material(Shader.Find("Particles/Additive"));
		defaultFireballAMaterial.name = "FireballA-Default";
        Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/Fireball") as Texture2D;
		defaultFireballAMaterial.SetColor("_TintColor", Color.white);
		defaultFireballAMaterial.mainTexture = tex;
		defaultFireballAMaterial.mainTextureScale = new Vector2(0.5f, 1f);
		return defaultFireballAMaterial;
	}

	public static Material DefaultFireballBMaterial()
	{
		if (defaultFireballBMaterial != null) return defaultFireballBMaterial;
        defaultFireballBMaterial =  new Material(Shader.Find("Particles/Additive"));
		defaultFireballBMaterial.name = "FireballB-Default";
        Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/Fireball") as Texture2D;
		defaultFireballBMaterial.SetColor("_TintColor", Color.white);
		defaultFireballBMaterial.mainTexture = tex;
		defaultFireballBMaterial.mainTextureScale = new Vector2(0.5f, 1f);
		defaultFireballBMaterial.mainTextureOffset = new Vector2(0.5f, 0f);
		return defaultFireballBMaterial;
	}
	
	public static Material DefaultSmokeAMaterial()
	{
		if (defaultSmokeAMaterial != null) return defaultSmokeAMaterial;
        defaultSmokeAMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
		defaultSmokeAMaterial.name = "SmokeA-Default";
        Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/Smoke") as Texture2D;
		defaultSmokeAMaterial.SetColor("_TintColor", Color.white);
		defaultSmokeAMaterial.mainTexture = tex;
		defaultSmokeAMaterial.mainTextureScale = new Vector2(0.5f, 1f);
		return defaultSmokeAMaterial;
	}
		
	public static Material DefaultSmokeBMaterial()
	{
		if (defaultSmokeBMaterial != null) return defaultSmokeBMaterial;
        defaultSmokeBMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
		defaultSmokeBMaterial.name = "SmokeB-Default";
        Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/Smoke") as Texture2D;
		defaultSmokeBMaterial.SetColor("_TintColor", Color.white);
		defaultSmokeBMaterial.mainTexture = tex;
		defaultSmokeBMaterial.mainTextureScale = new Vector2(0.5f, 1f);
		defaultSmokeBMaterial.mainTextureOffset = new Vector2(0.5f, 0f);
		return defaultSmokeBMaterial;
	}
	
	public static Material DefaultSparksMaterial()
	{
		if (defaultSparksMaterial != null) return defaultSparksMaterial;
        defaultSparksMaterial = new Material(Shader.Find("Particles/Additive"));
		defaultSparksMaterial.name = "Sparks-Default";
        Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/GlowDot") as Texture2D;
		defaultSparksMaterial.SetColor("_TintColor", Color.white);
		defaultSparksMaterial.mainTexture = tex;
		return defaultSparksMaterial;
	}
	
	public static Material DefaultShockwaveMaterial()
	{	
		if (defaultShockwaveMaterial != null) return defaultShockwaveMaterial;
        defaultShockwaveMaterial = new Material(Shader.Find("Particles/Additive"));
		defaultShockwaveMaterial.name = "Shockwave-Default";
        Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/Shockwave") as Texture2D;
		defaultShockwaveMaterial.SetColor("_TintColor", new Color(0.1f,0.1f,0.1f,1f));
		defaultShockwaveMaterial.mainTexture = tex;
		return defaultShockwaveMaterial;
	}
	
	public static Material DefaultGlowMaterial()
	{
		if (defaultGlowMaterial != null) return defaultGlowMaterial;
        defaultGlowMaterial = new Material(Shader.Find("Particles/Additive"));
		defaultGlowMaterial.name = "Glow-Default";
        Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/Glow") as Texture2D;
		defaultGlowMaterial.SetColor("_TintColor", Color.white);
		defaultGlowMaterial.mainTexture = tex;
		return defaultGlowMaterial;
	}
	
	public static Material DefaultHeatwaveMaterial()
	{
//        //Unity Pro Only
//        if (SystemInfo.supportsImageEffects)
//        {
//            if (defaultHeatwaveMaterial != null) return defaultHeatwaveMaterial;
//            defaultHeatwaveMaterial = new Material(Shader.Find("HeatDistort"));
//            defaultHeatwaveMaterial.name = "Heatwave-Default";
//            Texture2D tex = UnityEngine.Resources.Load("Detonator/Textures/Heatwave") as Texture2D;
//            defaultHeatwaveMaterial.SetTexture("_BumpMap", tex);
//            return defaultHeatwaveMaterial;
//        }
//        else
//        {
            return null;
//        }
	}
}
