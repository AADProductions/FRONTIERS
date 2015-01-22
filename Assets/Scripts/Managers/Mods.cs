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
using System.Reflection;

namespace Frontiers
{
		[ExecuteInEditMode]
		public class Mods : Manager
		{
				public static Mods Get;
				public ModsEditor Editor = new ModsEditor();
				public ModsRuntime Runtime = new ModsRuntime();

				public override void WakeUp()
				{
						Get = this;
						Editor.mods = this;
						Runtime.mods = this;
						mParentUnderManager = false;
				}

				#region runtime / editor safe stuff

				public string WorldDescription(string worldName)
				{
						string description = "No description available.";
						string error = string.Empty;
						WorldSettings world = null;
						if (GameData.IO.LoadWorld(ref world, worldName, out error)) {
								description = world.Description;
						}
						return description;
				}

				public string Description <T>(string dataType, string modName) where T : Mod, new()
				{
						string description = "No description available.";
						T mod = null;
						if (Runtime.LoadMod <T>(ref mod, dataType, modName)) {
								description = mod.FullDescription;
						} else {
								Debug.Log("Couldn't load!");
						}
						return description;
				}

				public override void OnTextureLoadStart()
				{
						//prevent hiccups
						Shader.WarmupAllShaders();
						//this seems to load some dangly bits that we don't need yet
						//so force a collect before loading textures
						StartCoroutine(LoadGenericTexturesOverTime());
				}

				public override void OnModsLoadStart()
				{
						BuildCurrentWorld();
						mModsLoaded = true;
				}

				public void BuildCurrentWorld()
				{

				}

				public List <string> Types()
				{
						List <string> types = new List <string>();
						HashSet <string> folderNames = new HashSet <string>();
						GameData.IO.GetFolderNamesInProfileDirectory(folderNames, "");
						types.AddRange(folderNames);
						return types;
				}

				public void SaveLiveGame(string toSaveGame)
				{
						if (!GameData.IO.CopyGameData(GameData.IO.gLiveGameFolderName, toSaveGame)) {
								Debug.LogWarning("Couldn't save live game to " + toSaveGame);
						}
				}

				public void LoadLiveGame(string fromSaveGame)
				{
						if (!GameData.IO.CopyGameData(fromSaveGame, GameData.IO.gLiveGameFolderName)) {
								Debug.LogError("Couldn't load " + fromSaveGame + " to live game");
						}
				}

				public List <string> Available(string dataType)
				{
						List <string> available = new List <string>();
						HashSet <string> fileNames = new HashSet <string>();
						GameData.IO.GetFileNamesInProfileDirectory(fileNames, dataType);
						GameData.IO.GetFileNamesInWorldDirectory(fileNames, dataType);
						GameData.IO.GetFileNamesInBaseDirectory(fileNames, dataType);
						available.AddRange(fileNames);
						return available;
				}

				public List <string> Available(string dataType, string searchString)
				{
						List <string> available = new List <string>();
						HashSet <string> fileNames = new HashSet <string>();
						GameData.IO.GetFileNamesInProfileDirectory(fileNames, dataType);
						GameData.IO.GetFileNamesInWorldDirectory(fileNames, dataType);
						GameData.IO.GetFileNamesInBaseDirectory(fileNames, dataType);
						foreach (string availableItem in fileNames) {
								if (availableItem.Contains(searchString)) {
										available.Add(availableItem);
								}
						}
						return available;
				}

				public List <string> Available(string type, DataType from)
				{
						List <string> available = new List <string>();
						HashSet <string> fileNames = new HashSet <string>();
						switch (type) {
								case "World"://world is a special case - move it to AvailableWorlds
										available = GameData.IO.GetWorldNames();
										break;

								default:
										GameData.IO.GetFileNamesInDirectory(fileNames, type, false, from);
										available.AddRange(fileNames);
										break;
						}
						return available;
				}

				public List <string> GlobalDataNames(string dataType, string dataPath)
				{
						HashSet <string> fileNames = new HashSet <string>();
						GameData.IO.GetFileNamesInWorldDirectory(fileNames, System.IO.Path.Combine(dataType, dataPath));
						return new List <string>(fileNames);
				}

				public List <string> ModDataNames(string dataType)
				{
						HashSet <string> fileNames = new HashSet <string>();
						GameData.IO.GetFileNamesInWorldDirectory(fileNames, dataType);
						return new List <string>(fileNames);
				}

				#endregion

				public bool LoadPrefab(ref GameObject prefab, string prefabType, string prefabName)
				{
						string path = System.IO.Path.Combine(prefabType, prefabName);
						prefab = Resources.Load(path) as GameObject;

						if (prefab != null) {
								return true;
						}
						return false;
				}

