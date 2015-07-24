using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers
{
	public class FXManager : Manager
	{
		public static FXManager Get;
		public GameObject MagicEffectSpherePrefab;
		public GameObject ExplosionEffectSpherePrefab;
		public GameObject BeamPrefab;
		public GameObject MapMarkerAvatar;
		public GameObject RockslideRockPrefab;
		public List <GameObject> ExplosionPrefabs = new List<GameObject> ();
		public List <GameObject> FXPrefabs = new List <GameObject> ();
		public List <GameObject> FirePrefabs = new List<GameObject> ();
		protected Dictionary <string, GameObject> mFXLookup = new Dictionary <string, GameObject> ();
		protected Dictionary <ExplosionType, GameObject> mExplosionLookup = new Dictionary <ExplosionType, GameObject> ();
		public List <FXPiece> DelayedFXPieces = new List <FXPiece> ();
		public float DefaultExplosionForceAtEdge = 1f;
		public float DefaultExplosionRTDuration = 5f;
		public float DefaultExplosionMinimumForce = 0.1f;
		public DamagePackage DefaultExplosionDamage = new DamagePackage ();
		public Transform SoundOrigin;
		public List <GameObject> ActiveMapMarkerAvatars = new List<GameObject> ();

		public override void WakeUp ()
		{
			base.WakeUp ();

			mFXLookup.Clear ();
			mExplosionLookup.Clear ();

			Get = this;
			for (int i = 0; i < FXPrefabs.Count; i++) {
				mFXLookup.Add (FXPrefabs [i].name, FXPrefabs [i]);
			}

			for (int i = 0; i < ExplosionPrefabs.Count; i++) {
				ExplosionType type = (ExplosionType)Enum.Parse (typeof(ExplosionType), ExplosionPrefabs [i].name, true);
				//Debug.Log ("EXPLOSIONS: adding type " + type.ToString () + " with name " + ExplosionPrefabs [i].name);
				mExplosionLookup.Add (type, ExplosionPrefabs [i]);
			}

			SoundOrigin = gameObject.FindOrCreateChild ("SoundOrigin");
		}

		public string [] FXNames {
			get {
				if (mFXNames == null || mFXNames.Length == 0) {
					List <string> fxNames = new List <string> () { "(None)" };
					for (int i = 0; i < FXPrefabs.Count; i++) {
						fxNames.Add (FXPrefabs [i].name);
					}
					fxNames.Sort ();
					mFXNames = fxNames.ToArray ();
				}
				return mFXNames;
			}
		}

		protected string[] mFXNames = null;

		public void SpawnMapMarkers (List <MapMarker> activeMapMarkers)
		{	//get rid of the existing markers
			for (int i = 0; i < ActiveMapMarkerAvatars.Count; i++) {
				GameObject.Destroy (ActiveMapMarkerAvatars [i]);
			}
			ActiveMapMarkerAvatars.Clear ();
			for (int i = 0; i < activeMapMarkers.Count; i++) {
				WorldChunk chunk = null;
				if (GameWorld.Get.ChunkByID (activeMapMarkers [i].ChunkID, out chunk)) {
					Vector3 worldPosition = WorldChunk.ChunkPositionToWorldPosition (chunk.ChunkBounds, activeMapMarkers [i].ChunkPosition) + activeMapMarkers [i].ChunkOffset;
					GameObject newMarker = GameObject.Instantiate (MapMarkerAvatar, worldPosition, Quaternion.identity) as GameObject;
					ActiveMapMarkerAvatars.Add (newMarker);
				}
			}
		}

		public GameObject SpawnFX (IItemOfInterest parentObject, string fxName)
		{
			if (parentObject != null) {
				return SpawnFX (parentObject.gameObject, fxName);
			}
			return null;
		}

		public void SpawnFX (Transform parentObject, string fxName, float delay) {
			FXPiece fxPiece = new FXPiece ();
			fxPiece.FXName = fxName;
			fxPiece.Delay = delay;
			fxPiece.FXParent = parentObject;
			fxPiece.Position = Vector3.zero;
			fxPiece.TimeAdded = WorldClock.AdjustedRealTime;
			DelayedFXPieces.Add (fxPiece);
		}

		public GameObject SpawnFX (Transform parentObject, string fxName)
		{
			GameObject prefab = null;

			if (parentObject == null || string.IsNullOrEmpty (fxName) || !mFXLookup.TryGetValue (fxName, out prefab)) {
				return null;
			}

			GameObject fx = GameObject.Instantiate (prefab) as GameObject;
			fx.transform.parent = parentObject;
			fx.transform.localPosition = Vector3.zero;
			fx.transform.localRotation = Quaternion.identity;
			return fx;
		}

		public GameObject SpawnFX (GameObject parentObject, string fxName)
		{
			return SpawnFX (parentObject.transform, fxName);
		}

		public void SpawnFX (Transform parentObject, FXPiece piece)
		{
			piece.TimeAdded = WorldClock.AdjustedRealTime;
			piece.FXParent = parentObject;
			DelayedFXPieces.Add (piece);
		}

		public GameObject SpawnFX (GameObject parentObject, string fxName, Vector3 offset)
		{
			GameObject fx = SpawnFX (parentObject, fxName);
			if (fx != null) {
				fx.transform.localPosition = offset;
			} else {
				Debug.Log ("FX name " + fxName + " was null");
			}
			return fx;
		}

		public GameObject GetOrSpawnFx (GameObject fxObject, GameObject parentObject, string fxName)
		{
			if (gameObject == null || gameObject.name != fxName) {
				GameObject.Destroy (fxObject);
				return SpawnFX (parentObject, fxName);
			}
			return fxObject;
		}

		public GameObject SpawnFX (Vector3 position, string fxName)
		{
			GameObject prefab = null;

			if (string.IsNullOrEmpty (fxName) || !mFXLookup.TryGetValue (fxName, out prefab)) {
				return null;
			}

			GameObject fx = GameObject.Instantiate (prefab) as GameObject;
			fx.transform.localPosition = position;
			fx.transform.localRotation = Quaternion.identity;

			return null;
		}

		public ExplosionEffectSphere SpawnExplosion (ExplosionType explosionType, Vector3 position, float targetRadius, float forceAtEdge, float minimumForce, float rtDuration, DamagePackage damage)
		{
			//Debug.Log ("Spawning explosion... with effects");
			GameObject explosionEffectSphereObject = GameObject.Instantiate (ExplosionEffectSpherePrefab, position, Quaternion.identity) as GameObject;
			ExplosionEffectSphere explosionEffectSphere = explosionEffectSphereObject.GetComponent <ExplosionEffectSphere> ();
			explosionEffectSphere.ExplosionDamage = damage;
			explosionEffectSphere.ForceAtEdge = forceAtEdge;
			explosionEffectSphere.MinimumForce = minimumForce;
			explosionEffectSphere.RTDuration = rtDuration;//Mathf.Max (rtDuration, (Time.fixedDeltaTime * 2));//make sure it has 2 ticks to capture items

			SpawnExplosionFX (explosionType, null, position);

			return explosionEffectSphere;
		}

		public ExplosionEffectSphere SpawnExplosion (string explosionType, Vector3 position, float targetRadius, float forceAtEdge, float minimumForce, float rtDuration, DamagePackage damage)
		{
			ExplosionType type = (ExplosionType)Enum.Parse (typeof(ExplosionType), explosionType, true);
			return SpawnExplosion (type, position, targetRadius, forceAtEdge, minimumForce, rtDuration, damage);
		}

		public void SpawnExplosionFX (string explosionType, Transform explosionParent, Vector3 position)
		{
			ExplosionType type = (ExplosionType)Enum.Parse (typeof(ExplosionType), explosionType, true);
			SpawnExplosionFX (type, explosionParent, position);
		}

		public void SpawnExplosionFX (ExplosionType explosionType, Transform explosionParent, Vector3 position)
		{
			GameObject prefab = mExplosionLookup [explosionType];
			GameObject instantiatedPrefab = null;
			if (explosionParent != null) {
				instantiatedPrefab = GameObject.Instantiate (prefab) as GameObject;
				instantiatedPrefab.transform.parent = explosionParent;
				instantiatedPrefab.transform.localPosition = position;
			} else {
				instantiatedPrefab = GameObject.Instantiate (prefab, position, Quaternion.identity) as GameObject;
			}
			//Debug.Log ("instantiated prefab " + prefab.name + " in spawn explosionFX");
		}

		public GameObject SpawnFire (FireType fireType, Transform fireParent, Vector3 position, Vector3 rotation, float scale, bool justForLooks)
		{
			GameObject firePrefab = null;
			for (int i = 0; i < FirePrefabs.Count; i++) {
				if (FirePrefabs [i].name.Equals (fireType.ToString (), StringComparison.OrdinalIgnoreCase)) {
					firePrefab = FirePrefabs [i];
					break;
				}
			}
			GameObject instantiatedFire = GameObject.Instantiate (firePrefab) as GameObject;
			instantiatedFire.transform.parent = fireParent;
			instantiatedFire.transform.localPosition = position;
			instantiatedFire.transform.localRotation = Quaternion.Euler (rotation);

			return instantiatedFire;
		}

		public void DestroyFX (GameObject fx)
		{
			if (fx != null) {
				GameObject.Destroy (fx);
			}
		}

		public GameObject SpawnFire (string fireType, Transform fireParent, Vector3 position, Vector3 rotation, float scale, bool justForLooks)
		{
			GameObject firePrefab = null;
			for (int i = 0; i < FirePrefabs.Count; i++) {
				if (FirePrefabs [i].name.Equals (fireType, StringComparison.OrdinalIgnoreCase)) {
					firePrefab = FirePrefabs [i];
					break;
				}
			}
			GameObject instantiatedFire = GameObject.Instantiate (firePrefab) as GameObject;
			instantiatedFire.transform.parent = fireParent;
			instantiatedFire.transform.localPosition = position;
			instantiatedFire.transform.localRotation = Quaternion.Euler (rotation);

			return instantiatedFire;
		}

		public void Update ()
		{
			for (int i = DelayedFXPieces.LastIndex (); i >= 0; i--) {
				FXPiece piece = DelayedFXPieces [i];
				if (WorldClock.AdjustedRealTime > (piece.TimeAdded + piece.Delay)) {
					//Debug.Log ("piece was ready to be spawned at " + WorldClock.AdjustedRealTime.ToString ());
					DelayedFXPieces.RemoveAt (i);
					if (piece.Explosion) {
						if (piece.JustForShow) {
							SpawnExplosionFX (piece.FXName, piece.FXParent, piece.Position);
						} else {
							DefaultExplosionDamage.Point = piece.Position;
							DefaultExplosionDamage.Origin = piece.Position;
							SpawnExplosion (piece.FXName, piece.FXParent.position + piece.Position, piece.Scale, DefaultExplosionForceAtEdge, DefaultExplosionMinimumForce, DefaultExplosionRTDuration, DefaultExplosionDamage);
						}
					} else {
						SpawnFX (piece.FXParent.gameObject, piece.FXName, piece.Position);
					}

					if (!string.IsNullOrEmpty (piece.SoundName)) {
						SoundOrigin.position = piece.FXParent.position + piece.Position;
						MasterAudio.PlaySound (piece.SoundType, SoundOrigin, piece.SoundName);
					}
				}
			}
		}
	}
}