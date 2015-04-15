using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUIPlantsBrowser : GUIBrowserSelectView <Plant>
		{
				GUITabPage TabPage;
				public GUITabs SubSelectionTabs;
				public int NumAboveGroundPlantsEncountered;
				public int NumBelowGroundPlantsEncountered;
				public ClimateType SelectedClimate = ClimateType.Arctic;
				public TimeOfYear SelectedSeasonality = TimeOfYear.SeasonSummer;
				public bool SelectedAboveGround = true;
				protected bool mRefreshingOverTime = false;

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						if (flag < 0) { flag = GUILogInterface.Get.GUIEditorID; }
						//this will get everything on all tabs
						GUILogInterface.Get.GetActiveInterfaceObjects(currentObjects, flag);
						base.GetActiveInterfaceObjects(currentObjects, flag);
				}

				public override void WakeUp()
				{
						base.WakeUp();

						SubSelectionTabs = gameObject.GetComponent <GUITabs>();
						SubSelectionTabs.OnSetSelection += OnSetSelection;
						mPlantDoppleganger.PackName = "Plants";
						mPlantDoppleganger.PrefabName = "WorldPlant";
						mPlantDoppleganger.State = "Raw";
						TabPage = gameObject.GetComponent <GUITabPage>();
						TabPage.OnDeselected += OnDeselected;
				}

				public override void Start()
				{
						base.Start();
						//we're a parent of the log
						NGUICamera = GUIManager.Get.PrimaryCamera;
				}

				public void OnDeselected()
				{
						if (HasFocus) {
								GUIManager.Get.ReleaseFocus(this);
						}
				}

				public override IEnumerable <Plant> FetchItems()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						return Plants.Get.KnownPlants(SelectedClimate, TimeOfYear.AllYear, SelectedAboveGround, true).AsEnumerable();
				}

				public void OnSetSelection()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return;
						}
						switch (SubSelectionTabs.SelectedTab.ToLower()) {
								case "arctic":
										SelectedClimate = ClimateType.Arctic;
										break;

								case "temperate":
								default:
										SelectedClimate = ClimateType.Temperate;
										break;

								case "tropical coast":
										SelectedClimate = ClimateType.TropicalCoast;
										break;

								case "desert":
										SelectedClimate = ClimateType.Desert;
										break;

								case "wetlands":
										SelectedClimate = ClimateType.Wetland;
										break;

								case "rainforest":
										SelectedClimate = ClimateType.Rainforest;
										break;
						}

						IEnumerable <Plant> plants = Plants.Get.KnownPlants(SelectedClimate, SelectedSeasonality, SelectedAboveGround, false).AsEnumerable();
						ReceiveFromParentEditor(plants);
				}

				public override void PushEditObjectToNGUIObject()
				{
						NumAboveGroundPlantsEncountered = 0;
						NumBelowGroundPlantsEncountered = 0;
						base.PushEditObjectToNGUIObject();
				}

				public override void CreateDividerObjects()
				{
						GUIGenericBrowserObject dividerObject = null;
						IGUIBrowserObject newDivider = null;

						newDivider = CreateDivider();
						dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
						dividerObject.name = "a_aboveGround";
						dividerObject.UseAsDivider = true;
						dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
						dividerObject.Name.text = "You know of " + NumAboveGroundPlantsEncountered.ToString() + " / " + Plants.Get.TotalPlantsInClimate(SelectedClimate, true).ToString() + " above-ground plants in this climate";
						dividerObject.Initialize("Divider");

						newDivider = CreateDivider();
						dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
						dividerObject.name = "c_belowGround";
						dividerObject.UseAsDivider = true;
						dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
						dividerObject.Name.text = "You know of " + NumBelowGroundPlantsEncountered.ToString() + " / " + Plants.Get.TotalPlantsInClimate(SelectedClimate, false).ToString() + " below-ground plants in this climate";
						dividerObject.Initialize("Divider");

				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject (Plant editObject)
				{
						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						newBrowserObject.name = editObject.CommonName + "_" + editObject.Seasonality.ToString();
						GUIGenericBrowserObject plantBrowserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();

						#if UNITY_EDITOR
						if (VRManager.VRDeviceAvailable | VRManager.VRTestingMode) {
						#else
						if (VRManager.VRDeviceAvailable) {
						#endif
								plantBrowserObject.AutoSelect = false;
						} else {
								plantBrowserObject.AutoSelect = true;
						}

						plantBrowserObject.EditButton.target = this.gameObject;
						plantBrowserObject.EditButton.functionName = "OnClickBrowserObject";
						plantBrowserObject.Icon.atlas = Mats.Get.IconsAtlas;
						plantBrowserObject.Icon.spriteName = "PlantIcon";

						if (editObject.AboveGround) {
								NumAboveGroundPlantsEncountered++;
								newBrowserObject.name = "b_" + newBrowserObject.name;
						} else {
								NumBelowGroundPlantsEncountered++;
								newBrowserObject.name = "d_" + newBrowserObject.name;
						}

						plantBrowserObject.Name.text = editObject.CommonName;

						Color plantColor = Colors.ColorFromString(editObject.CommonName, 100);
						if (editObject.EncounteredTimesOfYear != TimeOfYear.None) {
								newBrowserObject.name = "a_" + newBrowserObject.name;
								plantBrowserObject.Name.text += (" (Encountered " + editObject.NumTimesEncountered.ToString() + " times)");
						} else {
								newBrowserObject.name = "b_" + newBrowserObject.name;
								plantColor = Colors.Darken(plantColor);
								plantBrowserObject.Name.text += " (Never encountered)";
								//plantBrowserObject.Name.color = Colors.Darken(plantBrowserObject.Name.color);
						}
						plantBrowserObject.BackgroundHighlight.enabled = false;
						plantBrowserObject.Icon.color = plantColor;
			
						plantBrowserObject.Initialize(editObject.Name);
						plantBrowserObject.Refresh();
			
						return newBrowserObject;
				}

				public void OnClickNextSeason()
				{
						switch (SelectedSeasonality) {
								case TimeOfYear.SeasonSummer:
								default:
										SelectedSeasonality = TimeOfYear.SeasonAutumn;
										break;

								case TimeOfYear.SeasonAutumn:
										SelectedSeasonality = TimeOfYear.SeasonWinter;
										break;

								case TimeOfYear.SeasonWinter:
										SelectedSeasonality = TimeOfYear.SeasonSpring;
										break;

								case TimeOfYear.SeasonSpring:
										SelectedSeasonality = TimeOfYear.SeasonSummer;
										break;
						}
						mPlantDoppleganger.TOY = SelectedSeasonality;
						PushSelectedObjectToViewer();
				}

				protected List <WIExamineInfo> mExamineInfo = new List<WIExamineInfo>();

				public override void PushSelectedObjectToViewer()
				{
						//turn this into a string builder
						Color plantColor = Colors.ColorFromString(mSelectedObject.CommonName, 100);
						List <string> detailText = new List <string>();
						mExamineInfo.Clear();

						Skill examineSkill = null;
						GenericWorldItem dopplegangerProps = null;
						if (Skills.Get.SkillByName("Gathering", out examineSkill)) {
								bool showDetails = false;
								Plants.Examine(mSelectedObject, mExamineInfo);
								detailText.Add("Viewing season " + WorldClock.TimeOfYearToString(SelectedSeasonality));
								if (Flags.Check((uint)mSelectedObject.EncounteredTimesOfYear, (uint)SelectedSeasonality, Flags.CheckType.MatchAny)
								    || examineSkill.State.NormalizedUsageLevel > Plants.MinimumGatheringSkillToRevealBasicProps) {
										showDetails = true;
										dopplegangerProps = mPlantDoppleganger;
										dopplegangerProps.Subcategory = mSelectedObject.Name;
								} else {
										detailText.Add("(You don't know what this plant looks like during this time of year.)");
								}
								detailText.Add("_");
								for (int i = 0; i < mExamineInfo.Count; i++) {
										if (examineSkill.State.NormalizedUsageLevel > mExamineInfo[i].RequiredSkillUsageLevel) {
												detailText.Add(mExamineInfo[i].StaticExamineMessage);
										} else {
												detailText.Add(mExamineInfo[i].ExamineMessageOnFail);
										}
								}
								string finalDetailText = detailText.JoinToString("\n");
								GUIDetailsPage.Get.DisplayDetail(
										this,
										mSelectedObject.CommonName,
										finalDetailText,
										"PlantIcon",
										Mats.Get.IconsAtlas,
										plantColor,
										Color.white,
										dopplegangerProps);
								if (showDetails) {
										GUIDetailsPage.Get.DisplayDopplegangerButton("Next Season", "OnClickNextSeason", gameObject);
								}
						}
				}

				protected GenericWorldItem mPlantDoppleganger = new GenericWorldItem();
		}
}