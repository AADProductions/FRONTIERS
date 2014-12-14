using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

public class PlayerFocusHighlight : MonoBehaviour
{		//puts little halos around things if the player focuses on them
		//this script is very, very old
		//and should probably be turned into a proper WIScript
		public WorldItem worlditem;
		protected static List <string> ForceHighlightScripts = new List <string>() { "Dynamic", "Trigger", "Character" };
		protected static List <string> ForceNoHighlightScripts = new List <string>() { "Window" };
		protected static List <string> ForceNoOutlineScripts = new List <string>() { "Character", "Window" };
		public bool IsSpecialObject = false;
		public List <Material> CustomMats = new List<Material>();
		public Dictionary <Renderer,List<Material>> MaterialsBeforeFocus = new Dictionary<Renderer, List<Material>>();

		public void Awake()
		{
				worlditem = gameObject.GetComponent <WorldItem>();
				worlditem.OnGainPlayerFocus += OnGainPlayerFocus;
				worlditem.OnLosePlayerFocus += OnLosePlayerFocus;
		}

		public void Start()
		{
				if (IsSpecialObject) {
						OnBecomeSpecialObject();
				}
		}

		public void OnGainPlayerAttention()
		{
				GainAttention();
		}

		public void OnLosePlayerAttention()
		{
				LoseAttention();
		}

		public void OnGainPlayerFocus()
		{
				GainFocusOnObject();
		}

		public void OnBecomeSpecialObject()
		{

		}

		public void OnLosePlayerFocus()
		{
				LoseFocusOnObject();
		}

		protected void GainSpecialOnObject()
		{

		}

		protected void GainFocusOnObject()
		{
				if (worlditem == null || worlditem.Destroyed) {
						return;
				}

				if (mHasAddedFocusMats || !Profile.Get.CurrentPreferences.Immersion.WorldItemOverlay) {
						return;
				}

				bool useHighlight = worlditem.CanEnterInventory;
				bool useOutline = true;//by default
				for (int i = 0; i < ForceHighlightScripts.Count; i++) {
						if (worlditem.Is(ForceHighlightScripts[i])) {
								useHighlight = true;
								break;
						}
				}
				for (int i = 0; i < ForceNoHighlightScripts.Count; i++) {
						if (worlditem.Is(ForceNoHighlightScripts[i])) {
								useHighlight = false;//overrides
								break;
						}
				}
				for (int i = 0; i < ForceNoOutlineScripts.Count; i++) {
						if (worlditem.Is(ForceNoOutlineScripts[i])) {
								useOutline = false;
								break;
						}
				}

				for (int i = 0; i < worlditem.Renderers.Count; i++) {
						List <Material> preFocusMaterials = null;
						if (!MaterialsBeforeFocus.TryGetValue(worlditem.Renderers[i], out preFocusMaterials)) {
								preFocusMaterials = new List<Material>();
								MaterialsBeforeFocus.Add(worlditem.Renderers[i], preFocusMaterials);
						} else {
								preFocusMaterials.Clear();
						}
						preFocusMaterials.AddRange(worlditem.Renderers[i].sharedMaterials);
				}

				if (useHighlight) {
						AddMatToRenderers(worlditem.Renderers, Mats.Get.FocusHighlightMaterial);
				}
				if (useOutline) {
						if (worlditem.Props.Global.UseCutoutShader) {
								AddCustomMatToRenderers(worlditem.Renderers, Mats.Get.FocusOutlineCutoutMaterial, CustomMats);
						} else {
								AddMatToRenderers(worlditem.Renderers, Mats.Get.FocusOutlineMaterial);
						}
				}
				mHasAddedFocusMats = true;
		}

		protected void LoseFocusOnObject()
		{
				if (worlditem == null || worlditem.Destroyed) {
						return;
				}
				foreach (KeyValuePair <Renderer,List <Material>> preFocusMats in MaterialsBeforeFocus) {
						preFocusMats.Key.sharedMaterials = preFocusMats.Value.ToArray();
				}
				MaterialsBeforeFocus.Clear();
				mHasAddedFocusMats = false;
		}

		protected void GainAttention()
		{
				if (worlditem == null || worlditem.Destroyed) {
						return;
				}

				if (mHasAddedAttentionMats || !Profile.Get.CurrentPreferences.Immersion.WorldItemOverlay) {
						return;
				}

				AddMatToRenderers(worlditem.Renderers, Mats.Get.AttentionOutlineMaterial);
				mHasAddedAttentionMats = true;
		}

