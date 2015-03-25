using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.GUI
{
		public class GUIMissionBrowser : GUIBrowserSelectView <MissionState>
		{
				[BitMask(typeof(MissionStatus))]
				public MissionStatus DisplayStatus;
				public bool CreateEmptyDivider;
				public bool CreateActiveDivider;
				public bool CreateCompletedDivider;
				GUITabPage TabPage;

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						//this will get everything on all tabs
						GUILogInterface.Get.GetActiveInterfaceObjects(currentObjects, flag);
				}

				public override void WakeUp()
				{
						base.WakeUp();

						TabPage = gameObject.GetComponent <GUITabPage>();
						TabPage.OnDeselected += OnDeselected;
				}

				public void OnDeselected()
				{
						if (HasFocus) {
								GUIManager.Get.ReleaseFocus(this);
								mSelectedObject = null;
						}
				}

				public UILabel MissionStatusLabel;

				public override void PushEditObjectToNGUIObject()
				{
						CreateEmptyDivider = true;
						CreateActiveDivider = false;
						base.PushEditObjectToNGUIObject();
				}

				public override void Start()
				{
						base.Start();

						//Subscribe (InterfaceActionType.ToggleLog, ActionCancel);
						//we're a parent of the log
						NGUICamera = GUIManager.Get.PrimaryCamera;
						Player.Get.AvatarActions.Subscribe(AvatarAction.MissionUpdated, new ActionListener(MissionUpdated));
				}

				public bool MissionUpdated(double timeStamp)
				{
						if (!TabPage.Selected)
								return true;

						if (HasFocus) {
								if (!mRefreshingOverTime) {
										mRefreshingOverTime = true;
										StartCoroutine(RefreshOverTime());
								}
						}
						return true;
				}

				protected bool mRefreshingOverTime = false;

				public override IEnumerable <MissionState> FetchItems()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						return Missions.Get.MissionStatesByStatus(DisplayStatus).AsEnumerable();
				}

				protected IEnumerator RefreshOverTime()
				{
						while (Missions.TryingToComplete) {
								yield return null;
						}

						if (!TabPage.Selected) {
								yield break;
						}

						if (HasFocus) {
								try {
									IEnumerable <MissionState> missions = Missions.Get.MissionStatesByStatus(DisplayStatus).AsEnumerable();
									ReceiveFromParentEditor(missions);
									PushSelectedObjectToViewer();
									if (mBrowserObjectsList.Count == 0) {
											MissionStatusLabel.enabled = true;
									} else {
											MissionStatusLabel.enabled = false;
									}
								}
								catch (Exception e) {
										Debug.Log("Exception in mission browser, proceeding normally: " + e.ToString());
								}
						}
						mRefreshingOverTime = false;
						yield break;
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(MissionState editObject)
				{
						CreateEmptyDivider = false;

						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						GUIGenericBrowserObject missionBrowserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();

						newBrowserObject.AutoSelect = true;
			
						missionBrowserObject.EditButton.target = this.gameObject;
						missionBrowserObject.EditButton.functionName = "OnClickBrowserObject";

						string missionText = editObject.Title + " - ";

						missionBrowserObject.Icon.atlas = Mats.Get.IconsAtlas;
						missionBrowserObject.Icon.spriteName = "IconMission";

						if (editObject.GetPlayerAttention) {
								missionBrowserObject.BackgroundHighlight.enabled = true;
								missionBrowserObject.BackgroundHighlight.color = Colors.Get.GeneralHighlightColor;
						} else {
								missionBrowserObject.BackgroundHighlight.enabled = false;
						}

						Color missionColor = Colors.Get.MessageInfoColor;
						Color nameColor = Color.white;
						if (editObject.ObjectivesCompleted) {
								CreateCompletedDivider = true;
								if (Flags.Check((uint)editObject.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny)) {
										//Debug.Log ("We've failed the mission");
										missionBrowserObject.name = "e_" + editObject.Name;
										missionColor = Colors.Get.MessageDangerColor;
										missionText += "(Failed)";
								} else {
										missionBrowserObject.name = "d_" + editObject.Name;
										missionColor = Colors.Get.MessageSuccessColor;
										missionText += "(Completed)";
								}
						} else {
								//Debug.Log ("We're not completed");
								if (Flags.Check((uint)editObject.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny)) {
										missionColor = Colors.Get.MessageDangerColor;
										missionText += "(Failed)";
										missionBrowserObject.name = "g_" + editObject.Name;
								} else if (Flags.Check((uint)editObject.Status, (uint)MissionStatus.Ignored, Flags.CheckType.MatchAny)) {
										CreateCompletedDivider = true;
										missionColor = Colors.Get.MessageWarningColor;
										missionText += "(Ignored)";
										missionBrowserObject.name = "f_" + editObject.Name;
								} else {
										CreateActiveDivider = true;
										missionBrowserObject.name = "b_" + editObject.Name;
										//the quest is active but not completed
										int numObjectivesOutstanding = 0;
										for (int i = 0; i < editObject.Objectives.Count; i++) {
												if (editObject.Objectives[i].Hidden || editObject.Objectives[i].Completed) {
														//doesn't count towards outstanding tasks
														continue;
												}
												if (Flags.Check((uint)editObject.Objectives[i].Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {
														numObjectivesOutstanding++;
												}
										}
										if (numObjectivesOutstanding > 1) {
												missionText += Colors.ColorWrap("(" + numObjectivesOutstanding.ToString() + " objectives outstanding)", Colors.Dim (nameColor));
										} else {
												missionText += Colors.ColorWrap("(" + numObjectivesOutstanding.ToString() + " objective outstanding)", Colors.Dim (nameColor));
										}
								}
						}

						missionBrowserObject.Name.text = missionText;
						missionBrowserObject.Name.color = nameColor;
						missionBrowserObject.Background.color = Colors.Darken (missionColor);
						missionBrowserObject.Icon.color = missionColor;
						missionBrowserObject.MiniIcon.enabled = false;
						missionBrowserObject.MiniIconBackground.enabled = false;
			
						missionBrowserObject.Initialize(editObject.Name);
						missionBrowserObject.Refresh();
			
						return newBrowserObject;
				}

				public override void CreateDividerObjects()
				{
						GUIGenericBrowserObject dividerObject = null;
						IGUIBrowserObject newDivider = null;

						if (CreateEmptyDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "a_empty";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "You have no missions";
								dividerObject.Initialize("Divider");
						}

						if (CreateActiveDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "a_active";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Active Missions:";
								dividerObject.Initialize("Divider");
						}

						if (CreateCompletedDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "c_completed";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Completed Missions:";
								dividerObject.Initialize("Divider");
						}
				}

				protected override void RefreshEditObjectToBrowserObject(MissionState editObject, IGUIBrowserObject browserObject)
				{
						//		GUIGenericBrowserObject missionBrowserObject = browserObject.GetComponent <GUIGenericBrowserObject> ( );
						//		
						//		missionBrowserObject.EditButton.target 				= this.gameObject;
						//		missionBrowserObject.EditButton.functionName		= "OnClickBrowserObject";
						//		
						//		if (editObject.Props.Status == MissionStatus.Active)
						//		{
						//			missionBrowserObject.BackgroundHighlight.enabled 	= true;
						//			missionBrowserObject.BackgroundHighlight.color		= Colors.Get.SuccessHighlightColor;
						//		}
						//		else if (editObject.Props.Status == MissionStatus.Failed)
						//		{
						//			missionBrowserObject.BackgroundHighlight.enabled 	= true;
						//			missionBrowserObject.BackgroundHighlight.color		= Colors.Get.WarningHighlightColor;			
						//		}
						//		else
						//		{
						//			missionBrowserObject.BackgroundHighlight.enabled 	= false;
						//		}		
				}

				public override void PushSelectedObjectToViewer()
				{
						mSelectedObject.GetPlayerAttention = false;
						//Missions.Get.MissionStateByName (mSelectedObject.Name);
						List <string> detailText = new List <string>();
						detailText.Add(mSelectedObject.Description);

						Color missionColor = Colors.Get.MessageInfoColor;
						if (mSelectedObject.ObjectivesCompleted) {
								if (Flags.Check((uint)mSelectedObject.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny)) {
										missionColor = Colors.Get.MessageDangerColor;
								} else {
										missionColor = Colors.Get.MessageSuccessColor;
								}
						} else {
								if (Flags.Check((uint)mSelectedObject.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny)) {
										missionColor = Colors.Get.MessageDangerColor;
								} else if (Flags.Check((uint)mSelectedObject.Status, (uint)MissionStatus.Ignored, Flags.CheckType.MatchAny)) {
										missionColor = Colors.Get.MessageWarningColor;
								}
						}

						detailText.Add("_");//divider

						foreach (ObjectiveState objectiveToDisplay in mSelectedObject.Objectives) {
								if (!objectiveToDisplay.Hidden && objectiveToDisplay.Status != MissionStatus.Dormant) {
										Color objectiveColor = Colors.Get.MessageInfoColor;
										if (objectiveToDisplay.Completed) {
												//Debug.Log ("Objective " + objectiveToDisplay.Name + " is completed");
												if (Flags.Check((uint)objectiveToDisplay.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny)) {
														objectiveColor = Colors.Get.MessageDangerColor;
												} else {
														//Debug.Log ("I'm turning you green");
														objectiveColor = Colors.Get.MessageSuccessColor;
												}
										} else if (objectiveToDisplay.Status != MissionStatus.Dormant) {
												//Debug.Log ("It's not completed, but it's not dormant either");
												if (Flags.Check((uint)objectiveToDisplay.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny)) {
														objectiveColor = Colors.Get.MessageDangerColor;
												} else if (Flags.Check((uint)objectiveToDisplay.Status, (uint)MissionStatus.Ignored, Flags.CheckType.MatchAny)) {
														objectiveColor = Colors.Get.MessageWarningColor;
												}
										}

										if (objectiveToDisplay.Completed) {
												detailText.Add(Colors.ColorWrap(objectiveToDisplay.Name.ToUpper(), objectiveColor));
										} else {
												detailText.Add(Colors.ColorWrap(objectiveToDisplay.Name.ToUpper() + ": ", Color.white) + Colors.ColorWrap(objectiveToDisplay.Description, objectiveColor));
										}
								}
						}

						string finalDetailText = string.Join("\n", detailText.ToArray());
						GUIDetailsPage.Get.DisplayDetail(
								this,
								Frontiers.Data.GameData.InterpretScripts(mSelectedObject.Title, Profile.Get.CurrentGame.Character, null),
								Frontiers.Data.GameData.InterpretScripts(finalDetailText, Profile.Get.CurrentGame.Character, null),
								"IconMission",
								Mats.Get.IconsAtlas,
								missionColor,
								Color.white);
				}
		}
}