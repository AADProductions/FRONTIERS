using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers.Data;
using Frontiers.World;
using System.Text.RegularExpressions;
using Frontiers.Story;
using System.Globalization;

namespace Frontiers
{
		[Serializable]
		public class TerrainTextureTemplate
		{
				public string DiffuseName = string.Empty;
				public string NormalName = string.Empty;
				public SVector2 Size = SVector2.zero;
				public SVector2 Offset = SVector2.zero;
		}

		[Serializable]
		public class TerrainPrototypeTemplate
		{
				//frontiers-specific data
				//Mode tells the mode changer when to include this template
				public ChunkMode Mode = ChunkMode.Immediate;
				public string AssetName;
				public PrototypeTemplateType Type = PrototypeTemplateType.TreeMesh;
				public DetailRenderMode RenderMode = DetailRenderMode.Grass;
				//detail texture
				public float MinWidth = 0.5f;
				public float MinHeight = 0.5f;
				public float MaxWidth = 1.0f;
				public float MaxHeight = 1.0f;
				public float NoiseSpread = 2.5f;
				public SColor HealthyColor = Color.white;
				public SColor DryColor = Color.white;
				public bool UsePrototypeMesh = false;
				//detail mesh
				public float RandomWidth {
						get {
								return MaxWidth;
						}
				}

				public float RandomHeight {
						get {
								return MaxHeight;
						}
				}
				//tree mesh
				public float BendFactor = 3.0f;
		}

		[Serializable]
		public class TerrainkMaterialSettings
		{
				public void GetSettings(Material atsMaterial)
				{
						Vector4 terrainCombinedFloats = atsMaterial.GetVector("_terrainCombinedFloats");
						MultiUV = terrainCombinedFloats.x;
						Desaturation = terrainCombinedFloats.y;
						SplattingDistance = terrainCombinedFloats.z;
						TerrainSpecPower = terrainCombinedFloats.w;

						TerrainSpecColor = atsMaterial.GetColor("_SpecColor");

						Splat0Tiling = atsMaterial.GetFloat("_Splat0Tiling");
						Splat1Tiling = atsMaterial.GetFloat("_Splat1Tiling");
						Splat2Tiling = atsMaterial.GetFloat("_Splat2Tiling");
						Splat3Tiling = atsMaterial.GetFloat("_Splat3Tiling");
						Splat4Tiling = atsMaterial.GetFloat("_Splat4Tiling");
						Splat5Tiling = atsMaterial.GetFloat("_Splat5Tiling");

						Texture1Average = atsMaterial.GetColor("_ColTex1");
						Texture2Average = atsMaterial.GetColor("_ColTex2");
						Texture3Average = atsMaterial.GetColor("_ColTex3");
						Texture4Average = atsMaterial.GetColor("_ColTex4");
						Texture5Average = atsMaterial.GetColor("_ColTex5");
						Texture6Average = atsMaterial.GetColor("_ColTex6");

						Texture1Shininess = atsMaterial.GetFloat("_Spec1");
						Texture2Shininess = atsMaterial.GetFloat("_Spec2");
						Texture3Shininess = atsMaterial.GetFloat("_Spec3");
						Texture4Shininess = atsMaterial.GetFloat("_Spec4");
						Texture5Shininess = atsMaterial.GetFloat("_Spec5");
						Texture6Shininess = atsMaterial.GetFloat("_Spec6");

						Texture cn12 = atsMaterial.GetTexture("_CombinedNormal12");
						if (cn12 != null) {
								CombinedNormals12 = cn12.name;
						} else {
								CombinedNormals12 = string.Empty;
						}
						Texture cn34 = atsMaterial.GetTexture("_CombinedNormal34");
						if (cn34 != null) {
								CombinedNormals34 = cn34.name;
						} else {
								CombinedNormals34 = string.Empty;
						}
						Texture cn56 = atsMaterial.GetTexture("_CombinedNormal56");
						if (cn56 != null) {
								CombinedNormals56 = cn56.name;
						} else {
								CombinedNormals56 = string.Empty;
						}

						Vector4 fresnelSettings = atsMaterial.GetVector("_Fresnel");
						FresnelIntensity = fresnelSettings.x;
						FresnelPower = fresnelSettings.y;
						FresnelBias = fresnelSettings.z;

						/*			
						_CustomColorMap ("Color Map (RGB)", 2D) = "white" {}
						_TerrainNormalMap ("Terrain Normalmap", 2D) = "bump" {}
						_Control ("SplatAlpha 0", 2D) = "red" {}
						_Control2nd ("SplatAlpha 1", 2D) = "black" {}

						_terrainCombinedFloats ("MultiUV, Desaturation, Splatting Distance, Specular Power", Vector) = (0.5,600.0,0.5,1.0)
						_SpecColor ("Terrain Specular Color", Color) = (0.5, 0.5, 0.5, 1)

						_Splat0 ("Layer 0 (R)", 2D) = "white" {}
						_Splat0Tiling ("Tiling Detail Texture 1", Float) = 100
						_Splat1 ("Layer 1 (G)", 2D) = "white" {}
						_Splat1Tiling ("Tiling Detail Texture 2", Float) = 100
						_Splat2 ("Layer 2 (B)", 2D) = "white" {}
						_Splat2Tiling ("Tiling Detail Texture 3", Float) = 100
						_Splat3 ("Layer 3 (A)", 2D) = "white" {}
						_Splat3Tiling ("Tiling Detail Texture 4", Float) = 100
						_Splat4 ("Layer 4 (R)", 2D) = "white" {}
						_Splat4Tiling ("Tiling Detail Texture 5", Float) = 100
						_Splat5 ("Layer 5 (G)", 2D) = "white" {}
						_Splat5Tiling ("Tiling Detail Texture 6", Float) = 100

						// color correction and spec values
						_ColTex1 ("Avrg. Color Tex 1", Color) = (.5,.5,.5,1)
						_Spec1 ("Shininess Tex 1", Range (0.03, 1)) = 0.078125
						_ColTex2 ("Avrg. Color Tex 2", Color) = (.5,.5,.5,1)
						_Spec2 ("Shininess Tex 2", Range (0.03, 1)) = 0.078125
						_ColTex3 ("Avrg. Color Tex 3", Color) = (.5,.5,.5,1)
						_Spec3 ("Shininess Tex 3", Range (0.03, 1)) = 0.078125
						_ColTex4 ("Avrg. Color Tex 4", Color) = (.5,.5,.5,1)
						_Spec4 ("Shininess Tex 4", Range (0.03, 1)) = 0.078125
						_ColTex5 ("Avrg. Color Tex 5", Color) = (.5,.5,.5,1)
						_Spec5 ("Shininess Tex 5", Range (0.03, 1)) = 0.078125
						_ColTex6 ("Avrg. Color Tex 6", Color) = (.5,.5,.5,1)
						_Spec6 ("Shininess Tex 6", Range (0.03, 1)) = 0.078125

						_Decal1_ColorCorrectionStrenght ("Decal 1 Color Correction Strength", Range (0, 1)) = 0.5
						_Decal1_Sharpness ("Decal 1 Sharpness", Range (0, 32)) = 16
						_Decal2_ColorCorrectionStrenght ("Decal 2 Color Correction Strength", Range (0, 1)) = 0.5
						_Decal2_Sharpness ("Decal 2 Sharpness", Range (0, 32)) = 16

						_CombinedNormal12 (" Combined Normal 1 (RG) Normal 2 (BA)", 2D) = "white" {}
						_CombinedNormal34 (" Combined Normal 3 (RG) Normal 4 (BA)", 2D) = "white" {}
						_CombinedNormal56 (" Combined Normal 5 (RG) Normal 6 (BA)", 2D) = "white" {}

						_Fresnel ("Fresnel: Intensity/Power/Bias/-)", Vector) = (2.0, 1.5, -0.5,0.0)
						_ReflectionColor ("Terrain Reflection Color", Color) = (1,1,1,1)

						_Elev ("Elevation for Tex 1-4)", Vector) = (1.0, 1.0, 1.0, 1.0)
						_Elev1 ("Elevation for Tex 5-6)", Vector) = (1.0, 1.0, 1.0, 1.0)
						*/
				}

