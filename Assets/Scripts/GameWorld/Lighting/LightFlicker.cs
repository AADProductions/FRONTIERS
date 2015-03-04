using UnityEngine;

public class LightFlicker : MonoBehaviour 
{	
	
	public float baseIntensity;
	
	void Update () 
	{
		float brightness = (float)Mathf.Sin(transform.position.x + 15.58213f * Time.time) + (float)Mathf.Sin(transform.position.y + 6.4624f * Time.time);
		brightness *= 0.25f;
		brightness += 0.5f;
		light.intensity = baseIntensity + 0.35f * brightness;
	}
}