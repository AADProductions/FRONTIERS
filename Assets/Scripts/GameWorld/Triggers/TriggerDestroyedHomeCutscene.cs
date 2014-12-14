using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.Data;

namespace Frontiers.World
{
		public class TriggerDestroyedHomeCutscene : WorldTrigger
		{		//another specific sequence that couldn't be done generically
				public TriggerDestroyedHomeCutsceneState State = new TriggerDestroyedHomeCutsceneState();
				public GameObject CreatureCutsceneObject;
				public CreatureBody CreatureCutsceneBody;
				public Animation CreatureCutsceneAnimation;

				public override bool OnPlayerEnter()
				{
						if (!mPlayingCutscene) {
								mPlayingCutscene = true;
								StartCoroutine(PlayCutsceneOverTime());
								return true;
						}
						return false;
				}

				public IEnumerator PlayCutsceneOverTime()
				{
						yield return new WaitForSeconds(State.InitialDelay);
						//get the creature body - we're only using a shell here
						CreatureBody body = null;
						CreatureTemplate template = null;
						AnimationClip clip = null;
						if (Creatures.GetBody(State.CreatureBodyName, out body)) {
								if (Creatures.GetCutsceneClip(State.CutsceneClipName, out clip)) {
										//create the base object and add the animation
										//parent it under this trigger
										CreatureCutsceneObject = gameObject.CreateChild(State.Name + " - " + State.CreatureBodyName).gameObject;
										CreatureCutsceneAnimation = CreatureCutsceneObject.AddComponent <Animation>();
										CreatureCutsceneAnimation.AddClip(clip, State.CutsceneClipName);
										//this script will implement the IBodyOwner interface
										CreatureCutsceneObject.AddComponent(State.CutsceneScriptName);
										IBodyOwner bodyOwner = CreatureCutsceneObject.GetComponent(typeof(IBodyOwner)) as IBodyOwner;
										//create an empty body shell
										GameObject creatureCutsceneBodyObject = GameObject.Instantiate(body.gameObject, transform.position, Quaternion.identity) as GameObject;
										CreatureCutsceneBody = creatureCutsceneBodyObject.GetComponent <CreatureBody>();
										CreatureCutsceneBody.Owner = bodyOwner;
										//and we're off!
										CreatureCutsceneAnimation.Play(State.CutsceneClipName, PlayMode.StopAll);
								}
						}

						while (CreatureCutsceneObject != null) {
								yield return null;
						}

						GameObject.Destroy(CreatureCutsceneBody.gameObject);

						mPlayingCutscene = false;
						yield break;
				}

				protected bool mPlayingCutscene = false;
		}

		[Serializable]
		public class TriggerDestroyedHomeCutsceneState : WorldTriggerState
		{
				public string CreatureBodyName = "Orb";
				public string CutsceneClipName = string.Empty;
				public string CutsceneScriptName = string.Empty;
				public float InitialDelay = 0.5f;
		}
}