using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class WaterAnimScrolling : MonoBehaviour
	{
		public Vector2 FlowSpeed = new Vector2 (0.0015f, 0.0015f);
		public Vector2 WaveSpeed = new Vector2 (0.0015f, 0.0015f);
		public Vector2 FoamSpeed = new Vector2 (-0.02f, -0.02f);
		public Vector2 WaterForce = new Vector2 (0.0f, 0.0f);
		public Material WaterMaterial;
		public PathDirection FlowDirection;
		protected float animationSpeed = 1.0f;
		protected float m_fFlowMapOffset0 = 0.0f;
		protected float m_fFlowMapOffset1 = 0.0f;
		protected float m_fFlowSpeed = 0.05f;
		protected float m_fCycle = 1.0f;
		protected float m_fWaveMapScale = 2.0f;
		protected float m_flowDirection = 1f;
		protected Vector2 heightMap1Offset;
		protected Vector2 heightMap2Offset;
		protected Vector2 foamTexOffset;
		protected Renderer r;
		protected int updateColor;
		public bool updateColorAutomatically = true;

		public void Start ()
		{
			r = GetComponent<Renderer>();
			WaterMaterial = r.material;//get an instance of the material
			//get MASTER animation Speed
			animationSpeed = WaterMaterial.GetFloat ("_AnimSpeed");
			animationSpeed = Mathf.Clamp (animationSpeed, 0.0f, 1.0f);
		}

		public void Update ()
		{
			if (WaterMaterial == null || !r.isVisible) {
				return;
			}

			if (FlowDirection == PathDirection.Forward) {
				m_flowDirection = 1f;
			} else {
				m_flowDirection = -1f;
			}

			float flowAmount = (float)((WorldClock.AdjustedRealTime % WorldClock.gDayCycleRT) * animationSpeed * m_flowDirection);

			if (Application.isPlaying) {
				heightMap1Offset.Set (
					FlowSpeed.x * flowAmount, 
					FlowSpeed.y * flowAmount);
				heightMap2Offset.Set (
					WaveSpeed.x * flowAmount, 
					WaveSpeed.y * flowAmount);
				foamTexOffset.Set (
					FoamSpeed.x * flowAmount, 
					FoamSpeed.y * flowAmount);
			} else {
				//set speed limits
				WaveSpeed.x = Mathf.Clamp (WaveSpeed.x, -0.5f, 0.5f);
				WaveSpeed.y = Mathf.Clamp (WaveSpeed.y, -0.5f, 0.5f);
				FlowSpeed.x = Mathf.Clamp (FlowSpeed.x, -0.5f, 0.5f);
				FlowSpeed.y = Mathf.Clamp (FlowSpeed.y, -0.5f, 0.5f);
				FoamSpeed.x = Mathf.Clamp (FoamSpeed.x, -0.5f, 0.5f);
				FoamSpeed.y = Mathf.Clamp (FoamSpeed.y, -0.5f, 0.5f);

				heightMap1Offset.Set (
					FlowSpeed.x * Time.realtimeSinceStartup * animationSpeed * m_flowDirection,
					FlowSpeed.y * Time.realtimeSinceStartup * animationSpeed * m_flowDirection);
				heightMap2Offset.Set (
					WaveSpeed.x * Time.realtimeSinceStartup * animationSpeed * m_flowDirection,
					WaveSpeed.y * Time.realtimeSinceStartup * animationSpeed * m_flowDirection);
				foamTexOffset.Set (
					FoamSpeed.x * Time.realtimeSinceStartup * animationSpeed * m_flowDirection,
					FoamSpeed.y * Time.realtimeSinceStartup * animationSpeed * m_flowDirection);
			}

			//assign speed to shader
			WaterMaterial.SetTextureOffset ("_HeightMap", heightMap1Offset);
			WaterMaterial.SetTextureOffset ("_HeightMap2", heightMap2Offset);
			WaterMaterial.SetTextureOffset ("_FoamTex", foamTexOffset);

			updateColor++;

			if (updateColorAutomatically && updateColor > 15) {
				updateColor = 0;
				WaterMaterial.SetColor ("_FogColor", TOD_Sky.GlobalFogColor);
				WaterMaterial.SetColor ("_FoamColor", Colors.Alpha (Color.Lerp (Color.white, RenderSettings.fogColor, 0.25f), 0.25f));
				WaterMaterial.SetColor ("_ReflColor", GameWorld.Get.Sky.SunColor);
				WaterMaterial.SetColor ("_ReflColor2", RenderSettings.fogColor);
				WaterMaterial.SetColor ("_CrestColor", Colors.Alpha (Color.Lerp (Color.white, Colors.Brighten (RenderSettings.fogColor), 0.15f), 0.45f));
				WaterMaterial.SetColor ("_WaveColor", Colors.Alpha (Color.Lerp (Color.white, Colors.Desaturate (RenderSettings.fogColor), 0.15f), 0.55f));
				//renderer.material.SetTextureOffset("_FoamTex",Vector2(FoamSpeed.x*Time.time*animationSpeed,FoamSpeed.y*Time.time*animationSpeed));
			}
		}
	}
}