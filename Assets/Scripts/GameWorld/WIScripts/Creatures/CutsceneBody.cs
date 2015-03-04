using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class CutsceneBody : CharacterBody
		{
				public string CharacterName = "Daniel";
				[FrontiersAvailableModsAttribute("Character/Face")]
				public string FaceTextureName;
				[FrontiersAvailableModsAttribute("Character/Mask")]
				public string MaskTextureName;
				[FrontiersAvailableModsAttribute("Character/Body")]
				public string BodyTextureName;
				public string HairTextureName;

				public override void Awake()
				{
						Animator = gameObject.GetComponent <BodyAnimator>();
						if (Animator != null) {
								Animator.VerticalAxisMovement = 0f;
								Animator.HorizontalAxisMovement = 0f;
								Animator.animator.Play("TreeW", 0, UnityEngine.Random.Range(0.0f, 1.0f));
						}
						//do nothing else
						return;
				}

				public void Start()
				{
						if (CharacterName == "[Player]") {
								if (Flags.Gender != (int)Profile.Get.CurrentGame.Character.Gender) {
										gameObject.SetActive(false);
										return;
								} else {
										FaceTextureName = Profile.Get.CurrentGame.Character.FaceTextureName;
										BodyTextureName = Profile.Get.CurrentGame.Character.BodyTextureName;
										HairTextureName = Profile.Get.CurrentGame.Character.HairTextureName;
										HairLength = Profile.Get.CurrentGame.Character.HairLength;
										HairColor = Profile.Get.CurrentGame.Character.HairColor;
								}
						} else {
								CharacterTemplate template = null;
								if (Characters.GetTemplate (false, CharacterName, out template)) {
										FaceTextureName = template.StateTemplate.FaceTextureName;
										BodyTextureName = template.StateTemplate.BodyTextureName;
										HairTextureName = BodyTextureName;
										HairLength = template.StateTemplate.HairLength;
										HairColor = template.StateTemplate.HairColor;
								}
						}


						Renderers.Clear();
						Renderers.AddRange(transform.GetComponentsInChildren <Renderer>());

						gameObject.SetLayerRecursively(Globals.LayerNumScenery);

						Texture2D bodyTexture = null;
						Texture2D faceTexture = null;
						Texture2D hairTexture = null;
						Material bodyMaterial = null;
						Material faceMaterial = null;
						Material hairMaterial = null;
						CharacterBody body = this;
						//update textures
						bodyMaterial = body.MainMaterial;
						if (bodyMaterial == null) {
								bodyMaterial = new Material(Characters.Get.CharacterBodyMaterial);
								bodyMaterial.name = "NewBodyMaterial";
								body.MainMaterial = bodyMaterial;
						}
						faceMaterial = body.FaceMaterial;
						if (faceMaterial == null) {
								faceMaterial = new Material(Characters.Get.CharacterFaceMaterial);
								faceMaterial.name = "NewFaceMaterial";
								body.FaceMaterial = faceMaterial;
						}
						hairMaterial = body.HairMaterial;
						if (hairMaterial == null) {
								hairMaterial = new Material(Characters.Get.CharacterHairMaterial);
								hairMaterial.name = "NewHairMaterial";
								body.HairMaterial = hairMaterial;
						}

						if (Mods.Get.Runtime.BodyTexture(ref bodyTexture, BodyTextureName)) {
								bodyMaterial.SetTexture("_MainTex", bodyTexture);
						}
						if (Mods.Get.Runtime.FaceTexture(ref faceTexture, FaceTextureName)) {
								faceMaterial.SetTexture("_MainTex", faceTexture);
						}
						if (Mods.Get.Runtime.BodyTexture(ref hairTexture, HairTextureName)) {
								hairMaterial.SetTexture("_MainTex", hairTexture);
								//now apply the hair color - we use a full red texture to color the whole texture
								hairMaterial.SetColor("_EyeColor", Colors.Get.HairColor (HairColor));
						}

						body.SetHairLength(HairLength);
				}

				public override void Update()
				{
						if (Animator != null) {
								Animator.animator.SetBool("Walking", true);
								Animator.VerticalAxisMovement = Vector3.Distance(transform.position, mPositionLastFrame);
								mPositionLastFrame = transform.position;
						}
						return;
				}

				protected Vector3 mPositionLastFrame;
		}
}