				protected IEnumerator LoadGenericTexturesOverTime()
				{
						//give the other managers a second to put in their requests
						yield return null;
						while (TexturesToLoad.Count > 0) {
								TextureLoadRequest tlr = TexturesToLoad.Dequeue();
								//get a list of textures from that mod folder with that search string in the name
								List <string> availableTextures = null;
								if (!string.IsNullOrEmpty(tlr.TextureKeyword)) {
										availableTextures = Available(tlr.TextureType, tlr.TextureKeyword);
								} else {
										availableTextures = Available(tlr.TextureType);
								}

								foreach (string availableTexture in availableTextures) {
										Texture2D texture = null;
										IEnumerator loader = null;
										GUI.GUILoading.DetailsInfo = availableTexture;
										if (!Runtime.GenericTexture(ref texture, ref loader, availableTexture, tlr.TextureType, tlr.AsNormalMap, tlr.DesiredWidth, tlr.DesiredHeight)) {
												Debug.LogWarning("Couldn't load generic texture " + availableTexture);
												yield return null;
										} else {
												//if we loaded the texture add it to the list
												//if there is no list that's fine, it's cached in mods
												if (tlr.Textures != null) {
														tlr.Textures.Add(texture);
												}
												//if we need to resize/normal-map-ize it use the loader
												if (loader != null) {
														while (loader.MoveNext()) {
																//load...
																yield return null;
														}
												}
												System.GC.Collect();
												System.GC.WaitForPendingFinalizers();
										}
								}
								yield return null;
								Resources.UnloadUnusedAssets();
						}
						Manager.TexturesLoadFinish();
						yield break;
				}

				public Queue <TextureLoadRequest> TexturesToLoad = new Queue <TextureLoadRequest>();
				//pre-loads and caches all the textures with this keyword in this folder
				public void LoadAvailableGenericTextures(string textureKeyword, string textureType, bool asNormalMap, int desiredWidth, int desiredHeight, List <Texture2D> textures)
				{
						TexturesToLoad.Enqueue(new TextureLoadRequest(textureKeyword, textureType, asNormalMap, desiredWidth, desiredHeight, textures));
				}

				public void LoadAvailableGenericTextures(string textureType, bool asNormalMap, List <Texture2D> textures)
				{
						TexturesToLoad.Enqueue(new TextureLoadRequest(string.Empty, textureType, asNormalMap, -1, -1, textures));
				}

				public void LoadAvailableGenericTextures(string textureType, bool asNormalMap, int desiredWidth, int desiredHeight, List <Texture2D> textures)
				{
						TexturesToLoad.Enqueue(new TextureLoadRequest(string.Empty, textureType, asNormalMap, desiredWidth, desiredHeight, textures));
				}

				public class ModsRuntime
				{
						public Mods mods;

						#region game and profile data

						public bool LoadGame(ref PlayerGame game, string worldName, string gameName)
						{
								return GameData.IO.LoadGame(ref game, worldName, gameName);
						}

						public void DeleteGame(string gameName)
						{
								if (gameName != GameData.IO.gLiveGameFolderName) {
										GameData.IO.DeleteGameData(gameName);
								}
						}

						public void SaveGame(PlayerGame game)
						{
								game.Version = GameManager.VersionString;
								GameData.IO.SaveGame(game);
						}

						public string FullPath(string fileName, string dataType, string extension)
						{
								//TODO make this work with more than just the base
								System.IO.DirectoryInfo directory = System.IO.Directory.GetParent(Application.dataPath);
								string dataPath = directory.FullName;
								dataPath = System.IO.Path.Combine(dataPath, GameData.IO.GetDataPath(DataType.Base));
								dataPath = System.IO.Path.Combine(dataPath, System.IO.Path.Combine(dataType, (fileName + extension)));
								dataPath = System.IO.Path.Combine(Application.absoluteURL, dataPath);
								return dataPath;
						}

						public void ResetProfileData(string dataType)
						{
								if (dataType == "all") {
										foreach (string modType in Mods.Get.Types ( )) {
												GameData.IO.DeleteData(modType, DataType.Profile, DataCompression.None);
										}
								} else {
										dataType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataType.ToLower());
										//deletes local (profile) data
										GameData.IO.DeleteData(dataType, DataType.Profile, DataCompression.None);
								}
						}

						public void ResetProfileData(string dataType, string dataName)
						{
								dataType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataType.ToLower());
								//deletes local (profile) data
								GameData.IO.DeleteData(dataType, dataName, DataType.Profile, DataCompression.None);
						}

						public void DeleteMod(string dataType, string dataName)
						{
								GameData.IO.DeleteData(dataType, dataName, DataType.Profile, DataCompression.None);
						}

						#endregion

						#region textures

						public bool Texture(Texture2D texture, string dataType, string dataName)
						{
								//loads the pixels from the mod data into the existing texture
								//check to see if a local copy exists
								if (GameData.IO.LoadProfileTexture(texture, dataType, dataName)
								|| GameData.IO.LoadWorldTexture(texture, dataType, dataName)
								|| GameData.IO.LoadBaseTexture(texture, dataType, dataName)) {
										//always set this just in case
										texture.name = dataName;
										return true;
								}
								return false;
						}

