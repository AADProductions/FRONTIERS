using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour
{
		public float lampSpeed = 0.1f;
		public float intens_Speed = 9f;
		public bool timung = false;
		public float minIntens = 0.8f;
		public float maxIntens = 3.5f;
		public bool loopEnd = false;
		public float range_Speed = 12f;
		public float minRange = 2.8f;
		public float maxRange = 13.5f;
		public Color col_Main = Color.white;
		public float col_Speed = 1.5f;
		public Color col_Blend1 = Color.yellow;
		public Color col_Blend2 = Color.red;
		Color refCol;
		float intens;
		float randomIntens;
		float range;
		float randomRange;
		GameObject lamp;
		// Use this for initialization
		void Start()
		{
				lamp = this.gameObject;
				intens = lamp.light.intensity;
				range = lamp.light.range;
				lamp.light.color = col_Main;
		
				StartCoroutine(Timer());
		}

		void LateUpdate()
		{
				if (loopEnd) {
						StartCoroutine(Timer());
				}
				//loopEnd = false;
		
				intens = Mathf.SmoothStep(intens, randomIntens, (float)(Frontiers.WorldClock.ARTDeltaTime * intens_Speed));
				range = Mathf.SmoothStep(range, randomRange, (float)(Frontiers.WorldClock.ARTDeltaTime * range_Speed));
				lamp.light.intensity = intens;
				lamp.light.range = range;
				col_Main = Color.Lerp(col_Main, refCol, (float)(Frontiers.WorldClock.ARTDeltaTime * col_Speed));
				lamp.light.color = col_Main;
		}

		IEnumerator Timer()
		{
				timung = false;
				randomIntens = Random.Range(minIntens, maxIntens);
				randomRange = Random.Range(minRange, maxRange);
				refCol = Color.Lerp(col_Blend1, col_Blend2, Random.value);
				double waitUntil = Frontiers.WorldClock.AdjustedRealTime + lampSpeed;
				while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
						yield return null;
				}
				timung = true;
				randomIntens = Random.Range(minIntens, maxIntens);
				randomRange = Random.Range(minRange, maxRange);
				refCol = Color.Lerp(col_Blend1, col_Blend2, Random.value);
		
				waitUntil = Frontiers.WorldClock.AdjustedRealTime + lampSpeed;
				while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
						yield return null;
				}
				loopEnd = true;
		
				randomIntens = Random.Range(minIntens, maxIntens);
				randomRange = Random.Range(minRange, maxRange);
				refCol = Color.Lerp(col_Blend1, col_Blend2, Random.value);
				yield return null;
		}
}
