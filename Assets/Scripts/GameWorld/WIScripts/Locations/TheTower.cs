using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World.WIScripts
{
	public class TheTower : WIScript
	{
		public GameObject BaseTowerPrefab;
		public GameObject TowerElevatorsPrefab;
		public GameObject CloudPuffPrefab;
		public Material BaseTowerMaterial;
		public Material CloudPuffMaterial;
		public Material BaseTowerInnardsMaterial;
		public TheTowerState State = new TheTowerState();
		public float TowerBase = 0f;
		public float TowerHeight = 3000f;
		public float TowerTopAlpha = 0.5f;
		public Transform CloudPuffParent;
		public List <Transform> CloudPuffTransforms = new List<Transform>();
		public List< Vector3> CloudPuffPositions = new List<Vector3> ();
		public List <ParticleSystem> CloudPuffs = new List<ParticleSystem>();
		public float CloudPuffAlpha = 0.15f;
		public float CloudPuffAlphaNight = 0.15f;
		public float CloudPuffRandomOffset = 50f;
		public float CloudPuffTransformMaxBob = 20f;
		public float CloudPuffTransformBobSpeed = 0.1f;
		public float CloudPuffParentRotateSpeed = 0.001f;
		public float MaxTowerDistance;
		public float MinTowerDistance;
		public float InnardGlowSpeed = 5f;
		public Vector3 BobTranslate;
		public Color CloudColorNight = new Color32(100, 130, 190, 32);
		public Color CloudColor;
		public Collider [] TowerExteriorColliders;

		public override void OnInitialized()
		{
			worlditem.OnAddedToGroup += OnAddedToGroup;

			MaxTowerDistance = 1000f;
			MinTowerDistance = 200f;
			InnardGlowSpeed = 0.4f;

			CloudPuffAlpha = 0.1f;
			CloudPuffAlphaNight = 0.15f;
			CloudPuffTransformMaxBob = 50f;
			CloudPuffTransformBobSpeed = 0.05f;
			CloudPuffParentRotateSpeed = 0.075f;

			TowerBase = -500f;
			TowerHeight = 700f;
			TowerTopAlpha = 0.8f;

			for (int i = 0; i < CloudPuffTransforms.Count; i++) {
				CloudPuffPositions.Add (CloudPuffTransforms[i].localPosition);
				GameObject cloudPuff = GameObject.Instantiate(CloudPuffPrefab) as GameObject;
				cloudPuff.transform.parent = CloudPuffTransforms[i];
				cloudPuff.transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
				cloudPuff.transform.localPosition = new Vector3(UnityEngine.Random.Range(-CloudPuffRandomOffset, CloudPuffRandomOffset), 0f, UnityEngine.Random.Range(-CloudPuffRandomOffset, CloudPuffRandomOffset));
			}

			Player.Get.AvatarActions.Subscribe (AvatarAction.LocationUndergroundEnter, new ActionListener (LocationUndergroundEnter));
			Player.Get.AvatarActions.Subscribe (AvatarAction.LocationUndergroundEnter, new ActionListener (LocationUndergroundExit));

		}

		public bool LocationUndergroundEnter (double timeStamp) {
			for (int i = 0; i < TowerExteriorColliders.Length; i++) {
				TowerExteriorColliders [i].enabled = false;
			}
			return true;
		}

		public bool LocationUndergroundExit (double timeStamp) {
			for (int i = 0; i < TowerExteriorColliders.Length; i++) {
				TowerExteriorColliders [i].enabled = true;
			}
			return false;
		}

		public void OnAddedToGroup()
		{
			GameObject tower = GameObject.Instantiate (BaseTowerPrefab, worlditem.Position, Quaternion.identity) as GameObject;
			GameObject.Instantiate(TowerElevatorsPrefab, worlditem.Position, Quaternion.identity);
			//we only need to set these once
			BaseTowerMaterial.SetFloat("_FogBottomHeight", TowerBase);
			BaseTowerMaterial.SetFloat("_FogTopHeight", TowerHeight);

			TowerExteriorColliders = tower.GetComponentsInChildren <Collider> ();

			enabled = true;
		}

		public void Update()
		{
			#if UNITY_EDITOR
			BaseTowerMaterial.SetFloat("_FogBottomHeight", TowerBase);
			BaseTowerMaterial.SetFloat("_FogTopHeight", TowerHeight);
			#endif
			BaseTowerMaterial.SetFloat("_FogStart", RenderSettings.fogStartDistance);
			BaseTowerMaterial.SetFloat("_FogEnd", RenderSettings.fogEndDistance);
			BaseTowerMaterial.SetColor("_FogBottomColor", RenderSettings.fogColor);
			BaseTowerMaterial.SetColor("_FogTopColor", Colors.Alpha(Color.Lerp(RenderSettings.fogColor, Color.black, 0.25f), TowerTopAlpha));

			TowerBase = Mathf.Lerp(0, -500f, (Vector3.Distance(worlditem.Position, Player.Local.Position) / (MaxTowerDistance - MinTowerDistance)));

			BaseTowerInnardsMaterial.SetFloat ("_IllumAmount", Mathf.Clamp01 (Mathf.Sin ((float)(WorldClock.AdjustedRealTime * InnardGlowSpeed))) * 3f);

			if (WorldClock.IsNight) {
				CloudColor =  Colors.Alpha(CloudColorNight, CloudPuffAlphaNight);
			} else {
				CloudColor = Color.Lerp(CloudColorNight, Colors.Alpha(GameWorld.Get.Sky.CloudColor, CloudPuffAlpha), 0.15f);
			}
			mCloudColorSmooth = Color.Lerp(mCloudColorSmooth, CloudColor, 0.01f);
			CloudPuffMaterial.SetColor("_TintColor", mCloudColorSmooth);
			CloudPuffMaterial.SetColor("_FogColor", mCloudColorSmooth);
			CloudPuffMaterial.SetFloat("_FogStart", RenderSettings.fogStartDistance);
			CloudPuffMaterial.SetFloat("_FogEnd", RenderSettings.fogEndDistance * 1.5f);

			CloudPuffParent.Rotate(0f, CloudPuffParentRotateSpeed, 0f);
			double offset = 1000;
			for (int i = 0; i < CloudPuffTransforms.Count; i++) {
				BobTranslate.y = Mathf.Sin((float)((WorldClock.AdjustedRealTime + (offset * i)) * CloudPuffTransformBobSpeed)) * CloudPuffTransformMaxBob;
				CloudPuffTransforms[i].localPosition = CloudPuffPositions[i] + BobTranslate;
				if (i % 2 == 0) { 
					CloudPuffTransforms[i].Rotate(0f, CloudPuffParentRotateSpeed, 0f);
				} else {
					CloudPuffTransforms[i].Rotate(0f, -CloudPuffParentRotateSpeed, 0f);
				}
			}
		}

		protected Color mCloudColorSmooth;
	}

	[Serializable]
	public class TheTowerState
	{
	}
}
