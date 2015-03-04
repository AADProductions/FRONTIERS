using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class ObexLight : WIScript
	{
		public Light LightSource;
		public AudioSource Audio;
		public Color LightColor;
		public float MaxLightIntensity = 1f;

		public override void OnStartup()
		{
			Audio.Play();
			LightSource.color = LightColor;
		}

		public override bool EnableAutomatically {
			get {
				return true;
			}
		}

		public void Update ( )
		{
			if (Audio.time < mLastAudioTime) {
				//we've looped
				mSmoothIntensity = MaxLightIntensity;
			}
			mSmoothIntensity = Mathf.Lerp(MaxLightIntensity, 0f, Audio.clip.length - Audio.time);
			LightSource.intensity = Mathf.Lerp(LightSource.intensity, mSmoothIntensity, 0.5f);
			mLastAudioTime = Audio.time;
		}

		protected float mSmoothIntensity = 0f;
		protected float mLastAudioTime = -1f;
	}
}