						public bool CameraLUT(ref Texture2D lut, string lutName)
						{
								if (!GameData.IO.LoadLUT(ref lut, lutName, DataType.Profile)) {
										if (!GameData.IO.LoadLUT(ref lut, lutName, DataType.World)) {
												return GameData.IO.LoadLUT(ref lut, lutName, DataType.Base);
										}
								}
								return true;
						}

						public bool BodyTexture(ref Texture2D bodyTexture, string textureName)
						{
								int resolution = Globals.SmallCharacterBodyTextureResolution;
								if (textureName.StartsWith("Med")) {
										resolution = Globals.MediumCharacterBodyTextureResolution;
								} else if (textureName.StartsWith("Lrg")) {
										resolution = Globals.LargeCharacterBodyTextureResolution;
								}

								if (!GameData.IO.LoadCharacterTexture(ref bodyTexture, textureName, "Body", resolution, DataType.Profile)) {
										if (!GameData.IO.LoadCharacterTexture(ref bodyTexture, textureName, "Body", resolution, DataType.World)) {
												return GameData.IO.LoadCharacterTexture(ref bodyTexture, textureName, "Body", resolution, DataType.Base);
										}
								}
								return true;
						}

						public bool MaskTexture(ref Texture2D maskTexture, string textureName)
						{
								if (!GameData.IO.LoadCharacterTexture(ref maskTexture, textureName, "Mask", Globals.SmallCharacterBodyTextureResolution, DataType.Profile)) {
										if (!GameData.IO.LoadCharacterTexture(ref maskTexture, textureName, "Mask", Globals.SmallCharacterBodyTextureResolution, DataType.World)) {
												return GameData.IO.LoadCharacterTexture(ref maskTexture, textureName, "Mask", Globals.SmallCharacterBodyTextureResolution, DataType.Base);
										}
								}
								return true;
						}

						public bool FaceTexture(ref Texture2D faceTexture, string textureName)
						{
								if (!GameData.IO.LoadCharacterTexture(ref faceTexture, textureName, "Face", Globals.CharacterFaceTextureResolution, DataType.Profile)) {
										if (!GameData.IO.LoadCharacterTexture(ref faceTexture, textureName, "Face", Globals.CharacterFaceTextureResolution, DataType.World)) {
												return GameData.IO.LoadCharacterTexture(ref faceTexture, textureName, "Face", Globals.CharacterFaceTextureResolution, DataType.Base);
										}
								}
								return true;
						}

						public bool ChunkMap(ref Texture2D map, string chunkName, string mapName)
						{
								map = null;
								int resolution = 0;
								TextureFormat format = TextureFormat.ARGB32;
								bool linear = false;
								GameData.GetTextureFormat(mapName, ref resolution, ref format, ref linear);

								if (!GameData.IO.LoadTerrainMap(ref map, resolution, format, linear, chunkName, mapName, DataType.Profile)) {
										if (!GameData.IO.LoadTerrainMap(ref map, resolution, format, linear, chunkName, mapName, DataType.World)) {
												return GameData.IO.LoadTerrainMap(ref map, resolution, format, linear, chunkName, mapName, DataType.Base);
										}
								}
								return true;
						}

						public void UnloadChunkMap(Texture2D map)
						{	//remove it from the global lookup
								GameData.IO.gLoadedMaps.Remove(map.name);
								//destroy it to release memory
								GameObject.Destroy(map);
						}

						public bool GenericTexture(ref Texture2D texture, ref IEnumerator loader, string textureName, string textureType, bool asNormalMap)
						{
								return GenericTexture(ref texture, ref loader, textureName, textureType, asNormalMap, -1, -1);
						}

						public bool GenericTexture(ref Texture2D texture, ref IEnumerator loader, string textureName, string textureType, bool asNormalMap, int desiredWidth, int desiredHeight)
						{
								texture = null;
								if (!GameData.IO.LoadGenericTexture(ref texture, ref loader, textureName, textureType, asNormalMap, desiredWidth, desiredHeight, DataType.Profile)) {
										if (!GameData.IO.LoadGenericTexture(ref texture, ref loader, textureName, textureType, asNormalMap, desiredWidth, desiredHeight, DataType.World)) {
												return GameData.IO.LoadGenericTexture(ref texture, ref loader, textureName, textureType, asNormalMap, desiredWidth, desiredHeight, DataType.Base);
										}
								}
								return true;
						}

						#endregion

						#region terrain

						public bool LoadTerrainDetailSlice(int[,] slice, string chunkPathName, string sliceFileName)
						{
								if (!GameData.IO.LoadDetailSlice(slice, chunkPathName, sliceFileName, DataType.Profile)) {
										if (!GameData.IO.LoadDetailSlice(slice, chunkPathName, sliceFileName, DataType.World)) {
												return GameData.IO.LoadDetailSlice(slice, chunkPathName, sliceFileName, DataType.Base);
										}
								}
								return true;
						}

