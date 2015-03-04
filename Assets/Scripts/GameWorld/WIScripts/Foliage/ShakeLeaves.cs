using UnityEngine;
using System.Collections;

public class ShakeLeaves : MonoBehaviour
{
	public GameObject 	WindObject;
	
	public void 		OnTakeDamage (float damageAmount)
	{
		WindObject.animation ["WindZoneHit"].normalizedTime = 0.0f;
		WindObject.animation.Play ("WindZoneHit", PlayMode.StopSameLayer);
	}
}
