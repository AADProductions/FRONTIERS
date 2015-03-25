using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIStatusKeeper : MonoBehaviour
		{
				public StatusKeeper Keeper;
				public float PingValue;
				public UIPanel StatusKeeperPanel;
				public UISprite IconSprite;
				public UISprite PingSprite;
				public UISprite IconSpriteOverlay;
				public UISprite StateIconSprite;
				public UISprite IconBorder;
				public int DisplayPosition;
				public float PositionScale = 1f;
				public GUIButtonHover ButtonHover;
				public GameObject ActiveObjectsParent;
				public BoxCollider Collider;
				public Transform tr;
				public GUIStatusKeeper Neighbor;

				public Vector3 TargetPosition {
						get {
								if (Neighbor != null) {
										BasePosition = Neighbor.tr.localPosition;
										BasePosition.x = BasePosition.x + ((Neighbor.MeterSize * Neighbor.TargetScale) * PositionScale);
								} else {
										if (PositionScale < 0f) {
												BasePosition.x = (MeterSize * TargetScale) * PositionScale;
										} else {
												BasePosition.x = 0f;
										}
								}
								return BasePosition;
								//float xVal = BasePosition.x + (MeterSize * TargetScale);// * DisplayPosition;
								//return new Vector3(xVal, BasePosition.y, BasePosition.z);
						}
				}

				public Vector3 Offset;
				public Vector3 BasePosition;
				public float MeterSize;
				public float FlowSize;
				public float SymptomSize;
				public float TargetScaleMin;
				public float TargetScaleMax;
				public float TargetScale;
				public Color TargetColor;
				public Color CurrentColor;
				public GameObject StatusFlowPrefab;
				public GameObject ConditionSymptomPrefab;
				public List <GUIStatusFlow> Flows;
				public List <GUIStatusCondition> SymptomDisplays = new List <GUIStatusCondition>();

				public void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, Camera NGUICamera, int flag)
				{
						FrontiersInterface.Widget w = new FrontiersInterface.Widget(flag);
						w.SearchCamera = NGUICamera;
						w.BoxCollider = Collider;
						currentObjects.Add(w);
						for (int i = 0; i < Flows.Count; i++) {
								w.BoxCollider = Flows [i].GetComponent <BoxCollider> ();
								currentObjects.Add(w);
						}
						for (int i = 0; i < SymptomDisplays.Count; i++) {
								w.BoxCollider = SymptomDisplays [i].GetComponent <BoxCollider> ();
								currentObjects.Add(w);
						}
				}

				public void OnClickStatusKeeper()
				{
						//GUIPlayerStatusInterface.Get.PostInfo(UICamera.hoveredObject, Keeper.CurrentDescription);
						//GUIManager.PostLongFormIntrospection(Keeper.CurrentDescription, true, true);
				}

				public float StateIconOffset {
						get {
								return MeterSize;// * TargetScale;//just in case we need it later
						}
				}

				public float FlowOffset {
						get {
								return StateIconOffset;
						}
				}

				public float SymptomOffset {
						get {
								return FlowOffset + (FlowSize * Flows.Count);
						}
				}

				public void Awake()
				{
						Keeper = null;
						ButtonHover = gameObject.GetOrAdd <GUIButtonHover>();
						ButtonHover.OnButtonHover += OnButtonHover;
						tr = transform;
						Collider = gameObject.GetComponent <BoxCollider>();
				}

				public void OnButtonHover()
				{
						GUIPlayerStatusInterface.Get.PostInfo(UICamera.hoveredObject, Keeper.CurrentDescription);
				}

				public void Initialize(StatusKeeper newKeeper, GUIStatusKeeper neighbor, int displayPosition, float iconScale)
				{
						Keeper = newKeeper;
						Neighbor = neighbor;
						DisplayPosition = displayPosition;
						TargetScaleMin = iconScale;
						TargetScaleMax = iconScale * 2f;
						TargetScale = iconScale;

						if (!mInitialized) {
								//if we've initialized before we can skip all this other stuff
								//we're just getting reset because a status keeper was added or removed
								mRecentChange = 0f;
								Keeper.Ping = false;
								IconSprite.atlas = Mats.Get.ConditionIconsAtlas;//TODO implement atlas selection support
								IconSprite.spriteName = newKeeper.IconName;
								gameObject.name = newKeeper.Name;
								transform.localPosition = TargetPosition;
								//TODO get colors from Colors
								IconSpriteOverlay.alpha = 0.35f;
								TargetColor = Colors.Alpha(Colors.BlendThree(Keeper.LowColorValue, Keeper.MidColorValue, Keeper.HighColorValue, Keeper.NormalizedValue), 1f);
								CurrentColor = TargetColor;
								IconBorder.color = CurrentColor;
						}

						mInitialized = true;
				}

				int mUpdatePing = 0;
				int mUpdateSymptoms = 0;
				int mUpdateFlows = 0;

				public void Update()
				{	
						if (Keeper == null || !Keeper.Initialized)
								return;

						//update the size based on the urgency
						TargetScale = Mathf.Lerp(TargetScaleMin, TargetScaleMax, Keeper.NormalizedUrgency);

						if (TargetPosition != tr.localPosition) {
								tr.localPosition = Vector3.Lerp(tr.localPosition, TargetPosition, 0.125f);
						}
						if (GUIManager.Get.ActiveButton != ButtonHover) {
								if (tr.localScale.x != TargetScale) {
										tr.localScale = Vector3.Lerp(tr.localScale, Vector3.one * TargetScale, 0.25f);
								}
						}

						mUpdatePing++;
						mUpdateSymptoms++;
						mUpdateFlows++;

						if (mUpdatePing > 5) {
								UpdatePing();
								mUpdatePing = 0;
						}
						if (mUpdateSymptoms > 10) {
								UpdateSymptoms();
								UpdateColor();
								mUpdateSymptoms = 0;
						}
						if (mUpdateFlows > 30) {
								UpdateFlows();
								mUpdateFlows = 0;
						}
						if (Keeper.ActiveState.StateName != mLastUpdateState) {	//only need to do this if/when the state changes
								UpdateStateIcon();
						}
						mLastUpdateState = Keeper.ActiveState.StateName;

						//when the game is paused these don't get updated
						//so do it manually
						if (GameManager.Is(FGameState.GamePaused)) {
								StatusKeeperPanel.LateUpdate();
						}
				}

				public void UpdatePing()
				{
						if (Keeper.LastChangeType == StatusSeekType.Positive) {
								PingSprite.color = Colors.Get.GenericLowValue;
						} else {
								PingSprite.color = Colors.Get.GenericHighValue;
						}
						mRecentChange = Mathf.Lerp(mRecentChange, 0f, (float)WorldClock.RTDeltaTimeSmooth * 3.5f);
						mRecentChange += Keeper.ChangeLastUpdate;
						//if the keeper has been pinged
						//set the recent change to 1
						if (Keeper.Ping) {
								if (Keeper.LastChangeType == StatusSeekType.Positive) {
										PingSprite.color = Colors.Get.GenericLowValue;
								} else {
										PingSprite.color = Colors.Get.GenericHighValue;
								}
								mRecentChange = 1f;
								Keeper.Ping = false;
						}
						PingSprite.alpha = mRecentChange;
				}

				public void UpdateColor()
				{
						//TODO update color
						TargetColor = Colors.BlendThree(Keeper.LowColorValue, Keeper.MidColorValue, Keeper.HighColorValue, Keeper.NormalizedValue);
						CurrentColor = Color.Lerp(CurrentColor, TargetColor, 0.25f);
						IconSprite.color = CurrentColor;
						if (Keeper.Value > 1) {
								IconBorder.color = Colors.Brighten(CurrentColor);
						} else if (Keeper.Value < 1) {
								IconBorder.color = Colors.Darken(CurrentColor);
						} else {
								IconBorder.color = CurrentColor;
						}
				}

				public void UpdateSymptoms()
				{
						for (int i = 0; i < Keeper.Conditions.Count; i++) {
								if (Keeper.Conditions[i] != null && !Keeper.Conditions[i].HasExpired) {
										bool alreadyExists = false;
										for (int j = 0; j < SymptomDisplays.Count; j++) {
												if (SymptomDisplays[j].condition == Keeper.Conditions[i]) {
														alreadyExists = true;
														break;
												}
										}
										if (!alreadyExists) {
												GameObject newConditionSymptomObject = NGUITools.AddChild(ActiveObjectsParent, ConditionSymptomPrefab);
												GUIStatusCondition newGUISymptom = newConditionSymptomObject.GetComponent <GUIStatusCondition>();
												newGUISymptom.Initialize(Keeper.Conditions[i], Keeper.Name);
												SymptomDisplays.Add(newGUISymptom);
										}
								}
						}
						//clear out finished conditions
						//refresh existing conditions
						int displayOrder = 0;
						for (int i = SymptomDisplays.Count - 1; i >= 0; i--) {
								if (SymptomDisplays[i] == null) {//TODO move into spawn pool
										SymptomDisplays.RemoveAt(i);
								} else {
										SymptomDisplays[i].DisplayOffset = SymptomOffset;
										SymptomDisplays[i].DisplaySize = SymptomSize;
										SymptomDisplays[i].DisplayPosition = displayOrder;
										displayOrder++;
								}
						}
				}

				public void UpdateFlows()
				{	
						//add new flows
						for (int i = 0; i < Keeper.StatusFlows.Count; i++) {
								if (Keeper.StatusFlows[i] != null && Keeper.StatusFlows[i].HasEffect) {	//see if we've already made one
										bool alreadyExists = false;
										for (int j = 0; j < Flows.Count; j++) {
												if (Flows[j].Flow == Keeper.StatusFlows[i]) {
														alreadyExists = true;
														break;
												}					   
										}
										if (!alreadyExists) {
												GameObject newGUIStatusFlowGameObject = NGUITools.AddChild(ActiveObjectsParent, StatusFlowPrefab);
												GUIStatusFlow newGUIStatusFlow = newGUIStatusFlowGameObject.GetComponent <GUIStatusFlow>();
												newGUIStatusFlow.ParentStatusKeeper = this;
												newGUIStatusFlow.Initialize(Keeper.StatusFlows[i]);
												Flows.Add(newGUIStatusFlow);
										}
								}
						}
						//clear out finished flows
						//refresh existing flows
						int displayOrder = 0;
						for (int i = Flows.Count - 1; i >= 0; i--) {
								if (Flows[i] == null) {//TODO move into spawn pool
										Flows.RemoveAt(i);
								} else {
										Flows[i].DisplayOffset = MeterSize;
										Flows[i].DisplaySize = FlowSize;
										Flows[i].DisplayPosition = displayOrder;
										displayOrder++;
								}
						}
				}

				public void UpdateConditions()
				{

				}

				public void UpdateStateIcon()
				{
						if (string.IsNullOrEmpty(Keeper.ActiveState.StateIconName)) {
								StateIconSprite.enabled = false;
						} else {
								//TODO tie to name
								StateIconSprite.atlas = Mats.Get.ConditionIconsAtlas;
								StateIconSprite.spriteName = Keeper.ActiveState.StateIconName;
								StateIconSprite.enabled = true;
								StateIconSprite.transform.position = new Vector3(0f, StateIconOffset, 0f);
								if (string.IsNullOrEmpty(Keeper.ActiveState.StateIconColor)) {
										StateIconSprite.color = Colors.Get.ByName(Keeper.ActiveState.StateIconColor);
								} else {
										StateIconSprite.color = Color.white;
								}
						}
				}

				protected float mRecentChange = 0f;
				protected string mLastUpdateState = string.Empty;
				protected bool mInitialized = false;
		}
}