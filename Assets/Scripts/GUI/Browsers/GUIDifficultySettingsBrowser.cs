﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIDifficultySettingsBrowser : GUIBrowserSelectView <FieldInfo>
		{
				public GUITabPage ControllingTabPage;
				public UIInput CustomSettingNameInput;
				public UIInput IntegerInput;
				public UIInput DecimalInput;
				public UICheckbox BooleanInput;
				public UILabel GlobalVariableNameLabel;
				public UILabel GlobalVariableTypeLabel;
				public UILabel GlobalVariableDescLabel;
				public UILabel DeathStyleLabel;
				public UILabel FallDamageStyle;
				public UILabel SaveChangesLabel;

				public bool SavingNew = false;

				public UIButtonMessage CreateNewSettingButton;
				public UIButtonMessage CancelCreateNewButton;
				public UIButtonMessage SaveCustomSettingsButton;
				public UIButtonMessage HigherDifficultyButton;
				public UIButtonMessage LowerDifficulyButton;
				public UIButtonMessage ApplyChangesButton;
				public UIButtonMessage PrevDeathSetting;
				public UIButtonMessage NextDeathSetting;
				public UIButtonMessage PrevFallDamageSetting;
				public UIButtonMessage NextFallDamageSetting;
				public UIButtonMessage RemoveSettingButton;
				public GameObject TagTogglePrefab;
				//the setting selected by the player
				[HideInInspector]
				public DifficultySetting CurrentDifficultySetting;
				public UILabel DifficultyLabel;
				public List <UICheckbox> TagCheckboxes = new List<UICheckbox>();

				public string SelectedDifficultyName {
						get {
								if (CurrentDifficultySetting != null) {
										return CurrentDifficultySetting.Name;
								} else {
										return Globals.DefaultDifficultyName;
								}
						}
				}

				public List <DifficultySetting> AvailableDifficulties = new List <DifficultySetting>();
				public float CurrentDecimalValue;
				public bool CurrentToggleValue;
				public int CurrentIntegerValue;
				protected List <DifficultyDeathStyle> DeathStyles = new List<DifficultyDeathStyle>() {
						DifficultyDeathStyle.NoDeath,
						DifficultyDeathStyle.Blackout,
						DifficultyDeathStyle.PermaDeath
				};
				protected List <FallDamageStyle> FallDamageStyles = new List<FallDamageStyle>() {
						Frontiers.FallDamageStyle.None,
						Frontiers.FallDamageStyle.Forgiving,
						Frontiers.FallDamageStyle.Realistic
				};
				public int NumAffectedGlobalValues;
				public int NumUnaffectedGlobalValues;

				public override void WakeUp()
				{
						Mods.Get.Runtime.LoadAvailableMods(AvailableDifficulties, "DifficultySetting");

						CurrentDifficultySetting = null;

						for (int i = 0; i < AvailableDifficulties.Count; i++) {
								if (AvailableDifficulties[i].Name == Globals.DefaultDifficultyName) {
										CurrentDifficultySetting = AvailableDifficulties[i];
										break;
								}
						}

						if (CurrentDifficultySetting == null) {
								CurrentDifficultySetting = AvailableDifficulties[0];
						}

						mSuspendUpdates = true;

						ControllingTabPage.OnSelected += Show;
						ControllingTabPage.OnDeselected += Hide;

						CreateNewSettingButton.functionName = "OnClickCreateNewSetting";
						CreateNewSettingButton.target = gameObject;
						CancelCreateNewButton.functionName = "OnClickCancelCreateNewSetting";
						CancelCreateNewButton.target = gameObject;

						PrevDeathSetting.functionName = "OnClickPrevDeathSetting";
						PrevDeathSetting.target = gameObject;
						NextDeathSetting.functionName = "OnClickNextDeathSetting";
						NextDeathSetting.target = gameObject;

						PrevFallDamageSetting.functionName = "OnClickPrevFallDamageSetting";
						PrevFallDamageSetting.target = gameObject;
						NextFallDamageSetting.functionName = "OnClickNextFallDamageSetting";
						NextFallDamageSetting.target = gameObject;

						BooleanInput.functionName = "OnChangeBooleanInput";
						BooleanInput.eventReceiver = gameObject;
						DecimalInput.functionName = "OnChangeDecimalInputText";
						DecimalInput.functionNameEnter = "OnChangeDecimalInput";
						DecimalInput.eventReceiver = gameObject;
						IntegerInput.functionName = "OnChangeIntegerInputText";
						IntegerInput.functionNameEnter = "OnChangeIntegerInput";
						IntegerInput.eventReceiver = gameObject;

						CustomSettingNameInput.functionName = "OnChangeCustomSettingNameText";
						CustomSettingNameInput.functionNameEnter = "OnChangeCustomSettingName";
						CustomSettingNameInput.eventReceiver = gameObject;

						HigherDifficultyButton.target = gameObject;
						HigherDifficultyButton.functionName = "OnClickHigherDifficulty";
						LowerDifficulyButton.target = gameObject;
						LowerDifficulyButton.functionName = "OnClickLowerDifficulty";

						SaveCustomSettingsButton.target = gameObject;
						SaveCustomSettingsButton.functionName = "OnClickSaveCustomSettings";

						ApplyChangesButton.target = gameObject;
						ApplyChangesButton.functionName = "OnClickApplySettingButton";

						RemoveSettingButton.target = gameObject;
						RemoveSettingButton.functionName = "OnClickRemoveSettingButton";

						foreach (UICheckbox checkbox in TagCheckboxes) {
								checkbox.functionName = "OnChangeTag";
								checkbox.eventReceiver = gameObject;
						}

						mSuspendUpdates = false;
				}

				public override IEnumerable<FieldInfo> FetchItems()
				{
						return Globals.GetDifficultySettings();
				}

				public override void Hide()
				{
						mSuspendUpdates = true;

						if (CurrentDifficultySetting != null && Profile.Get.HasCurrentGame) {
								Profile.Get.CurrentGame.Difficulty = CurrentDifficultySetting;
								Profile.Get.CurrentGame.DifficultyName = CurrentDifficultySetting.Name;
						}

						base.Hide();
				}

				public override void Show()
				{
						mSuspendUpdates = true;

						base.Show();

						IntegerInput.gameObject.SetActive(false);
						DecimalInput.gameObject.SetActive(false);
						BooleanInput.gameObject.SetActive(false);

						foreach (UICheckbox checkbox in TagCheckboxes) {
								checkbox.gameObject.SetActive(false);
						}

						for (int i = 0; i < DifficultySetting.AvailableTags.Count; i++) {
								UILabel l = TagCheckboxes[i].gameObject.FindOrCreateChild("Label").GetComponent <UILabel>();
								l.text = DifficultySetting.AvailableTags[i];
								TagCheckboxes[i].gameObject.SetActive(true);
								TagCheckboxes[i].gameObject.name = DifficultySetting.AvailableTags[i];
						}

						mSuspendUpdates = false;

						ResetInputs();
				}

				public void OnClickCancelCreateNewSetting ( ) {
						//get rid of the one we were saving
						//assume we have more than 0 (safe bet)
						CurrentDifficultySetting = AvailableDifficulties[0];

						SavingNew = false;

						ResetInputs();
				}

				public void OnClickCreateNewSetting () {

						mSuspendUpdates = true;

						CurrentDifficultySetting = ObjectClone.Clone <DifficultySetting>(CurrentDifficultySetting);
						CurrentDifficultySetting.HasBeenCustomized = true;
						CurrentDifficultySetting.Name = "New Setting";
						CustomSettingNameInput.text = CurrentDifficultySetting.Name;
						CustomSettingNameInput.label.text = CustomSettingNameInput.text;
						UICamera.selectedObject = CustomSettingNameInput.gameObject;
						UIInput.current = CustomSettingNameInput;

						SavingNew = true;

						mSuspendUpdates = false;

						ResetInputs();
				}

				public void OnClickSaveCustomSettings()
				{
						CurrentDifficultySetting.Name = CustomSettingNameInput.text;
						Mods.Get.Runtime.SaveMod(CurrentDifficultySetting, "DifficultySetting", CurrentDifficultySetting.Name);
						CurrentDifficultySetting.HasBeenCustomized = false;
						AvailableDifficulties.SafeAdd(CurrentDifficultySetting);

						SavingNew = false;

						ResetInputs();
				}

				public override bool PushToViewerAutomatically {
						get {
								return true;
						}
				}

				public void Update ( ) {
						if (CurrentDifficultySetting != null && CurrentDifficultySetting.HasBeenCustomized && !SavingNew) {
								Mods.Get.Runtime.SaveMod <DifficultySetting> (CurrentDifficultySetting, "DifficultySetting", CurrentDifficultySetting.Name);
								CurrentDifficultySetting.HasBeenCustomized = false;
						}
				}

				public void OnClickApplySettingButton()
				{
						if (mSuspendUpdates)
								return;

						mSuspendUpdates = true;

						bool createdNewSetting = false;

						var atts = SelectedObject.GetCustomAttributes(typeof(EditableDifficultySettingAttribute), true);
						EditableDifficultySettingAttribute dsa = null;
						foreach (var att in atts) {
								dsa = (EditableDifficultySettingAttribute)att;
								if (dsa != null) {
										break;
								}
						}

						DifficultySettingGlobal dsg = null;
						for (int i = 0; i < CurrentDifficultySetting.GlobalVariables.Count; i++) {
								if (CurrentDifficultySetting.GlobalVariables[i].GlobalVariableName == SelectedObject.Name) {
										dsg = CurrentDifficultySetting.GlobalVariables[i];
										break;
								}
						}

						if (dsg == null) {
								dsg = new DifficultySettingGlobal();
								dsg.GlobalVariableName = SelectedObject.Name;
								CurrentDifficultySetting.GlobalVariables.Add(dsg);
								createdNewSetting = true;
						}

						switch (dsa.Type) {
								case EditableDifficultySettingAttribute.SettingType.Bool:
										dsg.VariableValue = CurrentToggleValue.ToString();
										//SelectedObject.SetValue(null, CurrentToggleValue);
										break;

								case EditableDifficultySettingAttribute.SettingType.FloatRange:
										dsg.VariableValue = CurrentDecimalValue.ToString();
										//SelectedObject.SetValue(null, CurrentDecimalValue);
										break;

								case EditableDifficultySettingAttribute.SettingType.IntRange:
										dsg.VariableValue = CurrentIntegerValue.ToString();
										//SelectedObject.SetValue(null, CurrentIntegerValue);
										break;

								case EditableDifficultySettingAttribute.SettingType.String:
										//StringInput.gameObject.SetActive(true);
										break;

								case EditableDifficultySettingAttribute.SettingType.Hidden:
								default:
										break;
						}

						ApplyChangesButton.SendMessage("SetDisabled");

						mSuspendUpdates = false;

						CurrentDifficultySetting.HasBeenCustomized = true;

						if (createdNewSetting) {
								ReceiveFromParentEditor(FetchItems());
								ResetInputs();
						} else {
								ResetInputs();
						}
				}

				public void ResetInputs()
				{
						mSuspendUpdates = true;

						if (SavingNew) {
								CreateNewSettingButton.gameObject.SetActive(false);
								CancelCreateNewButton.gameObject.SetActive(true);
								SaveCustomSettingsButton.gameObject.SetActive(true);
								CustomSettingNameInput.gameObject.SetActive(true);
						} else {
								CreateNewSettingButton.gameObject.SetActive(true);
								CancelCreateNewButton.gameObject.SetActive(false);
								SaveCustomSettingsButton.gameObject.SetActive(false);
								CustomSettingNameInput.gameObject.SetActive(false);
						}

						if (CurrentDifficultySetting == null || SavingNew) {
								Debug.Log("Current setting has not been customized, not setting inputs");
								GlobalVariableNameLabel.text = "(Click a global setting to edit)";
								GlobalVariableTypeLabel.text = string.Empty;
								GlobalVariableDescLabel.text = string.Empty;
								BooleanInput.gameObject.SetActive(false);
								IntegerInput.gameObject.SetActive(false);
								DecimalInput.gameObject.SetActive(false);
								ApplyChangesButton.gameObject.SetActive(false);
								RemoveSettingButton.gameObject.SetActive(false);
						} else {
								//enable custom saving
								if (mSelectedObject == null) {
										GlobalVariableNameLabel.text = "(Click a global setting to edit)";
										GlobalVariableTypeLabel.text = string.Empty;
										GlobalVariableDescLabel.text = string.Empty;
										ApplyChangesButton.gameObject.SetActive(false);
										RemoveSettingButton.gameObject.SetActive(false);
								}
								DifficultyLabel.text = SelectedDifficultyName;
								DeathStyleLabel.text = CurrentDifficultySetting.DeathStyle.ToString();
								FallDamageStyle.text = CurrentDifficultySetting.FallDamage.ToString();
						}

						for (int i = 0; i < TagCheckboxes.Count; i++) {
								if (TagCheckboxes[i].gameObject.activeSelf) {
										if (CurrentDifficultySetting.IsDefined(TagCheckboxes[i].gameObject.name)) {
												TagCheckboxes[i].isChecked = true;
										} else {
												TagCheckboxes[i].isChecked = false;
										}
								}
						}

						mSuspendUpdates = false;
				}

				public override void ReceiveFromParentEditor(IEnumerable<FieldInfo> editObject, ChildEditorCallback<IEnumerable<FieldInfo>> callBack)
				{
						mEditObject = editObject;
						mCallBack = callBack;
						HasFocus = true;
						NumAffectedGlobalValues = 0;
						NumUnaffectedGlobalValues = 0;
						PushEditObjectToNGUIObject();
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(FieldInfo editObject)
				{
						IGUIBrowserObject browserObject = base.ConvertEditObjectToBrowserObject(editObject);
						GUIGenericBrowserObject uabo = browserObject.gameObject.GetComponent <GUIGenericBrowserObject>();

						uabo.EditButton.functionName = "OnClickBrowserObject";
						uabo.EditButton.target = gameObject;
						uabo.Icon.atlas = Mats.Get.IconsAtlas;
						uabo.Icon.spriteName = "SkillIconCraftRepairMachine";
						uabo.Icon.enabled = true;
						Vector3 iconPosition = uabo.Icon.transform.localPosition;
						iconPosition.z = -25f;//this is necessary with this layout for some reason
						uabo.Icon.transform.localPosition = iconPosition;

						DifficultySettingGlobal dsg = null;
						if (CurrentDifficultySetting != null) {
								bool foundSetting = false;
								//see if the current difficulty setting has this global setting
								for (int i = 0; i < CurrentDifficultySetting.GlobalVariables.Count; i++) {
										if (CurrentDifficultySetting.GlobalVariables[i].GlobalVariableName == editObject.Name) {
												dsg = CurrentDifficultySetting.GlobalVariables[i];
												browserObject.name = "B_" + editObject.Name;
												uabo.Background.color = Colors.Darken(Colors.Get.BookColorLore);
												NumAffectedGlobalValues++;
												foundSetting = true;
												break;
										}
								}

								if (!foundSetting) {
										//unassigned difficulty settings are listed below
										browserObject.name = "D_" + editObject.Name;
										NumUnaffectedGlobalValues++;
										uabo.Name.text = editObject.Name + ":  " + editObject.GetValue(null).ToString();
								} else {
										//get the value of the global setting
										switch (editObject.FieldType.Name) {
												case "Int32":
														uabo.Name.text = editObject.Name + ":  " + dsg.VariableValue;
														break;

												case "Boolean":
														uabo.Name.text = editObject.Name + ":  " + dsg.VariableValue;
														break;

												case "Single":
														//we have to make sure it's displayed correctly
														float floatValue = (float)Single.Parse(dsg.VariableValue);
														uabo.Name.text = editObject.Name + ":  " + floatValue.ToString("0.0#####");
														break;

												case "Double":
														double doubleValue = (double)Double.Parse(dsg.VariableValue);
														uabo.Name.text = editObject.Name + ":  " + doubleValue.ToString("0.0#####");
														break;

												default:
														break;
										}
								}
						}

						uabo.Initialize(null);
						return browserObject;
				}

				public override void CreateDividerObjects()
				{
						GUIGenericBrowserObject dividerObject = null;
						IGUIBrowserObject newDivider = null;

						newDivider = CreateDivider();
						dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
						dividerObject.name = "A_empty";
						dividerObject.UseAsDivider = true;
						dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;

						if (NumAffectedGlobalValues > 0) {
								dividerObject.Name.text = "Difficulty variables changed by setting:";
						} else {
								dividerObject.Name.text = "(No difficulty variables changed by setting)";
						}
						dividerObject.Initialize("Divider");

						newDivider = CreateDivider();
						dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
						dividerObject.name = "C_empty";
						dividerObject.UseAsDivider = true;
						dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
						dividerObject.Name.text = "Unaffected difficulty variables:";
						dividerObject.Initialize("Divider");
				}

				public void OnChangeCustomSettingNameText() {

						Debug.Log("On change setting name text");

						if (mSuspendUpdates) {
								Debug.Log("Suspending updates, returning");
								return;
						}

						mSuspendUpdates = true;

						CustomSettingNameInput.label.text = CustomSettingNameInput.text + "[FF00FF]" + CustomSettingNameInput.caratChar + "[-]";

						bool isValid = CustomSettingNameInput.text.Length > 2;
						if (isValid) {
								for (int i = 0; i < AvailableDifficulties.Count; i++) {
										if (AvailableDifficulties[i].Name.ToLower() == CustomSettingNameInput.text.ToLower().Trim()) {
												isValid = false;
												break;
										}
								}
						}

						if (isValid) {
								SaveCustomSettingsButton.gameObject.SendMessage("SetEnabled");
						} else {
								SaveCustomSettingsButton.gameObject.SendMessage("SetDisabled");
						}

						mSuspendUpdates = false;
				}

				public void OnChangeCustomSettingName()
				{
						Debug.Log("On change setting name");

						if (mSuspendUpdates) {
								Debug.Log("Suspending updates, returning");
								return;
						}

						mSuspendUpdates = true;

						CustomSettingNameInput.label.text = CustomSettingNameInput.text;

						bool isValid = CustomSettingNameInput.text.Length > 2;
						if (isValid) {
								for (int i = 0; i < AvailableDifficulties.Count; i++) {
										if (AvailableDifficulties[i].Name.ToLower() == CustomSettingNameInput.text.ToLower().Trim()) {
												isValid = false;
												break;
										}
								}
						}

						if (isValid) {
								SaveCustomSettingsButton.gameObject.SendMessage("SetEnabled");
						} else {
								SaveCustomSettingsButton.gameObject.SendMessage("SetDisabled");
						}

						mSuspendUpdates = false;
				}

				public void OnChangeTag()
				{
						if (SavingNew) {
								return;
						}

						if (mSuspendUpdates)
								return;

						mSuspendUpdates = true;

						CurrentDifficultySetting.DifficultyFlags.Clear();

						for (int i = 0; i < TagCheckboxes.Count; i++) {
								if (TagCheckboxes[i].gameObject.activeSelf) {
										if (TagCheckboxes[i].isChecked) {
												CurrentDifficultySetting.DifficultyFlags.SafeAdd(TagCheckboxes[i].gameObject.name);
										}
								}
						}

						mSuspendUpdates = false;

						CurrentDifficultySetting.HasBeenCustomized = true;
						ResetInputs();
				}

				public void OnChangeIntegerInputText() {
						if (SavingNew) {
								return;
						}

						if (mSuspendUpdates)
								return;

						if (SelectedObject != null) {
								mSuspendUpdates = true;
								IntegerInput.label.text = IntegerInput.text + "[FF00FF]" + IntegerInput.caratChar + "[-]";
								mSuspendUpdates = false;
						}
				}

				public void OnChangeDecimalInputText() {
						if (SavingNew) {
								return;
						}

						if (mSuspendUpdates)
								return;

						if (SelectedObject != null) {
								mSuspendUpdates = true;
								DecimalInput.label.text = DecimalInput.text + "[FF00FF]" + DecimalInput.caratChar + "[-]";
								mSuspendUpdates = false;
						}
				}

				public void OnChangeIntegerInput()
				{
						if (SavingNew) {
								return;
						}

						if (mSuspendUpdates)
								return;

						if (SelectedObject != null) {
								mSuspendUpdates = true;

								Debug.Log("Changed integer input");
								int newValue = 0;
								Int32.TryParse(IntegerInput.text, out newValue);
								var atts = SelectedObject.GetCustomAttributes(typeof(EditableDifficultySettingAttribute), true);
								EditableDifficultySettingAttribute dsa = null;
								foreach (var att in atts) {
										dsa = (EditableDifficultySettingAttribute)att;
										if (dsa != null) {
												break;
										}
								}
								newValue = Mathf.Clamp(newValue, dsa.MinRangeInt, dsa.MaxRangeInt);
								IntegerInput.text = newValue.ToString();
								IntegerInput.label.text = IntegerInput.text;
								//SelectedObject.SetValue(null, newValue);
								ApplyChangesButton.SendMessage("SetEnabled");
								mSuspendUpdates = false;
						}
				}

				public void OnChangeDecimalInput()
				{
						if (SavingNew) {
								return;
						}

						if (mSuspendUpdates)
								return;

						if (SelectedObject != null) {
								mSuspendUpdates = true;

								Debug.Log("Changed decimal input");

								Single.TryParse(DecimalInput.text, out CurrentDecimalValue);
								//get the range and make sure it's clamped
								var atts = SelectedObject.GetCustomAttributes(typeof(EditableDifficultySettingAttribute), true);
								EditableDifficultySettingAttribute dsa = null;
								foreach (var att in atts) {
										dsa = (EditableDifficultySettingAttribute)att;
										if (dsa != null) {
												break;
										}
								}
								//this is tricky because we need a decimal point if it was added
								CurrentDecimalValue = Mathf.Clamp(CurrentDecimalValue, dsa.MinRangeFloat, dsa.MaxRangeFloat);
								DecimalInput.text = CurrentDecimalValue.ToString();
								DecimalInput.label.text = DecimalInput.text;
								//SelectedObject.SetValue(null, CurrentDecmalValue);
								ApplyChangesButton.SendMessage("SetEnabled");
								mSuspendUpdates = false;
						}
				}

				public void OnChangeBooleanInput()
				{
						if (SavingNew) {
								return;
						}

						if (mSuspendUpdates)
								return;

						if (SelectedObject != null) {
								mSuspendUpdates = true;

								CurrentToggleValue = BooleanInput.isChecked;
								ApplyChangesButton.SendMessage("SetEnabled");
								mSuspendUpdates = false;
						}
				}

				public void OnClickNextFallDamageSetting()
				{
						if (SavingNew) {
								return;
						}

						int currentIndex = FallDamageStyles.IndexOf(CurrentDifficultySetting.FallDamage);
						CurrentDifficultySetting.FallDamage = FallDamageStyles.NextItem(currentIndex);
						FallDamageStyle.text = CurrentDifficultySetting.FallDamage.ToString();

						CurrentDifficultySetting.HasBeenCustomized = true;
						ResetInputs();
				}

				public void OnClickPrevFallDamageSetting()
				{
						if (SavingNew) {
								return;
						}

						int currentIndex = FallDamageStyles.IndexOf(CurrentDifficultySetting.FallDamage);
						CurrentDifficultySetting.FallDamage = FallDamageStyles.PrevItem(currentIndex);
						FallDamageStyle.text = CurrentDifficultySetting.FallDamage.ToString();

						CurrentDifficultySetting.HasBeenCustomized = true;
						ResetInputs();
				}

				public void OnClickNextDeathSetting()
				{
						if (SavingNew) {
								return;
						}

						int currentIndex = DeathStyles.IndexOf(CurrentDifficultySetting.DeathStyle);
						CurrentDifficultySetting.DeathStyle = DeathStyles.NextItem(currentIndex);
						DeathStyleLabel.text = CurrentDifficultySetting.DeathStyle.ToString();

						CurrentDifficultySetting.HasBeenCustomized = true;
						ResetInputs();
				}

				public void OnClickPrevDeathSetting()
				{
						if (SavingNew) {
								return;
						}

						int currentIndex = DeathStyles.IndexOf(CurrentDifficultySetting.DeathStyle);
						CurrentDifficultySetting.DeathStyle = DeathStyles.PrevItem(currentIndex);
						DeathStyleLabel.text = CurrentDifficultySetting.DeathStyle.ToString();

						CurrentDifficultySetting.HasBeenCustomized = true;
						ResetInputs();
				}

				public void OnClickLowerDifficulty()
				{
						if (SavingNew) {
								return;
						}

						mSuspendUpdates = true;

						for (int i = 0; i < AvailableDifficulties.Count; i++) {
								if (CurrentDifficultySetting.Name == AvailableDifficulties[i].Name) {
										CurrentDifficultySetting = AvailableDifficulties.PrevItem(i);
										break;
								} else {
										Debug.Log("Doesn't match " + AvailableDifficulties[i].Name);
								}
						}

						Profile.Get.SetDifficulty(CurrentDifficultySetting);
						mSuspendUpdates = false;
						ReceiveFromParentEditor(FetchItems());
						ResetInputs();
				}

				public void OnClickHigherDifficulty()
				{
						if (SavingNew) {
								return;
						}

						Debug.Log("OnClickHigherrDifficulty");

						mSuspendUpdates = true;

						for (int i = 0; i < AvailableDifficulties.Count; i++) {
								if (CurrentDifficultySetting.Name == AvailableDifficulties[i].Name) {
										CurrentDifficultySetting = AvailableDifficulties.NextItem(i);
										break;
								} else {
										Debug.Log("Doesn't match " + AvailableDifficulties[i].Name);
								}
						}

						Profile.Get.SetDifficulty(CurrentDifficultySetting);
						mSuspendUpdates = false;
						ReceiveFromParentEditor(FetchItems());
						ResetInputs();
				}

				public override void PushSelectedObjectToViewer()
				{
						if (SavingNew) {
								return;
						}

						mSuspendUpdates = true;

						var atts = SelectedObject.GetCustomAttributes(typeof(EditableDifficultySettingAttribute), true);
						EditableDifficultySettingAttribute dsa = null;
						foreach (var att in atts) {
								dsa = (EditableDifficultySettingAttribute)att;
								if (dsa != null) {
										break;
								}
						}

						if (dsa == null) {
								GlobalVariableNameLabel.text = "Unknown / Not a difficulty setting";
								GlobalVariableTypeLabel.text = "Unknown";
								GlobalVariableDescLabel.text = "This type is unknown and/or uneditable";
								return;
						}

						DifficultySettingGlobal dsg = null;
						for (int i = 0; i < CurrentDifficultySetting.GlobalVariables.Count; i++) {
								if (CurrentDifficultySetting.GlobalVariables[i].GlobalVariableName == SelectedObject.Name) {
										dsg = CurrentDifficultySetting.GlobalVariables[i];
										break;
								}
						}

						GlobalVariableNameLabel.text = SelectedObject.Name;
						GlobalVariableDescLabel.text = dsa.Description;

						switch (dsa.Type) {
								case EditableDifficultySettingAttribute.SettingType.Bool:
										GlobalVariableTypeLabel.text = "Type: True/False";
										CurrentToggleValue = (bool)SelectedObject.GetValue(null);
										if (dsg != null) {
												Boolean.TryParse(dsg.VariableValue, out CurrentToggleValue);
										}
										BooleanInput.isChecked = CurrentToggleValue;
										BooleanInput.gameObject.SetActive(true);
										IntegerInput.gameObject.SetActive(false);
										DecimalInput.gameObject.SetActive(false);
										break;

								case EditableDifficultySettingAttribute.SettingType.FloatRange:
										GlobalVariableTypeLabel.text = "Type: Decimal Range (" + dsa.MinRangeFloat.ToString("0.0#####") + " - " + dsa.MaxRangeFloat.ToString("0.0#####") + ")";
										CurrentDecimalValue = (float)SelectedObject.GetValue(null);
										if (dsg != null) {
												Single.TryParse(dsg.VariableValue, out CurrentDecimalValue);
										}
										DecimalInput.text = CurrentDecimalValue.ToString("0.0#####");
										DecimalInput.label.text = DecimalInput.text;
										DecimalInput.gameObject.SetActive(true);
										UIInput.current = DecimalInput;
										IntegerInput.gameObject.SetActive(false);
										BooleanInput.gameObject.SetActive(false);
										break;

								case EditableDifficultySettingAttribute.SettingType.IntRange:
										GlobalVariableTypeLabel.text = "Type: Integer Range (" + dsa.MinRangeInt.ToString() + " - " + dsa.MaxRangeInt.ToString() + ")";
										CurrentIntegerValue = (int)SelectedObject.GetValue(null);
										if (dsg != null) {
												Int32.TryParse(dsg.VariableValue, out CurrentIntegerValue);
										}
										IntegerInput.text = CurrentIntegerValue.ToString();
										IntegerInput.label.text = IntegerInput.text;
										UIInput.current = IntegerInput;
										IntegerInput.gameObject.SetActive(true);
										DecimalInput.gameObject.SetActive(false);
										BooleanInput.gameObject.SetActive(false);
										break;

								case EditableDifficultySettingAttribute.SettingType.String:
										//StringInput.gameObject.SetActive(true);
										break;

								case EditableDifficultySettingAttribute.SettingType.Hidden:
								default:
										IntegerInput.gameObject.SetActive(false);
										DecimalInput.gameObject.SetActive(false);
										BooleanInput.gameObject.SetActive(false);
										GlobalVariableTypeLabel.text = "(Hidden)";
										GlobalVariableDescLabel.text = "This type is unknown and/or uneditable";
										break;
						}

						ApplyChangesButton.gameObject.SetActive(true);
						ApplyChangesButton.SendMessage("SetDisabled");
						RemoveSettingButton.gameObject.SetActive(true);

						mSuspendUpdates = false;
				}

				public void OnClickRemoveSettingButton()
				{
						for (int i = CurrentDifficultySetting.GlobalVariables.LastIndex(); i >= 0; i--) {
								if (CurrentDifficultySetting.GlobalVariables[i].GlobalVariableName == SelectedObject.Name) {
										CurrentDifficultySetting.GlobalVariables.RemoveAt(i);
								}
						}

						mSelectedObject = null;
						ReceiveFromParentEditor(FetchItems());
						CurrentDifficultySetting.HasBeenCustomized = true;
						ResetInputs();
				}

				protected bool mSuspendUpdates = false;
		}
}