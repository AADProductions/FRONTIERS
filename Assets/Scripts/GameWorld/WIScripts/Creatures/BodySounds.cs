using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
		public class BodySounds : MonoBehaviour
		{
				public Transform tr;
				public BodyAnimator Animator;
				public MasterAudio.SoundType FootstepSoundType = MasterAudio.SoundType.FootstepDirt;
				public string FootstepSound = "Step1";
				public MasterAudio.SoundType MotionSoundType = MasterAudio.SoundType.AnimalVoice;
				//common sounds
				//[FrontiersAudioClipAttribute ()]
				public string IdleSound;
				//[FrontiersAudioClipAttribute ()]
				public string Attack1Sound;
				//[FrontiersAudioClipAttribute ()]
				public string Attack2Sound;
				//[FrontiersAudioClipAttribute ()]
				public string WarnSound;
				//[FrontiersAudioClipAttribute ()]
				public string TakeDamageSound;
				//[FrontiersAudioClipAttribute ()]
				public string DieSound;
				//animation state sounds
				public List <BodySound> Sounds = new List<BodySound>();

				public void Awake()
				{
						tr = transform;
				}

				public void MakeFootStep()
				{
						if (Player.Local.Surroundings.IsSoundAudible(tr.position, Globals.MaxAudibleRange)) {
								MasterAudio.PlaySound(FootstepSoundType, tr, FootstepSound);
						}
				}

				public class BodySound
				{
						public MasterAudio.SoundType SoundType = MasterAudio.SoundType.AnimalVoice;
						public string MovementState;
						public string SoundName;
				}

				public void Refresh()
				{
						if (Animator.Idling) {
								if (UnityEngine.Random.value < 0.05f) {
										StartCoroutine(PlaySoundOverTime(IdleSound));
								}
						}

						if (Animator.Attack1) {
								StartCoroutine(PlaySoundOverTime(Attack1Sound));
						} else if (Animator.Attack2) {
								StartCoroutine(PlaySoundOverTime(Attack2Sound));
						} else if (Animator.Warn) {
								StartCoroutine(PlaySoundOverTime(WarnSound));
						} else if (Animator.TakingDamage) {
								StartCoroutine(PlaySoundOverTime(TakeDamageSound));
						} else if (Animator.Dead) {
								StartCoroutine(PlaySoundOverTime(DieSound));
								mDisabled = true;
						}
				}

				protected IEnumerator PlaySoundOverTime(string sound)
				{
						MasterAudio.PlaySound(MotionSoundType, transform, Attack1Sound);
						double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.5f;
						while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
						}
						mPlayingSound = false;
						if (mDisabled) {
								enabled = false;
						}
						yield break;
				}

				protected bool mDisabled = false;
				protected bool mPlayingSound = false;
				protected BodyAnimator mAnimator;
		}
}