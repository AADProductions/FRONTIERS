using UnityEngine;
using System.Collections;
using Frontiers;

public class DamageAudio : MonoBehaviour
{		//this class has been replaced by MasterAudio
		//should remove it at some point
		public AudioSource Audio;
		public AudioClip TakeDamage;
		public AudioClip TakeCriticalDamage;
		public AudioClip TakeOverkillDamage;
		public AudioClip KilledByDamage;
		public bool DisableOnDestroy = true;
		public float PitchVariance = 1.0f;

		public void Start()
		{
				Audio = gameObject.GetOrAdd <AudioSource>();
		}

		protected void OnTakeDamage()
		{
				Audio.PlayOneShot(TakeDamage);
		}

		protected void OnTakeCriticalDamage()
		{
				Audio.PlayOneShot(TakeCriticalDamage);
		}
		 
		protected void OnTakeOverkillDamage()
		{
				Audio.PlayOneShot(TakeOverkillDamage);
		}

		protected void OnDie()
		{
				Audio.PlayOneShot(KilledByDamage);
				GameObject.Destroy(this);
		}
}