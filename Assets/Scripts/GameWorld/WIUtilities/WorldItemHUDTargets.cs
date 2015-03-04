using UnityEngine;
using System.Collections;

public class WorldItemHUDTargets : MonoBehaviour
{
	public GameObject 	TopTarget;
	public GameObject 	BotTarget;
	public GameObject	Pivot;
	public float		Padding		= 1.25f;
	
	public void 		Awake ( )
	{
		TopTarget 						= new GameObject ("TopTarget");
		TopTarget.layer 				= Globals.LayerNumWorldItemActive;
		TopTarget.transform.parent		= transform;
		BotTarget 						= new GameObject ("BotTarget");
		BotTarget.layer 				= Globals.LayerNumWorldItemActive;
		BotTarget.transform.parent 		= transform;
	}
	
	public void 		Update ( )
	{
		TopTarget.transform.position 	= transform.position + new Vector3 (0f, renderer.bounds.extents.y * Padding, 0f);
		BotTarget.transform.position 	= transform.position + new Vector3 (0f, -renderer.bounds.extents.y * Padding, 0f);
	}
}
