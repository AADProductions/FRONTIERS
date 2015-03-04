using UnityEngine;
using System.Collections;

public class CutsceneGlider : MonoBehaviour {

	public Material GliderTailMaterial;
	public float DisintegrateAmount = 0f;

	public void Update ( )
	{
		GliderTailMaterial.SetFloat ("_DisintegrateAmount", DisintegrateAmount);
	}
}
