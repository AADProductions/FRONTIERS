using UnityEngine;
using System.Collections;

public class ShakeLeaves : MonoBehaviour
{
	public GameObject 	WindObject;
	
	public void 		OnTakeDamage (float damageAmount)
	{
		WindObject.GetComponent<Animation>() ["WindZoneHit"].normalizedTime = 0.0f;
		WindObject.GetComponent<Animation>().Play ("WindZoneHit", PlayMode.StopSameLayer);
	}
}
