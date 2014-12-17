using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class CharacterBody : WorldBody
		{
				public string OriginalName;
				public CharacterFlags Flags = new CharacterFlags();
				public Renderer LongHairRenderer;
				public Renderer ShortHairRenderer;
				public CharacterHairLength HairLength = CharacterHairLength.Short;
				public CharacterHairColor HairColor = CharacterHairColor.Brown;

				public Material FaceMaterial { 
						get {
								return mFaceMaterial;
						}
						set {
								try {
										mFaceMaterial = value;
										if (mBloodSplatterMaterial == null) {
												//make a copy of the local blood splatter material
												//Debug.Log ("Creating blood material");
												if (BloodSplatterMaterial == null) {
														BloodSplatterMaterial = Mats.Get.BloodSplatterMaterial;
												}
												mBloodSplatterMaterial = new Material(BloodSplatterMaterial);
												mBloodSplatterMaterial.SetFloat("_Cutoff", 1f);
										}
										for (int i = 0; i < Renderers.Count; i++) {
												Renderer r = Renderers[i];
												if (r.CompareTag("BodyGeneral")) {
														Material[] currentSharedMaterials = r.sharedMaterials;
														if (currentSharedMaterials.Length > 1) {
																//we'll have to check for blood splatter mats
																System.Collections.Generic.List <Material> newSharedMaterials = new System.Collections.Generic.List <Material>(currentSharedMaterials);
																bool foundBloodMat = false;
																for (int j = 0; j < newSharedMaterials.Count; j++) {
																		if (newSharedMaterials[j] != null) {
																				if (newSharedMaterials[j].name.Contains("Blood")) {
																						newSharedMaterials[j] = mBloodSplatterMaterial;
																						foundBloodMat = true;
																				} else if (newSharedMaterials[j].name.Contains("Face")) {
																						newSharedMaterials[j] = mFaceMaterial;
																				}
																		}
																}
																if (!foundBloodMat) {
																		newSharedMaterials.Add(mBloodSplatterMaterial);
																}
																r.sharedMaterials = newSharedMaterials.ToArray();
																newSharedMaterials.Clear();
														} else if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Face")) {
																Material[] newSharedMaterials = new Material [2];
																newSharedMaterials[0] = mFaceMaterial;
																newSharedMaterials[1] = mBloodSplatterMaterial;
																r.sharedMaterials = newSharedMaterials;
														}
												}
										}
								} catch (Exception e) {
										Debug.LogException(e);
								}
						}
				}

				public override void Initialize(IItemOfInterest worlditem)
				{
						base.Initialize(worlditem);

						NObject = gameObject.GetComponent <TNObject>();

						Renderers.Clear();
						Renderers.AddRange(gameObject.GetComponentsInChildren <Renderer>(true));

						for (int i = 0; i < Renderers.Count; i++) {
								Renderers[i].gameObject.layer = Globals.LayerNumWorldItemActive;
						}

						if (HairLength == CharacterHairLength.Long) {
								if (ShortHairRenderer != null) {
										ShortHairRenderer.enabled = false;
								}
								if (LongHairRenderer != null) {
										LongHairRenderer.enabled = true;
								}

						} else if (HairLength == CharacterHairLength.Short) {
								if (ShortHairRenderer != null) {
										ShortHairRenderer.enabled = true;
								}
								if (LongHairRenderer != null) {
										LongHairRenderer.enabled = false;
								}

						} else {
								if (ShortHairRenderer != null) {
										ShortHairRenderer.enabled = false;
								}
								if (LongHairRenderer != null) {
										LongHairRenderer.enabled = false;
								}
						}
				}

				protected Material mFaceMaterial = null;
		}
}