				public void ApplySettings(Material atsMaterial)
				{
						/*
						Vector4 terrainCombinedFloats = new Vector4 (MultiUV, Desaturation, SplattingDistance, TerrainSpecPower);
						atsMaterial.SetVector ("_terrainCombinedFloats", terrainCombinedFloats);

						atsMaterial.SetColor ("_SpecColor", TerrainSpecColor);
						*/

						atsMaterial.SetFloat("_Splat0Tiling", Splat0Tiling);
						atsMaterial.SetFloat("_Splat1Tiling", Splat1Tiling);
						atsMaterial.SetFloat("_Splat2Tiling", Splat2Tiling);
						atsMaterial.SetFloat("_Splat3Tiling", Splat3Tiling);
						atsMaterial.SetFloat("_Splat4Tiling", Splat4Tiling);
						atsMaterial.SetFloat("_Splat5Tiling", Splat5Tiling);

						atsMaterial.SetColor("_ColTex1", Texture1Average);
						atsMaterial.SetColor("_ColTex2", Texture2Average);
						atsMaterial.SetColor("_ColTex3", Texture3Average);
						atsMaterial.SetColor("_ColTex4", Texture4Average);
						atsMaterial.SetColor("_ColTex5", Texture5Average);
						atsMaterial.SetColor("_ColTex6", Texture6Average);

						/*
						atsMaterial.SetFloat ("_Spec1", Texture1Shininess);
						atsMaterial.SetFloat ("_Spec2", Texture2Shininess);
						atsMaterial.SetFloat ("_Spec3", Texture3Shininess);
						atsMaterial.SetFloat ("_Spec4", Texture4Shininess);
						atsMaterial.SetFloat ("_Spec5", Texture5Shininess);
						atsMaterial.SetFloat ("_Spec6", Texture6Shininess);

						Vector4 fresnelSettings	= new Vector4 (FresnelIntensity, FresnelPower, FresnelBias, 0f);
						atsMaterial.SetVector ("_Fresnel", fresnelSettings);
						*/
				}

				public void ApplyMaps(Material atsMaterial, string chunkName, Dictionary <string, Texture2D> maps)
				{
						ApplyMap("ColorOverlay", "_CustomColorMap", maps, atsMaterial);
						ApplyMap("NormalOverlay", "_TerrainNormalMap", maps, atsMaterial);
						ApplyMap("Splat1", "_Control", maps, atsMaterial);
						ApplyMap("Splat2", "_Control2nd", maps, atsMaterial);

						/*
						Texture2D splat = null;
						if (maps.TryGetValue ("Splat1", out splat)) {
							atsMaterial.SetTexture ("_Control", splat);
						}
						if (maps.TryGetValue ("Splat2", out splat)) {
							atsMaterial.SetTexture ("_Control2nd", splat);
						}

						ApplyMap ("Splat1", "_Control", maps, atsMaterial);
						ApplyMap ("Splat2", "_Control2nd", maps, atsMaterial);
						*/

						ApplyMap("Ground0", "_Splat0", maps, atsMaterial);
						ApplyMap("Ground1", "_Splat1", maps, atsMaterial);
						ApplyMap("Ground2", "_Splat2", maps, atsMaterial);
						ApplyMap("Ground3", "_Splat3", maps, atsMaterial);
						ApplyMap("Ground4", "_Splat4", maps, atsMaterial);
						ApplyMap("Ground5", "_Splat5", maps, atsMaterial);

						Texture2D cn12 = null;
						if (Mats.Get.GetTerrainGroundTexture(CombinedNormals12, out cn12)) {
								atsMaterial.SetTexture("_CombinedNormal12", cn12);
						}
						Texture2D cn34 = null;
						if (Mats.Get.GetTerrainGroundTexture(CombinedNormals34, out cn34)) {
								atsMaterial.SetTexture("_CombinedNormal34", cn34);
						}
						Texture2D cn56 = null;
						if (Mats.Get.GetTerrainGroundTexture(CombinedNormals56, out cn56)) {
								atsMaterial.SetTexture("_CombinedNormal56", cn56);
						}

						/*			
						_CustomColorMap ("Color Map (RGB)", 2D) = "white" {}
						_TerrainNormalMap ("Terrain Normalmap", 2D) = "bump" {}
						_Control ("SplatAlpha 0", 2D) = "red" {}
						_Control2nd ("SplatAlpha 1", 2D) = "black" {}

						_CombinedNormal12 (" Combined Normal 1 (RG) Normal 2 (BA)", 2D) = "white" {}
						_CombinedNormal34 (" Combined Normal 3 (RG) Normal 4 (BA)", 2D) = "white" {}
						_CombinedNormal56 (" Combined Normal 5 (RG) Normal 6 (BA)", 2D) = "white" {}
						*/
				}

				protected void	ApplyMap(string mapName, string propertyName, Dictionary <string, Texture2D> maps, Material atsMaterial)
				{
						Texture2D map = null;
						if (maps.TryGetValue(mapName, out map)) {
								//Debug.Log ("Setting map name " + mapName);
								atsMaterial.SetTexture(propertyName, map);
						} else {
								//Debug.Log ("Couldn't get map name " + mapName);
						}
				}
				//used to store ATS material settings
				public string CombinedNormals12 = "CombinedNormalsA";
				public string CombinedNormals34 = "CombinedNormalsB";
				public string CombinedNormals56 = "CombinedNormalsC";
				public SColor	TerrainSpecColor = new SColor(0.25f, 0.25f, 0.25f, 0f);
				public float	TerrainSpecPower = 0.25f;
				public SColor	Texture1Average = Color.black;
				public SColor Texture2Average = Color.black;
				public SColor	Texture3Average = Color.black;
				public SColor	Texture4Average = Color.black;
				public SColor	Texture5Average = Color.black;
				public SColor	Texture6Average = Color.black;
				public float	Texture1Shininess = 0.07812f;
				public float	Texture2Shininess = 0.07812f;
				public float	Texture3Shininess = 0.07812f;
				public float	Texture4Shininess = 0.07812f;
				public float	Texture5Shininess = 0.07812f;
				public float	Texture6Shininess = 0.07812f;
				public float	MultiUV = 0.5f;
				public float	Desaturation = 0.5f;
				public float	SplattingDistance = 600.0f;
				public float	Decal1CCStrength = 0.5f;
				public float	Decal1Sharpness = 0.5f;
				public float	Decal2CCStrength = 0.5f;
				public float	Decal2Sharpness = 0.5f;
				public float	Splat0Tiling = 100.0f;
				public float	Splat1Tiling = 100.0f;
				public float	Splat2Tiling = 100.0f;
				public float	Splat3Tiling = 100.0f;
				public float	Splat4Tiling = 100.0f;
				public float	Splat5Tiling = 100.0f;
				public float	FresnelIntensity = 2.0f;
				public float	FresnelPower = 1.5f;
				public float	FresnelBias = -0.5f;
		}

		[Serializable]
		public class EventSequence : Mod
		{
				public string Commands = string.Empty;

