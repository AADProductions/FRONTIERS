using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class LavaRiver : MonoBehaviour
{
		public Renderer LavaRenderer;
		public Renderer DistortionRenderer;
		public Vector2 ScrollSpeed;
		public Vector2 DistortionScrollSpeed;
		public Spline MasterSpline;
		public Material LavaMaterial;
		public Material DistortionMaterial;
		public float LavaElevation;
		public float FlowTime = 1.0f;
		public float FlowSpread = 0.1f;
		public float FlowAmount = 10.0f;
		public float SpoutRTIntervalMin = 1f;
		public float SpoutRTIntervalMax = 10f;
		public List <Transform> LavaSpouts = new List <Transform>();
		public int CurrentSpoutIndex = 0;
		public string LavaSpoutFXName;

		public void Start()
		{
				LavaMaterial = LavaRenderer.material;
				DistortionMaterial = DistortionRenderer.material;
		}

		public void Update()
		{
				for (int i = 0; i < MasterSpline.splineNodesArray.Count; i++) {
						SplineNode node = MasterSpline.splineNodesArray[i];
						Vector3 nodePosition = node.Position;
						nodePosition.y = LavaElevation + (Mathf.Sin((float)(WorldClock.AdjustedRealTime * FlowTime) + (nodePosition.x * FlowSpread)) * FlowAmount);
						node.Position = nodePosition;

						LavaMaterial.SetTextureOffset("_MainTex", ScrollSpeed * (float)(WorldClock.AdjustedRealTime * FlowTime));
						LavaMaterial.SetTextureOffset("_Illum", ScrollSpeed * (float)(WorldClock.AdjustedRealTime * FlowTime));
						LavaMaterial.SetTextureOffset("_BumpMap", ScrollSpeed * (float)(WorldClock.AdjustedRealTime * FlowTime));
						DistortionMaterial.SetTextureOffset("_BumpMap", DistortionScrollSpeed * (float)(WorldClock.AdjustedRealTime * FlowTime));
				}

				if (WorldClock.AdjustedRealTime > mNextSpoutTime) {
						mNextSpoutTime = (float)(WorldClock.AdjustedRealTime + UnityEngine.Random.Range(SpoutRTIntervalMin, SpoutRTIntervalMax));
						CurrentSpoutIndex = LavaSpouts.NextIndex(CurrentSpoutIndex);
						FXManager.Get.SpawnFX(LavaSpouts[CurrentSpoutIndex].position, LavaSpoutFXName);
				}
		}

		protected float mNextSpoutTime = 0f;
}