						public bool LoadTerrainDetailLayer(byte[,] detailLayer, string chunkName, int detailIndex)
						{
								string detailFileName = DetailAssetFileName(chunkName, detailIndex);
								string chunkPathName = System.IO.Path.Combine("Chunk", chunkName);
								if (!GameData.IO.LoadDetailLayer(detailLayer, chunkPathName, detailFileName, DataType.Profile)) {
										if (!GameData.IO.LoadDetailLayer(detailLayer, chunkPathName, detailFileName, DataType.World)) {
												return GameData.IO.LoadDetailLayer(detailLayer, chunkPathName, detailFileName, DataType.Base);
										}
								}
								return true;
						}

						public bool TerrainHeights(float[,] heights, int resolution, string chunkName, int terrainIndex)
						{
								string terrainFileName = TerrainAssetFileName(chunkName, terrainIndex);
								if (!GameData.IO.LoadTerrainHeights(heights, resolution, chunkName, terrainFileName, DataType.Profile)) {
										if (!GameData.IO.LoadTerrainHeights(heights, resolution, chunkName, terrainFileName, DataType.World)) {
												return GameData.IO.LoadTerrainHeights(heights, resolution, chunkName, terrainFileName, DataType.Base);
										}
								}
								return true;
						}
						//receives a stack item and a path from the server
						//find the object and updates its state or creates the object if it doesn't exist

						#endregion

						#region stackitmes & groups

						public void ReceiveStackItem(StackItem stackItem, MobileReference reference)
						{
								//first check to see if the item exists
								WorldItem childItem = null;
								if (WIGroups.FindChildItem(reference.GroupPath, reference.FileName, out childItem)) {
										childItem.ReceiveState(ref stackItem);
								} else {
										//if the item doesn't exist, see if the group exists
										WIGroup group = null;
										if (WIGroups.FindGroup(reference.GroupPath, out group)) {
												//if we found the group, add the stack item to the group
												//if the group is loaded it'll instantiate the worlditem automatically
												group.AddChildItem(stackItem);
										} else {
												//if we didn't find the child item or the group, that means this object isn't loaded
												//save it to disk and it will load the next time it's needed
												Mods.Get.Runtime.SaveStackItemToGroup(stackItem, reference.GroupPath);
										}
								}
						}
						//sends a stack item to the server
						//now that we're handling stuff with properties I'm actually having a tough time imagining when to use this
						//but I'm sure I'll think of something
						public void SendStackItem(StackItem stackItem, MobileReference reference)
						{
								//dum dee doo, send it to everyone
						}

						public List <string> GroupChildItemNames(string groupPath, bool includePathInName)
						{
								HashSet <string> childItemNames = new HashSet <string>();
								string fullPath = System.IO.Path.Combine("Group", groupPath);
				
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
								GameData.IO.GetFileNamesInProfileDirectory(childItemNames, fullPath);
								GameData.IO.GetFileNamesInWorldDirectory(childItemNames, fullPath);
								GameData.IO.GetFileNamesInBaseDirectory(childItemNames, fullPath);
								List <string> finalList = new List <string>();
								if (includePathInName) {
										foreach (string childItemName in childItemNames) {
												if (!childItemName.StartsWith("_")) {
														finalList.Add(string.Join(WIGroup.gPathJoinString, new string [] {
																groupPath,
																childItemName
														}));
												}
										}
								} else {
										finalList.AddRange(childItemNames);
								}
								return finalList;
						}

						public List <string> GroupChildGroupNames(string groupPath, bool includePathInName)
						{	//TODO filter by mobile reference
								HashSet <string> childGroupNames = new HashSet <string>();
								string path = System.IO.Path.Combine("Group", groupPath);
								GameData.IO.GetFolderNamesInProfileDirectory(childGroupNames, path);
								GameData.IO.GetFolderNamesInWorldDirectory(childGroupNames, path);
								GameData.IO.GetFolderNamesInBaseDirectory(childGroupNames, path);
								List <string> finalList = new List <string>();
								if (includePathInName) {
										foreach (string childGroupName in childGroupNames) {
												finalList.Add(string.Join(WIGroup.gPathJoinString, new string [] {
														groupPath,
														childGroupName
												}));
										}
								} else {
										finalList.AddRange(childGroupNames);
								}
								return finalList;
						}

						public void SaveGroupProps(WIGroupProps groupProps)
						{	
								groupProps.ListInAvailable = false;
								SaveMod <WIGroupProps>(groupProps, "Group", groupProps.UniqueID, groupProps.UniqueID);
						}

						public bool LoadGroupProps(ref WIGroupProps groupProps, string groupUniqueID)
						{
								return LoadMod <WIGroupProps>(ref groupProps, "Group", groupUniqueID, groupUniqueID);
						}

						public bool SaveStackItemToGroup(StackItem stackItem, string groupPath)
						{
								bool result = false;
								string fileName = stackItem.FileName;
								string fullPath = System.IO.Path.Combine("Group", groupPath);
				
								//get a stack item and save it to the group folder in profile
								stackItem.Version = GameManager.VersionString;
								GameData.IO.SaveProfileData <StackItem>(stackItem, fullPath, fileName, DataCompression.GZip);
								return result;
						}

