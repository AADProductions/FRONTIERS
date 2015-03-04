using UnityEngine;
using System.Collections;
using Frontiers;
using System;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUICharacterCreator : GUIEditor <CharacterCreator>
		{
				public GameObject FinishedButton;
				public GameObject CancelButton;
				public UIInput CharacterName;
				public UILabel NameLabel;
				public UILabel GenderLabel;
				public UILabel SkinLabel;
				public UILabel HairColorLabel;
				public UILabel HairLengthLabel;
				public UILabel EyeColorLabel;
				public UILabel AgeLabel;
				public GameObject SkinBrownButton;
				public GameObject SkinPinkButton;
				public GameObject SkinOliveButton;
				public GameObject SkinTanButton;
				public GameObject HairBlondeButton;
				public GameObject HairRedButton;
				public GameObject HairBlackButton;
				public GameObject HairBrownButton;
				public GameObject EyeGreyButton;
				public GameObject EyeBlueButton;
				public GameObject EyeBrownButton;
				public GameObject EyeGreenButton;
				public GameObject GenderMaleButton;
				public GameObject GenderFemaleButton;
				public GameObject HairLongButton;
				public GameObject HairShortButton;
				public UISlider AgeSlider;
				public string ErrorMessage;
				//stuff that may or may not be used in this editor
				public GUITabPage ControllingTabPage;

				public bool UseAvatars {
						get {
								return CharacterBodiesParent != null;
						}
				}

				public Transform CharacterBodiesParent;
				public CharacterBody MaleBody;
				public CharacterBody FemaleBody;

				public override void WakeUp()
				{
						if (ControllingTabPage != null) {
								ControllingTabPage.OnSelected += Show;
								ControllingTabPage.OnDeselected += Hide;
						}
				}

				public override void Show()
				{
						base.Show();
						if (UseAvatars) {
								UpdateAvatars();
								Light[] lights = CharacterBodiesParent.GetComponentsInChildren<Light>();
								foreach (Light light in lights) {
										light.enabled = true;
								}
						}

						if (ControllingTabPage != null && !HasEditObject) {
								CharacterCreator cc = new CharacterCreator();
								cc.Character = Profile.Get.CurrentGame.Character;
								//cc.Character.CreatedManually = true;
								ReceiveFromParentEditor(cc);
						}
				}

				public override void Hide()
				{
						base.Hide();
						if (UseAvatars) {
								if (MaleBody != null) {
										MaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
										FemaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
								}
								Light[] lights = CharacterBodiesParent.GetComponentsInChildren<Light>();
								foreach (Light light in lights) {
										light.enabled = false;
								}
						}
				}

				public override void ReceiveFromParentEditor(CharacterCreator editObject, ChildEditorCallback<CharacterCreator> callBack)
				{
						if (ControllingTabPage != null) {
								mEditObject = editObject;
								PushEditObjectToNGUIObject();
								if (!Visible) {
										Show();
								}
								return;
						} else {
								base.ReceiveFromParentEditor(editObject, callBack);
						}
				}

				public override void PushEditObjectToNGUIObject()
				{
						Debug.Log("Pushing edit object to editor in character creator");
						mEditObject.Cancelled = false;
						OnChangeCharacter(false);
				}

				public override bool ActionCancel(double timeStamp)
				{
						if (ControllingTabPage != null) {
								return true;
						}
						mEditObject.Cancelled = true;
						return base.ActionCancel(timeStamp);
				}

				public void OnChangeCharacter(bool countsAsCreation)
				{
						if (!HasEditObject)
								return;

						if (ControllingTabPage != null) {
								NameLabel.text = mEditObject.Character.FirstName + " Benneton";
						} else {
								NameLabel.text = string.Empty;
						}

						if (mChangingCharacter)
								return;

						mChangingCharacter = true;

						CharacterName.text = mEditObject.Character.FirstName;
						CharacterName.label.text = mEditObject.Character.FirstName;
						AgeLabel.text = mEditObject.Character.Age.ToString();
						HairColorLabel.text = mEditObject.Character.HairColor.ToString();
						HairLengthLabel.text = mEditObject.Character.HairLength.ToString();
						if (mEditObject.Character.Ethnicity != CharacterEthnicity.None) {
								switch (mEditObject.Character.Ethnicity) {
										case CharacterEthnicity.BlackCarribean:
												SkinLabel.text = "Brown";
												break;

										case CharacterEthnicity.Caucasian:
												SkinLabel.text = "Pink";
												break;

										case CharacterEthnicity.EastIndian:
												SkinLabel.text = "Tan";
												break;

										case CharacterEthnicity.HanChinese:
												SkinLabel.text = "Olive";
												break;

										default:
												break;
								}
						} else {
								SkinLabel.text = "None";
						}
						EyeColorLabel.text = mEditObject.Character.EyeColor.ToString();
						GenderLabel.text = mEditObject.Character.Gender.ToString();

						ErrorMessage = string.Empty;
						if (mEditObject.Confirm(out ErrorMessage)) {
								if (FinishedButton != null) {
										FinishedButton.SendMessage("SetEnabled");
								}
						} else {
								if (FinishedButton != null) {
										FinishedButton.SendMessage("SetDisabled");
								}
								Debug.Log(ErrorMessage);
						}

						if (UseAvatars) {
								//this will update the face / body / etc.
								mEditObject.Character.RefreshVisual();
								UpdateAvatars();
						}

						if (countsAsCreation) {
								EditObject.Character.CreatedManually = true;
						}

						mChangingCharacter = false;
				}

				protected bool mChangingCharacter = false;

				public void UpdateAvatars()
				{
						if (MaleBody == null) {
								CreateAvatars();
						}

						Debug.Log("Updating avatars");

						if (!HasEditObject) {
								Debug.Log("No edit object, returning");
								return;
						}

						//get the right body and textures etc.
						EditObject.Character.RefreshVisual();

						if (!Visible) {
								MaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
								FemaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
								Debug.Log("Not visible, returning");
								return;
						}

						CharacterBody body = null;
						Texture2D bodyTexture = null;
						Texture2D faceTexture = null;
						Texture2D hairTexture = null;
						Material bodyMaterial = null;
						Material faceMaterial = null;
						Material hairMaterial = null;

						if (mEditObject.Character.Gender == CharacterGender.Male) {
								body = MaleBody;
								FemaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
						} else {
								body = FemaleBody;
								MaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
						}
						body.gameObject.SetLayerRecursively(Globals.LayerNumGUIRaycastIgnore);
						//update textures
						bodyMaterial = body.MainMaterial;
						if (bodyMaterial == null) {
								Debug.Log("Assigning new body material");
								bodyMaterial = new Material(Characters.Get.CharacterBodyMaterial);
								bodyMaterial.name = "NewBodyMaterial";
								body.MainMaterial = bodyMaterial;
						}
						faceMaterial = body.FaceMaterial;
						if (faceMaterial == null) {
								Debug.Log("Assigning new face material");
								faceMaterial = new Material(Characters.Get.CharacterFaceMaterial);
								faceMaterial.name = "NewFaceMaterial";
								body.FaceMaterial = faceMaterial;
						}
						hairMaterial = body.HairMaterial;
						if (hairMaterial == null) {
								Debug.Log("Assigning new body material");
								hairMaterial = new Material(Characters.Get.CharacterHairMaterial);
								hairMaterial.name = "NewHairMaterial";
								body.HairMaterial = hairMaterial;
						}

						if (Mods.Get.Runtime.BodyTexture(ref bodyTexture, EditObject.Character.BodyTextureName)) {
								Debug.Log("Loaded " + EditObject.Character.BodyTextureName + ", applying to body material");
								bodyMaterial.SetTexture("_MainTex", bodyTexture);
						} else {
								Debug.Log("Couldn't load body texture " + EditObject.Character.BodyTextureName);
						}
						if (Mods.Get.Runtime.FaceTexture(ref faceTexture, EditObject.Character.FaceTextureName)) {
								Debug.Log("Loaded " + EditObject.Character.FaceTextureName + ", applying to face material");
								faceMaterial.SetTexture("_MainTex", faceTexture);
						} else {
								Debug.Log("Couldn't load face texture " + EditObject.Character.FaceTextureName);
						}
						if (Mods.Get.Runtime.BodyTexture(ref hairTexture, EditObject.Character.HairTextureName)) {
								Debug.Log("Loaded " + EditObject.Character.HairTextureName + ", applying to hair material");
								hairMaterial.SetTexture("_MainTex", hairTexture);
								//now apply the hair color - we use a full red texture to color the whole texture
								hairMaterial.SetColor("_EyeColor", Colors.Get.HairColor (EditObject.Character.HairColor));
						} else {
								Debug.Log("Couldn't load hair texture " + EditObject.Character.FaceTextureName);
						}

						body.SetHairLength(EditObject.Character.HairLength);
				}

				public void CreateAvatars()
				{
						//clone the default male and female bodies
						//make sure the character bodies parent doesn't get changed by tab visibility
						CharacterBodiesParent.tag = Globals.TagIgnoreTab;
						CharacterBody body = null;
						if (Characters.Get.GetBody(true, Globals.DefaultFemalePlayerBody, out body)) {
								GameObject newBody = GameObject.Instantiate(body.gameObject) as GameObject;
								newBody.transform.parent = CharacterBodiesParent;
								newBody.transform.localScale = Vector3.one;
								newBody.transform.localPosition = Vector3.zero;
								newBody.transform.localRotation = Quaternion.identity;
								FemaleBody = newBody.GetComponent <CharacterBody>();
						} else {
								Debug.LogError("Couldn't get female body in character creator");
						}

						if (Characters.Get.GetBody(true, Globals.DefaultMalePlayerBody, out body)) {
								GameObject newBody = GameObject.Instantiate(body.gameObject) as GameObject;
								newBody.transform.parent = CharacterBodiesParent;
								newBody.transform.localScale = Vector3.one;
								newBody.transform.localPosition = Vector3.zero;
								newBody.transform.localRotation = Quaternion.identity;
								MaleBody = newBody.GetComponent <CharacterBody>();
						} else {
								Debug.LogError("Couldn't get female body in character creator");
						}
						MaleBody.Initialize(null);
						FemaleBody.Initialize(null);
						MaleBody.SetVisible(true);
						FemaleBody.SetVisible(true);
						MaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
						FemaleBody.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
				}

				public void OnClickFinishedButton()
				{
						if (!HasEditObject)
								return;

						if (mEditObject.Confirm(out ErrorMessage)) {
								Finish();
						} else {
								Debug.Log(ErrorMessage);
						}
				}

				public void OnClickCancelButton()
				{
						if (!HasEditObject)
								return;

						mEditObject.Cancelled = true;
						Finish();
				}

				public void OnSubmitWithEnter()
				{
						if (!HasEditObject)
								return;

						mEditObject.SetName(CharacterName.text);
						OnChangeCharacter(true);
				}

				public void ChangeName()
				{
						if (!HasEditObject)
								return;

						mEditObject.SetName(CharacterName.text);
						OnChangeCharacter(true);
				}

				public void OnClickSkinButton(GameObject button)
				{
						if (!HasEditObject)
								return;

						switch (button.name) {
								case "Pink":
								default:
										mEditObject.SetEthnicity(CharacterEthnicity.Caucasian);
										break;

								case "Brown":
										mEditObject.SetEthnicity(CharacterEthnicity.BlackCarribean);
										break;

								case "Olive":
										mEditObject.SetEthnicity(CharacterEthnicity.HanChinese);
										break;

								case "Tan":
										mEditObject.SetEthnicity(CharacterEthnicity.EastIndian);
										break;
						}
						OnChangeCharacter(true);
				}

				public void OnClickHairColorButton(GameObject button)
				{
						if (!HasEditObject)
								return;

						mEditObject.SetHairColor((CharacterHairColor)Enum.Parse(typeof(CharacterHairColor), button.name, true));
						OnChangeCharacter(true);
				}

				public void OnClickHairLengthButton(GameObject button)
				{
						if (!HasEditObject)
								return;

						mEditObject.SetHairLength((CharacterHairLength)Enum.Parse(typeof(CharacterHairLength), button.name, true));
						OnChangeCharacter(true);
				}

				public void OnClickEyeColorButton(GameObject button)
				{
						if (!HasEditObject)
								return;

						mEditObject.SetEyeColor((CharacterEyeColor)Enum.Parse(typeof(CharacterEyeColor), button.name, true));
						OnChangeCharacter(true);
				}

				public void OnClickGenderButton(GameObject button)
				{
						if (!HasEditObject)
								return;

						switch (button.name) {
								case "Male":
										mEditObject.SetGender(CharacterGender.Male);
										break;

								case "Female":
										mEditObject.SetGender(CharacterGender.Female);
										break;
						}
						OnChangeCharacter(true);
				}

				public void OnChangeAgeValue(float value)
				{
						if (!HasEditObject)
								return;

						mEditObject.SetAge(Mathf.FloorToInt((AgeSlider.sliderValue * (Globals.MaxCharacterAge - Globals.MinCharacterAge)) + Globals.MinCharacterAge));
						OnChangeCharacter(true);
				}
		}
}