				public bool GetNext(ref SequenceStep step)
				{
						if (mStepQueue == null) {
								BuildStepStack();
						}

						if (mStepQueue.Count > 0) {
								step = mStepQueue.Dequeue();
								return true;
						}
						return false;
				}

				protected void BuildStepStack()
				{
						mStepQueue = new Queue <SequenceStep>();

						string[] commandLines = Commands.Split(gNewlineSeparators, StringSplitOptions.RemoveEmptyEntries);
						foreach (string commandLine in commandLines) {
								string[] splitCommand	= commandLine.Split(gSpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
								//Command Target Value Duration
								SequenceStep step = new SequenceStep();
								step.Duration = Single.Parse(splitCommand[0]);
								step.Command = splitCommand[1];
								step.Target = splitCommand[2];
								if (splitCommand.Length > 3)
										step.Value	= splitCommand[3];
								else
										step.Value	= string.Empty;

								if (splitCommand.Length > 4)
										step.Assignment = splitCommand[4];
								else
										step.Assignment = string.Empty;

								mStepQueue.Enqueue(step);
						}
				}

				protected Queue <SequenceStep> mStepQueue = null;
				protected static string[] gNewlineSeparators = new string [] {
						"\n",
						"\n\r"
				};
				protected static string[] gSpaceSeparators = new string [] { "\t" };

				public struct SequenceStep
				{
						public float Duration;
						public string Command;
						public string Target;
						public string Value;
						public string Assignment;
				}
		}

		[Serializable]
		public class WorldState
		{
				public List <string> DestroyedQuestItems = new List <string>();
		}

		[Serializable]
		public class PlayerStartupPosition : Mod
		{
				public bool CanBeUsedForNewGame = false;
				public PlayerIDFlag PlayerID = PlayerIDFlag.Local;
				//where to put the player
				public int ChunkID;
				public STransform ChunkPosition;
				LocationTerrainType LocationType = LocationTerrainType.AboveGround;
				[XmlIgnore]
				public STransform WorldPosition = new STransform();

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public bool RequiresStructure {
						get {
								return !string.IsNullOrEmpty(StructureName);
						}
				}

				public bool Interior = false;
				public MobileReference LocationReference;
				public string StructureName;
				public bool ClearInventory = false;
				public bool DestroyClearedItems = true;
				public bool ClearWearables = false;
				public bool ClearLog = false;
				public string CharacterName = string.Empty;
				//what time it should be
				public bool AbsoluteTime = false;
				//whether to add the time or set it outright
				public float TimeHours = 0f;
				public float TimeDays = 0f;
				public float TimeMonths = 0f;
				public float TimeYears = 0f;
				//stuff we could be standing on
				public bool RequiresMeshTerrain = false;
				//player stater
				public string ControllerState;
				public string InventoryFillCategory;
				public string WearableFillCategory;
				public List <string> BooksToAdd = new List<string>();// { "PlayerSurvivalGuideIndex", "PlayerSkillGuideIndex" };
				public List <StatusKeeperValue> StatusValues = new List<StatusKeeperValue>();
				public bool ClearRevealedLocations = false;
				public List <CurrencyValue> CurrencyToAdd = new List<CurrencyValue>();
				public List <MobileReference> NewLocationsToReveal = new List<MobileReference>();
				public List <MobileReference> NewVisitedRespawnStructures = new List<MobileReference>();

				[Serializable]
				public class StatusKeeperValue
				{
						public string StatusKeeperName = "Health";
						public float Value = 1.0f;
				}
		}

		[Serializable]
		public class CurrencyValue
		{
				public WICurrencyType Type = WICurrencyType.A_Bronze;
				public int Number = 0;
		}

		[Serializable]
		public class WorldSettings : Mod
		{
				public WorldSettings()
				{
						Type = "World";
						Name = "FRONTIERS";
						Description = "A new world.";
				}

				public SColor DefaultTerrainType = Color.black;
				[FrontiersAvailableModsAttribute("Biome")]
				public string DefaultBiome;
				public AmbientAudioManager.ChunkAudioSettings DefaultAmbientAudio = new AmbientAudioManager.ChunkAudioSettings();
				public AmbientAudioManager.ChunkAudioItem DefaultAmbientAudioInterior = new AmbientAudioManager.ChunkAudioItem();

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public bool RequiresCompletedWorlds {
						get {
								return RequiredCompletedWorlds.Count > 0;
						}
				}

				public List <string> RequiredCompletedWorlds = new List <string>();
				[FrontiersAvailableModsAttribute("Music")]
				public string DefaultMusicCombat;
				[FrontiersAvailableModsAttribute("Music")]
				public string DefaultMusicCutscene;
				[FrontiersAvailableModsAttribute("Music")]
				public string DefaultMusicMainMenu;
				[FrontiersAvailableModsAttribute("Music")]
				public string DefaultMusicNight;
				[FrontiersAvailableModsAttribute("Music")]
				public string DefaultMusicRegional;
				[FrontiersAvailableModsAttribute("Music")]
				public string DefaultMusicSafeLocation;
				[FrontiersAvailableModsAttribute("Music")]
				public string DefaultMusicUnderground;
				public float TimeHours = 0f;
				public float TimeDays = 0f;
				public float TimeMonths = 0f;
				public float TimeYears = 0f;
				public int WorldChunkTerrainHeightmapResolution = Globals.WorldChunkTerrainHeightmapResolution;
				public int MaxSpawnedChunks = Globals.MaxSpawnedChunks;
				public List <MobileReference> DefaultRevealedLocations = new List<MobileReference>();
				public CharacterFlags DefaultResidentFlags = new CharacterFlags();
				public WIFlags DefaultContainerFlags = new WIFlags();
				public ChunkBiomeData DefaultBiomeData = new ChunkBiomeData();
				public List <string> BaseDifficultySettingNames	= new List <string>();
				public MobileReference DefaultHouseOfHealing = new MobileReference();
				public int NumChunkTilesX;
				public int NumChunkTilesZ;
				[NonSerialized]
				public List <DifficultySetting> BaseDifficultySettings = new List <DifficultySetting>();
				//these are used the first time you enter the game
				//this includes positions for all multiplayer players
				[FrontiersAvailableModsAttribute("PlayerStartupPosition")]
				public string FirstStartupPosition = "PrologueSpawn";
		}

		[Serializable]
		public class DifficultySetting : Mod
		{
				public DifficultySetting () : base () { }

				public bool IsDefined(string setting)
				{
						if (DifficultyFlags != null && !string.IsNullOrEmpty(setting)) {
								return DifficultyFlags.Contains(setting);
						}
						return false;
				}

				[XmlIgnore]
				public bool HasBeenCustomized = false;
				public FallDamageStyle FallDamage = FallDamageStyle.Forgiving;
				public DifficultyDeathStyle DeathStyle = DifficultyDeathStyle.Respawn;
				public List <DifficultySettingGlobal> GlobalVariables = new List<DifficultySettingGlobal> ();
				// = new List <DifficultySettingGlobal> ();
				public List <string> DifficultyFlags = new List<string>();
				// = new List <string> ();
				public void Apply( )
				{
						//set the globals to default
						//then apply the difficulty setting on top of the default values
						List <KeyValuePair <string,string>> globalPairs = null;
						string errorMessage = null;
						if (GameData.IO.LoadGlobals(ref globalPairs, out errorMessage)) {
								Globals.LoadDifficultySettingData(globalPairs);
								for (int i = 0; i < GlobalVariables.Count; i++) {
										Globals.SetDifficultyVariable(GlobalVariables[i].GlobalVariableName, GlobalVariables[i].VariableValue);
								}
						}
				}

