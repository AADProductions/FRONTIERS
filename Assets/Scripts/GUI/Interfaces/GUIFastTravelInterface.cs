using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
		public class GUIFastTravelInterface : PrimaryInterface
		{
				public UISlider TimeScaleTravelSlider;
				public float TimeScaleTravelMin = 0.05f;
				public float TimeScaleTravelMax = 1f;
				public float ScrollWheelSensitivity = 0.1f;
				public GUICircleBrowser CurrentBrowser;

				public override void Start()
				{
						Filter = InterfaceActionType.NoAction;
						Behavior = PassThroughBehavior.InterceptByFocus;
						FilterExceptions = InterfaceActionType.FlagsAll;

						UserActions.Filter = UserActionType.FlagsActions;
						UserActions.FilterExceptions = UserActionType.FlagsAllButActions;
						UserActions.Behavior = PassThroughBehavior.InterceptByFocus;

						Subscribe(InterfaceActionType.SelectionNext, new ActionListener(SelectionNext));
						Subscribe(InterfaceActionType.SelectionPrev, new ActionListener(SelectionPrev));
						UserActions.Subscribe(UserActionType.ItemUse, ItemUse);

						HideCrosshair = false;
						ShowCursor = false;

						base.Start();
				}

				public bool SelectionNext(double timeStamp)
				{
						if (Maximized) {
								TimeScaleTravelSlider.sliderValue += ScrollWheelSensitivity;
						}
						return true;
				}

				public bool SelectionPrev(double timeStamp)
				{
						if (Maximized) {
								TimeScaleTravelSlider.sliderValue -= ScrollWheelSensitivity;
						}
						return true;
				}

				public bool ItemUse(double timeStamp)
				{
						if (Maximized) {
								//man this is really spread out... i might want to pull projections into this class
								Player.Local.Projections.TryToSelectArrow();
						}
						return true;
				}

				public override bool ActionCancel(double timeStamp)
				{
						//Debug.Log ("Canceling travel from interface");
						TravelManager.Get.CancelTraveling();
						Minimize();
						return true;
				}

				public override bool ShowQuickslots {
						get {
								if (Maximized) {
										return TravelManager.Get.State == FastTravelState.WaitingForNextChoice;
								}
								return true;
						}
						set {
								//do nothing
						}
				}
				/*public void OnFocusBrowserChoice(int focusIndex)
				{		
						if (focusIndex < 0) {
								ActionCancel(WorldClock.AdjustedRealTime);
						} else {
								KeyValuePair <PathMarkerInstanceTemplate,int> focusPathMarker = mLastDisplayedPathMarkers[focusIndex];
								//Player.Local.Projections.HighlightLocation (focusPathMarker);
								//TravelManager.Get.ConsiderChoice(focusPathMarker);
						}
				}*/
				public override bool Maximize()
				{
						if (!base.Maximize()) {
								return false;
						}
						foreach (UIPanel masterPanel in MasterPanels) {
								masterPanel.enabled = true;
						}

						/*if (CurrentBrowser != null && !CurrentBrowser.IsFinished) {
								CurrentBrowser.Finish();
						}
			
						CircleBrowserResult editObject = new CircleBrowserResult();
						editObject.FocusListener = gameObject;
						editObject.OnFocusMessage = "OnFocusBrowserChoice";
						editObject.RotateToFocus = false;
						editObject.CenterSize = 175f;
						editObject.Offset = new Vector3(0f, -225f, 0f);

						for (int i = 0; i < mLastDisplayedPathMarkers.Count; i++) {
								//todo add arrows etc
								CircleBrowserObjectTemplate template = new CircleBrowserObjectTemplate();
								template.OnFocusTitle = mLastDisplayedPathMarkers[i].Key.ParentPath.Name;
								editObject.Objects.Add(template);
						}

						foreach (UIPanel masterPanel in MasterPanels) {
								masterPanel.enabled = true;
						}*/

						TimeScaleTravelSlider.gameObject.SetLayerRecursively(Globals.LayerNumGUIRaycast);
				
						/*GameObject childEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUICircleBrowserGeneric, false);
						CurrentBrowser = childEditor.GetComponent <GUICircleBrowser>();
						GUIManager.SendEditObjectToChildEditor <CircleBrowserResult>(new ChildEditorCallback <CircleBrowserResult>(ReceiveFromChildEditor), childEditor, editObject);
						//TravelManager.Get.ConsiderChoice(mLastDisplayedPathMarkers[0]);//look at the first choice*/
						return true;
				}
				/*public void ReceiveFromChildEditor(CircleBrowserResult result, IGUIChildEditor <CircleBrowserResult> childEditor)
				{
						try {
								Debug.Log("receiving from child editor - click index " + result.ClickIndex);
								if (!result.Cancelled) {
										//TravelManager.Get.MakeChoice(mLastDisplayedPathMarkers[result.ClickIndex]);
								}
						} catch (Exception e) {
								Debug.LogError("Exception in fast travel interface: " + e.ToString());
								TravelManager.Get.State = FastTravelState.Finished;
						}
				}*/
				public void OnChangeTimeScaleTravel()
				{
						TravelManager.Get.TimeScaleTravel = (TimeScaleTravelMin + ((TimeScaleTravelMax - TimeScaleTravelMin) * TimeScaleTravelSlider.sliderValue));
				}

				public override bool LoseFocus()
				{
						if (base.LoseFocus()) {
								Minimize();
						}
						return false;
				}

				public override bool Minimize()
				{
						if (base.Minimize()) {
								//Debug.Log("Minimizing fast travel manager");
								foreach (UIPanel masterPanel in MasterPanels) {
										masterPanel.enabled = false;
								}
								/*if (CurrentBrowser != null) {
										CurrentBrowser.Finish();
								}*/
								TimeScaleTravelSlider.gameObject.SetLayerRecursively(Globals.LayerNumGUIRaycastIgnore);
								TravelManager.Get.CancelTraveling();
								return true;
						}
						return false;
				}
				/*public void AddAvailablePathMarkers(Dictionary <PathMarkerInstanceTemplate,int> availablePathMarkers)
				{
						mLastDisplayedPathMarkers.Clear();
						foreach (KeyValuePair <PathMarkerInstanceTemplate,int> pair in availablePathMarkers) {
								mLastDisplayedPathMarkers.Add(pair);
						}
				}

				protected List <KeyValuePair<PathMarkerInstanceTemplate,int>> mLastDisplayedPathMarkers = new List<KeyValuePair<PathMarkerInstanceTemplate, int>>();*/
		}
}
