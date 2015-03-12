using UnityEngine;
using System.Collections;
using ExtensionMethods;
using Frontiers;

//[ExecuteInEditMode]
public class UpdateGRT : MonoBehaviour
{
		public Material GrtDistantMaterial;
		public float FarthestDistanceSize = 0.5f;
		public float ClosestDistanceSize = 1.0f;
		public float MaxDistance = 7000f;
		public float MinDistance = 250f;
		public float CurrentDistance = 0f;
		public float NormalizedDistance = 0f;
		public Transform ActualPosition;
		public Transform tr;
		public Renderer GRTRenderer;

		public void Start()
		{
				tr = transform;
				mParent = tr.parent;
		}

		public void Update()
		{
				GrtDistantMaterial.SetColor("_ColorFrom", Color.Lerp(Color.Lerp(RenderSettings.fogColor, RenderSettings.ambientLight, 0.05f), Color.black, 0.05f));
				GrtDistantMaterial.SetColor("_ColorTo", Color.Lerp(Color.Lerp(RenderSettings.fogColor, RenderSettings.ambientLight, 0.1f), Color.black, 0.1f));
		}

		public void LateUpdate()
		{
				if (!GameManager.Is(FGameState.InGame) || GameManager.Get.TestingEnvironment)
						return;

				if (!GameWorld.Get.Settings.GRTVisible) {
						GRTRenderer.enabled = false;
						return;
				} else {
						GRTRenderer.enabled = true;
				}

				tr.parent = null;
				//scale by world position
				CurrentDistance = Vector3.Distance(Player.Local.Position, ActualPosition.position);
				NormalizedDistance = Mathf.Clamp01((CurrentDistance - MinDistance) / (MaxDistance - MinDistance));
				//set position to the ground
				tr.position = transform.position.WithY(0f);
				tr.parent = mParent;
				tr.localScale = Vector3.one * Mathf.Lerp(ClosestDistanceSize, FarthestDistanceSize, NormalizedDistance);
				//transform.localScale = 1f;
		}

		protected Transform mParent;
}