				protected void GenerateFullDescription()
				{
						if (GlobalVariables != null && DifficultyFlags != null) {
								List <string> descriptionLines = new List <string>();
								descriptionLines.Add(Description);
								descriptionLines.Add("_");
								switch (DeathStyle) {
										case DifficultyDeathStyle.NoDeath:
												descriptionLines.Add("Death: If your health reaches zero nothing will happen.");
												break;

										case DifficultyDeathStyle.Respawn:
										default:
												descriptionLines.Add("Death: If your health reaches zero you will black out for a short time. Upon waking half of your money will be gone.");
												break;

										case DifficultyDeathStyle.PermaDeath:
												descriptionLines.Add("Death: If your health reaches zero your game is over permanently.");
												break;
								}
								descriptionLines.Add("Globals defined in this setting:");
								if (GlobalVariables != null && GlobalVariables.Count > 0) {
										for (int i = 0; i < GlobalVariables.Count; i++) {
												descriptionLines.Add(GlobalVariables[i].Description);
										}
								} else {
										descriptionLines.Add("(None)");
								}
								descriptionLines.Add("Flags defined in this setting:");
								if (DifficultyFlags.Count > 0) {

										descriptionLines.Add(GameData.CommaJoinWithLast(DifficultyFlags, "and"));
								} else {
										descriptionLines.Add("(None)");
								}
								mFullDescription = descriptionLines.JoinToString("\n");
						}
				}

				public static List <string> AvailableTags {
						get {
								if (mAvailableTags == null) {
										mAvailableTags = new List<string>();
								}
								return mAvailableTags;
						}
				}

				protected static List <string> mAvailableTags = null;
		}

		[Serializable]
		public class DifficultySettingGlobal
		{
				public DifficultySettingGlobal () { }

				public string GlobalVariableName;
				public string VariableValue;
				public string Description;
		}

		[Serializable]
		public class ChunkTriggerData : Mod
		{
				public ChunkTriggerData() : base ()
				{
						Type = "ChunkTriggerData";
						Name = "Trigger Group";
						Description = "A group of triggers";
				}

				public SDictionary <string, KeyValuePair <string,string>> TriggerStates = new SDictionary <string, KeyValuePair <string,string>>();
		}

		[Serializable]
		public class ChunkNodeData : Mod
		{
				public ChunkNodeData()
				{
						Type = "ChunkNodeData";
						Name = "Node Group";
						Description = "A group of nodes";
				}

				public SDictionary <string, List <ActionNodeState>> NodeStates = new SDictionary <string, List <ActionNodeState>>();
				public List <TerrainNode> TerrainNodes = new List <TerrainNode>();
		}

		[Serializable]
		public struct TerrainNode
		{
				public TerrainNode(Vector3 chunkPosition)
				{
						X = chunkPosition.x;
						Y = chunkPosition.y;
						Z = chunkPosition.z;
						ID = 0;
						Parent = 0;
						Type = 0;
						Terrain = 0;
						Location = 0;
				}

				[XmlIgnore]
				public Vector3 Position {
						get {
								return new Vector3(X, Y, Z);
						}
						set {
								X = value.x;
								Y = value.y;
								Z = value.z;
						}
				}

				[XmlIgnore]
				public LocationTerrainType LocationType {
						get {
								LocationTerrainType location = LocationTerrainType.AboveGround;
								switch (Location) {
										case 0:
												Location = 1;
												break;

										case 1:
										default:
												break;

										case 2:
												location = LocationTerrainType.BelowGround;
												break;

										case 3:
												location = LocationTerrainType.Transition;
												break;
								}
								return location;
						}
						set {
								switch (value) {
										case LocationTerrainType.AboveGround:
										default:
												Location = 1;
												break;

										case LocationTerrainType.BelowGround:
												Location = 2;
												break;

										case LocationTerrainType.Transition:
												Location = 3;
												break;
								}
						}
				}

				public float X;
				public float Y;
				public float Z;
				public int ID;
				public int Parent;
				public int Type;
				public int Terrain;
				public int Location;
		}

		[Serializable]
		public class ChunkRegionData : Mod
		{
				public CharacterFlags ResidentFlags = new CharacterFlags();
				public WIFlags StructureFlags = new WIFlags();
		}

		[Serializable]
		public class Region : Mod
		{
				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public int RegionID = 0;
				[FrontiersBitMaskAttribute("Region")]
				public int RegionFlag;
				public CharacterFlags ResidentFlags = new CharacterFlags();
				public WIFlags StructureFlags = new WIFlags();
				[FrontiersAvailableModsAttribute("Music")]
				public string DayMusic;
				[FrontiersAvailableModsAttribute("Music")]
				public string NightMusic;
				[FrontiersAvailableModsAttribute("Music")]
				public string UndergroundMusic;
				[SColorAttribute]
				public SColor BannerColor;
				[SColorAttribute]
				public SColor SymbolColor;
				public string Symbol;
				public MobileReference Capital = new MobileReference();
				public WISize RegionSize = WISize.Small;
				public List <string> MaleFirstNames = new List <string>();
				public List <string> FemaleFirstNames = new List <string>();
				public List <string> FamilyNames = new List <string>();
				public float LocalNameUsage = 0.5f;
				public MobileReference DefaultRespawnStructure = new MobileReference();
		}

		[Serializable]
		public class Biome : Mod
		{
				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public int BiomeID = 0;
				[FrontiersBitMaskAttribute("Climate")]
				public int BiomeFlag = 0;
				[FrontiersAvailableModsAttribute("AudioProfile")]
				public string SummerAudioProfile;
				[FrontiersAvailableModsAttribute("AudioProfile")]
				public string AutumnAudioProfile;
				[FrontiersAvailableModsAttribute("AudioProfile")]
				public string WinterAudioProfile;
				[FrontiersAvailableModsAttribute("AudioProfile")]
				public string SpringAudioProfile;
				public ClimateType Climate = ClimateType.Temperate;
				[FrontiersAvailableModsAttribute("CameraLut")]
				public string ColorSetting = "TemperateRegion";
				public string ColorSettingNight = "TemperateRegionNight";
				public float AmbientLightMultiplier = 1.0f;
				public float SunlightIntensityMultiplier = 1.0f;
				public float ExposureMultiplier = 1.0f;
				public float PrecipitationLevel = 0.5f;
				public float TideVariation = 5.0f;
				public float WaveIntensity = 0.15f;
				public float TideBaseElevation = 15f;
				public float WaveSpeed = 4f;
				public float FogDistanceMultiplier = 1f;
				public List<string> DayCritterTypes = new List<string>();
				public List<string> NightCritterTypes = new List<string>();
				public float CritterDensity = 1f;
				public BiomeStatusTemps StatusTempsSummer = new BiomeStatusTemps();
				public BiomeStatusTemps StatusTempsSpring = new BiomeStatusTemps();
				public BiomeStatusTemps StatusTempsAutumn = new BiomeStatusTemps();
				public BiomeStatusTemps StatusTempsWinter = new BiomeStatusTemps();
				public BiomeWeatherSetting WeatherSummer = new BiomeWeatherSetting();
				public BiomeWeatherSetting WeatherSpring = new BiomeWeatherSetting();
				public BiomeWeatherSetting WeatherAutumn = new BiomeWeatherSetting();
				public BiomeWeatherSetting WeatherWinter = new BiomeWeatherSetting();
				[XmlIgnore]
				[NonSerialized]
				public WeatherSetting[] Almanac = null;

