using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Trap : WIScript
		{		//general purpose indiana jones style trap
				//not to be confused with LandTrap and WaterTrap
				//which are used for catching animals
				public TrapState State = new TrapState();
				public Dynamic dynamic;
				public GameObject TrapObjectPrefab;
				public List <Transform> TrabObjectPrefabSpawnPoints = new List<Transform>();
				public int LastTrapObjectPrefabSpawnPointIndex = 0;
				public string TriggerAnimationName;
				public string TriggerAnimationSound;
				public MasterAudio.SoundType TriggerAnimationSoundType;
				[FrontiersFXAttribute]
				public string TriggerFX;
				public List <GameObject> TriggerFXParents = new List <GameObject>();
				public GameObject TriggerFXOffset;
				public List <Collider> DamageColliders = new List <Collider>();
				public string ResetAnimationName;
				public string ResetAnimationSound;
				public MasterAudio.SoundType ResetAnimationSoundType;
				public string MisfireAnimationSound;
				public MasterAudio.SoundType MisfireAnimationSoundType;
				public float InitialDelay = 0f;
				public float ResetDelay = 5f;
				public Animation AnimationTarget;

				public override void OnStartup()
				{
						for (int i = 0; i < DamageColliders.Count; i++) {
								DamageColliders[i].enabled = false;
						}
				}

				public override void OnInitialized()
				{
						dynamic = worlditem.Get <Dynamic>();
						dynamic.State.Type = WorldStructureObjectType.Trap;
						dynamic.OnTriggersLoaded += OnTriggersLoaded;
				}

				public void OnTriggersLoaded()
				{
						Trigger trigger = null;
						List <Trigger> triggersToCheck = new List <Trigger>(dynamic.Triggers);
						//check this just in case we don't have an 'officially' registered trigger
						if (worlditem.Is <Trigger>(out trigger)) {
								triggersToCheck.Add(trigger);
						}
						for (int i = 0; i < triggersToCheck.Count; i++) {
								triggersToCheck[i].OnTriggerStart += OnTriggerStart;
						}
				}

				public void OnTriggerStart(Trigger source)
				{
						if (mUpdatingTrapState) {
								return;
						}
						//if we've been disabled, misfire and return
						if (State.Mode == TrapMode.Disabled) {
								MasterAudio.PlaySound(MisfireAnimationSoundType, worlditem.tr, MisfireAnimationSound);
								return;
						}
						//there's a chance we'll misfire
						//based on player's skill
						Skill skill = null;
						//TODO this is a kludge, move this to a player script variable
						if (Skills.Get.HasLearnedSkill("LightStep", out skill)) {
								skill.Use(worlditem);
								if (skill.LastSkillResult) {
										MasterAudio.PlaySound(MisfireAnimationSoundType, worlditem.tr, MisfireAnimationSound);
										State.Mode = TrapMode.Misfired;
										GUI.GUIManager.PostDanger("Trap misfired");
										return;
								}
						}

						//nope we're all good
						mUpdatingTrapState = true;
						StartCoroutine(UpdateTrapState());
				}

				public void SpawnNextTrapObjectPrefab()
				{//usually called as an animation event
						LastTrapObjectPrefabSpawnPointIndex = TrabObjectPrefabSpawnPoints.NextIndex(LastTrapObjectPrefabSpawnPointIndex);
						Transform spawnPoint = TrabObjectPrefabSpawnPoints[LastTrapObjectPrefabSpawnPointIndex];
						GameObject trapObject = GameObject.Instantiate(TrapObjectPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
						TrapObject tr = (TrapObject)trapObject.GetOrAdd(State.TrapObjectScript);
						tr.ParentTrap = this;
				}

				protected IEnumerator UpdateTrapState()
				{
						State.Mode = TrapMode.Triggered;
						//delay before trigger
						yield return WorldClock.WaitForSeconds(InitialDelay);
						//spawn fx and sounds
						MasterAudio.PlaySound(TriggerAnimationSoundType, worlditem.tr, TriggerAnimationSound);
						for (int i = 0; i < TriggerFXParents.Count; i++) {
								FXManager.Get.SpawnFX(TriggerFXParents[i], TriggerFX);
						}
						//enable our damage colliders
						for (int i = 0; i < DamageColliders.Count; i++) {
								DamageColliders[i].enabled = true;
						}
						//play the trigger animation
						AnimationTarget.Play(TriggerAnimationName, PlayMode.StopSameLayer);
						AnimationTarget[TriggerAnimationName].normalizedTime = 0f;
						while (AnimationTarget[TriggerAnimationName].normalizedTime < 1f) {
								//while this is going on
								//check for intersections with items of interest
								yield return null;
						}
						//delay before reset
						yield return WorldClock.WaitForSeconds(ResetDelay);
						//play the reset animation
						AnimationTarget[ResetAnimationName].normalizedTime = 0f;
						AnimationTarget.Play(ResetAnimationName, PlayMode.StopSameLayer);
						while (AnimationTarget[ResetAnimationName].normalizedTime < 1f) {
								yield return null;
						}
						//turn our damage colliders
						for (int i = 0; i < DamageColliders.Count; i++) {
								DamageColliders[i].enabled = false;
						}
						//done
						State.Mode = TrapMode.Set;
						mUpdatingTrapState = false;
						yield break;
				}

				public bool OnTrapObjectTriggerEnter(Collider other)
				{
						IItemOfInterest ioi = null;
						if (WorldItems.GetIOIFromCollider(other, out ioi) && ioi != worlditem) {
								State.TrapDamage.Target = ioi;
								State.TrapDamage.Origin = worlditem.Position;
								State.TrapDamage.Point = worlditem.Position;
								DamageManager.Get.SendDamage(State.TrapDamage);
								if (State.TrapDamage.HitTarget) {
										State.NumTargetsHit++;
								}
								return true;
						}
						return false;
				}

				public void OnTriggerEnter(Collider other)
				{
						IItemOfInterest ioi = null;
						if (WorldItems.GetIOIFromCollider(other, out ioi) && ioi != worlditem) {
								State.TrapDamage.Target = ioi;
								State.TrapDamage.Origin = worlditem.Position;
								State.TrapDamage.Point = worlditem.Position;
								DamageManager.Get.SendDamage(State.TrapDamage);
								if (State.TrapDamage.HitTarget) {
										State.NumTargetsHit++;
								}
						}
				}

				protected bool mUpdatingTrapState = false;
		}

		[Serializable]
		public class TrapState
		{
				public TrapMode Mode = TrapMode.Set;
				public DamagePackage TrapDamage = new DamagePackage();
				public string TrapObjectScript = "TrapObject";
				public int NumTargetsHit = 0;
		}
}
