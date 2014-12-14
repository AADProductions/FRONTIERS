using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class WorldLight : MonoBehaviour, IItemOfInterest
		{		//detectable light source
				//affects IPhotosensitive via LightManager
				public SphereCollider LightCollider;
				public WorldLightTemplate Template;
				public Transform tr;

				#region itemofinterest implementation

				public ItemOfInterestType IOIType {
						get {
								if (IsFireLight) {
										return ItemOfInterestType.Fire;
								} else {
										return ItemOfInterestType.Light;
								}
						}
				}

				public Vector3 Position { get { return tr.position; } }

				public bool Has(string scriptName)
				{
						return false;
				}

				public bool HasAtLeastOne(List <string> scriptNames)
				{
						return scriptNames.Count == 0;
				}

				public WorldItem worlditem { get { return null; } }

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return null; } }

				public WorldLight worldlight { get { return this; } }

				public Fire fire { get { return ParentFire; } }

				public bool Destroyed { get { return mDestroyed; } }

				public bool HasPlayerFocus { get; set; }

				#endregion

				public WorldLightType Type = WorldLightType.Exterior;

				public float MasterBrightness {
						get {
								return mMasterBrightness;
						}
						set {
								if (value < 0.01f) {
										value = 0f;
								}
								if (!Mathf.Approximately(mMasterBrightness, value)) {
										//Debug.Log ("Master brightness in light is not equal to " + value.ToString () + ", updating now");
										mMasterBrightness = value;
										enabled = true;
								}
						}
				}

				public Light SpotlightTop;
				public Light SpotlightBottom;
				public Light SpotlightFront;
				public Light SpotlightBack;
				public Light SpotlightLeft;
				public Light SpotlightRight;
				public Light BaseLight;
				public Fire ParentFire;

				public bool IsFireLight {
						get {
								return ParentFire != null;
						}
				}

				public void Awake()
				{
						mSpotlights.Add(SpotlightTop);
						mSpotlights.Add(SpotlightBottom);
						mSpotlights.Add(SpotlightFront);
						mSpotlights.Add(SpotlightBack);
						mSpotlights.Add(SpotlightLeft);
						mSpotlights.Add(SpotlightRight);
				}

				public float TargetSpotIntensity {
						get {
								return mTargetSpotIntensity * mMasterBrightness;
						}
						set {
								if (!Mathf.Approximately(mTargetSpotIntensity, value)) {
										mTargetSpotIntensity = value;
										enabled = true;
								}
						}
				}

				public float TargetBaseIntensity {
						get {
								return mTargetBaseIntensity * mMasterBrightness;
						}
						set {
								if (!Mathf.Approximately(mTargetBaseIntensity, value)) {
										mTargetBaseIntensity = value;
										enabled = true;
								}
						}
				}

				public float TargetHeatIntensity {
						get {
								return mTargetHeatIntensity;
						}
						set {
								if (!Mathf.Approximately(mTargetHeatIntensity, value)) {
										mTargetHeatIntensity = value;
										enabled = true;
								}
						}
				}

				public Color TargetColor {
						get {
								return mTargetColor;
						}set {
								mTargetColor = value;
						}
				}

				public float TargetBaseRange {
						get {
								return mTargetBaseRange;
						}set {
								if (!Mathf.Approximately(mTargetBaseRange, value)) {
										mTargetBaseRange = value;
										enabled = true;
								}
						}
				}

				public float TargetSpotRange {
						get {
								return mTargetSpotRange;
						}set {
								if (!Mathf.Approximately(mTargetSpotRange, value)) {
										mTargetSpotRange = value;
										enabled = true;
								}
						}
				}

				public bool IsOff {
						get {
								return mIsOff;
						}
						set {
								if (mIsOff != value) {
										mIsOff = value;
										enabled = true;
								}
						}
				}

				public void Deactivate()
				{
						LightManager.DeactivateWorldLight(this);
				}

				public void OnDestroy()
				{
						LightManager.DestroyWorldLight(this);
						mDestroyed = true;
				}

				public void SetTemplate(WorldLightTemplate newTemplate)
				{
						Template = newTemplate;

						SpotlightTop.enabled = Flags.Check((uint)Template.Spotlights, (uint)SpotlightDirection.Top, Flags.CheckType.MatchAny);
						SpotlightBottom.enabled = Flags.Check((uint)Template.Spotlights, (uint)SpotlightDirection.Bottom, Flags.CheckType.MatchAny);
						SpotlightLeft.enabled = Flags.Check((uint)Template.Spotlights, (uint)SpotlightDirection.Left, Flags.CheckType.MatchAny);
						SpotlightRight.enabled = Flags.Check((uint)Template.Spotlights, (uint)SpotlightDirection.Right, Flags.CheckType.MatchAny);
						SpotlightFront.enabled = Flags.Check((uint)Template.Spotlights, (uint)SpotlightDirection.Front, Flags.CheckType.MatchAny);
						SpotlightBack.enabled = Flags.Check((uint)Template.Spotlights, (uint)SpotlightDirection.Back, Flags.CheckType.MatchAny);

						Color color = Colors.Get.ByName(Template.Color1);
						Texture2D cookie = LightManager.Get.Cookie(Template.Cookie);

						for (int i = 0; i < mSpotlights.Count; i++) {
								Light spotlight = mSpotlights[i];
								spotlight.color = color;
								spotlight.intensity = 0f;
								spotlight.range = 0f;
								spotlight.spotAngle = Template.SpotlightAngle;
								spotlight.cookie = cookie;
						}

						TargetSpotIntensity = Template.SpotlightIntensity;
						TargetSpotRange = Template.SpotlightRange;

						BaseLight.enabled = Template.BaseLight;
						BaseLight.intensity = 0f;
						BaseLight.color = color;
						TargetBaseRange = Template.BaseRange;
						TargetBaseIntensity = Template.BaseIntensity;
				}

				public void Reset()
				{
						mIsOff = false;
				}

				public static double gLerpSpeed = 2.0;
				protected float mLerpThisFrame;

				public void Update()
				{
						if (GameManager.Is(FGameState.Cutscene)) {
								mLerpThisFrame = (float)(WorldClock.RTDeltaTime * gLerpSpeed);
						} else if (GameManager.Is(FGameState.InGame)) {
								mLerpThisFrame = (float)(WorldClock.ARTDeltaTime * gLerpSpeed);
						} else {
								return;
						}

						bool readyToDisable = true;
						for (int i = 0; i < mSpotlights.Count; i++) {
								Light spot = mSpotlights[i];
								if (spot.enabled) {
										if (mIsOff) {
												spot.intensity = Mathf.Lerp(spot.intensity, 0f, mLerpThisFrame);
												if (!Mathf.Approximately(spot.intensity, 0f)) {
														readyToDisable = false;
												}
										} else {
												spot.intensity = Mathf.Lerp(spot.intensity, TargetSpotIntensity, mLerpThisFrame);
												if (!Mathf.Approximately(spot.intensity, TargetSpotIntensity)) {
														readyToDisable = false;
												}
										}
										spot.range = Mathf.Lerp(spot.range, TargetSpotRange, Time.deltaTime * 2f);
								}
						}
						if (BaseLight.enabled) {
								if (mIsOff) {
										BaseLight.intensity = Mathf.Lerp(BaseLight.intensity, 0f, mLerpThisFrame);
										if (!Mathf.Approximately(BaseLight.intensity, 0f)) {
												readyToDisable = false;
										}
								} else {
										BaseLight.intensity = Mathf.Lerp(BaseLight.intensity, TargetBaseIntensity, mLerpThisFrame);
										if (!Mathf.Approximately(BaseLight.intensity, TargetBaseIntensity)) {
												readyToDisable = false;
										}
								}
								BaseLight.range = Mathf.Lerp(BaseLight.range, TargetBaseRange, mLerpThisFrame);
								if (!Mathf.Approximately(BaseLight.range, TargetBaseRange)) {
										readyToDisable = false;
								}
						}

						if (TargetBaseRange > 0f || TargetSpotRange > 0f) {
								LightCollider.enabled = true;
						} else {
								LightCollider.enabled = false;
						}

						if (readyToDisable) {
								enabled = false;
						}
				}
				#if UNITY_EDITOR
				public void DrawEditor()
				{
						if (GUILayout.Button("Save Template")) {

								if (!Manager.IsAwake <Colors>()) {
										Manager.WakeUp <Colors>("Frontiers_ArtResourceManagers");
								}

								if (Template == null) {
										Template = new WorldLightTemplate();
										Template.Spotlights = SpotlightDirection.None;
								}

								float spotlightAngle = 75f;
								float spotlightRange = 15f;
								string cookie = string.Empty;

								if (SpotlightTop.enabled) {
										Template.Spotlights |= SpotlightDirection.Top;
										spotlightAngle = SpotlightTop.spotAngle;
										spotlightRange = SpotlightTop.range;
										if (SpotlightTop.cookie != null) {
												cookie = SpotlightTop.cookie.name;
										}
								}
								if (SpotlightBottom.enabled) {
										Template.Spotlights |= SpotlightDirection.Bottom;
										spotlightAngle = SpotlightBottom.spotAngle;
										spotlightRange = SpotlightBottom.range;
										if (SpotlightBottom.cookie != null) {
												cookie = SpotlightBottom.cookie.name;
										}
								}
								if (SpotlightLeft.enabled) {
										Template.Spotlights |= SpotlightDirection.Left;
										spotlightAngle = SpotlightLeft.spotAngle;
										spotlightRange = SpotlightLeft.range;
										if (SpotlightLeft.cookie != null) {
												cookie = SpotlightLeft.cookie.name;
										}
								}
								if (SpotlightRight.enabled) {
										Template.Spotlights |= SpotlightDirection.Right;
										spotlightAngle = SpotlightRight.spotAngle;
										spotlightRange = SpotlightRight.range;
										if (SpotlightRight.cookie != null) {
												cookie = SpotlightRight.cookie.name;
										}
								}
								if (SpotlightFront.enabled) {
										Template.Spotlights |= SpotlightDirection.Front;
										spotlightAngle = SpotlightFront.spotAngle;
										spotlightRange = SpotlightFront.range;
										if (SpotlightFront.cookie != null) {
												cookie = SpotlightFront.cookie.name;
										}
								}
								if (SpotlightBack.enabled) {
										Template.Spotlights |= SpotlightDirection.Back;
										spotlightAngle = SpotlightBack.spotAngle;
										spotlightRange = SpotlightBack.range;
										if (SpotlightBack.cookie != null) {
												cookie = SpotlightBack.cookie.name;
										}
								}

								Template.Cookie = cookie;

								if (BaseLight.enabled) {
										Template.BaseLight = true;
								} else {
										Template.BaseLight = false;
								}

								if (string.IsNullOrEmpty(Template.Color1)) {
										Template.Color1 = Colors.Get.GetOrCreateColor(BaseLight.color, 10, name + "Color");
								}
								if (EditorColor != Color.black) {
										Template.Color2 = Colors.Get.GetOrCreateColor(EditorColor, 10, name + "Color");
								}

								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor();

								Mods.Get.Editor.SaveMod <WorldLightTemplate>(Template, "Light", name);
						}
				}
				#endif
				public void TurnOff()
				{
						mIsOff = true;
				}

				public void Enable(bool isEnabled)
				{
						if (SpotlightTop != null) {
								SpotlightTop.enabled = isEnabled;
						}
						if (SpotlightBottom != null) {
								SpotlightBottom.enabled = isEnabled;
						}
						if (SpotlightFront != null) {
								SpotlightFront.enabled = isEnabled;
						}
						if (SpotlightBack != null) {
								SpotlightBack.enabled = isEnabled;
						}
						if (SpotlightLeft != null) {
								SpotlightLeft.enabled = isEnabled;
						}
						if (SpotlightRight != null) {
								SpotlightRight.enabled = isEnabled;
						}
						if (BaseLight != null) {
								BaseLight.enabled = isEnabled;
						}
				}
				#if UNITY_EDITOR
				public Color EditorColor;
				#endif
				protected List <Light> mSpotlights = new List <Light>();
				protected float mTargetHeatIntensity;
				protected float mTargetBaseRange;
				protected float mTargetSpotRange;
				protected float mTargetBaseIntensity;
				protected float mTargetSpotIntensity;
				protected float mMasterBrightness;
				protected Color mTargetColor;
				protected bool mLightEnabled = false;
				protected bool mIsOff = false;
				protected bool mDestroyed = false;
		}
}