		protected void LoseAttention()
		{
				if (worlditem == null || worlditem.Destroyed) {
						return;
				}

				RemoveMatFromRenderers(worlditem.Renderers, Mats.Get.AttentionOutlineMaterial);
				mHasAddedAttentionMats = false;
		}

		protected static void AddCustomMatToRenderers(List <Renderer> renderers, Material matBase, List <Material> customMats)
		{
				for (int i = 0; i < renderers.Count; i++) {
						//ugh, sooo inefficient...
						List <Material> finalRendererMaterials = new List<Material>();
						List <Material> finalCustomMaterials = new List<Material>();
						for (int j = 0; j < renderers[i].sharedMaterials.Length; j++) {
								//get the next shared material from the object
								Material customMat = null;
								Material sharedMat = renderers[i].sharedMaterials[j];
								//baseRenderQueue = sharedMat.renderQueue;
								finalRendererMaterials.Add(sharedMat);
								string customMatName = renderers[i].name + sharedMat.name;
								//see if the custom outline material already exists
								for (int k = 0; k < customMats.Count; k++) {
										if (customMats[k].name == customMatName) {
												customMat = customMats[k];
												break;
										}
								}
								//if we didn't find it then create it
								if (customMat == null) {
										customMat = new Material(matBase);
										customMat.name = customMatName;
										customMats.Add(customMat);
										//set the main texture to the main texture of the shared material
										customMat.SetTexture("_MainTex", sharedMat.GetTexture("_MainTex"));
								}
								finalCustomMaterials.Add(customMat);
						}
						finalRendererMaterials.AddRange(finalCustomMaterials);
						renderers[i].sharedMaterials = finalRendererMaterials.ToArray();
				}
		}

		protected static Material AddMatToRenderers(List <Renderer> renderers, Material mat)
		{
				for (int i = 0; i < renderers.Count; i++) {
						if (renderers[i] != null) {
								List <Material> currentMaterials = new List<Material>(renderers[i].sharedMaterials);
								if (!currentMaterials.Contains(mat)) {
										currentMaterials.Add(mat);
										renderers[i].sharedMaterials = currentMaterials.ToArray();
								}
						}
				}
				return mat;
		}

		protected static void RemoveMatFromRenderers(List <Renderer> renderers, List <Material> mats)
		{
				for (int i = 0; i < mats.Count; i++) {
						RemoveMatFromRenderers(renderers, mats[i]);
				}
		}

		protected static void RemoveMatFromRenderers(List <Renderer> renderers, Material mat)
		{
				if (mat == null) {
						return;
				}

				for (int i = 0; i < renderers.Count; i++) {
						if (renderers[i] != null) {
								List <Material> currentMaterials = new List<Material>(renderers[i].sharedMaterials);
								//use the name because it may be instanced
								for (int j = currentMaterials.Count - 1; j >= 0; j--) {
										Material currentMaterial = currentMaterials[j];
										if (currentMaterial != null && currentMaterial.name == mat.name) {
												currentMaterials.RemoveAt(j);
										}
								}
								currentMaterials.Remove(mat);
								renderers[i].sharedMaterials = currentMaterials.ToArray();
						}
				}
		}

		protected IEnumerator FadeOutDetect()
		{
				mFadingOutDetection = true;
				yield return null;
				while (mDetectionIntensity > 0.001f) {
						mDetectionIntensity -= (float)WorldClock.ARTDeltaTime;
				}
				RemoveMatFromRenderers(worlditem.Renderers, mDetectionMaterial);
				mFadingOutDetection = false;
				yield break;
		}

		public static void FocusHighlightDoppleganger(GameObject occupantDoppleganger, bool highlight)
		{
				List <Renderer> renderers = new List<Renderer>(occupantDoppleganger.GetComponentsInChildren <Renderer>(false));
				if (highlight) {
						AddMatToRenderers(renderers, Mats.Get.FocusOutlineMaterial);
				} else {
						RemoveMatFromRenderers(renderers, Mats.Get.FocusOutlineMaterial);
				}
		}

		protected float mDetectionIntensity = 0f;
		protected bool mFadingOutDetection = false;
		protected bool mHasAddedFocusMats = false;
		protected bool mHasAddedAttentionMats = false;
		protected bool mHasBeenMadeSpecialObject = false;
		protected Material mDetectionMaterial;
		//we keep a local copy of this because we change its properties
}
