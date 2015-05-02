using UnityEngine;
using System.Collections;
using Frontiers.World.WIScripts;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class OrientationOrbDestroyPlayerHome : MonoBehaviour, IBodyOwner, IHostile
		{
				#region IBodyOwner implementation

				public Vector3 Position {
						get {
								if (mDestroyed) {
										return Vector3.zero;
								}
								return transform.position;
						} 
						set {
								//do nothing
						}
				}

				public Quaternion Rotation {
						get {
								if (mDestroyed) {
										return Quaternion.identity;
								}
								return transform.rotation;
						}
				}

				public WorldBody Body { get; set; }

				public bool Initialized { get { return true; } }

				public double CurrentMovementSpeed { get; set; }

				public double CurrentRotationSpeed { get; set; }

				public bool IsImmobilized { get { return false; } }

				public bool IsGrounded { get; set; }

				public bool IsRagdoll { get; set; }

				public bool IsDestroyed { get { return mDestroyed; } }

				public int CurrentIdleAnimation { get; set; }

				public float NormalizedDamage { get { return 0f; } }
				//temp
				public string Speech1 = "eɗǁuabaɭaʒəɡ uaɗaw ʒipuaws";
				public string Speech2 = "eɗəadeaŋsuiɠ ɡluaɓ uiɭzzliimuej";
				public string OrbName = "oajʃawɡ";
				public PowerBeam Beam;
				public Transform BeamOrigin;
				public Transform BeamTarget;
				public Structure TargetStructure;

				#endregion

				#region IHostile implementation

				public IItemOfInterest hostile { get { return null; } }

				public string DisplayName { get { return "Orb"; } }

				public IItemOfInterest PrimaryTarget { get { return mPrimaryTarget; } }

				public bool HasPrimaryTarget { get { return true; } }

				public bool CanSeePrimaryTarget { get { return true; } }

				public HostileMode Mode { get { return HostileMode.Attacking; } }

				protected IItemOfInterest mPrimaryTarget;

				#endregion

				public void CoolOff () { 

				}

				public void SaySpeech1()
				{
						GUI.NGUIScreenDialog.AddSpeech(Speech1, OrbName, 2f);
						MasterAudio.PlaySound(MasterAudio.SoundType.AnimalVoice, Body.transform, "OrbWarn");
				}

				public void SaySpeech2()
				{
						GUI.NGUIScreenDialog.AddSpeech(Speech2, OrbName, 2f);
						MasterAudio.PlaySound(MasterAudio.SoundType.AnimalVoice, Body.transform, "OrbAttack1");
				}

				public void OnReachHome()
				{
						mPrimaryTarget = Player.Local;
						Player.Local.Surroundings.AddHostile(this);
						BeamTarget = gameObject.CreateChild("BeamTarget");
						BeamOrigin = Body.RootBodyPart.gameObject.FindOrCreateChild("OrbLuminiteLightPivot");
						//GUIManager.PostIntrospection ("What's that sound?");
						//get the structure, it should be loaded at this point
						if (!Structures.LoadedStructure("PlayerFarmHouse", out TargetStructure)) {
								Debug.Log("COULDN'T LOAD TARGET STRUCTURE PLAYER FARM HOUSE!");
						}
						//get the beam ready
						GameObject newBeam = GameObject.Instantiate(FXManager.Get.BeamPrefab) as GameObject;
						Beam = newBeam.GetComponent <PowerBeam>();
				}

				public void OnAttackHome()
				{
						TargetStructure.DestroyStructure();
						//time our beam hits with the structure explosions
						StartCoroutine(FireBeamsAtExplosions(TargetStructure.DestroyedFX));
				}

				protected IEnumerator FireBeamsAtExplosions(List<FXPiece> explosions)
				{
						//start the beam firing
						Beam.AttachTo(BeamOrigin, BeamTarget);
						Beam.Fire(1000f);
						bool finishedFiring = false;
						FXPiece currentExplosion = new FXPiece();
						currentExplosion.TimeAdded = Mathf.Infinity;
						double start;
						while (!finishedFiring) {
								finishedFiring = true;
								//go through each explosion (they're in no particular order)
								//find all explosions that haven't happened yet
								//choose the explosion that's closest to the current time

								for (int i = 0; i < explosions.Count; i++) {
										FXPiece explosion = explosions[i];
										if (WorldClock.AdjustedRealTime > explosion.TimeAdded + explosion.Delay) {
												//this explosion hasn't elapsed
												//we're also not done firing
												if (explosion.TimeAdded + explosion.Delay < currentExplosion.TimeAdded + currentExplosion.Delay) {
														//this explosion happens before the current explosion
														currentExplosion = explosion;
												}
										} else {
												//we still have more explosions to blow up
												finishedFiring = false;
										}
								}
								//aim the beam at that spot
								BeamTarget.position = TargetStructure.StructureBase.transform.position + currentExplosion.Position;
								start = WorldClock.AdjustedRealTime;
								while (WorldClock.AdjustedRealTime < start + 0.05f) {
										yield return null;
								}
						}
						//wait a sec for the last explosion
						start = WorldClock.AdjustedRealTime;
						while (WorldClock.AdjustedRealTime < start + 1.5f) {
								yield return null;
						}
						//we're done firing - power down the beam
						Beam.StopFiring();
						//it will destroy itself
						mPrimaryTarget = null;
						Beam.RequiresOriginAndTarget = true;
						yield break;
				}

				public void OnDisappear()
				{
						GUI.GUIManager.PostIntrospection("My home! I don't believe this!");
						Missions.Get.SetVariableValue("Orientation", "PlayerHomeDestroyed", 1);
						StartCoroutine(DestroyAfterBeamIsGone());
				}

				protected IEnumerator DestroyAfterBeamIsGone()
				{
						while (Beam != null) {
								yield return null;
						}
						GameObject.Destroy(gameObject);
						yield break;
				}

				public void OnDestroy()
				{
						mDestroyed = true;
				}

				protected bool mDestroyed = false;
		}
}