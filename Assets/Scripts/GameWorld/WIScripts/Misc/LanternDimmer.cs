using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Frontiers.World {
	public class LanternDimmer : WIScript {

		public LanternDimmerState State = new LanternDimmerState ( );
		public Material LitMaterial;
		public Material DimmedMaterial;
		public Renderer DimmedRenderer;

		public void StartDimming ( ) {

			Debug.Log ("Dimming...");
			State.Dimmed = true;
			FXManager.Get.SpawnFX (worlditem.Position, State.FXOnDimmed);
			if (worlditem.Light != null) {
				worlditem.Light.gameObject.SetActive (false);
			}

			WIState state = worlditem.States.GetState ("Light");
			DimmedRenderer = state.StateRenderer;
			Material[] sharedMaterials = DimmedRenderer.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++) {
				if (sharedMaterials [i].name.Equals (State.LitMaterialName)) {
					LitMaterial = sharedMaterials [i];
					DimmedMaterial = new Material (sharedMaterials [i]);
					DimmedMaterial.name = DimmedMaterial.name + " (Dimmed)";
					sharedMaterials [i] = DimmedMaterial;
					break;
				}
			}
			DimmedMaterial.SetColor ("_EmiTint", Colors.Alpha (DimmedMaterial.GetColor ("_EmiTint"), 0f));
			DimmedMaterial.SetFloat ("_RefIntensity", 0.1f);
			DimmedRenderer.sharedMaterials = sharedMaterials;

			MasterAudio.PlaySound (State.DimmingSoundType, worlditem.tr, State.DimmingSound);
		}

		public void StopDimming ( ) {
			Debug.Log ("UNDimming...");
			State.Dimmed = false;
			FXManager.Get.SpawnFX (worlditem.Position, State.FXOnUndimmed);
			if (worlditem.Light != null) {
				worlditem.Light.gameObject.SetActive (true);
			}

			Material[] sharedMaterials = DimmedRenderer.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++) {
				if (sharedMaterials [i] == DimmedMaterial) {
					sharedMaterials [i] = LitMaterial;
				}
			}
			DimmedRenderer.sharedMaterials = sharedMaterials;
		}

		public override bool CanEnterInventory {
			get {
				return false;
			}
		}

		public override bool CanBeCarried {
			get {
				return false;
			}
		}

	}

	[Serializable]
	public class LanternDimmerState {
		public string LitMaterialName;
		public bool Dimmed = false;
		public MasterAudio.SoundType DimmingSoundType;
		public string DimmingSound;
		[FrontiersFXAttribute]
		public string FXOnDimmed = string.Empty;
		[FrontiersFXAttribute]
		public string FXOnUndimmed = string.Empty;
	}
}