						public bool LoadStackItemFromGroup(ref StackItem stackItem, string groupPath, string fileName, bool followReferences)
						{
								bool result = false;
								string fullPath = System.IO.Path.Combine("Group", groupPath);
								//first see if there's a reference file
								result = GameData.IO.LoadProfileData <StackItem>(ref stackItem, fullPath, fileName, DataCompression.GZip);
								if (!result) {	//next check the world data
										result = GameData.IO.LoadWorldData <StackItem>(ref stackItem, fullPath, fileName, DataCompression.GZip);
										if (!result) {	//if that has nothing, check the base data - we've got nowhere else to go at that point
												result = GameData.IO.LoadBaseData <StackItem>(ref stackItem, fullPath, fileName, DataCompression.GZip);
										}
								}
								return result;
						}

						#endregion

						#region raw mods

						public void SaveMod <T>(T data, string dataType, string dataPath, string dataName) where T : Mod, new()
						{
								SaveMod<T>(data, System.IO.Path.Combine(dataType, dataPath), dataName);
						}

						public void SaveMod <T>(T data, string dataType, string dataName) where T : Mod, new()
						{
								data.Name = dataName;//just in case
								data.Type = typeof(T).ToString();
								data.Version = GameManager.VersionString;
								GameData.IO.SaveProfileData <T>(data, dataType, dataName, DataCompression.None);
						}

						public void SaveMods <T>(List <T> data, string dataType) where T : Mod, new()
						{
								for (int i = 0; i < data.Count; i++) {
										SaveMod <T>(data[i], data[i].Name, dataType);
								}
						}

						public bool LoadMod <T>(ref T data, string dataType, string dataPath, string dataName) where T : Mod, new()
						{
								return LoadMod <T>(ref data, System.IO.Path.Combine(dataType, dataPath), dataName);
						}

						public bool LoadMod <T>(ref T data, string dataType, string dataName) where T : Mod, new()
						{
								//check to see if a local copy exists
								bool result = GameData.IO.LoadProfileData <T>(ref data, dataType, dataName, DataCompression.None);
								//hooray it exists
								if (result && data.IgnoreProfileDataIfOutdated) {
										//but are we supposed to use it?
										//see if the version is below the current game version
										if (new Version(data.Version) < GameManager.Version) {
												//if it is, get it from the base data
												result = GameData.IO.LoadWorldData <T>(ref data, dataType, dataName, DataCompression.None);
												if (!result) {
														//if it doesn't exist in profile, try getting it from the world data, then from the base data
														result = GameData.IO.LoadBaseData <T>(ref data, dataType, dataName, DataCompression.None);
												}
										}
								} else {
										result = GameData.IO.LoadWorldData <T>(ref data, dataType, dataName, DataCompression.None);
										if (!result) {
												//if it doesn't exist in profile, try getting it from the world data, then from the base data
												result = GameData.IO.LoadBaseData <T>(ref data, dataType, dataName, DataCompression.None);
										}
								}

								if (result) {
										//always set this just in case
										data.Name = dataName;
								}

								//whew! done
								return result;
						}

						public void LoadAvailableMods <T>(List<T> mods, string dataType) where T : Mod, new()
						{
								List <string> availableMods = Get.Available(dataType);
								T mod = null;
								for (int i = 0; i < availableMods.Count; i++) {
										if (LoadMod <T>(ref mod, dataType, availableMods[i]) && mod.ListInAvailable) {
												mods.Add(mod);
										} else {
												Debug.Log("Couldn't load mod " + dataType + ", " + availableMods[i]);
										}
								}
						}

