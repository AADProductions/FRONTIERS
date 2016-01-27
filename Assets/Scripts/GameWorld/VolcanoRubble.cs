using UnityEngine;
using System.Collections;

namespace Frontiers
{
	public class VolcanoRubble : MonoBehaviour
	{

		public GameObject RubblePiece;
		public float TimeBetweenChunks;
		public float ForceMultiplier;
		public int MaxChunks = 10;
		public GameObject[] Chunks;
		Vector3 rubbleDirection;
		Vector3 randomDirection;
		float lastChunkTime;

		void Update ()
		{
			if (Chunks == null) {
				RubblePiece.SetActive (false);
				Chunks = new GameObject [MaxChunks];
				for (int i = 0; i < MaxChunks; i++) {
					Chunks [i] = GameObject.Instantiate (RubblePiece) as GameObject;
					Chunks [i].SetActive (false);
				}
			}
			if (WorldClock.AdjustedRealTime > lastChunkTime + TimeBetweenChunks) {
				for (int i = 0; i < Chunks.Length; i++) {
					if (!Chunks [i].activeSelf) {
						Chunks [i].transform.position = transform.position;
						Chunks [i].gameObject.SetActive (true);
						Chunks [i].rigidbody.AddForce (rubbleDirection * ForceMultiplier);
						Chunks [i].audio.Play ();
					}
				}
			}
		}
	}
}