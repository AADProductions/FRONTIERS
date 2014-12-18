using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
		public class Mats : Manager
		{
				public static Mats Get;
				public IconManager Icons;
				public ArtifactMatManager ArtifactMats;
				public SetupAdvancedFoliageShader AFS;
				public Cubemap FoliageShaderDiffuseMap;
				public Cubemap FoliageShaderSpecMap;

				public override void WakeUp()
				{
						Get = this;
				}

				public override void Initialize()
				{
						//first set all the properties
						AFS.isLinear = false;
						AFS.diffuseIsHDR = false;
						AFS.specularIsHDR = false;
						AFS.UseLinearLightingFixTrees = false;
						AFS.useIBL = false;
						AFS.controlIBL = false;
						AFS.AFS_IBL_DiffuseExposure = 1f;
						AFS.AFS_IBL_SpecularExposure = 1f;
						AFS.AFS_IBL_MasterExposure = 1f;
						//we want to use the same fog settings as biomes
						AFS.AFSFog_Mode = (int)FogMode.Linear;

						//we want to use global ambient light color for shadows
						AFS.AutosyncShadowColor = true;
						//we want to use a sunlight reference
						AFS.BillboardLightReference = GameWorld.Get.Sky.Components.LightSource.gameObject;
						//no billboard shadows, just fade the shadows
						AFS.BillboardAdjustToCamera = true;
						AFS.BillboardShadowEdgeFade = false;
						AFS.TreeShadowDissolve = true;
						AFS.TreeBillboardShadows = false;
						//regular animation on grass not normal animation
						AFS.GrassAnimateNormal = false;
						//we want to render grass as a single pass
						AFS.AllGrassObjectsCombined = true;
						AFS.BillboardFadeOutLength = Globals.ChunkTerrainTreeDistance;
						//we're not using camera culling unfortunately
						AFS.EnableCameraLayerCulling = false;

						AFS.Wind = new Vector4(0.85f, 0.05f, 0.4f, 1f);
						AFS.WindFrequency = 1f;
						AFS.WaveSizeForGrassshader = 2f;
						AFS.WaveSizeFoliageShader = 10f;
						AFS.WindMultiplierForGrassshader = 5f;

						//then apply the properties
						//one-time stuff first
						AFS.afsCheckColorSpace();
						AFS.afsSetupColorSpace();

						AFS.afsLightingSettings();
						AFS.afsSetupTerrainEngine();
						AFS.afsSetupGrassShader();

						AFS.afsUpdateWind();
						AFS.afsUpdateRain();
						AFS.afsAutoSyncToTerrain();
						AFS.afsUpdateTreeAndBillboardShaders();
						AFS.afsUpdateGrassTreesBillboards();
						AFS.afsSetupCameraLayerCulling();

						mInitialized = true;
				}

				public void Update()
				{
						if (mInitialized) {
								AFS.afsUpdateGrassTreesBillboards();
								AFS.afsLightingSettings();
								AFS.afsUpdateWind();
								AFS.afsUpdateRain();

								WindowsMaterial.SetColor("_ReflectColor", Color.Lerp(Color.black, RenderSettings.ambientLight, 0.5f));
								//WaterWavesMaterial.SetFloat("_AnimSpeed", (float)WorldClock.Get.TimeScale);
						}
				}

				public Material BodyOfWaterMaterial;
				public Material RiverMaterial;
				public List <Material> TimedGlowMaterials = new List<Material>();
				public UIAtlas ConditionIconsAtlas;
				public UIAtlas IconsAtlas;
				public UIAtlas MapIconsAtlas;
				public UIAtlas PrimaryAtlas;
				public UIFont DyslexiaFont;
				public UIFont Arimo14Font;
				public UIFont Arimo18Font;
				public UIFont Arimo20Font;
				public UIFont BlackChancery32Font;
				public UIFont CleanHandwriting42Font;
				public UIFont MusicNotation1Font;
				public UIFont PrintingPress40Font;
				public UIFont SloppyHandwriting48Font;
				public UIFont TrajanPro18Font;
				public UIFont VerySloppyHandwriting48Font;
				public UIFont WolgastCursive72Font;
				public Material DefaultDiffuseMaterial;
				public Material WaveOverlayMaterial;
				public Material SnowOverlayMaterial;
				public Material TrailRendererMaterial;
				public Material WorldPathGroundMaterial;
				public Material BloodSplatterMaterial;
				public Material CharacterBodyMaterial;
				public Material CharacterFaceMaterial;
				public Material ItemPlacementMaterial;
				public Material ItemPlacementOutlineMaterial;
				public Material ItemPlacementOutlineCutoutMaterial;
				public Material FocusHighlightMaterial;
				public Material FocusOutlineMaterial;
				public Material FocusOutlineCutoutMaterial;
				public Material AttentionOutlineMaterial;
				public Material CraftingDoppleGangerMaterial;
				public Material InventoryRimMaterial;
				public Material InventoryRimCutoutMaterial;
				public Material WindowsMaterial;
				public Material LuminiteGlowMaterial;
				public Material WaterSurfaceMaterial;
				public Material MagicEffectMaterial;
				public Material SpellEffectProjectorMaterial;
				public Material ReceptacleProjectorMaterial;

				public static bool IsCutoutShader(Shader shader)
				{
						return shader.name.ToLower().Contains("cutout");
				}

				public Material [] MaterialsFromList(List <string> matNames)
				{
						List <Material> matList = new List <Material>();
						return matList.ToArray();
				}
		}
}