				public WeatherQuarter GetWeather(int dayOfYear, int hourOfDay)
				{
						if (Almanac == null) {
								GenerateAlmanac();
						}

						WeatherQuarter weather = null;
						if (dayOfYear < Almanac.Length) {
								if (hourOfDay < 6) {
										weather = Almanac[dayOfYear].QuarterMorning;
								} else if (hourOfDay < 12) {
										weather = Almanac[dayOfYear].QuarterAfternoon;
								} else if (hourOfDay < 18) {
										weather = Almanac[dayOfYear].QuarterEvening;
								} else {
										weather = Almanac[dayOfYear].QuarterNight;
								}
						}
						return weather;
				}

				public void GenerateAlmanac()//365 days
				{
						Almanac = new WeatherSetting [365];

						WeatherSummer.Normalize();
						WeatherSpring.Normalize();
						WeatherAutumn.Normalize();
						WeatherWinter.Normalize();

						WeatherSummer.GenerateLookup();
						WeatherSpring.GenerateLookup();
						WeatherAutumn.GenerateLookup();
						WeatherWinter.GenerateLookup();

						//this creates a simple lookup table of weather values for this region
						BiomeWeatherSetting weather = null;
						WeatherSetting setting = new WeatherSetting();
						for (int i = 0; i < 365; i++) {
								//spring starts on day 45, ends on day 139 (95)
								//summer starts on day 140, ends on day 229 (90)
								//autumn starts on day 230, ends on day 319 (90)
								//winter starts on day 320, ends on day 49 (90)
								if (i >= 320 || i < 45) {
										//winter
										weather = WeatherWinter;
								} else if (i >= 230) {
										//autumn
										weather = WeatherAutumn;
								} else if (i >= 140) {
										//summer
										weather = WeatherSummer;
								} else {
										//spring
										weather = WeatherSpring;
								}
								//get the weather type
								//this will drive the wind / precipitation values
								////Debug.Log ("Getting cloud type for in weather cloud lookup, length is " + weather.CloudTypeLookup.Length.ToString ());
								setting.QuarterMorning.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];
								setting.QuarterAfternoon.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];
								setting.QuarterEvening.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];
								setting.QuarterNight.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];

