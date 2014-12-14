using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using System.Collections.Generic;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class Bomb : WIScript
		{
				public DamagePackage DamageOnExplode = new DamagePackage();
				public FXManager.ExplosionType ExplosionType = FXManager.ExplosionType.Base;
				public MasterAudio.SoundType ExplosionSoundType = MasterAudio.SoundType.Explosions;
				//TODO look into moving these into the bomb's state
				public string ExplosionSound;
				public float ExplosionRadius = 1.0f;
				public float ExplosionRTDuration = 0.25f;
				public float ForceAtEdge = 0.0f;
				public float MinimumForce = 0.5f;
				public bool CanUseFuse = true;
				public Vector3 FuseOffset;

				public override void PopulateOptionsList(List <GUIListOption> options, List <string> message)
				{
						if (CanUseFuse && !worlditem.Is <Fuse>()) {
								Fuse fuse = null;
								if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <Fuse>(out fuse)) {
										options.Add(new GUIListOption("Attach Fuse", "Fuse"));
								}
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;			
						switch (dialogResult.SecondaryResult) {
								case "Fuse":
										Fuse existingFuse = null;
										if (Player.Local.Tool.worlditem.Is <Fuse>(out existingFuse)) {
												//copy the properties from the fuse the player is holding
												//then attach the fuse to the bomb
												Fuse newFuse = worlditem.GetOrAdd <Fuse>();
												newFuse.CopyFrom(existingFuse);
												newFuse.AttachToBomb(this);
												existingFuse.worlditem.RemoveFromGame();
										}
										break;

								default:
										break;
						}
				}

				public override void OnInitialized()
				{
						DamageOnExplode.SenderName = worlditem.DisplayName;
						Flammable flammable = worlditem.Get <Flammable>();
						flammable.OnDepleted += OnDepleted;
						flammable.DieOnDepleted = false;//just in case
				}

				public void OnDepleted()
				{
						if (mIsExploding)
								return;

						mIsCountingDown = false;
						mIsExploding = true;
						StartCoroutine(ExplodeOverTime());
				}

				public void Explode()
				{
						if (mIsExploding)
								return;

						mIsExploding = true;
						StartCoroutine(ExplodeOverTime());
				}

				public void OnIgnited()
				{
						if (mIsExploding)
								return;

						mIsCountingDown = true;
						StartCoroutine(CountdownOverTime());
				}

				public void OnExtinguished()
				{
						if (mIsExploding)
								return;

						mIsCountingDown = false;
				}

				protected IEnumerator CountdownOverTime()
				{
						while (mIsCountingDown) {
								//yield return new WaitForSeconds (1.0f);
								yield return null;
						}
						mIsCountingDown = false;
						yield break;
				}

				protected IEnumerator ExplodeOverTime()
				{
						//create an explosion trigger
						//wait for it to deplete
						//die
						Player.Local.FPSCamera.DoBomb(Vector3.one, 0.0001f, 0.001f);
						MasterAudio.PlaySound(ExplosionSoundType, transform, ExplosionSound);
						ExplosionEffectSphere explosion = FXManager.Get.SpawnExplosion(ExplosionType, transform.position, ExplosionRadius, MinimumForce, ExplosionRTDuration, ForceAtEdge, DamageOnExplode);
						worlditem.ActiveState = WIActiveState.Invisible;
						worlditem.ActiveStateLocked = true;
						while (explosion.IsInEffect) {
								yield return null;
						}
						mIsExploding = false;
						worlditem.SetMode(WIMode.RemovedFromGame);
						yield break;
				}

				protected bool mIsExploding = false;
				protected bool mIsCountingDown = false;
		}
}
