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
		public CharacterFlags Flags = new CharacterFlags ();
		public Renderer LongHairRenderer;
		public Renderer ShortHairRenderer;
		public CharacterHairLength HairLength = CharacterHairLength.Short;
		public CharacterHairColor HairColor = CharacterHairColor.Brown;
		public bool Ghost = false;
		//character hair names are weird in the generic models we got, eg 'ponnytail'
		public static string[] gHairTags = new string [] { "hair", "ponnytail" };

		public override void Awake ()
		{
			base.Awake ();
			//characters don't use overrides
			Animator.UseOverrideController = false;
		}

		public Material HairMaterial {
			get {
				return mHairMaterial;
			}
			set {
				try {
					mHairMaterial = value;
					for (int i = 0; i < Renderers.Count; i++) {
						string rendererName = Renderers [i].name.ToLower ();
						for (int j = 0; j < gHairTags.Length; j++) {
							if (rendererName.Contains (gHairTags [j])) {
								Renderers [i].sharedMaterial = mHairMaterial;
								break;
							}
						}

					}
				} catch (Exception e) {
					Debug.LogException (e);
				}
			}
		}

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
						mBloodSplatterMaterial = new Material (BloodSplatterMaterial);
						mBloodSplatterMaterial.SetFloat ("_Cutoff", 1f);
					}
					for (int i = 0; i < Renderers.Count; i++) {
						Renderer r = Renderers [i];
						if (r.CompareTag ("BodyGeneral")) {
							Material[] currentSharedMaterials = r.sharedMaterials;
							if (currentSharedMaterials.Length > 1) {
								//we'll have to check for blood splatter mats
								System.Collections.Generic.List <Material> newSharedMaterials = new System.Collections.Generic.List <Material> (currentSharedMaterials);
								bool foundBloodMat = false;
								for (int j = 0; j < newSharedMaterials.Count; j++) {
									if (newSharedMaterials [j] != null) {
										if (newSharedMaterials [j].name.Contains ("Blood")) {
											newSharedMaterials [j] = mBloodSplatterMaterial;
											foundBloodMat = true;
										} else if (newSharedMaterials [j].name.Contains ("Face")) {
											newSharedMaterials [j] = mFaceMaterial;
										}
									}
								}
								if (!foundBloodMat) {
									newSharedMaterials.Add (mBloodSplatterMaterial);
								}
								r.sharedMaterials = newSharedMaterials.ToArray ();
								newSharedMaterials.Clear ();
							} else if (r.sharedMaterial != null && r.sharedMaterial.name.Contains ("Face")) {
								Material[] newSharedMaterials = new Material [2];
								newSharedMaterials [0] = mFaceMaterial;
								newSharedMaterials [1] = mBloodSplatterMaterial;
								r.sharedMaterials = newSharedMaterials;
							}
						}
					}
				} catch (Exception e) {
					Debug.LogException (e);
				}
			}
		}

		public override void Initialize (IItemOfInterest worlditem)
		{
			base.Initialize (worlditem);

			NObject = gameObject.GetComponent <TNObject> ();

			Renderers.Clear ();
			Renderers.AddRange (gameObject.GetComponentsInChildren <Renderer> (true));

			for (int i = 0; i < Renderers.Count; i++) {
				Renderers [i].gameObject.layer = Globals.LayerNumWorldItemActive;
			}

			SetHairLength (HairLength);

			/*if (HairLength == CharacterHairLength.Long) {
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
						}*/
		}

		public void SetHairLength (CharacterHairLength length)
		{
			HairLength = length;
			if (HairLength == CharacterHairLength.Long) {
				for (int i = 0; i < Renderers.Count; i++) {
					string rendererName = Renderers [i].name.ToLower ();
					for (int j = 0; j < gHairTags.Length; j++) {
						if (rendererName.Contains (gHairTags [j])) {
							Renderers [i].enabled = true;
							break;
						}
					}
				}
			} else {
				for (int i = 0; i < Renderers.Count; i++) {
					string rendererName = Renderers [i].name.ToLower ();
					for (int j = 0; j < gHairTags.Length; j++) {
						if (rendererName.Contains (gHairTags [j])) {
							Renderers [i].enabled = false;
							break;
						}
					}
				}
			}
		}

		protected Material mFaceMaterial = null;
		protected Material mHairMaterial = null;
	}
}