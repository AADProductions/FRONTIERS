#pragma warning disable 0219
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
	public class GUIPlayerStatusInterface : PrimaryInterface
	{
		public static GUIPlayerStatusInterface Get;
		public List <GUIStatusKeeper> StatusMeters = new List <GUIStatusKeeper>();
		public List <GUISkillObject> SkillsInUse = new List <GUISkillObject>();
		public float HeightOffset;
		public float HiddenOffset;
		public GameObject GUIStatusKeeperPrefab;
		public GameObject StatusKeeperPanel;
		public GameObject SymptomDisplayPanel;
		public GameObject SkillsInUsePanel;
		//info display
		public bool DisplayInfo = false;
		public GameObject CurrentInfoTarget;
		public float CurrentInfoTargetXOffset;
		public string CurrentInfo;
		public UILabel InfoLabel;
		public UISprite InfoSpriteShadow;
		public UISprite InfoSpriteBackground;
		public Transform InfoOffset;
		//civilization stuff
		public UILabel ActiveStatesLabel;
		public UISprite SurroundingsStatesIcon;
		public UISprite SurroundingsGlow;
		public UISprite SurroundingsShadow;
		public UIPanel MainPanel;
		public UISprite TemperatureSprite;
		public UISprite TemperatureSpriteFreezing;
		public UISprite TemperatureSpriteBurning1;
		public UISprite TemperatureSpriteBurning2;
		public UIPanel TemperatureClipPanel;
		public UIPanel SurroundingsPanel;
		public float FreezingSpriteTargetAlpha;
		public float BurningSpriteTargetAlpha;
		public Vector3 TempOffsetMin;
		public Vector3 TempOffsetMax;
		public Vector3 TargetTempOffset;
		public Vector3 StateOffsetInWild;
		public Vector3 StateOffsetInCivilization;
		public Vector3 StateOffsetInStructure;
		public Vector3 StateOffsetInSafeStructure;
		public Vector3 StateOffsetUnderground;
		public Vector3 TargetStateOffset;
		public float TemperatureRangeClipSpeed = 0.25f;
		public GUIButtonHover SurroundingsPanelHover;
		//vr stuff
		public bool VRModeHidden = false;
		public Vector2 AnchorVROffsetVisible;
		public Vector2 AnchorVROffsetHidden;

		public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
		{
			if (flag < 0) {
				flag = GUIEditorID;
			}

			if (MainPanel.enabled && !VRModeHidden) {
				FrontiersInterface.Widget w = new Widget(flag);
				w.SearchCamera = NGUICamera;
				for (int i = 0; i < StatusMeters.Count; i++) {
					StatusMeters[i].GetActiveInterfaceObjects(currentObjects, NGUICamera, flag);
				}
				w.BoxCollider = SurroundingsPanelHover.GetComponent <BoxCollider>();
				currentObjects.Add(w);
			}
		}

		public override void WakeUp()
		{
			base.WakeUp();

			Get = this;

			TargetStateOffset = StateOffsetInWild;
			TargetTempOffset = TempOffsetMin;
			SupportsControllerSearch = true;
		}

		public void Initialize()
		{		
			GenerateStatusKeepers();

			Player.Get.AvatarActions.Subscribe(AvatarAction.SkillUse, new ActionListener(SkillUse));
			Player.Get.AvatarActions.Subscribe(AvatarAction.SkillUseFail, new ActionListener(SkillUseFail));
			Player.Get.AvatarActions.Subscribe(AvatarAction.SkillUseFinish, new ActionListener(SkillUseFinish));
			Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalConditionAdd, new ActionListener(SurvivalConditionAdd));
			Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalConditionRemove, new ActionListener(SurvivalConditionRemove));

			SurroundingsPanelHover.OnButtonHover += OnHoverSurroundingsPanel;

			mInitialized = true;
		}

		public bool SurvivalConditionAdd(double timeStamp)
		{
			GenerateStatusKeepers();
			return true;
		}

		public bool SurvivalConditionRemove(double timeStamp)
		{
			GenerateStatusKeepers();
			return true;
		}

		public void GenerateStatusKeepers()
		{

			int statusKeeperIndex = -1;
			GUIStatusKeeper previsousGuiStatusKeeper = null;
			GUIStatusKeeper currentGuiStatusKeeper = null;
			for (int i = 0; i < Player.Local.Status.StatusKeepers.Count; i++) {
				StatusKeeper currentStatusKeeper = Player.Local.Status.StatusKeepers[i];
				if (currentStatusKeeper == null) {
					Debug.Log("Status keeper was null!");
					return;
				}

				bool hasExistingStatusKeeper = false;
				for (int j = 0; j < StatusMeters.Count; j++) {
					if (StatusMeters[j].Keeper == currentStatusKeeper) {
						currentGuiStatusKeeper = StatusMeters[j];
						hasExistingStatusKeeper = true;
						break;
					}
				}

				bool showStatusKeeper = currentStatusKeeper.ShowInStatusInterface;
				if (currentStatusKeeper.ShowOnlyWhenAffectedByCondition && currentStatusKeeper.Conditions.Count == 0) {
					//better make sure it has a condition
					showStatusKeeper = false;
				}

				if (showStatusKeeper) {
					//if we're showing it, bump up the index
					statusKeeperIndex++;
					//create a new status keeper if we need one
					if (!hasExistingStatusKeeper) {
						GameObject newGUIStatusKeeperGameObject = NGUITools.AddChild(StatusKeeperPanel, GUIStatusKeeperPrefab);
						currentGuiStatusKeeper = newGUIStatusKeeperGameObject.GetComponent <GUIStatusKeeper>();
						StatusMeters.Add(currentGuiStatusKeeper);
					}
					//initializing an already-initialized status keeper is fine
					currentGuiStatusKeeper.Initialize(currentStatusKeeper, previsousGuiStatusKeeper, statusKeeperIndex, 1f);//reverse the order, it will preserve the imporance on screen
					previsousGuiStatusKeeper = currentGuiStatusKeeper;
				} else if (hasExistingStatusKeeper) {
					//if we don't want to show it and it already exists
					//destroy it and remove it from the list without incrementing anything
					StatusMeters.Remove(currentGuiStatusKeeper);
					GameObject.Destroy(currentGuiStatusKeeper);
				}
			}
		}

		public void OnHoverSurroundingsPanel()
		{
			PostInfo(UICamera.hoveredObject, Player.Local.Surroundings.CurrentDescription);
		}

		protected int mUpdateSurroundings = 0;
		protected Vector2 mHiddenAnchorOffset = new Vector2(-1f, -1f);
		protected Vector3 mInfoOffset;

		public void PostInfo(GameObject target, string info)
		{
			CurrentInfoTarget = target;
			CurrentInfoTargetXOffset = target.transform.localPosition.x;
			CurrentInfo = info;
			InfoLabel.text = CurrentInfo;
			DisplayInfo = true;
			//update the box around the text to reflect its size
			Transform textTrans = InfoLabel.transform;
			Vector3 offset = textTrans.localPosition;
			Vector3 textScale = textTrans.localScale;

			// Calculate the dimensions of the printed text
			Vector3 size = InfoLabel.relativeSize;

			// Scale by the transform and adjust by the padding offset
			size.x *= textScale.x;
			size.y *= textScale.y;
			size.x += 50f;
			size.y += 50f;
			size.x += (InfoSpriteBackground.border.x + InfoSpriteBackground.border.z + (offset.x - InfoSpriteBackground.border.x) * 2f);
			size.y += (InfoSpriteBackground.border.y + InfoSpriteBackground.border.w + (-offset.y - InfoSpriteBackground.border.y) * 2f);
			size.z = 1f;

			InfoSpriteBackground.transform.localScale = size;
			InfoSpriteShadow.transform.localScale = size;
		}

		public void Update()
		{
			SurroundingsStatesIcon.transform.localPosition = Vector3.Lerp(SurroundingsStatesIcon.transform.localPosition, TargetStateOffset, (float)WorldClock.RTDeltaTime);

			TemperatureSprite.transform.localPosition = Vector3.Lerp(TemperatureSprite.transform.localPosition, TargetTempOffset, 0.125f);
			TemperatureSpriteFreezing.alpha = Mathf.Lerp(TemperatureSpriteFreezing.alpha, FreezingSpriteTargetAlpha, 0.125f);
			//burning alpha alternates
			if (BurningSpriteTargetAlpha > 0f) {
				TemperatureSpriteBurning2.alpha = Mathf.Abs(Mathf.Sin(Time.time)) * BurningSpriteTargetAlpha;
				TemperatureSpriteBurning1.alpha = BurningSpriteTargetAlpha - TemperatureSpriteBurning2.alpha;
			} else {
				TemperatureSpriteBurning2.alpha = Mathf.Lerp(TemperatureSpriteBurning2.alpha, BurningSpriteTargetAlpha, 0.125f);
				TemperatureSpriteBurning1.alpha = Mathf.Lerp(TemperatureSpriteBurning1.alpha, BurningSpriteTargetAlpha, 0.125f);
			}

			if (DisplayInfo) {
				if (UICamera.hoveredObject != CurrentInfoTarget) {
					DisplayInfo = false;
				}
				if (InfoSpriteShadow.alpha < 1f) {
					InfoSpriteShadow.alpha = Mathf.Lerp(InfoSpriteShadow.alpha, 1f, 0.25f);
					if (InfoSpriteShadow.alpha > 0.99f) {
						InfoSpriteShadow.alpha = 1f;
					}
				}
				//make sure the info doesn't overlay an icon
				float yOffset = 0f;
				for (int i = 0; i < StatusMeters.Count; i++) {
					yOffset = Mathf.Max(StatusMeters[i].SymptomOffset, yOffset);
				}
				mInfoOffset.y = yOffset + 50f;
				mInfoOffset.x = CurrentInfoTargetXOffset + 50f;
				InfoOffset.localPosition = mInfoOffset;
			} else {
				if (InfoSpriteShadow.alpha > 0f) {
					InfoSpriteShadow.alpha = Mathf.Lerp(InfoSpriteShadow.alpha, 0f, 0.25f);
					if (InfoSpriteShadow.alpha < 0.01f) {
						InfoSpriteShadow.alpha = 0f;
					}
				}
			}
			InfoLabel.alpha = InfoSpriteShadow.alpha;
			InfoSpriteBackground.alpha = InfoSpriteShadow.alpha;

			base.Update();

			if (!GameManager.Is(FGameState.InGame | FGameState.GamePaused)
			       || !Player.Local.HasSpawned
			       || PrimaryInterface.IsMaximized("WorldMap")
			       || PrimaryInterface.IsMaximized("Conversation")) {
				if (MainPanel.enabled) {
					MainPanel.enabled = false;
					for (int i = 0; i < MasterAnchors.Count; i++) {
						MasterAnchors[i].relativeOffset = mHiddenAnchorOffset;
					}
					TemperatureClipPanel.enabled = false;
					SurroundingsPanel.enabled = false;
				}
				return;
			} else {
				if (!MainPanel.enabled) {
					MainPanel.enabled = true;
					TemperatureClipPanel.enabled = true;
					SurroundingsPanel.enabled = true;
					for (int i = 0; i < MasterAnchors.Count; i++) {
						MasterAnchors[i].relativeOffset = Vector2.zero;
					}
				}
				#if UNITY_EDITOR
				if (VRManager.VRMode | VRManager.VRTestingModeEnabled) {
				#else
				if (VRManager.VRMode) {
				#endif
					for (int i = 0; i < MasterAnchors.Count; i++) {
						//mark this to skip interface objects
						VRModeHidden = ((GUIInventoryInterface.Get.Maximized && !GUIInventoryInterface.Get.VRFocusOnTabs) || GUILogInterface.Get.Maximized);
						if (VRModeHidden) {
							MasterAnchors[i].relativeOffset = Vector2.Lerp(MasterAnchors[i].relativeOffset, AnchorVROffsetHidden, 0.25f);
						} else {
							MasterAnchors[i].relativeOffset = Vector2.Lerp(MasterAnchors[i].relativeOffset, AnchorVROffsetVisible, 0.25f);
						}
					}
				}
			}

			if (!mInitialized) {
				return;
			}

			//clear out expired skills in use and set positions
			int positionInList = 0;
			for (int i = SkillsInUse.Count - 1; i >= 0; i--) {
				if (SkillsInUse[i] == null) {
					//it should be invisible now
					SkillsInUse.RemoveAt(i);
				} else {
					SkillsInUse[i].PositionInList = positionInList;
					positionInList++;
				}
			}

			if (mRefreshSkillsInUse) {
				mRefreshSkillsInUse = false;
				RefreshSkillsInUse();
			}

			mUpdateSurroundings++;
			if (mUpdateSurroundings < 30) {
				return;
			}
			mUpdateSurroundings = 0;

			switch (Player.Local.Status.LatestTemperatureExposure) {
				case TemperatureRange.A_DeadlyCold:
					TargetTempOffset = TempOffsetMin;
					FreezingSpriteTargetAlpha = 1f;
					BurningSpriteTargetAlpha = 0f;
					break;

				case TemperatureRange.B_Cold:
				default:
					TargetTempOffset = TempOffsetMin;
					FreezingSpriteTargetAlpha = 0f;
					BurningSpriteTargetAlpha = 0f;
					break;

				case TemperatureRange.C_Warm:
					TargetTempOffset = Vector3.zero;
					FreezingSpriteTargetAlpha = 0f;
					BurningSpriteTargetAlpha = 0f;
					break;

				case TemperatureRange.D_Hot:
					TargetTempOffset = TempOffsetMax;
					FreezingSpriteTargetAlpha = 0f;
					BurningSpriteTargetAlpha = 0f;
					break;

				case TemperatureRange.E_DeadlyHot:
					TargetTempOffset = TempOffsetMax;
					FreezingSpriteTargetAlpha = 0f;
					BurningSpriteTargetAlpha = 1f;
					break;
			}

			if (Player.Local.Surroundings.IsUnderground) {
				TargetStateOffset = StateOffsetUnderground;
			} else if (Player.Local.Surroundings.IsInSafeLocation) {
				TargetStateOffset = StateOffsetInSafeStructure;
			} else if (Player.Local.Surroundings.IsInsideStructure) {
				TargetStateOffset = StateOffsetInStructure;
			} else if (Player.Local.Surroundings.IsInCivilization) {
				TargetStateOffset = StateOffsetInCivilization;
			} else {
				TargetStateOffset = StateOffsetInWild;
			}
		}

		public override bool Minimize()
		{
			return true;
		}

		public override bool Maximize()
		{
			return true;
		}

		public bool SkillUseFail(double timeStamp)
		{
			mRefreshSkillsInUse = true;
			return true;
		}

		public bool SkillUse(double timeStamp)
		{
			mRefreshSkillsInUse = true;
			return true;
		}

		public bool SkillUseFinish(double timeStamp)
		{
			mRefreshSkillsInUse = true;
			return true;
		}

		public void RefreshSkillsInUse()
		{
			//Debug.Log ("Refreshing skills in use");
			foreach (Skill skillInUse in Skills.Get.SkillsInUse) {
				if (skillInUse.Usage.VisibleInInterface) {
					//do we already have a skill object displaying this?
					bool hasExisting = false;
					for (int i = SkillsInUse.Count - 1; i >= 0; i--) {
						if (SkillsInUse[i] == null) {
							SkillsInUse.RemoveAt(i);
							//if it hasn't expired then use it
						} else if (!SkillsInUse[i].IsExpired && SkillsInUse[i].name == skillInUse.name) {
							hasExisting = true;
							break;
						}
					}
					if (!hasExisting) {
						//if we don't already have one, create a new one
						GameObject newSkillInUse = NGUITools.AddChild(SkillsInUsePanel, GUIManager.Get.SkillObject);
						GUISkillObject skillObject = newSkillInUse.GetComponent <GUISkillObject>();
						skillObject.InitArgument = skillInUse.name;
						SkillsInUse.Add(skillObject);
					}
				}
			}
		}

		protected bool mRefreshSkillsInUse = false;
		protected bool mChangingCiv = false;
		protected bool mInitialized = false;
	}
}
