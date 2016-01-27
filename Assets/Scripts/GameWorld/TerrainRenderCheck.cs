using UnityEngine;
using System.Collections;

public class TerrainRenderCheck : MonoBehaviour {

	//this script is used to 'trick' unity
	//it wants to render terrains & trees even when they're not strictly visible
	//due to shadows etc.
	//so I use this cube to prevent that
	public Terrain terrain = null;

	void OnBecameVisible () {
		terrain.gameObject.layer = Globals.LayerNumSolidTerrain;
		//terrain.enabled = true;
	}

	void OnBecameInvisible () {
		terrain.gameObject.layer = Globals.LayerNumTrigger;
		//terrain.enabled = false;
	}
}
