using UnityEngine;
using System.Collections;
using Frontiers;

[ExecuteInEditMode]
public class Planet : MonoBehaviour {

	public MeshRenderer PlanetRenderer;
	public MeshRenderer CloudsRenderer;
	public Transform tr;
	public Transform Clouds;
	public Vector3 Rotation;
	public Vector3 CloudRotation;
	public TOD_Sky Sky;


	void Update () {
		if (!Sky.SpaceMode) {
			PlanetRenderer.enabled = false;
			CloudsRenderer.enabled = false;
			return;
		} else {
			PlanetRenderer.enabled = true;
			CloudsRenderer.enabled = true;
		}

		if (Application.isPlaying) {
			tr.Rotate (Rotation * Time.deltaTime);
			Clouds.Rotate (CloudRotation * Time.deltaTime);
			//float size = (GameManager.Get.GameCamera.transform.position - tr.position).magnitude;
			//tr.localScale = Vector3.one * 0.5f * size;
		} else {
			tr.Rotate (Rotation * 0.1f);
			Clouds.Rotate (CloudRotation * 0.1f);
		}
	}
}
