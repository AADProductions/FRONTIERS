using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Detonator))]
[AddComponentMenu("Detonator/Smoke")]
public class DetonatorSmoke : DetonatorComponent
{
	private const float _baseSize = 1f;
	private const float _baseDuration = 8f;
	private Color _baseColor = new Color(.5f, .5f, .5f, .5f);
	private const float _baseDamping = 0.1300004f;
	
	private float _scaledDuration;

	private GameObject _smokeA;
	private DetonatorBurstEmitter _smokeAEmitter;
	public Material smokeAMaterial;
	
	private GameObject _smokeB;
	private DetonatorBurstEmitter _smokeBEmitter;
	public Material smokeBMaterial;
		
	public bool drawSmokeA = true;
	public bool drawSmokeB = true;
	
	override public void Init()
	{
		gameObject.layer = Globals.LayerNumScenery;
		FillMaterials(false);
		BuildSmokeA();
		BuildSmokeB();
	}
	
	public void FillMaterials(bool wipe)
	{
		if (!smokeAMaterial || wipe)
		{
			smokeAMaterial = MyDetonator().smokeAMaterial;
		}
		if (!smokeBMaterial || wipe)
		{
			smokeBMaterial = MyDetonator().smokeBMaterial;
		}
	}

    public void BuildSmokeA()
    {
		_smokeA = new GameObject("SmokeA");
		_smokeAEmitter = (DetonatorBurstEmitter)_smokeA.AddComponent("DetonatorBurstEmitter");
		_smokeA.transform.parent = this.transform;
		_smokeA.transform.localPosition = localPosition;
		_smokeA.transform.localRotation = Quaternion.identity;
		_smokeAEmitter.material = smokeAMaterial;
		_smokeAEmitter.exponentialGrowth = false;
		_smokeAEmitter.sizeGrow = 0.095f;
		_smokeAEmitter.useWorldSpace = MyDetonator().useWorldSpace;
		_smokeAEmitter.upwardsBias = MyDetonator().upwardsBias;
    }
	
	public void UpdateSmokeA()
	{
		_smokeA.transform.localPosition = Vector3.Scale(localPosition,(new Vector3(size, size, size)));
		
		//move slightly away from the main camera so it sorts properly
		_smokeA.transform.LookAt(Camera.main.transform);
		_smokeA.transform.localPosition = -(Vector3.forward * -1.5f);
		
		_smokeAEmitter.color = color;
		_smokeAEmitter.duration =  duration * .5f;
		_smokeAEmitter.durationVariation =  0f;
		_smokeAEmitter.timeScale = timeScale;
		_smokeAEmitter.count = 4f;
		_smokeAEmitter.particleSize = 25f;
		_smokeAEmitter.sizeVariation = 3f;
		_smokeAEmitter.velocity = velocity;
		_smokeAEmitter.startRadius = 10f;
		_smokeAEmitter.size = size;		
		_smokeAEmitter.useExplicitColorAnimation = true;
		_smokeAEmitter.explodeDelayMin = explodeDelayMin;
		_smokeAEmitter.explodeDelayMax = explodeDelayMax;

		Color color1 = new Color(.2f, .2f, .2f, .4f);
		Color color2 = new Color(.2f, .2f, .2f, .7f);
		Color color3 = new Color(.2f, .2f, .2f, .4f);
		Color color4 = new Color(.2f, .2f, .2f, 0f);
		
		_smokeAEmitter.colorAnimation[0] = color1;
		_smokeAEmitter.colorAnimation[1] = color2;
		_smokeAEmitter.colorAnimation[2] = color2;
		_smokeAEmitter.colorAnimation[3] = color3;
		_smokeAEmitter.colorAnimation[4] = color4;
	}
	
	public void BuildSmokeB()
    {
		_smokeB = new GameObject("SmokeB");
		_smokeBEmitter = (DetonatorBurstEmitter)_smokeB.AddComponent("DetonatorBurstEmitter");
		_smokeB.transform.parent = this.transform;
		_smokeB.transform.localPosition = localPosition;
		_smokeB.transform.localRotation = Quaternion.identity;
		_smokeBEmitter.material = smokeBMaterial;
		_smokeBEmitter.exponentialGrowth = false;
		_smokeBEmitter.sizeGrow = 0.095f;
		_smokeBEmitter.useWorldSpace = MyDetonator().useWorldSpace;
		_smokeBEmitter.upwardsBias = MyDetonator().upwardsBias;
    }
	
	public void UpdateSmokeB()
	{
		_smokeB.transform.localPosition = Vector3.Scale(localPosition,(new Vector3(size, size, size)));
		
		//move slightly away from the main camera so it sorts properly
		_smokeB.transform.LookAt(Camera.main.transform);
		_smokeB.transform.localPosition = -(Vector3.forward * -1f);
		
		_smokeBEmitter.color = color;
		_smokeBEmitter.duration =  duration * .5f;
		_smokeBEmitter.durationVariation =  0f;
		_smokeBEmitter.count = 2f;
		_smokeBEmitter.particleSize = 25f;
		_smokeBEmitter.sizeVariation = 3f;
		_smokeBEmitter.velocity = velocity;
		_smokeBEmitter.startRadius = 10f;
		_smokeBEmitter.size = size;		
		_smokeBEmitter.useExplicitColorAnimation = true;
		_smokeBEmitter.explodeDelayMin = explodeDelayMin;
		_smokeBEmitter.explodeDelayMax = explodeDelayMax;

		Color color1 = new Color(.2f, .2f, .2f, .4f);
		Color color2 = new Color(.2f, .2f, .2f, .7f);
		Color color3 = new Color(.2f, .2f, .2f, .4f);
		Color color4 = new Color(.2f, .2f, .2f, 0f);
		
		_smokeBEmitter.colorAnimation[0] = color1;
		_smokeBEmitter.colorAnimation[1] = color2;
		_smokeBEmitter.colorAnimation[2] = color2;
		_smokeBEmitter.colorAnimation[3] = color3;
		_smokeBEmitter.colorAnimation[4] = color4;
	}

    public void Reset()
    {
		FillMaterials(true);
		on = true;
		size = _baseSize;
		duration = _baseDuration;
		explodeDelayMin = 0f;
		explodeDelayMax = 0f;
		color = _baseColor;
		velocity = new Vector3(3f,3f,3f);
    }

    override public void Explode()
    {
		if (detailThreshold > detail) return;
		
		if (on)
		{
			UpdateSmokeA();
			UpdateSmokeB();
			if (drawSmokeA) _smokeAEmitter.Explode();
			if (drawSmokeB) _smokeBEmitter.Explode();
		}
    }

}