								setting.QuarterMorning.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];
								setting.QuarterAfternoon.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];
								setting.QuarterEvening.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];
								setting.QuarterNight.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];

								//do precipitation by the day, then adjust it based on weather and clouds
								//this way we get 'rainy days' and not random ons and offs
								float precipitationValue = UnityEngine.Random.value;
								if (precipitationValue <= weather.Precipitation) {
										//looks like rain / snow!
										//clamp it to its min value
										precipitationValue = Mathf.Clamp(precipitationValue, 0.05f, PrecipitationLevel);
										//now multiply it by the cloud and weather type to get a final value
										setting.QuarterMorning.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterMorning.Weather, setting.QuarterMorning.CloudType);
										setting.QuarterAfternoon.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterAfternoon.Weather, setting.QuarterAfternoon.CloudType);
										setting.QuarterEvening.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterEvening.Weather, setting.QuarterEvening.CloudType);
										setting.QuarterNight.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterNight.Weather, setting.QuarterNight.CloudType);
								}

								Almanac[i] = setting;
						}
				}

				public static float WeightedPrecipitationValue(float precipitationValue, TOD_Weather.WeatherType weather, TOD_Weather.CloudType clouds)
				{
						switch (weather) {
								case TOD_Weather.WeatherType.Clear:
								default:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can't have rain if both are clear
														precipitationValue = 0f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.15f;
														break;

												case TOD_Weather.CloudType.Scattered:
														precipitationValue *= 0.25f;
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 0.5f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														break;
										}
										break;

								case TOD_Weather.WeatherType.Dust:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can't have rain if there are no clouds
														precipitationValue = 0f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.15f;
														break;

												case TOD_Weather.CloudType.Scattered:
														precipitationValue *= 0.25f;
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 0.5f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														break;
										}
										break;

								case TOD_Weather.WeatherType.Fog:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can have some rain if it's foggy
														precipitationValue *= 0.5f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.75f;
														break;

												case TOD_Weather.CloudType.Scattered:
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 1.125f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														precipitationValue *= 1.25f;
														break;
										}
										break;

								case TOD_Weather.WeatherType.Storm:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can have some rain if it's foggy
														precipitationValue *= 0.75f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.85f;
														break;

												case TOD_Weather.CloudType.Scattered:
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 1.5f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														precipitationValue *= 2.0f;
														break;
										}
										break;
						}
						return precipitationValue;
				}

				public static float RandomWeightedWindSpeed(float windChances, float maxWindSpeed, float minWindSpeed, TOD_Weather.WeatherType weather)
				{
						return 0f;
				}

				[XmlIgnore]
				public TemperatureRange StatusTempAverage = TemperatureRange.C_Warm;
		}

		[Serializable]
		public class ChunkBiomeData : Mod
		{
				public ChunkBiomeData() : base()
				{
				}

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public ClimateType Climate = ClimateType.Temperate;
				[FrontiersAvailableModsAttribute("CameraLut")]
				public string ColorSetting = "TemperateRegion";
				public float FreezingPointOffset = 0.15f;
				public float AltitutdeOffset = 250f;
				public float SnowHighpass = 0.9f;
				public float SnowLowpass = 0.5f;
				public float PrecipitationLevel = 0.5f;
				public float BaseTemperature = 0f;
				public float SpringTempOffset = 0f;
				public float SummerTempOffset = 0f;
				public float AutumnTempOffset = 0f;
				public float WinterTempOffset = 0f;
				public float ShorelineTempOffset = 0f;
				public float ForestTempOffset = 0f;
				public float CivilizationTempOffset = 0f;
				public float OpenFieldTempOffset = 0f;
				public float TideBaseElevation = 25f;
				public float TideMaxDifference = 5f;
				public float AmbientLightMultiplier = 1.0f;
				public float SunlightIntensityMultiplier = 1.0f;
				public float ExposureMultiplier = 1.0f;
				public BiomeStatusTemps StatusTempsSummer = new BiomeStatusTemps();
				public BiomeStatusTemps StatusTempsSpring = new BiomeStatusTemps();
				public BiomeStatusTemps StatusTempsAutumn = new BiomeStatusTemps();
				public BiomeStatusTemps StatusTempsWinter = new BiomeStatusTemps();
				public BiomeWeatherSetting WeatherSummer = new BiomeWeatherSetting();
				public BiomeWeatherSetting WeatherSpring = new BiomeWeatherSetting();
				public BiomeWeatherSetting WeatherAutumn = new BiomeWeatherSetting();
				public BiomeWeatherSetting WeatherWinter = new BiomeWeatherSetting();
				public WeatherSetting[] Almanac = null;

				public WeatherQuarter GetWeather(int dayOfYear, int hourOfDay)
				{
						if (Almanac == null) {
								GenerateAlmanac();
						}

						WeatherQuarter weather = null;
						if (dayOfYear < Almanac.Length) {
								if (hourOfDay < 6) {
										weather = Almanac[dayOfYear].QuarterMorning;
								} else if (hourOfDay < 12) {
										weather = Almanac[dayOfYear].QuarterAfternoon;
								} else if (hourOfDay < 18) {
										weather = Almanac[dayOfYear].QuarterEvening;
								} else {
										weather = Almanac[dayOfYear].QuarterNight;
								}
						}
						return weather;
				}

				public void GenerateAlmanac()//365 days
				{
						Almanac = new WeatherSetting [365];

						WeatherSummer.Normalize();
						WeatherSpring.Normalize();
						WeatherAutumn.Normalize();
						WeatherWinter.Normalize();

						WeatherSummer.GenerateLookup();
						WeatherSpring.GenerateLookup();
						WeatherAutumn.GenerateLookup();
						WeatherWinter.GenerateLookup();
						//this creates a simple lookup table of weather values for this region
						BiomeWeatherSetting weather = null;
						WeatherSetting setting = new WeatherSetting();
						for (int i = 0; i < 365; i++) {
								//spring starts on day 45, ends on day 139 (95)
								//summer starts on day 140, ends on day 229 (90)
								//autumn starts on day 230, ends on day 319 (90)
								//winter starts on day 320, ends on day 49 (90)
								if (i >= 320 || i < 45) {
										//winter
										weather = WeatherWinter;
								} else if (i >= 230) {
										//autumn
										weather = WeatherAutumn;
								} else if (i >= 140) {
										//summer
										weather = WeatherSummer;
								} else {
										//spring
										weather = WeatherSpring;
								}
								//get the weather type
								//this will drive the wind / precipitation values
								setting.QuarterMorning.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];
								setting.QuarterAfternoon.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];
								setting.QuarterEvening.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];
								setting.QuarterNight.CloudType = weather.CloudTypeLookup[UnityEngine.Random.Range(0, weather.CloudTypeLookup.Length)];

								setting.QuarterMorning.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];
								setting.QuarterAfternoon.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];
								setting.QuarterEvening.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];
								setting.QuarterNight.Weather = weather.WeatherTypeLookup[UnityEngine.Random.Range(0, weather.WeatherTypeLookup.Length)];

								//do precipitation by the day, then adjust it based on weather and clouds
								//this way we get 'rainy days' and not random ons and offs
								float precipitationValue = UnityEngine.Random.value;
								if (precipitationValue <= weather.Precipitation) {
										//looks like rain / snow!
										//clamp it to its min value
										precipitationValue = Mathf.Clamp(precipitationValue, 0.05f, PrecipitationLevel);
										//now multiply it by the cloud and weather type to get a final value
										setting.QuarterMorning.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterMorning.Weather, setting.QuarterMorning.CloudType);
										setting.QuarterAfternoon.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterAfternoon.Weather, setting.QuarterAfternoon.CloudType);
										setting.QuarterEvening.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterEvening.Weather, setting.QuarterEvening.CloudType);
										setting.QuarterNight.Precipitation = WeightedPrecipitationValue(precipitationValue, setting.QuarterNight.Weather, setting.QuarterNight.CloudType);
								}

								Almanac[i] = setting;
						}
				}

				public static float WeightedPrecipitationValue(float precipitationValue, TOD_Weather.WeatherType weather, TOD_Weather.CloudType clouds)
				{
						switch (weather) {
								case TOD_Weather.WeatherType.Clear:
								default:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can't have rain if both are clear
														precipitationValue = 0f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.15f;
														break;

												case TOD_Weather.CloudType.Scattered:
														precipitationValue *= 0.25f;
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 0.5f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														break;
										}
										break;

								case TOD_Weather.WeatherType.Dust:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can't have rain if there are no clouds
														precipitationValue = 0f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.15f;
														break;

												case TOD_Weather.CloudType.Scattered:
														precipitationValue *= 0.25f;
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 0.5f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														break;
										}
										break;

								case TOD_Weather.WeatherType.Fog:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can have some rain if it's foggy
														precipitationValue *= 0.5f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.75f;
														break;

												case TOD_Weather.CloudType.Scattered:
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 1.125f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														precipitationValue *= 1.25f;
														break;
										}
										break;

								case TOD_Weather.WeatherType.Storm:
										switch (clouds) {
												case TOD_Weather.CloudType.None:
					//we can have some rain if it's foggy
														precipitationValue *= 0.75f;
														break;

												case TOD_Weather.CloudType.Few:
														precipitationValue *= 0.85f;
														break;

												case TOD_Weather.CloudType.Scattered:
														break;

												case TOD_Weather.CloudType.Broken:
														precipitationValue *= 1.5f;
														break;

												case TOD_Weather.CloudType.Overcast:
												default:
														precipitationValue *= 2.0f;
														break;
										}
										break;
						}
						return precipitationValue;
				}

				public static float RandomWeightedWindSpeed(float windChances, float maxWindSpeed, float minWindSpeed, TOD_Weather.WeatherType weather)
				{
						return 0f;
				}
		}

		[Serializable]
		public class BiomeWeatherSetting
		{
				public bool UseDefault = true;

				public void Normalize()
				{
						float totalSky = SkyClear + SkyFog + SkyStorm + SkyDust;
						SkyClear = SkyClear / totalSky;
						SkyFog = SkyFog / totalSky;
						SkyStorm = SkyStorm / totalSky;
						SkyDust = SkyDust / totalSky;

						float totalClouds = CloudsClear + CloudsFew + CloudsScattered + CloudsBroken + CloudsOvercast;
						CloudsClear = CloudsClear / totalClouds;
						CloudsFew = CloudsFew / totalClouds;
						CloudsScattered = CloudsScattered / totalClouds;
						CloudsBroken = CloudsBroken / totalClouds;
						CloudsOvercast = CloudsOvercast / totalClouds;
				}

				public void GenerateLookup()
				{
						Normalize();
						//this is stupid but whatever, i'm tired
						List <TOD_Weather.WeatherType> weatherTypes = new List<TOD_Weather.WeatherType>();
						for (int i = 0; i < SkyClear * 100; i++) {
								weatherTypes.Add(TOD_Weather.WeatherType.Clear);
						}
						for (int i = 0; i < SkyStorm * 100; i++) {
								weatherTypes.Add(TOD_Weather.WeatherType.Storm);
						}
						for (int i = 0; i < SkyDust * 100; i++) {
								weatherTypes.Add(TOD_Weather.WeatherType.Dust);
						}
						for (int i = 0; i < SkyFog * 100; i++) {
								weatherTypes.Add(TOD_Weather.WeatherType.Fog);
						}
						mWeatherTypeLookup = weatherTypes.ToArray();

						List <TOD_Weather.CloudType> cloudTypes = new List <TOD_Weather.CloudType>();
						for (int i = 0; i < CloudsClear * 100; i++) {
								cloudTypes.Add(TOD_Weather.CloudType.None);
						}
						for (int i = 0; i < CloudsFew * 100; i++) {
								cloudTypes.Add(TOD_Weather.CloudType.Few);
						}
						for (int i = 0; i < CloudsScattered * 100; i++) {
								cloudTypes.Add(TOD_Weather.CloudType.Scattered);
						}
						for (int i = 0; i < CloudsBroken * 100; i++) {
								cloudTypes.Add(TOD_Weather.CloudType.Broken);
						}
						for (int i = 0; i < CloudsOvercast * 100; i++) {
								cloudTypes.Add(TOD_Weather.CloudType.Overcast);
						}
						mCloudTypeLookup = cloudTypes.ToArray();
				}

				public float Precipitation = 0.25f;
				public float Wind = 0.5f;
				public float SkyClear = 1f;
				public float SkyFog = 0f;
				public float SkyStorm = 0f;
				public float SkyDust = 0f;
				public float CloudsClear = 0.5f;
				public float CloudsFew = 0.25f;
				public float CloudsScattered = 0.125f;
				public float CloudsBroken = 0.125f;
				public float CloudsOvercast = 0.125f;
				public float LightningFrequency = 0.1f;

				[XmlIgnore]
				public TOD_Weather.WeatherType [] WeatherTypeLookup {
						get {
								return mWeatherTypeLookup;
						}
				}

				[XmlIgnore]
				public TOD_Weather.CloudType [] CloudTypeLookup {
						get {
								return mCloudTypeLookup;
						}
				}

				[NonSerialized]
				protected TOD_Weather.CloudType[] mCloudTypeLookup = null;
				[NonSerialized]
				protected TOD_Weather.WeatherType[] mWeatherTypeLookup = null;
		}

		[Serializable]
		public class WeatherSetting
		{
				public WeatherQuarter QuarterMorning = new WeatherQuarter();
				public WeatherQuarter QuarterAfternoon = new WeatherQuarter();
				public WeatherQuarter QuarterEvening = new WeatherQuarter();
				public WeatherQuarter QuarterNight = new WeatherQuarter();
		}

		[Serializable]
		public class WeatherQuarter
		{
				public float Wind;
				public float Mist;
				public float Precipitation;
				public float LightningFrequency = 0.1f;
				public TOD_Weather.WeatherType Weather;
				public TOD_Weather.CloudType CloudType;
		}

		[Serializable]
		public class AudioProfile : Mod
		{
				public AudioProfile() : base()
				{
				}

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public AmbientAudioManager.ChunkAudioSettings AmbientAudio = new AmbientAudioManager.ChunkAudioSettings();
		}

		[Serializable]
		public class ChunkState : Mod
		{
				public ChunkState() : base()
				{
				}

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public int ID = 0;
				public string WorldName = "FRONTIERS";
				//regions and neighboring chunks
				public int NeighboringChunkLeft = -1;
				public int NeighboringChunkTop = -1;
				public int NeighboringChunkRight = -1;
				public int NeighboringChunkBot = -1;
				public bool ArbitraryPosition = false;
				public int SizeX = 0;
				public int SizeZ = 0;
				public int XTilePosition = 0;
				public int ZTilePosition = 0;
				public float YOffset = 0.0f;
				public SVector3 TileOffset = SVector3.zero;
				public ChunkDisplaySettings DisplaySettings = new ChunkDisplaySettings();
		}

		[Serializable]
		public class ChunkSceneryData : Mod
		{
				public ChunkSceneryData() : base()
				{
				}

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public int TotalChunkPrefabs {
						get {
								return AboveGround.SolidTerrainPrefabs.Count
								+ AboveGround.SolidTerrainPrefabsAdjascent.Count
								+ AboveGround.SolidTerrainPrefabsDistant.Count
								+ BelowGround.SolidTerrainPrefabs.Count
								+ BelowGround.SolidTerrainPrefabsAdjascent.Count
								+ BelowGround.SolidTerrainPrefabsDistant.Count
								+ Transitions.SolidTerrainPrefabs.Count
								+ Transitions.SolidTerrainPrefabsAdjascent.Count
								+ Transitions.SolidTerrainPrefabsDistant.Count;
						}
				}
				//prefab information
				public ChunkSceneryPrefabs AboveGround = new ChunkSceneryPrefabs();
				public ChunkSceneryPrefabs BelowGround = new ChunkSceneryPrefabs();
				public ChunkSceneryPrefabs Transitions = new ChunkSceneryPrefabs();
		}

		[Serializable]
		public class ChunkPlantData : Mod
		{
				public ChunkPlantData() : base()
				{
				}

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public PlantInstanceTemplate[] PlantInstances = new PlantInstanceTemplate [0];
		}

		[Serializable]
		public class ChunkTreeData : Mod
		{
				public ChunkTreeData() : base()
				{
				}

				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public TreeInstanceTemplate[] TreeInstances = new TreeInstanceTemplate [0];
		}

		[Serializable]
		public class ChunkPathData : Mod
		{
				public ChunkPathData() : base()
				{
				}

				[XmlIgnore]//yes we actually ingore this and rebuild it on load, weird I know
		public List <PathMarkerInstanceTemplate> PathMarkerInstances = new List <PathMarkerInstanceTemplate>();
				[XmlIgnore]
				public SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>> PathMarkersByPathName = new SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>>();
				//this is where we keep path markers that are used in only one path
				public SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>> UniquePathMarkers = new SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>>();
				//this is where we keep references to path markers that are used in multiple paths
				public SDictionary <PathMarkerInstanceTemplate, SDictionary <string,int>> SharedPathMarkers = new SDictionary<PathMarkerInstanceTemplate, SDictionary<string, int>>();
		}

		[Serializable]
		public class ChunkSceneryPrefabs
		{
				public List <ChunkPrefab> SolidTerrainPrefabs = new List <ChunkPrefab>();
				public List <ChunkPrefab> SolidTerrainPrefabsAdjascent = new List <ChunkPrefab>();
				public List <ChunkPrefab> SolidTerrainPrefabsDistant = new List <ChunkPrefab>();
				public List <string> RiverNames = new List <string>();
				public FXPiece[] FXPieces = null;
		}

		[Serializable]
		public class ChunkPrefab : Mod
		{
				public ChunkPrefab()
				{

				}

				public ChunkPrefab(Transform transform, string prefabName)
				{
						Transform = new STransform(transform, true);
						Name = prefabName;
				}

				public int Layer = Globals.LayerNumSolidTerrain;
				public LocationTerrainType TerrainType = LocationTerrainType.AboveGround;
				public string Tag = "GroundStone";
				public string PackName;
				public string PrefabName;
				public bool EnableSnow = false;
				public bool UseMeshCollider = true;
				public bool UseConvexMesh = false;
				public STransform Transform = new STransform();
				public List <STransform> BoxColliders = new List <STransform>();
				public List <string> SharedMaterialNames = new List <string>();
				public SDictionary <string, string> SceneryScripts = new SDictionary <string, string>();
				[XmlIgnore]
				public WorldChunk ParentChunk;

				[XmlIgnore]
				public bool UseBoxColliders {
						get { return BoxColliders.Count > 0; }
				}

				public bool IsLoaded {
						get { return LoadedObject != null; }
				}

				[XmlIgnore]
				public ChunkPrefabObject LoadedObject;
		}

		[Serializable]
		public class ChunkDisplaySettings : Mod
		{
				public bool ArbitraryTilePosition = false;
				public bool ArbitraryTileSize = false;
		}

		[Serializable]
		public class ChunkTerrainData : Mod
		{
				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public int HeightmapResolution = 0;
				public int HeightmapHeight = 0;
				public List <GroundType> SplatmapGroundTypes = new List <GroundType>();
				public TerrainkMaterialSettings MaterialSettings = new TerrainkMaterialSettings();
				public SColor GrassTint = Color.white;
				public float WindSpeed = 0.497f;
				public float WindSize = 0.493f;
				public float WindBending = 0.495f;
				public List <TerrainPrototypeTemplate> DetailTemplates = new List <TerrainPrototypeTemplate>();
				public List <TerrainPrototypeTemplate> TreeTemplates = new List <TerrainPrototypeTemplate>();
				public List <TerrainTextureTemplate> TextureTemplates = new List <TerrainTextureTemplate>();
				public bool PassThroughChunkData = false;
		}
		//stores positions for where to spawn plants
		[Serializable]
		public class PlantInstanceTemplate : TreeInstanceTemplate
		{
				[XmlIgnore]
				public bool HasBeenPlanted {
						get {
								return !string.IsNullOrEmpty(PlantName);
						}
				}

				[XmlIgnore]
				public bool ReadyToBePlanted {
						get {
								if (PickedTime > 0) {
										return (WorldClock.AdjustedRealTime - PickedTime > Globals.PlantAutoRegrowInterval);
								}
								return true;
						}
				}

				[XmlIgnore]
				public override bool IsEmpty {
						get {
								return this == gEmptyPlantTemplate;
						}
				}

				public double PlantedTime = -1f;
				public double PickedTime = -1f;
				public string PlantName = string.Empty;
				public bool AboveGround = true;
				public int Climate = -1;

				public PlantInstanceTemplate(TreeInstance treeInstance, float terrainHeight, Vector3 chunkOffset, Vector3 chunkScale) : base(treeInstance, terrainHeight, chunkOffset, chunkScale)
				{
				}

				public PlantInstanceTemplate(bool empty) : base(empty)
				{
				}

				public PlantInstanceTemplate() : base()
				{

				}

				public static PlantInstanceTemplate Empty {
						get {
								return gEmptyPlantTemplate;
						}
				}

				protected static PlantInstanceTemplate gEmptyPlantTemplate = new PlantInstanceTemplate(true);
		}

		[Serializable]
		public class TreeInstanceTemplate : IHasPosition
		{
				public static TreeInstanceTemplate Empty {
						get {
								if (gEmptyTreeTemplate == null) {
										gEmptyTreeTemplate = new TreeInstanceTemplate(true);
								}
								return gEmptyTreeTemplate;
						}
				}

				[XmlIgnore]
				public WorldChunk ParentChunk;

				[XmlIgnore]
				public virtual bool IsEmpty {
						get {
								return this == Empty;
						}
				}

				[XmlIgnore]
				public Vector3 ChunkScale {
						get {
								mChunkScale.Set(CSX, CSY, CSZ);
								return mChunkScale;
						}
						set {
								CSX = value.x;
								CSY = value.y;
								CSZ = value.z;
						}
				}

				[XmlIgnore]
				protected Vector3 mChunkScale;

				[XmlIgnore]
				public Vector3 ChunkOffset {
						get {
								mChunkOffset.Set(CX, CY, CZ);
								return mChunkOffset;
						}
						set {
								CX = value.x;
								CY = value.y;
								CZ = value.z;
						}
				}

				[XmlIgnore]
				protected Vector3 mChunkOffset;

				[XmlIgnore]
				public Vector3 LocalPosition {
						get {
								mLocalPosition.Set(X, Y, Z);
								return mLocalPosition;
						}
				}

				[XmlIgnore]
				protected Vector3 mLocalPosition;

				[XmlIgnore]
				public Vector3 Position {
						get { return Vector3.Scale(ChunkScale, LocalPosition) + ChunkOffset; }
				}

				public TreeInstanceTemplate(bool empty)
				{
						CSX = 0f;
						CSY = 0f;
						CSZ = 0f;
						CX = 0f;
						CY = 0f;
						CZ = 0f;
						OriginalTerrain = true;
						PrototypeIndex = -1;
						R = 0f;
						G = 0f;
						B = 0f;
						A = 0f;
						HeightScale = 0f;
						WidthScale = 0f;
						X = 0f;
						Y = 0f;
						Z = 0f;

						RequiresInstance = true;
						HasInstance = false;
						LockInstance = false;
				}

				public TreeInstanceTemplate(TreeInstance treeInstance, float terrainHeight, Vector3 chunkOffset, Vector3 chunkScale)
				{
						CSX = chunkScale.x;
						CSY = chunkScale.y;
						CSZ = chunkScale.z;
						CX = chunkOffset.x;
						CY = chunkOffset.y;
						CZ = chunkOffset.z;
						OriginalTerrain = true;
						PrototypeIndex = treeInstance.prototypeIndex;
						R = treeInstance.color.r;
						G = treeInstance.color.g;
						B = treeInstance.color.b;
						A = treeInstance.color.a;
						HeightScale = treeInstance.heightScale;
						WidthScale = treeInstance.widthScale;
						X = treeInstance.position.x;
						Y = treeInstance.position.y * (terrainHeight / chunkScale.y);//difference between bounds / height
						Z = treeInstance.position.z;

						RequiresInstance = true;
						HasInstance = false;
						LockInstance = false;
				}

				public TreeInstanceTemplate()
				{
						CSX = 0f;
						CSY = 0f;
						CSZ = 0f;
						CX = 0f;
						CY = 0f;
						CZ = 0f;
						OriginalTerrain = true;
						PrototypeIndex = -1;
						R = 0f;
						G = 0f;
						B = 0f;
						A = 0f;
						HeightScale = 0f;
						WidthScale = 0f;
						X = 0f;
						Y = 0f;
						Z = 0f;

						RequiresInstance = true;
						HasInstance = false;
						LockInstance = false;
				}

				public float CX;
				public float CY;
				public float CZ;
				public float CSX;
				public float CSY;
				public float CSZ;
				public bool RequiresInstance;
				//ignore trees where this is set to false
				public bool HasInstance;
				public bool LockInstance;
				public bool OriginalTerrain;
				public int PrototypeIndex;
				public float R;
				public float G;
				public float B;
				public float A;
				public float HeightScale;
				public float WidthScale;
				public float X;
				public float Y;
				public float Z;

				public TreeInstance	ToInstance {
						get {
								TreeInstance treeInstance = new TreeInstance();
								treeInstance.lightmapColor = Color.white;
								gInstanceColor.r = R;
								gInstanceColor.g = G;
								gInstanceColor.b = B;
								gInstanceColor.a = A;
								treeInstance.color = gInstanceColor;
								gInstancePosition.x = X;
								gInstancePosition.y = Y;
								gInstancePosition.z = Z;
								treeInstance.position = gInstancePosition;
								treeInstance.heightScale = HeightScale;
								treeInstance.widthScale = WidthScale;
								treeInstance.prototypeIndex	= PrototypeIndex;
								return treeInstance;
						}
				}

				protected static Color gInstanceColor;
				protected static Vector3 gInstancePosition;
				protected static TreeInstanceTemplate gEmptyTreeTemplate;
		}

		public interface IHasPosition
		{
				Vector3 Position {
						get;
				}
		}
		namespace World
		{
				[Serializable]
				public class Path : Mod
				{
						public override bool IgnoreProfileDataIfOutdated {
								get {
										return true;
								}
						}

						public SBounds PathBounds = new SBounds();
						public List <PathMarkerInstanceTemplate> Templates = new List<PathMarkerInstanceTemplate>();

						public void SetActive(bool active)
						{
								for (int i = 0; i < Templates.Count; i++) {
										Templates[i].IsActive = active;
								}
						}

						public void InitializeTemplates()
						{
								if (mTemplatesInitialized)
										return;

								for (int i = 0; i < Templates.Count; i++) {
										if (i == 0 || i == Templates.LastIndex()) {
												Templates[i].IsTerminal = true;
												Templates[i].Type |= PathMarkerType.PathOrigin;
										}
										Templates[i].IsActive = false;
										Templates[i].Branches.Clear();
										Templates[i].ParentPath = this;
										Templates[i].PathName = this.Name;
										Templates[i].IndexInParentPath = i;
										Templates[i].ID = PathMarkerInstanceTemplate.gID++;
								}

								mTemplatesInitialized = true;
						}

						public void RefreshBranches()
						{		//this assumes parent paths have already been set
								//this is where we let other path markers know what paths are attached to it
								//if we don't own the path marker, it's a branch
								//so we add our name and the index where THIS path is using it
								for (int i = 0; i < Templates.Count; i++) {
										//paths can only have 1 branch to a path
										//so if it's already been set don't worry about nuking the previous setting
										//it's obviously wrong
										if (Templates[i].Branches.ContainsKey(this.Name)) {
												Templates[i].Branches[this.Name] = i;
										} else {
												if (Templates[i].ParentPath != this) {
														Templates[i].Type = Templates[i].Type | PathMarkerType.Cross;
												}
												Templates[i].Branches.Add(this.Name, i);
												if (Templates[i].Branches.Count > 1) {
														Templates[i].Type = Templates[i].Type | PathMarkerType.Cross;
												}
										}
								}
						}

						protected bool mTemplatesInitialized = false;
				}
		}
}