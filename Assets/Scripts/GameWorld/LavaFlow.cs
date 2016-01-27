using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LavaFlow : MonoBehaviour {

	public Vector2 Offset;
	public Transform tr;
	Material lavaMaterial;
	Vector2 textureOffset;

	void OnEnable () {
		tr = transform;
		if (lavaMaterial == null) {
			lavaMaterial = gameObject.GetComponent <Renderer> ().material;
		}
	}

	void Update () {
		if (Application.isPlaying) {
			textureOffset.x += Offset.x * Time.deltaTime;
			textureOffset.y += Offset.y * Time.deltaTime;
		} else {
			textureOffset.x += Offset.x;
			textureOffset.y += Offset.y;
		}
		lavaMaterial.mainTextureOffset = textureOffset;
	}
}
