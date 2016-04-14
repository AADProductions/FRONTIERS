using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
	//doesn't just destroy a structure - actually demolishes it entirely and
	//replaces it with a new structure
	public class StructureDemolitionController : WIScript {
		public List <Demolishable> DemolitionWeakPoints = new List <Demolishable> ();
		Structure structure = null;
		public bool DemolitionStarted = false;

		public StructureDemolitionControllerState State = new StructureDemolitionControllerState ( );

		public override void OnInitialized ()
		{
			structure = worlditem.Get <Structure> ();
		}

		public void AddDemolishable (Demolishable weakPoint) {
			Debug.Log ("Adding weak point " + weakPoint.name + " to demolition controller");
			if (DemolitionWeakPoints.SafeAdd (weakPoint)) {
				weakPoint.OnDemolished += CheckWeakPoints;
			}
		}

		public void CheckWeakPoints ( ) {
			Debug.Log ("Checking weak points...");
			mStartDemolitionTime = WorldClock.AdjustedRealTime;
			enabled = true;
		}

		public void Update ( ) {
			if (DemolitionStarted) {
				enabled = false;
				return;
			}

			if (WorldClock.AdjustedRealTime < mStartDemolitionTime + State.CheckDelay) {
				return;
			}

			//check each weak point and see if it's null or demolished
			Debug.Log ("Checking all weak points (" + DemolitionWeakPoints.Count.ToString ( ) + ") to see if we should demolish structure");
			bool allWeakPointsDestroyed = true;
			for (int i = 0; i < DemolitionWeakPoints.Count; i++) {
				Demolishable weakPoint = DemolitionWeakPoints [i];
				if (weakPoint != null) {
					if (!weakPoint.worlditem.Get <Damageable> ().IsDead) {
						allWeakPointsDestroyed = false;
						break;
					}
				}
			}

			OnAttempt ();

			if (allWeakPointsDestroyed) {
				Debug.Log ("All weak points were destroyed, demolishing structure");
				DemolitionStarted = true;
				StartCoroutine (DemolishStructureOverTime ());
				OnSuccess ();
				enabled = false;
			} else {
				Debug.Log ("Not all weak points were destroyed, not demolishing structure");
				Finish ();
			}
		}

		protected virtual void OnSuccess ( ) {
			if (!string.IsNullOrEmpty (State.MissionName) && !string.IsNullOrEmpty (State.VariableNameOnSuccess)) {
				WorldItem itemToDestroy = null;
				for (int i = 0; i < State.WorldItemsToDestroy.Count; i++) {
					if (WIGroups.FindChildItem (State.WorldItemsToDestroy [i].FullPath, out itemToDestroy)) {
						itemToDestroy.RemoveFromGame ();
					} else {
						Debug.Log ("Couldn't find child item " + State.WorldItemsToDestroy [i].FullPath);
					}
				}
				Missions.Get.ChangeVariableValue (State.MissionName, State.VariableNameOnSuccess, State.VariableValueOnSuccess, State.ChangeTypeOnSuccess);
			}
		}

		protected virtual void OnAttempt ( ) {
			if (!string.IsNullOrEmpty (State.MissionName) && !string.IsNullOrEmpty (State.VariableNameOnAttempt)) {
				Missions.Get.ChangeVariableValue (State.MissionName, State.VariableNameOnAttempt, State.VariableValueOnAttempt, State.ChangeTypeOnAttempt);
			}
		}

		protected IEnumerator DemolishStructureOverTime ( ) {

			Debug.Log ("Calling destroy structure on " + structure.name + " from demolition controller");
			structure.DestroyStructure ();

			Transform explosionSource = gameObject.FindOrCreateChild ("ExplosionSource").transform;
			double timeStarted = WorldClock.AdjustedRealTime;
			List <ExplosionTemplate> explosions = new List<ExplosionTemplate> ();
			explosions.AddRange (State.Explosions);
			while (explosions.Count > 0) {
				double currentTime = WorldClock.AdjustedRealTime;
				for (int i = explosions.LastIndex (); i >= 0; i--) {
					ExplosionTemplate explosion = explosions [i];
					if (explosion.Delay < currentTime - timeStarted) {
						explosionSource.position = transform.position + explosion.Position;
						State.ExplosionDamage.Point = explosionSource.position;
						State.ExplosionDamage.SenderName = "Explosion";
						FXManager.Get.SpawnExplosion (
							explosion.Type,
							State.ExplosionDamage.Point,
							explosion.Radius,
							explosion.ForceAtEdge,
							explosion.MinimumForce,
							explosion.Duration,
							State.ExplosionDamage);
						MasterAudio.PlaySound (MasterAudio.SoundType.Explosions, explosionSource, explosion.ExplosionSound);
						Player.Local.DoEarthquake (explosion.Duration, explosion.BombShake);

						explosions.RemoveAt (i);
					}
				}
				yield return null;
			}
			GameObject.Destroy (explosionSource.gameObject, 0.5f);

			Finish ();
			yield break;
		}

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			State.Explosions.Clear ();
			foreach (Transform child in transform) {
				if (child.name.Contains ("Explosion")) {
					ExplosionTemplate explosion = new ExplosionTemplate ();
					explosion.Delay = float.Parse (child.name.Replace ("Explosion ", ""));
					explosion.Radius = child.transform.localScale.x;
					explosion.Position = child.position - transform.position;
					explosion.Duration = 0.25f;
					explosion.ForceAtEdge = 1f;
					explosion.Type = ExplosionType.Simple;
					State.Explosions.Add (explosion);
					State.TotalDuration = Mathf.Max (State.TotalDuration, explosion.Delay);
				}
			}
		}

		public void OnDrawGizmos ( )
		{
			foreach (ExplosionTemplate explosion in State.Explosions) {
				Gizmos.color = Color.Lerp (Color.yellow, Color.red, explosion.Delay / State.TotalDuration);
				Gizmos.DrawWireSphere (transform.position + explosion.Position, explosion.Radius);
			}

			foreach (Transform child in transform) {
				if (child.gameObject.activeSelf && child.name.Contains ("Explosion")) {
					Gizmos.color = Color.white;
					Gizmos.DrawSphere (child.position, child.transform.localScale.x);
				}
			}
		}
		#endif

		protected double mStartDemolitionTime;

	}

	[Serializable]
	public class StructureDemolitionControllerState {
		public float CheckDelay = 1f;
		public List <ExplosionTemplate> Explosions = new List <ExplosionTemplate> ( );
		[FrontiersAvailableModsAttribute ("Structure")]
		public string DestroyedTemplate = string.Empty;
		public float TotalDuration = 1f;
		public DamagePackage ExplosionDamage = new DamagePackage ( );

		public string MissionName;
		public string VariableNameOnAttempt;
		public int VariableValueOnAttempt = 1;
		public ChangeVariableType ChangeTypeOnAttempt;

		public string VariableNameOnSuccess;
		public int VariableValueOnSuccess = 1;
		public ChangeVariableType ChangeTypeOnSuccess;

		public List <MobileReference> WorldItemsToDestroy = new List<MobileReference>();
	}
}