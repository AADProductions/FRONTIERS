using UnityEngine;
using System.Collections;

public class ScaleLightSource : MonoBehaviour {

	public Light 	LightSource;
	public float	RangeMultiplier	= 35.0f;
	
	void Start ( )
	{
		LightSource = transform.FindChild ("Light_Source").GetComponent<Light>();	
	}
	
	void Update ( )
	{
		LightSource.range = transform.localScale.x * RangeMultiplier;
	}
}
