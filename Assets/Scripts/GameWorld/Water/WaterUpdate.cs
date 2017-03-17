using UnityEngine;
using System.Collections;
using Frontiers;
using ExtensionMethods;

public class WaterUpdate : MonoBehaviour
{
	public Vector2 flow_speed = new Vector2(0.0015f,0.0015f);
	public Vector2 wave_speed = new Vector2(0.0015f,0.0015f);
	public Vector2 foam_speed = new Vector2(-0.02f,-0.02f);
	public Vector2 waterForce = new Vector2(0.0f,0.0f);
	public double WaterScale = 2000f;

	protected float animationSpeed = 1.0f;
	protected float m_fFlowMapOffset0  = 0.0f;
	protected float m_fFlowMapOffset1 = 0.0f;
	protected float m_fFlowSpeed = 0.05f;
	protected float m_fCycle = 1.0f;
	protected float m_fWaveMapScale = 2.0f;

	public double Heightmap1Tiling = 10f;
	public double Heightmap2Tiling = 30f;
	public double FoamTextureTiling = 25f;

	public Renderer WaterSurfaceRenderer;
	public Renderer WaterWavesRenderer;
	public Material WaterSurfaceMaterial;

	public double OffsetX;
	public double OffsetY;
	public Vector2 HeightMap1Offset;
	public Vector2 HeightMap2Offset;
	public Vector2 FoamTexOffset;	

	public void LateUpdate ( )
	{	
		if (!GameManager.Is (FGameState.InGame))
			return;

		GetComponent<Renderer>().material.SetColor ("_FogColor", TOD_Sky.GlobalFogColor);// Color.Lerp (RenderSettings.ambientLight, RenderSettings.fogColor, 0.5f));//Biomes.Get.CurrentWaterFogColor);
		GetComponent<Renderer>().material.SetColor ("_FoamColor", Color.Lerp (GameWorld.Get.Sky.SunColor, Color.white, 0.85f));// Biomes.Get.CurrentWaterFoamColor);
		GetComponent<Renderer>().material.SetColor ("_CrestColor", Color.Lerp (GameWorld.Get.Sky.SunColor, Color.white, 0.85f));
		GetComponent<Renderer>().material.SetColor ("_WaveColor", Color.Lerp (GameWorld.Get.Sky.SunColor, Color.white, 0.85f));
		GetComponent<Renderer>().material.SetFloat ("_FogFar", RenderSettings.fogEndDistance);

		OffsetX = transform.position.x * WaterScale;
        OffsetY = transform.position.z * WaterScale;

		//get MASTER animation Speed
		animationSpeed = GetComponent<Renderer>().material.GetFloat("_AnimSpeed");
//		animationSpeed = Mathf.Clamp01 (animationSpeed);

//		//set speed limits
//		wave_speed.x = Mathf.Clamp (wave_speed.x,-0.5f,0.5f);
//		wave_speed.y = Mathf.Clamp (wave_speed.y,-0.5f,0.5f);
//		flow_speed.x = Mathf.Clamp (flow_speed.x,-0.5f,0.5f);
//		flow_speed.y = Mathf.Clamp (flow_speed.y,-0.5f,0.5f);
//		foam_speed.x = Mathf.Clamp (foam_speed.x,-0.5f,0.5f);
//		foam_speed.y = Mathf.Clamp (foam_speed.y,-0.5f,0.5f);

		//assign speed to shader
		HeightMap1Offset.x = (float) (((flow_speed.x * WorldClock.AdjustedRealTime * animationSpeed) - OffsetX / Heightmap1Tiling) % Heightmap1Tiling);
		HeightMap1Offset.y = (float) (((flow_speed.y * WorldClock.AdjustedRealTime * animationSpeed) - OffsetY / Heightmap1Tiling) % Heightmap1Tiling);

		HeightMap2Offset.x = (float) (((wave_speed.x * WorldClock.AdjustedRealTime * animationSpeed) - OffsetX / Heightmap2Tiling) % Heightmap2Tiling);
		HeightMap2Offset.y = (float) (((wave_speed.y * WorldClock.AdjustedRealTime * animationSpeed) - OffsetY / Heightmap2Tiling) % Heightmap2Tiling);

		FoamTexOffset.x = (float) (((foam_speed.x * WorldClock.AdjustedRealTime * animationSpeed) - OffsetX / FoamTextureTiling) % FoamTextureTiling);
		FoamTexOffset.y = (float) (((foam_speed.y * WorldClock.AdjustedRealTime * animationSpeed) - OffsetY / FoamTextureTiling) % FoamTextureTiling);

		GetComponent<Renderer>().material.SetTextureOffset("_HeightMap", HeightMap1Offset);
		GetComponent<Renderer>().material.SetTextureOffset("_HeightMap2", HeightMap2Offset);
		GetComponent<Renderer>().material.SetTextureOffset("_FoamTex", FoamTexOffset);
	}
}
