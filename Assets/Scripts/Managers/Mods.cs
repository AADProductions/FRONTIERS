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
	[ExecuteInEditMode]
	public class Mods : Manager
	{
		public static Mods Get;
		public ModsEditor Editor = new ModsEditor ();
		public ModsRuntime Runtime = new ModsRuntime ();

		public override void WakeUp ()
		{
			Get = this;
			Editor.mods = this;
			Runtime.mods = this;
			mParentUnderManager = false;
		}

		#region runtime / editor safe stuff

		public string WorldDescription (string worldName) {
			string description = "No description available.";
			string error = string.Empty;
			WorldSettings world = null;
			if (GameData.IO.LoadWorld (ref world, worldName, out error)) {
				description = world.Description;
			}
			return description;
		}

		public string Description <T> (string dataType, string modName) where T : Mod, new()
		{
			string description = "No description available.";
			T mod = null;
			if (Runtime.LoadMod <T> (ref mod, dataType, modName)) {
				description = mod.FullDescription;
			} else {
				Debug.Log ("Couldn't load!");
			}
			return description;
		}

		public override void OnModsLoadStart ()
		{
			BuildCurrentWorld ();
			mModsLoaded = true;
			//prevent hiccups
			Shader.WarmupAllShaders ();
		}

		public void BuildCurrentWorld ( )
		{

		}

		public List <string> Types ()
		{
			List <string> types = new List <string> ();
			HashSet <string> folderNames = new HashSet <string> ();
			GameData.IO.GetFolderNamesInProfileDirectory (folderNames, "");
			types.AddRange (folderNames);
			return types;
		}

		public void SaveLiveGame (string toSaveGame) {
			if (!GameData.IO.CopyGameData (GameData.IO.gLiveGameFolderName, toSaveGame)) {
				Debug.LogWarning ("Couldn't save live game to " + toSaveGame);
			}
		}

		public void LoadLiveGame (string fromSaveGame) {
			if (!GameData.IO.CopyGameData (fromSaveGame, GameData.IO.gLiveGameFolderName)) {
				Debug.LogError ("Couldn't load " + fromSaveGame + " to live game");
			}
		}

		public List <string> Available (string dataType)
		{
			List <string> available = new List <string> ();
			HashSet <string> fileNames = new HashSet <string> ();
			GameData.IO.GetFileNamesInProfileDirectory (fileNames, dataType);
			GameData.IO.GetFileNamesInWorldDirectory (fileNames, dataType);
			GameData.IO.GetFileNamesInBaseDirectory (fileNames, dataType);
			available.AddRange (fileNames);
			return available;
		}

		public List <string> Available (ModType type)
		{
			List <string> available = new List <string> ();
			HashSet <string> fileNames = new HashSet <string> ();
			GameData.IO.GetFileNamesInProfileDirectory (fileNames, type.ToString ());
			GameData.IO.GetFileNamesInWorldDirectory (fileNames, type.ToString ());
			GameData.IO.GetFileNamesInBaseDirectory (fileNames, type.ToString ());
			available.AddRange (fileNames);
			return available;
		}

		public List <string> Available (string dataType, string searchString)
		{
			List <string> available = new List <string> ();
			HashSet <string> fileNames = new HashSet <string> ();
			GameData.IO.GetFileNamesInProfileDirectory (fileNames, dataType);
			GameData.IO.GetFileNamesInWorldDirectory (fileNames, dataType);
			GameData.IO.GetFileNamesInBaseDirectory (fileNames, dataType);
			foreach (string availableItem in fileNames) {
				if (availableItem.Contains (searchString)) {
					available.Add (availableItem);
				}
			}
			return available;
		}

		public List <string> Available (string type, DataType from)
		{
			List <string> available = new List <string> ();
			HashSet <string> fileNames = new HashSet <string> ();
			switch (type) {
			case "World"://world is a special case - move it to AvailableWorlds
				available = GameData.IO.GetWorldNames ();
				break;

			default:
				GameData.IO.GetFileNamesInDirectory (fileNames, type, false, from);
				available.AddRange (fileNames);
				break;
			}
			return available;
		}

		public List <string> GlobalDataNames (string dataType, string dataPath)
		{
			HashSet <string> fileNames = new HashSet <string> ();
			GameData.IO.GetFileNamesInWorldDirectory (fileNames, System.IO.Path.Combine (dataType, dataPath));
			return new List <string> (fileNames);
		}

		public List <string> ModDataNames (string dataType)
		{
			HashSet <string> fileNames = new HashSet <string> ();
			GameData.IO.GetFileNamesInWorldDirectory (fileNames, dataType);
			return new List <string> (fileNames);
		}

		#endregion

		public bool LoadPrefab (ref GameObject prefab, string prefabType, string prefabName)
		{
			string path = System.IO.Path.Combine (prefabType, prefabName);
			prefab = Resources.Load (path) as GameObject;

			if (prefab != null) {
				return true;
			}
			return false;
		}

		public class ModsRuntime
		{
			public Mods mods;

			public string FullPath (string fileName, string dataType, string extension)
			{
				//TODO make this work with more than just the base
				System.IO.DirectoryInfo directory = System.IO.Directory.GetParent (Application.dataPath);
				string dataPath = directory.FullName;
				dataPath = System.IO.Path.Combine (dataPath, GameData.IO.GetDataPath (DataType.Base));
				dataPath = System.IO.Path.Combine (dataPath, System.IO.Path.Combine (dataType, (fileName + extension)));
				dataPath = System.IO.Path.Combine (Application.absoluteURL, dataPath);
				return dataPath;
			}

			public void ResetProfileData (string dataType)
			{
				if (dataType == "all") {
					foreach (string modType in Mods.Get.Types ( ))
					{
						GameData.IO.DeleteData (modType, DataType.Profile, DataCompression.None);
					}
				} else {
					dataType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase (dataType.ToLower ());
					//deletes local (profile) data
					GameData.IO.DeleteData (dataType, DataType.Profile, DataCompression.None);
				}
			}

			public void ResetProfileData (string dataType, string dataName)
			{
				dataType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase (dataType.ToLower ());
				//deletes local (profile) data
				GameData.IO.DeleteData (dataType, dataName, DataType.Profile, DataCompression.None);
			}

			public void DeleteMod (string dataType, string dataName)
			{
				GameData.IO.DeleteData (dataType, dataName, DataType.Profile, DataCompression.None);
			}

			public bool PackMap (string packName, string mapName, out Texture2D packMap)
			{
				packMap = Resources.Load (packName + "/" + mapName) as Texture2D;
				//TODO move this to external loader
				return packMap != null;
			}

			public bool Texture (Texture2D texture, string dataType, string dataName) {
				//loads the pixels from the mod data into the existing texture
				//check to see if a local copy exists
				if (GameData.IO.LoadProfileTexture (texture, dataType, dataName)
					|| GameData.IO.LoadWorldTexture (texture, dataType, dataName)
					|| GameData.IO.LoadBaseTexture (texture, dataType, dataName)) {
					//always set this just in case
					texture.name = dataName;
					return true;
				}
				return false;
			}

			public bool BodyTexture (ref Texture2D bodyTexture, string textureName) {
				int resolution = Globals.SmallCharacterBodyTextureResolution;
				if (textureName.StartsWith ("Med")) {
					resolution = Globals.MediumCharacterBodyTextureResolution;
				} else if (textureName.StartsWith ("Lrg")) {
					resolution = Globals.LargeCharacterBodyTextureResolution;
				}
				return GameData.IO.LoadCharacterTexture (ref bodyTexture, textureName, "Body", resolution);
			}

			public bool MaskTexture (ref Texture2D maskTexture, string textureName) {
				return GameData.IO.LoadCharacterTexture (ref maskTexture, textureName, "Mask", Globals.SmallCharacterBodyTextureResolution);
			}

			public bool FaceTexture (ref Texture2D faceTexture, string textureName) {
				return GameData.IO.LoadCharacterTexture (ref faceTexture, textureName, "Face", Globals.CharacterFaceTextureResolution);
			}

			public bool ChunkMap (ref Texture2D map, string chunkName, string mapName)
			{
				map = null;
				int resolution = 0;
				TextureFormat format = TextureFormat.ARGB32;
				bool linear = false;
				GameData.GetTextureFormat (mapName, ref resolution, ref format, ref linear);
				return GameData.IO.LoadTerrainMap (ref map, resolution, format, linear, chunkName, mapName);
			}

			public bool ChunkPlantPrototype (ref GameObject prototype, string prototypeName)
			{
				prototype = Resources.Load ("TerrainPlantPrototypes/" + prototypeName) as GameObject;
				return prototype != null;
			}

			public bool ChunkGrassTexture (ref Texture2D diffuse, string diffuseName)
			{
				diffuse = Resources.Load ("TerrainGrassTextures/" + diffuseName) as Texture2D;
				return diffuse != null;
			}

			public bool ChunkGroundTexture (ref Texture2D diffuse, ref Texture2D normal, string diffuseName, string normalName)
			{
				diffuse = Resources.Load ("TerrainGroundTextures/" + diffuseName) as Texture2D;
				normal = null;
				//normal = Resources.Load ("TerrainGroundTextures/" + normalName) as Texture2D;
				return diffuse != null;
			}

			public bool ChunkCombinedNormal (ref Texture2D combinedNormal, string combinedNormalName)
			{
				combinedNormal = null;
				if (string.IsNullOrEmpty (combinedNormalName)) {
					return false;
				}
				combinedNormal = Resources.Load ("TerrainCombinedNormals/" + combinedNormalName) as Texture2D;
				return true;
			}

			public bool LoadTerrainDetailLayer (int[,] detailLayer, string chunkName, int detailIndex)
			{
				string detailFileName = DetailAssetFileName (chunkName, detailIndex);
				string chunkPathName = System.IO.Path.Combine ("Chunk", chunkName);
				bool result = GameData.IO.LoadDetailLayer (detailLayer, chunkPathName, detailFileName, DataType.Profile);
				if (!result) {
					result = GameData.IO.LoadDetailLayer (detailLayer, chunkPathName, detailFileName, DataType.World);
					if (!result) {
						result = GameData.IO.LoadDetailLayer (detailLayer, chunkPathName, detailFileName, DataType.Base);
					}
				}
				return result;
			}

			public bool TerrainHeights (float[,] heights, int resolution, string chunkName, int terrainIndex)
			{
				string terrainFileName = TerrainAssetFileName (chunkName, terrainIndex);
				return GameData.IO.LoadTerrainHeights (heights, resolution, chunkName, terrainFileName);
			}

			//receives a stack item and a path from the server
			//find the object and updates its state or creates the object if it doesn't exist
			public void ReceiveStackItem (StackItem stackItem, MobileReference reference)
			{
				//first check to see if the item exists
				WorldItem childItem = null;
				if (WIGroups.FindChildItem (reference.GroupPath, reference.FileName, out childItem)) {
					childItem.ReceiveState (stackItem);
				} else {
					//if the item doesn't exist, see if the group exists
					WIGroup group = null;
					if (WIGroups.FindGroup (reference.GroupPath, out group)) {
						//if we found the group, add the stack item to the group
						//if the group is loaded it'll instantiate the worlditem automatically
						group.AddChildItem (stackItem);
					} else {
						//if we didn't find the child item or the group, that means this object isn't loaded
						//save it to disk and it will load the next time it's needed
						Mods.Get.Runtime.SaveStackItemToGroup (stackItem, reference.GroupPath);
					}
				}
			}
			//sends a stack item to the server
			//now that we're handling stuff with properties I'm actually having a tough time imagining when to use this
			//but I'm sure I'll think of something
			public void SendStackItem (StackItem stackItem, MobileReference reference)
			{
				//dum dee doo, send it to everyone
			}

			public List <string> GroupChildItemNames (string groupPath, bool includePathInName)
			{
				HashSet <string> childItemNames = new HashSet <string> ();
				string fullPath = System.IO.Path.Combine ("Group", groupPath);
				
				//why are we getting names for all of these?
				//Profile 	- there may be items that don't appear in the world and base directories because they've spawned or the player has put them there.
				//			- there may NOT be items that appear in the world and base directories becuase the world item hasn't been modified / saved
				//
				//World		- there may be items which don't appear in base or which override base items
				//
				//Base		- all items which should be spawned - this includes items which have been destroyed or moved
				//			- destroyed or moved items will be handled with 'Destroyed' mode and with reference files
				//
				//we put the child item names in a hashset so that we don't try to load the same item multiple times
				//this way of getting child items names is kind of inefficient but for now it'll do
				GameData.IO.GetFileNamesInProfileDirectory (childItemNames, fullPath);
				GameData.IO.GetFileNamesInWorldDirectory (childItemNames, fullPath);
				GameData.IO.GetFileNamesInBaseDirectory (childItemNames, fullPath);
				List <string> finalList = new List <string> ();
				if (includePathInName) {
					foreach (string childItemName in childItemNames) {
						if (!childItemName.StartsWith ("_")) {
							finalList.Add (string.Join (WIGroup.gPathJoinString, new string [] {
								groupPath,
								childItemName
							}));
						}
					}
				} else {
					finalList.AddRange (childItemNames);
				}
				return finalList;
			}

			public List <string> GroupChildGroupNames (string groupPath, bool includePathInName)
			{	//TODO filter by mobile reference
				HashSet <string> childGroupNames = new HashSet <string> ();
				string path = System.IO.Path.Combine ("Group", groupPath);
				GameData.IO.GetFolderNamesInProfileDirectory (childGroupNames, path);
				GameData.IO.GetFolderNamesInWorldDirectory (childGroupNames, path);
				GameData.IO.GetFolderNamesInBaseDirectory (childGroupNames, path);
				List <string> finalList = new List <string> ();
				if (includePathInName) {
					foreach (string childGroupName in childGroupNames) {
						finalList.Add (string.Join (WIGroup.gPathJoinString, new string [] {
							groupPath,
							childGroupName
						}));
					}
				} else {
					finalList.AddRange (childGroupNames);
				}
				return finalList;
			}

			public void SaveGroupProps (WIGroupProps groupProps)
			{	
				groupProps.ListInAvailable = false;
				SaveMod <WIGroupProps> (groupProps, "Group", groupProps.UniqueID, groupProps.UniqueID);
			}

			public bool LoadGroupProps (ref WIGroupProps groupProps, string groupUniqueID)
			{
				return LoadMod <WIGroupProps> (ref groupProps, "Group", groupUniqueID, groupUniqueID);
			}

			public bool SaveStackItemToGroup (StackItem stackItem, string groupPath)
			{
				bool result = false;
				string fileName = stackItem.FileName;
				string fullPath = System.IO.Path.Combine ("Group", groupPath);
				
				//get a stack item and save it to the group folder in profile
				stackItem.Version = GameManager.Version;
				GameData.IO.SaveProfileData <StackItem> (stackItem, fullPath, fileName, DataCompression.GZip);
				return result;
			}

			public bool LoadStackItemFromGroup (ref StackItem stackItem, string groupPath, string fileName, bool followReferences)
			{
				bool result = false;
				string fullPath = System.IO.Path.Combine ("Group", groupPath);
				//first see if there's a reference file
				result = GameData.IO.LoadProfileData <StackItem> (ref stackItem, fullPath, fileName, DataCompression.GZip);
				if (!result) {	//next check the world data
					result = GameData.IO.LoadWorldData <StackItem> (ref stackItem, fullPath, fileName, DataCompression.GZip);
					if (!result) {	//if that has nothing, check the base data - we've got nowhere else to go at that point
						result = GameData.IO.LoadBaseData <StackItem> (ref stackItem, fullPath, fileName, DataCompression.GZip);
					}
				}
				return result;
			}

			public bool LoadGame (ref PlayerGame game, string worldName, string gameName)
			{
				return GameData.IO.LoadGame (ref game, worldName, gameName);
			}

			public void SaveGame (PlayerGame game)
			{
				game.Version = GameManager.Version;
				GameData.IO.SaveGame (game);
			}

			public void SaveMod <T> (T data, string dataType, string dataPath, string dataName) where T : Mod, new()
			{
				SaveMod<T> (data, System.IO.Path.Combine (dataType, dataPath), dataName);
			}

			public void SaveMod <T> (T data, string dataType, string dataName) where T : Mod, new()
			{
				data.Name = dataName;//just in case
				data.Type = typeof (T).ToString ();
				data.Version = GameManager.Version;
				GameData.IO.SaveProfileData <T> (data, dataType, dataName, DataCompression.None);
			}

			public void SaveMods <T> (List <T> data, string dataType) where T : Mod, new()
			{
				for (int i = 0; i < data.Count; i++) {
					SaveMod <T> (data [i], data [i].Name, dataType);
				}
			}

			public bool LoadMod <T> (ref T data, string dataType, string dataPath, string dataName) where T : Mod, new()
			{
				return LoadMod <T> (ref data, System.IO.Path.Combine (dataType, dataPath), dataName);
			}

			public bool LoadMod <T> (ref T data, string dataType, string dataName) where T : Mod, new()
			{
				//check to see if a local copy exists
				if (GameData.IO.LoadProfileData <T> (ref data, dataType, dataName, DataCompression.None)
					|| GameData.IO.LoadWorldData <T> (ref data, dataType, dataName, DataCompression.None)
					|| GameData.IO.LoadBaseData <T> (ref data, dataType, dataName, DataCompression.None)) {
					//always set this just in case
					data.Name = dataName;
					return true;
				}
				return false;
			}

			public void LoadAvailableMods <T> (List<T> mods, string dataType) where T : Mod, new()
			{
				List <string> availableMods = Get.Available (dataType);
				T mod = null;
				for (int i = 0; i < availableMods.Count; i++) {
					if (LoadMod <T> (ref mod, dataType, availableMods [i]) && mod.ListInAvailable) {
						mods.Add (mod);
					}
				}
			}
		}

		public class ModsEditor
		{	//this is the class used to save / load stuff in the editor while building the world
			//it's really similar to mods runtime but it assumes you're saving to the base data set
			public Mods mods;

			public void InitializeEditor ()
			{
				InitializeEditor (false);
			}

			public void InitializeEditor (bool overrideAppPlaying)
			{
				if (Application.isPlaying && !overrideAppPlaying) {
					return;
				}

				if (Mods.Get == null) {
					GameObject modsGameObject = GameObject.Find ("__MODS");
					Mods.Get = modsGameObject.GetComponent <Mods> ();
				}

				string errorMessage = string.Empty;
				GameData.IO.InitializeSystemPaths (out errorMessage);
				Data.GameData.IO.SetDefaultLocalDataPaths ();
			}

			public List <string> Available (string dataType)
			{
				List <string> available = new List <string> ();
				HashSet <string> fileNames = new HashSet <string> ();
				GameData.IO.GetFileNamesInWorldDirectory (fileNames, dataType);
				available.AddRange (fileNames);
				return available;
			}

			public List <string> GroupChildItemNames (string groupPath)
			{
				HashSet <string> childItemNames = new HashSet <string> ();
				string fullPath = System.IO.Path.Combine ("Group", groupPath);
				GameData.IO.GetFileNamesInBaseDirectory (childItemNames, fullPath);
				
				return new List <string> (childItemNames);
			}

			public List <string> GroupChildGroupNames (string groupPath)
			{
				HashSet <string> folderNames = new HashSet <string> ();
				string fullPath = System.IO.Path.Combine ("Group", groupPath);
				GameData.IO.GetFolderNamesInBaseDirectory (folderNames, System.IO.Path.Combine ("Group", groupPath));
				
				return new List <string> (folderNames);
			}

			public void SaveMod <T> (T data, string dataType, string dataPath, string dataName) where T : Mod, new()
			{
				SaveMod <T> (data, System.IO.Path.Combine (dataType, dataPath), dataName);
			}

			public void SaveMod <T> (T data, string dataType, string dataName) where T : Mod, new()
			{
				if (string.IsNullOrEmpty (dataName)) {
					return;
				}
				data.Name = dataName;
				data.Type = typeof (T).ToString ();
				data.Version = GameManager.Version;
				GameData.IO.SaveBaseData <T> (data, dataType, dataName, DataCompression.None);
				gAvailable.Clear ();
			}

			public void DeleteMod (string dataType, string dataPath)
			{
				GameData.IO.DeleteData (System.IO.Path.Combine (dataType, dataPath), DataType.World, DataCompression.None);
			}

			public void DeleteGroup (WIGroupProps groupProps)
			{
				GameData.IO.DeleteData (System.IO.Path.Combine ("Group", groupProps.UniqueID), DataType.Base, DataCompression.None);
			}

			public void SaveGroupProps (WIGroupProps groupProps)
			{	
				groupProps.ListInAvailable = false;
				SaveMod <WIGroupProps> (groupProps, "Group", groupProps.UniqueID, groupProps.UniqueID);
			}

			public bool LoadMod <T> (ref T data, string dataType, string dataName) where T : Mod, new()
			{
				if (GameData.IO.LoadBaseData <T> (ref data, dataType, dataName, DataCompression.None)) {
					data.Name = dataName;
					return true;
				}
				return false;
			}

			public void SaveMods <T> (List <T> data, string dataType) where T : Mod, new()
			{
				for (int i = 0; i < data.Count; i++) {
					SaveMod <T> (data [i], dataType, data [i].Name);
				}
			}

			public void LoadAvailableMods <T> (List<T> mods, string dataType) where T : Mod, new()
			{
				List <string> availableMods = Get.Available (dataType);
				T mod = null;
				for (int i = 0; i < availableMods.Count; i++) {
					if (LoadMod <T> (ref mod, dataType, availableMods [i])) {
						mods.Add (mod);
					}
				}
			}

			public bool SaveMesh (Mesh meshToSave, Material[] matsToSave, string dataType, string dataName)
			{
				return GameData.IO.SaveObjFromMesh (meshToSave, matsToSave, dataType, dataName, DataType.Base);
			}

			public bool LoadMesh (ref GameObject meshGameObject, string dataType, string dataName)
			{
				GameObject[] loadedMeshes = null;
				if (GameData.IO.LoadObjToGameObjects (ref loadedMeshes, dataType, dataName, DataType.Base)) {
					if (loadedMeshes.Length > 0) {
						meshGameObject = loadedMeshes [0];
						return true;
					}
				}
				return false;
			}

			public void SaveTerrainDetailLayer (int[,] detailLayer, string chunkName, int detailIndex)
			{
				string detailFileName = DetailAssetFileName (chunkName, detailIndex);
				string chunkPathName = System.IO.Path.Combine ("Chunk", chunkName);
				GameData.IO.SaveDetailLayer (detailLayer, chunkPathName, detailFileName, DataType.World);
			}

			public void SaveWorldSettings (WorldSettings settings)
			{

			}

			public bool ChunkMap (ref Texture2D map, string chunkName, string mapName)
			{
				map = null;
				int resolution = 0;
				TextureFormat format = TextureFormat.ARGB32;
				bool linear = false;
				if (!Manager.IsAwake <GameWorld> ()) {
					Manager.WakeUp <GameWorld> ("__WORLD");
				}
				GameData.GetTextureFormat (mapName, ref resolution, ref format, ref linear);
				return GameData.IO.LoadTerrainMap (ref map, resolution, format, linear, chunkName, mapName);
			}

			public bool LoadStackItemFromGroup (ref StackItem stackItem, string groupPath, string fileName)
			{
				bool result = false;
				string fullPath = System.IO.Path.Combine ("Group", groupPath);
				return GameData.IO.LoadBaseData <StackItem> (ref stackItem, fullPath, fileName, DataCompression.GZip);
			}

			#if UNITY_EDITOR
			//editor utilities
			public static string GUILayoutMissionVariable (string missionName, string variableName, bool includeNone, string noneOption, int maxWidth) {

				noneOption = "(" + noneOption + ")";
				Frontiers.World.Gameplay.MissionState mission = null;
				if (Mods.Get.Editor.LoadMod <Frontiers.World.Gameplay.MissionState> (ref mission, "Mission", missionName)) {
					List <string> variableNames = new List <string> ();
					foreach (KeyValuePair <string,int> missionVar in mission.Variables) {
						variableNames.Add (missionVar.Key);
					}
					if (includeNone) {
						variableNames.Insert (0, noneOption);
					}
					int currentItemIndex = variableNames.IndexOf (variableName);
					currentItemIndex = UnityEditor.EditorGUILayout.Popup (currentItemIndex, variableNames.ToArray (), GUILayout.MaxWidth (maxWidth));
					if (currentItemIndex < 0 || currentItemIndex >= variableNames.Count) {
						currentItemIndex = 0;
					}
					string newItem = variableNames [currentItemIndex];
					if (newItem == noneOption) {
						return string.Empty;
					}
					return newItem;
				}
				return string.Empty;
			}

			public static string GUILayoutMissionObjective (string missionName, string objectiveName, bool includeNone, string noneOption, int maxWidth) {

				noneOption = "(" + noneOption + ")";
				Frontiers.World.Gameplay.MissionState mission = null;
				if (Mods.Get.Editor.LoadMod <Frontiers.World.Gameplay.MissionState> (ref mission, "Mission", missionName)) {
					List <string> objectiveNames = mission.GetObjectiveNames ();
					if (includeNone) {
						objectiveNames.Insert (0, noneOption);
					}
					int currentItemIndex = objectiveNames.IndexOf (objectiveName);
					currentItemIndex = UnityEditor.EditorGUILayout.Popup (currentItemIndex, objectiveNames.ToArray (), GUILayout.MaxWidth (maxWidth));
					if (currentItemIndex < 0 || currentItemIndex >= objectiveNames.Count) {
						currentItemIndex = 0;
					}
					string newItem = objectiveNames [currentItemIndex];
					if (newItem == noneOption) {
						return string.Empty;
					}
					return newItem;
				}
				return string.Empty;
			}

			public static string GUILayoutAvailable (string currentItem, string modType, int maxWidth) {
				return GUILayoutAvailable (currentItem, modType, false, string.Empty, maxWidth);
			}

			public static string GUILayoutAvailable (string currentItem, string modType, bool includeNone, int maxWidth) {
				return GUILayoutAvailable (currentItem, modType, includeNone, "None", maxWidth);
			}

			public static string GUILayoutAvailable (string currentItem, string modType, bool includeNone, string noneOption, int maxWidth)
			{
				noneOption = "(" + noneOption + ")";
				List <string> available = null;
				if (!gAvailable.TryGetValue (modType, out available)) {
					if (!Manager.IsAwake <Mods> ()) {
						Manager.WakeUp <Mods> ("__MODS");
						Mods.Get.Editor.InitializeEditor (true);
					}
					available = Get.Available (modType);
					gAvailable.Add (modType, available);
				}
				if (includeNone) {
					if (!available.Contains (noneOption)) {
						available.Insert (0, noneOption);
					}
				} else {
					available.Remove (noneOption);
				}
				int currentItemIndex = available.IndexOf (currentItem);
				currentItemIndex = UnityEditor.EditorGUILayout.Popup (currentItemIndex, available.ToArray (), GUILayout.MaxWidth (maxWidth));
				if (currentItemIndex < 0 || currentItemIndex >= available.Count) {
					currentItemIndex = 0;
				}
				string newItem = available [currentItemIndex];
				if (newItem == noneOption) {
					return string.Empty;
				}
				//take it out again just in case
				available.Remove (noneOption);
				return newItem;
			}
			#endif

			protected static Dictionary <string, List <string>> gAvailable = new Dictionary<string, List<string>> ( );
		}

		#region static helper classes

		public static string WorldRegionName (int xTilePosition, int zTilePosition)
		{
			return xTilePosition.ToString ("D4") + "_" + zTilePosition.ToString ("D4");
		}

		public static string TerrainAssetFileName (string chunkName, int terrainIndex)
		{
			return "Raw16Mac";
		}

		public static string DetailAssetFileName (string chunkName, int detailIndex)
		{
			return "Int2DAsByteArray-" + detailIndex.ToString ();
		}

		#endregion

	}

	public enum ModType
	{
		World,
		DifficultySetting,
		Group,
		Blueprint,
		Category,
		Character,
		Chunk,
		Conversation,
		Structure,
		Mission,
		ChunkNodeData,
		ChunkTriggerData,
		FlagSet,
		Speech,
		StructurePack,
	}

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
		public void GetSettings (Material atsMaterial)
		{
			Vector4 terrainCombinedFloats = atsMaterial.GetVector ("_terrainCombinedFloats");
			MultiUV = terrainCombinedFloats.x;
			Desaturation = terrainCombinedFloats.y;
			SplattingDistance = terrainCombinedFloats.z;
			TerrainSpecPower = terrainCombinedFloats.w;

			TerrainSpecColor = atsMaterial.GetColor ("_SpecColor");

			Splat0Tiling = atsMaterial.GetFloat ("_Splat0Tiling");
			Splat1Tiling = atsMaterial.GetFloat ("_Splat1Tiling");
			Splat2Tiling = atsMaterial.GetFloat ("_Splat2Tiling");
			Splat3Tiling = atsMaterial.GetFloat ("_Splat3Tiling");
			Splat4Tiling = atsMaterial.GetFloat ("_Splat4Tiling");
			Splat5Tiling = atsMaterial.GetFloat ("_Splat5Tiling");

			Texture1Average = atsMaterial.GetColor ("_ColTex1");
			Texture2Average = atsMaterial.GetColor ("_ColTex2");
			Texture3Average = atsMaterial.GetColor ("_ColTex3");
			Texture4Average = atsMaterial.GetColor ("_ColTex4");
			Texture5Average = atsMaterial.GetColor ("_ColTex5");
			Texture6Average = atsMaterial.GetColor ("_ColTex6");

			Texture1Shininess = atsMaterial.GetFloat ("_Spec1");
			Texture2Shininess = atsMaterial.GetFloat ("_Spec2");
			Texture3Shininess = atsMaterial.GetFloat ("_Spec3");
			Texture4Shininess = atsMaterial.GetFloat ("_Spec4");
			Texture5Shininess = atsMaterial.GetFloat ("_Spec5");
			Texture6Shininess = atsMaterial.GetFloat ("_Spec6");

			Texture cn12 = atsMaterial.GetTexture ("_CombinedNormal12");
			if (cn12 != null) {
				CombinedNormals12 = cn12.name;
			} else {
				CombinedNormals12 = string.Empty;
			}
			Texture cn34 = atsMaterial.GetTexture ("_CombinedNormal34");
			if (cn34 != null) {
				CombinedNormals34 = cn34.name;
			} else {
				CombinedNormals34 = string.Empty;
			}
			Texture cn56 = atsMaterial.GetTexture ("_CombinedNormal56");
			if (cn56 != null) {
				CombinedNormals56 = cn56.name;
			} else {
				CombinedNormals56 = string.Empty;
			}

			Vector4 fresnelSettings = atsMaterial.GetVector ("_Fresnel");
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

		public void ApplySettings (Material atsMaterial)
		{
//			Vector4 terrainCombinedFloats = new Vector4 (MultiUV, Desaturation, SplattingDistance, TerrainSpecPower);
//			atsMaterial.SetVector ("_terrainCombinedFloats", terrainCombinedFloats);

//			atsMaterial.SetColor ("_SpecColor", TerrainSpecColor);

			atsMaterial.SetFloat ("_Splat0Tiling", Splat0Tiling);
			atsMaterial.SetFloat ("_Splat1Tiling", Splat1Tiling);
			atsMaterial.SetFloat ("_Splat2Tiling", Splat2Tiling);
			atsMaterial.SetFloat ("_Splat3Tiling", Splat3Tiling);
			atsMaterial.SetFloat ("_Splat4Tiling", Splat4Tiling);
			atsMaterial.SetFloat ("_Splat5Tiling", Splat5Tiling);

			atsMaterial.SetColor ("_ColTex1", Texture1Average);
			atsMaterial.SetColor ("_ColTex2", Texture2Average);
			atsMaterial.SetColor ("_ColTex3", Texture3Average);
			atsMaterial.SetColor ("_ColTex4", Texture4Average);
			atsMaterial.SetColor ("_ColTex5", Texture5Average);
			atsMaterial.SetColor ("_ColTex6", Texture6Average);

//			atsMaterial.SetFloat ("_Spec1", Texture1Shininess);
//			atsMaterial.SetFloat ("_Spec2", Texture2Shininess);
//			atsMaterial.SetFloat ("_Spec3", Texture3Shininess);
//			atsMaterial.SetFloat ("_Spec4", Texture4Shininess);
//			atsMaterial.SetFloat ("_Spec5", Texture5Shininess);
//			atsMaterial.SetFloat ("_Spec6", Texture6Shininess);

//			Vector4 fresnelSettings	= new Vector4 (FresnelIntensity, FresnelPower, FresnelBias, 0f);
//			atsMaterial.SetVector ("_Fresnel", fresnelSettings);
		}

		public void ApplyMaps (Material atsMaterial, string chunkName, Dictionary <string, Texture2D> maps)
		{
			ApplyMap ("ColorOverlay", "_CustomColorMap", maps, atsMaterial);
			ApplyMap ("NormalOverlay", "_TerrainNormalMap", maps, atsMaterial);
			ApplyMap ("Splat1", "_Control", maps, atsMaterial);
			ApplyMap ("Splat2", "_Control2nd", maps, atsMaterial);

//			Texture2D splat = null;
//			if (maps.TryGetValue ("Splat1", out splat)) {
//				atsMaterial.SetTexture ("_Control", splat);
//			}
//			if (maps.TryGetValue ("Splat2", out splat)) {
//				atsMaterial.SetTexture ("_Control2nd", splat);
//			}

//			ApplyMap ("Splat1", "_Control", maps, atsMaterial);
//			ApplyMap ("Splat2", "_Control2nd", maps, atsMaterial);

			ApplyMap ("Ground0", "_Splat0", maps, atsMaterial);
			ApplyMap ("Ground1", "_Splat1", maps, atsMaterial);
			ApplyMap ("Ground2", "_Splat2", maps, atsMaterial);
			ApplyMap ("Ground3", "_Splat3", maps, atsMaterial);
			ApplyMap ("Ground4", "_Splat4", maps, atsMaterial);
			ApplyMap ("Ground5", "_Splat5", maps, atsMaterial);

			Texture2D cn12 = null;
			if (Plants.Get.GetTerrainGroundTexture (CombinedNormals12, out cn12)) {
				atsMaterial.SetTexture ("_CombinedNormal12", cn12);
			}
			Texture2D cn34 = null;
			if (Plants.Get.GetTerrainGroundTexture (CombinedNormals34, out cn34)) {
				atsMaterial.SetTexture ("_CombinedNormal34", cn34);
			}
			Texture2D cn56 = null;
			if (Plants.Get.GetTerrainGroundTexture (CombinedNormals56, out cn56)) {
				atsMaterial.SetTexture ("_CombinedNormal56", cn56);
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

		protected void	ApplyMap (string mapName, string propertyName, Dictionary <string, Texture2D> maps, Material atsMaterial)
		{
			Texture2D map = null;
			if (maps.TryGetValue (mapName, out map)) {
				//Debug.Log ("Setting map name " + mapName);
				atsMaterial.SetTexture (propertyName, map);
			} else {
				//Debug.Log ("Couldn't get map name " + mapName);
			}
		}
		//used to store ATS material settings
		public string CombinedNormals12 = "CombinedNormalsA";
		public string CombinedNormals34 = "CombinedNormalsB";
		public string CombinedNormals56 = "CombinedNormalsC";
		public SColor	TerrainSpecColor = new SColor (0.25f, 0.25f, 0.25f, 0f);
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

		public bool GetNext (ref SequenceStep step)
		{
			if (mStepQueue == null) {
				BuildStepStack ();
			}

			if (mStepQueue.Count > 0) {
				step = mStepQueue.Dequeue ();
				return true;
			}
			return false;
		}

		protected void BuildStepStack ()
		{
			mStepQueue = new Queue <SequenceStep> ();

			string[] commandLines = Commands.Split (gNewlineSeparators, StringSplitOptions.RemoveEmptyEntries);
			foreach (string commandLine in commandLines) {
				string[] splitCommand	= commandLine.Split (gSpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
				//Command Target Value Duration
				SequenceStep step = new SequenceStep ();
				step.Duration = Single.Parse (splitCommand [0]);
				step.Command = splitCommand [1];
				step.Target = splitCommand [2];
				if (splitCommand.Length > 3)
					step.Value	= splitCommand [3];
				else
					step.Value	= string.Empty;

				if (splitCommand.Length > 4)
					step.Assignment = splitCommand [4];
				else
					step.Assignment = string.Empty;

				mStepQueue.Enqueue (step);
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
		public List <string> DestroyedQuestItems = new List <string> ();
	}

	[Serializable]
	public class PlayerStartupPosition : Mod
	{
		public PlayerIDFlag PlayerID = PlayerIDFlag.Local;
		//where to put the player
		public int ChunkID;
		public STransform ChunkPosition;
		LocationTerrainType LocationType = LocationTerrainType.AboveGround;
		[XmlIgnore]
		public STransform WorldPosition = new STransform ();
		public bool RequiresStructure {
			get {
				return !string.IsNullOrEmpty (StructureName);
			}
		}
		public bool Interior = false;
		public MobileReference LocationReference;
		public string StructureName;
		public bool ClearInventory = false;
		public bool DestroyClearedItems = true;
		public bool ClearLog = false;
		public string CharacterName = string.Empty;
		//what time it should be
		public bool AbsoluteTime = false;//whether to add the time or set it outright
		public float TimeHours = 0f;
		public float TimeDays = 0f;
		public float TimeMonths = 0f;
		public float TimeYears = 0f;
		//player stater
		public string ControllerState;
		public string InventoryFillCategory;
		public List <StatusKeeperValue> StatusValues = new List<StatusKeeperValue> ( );
		public bool ClearRevealedLocations = false;
		public List <CurrencyValue> CurrencyToAdd = new List<CurrencyValue> ( );
		public List <MobileReference> NewLocationsToReveal = new List<MobileReference> ( );
		public List <MobileReference> NewVisitedRespawnStructures = new List<MobileReference> ( );
		[Serializable]
		public class StatusKeeperValue {
			public string StatusKeeperName = "Health";
			public float Value = 1.0f;
		}
	}

	[Serializable]
	public class CurrencyValue {
		public WICurrencyType Type = WICurrencyType.A_Bronze;
		public int Number = 0;
	}

	[Serializable]
	public class WorldSettings : Mod
	{
		public WorldSettings ()
		{
			Type = "World";
			Name = "FRONTIERS";
			Description = "A new world.";
		}

		public SColor DefaultTerrainType = Color.white;
		[FrontiersAvailableModsAttribute ("Biome")]
		public string DefaultBiome;
		public AmbientAudioManager.ChunkAudioSettings DefaultAmbientAudio = new AmbientAudioManager.ChunkAudioSettings ();
		public AmbientAudioManager.ChunkAudioItem DefaultAmbientAudioInterior = new AmbientAudioManager.ChunkAudioItem ();
		public bool RequiresCompletedWorlds {
			get {
				return RequiredCompletedWorlds.Count > 0;
			}
		}
		public List <string> RequiredCompletedWorlds = new List <string> ( );

		[FrontiersAvailableModsAttribute ("Music")]
		public string DefaultMusicCombat;
		[FrontiersAvailableModsAttribute ("Music")]
		public string DefaultMusicCutscene;
		[FrontiersAvailableModsAttribute ("Music")]
		public string DefaultMusicMainMenu;
		[FrontiersAvailableModsAttribute ("Music")]
		public string DefaultMusicNight;
		[FrontiersAvailableModsAttribute ("Music")]
		public string DefaultMusicRegional;
		[FrontiersAvailableModsAttribute ("Music")]
		public string DefaultMusicSafeLocation;
		[FrontiersAvailableModsAttribute ("Music")]
		public string DefaultMusicUnderground;

		public float TimeHours = 0f;
		public float TimeDays = 0f;
		public float TimeMonths = 0f;
		public float TimeYears = 0f;
		public List <MobileReference> DefaultRevealedLocations = new List<MobileReference> ();
		public CharacterFlags DefaultResidentFlags = new CharacterFlags ( );
		public WIFlags DefaultContainerFlags = new WIFlags ( );
		public ChunkBiomeData DefaultBiomeData = new ChunkBiomeData ();
		public List <string> BaseDifficultySettingNames	= new List <string> ();
		public MobileReference DefaultHouseOfHealing = new MobileReference ( );
		public int NumChunkTilesX;
		public int NumChunkTilesZ;
		[NonSerialized]
		public List <DifficultySetting> BaseDifficultySettings = new List <DifficultySetting> ();
		//these are used the first time you enter the game
		//this includes positions for all multiplayer players
		[FrontiersAvailableModsAttribute ("PlayerStartupPosition")]
		public string FirstStartupPosition = "PrologueSpawn";
	}

	[Serializable]
	public class Mod : IComparable <Mod>
	{
		public Mod ( ) { }

		public virtual string FullDescription { 
			get {
				if (string.IsNullOrEmpty (mFullDescription)) {
					mFullDescription = Description;
				}
				return Description;
			}
		}

		public string Name = string.Empty;
		public string Description = string.Empty;
		public string Type = string.Empty;
		public string Dependencies = string.Empty;
		public string Version = string.Empty;
		public int DisplayOrder = 0;
		public bool Enabled = true;
		public bool ListInAvailable = true;

		public virtual int CompareTo (Mod other)
		{
			return DisplayOrder.CompareTo (other.DisplayOrder);
		}

		[NonSerialized]
		protected string mFullDescription = string.Empty;
	}

	[Serializable]
	public class DifficultySetting : Mod
	{
//		public override string FullDescription {
//			get {
//				if (string.IsNullOrEmpty (mFullDescription)) {
//					GenerateFullDescription ();
//				}
//				return mFullDescription;
//			}
//		}

//		public DifficultySetting ()
//		{
//			DifficultyFlags = new List <string> ();
//			GlobalVariables = new List <DifficultySettingGlobal> ();
//		}

		public bool IsDefined (string creaturesNeverHostile)
		{
			return DifficultyFlags.Contains (creaturesNeverHostile);
		}
		
		public DifficultyDeathStyle DeathStyle = DifficultyDeathStyle.Respawn;
		public List <DifficultySettingGlobal> GlobalVariables;// = new List <DifficultySettingGlobal> ();
		public List <string> DifficultyFlags;// = new List <string> ();

		public static void Apply (DifficultySetting difficulty)
		{
			//set the globals to default
			//then apply the difficulty setting on top of the default values
			List <KeyValuePair <string,string>> globalPairs = null;
			string errorMessage = null;
			if (GameData.IO.LoadGlobals (ref globalPairs, out errorMessage)) {
				Globals.LoadDifficultySettingData (globalPairs);
				for (int i = 0; i < difficulty.GlobalVariables.Count; i++) {
					Globals.SetDifficultyVariable (difficulty.GlobalVariables [i].GlobalVariableName, difficulty.GlobalVariables [i].VariableValue);
				}
			}
		}

		protected void GenerateFullDescription ( ) {
			if (GlobalVariables != null && GlobalVariables.Count > 0) {
				List <string> descriptionLines = new List <string> ();
				descriptionLines.Add (Description);
				descriptionLines.Add ("_");
				descriptionLines.Add ("Changes defined in this setting:");
				for (int i = 0; i < GlobalVariables.Count; i++) {
					descriptionLines.Add (GlobalVariables [i].Description);
				}
				mFullDescription = descriptionLines.JoinToString ("\n");
			} else {
				mFullDescription = Description;
			}
		}
	}

	[Serializable]
	public enum DifficultyDeathStyle {
		BlackOut,//black out for a bit, then wake up
		Respawn,//respawn in the nearest respawn structure
		PermaDeath,//hardcore mode
	}

	[Serializable]
	public class DifficultySettingGlobal {
		public string GlobalVariableName;
		public string VariableValue;
		public string Description;
	}

	[Serializable]
	public class ChunkTriggerData : Mod
	{
		public ChunkTriggerData ()
		{
			Type = "ChunkTriggerData";
			Name = "Trigger Group";
			Description = "A group of triggers";
		}

		public SDictionary <string, KeyValuePair <string,string>> TriggerStates = new SDictionary <string, KeyValuePair <string,string>> ();
	}

	[Serializable]
	public class ChunkNodeData : Mod
	{
		public ChunkNodeData ()
		{
			Type = "ChunkNodeData";
			Name = "Node Group";
			Description = "A group of nodes";
		}

		public SDictionary <string, List <ActionNodeState>>	NodeStates = new SDictionary <string, List <ActionNodeState>> ();
		public List <TerrainNode> TerrainNodes = new List <TerrainNode> ();
	}

	[Serializable]
	public struct TerrainNode
	{
		public TerrainNode (Vector3 chunkPosition)
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
		public Vector3				Position {
			get {
				return new Vector3 (X, Y, Z);
			}
			set {
				X = value.x;
				Y = value.y;
				Z = value.z;
			}
		}

		[XmlIgnore]
		public LocationTerrainType	LocationType {
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

	public enum PrototypeTemplateType
	{
		DetailTexture,
		DetailMesh,
		TreeMesh,
	}

	[Serializable]
	public class ChunkRegionData : Mod
	{
		public CharacterFlags ResidentFlags = new CharacterFlags ();
		public WIFlags StructureFlags = new WIFlags ();
	}

	[Serializable]
	public class Region : Mod {
		public int RegionID = 0;
		[FrontiersBitMaskAttribute ("Region")]
		public int RegionFlag;
		public CharacterFlags ResidentFlags = new CharacterFlags ();
		public WIFlags StructureFlags = new WIFlags ();
		[FrontiersAvailableModsAttribute ("Music")]
		public string DayMusic;
		[FrontiersAvailableModsAttribute ("Music")]
		public string NightMusic;
		[FrontiersAvailableModsAttribute ("Music")]
		public string UndergroundMusic;
		[SColorAttribute]
		public SColor BannerColor;
		[SColorAttribute]
		public SColor SymbolColor;
		public string Symbol;
		public MobileReference Capital = new MobileReference ( );
		public WISize RegionSize = WISize.Small;
		public List <string> MaleFirstNames = new List <string> ( );
		public List <string> FemaleFirstNames = new List <string> ( );
		public List <string> FamilyNames = new List <string> ( );
		public float LocalNameUsage = 0.5f;
		public MobileReference DefaultRespawnStructure = new MobileReference ( );
	}

	[Serializable]
	public class Biome : Mod {
		public int BiomeID = 0;
		[FrontiersBitMaskAttribute ("Climate")]
		public int BiomeFlag = 0;

		[FrontiersAvailableModsAttribute ("AudioProfile")]
		public string SummerAudioProfile;
		[FrontiersAvailableModsAttribute ("AudioProfile")]
		public string AutumnAudioProfile;
		[FrontiersAvailableModsAttribute ("AudioProfile")]
		public string WinterAudioProfile;
		[FrontiersAvailableModsAttribute ("AudioProfile")]
		public string SpringAudioProfile;

		public ClimateType Climate = ClimateType.Temperate;
		[FrontiersAvailableModsAttribute ("CameraLut")]
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

		public BiomeStatusTemps StatusTempsSummer = new BiomeStatusTemps ();
		public BiomeStatusTemps StatusTempsSpring = new BiomeStatusTemps ();
		public BiomeStatusTemps StatusTempsAutumn = new BiomeStatusTemps ();
		public BiomeStatusTemps StatusTempsWinter = new BiomeStatusTemps ();
		public BiomeWeatherSetting WeatherSummer = new BiomeWeatherSetting ();
		public BiomeWeatherSetting WeatherSpring = new BiomeWeatherSetting ();
		public BiomeWeatherSetting WeatherAutumn = new BiomeWeatherSetting ();
		public BiomeWeatherSetting WeatherWinter = new BiomeWeatherSetting ();

		[XmlIgnore]
		[NonSerialized]
		public WeatherSetting [] Almanac = null;

		public WeatherQuarter GetWeather (int dayOfYear, int hourOfDay)
		{
			if (Almanac == null) {
				GenerateAlmanac ();
			}

			WeatherQuarter weather = null;
			if (dayOfYear < Almanac.Length) {
				if (hourOfDay < 6) {
					weather = Almanac [dayOfYear].QuarterMorning;
				} else if (hourOfDay < 12) {
					weather = Almanac [dayOfYear].QuarterAfternoon;
				} else if (hourOfDay < 18) {
					weather = Almanac [dayOfYear].QuarterEvening;
				} else {
					weather = Almanac [dayOfYear].QuarterNight;
				}
			}
			return weather;
		}

		public void GenerateAlmanac ()//365 days
		{
			Almanac = new WeatherSetting [365];

			WeatherSummer.Normalize ();
			WeatherSpring.Normalize ();
			WeatherAutumn.Normalize ();
			WeatherWinter.Normalize ();

			WeatherSummer.GenerateLookup ();
			WeatherSpring.GenerateLookup ();
			WeatherAutumn.GenerateLookup ();
			WeatherWinter.GenerateLookup ();

			//this creates a simple lookup table of weather values for this region
			BiomeWeatherSetting weather = null;
			WeatherSetting setting = new WeatherSetting ( );
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
				setting.QuarterMorning.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];
				setting.QuarterAfternoon.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];
				setting.QuarterEvening.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];
				setting.QuarterNight.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];

				setting.QuarterMorning.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];
				setting.QuarterAfternoon.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];
				setting.QuarterEvening.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];
				setting.QuarterNight.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];

				//do precipitation by the day, then adjust it based on weather and clouds
				//this way we get 'rainy days' and not random ons and offs
				float precipitationValue = UnityEngine.Random.value;
				if (precipitationValue <= weather.Precipitation) {
					//looks like rain / snow!
					//clamp it to its min value
					precipitationValue = Mathf.Clamp (precipitationValue, 0.05f, PrecipitationLevel);
					//now multiply it by the cloud and weather type to get a final value
					setting.QuarterMorning.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterMorning.Weather, setting.QuarterMorning.CloudType);
					setting.QuarterAfternoon.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterAfternoon.Weather, setting.QuarterAfternoon.CloudType);
					setting.QuarterEvening.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterEvening.Weather, setting.QuarterEvening.CloudType);
					setting.QuarterNight.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterNight.Weather, setting.QuarterNight.CloudType);
				}

				Almanac [i] = setting;
			}
		}

		public static float WeightedPrecipitationValue (float precipitationValue, TOD_Weather.WeatherType weather, TOD_Weather.CloudType clouds)
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

		public static float RandomWeightedWindSpeed (float windChances, float maxWindSpeed, float minWindSpeed, TOD_Weather.WeatherType weather)
		{
			return 0f;
		}
	}

	[Serializable]
	public class ChunkBiomeData : Mod
	{
		public ChunkBiomeData () : base ()
		{
		}

		public ClimateType Climate = ClimateType.Temperate;
		[FrontiersAvailableModsAttribute ("CameraLut")]
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
		public BiomeStatusTemps StatusTempsSummer = new BiomeStatusTemps ();
		public BiomeStatusTemps StatusTempsSpring = new BiomeStatusTemps ();
		public BiomeStatusTemps StatusTempsAutumn = new BiomeStatusTemps ();
		public BiomeStatusTemps StatusTempsWinter = new BiomeStatusTemps ();
		public BiomeWeatherSetting WeatherSummer = new BiomeWeatherSetting ();
		public BiomeWeatherSetting WeatherSpring = new BiomeWeatherSetting ();
		public BiomeWeatherSetting WeatherAutumn = new BiomeWeatherSetting ();
		public BiomeWeatherSetting WeatherWinter = new BiomeWeatherSetting ();
		public WeatherSetting[] Almanac = null;

		public WeatherQuarter GetWeather (int dayOfYear, int hourOfDay)
		{
			if (Almanac == null) {
				GenerateAlmanac ();
			}

			WeatherQuarter weather = null;
			if (dayOfYear < Almanac.Length) {
				if (hourOfDay < 6) {
					weather = Almanac [dayOfYear].QuarterMorning;
				} else if (hourOfDay < 12) {
					weather = Almanac [dayOfYear].QuarterAfternoon;
				} else if (hourOfDay < 18) {
					weather = Almanac [dayOfYear].QuarterEvening;
				} else {
					weather = Almanac [dayOfYear].QuarterNight;
				}
			}
			return weather;
		}

		public void GenerateAlmanac ()//365 days
		{
			Almanac = new WeatherSetting [365];

			WeatherSummer.Normalize ();
			WeatherSpring.Normalize ();
			WeatherAutumn.Normalize ();
			WeatherWinter.Normalize ();

			WeatherSummer.GenerateLookup ();
			WeatherSpring.GenerateLookup ();
			WeatherAutumn.GenerateLookup ();
			WeatherWinter.GenerateLookup ();
			//this creates a simple lookup table of weather values for this region
			BiomeWeatherSetting weather = null;
			WeatherSetting setting = new WeatherSetting ( );
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
				setting.QuarterMorning.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];
				setting.QuarterAfternoon.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];
				setting.QuarterEvening.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];
				setting.QuarterNight.CloudType = weather.CloudTypeLookup [UnityEngine.Random.Range (0, weather.CloudTypeLookup.Length)];

				setting.QuarterMorning.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];
				setting.QuarterAfternoon.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];
				setting.QuarterEvening.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];
				setting.QuarterNight.Weather = weather.WeatherTypeLookup [UnityEngine.Random.Range (0, weather.WeatherTypeLookup.Length)];

				//do precipitation by the day, then adjust it based on weather and clouds
				//this way we get 'rainy days' and not random ons and offs
				float precipitationValue = UnityEngine.Random.value;
				if (precipitationValue <= weather.Precipitation) {
					//looks like rain / snow!
					//clamp it to its min value
					precipitationValue = Mathf.Clamp (precipitationValue, 0.05f, PrecipitationLevel);
					//now multiply it by the cloud and weather type to get a final value
					setting.QuarterMorning.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterMorning.Weather, setting.QuarterMorning.CloudType);
					setting.QuarterAfternoon.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterAfternoon.Weather, setting.QuarterAfternoon.CloudType);
					setting.QuarterEvening.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterEvening.Weather, setting.QuarterEvening.CloudType);
					setting.QuarterNight.Precipitation = WeightedPrecipitationValue (precipitationValue, setting.QuarterNight.Weather, setting.QuarterNight.CloudType);
				}

				Almanac [i] = setting;
			}
		}

		public static float WeightedPrecipitationValue (float precipitationValue, TOD_Weather.WeatherType weather, TOD_Weather.CloudType clouds)
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

		public static float RandomWeightedWindSpeed (float windChances, float maxWindSpeed, float minWindSpeed, TOD_Weather.WeatherType weather)
		{
			return 0f;
		}
	}

	[Serializable]
	public class BiomeWeatherSetting
	{
		public bool UseDefault = true;
		public void Normalize ()
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

		public void GenerateLookup ( ) 
		{
			Normalize ();
			//this is stupid but whatever, i'm tired
			List <TOD_Weather.WeatherType> weatherTypes = new List<TOD_Weather.WeatherType> ();
			for (int i = 0; i < SkyClear * 100; i++) {
				weatherTypes.Add (TOD_Weather.WeatherType.Clear);
			}
			for (int i = 0; i < SkyStorm * 100; i++) {
				weatherTypes.Add (TOD_Weather.WeatherType.Storm);
			}
			for (int i = 0; i < SkyDust * 100; i++) {
				weatherTypes.Add (TOD_Weather.WeatherType.Dust);
			}
			for (int i = 0; i < SkyFog * 100; i++) {
				weatherTypes.Add (TOD_Weather.WeatherType.Fog);
			}
			mWeatherTypeLookup = weatherTypes.ToArray ();

			List <TOD_Weather.CloudType> cloudTypes = new List <TOD_Weather.CloudType> ( );
			for (int i = 0; i < CloudsClear * 100; i++) {
				cloudTypes.Add (TOD_Weather.CloudType.None);
			}
			for (int i = 0; i < CloudsFew * 100; i++) {
				cloudTypes.Add (TOD_Weather.CloudType.Few);
			}
			for (int i = 0; i < CloudsScattered * 100; i++) {
				cloudTypes.Add (TOD_Weather.CloudType.Scattered);
			}
			for (int i = 0; i < CloudsBroken * 100; i++) {
				cloudTypes.Add (TOD_Weather.CloudType.Broken);
			}
			for (int i = 0; i < CloudsOvercast * 100; i++) {
				cloudTypes.Add (TOD_Weather.CloudType.Overcast);
			}
			mCloudTypeLookup = cloudTypes.ToArray ();
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
		protected TOD_Weather.CloudType [] mCloudTypeLookup = null;
		[NonSerialized]
		protected TOD_Weather.WeatherType [] mWeatherTypeLookup = null;
	}

	[Serializable]
	public class WeatherSetting
	{
		public WeatherQuarter QuarterMorning = new WeatherQuarter ( );
		public WeatherQuarter QuarterAfternoon = new WeatherQuarter ( );
		public WeatherQuarter QuarterEvening = new WeatherQuarter ( );
		public WeatherQuarter QuarterNight = new WeatherQuarter ( );
	}

	[Serializable]
	public class WeatherQuarter
	{
		public float Wind;
		public float Precipitation;
		public TOD_Weather.WeatherType Weather;
		public TOD_Weather.CloudType CloudType;
	}

	[Serializable]
	public class AudioProfile : Mod
	{
		public AudioProfile () : base ()
		{
		}

		public AmbientAudioManager.ChunkAudioSettings AmbientAudio = new AmbientAudioManager.ChunkAudioSettings ();
	}

	[Serializable]
	public class ChunkState : Mod
	{
		public ChunkState () : base ()
		{
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
		public ChunkDisplaySettings DisplaySettings = new ChunkDisplaySettings ();
	}

	[Serializable]
	public class ChunkSceneryData : Mod
	{
		public ChunkSceneryData () : base ()
		{
		}
		//prefab information
		public ChunkSceneryPrefabs AboveGround = new ChunkSceneryPrefabs ();
		public ChunkSceneryPrefabs BelowGround = new ChunkSceneryPrefabs ();
		public ChunkSceneryPrefabs Transitions = new ChunkSceneryPrefabs ();
	}

	[Serializable]
	public class ChunkPlantData : Mod
	{
		public ChunkPlantData () : base ()
		{
		}

		public PlantInstanceTemplate[] PlantInstances = new PlantInstanceTemplate [0];
	}

	[Serializable]
	public class ChunkTreeData : Mod
	{
		public ChunkTreeData () : base ()
		{
		}

		public TreeInstanceTemplate[] TreeInstances = new TreeInstanceTemplate [0];
	}

	[Serializable]
	public class ChunkPathData : Mod
	{
		public ChunkPathData () : base ()
		{
		}

		[XmlIgnore]//yes we actually ingore this and rebuild it on load, weird I know
		public List <PathMarkerInstanceTemplate> PathMarkerInstances = new List <PathMarkerInstanceTemplate> ();
		[XmlIgnore]
		public SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>> PathMarkersByPathName = new SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>> ();
		//this is where we keep path markers that are used in only one path
		public SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>> UniquePathMarkers = new SDictionary <string, SDictionary <int,PathMarkerInstanceTemplate>> ();
		//this is where we keep references to path markers that are used in multiple paths
		public SDictionary <PathMarkerInstanceTemplate, SDictionary <string,int>> SharedPathMarkers = new SDictionary<PathMarkerInstanceTemplate, SDictionary<string, int>> ();
	}

	[Serializable]
	public class PathMarkerInstanceTemplate : IHasPosition
	{
		[XmlIgnore]
		public int Marker = -1;

		[XmlIgnore]
		public bool IsActive = false;

		[XmlIgnore]
		[NonSerialized]
		public Frontiers.World.Locations.PathMarker Owner;

		[XmlIgnore]
		public bool HasInstance {
			get {
				return Owner != null && !Owner.IsFinished;
			}
			set {
				if (!value) {
					Owner = null;
				}
			}
		}

		[XmlIgnore]
		public bool HasParentPath {
			get {
				return ParentPath != null;
			}
		}

		public int IndexInParentPath = 0;
		public bool IsTerminal = false;

		[XmlIgnore]
		[NonSerialized]
		[HideInInspector]
		public Frontiers.World.Path ParentPath;

		public static PathMarkerInstanceTemplate Empty {
			get {
				if (gEmptyInstance == null) {
					gEmptyInstance = new PathMarkerInstanceTemplate ();
				}
				return gEmptyInstance;
			}
		}

		public bool RequiresInstance {
			get {
				return Type != PathMarkerType.Location;
			}
		}

		public PathMarkerType Type = PathMarkerType.PathMarker;
		public MobileReference Location = MobileReference.Empty;
		public float XPos = 0f;
		public float YPos = 0f;
		public float ZPos = 0f;
		public float XRot = 0f;
		public float YRot = 0f;
		public float ZRot = 0f;

		public VisitableState Visitable = new VisitableState ();
		public RevealableState Revealable = new RevealableState ();

		[XmlIgnore]
		public Vector3 Position { 
			get {
				mPosition.Set (XPos, YPos, ZPos);
				return mPosition;
			}
			set{
				XPos = value.x;
				YPos = value.y;
				ZPos = value.z;
				mPosition.Set (XPos, YPos, ZPos);
			}
		}
		[XmlIgnore]
		public Vector3 Rotation {
			get {
				mRotation.Set (XRot, YRot, ZRot);
				return mRotation;
			}
			set {
				XRot = value.x;
				YRot = value.y;
				ZRot = value.z;
				mRotation.Set (XRot, YRot, ZRot);
			}
		}
		public string PathName = string.Empty;
		public SDictionary <string,int> Branches = new SDictionary <string,int> ();//outgoing branches
		[XmlIgnore]
		public SDictionary <string,int> IncomingBranches = new SDictionary<string, int> ();
		protected Vector3 mPosition;
		protected Vector3 mRotation;
		protected static PathMarkerInstanceTemplate gEmptyInstance;// = new PathMarkerInstanceTemplate ();
	}

	[Serializable]
	public class ChunkSceneryPrefabs
	{
		public List <ChunkPrefab> SolidTerrainPrefabs = new List <ChunkPrefab> ();
		public List <ChunkPrefab> SolidTerrainPrefabsAdjascent = new List <ChunkPrefab> ();
		public List <ChunkPrefab> SolidTerrainPrefabsDistant = new List <ChunkPrefab> ();
		public List <string> RiverNames = new List <string> ( );
		public FXPiece [] FXPieces = null;
	}

	[Serializable]
	public class ChunkPrefab : Mod
	{
		public ChunkPrefab ()
		{

		}

		public ChunkPrefab (Transform transform, string prefabName)
		{
			Transform = new STransform (transform, true);
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
		public STransform Transform = new STransform ();
		public List <STransform> BoxColliders = new List <STransform> ( );
		public List <string> SharedMaterialNames = new List <string> ( );
		public SDictionary <string, string> SceneryScripts = new SDictionary <string, string> ();

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
		public int HeightmapResolution = 0;
		public int HeightmapHeight = 0;
		public List <GroundType> SplatmapGroundTypes = new List <GroundType> ();
		public TerrainkMaterialSettings MaterialSettings = new TerrainkMaterialSettings ();
		public SColor GrassTint = Color.white;
		public float WindSpeed = 0.497f;
		public float WindSize = 0.493f;
		public float WindBending = 0.495f;
		public List <TerrainPrototypeTemplate> DetailTemplates = new List <TerrainPrototypeTemplate> ();
		public List <TerrainPrototypeTemplate> TreeTemplates = new List <TerrainPrototypeTemplate> ();
		public List <TerrainTextureTemplate> TextureTemplates = new List <TerrainTextureTemplate> ();
		public bool PassThroughChunkData = false;
	}
	//stores positions for where to spawn plants
	[Serializable]
	public class PlantInstanceTemplate : TreeInstanceTemplate
	{
		[XmlIgnore]
		public bool HasBeenPlanted {
			get {
				return !string.IsNullOrEmpty (PlantName);
			}
		}

		[XmlIgnore]
		public bool ReadyToBePlanted {
			get {
				if (PickedTime > 0) {
					return (WorldClock.Time - PickedTime > WorldClock.RTSecondsToGameSeconds (Globals.PlantAutoRegrowInterval));
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

		public PlantInstanceTemplate (TreeInstance treeInstance, float terrainHeight, Vector3 chunkOffset, Vector3 chunkScale) : base (treeInstance, terrainHeight, chunkOffset, chunkScale)
		{
		}

		public PlantInstanceTemplate (bool empty) : base (empty)
		{
		}

		public PlantInstanceTemplate () : base ()
		{

		}

		public static PlantInstanceTemplate Empty {
			get {
				return gEmptyPlantTemplate;
			}
		}

		protected static PlantInstanceTemplate gEmptyPlantTemplate = new PlantInstanceTemplate (true);
	}

	[Serializable]
	public class TreeInstanceTemplate : IHasPosition
	{
		public static TreeInstanceTemplate Empty {
			get {
				if (gEmptyTreeTemplate == null) {
					gEmptyTreeTemplate = new TreeInstanceTemplate (true);
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
				mChunkScale.Set (CSX, CSY, CSZ);
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
				mChunkOffset.Set (CX, CY, CZ);
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
				mLocalPosition.Set (X, Y, Z);
				return mLocalPosition;
			}
		}

		[XmlIgnore]
		protected Vector3 mLocalPosition;

		[XmlIgnore]
		public Vector3 Position {
			get { return Vector3.Scale (ChunkScale, LocalPosition) + ChunkOffset; }
		}

		public TreeInstanceTemplate (bool empty)
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

		public TreeInstanceTemplate (TreeInstance treeInstance, float terrainHeight, Vector3 chunkOffset, Vector3 chunkScale)
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

		public TreeInstanceTemplate ()
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
				TreeInstance treeInstance = new TreeInstance ();
				treeInstance.lightmapColor	= Color.white;
				treeInstance.color = new Color (R, G, B, A);
				treeInstance.position = new Vector3 (X, Y, Z);
				treeInstance.heightScale = HeightScale;
				treeInstance.widthScale = WidthScale;
				treeInstance.prototypeIndex	= PrototypeIndex;
				return treeInstance;
			}
		}

		protected static TreeInstanceTemplate gEmptyTreeTemplate;// = new TreeInstanceTemplate (true);
	}

	public interface IHasPosition
	{
		Vector3 Position {
			get;
		}
	}

	namespace World {
		[Serializable]
		public class Path : Mod
		{
			public SBounds PathBounds = new SBounds ();
			public List <PathMarkerInstanceTemplate> Templates = new List<PathMarkerInstanceTemplate> ();

			public void SetActive (bool active)
			{
				for (int i = 0; i < Templates.Count; i++) {
					Templates [i].IsActive = active;
				}
			}

			public void InitializeTemplates ()
			{
				for (int i = 0; i < Templates.Count; i++) {
					Templates [i].IsActive = false;
				}
			}

			public void RefreshTemplates ()
			{
				for (int i = 0; i < Templates.Count; i++) {
					PathMarkerInstanceTemplate pm = Templates [i];
					if (pm.PathName == Name) {
						pm.IndexInParentPath = i;
						pm.ParentPath = this;
						if (i == 0 || i == Templates.LastIndex ()) {
							pm.IsTerminal = true;
						}
					}
				}
			}

			public void RefreshBranches ()
			{	//this assumes parent paths have already been set
				for (int i = 0; i < Templates.Count; i++) {
					PathMarkerInstanceTemplate pm = Templates [i];
					if (pm.ParentPath != this) {
						//make sure each branch knows where the template is being used
						if (pm.Branches.ContainsKey (Name)) {
							//this template is being used in position i
							pm.Branches [Name] = i;
						}
					}
				}
			}
		}
	}

	[Serializable]
	public class ChunkTransforms
	{
		public Transform	Plants;
		public Transform	Terrain;
		public Transform	Nodes;
		public Transform	Triggers;
		public Transform	Paths;
		public Transform	WorldItems;
		public Transform	AboveGround;
		public Transform	BelowGround;
		public Transform	AboveGroundStaticImmediate;
		public Transform	AboveGroundStaticAdjascent;
		public Transform	AboveGroundStaticDistant;
		public Transform	AboveGroundGenerated;
		public Transform	AboveGroundOcean;
		public Transform	AboveGroundFX;
		public Transform	AboveGroundAudio;
		public Transform	BelowGroundStatic;
		public Transform	BelowGroundGenerated;
		public Transform	BelowGroundFX;
		public Transform	BelowGroundAudio;
		public Transform	AboveGroundWorldItems;
		public Transform	BelowGroundWorldItems;

		public Transform	AboveGroundRivers;
		public Transform	BelowGroundRivers;
	}
}