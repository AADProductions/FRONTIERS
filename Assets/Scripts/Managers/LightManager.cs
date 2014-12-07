using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers {
	public class LightManager : Manager
	{
		public static LightManager Get;
		public WorldLight WorldLightPrefab;
		public Light LightPrefab;
		public WorldLightTemplate DefaultTemplate = new WorldLightTemplate ();
		public List <WorldLightTemplate> Templates = new List <WorldLightTemplate> ( );
		public List <WorldLight> WorldLights = new List<WorldLight> ();
		public List <WorldLight> ActiveWorldLights = new List <WorldLight> ();
		public List <WorldLight> DisabledWorldLights = new List<WorldLight> ( );
		public Queue <WorldLight> InactiveWorldLights = new Queue <WorldLight> ();
		public List <Texture2D> Cookies = new List <Texture2D> ( );
		public AnimationCurve LightFalloff = new AnimationCurve ( );

		public override void WakeUp ()
		{
			Get = this;
			mTemplateLookup = new Dictionary<string, WorldLightTemplate> ();
		}

		public override void OnGameLoadStart ()
		{
			WorldClock.Get.TimeActions.Subscribe (TimeActionType.DaytimeStart, new ActionListener (DaytimeStart));
			WorldClock.Get.TimeActions.Subscribe (TimeActionType.NightTimeStart, new ActionListener (NightTimeStart));
			Templates.Clear ();
			mTemplateLookup.Clear ();
			Mods.Get.Runtime.LoadAvailableMods <WorldLightTemplate> (Templates, "Light");
			for (int i = 0; i < Templates.Count; i++) {
				mTemplateLookup.Add (Templates [i].Name, Templates [i]);
			}
		}

		public static void OnTriggerEnter (WorldLight lightObject, Collider other)
		{
			//see if other object is photosensitive
			//and if it is, keep track of it
			IItemOfInterest ioi = null;
			IPhotosensitive ps = null;
			if (WorldItems.GetIOIFromGameObject (other.gameObject, out ioi)) {
				switch (ioi.IOIType) {
				case ItemOfInterestType.WorldItem:
					ps = ioi.worlditem.Get <Photosensitive> ();
					break;

				case ItemOfInterestType.Player:
					ps = Player.Local.Surroundings;
					break;

				case ItemOfInterestType.Fire:
				case ItemOfInterestType.Light:
				case ItemOfInterestType.ActionNode:
					break;

				default:
					ps = (IPhotosensitive) ioi.gameObject.GetComponent (typeof(IPhotosensitive));
					break;
				}
			}

			if (ps == null) {
				return;
			}

			ps.LightSources.SafeAdd (lightObject);
			RefreshExposure (ps);
		}

		public static void OnTriggerExit (WorldLight lightObject, Collider other)
		{
			IItemOfInterest ioi = null;
			IPhotosensitive ps = null;
			if (WorldItems.GetIOIFromGameObject (other.gameObject, out ioi)) {
				switch (ioi.IOIType) {
				case ItemOfInterestType.WorldItem:
					ps = ioi.worlditem.Get <Photosensitive> ();
					break;

				case ItemOfInterestType.Player:
					ps = Player.Local.Surroundings;
					break;

				case ItemOfInterestType.Fire:
				case ItemOfInterestType.Light:
				case ItemOfInterestType.ActionNode:
					break;

				default:
					ps = (IPhotosensitive)ioi.gameObject.GetComponent (typeof(IPhotosensitive));
					break;
				}
			}

			if (ps == null) {
				return;
			}

			ps.LightSources.Remove (lightObject);
			RefreshExposure (ps);
		}

		public static void RefreshExposure (IPhotosensitive ps) {
			float exposureBeforeRefresh = ps.LightExposure;
			float heatBeforeRefresh = ps.HeatExposure;

			ps.FireSources.Clear ();

			ps.LightExposure = 0f;
			for (int i = ps.LightSources.LastIndex (); i >= 0; i--) {
				WorldLight lightSource = ps.LightSources [i];
				if (lightSource == null) {
					ps.LightSources.RemoveAt (i);
				} else {
					ps.LightExposure += lightSource.TargetBaseIntensity;
					if (lightSource.IsFireLight) {
						ps.FireSources.Add (lightSource.fire);
					}
				}
			}
			ps.HeatExposure = 0f;
			for (int i = 0; i < ps.FireSources.Count; i++) {
				Fire fire = ps.FireSources [i];
				if (Vector3.Distance (ps.Position, fire.FireLight.Position) < fire.BurnScale) {
					ps.HeatExposure += fire.BurnHeat;
				} else {
					ps.HeatExposure += fire.WarmHeat;
				}
			}

			if (ps.LightExposure > exposureBeforeRefresh) {
				ps.OnExposureIncrease.SafeInvoke ();
			} else if (ps.LightExposure < exposureBeforeRefresh) {
				ps.OnExposureDecrease.SafeInvoke ();
			}

			if (ps.HeatExposure > heatBeforeRefresh) {
				ps.OnHeatIncrease.SafeInvoke ();
			} else if (ps.HeatExposure < heatBeforeRefresh) {
				ps.OnHeatDecrease.SafeInvoke ();
			}
		}

		public Texture2D Cookie (string cookie)
		{
			for (int i = 0; i < Cookies.Count; i++) {
				if (Cookies [i].name == cookie) {
					return Cookies [i];
				}
			}
			return null;
		}

		public static WorldLight GetWorldLight (string lightType, Transform parent, Vector3 offset, bool enabled, WorldLightType wlType) {
			return GetWorldLight (null, lightType, parent, offset, Vector3.zero, enabled, wlType);
		}

		public static WorldLight GetWorldLight (WorldLight existingLight, string templateName, Transform lightParent, Vector3 lightOffset, Vector3 lightRotation, bool enabled, WorldLightType wlType)
		{
			#if UNITY_EDITOR
			if (Get == null) {
				Mods.WakeUp <LightManager> ("Frontiers_ObjectManagers");
			}
			if (Colors.Get == null)
			{
				Mods.WakeUp <Colors> ("Frontiers_ArtResourceManagers");
			}
			#endif
			WorldLightTemplate wlt = null;
			//if the existing light isn't null, just apply the new template
			if (existingLight != null) {
				existingLight.gameObject.SetActive (true);
				existingLight.tr.parent = null;
				//set the scale first so the collider range isn't affected
				existingLight.tr.localScale = Vector3.one;
				existingLight.tr.parent = lightParent;
				existingLight.tr.localPosition = lightOffset;
				if (lightRotation != Vector3.zero) {
					existingLight.tr.localRotation = Quaternion.Euler (lightRotation);
				} else {
					existingLight.tr.localRotation = Quaternion.identity;
				}
				if (!mTemplateLookup.TryGetValue (templateName, out wlt)) {
					wlt = Get.DefaultTemplate;
				}
				existingLight.SetTemplate (wlt);
				return existingLight;
			}

			WorldLight newWorldLight = null;
			if (Get.InactiveWorldLights.Count > 0) {
				newWorldLight = Get.InactiveWorldLights.Dequeue ();
				Get.ActiveWorldLights.Add (newWorldLight);
			} else {
				newWorldLight = CreateWorldLight ( );
			}
			newWorldLight.Reset ();
			newWorldLight.Type = wlType;
			newWorldLight.Enable (true);
			newWorldLight.tr.parent = null;
			//set the scale first so the collider range isn't affected
			newWorldLight.tr.localScale = Vector3.one;
			newWorldLight.tr.parent = lightParent;
			newWorldLight.tr.localPosition = lightOffset;
			if (lightRotation != Vector3.zero) {
				newWorldLight.tr.localRotation = Quaternion.Euler (lightRotation);
			} else {
				newWorldLight.tr.localRotation = Quaternion.identity;
			}
			if (!mTemplateLookup.TryGetValue (templateName, out wlt)) {
				wlt = Get.DefaultTemplate;
			}
			newWorldLight.SetTemplate (wlt);

			return newWorldLight;
		}

		public static void DeactivateWorldLight (WorldLight lightToRetire)
		{
			//Debug.Log ("Deactivating light");
			if (lightToRetire != null) {
				//send it to the queue to be removed
				if (!Get.DisabledWorldLights.Contains (lightToRetire)) {
					Get.DisabledWorldLights.Add (lightToRetire);
				}
				Get.ActiveWorldLights.Remove (lightToRetire);
				lightToRetire.TurnOff ();
			}
		}

		public static void DestroyWorldLight (WorldLight lightToDestroy)
		{
			//this will be cleand up during update
		}

	
		public bool NightTimeStart (double timeStamp)
		{
			return true;
		}

		public bool DaytimeStart (double timeStamp)
		{
			return true;
		}

		public static void CalculateExposure (IPhotosensitive ps, float wDeltaTime)
		{
			ps.LightExposure = 0;
			ps.HeatExposure = 0;
			for (int i = ps.LightSources.LastIndex ( ); i >=0; i--) {
				WorldLight worldLight = ps.LightSources [i];
				if (worldLight == null) {
					ps.LightSources.RemoveAt (i);
				} else {
					//distance to center point of light from outer edge of object
					float distance = Mathf.Clamp ((Vector3.Distance (worldLight.transform.position, ps.Position) - ps.Radius), 0.0001f, worldLight.TargetBaseRange);
					//exposure = time * light intensity / distance or minimum intensity, whichever is greater
					//multiply that by global light exposure multiplier
					ps.LightExposure += ((wDeltaTime * worldLight.TargetBaseIntensity) / distance) * Globals.LightExposureMultiplier;
					ps.HeatExposure += ((wDeltaTime * worldLight.TargetHeatIntensity) / distance) * Globals.HeatExposureMultiplier;
				}
			}
		}

		protected static WorldLight CreateWorldLight ( )
		{
			GameObject newWorldLightObject = GameObject.Instantiate (Get.WorldLightPrefab.gameObject) as GameObject;
			WorldLight newWorldLight = newWorldLightObject.GetComponent <WorldLight> ();
			newWorldLight.Template = Get.DefaultTemplate;
			newWorldLight.LightCollider.enabled = false;

			Get.WorldLights.Add (newWorldLight);
			Get.ActiveWorldLights.Add (newWorldLight);
			return newWorldLight;
		}

		protected static Dictionary <string, WorldLightTemplate> mTemplateLookup;

		public float MasterBrightness;

		public int mUpdateLights = 0;

		protected float mLerpThisFrame;

		public void Update ( )
		{
			if (GameManager.Is (FGameState.Cutscene)) {
				mLerpThisFrame = 1f;
				MasterBrightness = 1.0f;
				if (WorldClock.IsDay) {
					MasterBrightness = 1.0f - Mathf.Clamp01 (GameWorld.Get.Sky.LightIntensity + Colors.Value (GameWorld.Get.Sky.AmbientColor));
				}
				mUpdateLights = 10;
			} else if (GameManager.Is (FGameState.InGame)) {
				mLerpThisFrame = (float) (Frontiers.WorldClock.ARTDeltaTime * 0.5f);
			} else {
				return;
			}

			mUpdateLights++;
			if (mUpdateLights > 10) {
				mUpdateLights = 0;
				//update all world lights based on the time of day
				float masterBrightness = 1.0f;
				if (WorldClock.IsDay) {
					masterBrightness = 1.0f - Mathf.Clamp01 (GameWorld.Get.Sky.LightIntensity + Colors.Value (GameWorld.Get.Sky.AmbientColor));
				}
				MasterBrightness = Mathf.Lerp (MasterBrightness, masterBrightness, mLerpThisFrame);

				for (int i = WorldLights.LastIndex (); i >= 0; i--) {
					WorldLight wl = WorldLights [i];
					if (wl == null) {
						WorldLights.RemoveAt (i);
					} else {
						switch (wl.Type) {
						case WorldLightType.Exterior:
						default:
							wl.MasterBrightness = MasterBrightness;
							break;

						case WorldLightType.InteriorOrUnderground:
							wl.MasterBrightness = 1.0f;
							break;

						case WorldLightType.Equipped:
							if (Player.Local.Surroundings.IsOutside) {
								wl.MasterBrightness = MasterBrightness;
							} else {
								wl.MasterBrightness = 1.0f;
							}
							break;

						case WorldLightType.AlwaysOn:
							wl.MasterBrightness = 1.0f;
							break;
						}
					}
				}

				for (int i = 0; i < Mats.Get.TimedGlowMaterials.Count; i++) {
					Material mat = Mats.Get.TimedGlowMaterials [i];
					mat.SetColor ("_EmiTint", Colors.Alpha (mat.GetColor ("_EmiTint"), MasterBrightness));
				}
			}
		}

		#if UNITY_EDITOR
		public void EditorSaveLightTemplates ( )
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor (true);

			foreach (WorldLightTemplate wlt in Templates) {
				Mods.Get.Editor.SaveMod <WorldLightTemplate> (wlt, "Light", wlt.Name);
			}
		}

		public void EditorLoadLightTemplates ( )
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor (true);
			Templates.Clear ();

			List <string> availableTemplates = Mods.Get.Editor.Available ("Light");
			foreach (string availableTemplate in availableTemplates) {
				WorldLightTemplate template = null;
				if (Mods.Get.Editor.LoadMod <WorldLightTemplate> (ref template, "Light", availableTemplate)) {
					Templates.Add (template);
				}
			}
		}
		#endif
	}

	public enum WorldLightType {
		Exterior,
		InteriorOrUnderground,
		Equipped,
		AlwaysOn,
	}

	[Serializable]
	public class WorldLightTemplate : Mod
	{
		[BitMaskAttribute (typeof (SpotlightDirection))]
		public SpotlightDirection Spotlights = SpotlightDirection.None;
		public bool BaseLight = true;
		public string Cookie = string.Empty;

		[FrontiersColorAttribute]
		public string Color1;
		[FrontiersColorAttribute]
		public string Color2;

		public float BaseRange = 1f;
		public float BaseIntensity = 1.5f;
		public float BaseHaloIntensity = 0f;

		public float SpotlightRange = 15f;
		public float SpotlightAngle = 85f;
		public float SpotlightIntensity = 1f;

		public float DayMultiplier = 1f;
		public float NightMultiplier = 1f;
		public float UndergroundMultiplier = 1f;
		public float InteriorMultiplier = 1f;
		public float BrightnessFlicker = 0f;
		public float BrightnessFlickerSpeed = 0f;
		public float ColorFlicker = 0f;
		public float ColorFlickerSpeed = 0f;
		public float TransitionSpeedUp = 1f;
		public float TransitionSpeedDown = 1f;
	}

	[Flags]
	[Serializable]
	public enum SpotlightDirection : int
	{
		None = 0,
		Top = 1,
		Bottom = 2,
		Front = 4,
		Back = 8,
		Left = 16,
		Right = 32,
	}
}