						#endregion

				}

				public class ModsEditor
				{
						//this is the class used to save / load stuff in the editor while building the world
						//it's really similar to mods runtime but it assumes you're saving to the base data set
						public Mods mods;

						public void InitializeEditor()
						{
								InitializeEditor(false);
						}

						public void InitializeEditor(bool overrideAppPlaying)
						{
								if (Application.isPlaying && !overrideAppPlaying) {
										return;
								}

								if (Mods.Get == null) {
										GameObject modsGameObject = GameObject.Find("__MODS");
										Mods.Get = modsGameObject.GetComponent <Mods>();
								}

								string errorMessage = string.Empty;
								GameData.IO.InitializeSystemPaths(out errorMessage);
								Data.GameData.IO.SetDefaultLocalDataPaths();
						}

						public List <string> Available(string dataType)
						{
								List <string> available = new List <string>();
								HashSet <string> fileNames = new HashSet <string>();
								GameData.IO.GetFileNamesInWorldDirectory(fileNames, dataType);
								available.AddRange(fileNames);
								return available;
						}

						public List <string> GroupChildItemNames(string groupPath)
						{
								HashSet <string> childItemNames = new HashSet <string>();
								string fullPath = System.IO.Path.Combine("Group", groupPath);
								GameData.IO.GetFileNamesInBaseDirectory(childItemNames, fullPath);
				
								return new List <string>(childItemNames);
						}

						public List <string> GroupChildGroupNames(string groupPath)
						{
								HashSet <string> folderNames = new HashSet <string>();
								string fullPath = System.IO.Path.Combine("Group", groupPath);
								GameData.IO.GetFolderNamesInBaseDirectory(folderNames, System.IO.Path.Combine("Group", groupPath));
				
								return new List <string>(folderNames);
						}

						public void SaveMod <T>(T data, string dataType, string dataPath, string dataName) where T : Mod, new()
						{
								SaveMod <T>(data, System.IO.Path.Combine(dataType, dataPath), dataName);
						}

						public void SaveMod <T>(T data, string dataType, string dataName) where T : Mod, new()
						{
								if (string.IsNullOrEmpty(dataName)) {
										return;
								}
								data.Name = dataName;
								data.Type = typeof(T).ToString();
								data.Version = GameManager.VersionString;
								GameData.IO.SaveBaseData <T>(data, dataType, dataName, DataCompression.None);
								gAvailable.Clear();
						}

						public void DeleteMod(string dataType, string dataPath)
						{
								GameData.IO.DeleteData(System.IO.Path.Combine(dataType, dataPath), DataType.World, DataCompression.None);
						}

						public void DeleteGroup(WIGroupProps groupProps)
						{
								GameData.IO.DeleteData(System.IO.Path.Combine("Group", groupProps.UniqueID), DataType.Base, DataCompression.None);
						}

						public void SaveGroupProps(WIGroupProps groupProps)
						{	
								groupProps.ListInAvailable = false;
								SaveMod <WIGroupProps>(groupProps, "Group", groupProps.UniqueID, groupProps.UniqueID);
						}

						public bool LoadMod <T>(ref T data, string dataType, string dataName) where T : Mod, new()
						{
								if (GameData.IO.LoadBaseData <T>(ref data, dataType, dataName, DataCompression.None)) {
										data.Name = dataName;
										return true;
								}
								return false;
						}

						public void SaveMods <T>(List <T> data, string dataType) where T : Mod, new()
						{
								for (int i = 0; i < data.Count; i++) {
										SaveMod <T>(data[i], dataType, data[i].Name);
								}
						}

						public void LoadAvailableMods <T>(List<T> mods, string dataType) where T : Mod, new()
						{
								List <string> availableMods = Get.Available(dataType);
								T mod = null;
								for (int i = 0; i < availableMods.Count; i++) {
										if (LoadMod <T>(ref mod, dataType, availableMods[i])) {
												mods.Add(mod);
										}
								}
						}

						public bool SaveMesh(Mesh meshToSave, Material[] matsToSave, string dataType, string dataName)
						{
								return GameData.IO.SaveObjFromMesh(meshToSave, matsToSave, dataType, dataName, DataType.Base);
						}

						public bool LoadMesh(ref GameObject meshGameObject, string dataType, string dataName)
						{
								GameObject[] loadedMeshes = null;
								if (GameData.IO.LoadObjToGameObjects(ref loadedMeshes, dataType, dataName, DataType.Base)) {
										if (loadedMeshes.Length > 0) {
												meshGameObject = loadedMeshes[0];
												return true;
										}
								}
								return false;
						}

						public void SaveTerrainDetailSlice(int[,] detailSlice, string chunkName, string detailSliceFileName)
						{
								string chunkPathName = System.IO.Path.Combine("Chunk", chunkName);
								GameData.IO.SaveDetailSlice(detailSlice, chunkPathName, detailSliceFileName, DataType.World);
						}

						public void SaveTerrainDetailLayer(int[,] detailLayer, string chunkName, int detailIndex)
						{
								string detailFileName = DetailAssetFileName(chunkName, detailIndex);
								string chunkPathName = System.IO.Path.Combine("Chunk", chunkName);
								GameData.IO.SaveDetailLayer(detailLayer, chunkPathName, detailFileName, DataType.World);
						}

						public void SaveWorldSettings(WorldSettings settings)
						{

						}

						public bool ChunkMap(ref Texture2D map, string chunkName, string mapName)
						{
								map = null;
								int resolution = 0;
								TextureFormat format = TextureFormat.ARGB32;
								bool linear = false;
								if (!Manager.IsAwake <GameWorld>()) {
										Manager.WakeUp <GameWorld>("__WORLD");
								}
								GameData.GetTextureFormat(mapName, ref resolution, ref format, ref linear);
								if (!GameData.IO.LoadTerrainMap(ref map, resolution, format, linear, chunkName, mapName, DataType.Profile)) {
										if (!GameData.IO.LoadTerrainMap(ref map, resolution, format, linear, chunkName, mapName, DataType.World)) {
												return GameData.IO.LoadTerrainMap(ref map, resolution, format, linear, chunkName, mapName, DataType.Base);
										}
								}
								return true;
						}

						public bool LoadStackItemFromGroup(ref StackItem stackItem, string groupPath, string fileName)
						{
								bool result = false;
								string fullPath = System.IO.Path.Combine("Group", groupPath);
								return GameData.IO.LoadBaseData <StackItem>(ref stackItem, fullPath, fileName, DataCompression.GZip);
						}

						public bool BodyTexture(ref Texture2D bodyTexture, string textureName)
						{
								int resolution = Globals.SmallCharacterBodyTextureResolution;
								if (textureName.StartsWith("Med")) {
										resolution = Globals.MediumCharacterBodyTextureResolution;
								} else if (textureName.StartsWith("Lrg")) {
										resolution = Globals.LargeCharacterBodyTextureResolution;
								}
								return GameData.IO.LoadCharacterTexture(ref bodyTexture, textureName, "Body", resolution, DataType.Base);
						}

						public bool MaskTexture(ref Texture2D maskTexture, string textureName)
						{
								return GameData.IO.LoadCharacterTexture(ref maskTexture, textureName, "Mask", Globals.SmallCharacterBodyTextureResolution, DataType.Base);
						}

						public bool FaceTexture(ref Texture2D faceTexture, string textureName)
						{
								return GameData.IO.LoadCharacterTexture(ref faceTexture, textureName, "Face", Globals.CharacterFaceTextureResolution, DataType.Base);
						}
						#if UNITY_EDITOR
						//editor utilities
						public static string GUILayoutMissionVariable(string missionName, string variableName, bool includeNone, string noneOption, int maxWidth)
						{

								noneOption = "(" + noneOption + ")";
								Frontiers.World.Gameplay.MissionState mission = null;
								if (Mods.Get.Editor.LoadMod <Frontiers.World.Gameplay.MissionState>(ref mission, "Mission", missionName)) {
										List <string> variableNames = new List <string>();
										foreach (KeyValuePair <string,int> missionVar in mission.Variables) {
												variableNames.Add(missionVar.Key);
										}
										if (includeNone) {
												variableNames.Insert(0, noneOption);
										}
										int currentItemIndex = variableNames.IndexOf(variableName);
										currentItemIndex = UnityEditor.EditorGUILayout.Popup(currentItemIndex, variableNames.ToArray(), GUILayout.MaxWidth(maxWidth));
										if (currentItemIndex < 0 || currentItemIndex >= variableNames.Count) {
												currentItemIndex = 0;
										}
										string newItem = variableNames[currentItemIndex];
										if (newItem == noneOption) {
												return string.Empty;
										}
										return newItem;
								}
								return string.Empty;
						}

						public static string GUILayoutMissionObjective(string missionName, string objectiveName, bool includeNone, string noneOption, int maxWidth)
						{

								noneOption = "(" + noneOption + ")";
								Frontiers.World.Gameplay.MissionState mission = null;
								if (Mods.Get.Editor.LoadMod <Frontiers.World.Gameplay.MissionState>(ref mission, "Mission", missionName)) {
										List <string> objectiveNames = mission.GetObjectiveNames();
										if (includeNone) {
												objectiveNames.Insert(0, noneOption);
										}
										int currentItemIndex = objectiveNames.IndexOf(objectiveName);
										currentItemIndex = UnityEditor.EditorGUILayout.Popup(currentItemIndex, objectiveNames.ToArray(), GUILayout.MaxWidth(maxWidth));
										if (currentItemIndex < 0 || currentItemIndex >= objectiveNames.Count) {
												currentItemIndex = 0;
										}
										string newItem = objectiveNames[currentItemIndex];
										if (newItem == noneOption) {
												return string.Empty;
										}
										return newItem;
								}
								return string.Empty;
						}

						public static string GUILayoutAvailable(string currentItem, string modType, int maxWidth)
						{
								return GUILayoutAvailable(currentItem, modType, false, string.Empty, maxWidth);
						}

						public static string GUILayoutAvailable(string currentItem, string modType, bool includeNone, int maxWidth)
						{
								return GUILayoutAvailable(currentItem, modType, includeNone, "None", maxWidth);
						}

						public static string GUILayoutGlobalValue(string currentValue, string globalVariableName)
						{
								FieldInfo field = null;
								string finalValue = currentValue;
								if (string.IsNullOrEmpty(finalValue)) {
										finalValue = "0";
								}

								if (string.IsNullOrEmpty(globalVariableName)) {
										Debug.Log("Global variable name was null");
										finalValue = UnityEditor.EditorGUILayout.TextField(finalValue);
								} else {

										if (Globals.Fields == null) {
												Globals.RefreshLookup();
										}

										if (Globals.Fields.TryGetValue(globalVariableName.ToLower(), out field)) {
												switch (field.FieldType.Name) {
														case "Int32":
																finalValue = UnityEditor.EditorGUILayout.IntField((int)Int32.Parse(finalValue)).ToString();
																break;

														case "Boolean":
																finalValue = UnityEditor.EditorGUILayout.Toggle((bool)Boolean.Parse(finalValue)).ToString();
																break;

														case "Single":
																finalValue = UnityEditor.EditorGUILayout.FloatField((float)Single.Parse(finalValue)).ToString();
																break;

														case "Double":
																finalValue = UnityEditor.EditorGUILayout.FloatField((float)Double.Parse(finalValue)).ToString();
																break;

														default:
																Debug.Log("Couldn't determine field type for global " + globalVariableName);
																finalValue = UnityEditor.EditorGUILayout.TextField(finalValue);
																break;
												}
										}
								}
								return finalValue;
						}

						public static string GUILayoutGLobal(string currentGlobal)
						{
								List <string> difficultySettingVariables = Globals.GetDifficultySettingNames();
								int currentItemIndex = difficultySettingVariables.IndexOf(currentGlobal);
								currentItemIndex = UnityEditor.EditorGUILayout.Popup(currentItemIndex, difficultySettingVariables.ToArray());
								if (currentItemIndex < 0 || currentItemIndex >= difficultySettingVariables.Count) {
										currentItemIndex = 0;
								}
								string newItem = currentGlobal;
								try {
										newItem = difficultySettingVariables[currentItemIndex];
										//take it out again just in case
								} catch (Exception e) {
										Debug.LogError("Exception with available " + e.ToString());
								}
								return newItem;
						}

						public static string GUILayoutAvailable(string currentItem, string modType, bool includeNone, string noneOption, int maxWidth)
						{
								noneOption = "(" + noneOption + ")";
								List <string> available = null;
								if (!gAvailable.TryGetValue(modType, out available) || available.Count == 0) {
										if (!Manager.IsAwake <Mods>()) {
												Manager.WakeUp <Mods>("__MODS");
												Mods.Get.Editor.InitializeEditor(true);
										}
										available = Get.Available(modType);
										gAvailable.Add(modType, available);
								}
								if (includeNone) {
										if (!available.Contains(noneOption)) {
												available.Insert(0, noneOption);
										}
								} else {
										available.Remove(noneOption);
								}
								int currentItemIndex = available.IndexOf(currentItem);
								currentItemIndex = UnityEditor.EditorGUILayout.Popup(currentItemIndex, available.ToArray(), GUILayout.MaxWidth(maxWidth));
								if (currentItemIndex < 0 || currentItemIndex >= available.Count) {
										currentItemIndex = 0;
								}
								string newItem = currentItem;
								try {
										newItem = available[currentItemIndex];
										if (newItem == noneOption) {
												return string.Empty;
										}
										//take it out again just in case
										available.Remove(noneOption);
								} catch (Exception e) {
										Debug.LogError("Exception with available " + modType + " - num available: " + available.Count.ToString() + " " + e.ToString());
								}
								return newItem;
						}
						#endif
						protected static Dictionary <string, List <string>> gAvailable = new Dictionary<string, List<string>>();
				}

				#region static helper classes

				public struct TextureLoadRequest : IEquatable <TextureLoadRequest>
				{
						public TextureLoadRequest(string textureKeyword, string textureType, bool asNormalMap, List <Texture2D> textures)
						{
								TextureType = textureType;
								TextureKeyword = textureKeyword;
								AsNormalMap = asNormalMap;
								DesiredWidth = -1;
								DesiredHeight = -1;
								Textures = textures;
						}

						public TextureLoadRequest(string textureKeyword, string textureType, bool asNormalMap, int desiredWidth, int desiredHeight, List <Texture2D> textures)
						{
								TextureType = textureType;
								TextureKeyword = textureKeyword;
								AsNormalMap = asNormalMap;
								DesiredWidth = desiredWidth;
								DesiredHeight = desiredHeight;
								Textures = textures;
						}

						public string TextureType;
						public string TextureKeyword;
						public bool AsNormalMap;
						public int DesiredWidth;
						public int DesiredHeight;
						public List<Texture2D> Textures;

						public bool Equals(TextureLoadRequest other)
						{
								return TextureType.Equals(TextureType)
								&& TextureKeyword.Equals(TextureKeyword)
								&& AsNormalMap == AsNormalMap
								&& DesiredWidth == DesiredWidth
								&& DesiredHeight == DesiredHeight;
						}
				}

				public static string WorldRegionName(int xTilePosition, int zTilePosition)
				{
						return xTilePosition.ToString("D4") + "_" + zTilePosition.ToString("D4");
				}

				public static string TerrainAssetFileName(string chunkName, int terrainIndex)
				{
						return "Raw16Mac";
				}

				public static string DetailAssetFileName(string chunkName, int detailIndex)
				{
						return "Int2DAsByteArray-" + detailIndex.ToString();
				}

				#endregion

		}

		[Serializable]
		public class Mod : IComparable <Mod>
		{
				public Mod()
				{
				}

				public virtual string FullDescription { 
						get {
								if (string.IsNullOrEmpty(mFullDescription)) {
										mFullDescription = Description;
								}
								return Description;
						}
				}

				public virtual bool IgnoreProfileDataIfOutdated {
						get {
								return false;
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

				public virtual int CompareTo(Mod other)
				{
						return DisplayOrder.CompareTo(other.DisplayOrder);
				}

				[NonSerialized]
				protected string mFullDescription = string.Empty;
		}
}