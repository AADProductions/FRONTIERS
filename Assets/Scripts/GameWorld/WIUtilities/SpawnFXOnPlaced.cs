using UnityEngine;
using System.Collections;

public class SpawnFXOnPlaced : MonoBehaviour
{
	public GameObject 				FXPrefab;
	public MasterAudio.SoundType	SoundTypeOnPlaced;
	public string					SoundChildName;
	
	public void OnPlacedByPlayer (GameObject placedOn)
	{
		GameObject.Instantiate (FXPrefab, transform.position, transform.rotation);
		MasterAudio.PlaySound (SoundTypeOnPlaced, transform, SoundChildName);
	}
}
