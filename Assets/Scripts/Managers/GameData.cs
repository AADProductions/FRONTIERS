#pragma warning disable 0219
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Hydrogen.Serialization;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Frontiers
{
	namespace Data
	{
		[ExecuteInEditMode]
		public class GameData : Manager
		{
			public GameData Get;

			public override void Awake ()
			{
				mParentUnderManager = false;
				Get = this;
				IO.gLoadedMaps = new Dictionary<string, Texture2D> ();
				IO.gLoadedAudioClips = new Dictionary<string, AudioClip> ();
				base.Awake ();
			}

			public static class IO
			{	//this is what most of the game uses to read / save files
				//if it's not going through MODS, it's going through this class
				//get ready for tons of ooooooverloooooading!
				#region initialize

				public static void GetChangeLog (ref string changeLogText, ref System.DateTime changeLogTime)
				{
					if (File.Exists (gGlobalChangeLogPath)) {
						changeLogText = File.ReadAllText (gGlobalChangeLogPath);
						changeLogTime = File.GetLastWriteTime (gGlobalChangeLogPath);
					} else {
						changeLogText = "Change log not found";
						changeLogTime = DateTime.Now;
					}
				}

				public static void SaveGlobals (List<KeyValuePair<string, string>> globalPairs)
				{
					string iniPath = System.IO.Path.Combine (gGlobalDataPath, "Globals.ini");
					File.WriteAllText (iniPath, INI.Serialize (globalPairs));
				}

				public static bool LoadGlobals (ref List <KeyValuePair <string,string>> globalKeys, out string error)
				{
					error = string.Empty;
					string iniPath = System.IO.Path.Combine (gGlobalDataPath, "Globals.ini");
					if (File.Exists (iniPath)) {
						string objString = File.ReadAllText (iniPath);
						globalKeys = INI.Deserialize (objString);
						return true;
					} else {
						error = "Globals.ini not found in " + iniPath;
					}
					return false;
				}

				public static bool InitializeSystemPaths (out string errorMessage)
				{
					bool result = true;
					errorMessage = string.Empty;
					//gGlobalDataPath
					//gGlobalProfilesPath
					//gGlobalWorldsPath
					//gBaseWorldPath
					//gBaseWorldModsPath
					//gCurrentWorldPath
					//gCurrentWorldModsPath

					switch (Application.platform) {
					case RuntimePlatform.LinuxPlayer:
						//this will give us ??
						gGlobalDataPath = System.IO.Path.Combine (Application.dataPath, gFrontiersPrefix);
						gGlobalChangeLogPath = System.IO.Path.Combine (gGlobalDataPath, "changelog.txt");
						break;

					case RuntimePlatform.WindowsEditor:
					case RuntimePlatform.WindowsPlayer:
						gGlobalDataPath	= gFrontiersPrefix;
						gGlobalChangeLogPath = "changelog.txt";
						break;

					case RuntimePlatform.OSXPlayer:
						gGlobalDataPath = System.IO.Path.Combine (Application.dataPath, gFrontiersPrefix);
						gGlobalChangeLogPath = System.IO.Path.Combine (gGlobalDataPath, "changelog.txt");
						break;
					// HACK: This is only to fix your building into the root of the project. Bad form!
					case RuntimePlatform.OSXEditor:
						gGlobalDataPath = gFrontiersPrefix;
						break;

					default:
						errorMessage = "Don't know what platform you're running, dude.\n";
						result = false;
						break;
					}

					gGlobalWorldsPath = System.IO.Path.Combine (gGlobalDataPath, gGlobalWorldFolderName);
					gGlobalProfilesPath = System.IO.Path.Combine (gGlobalDataPath, gProfilesFolderName);
					gBaseWorldPath = System.IO.Path.Combine (gGlobalWorldsPath, gBaseWorldFolderName);
					gBaseWorldModsPath = System.IO.Path.Combine (gBaseWorldPath, gModsFolderName);
					if (string.IsNullOrEmpty (gCurrentWorldPath)) {
						gCurrentWorldPath = gBaseWorldPath;
					}
					gCurrentWorldModsPath = gBaseWorldModsPath;

					if (!Directory.Exists (gGlobalDataPath)) {
						errorMessage += ("Global data path not found at " + gGlobalDataPath + "\n");
						result = false;
					}
					if (!Directory.Exists (gGlobalProfilesPath)) {
						Directory.CreateDirectory (gGlobalProfilesPath);
					}
					if (!Directory.Exists (gGlobalWorldsPath)) {
						errorMessage += ("Global worlds path not found at " + gGlobalWorldsPath + "\n");
						result = false;
					}
					if (!Directory.Exists (gBaseWorldPath)) {
						errorMessage += ("Base world path not found at " + gBaseWorldPath + "\n");
						result = false;
					}
					if (!Directory.Exists (gBaseWorldModsPath)) {
						errorMessage += ("Base world mods path not found at " + gBaseWorldModsPath + "\n");
						result = false;
					}

					return result;
				}

				public static void SetDefaultLocalDataPaths ()
				{
					//this function is no longer necessary
					//string errorMessage = string.Empty;
					//InitializeLocalDataPaths ("Default", "FRONTIERS", "Default", out errorMessage);
				}

				public static bool InitializeLocalDataPaths (string profileName, string worldName, string gameName, out string errorMessage)
				{
					//gCurrentProfilePath
					//gCurrentProfileWorldPath
					//gCurrentProfileGamePath
					//gCurrentProfileModsPath
					//gCurrentProfileLiveGamePath

					//the live game path is where game data is stored until the player issues an explicit 'save' command
					//then the live game data is copied to another folder with that name

					bool result = true;
					errorMessage	= string.Empty;

					//FRONTIERS/Profiles/
					gCurrentProfilePath = System.IO.Path.Combine (gGlobalProfilesPath, profileName);
					if (!Directory.Exists (gCurrentProfilePath)) {
						try {
							Directory.CreateDirectory (gCurrentProfilePath);
						} catch (Exception e) {
							errorMessage += e.ToString () + "\n";
							result = false;
						}
					}

					//FRONTIERS/Profiles/ProfileName/WorldName/
					gCurrentWorldPath = System.IO.Path.Combine (gCurrentProfilePath, worldName);
					if (!Directory.Exists (gCurrentWorldPath)) {
						try {
							Directory.CreateDirectory (gCurrentWorldPath);
						} catch (Exception e) {
							errorMessage += e.ToString () + "\n";
							result = false;
						}
					}

					//FRONTIERS/Profiles/ProfileName/WorldName/GameName/
					gCurrentGamePath = System.IO.Path.Combine (gCurrentWorldPath, gameName);
					if (!Directory.Exists (gCurrentGamePath)) {
						try {
							Directory.CreateDirectory (gCurrentGamePath);
						} catch (Exception e) {
							errorMessage += e.ToString () + "\n";
							result = false;
						}
					}

					//FRONTIERS/Profiles/ProfileName/WorldName/GameName/Mods/
					gCurrentProfileModsPath = System.IO.Path.Combine (gCurrentGamePath, gModsFolderName);
					if (!Directory.Exists (gCurrentProfileModsPath)) {
						try {
							Directory.CreateDirectory (gCurrentProfileModsPath);
						} catch (Exception e) {
							errorMessage += e.ToString () + "\n";
							result = false;
						}
					}

					gCurrentProfileLiveGamePath = System.IO.Path.Combine (gCurrentWorldPath, gLiveGameFolderName);
					if (!Directory.Exists (gCurrentProfileLiveGamePath)) {
						try {
							Directory.CreateDirectory (gCurrentProfileLiveGamePath);
						} catch (Exception e) {
							errorMessage += e.ToString () + "\n";
							result = false;
						}
					}

					return result;
				}

				#endregion

				#region list objects
				//these are used by MODS & editor classes to figure out which of [x] mods / games / worlds are available
				public static bool GetFolderNamesInBaseDirectory (HashSet <string> folderNames, string directoryName)
				{
					return GetFolderNamesInDirectory (folderNames, directoryName, false, DataType.Base);
				}

				public static bool GetFolderNamesInWorldDirectory (HashSet <string> folderNames, string directoryName)
				{
					return GetFolderNamesInDirectory (folderNames, directoryName, false, DataType.World);
				}

				public static bool GetFolderNamesInProfileDirectory (HashSet <string> folderNames, string directoryName)
				{
					return GetFolderNamesInDirectory (folderNames, directoryName, false, DataType.Profile);
				}

				public static bool GetFileNamesInBaseDirectory (HashSet <string> fileNames, string directoryName)
				{
					return GetFileNamesInDirectory (fileNames, directoryName, false, DataType.Base);
				}

				public static bool GetFileNamesInWorldDirectory (HashSet <string> fileNames, string directoryName)
				{
					return GetFileNamesInDirectory (fileNames, directoryName, false, DataType.World);
				}

				public static bool GetFileNamesInProfileDirectory (HashSet <string> fileNames, string directoryName)
				{
					return GetFileNamesInDirectory (fileNames, directoryName, false, DataType.Profile);
				}

				public static List <string> GetWorldNames ()
				{
					string directory = gGlobalWorldsPath;

					//////Debug.Log ("Final directory: " + directory);
					List <string> filesInDirectory	= new List <string> ();
					//#if UNITY_STANDALONE_WIN
					if (Directory.Exists (directory)) {
						//////Debug.Log ("Directory " + directory + " exists");
						System.IO.DirectoryInfo filesDirectory = new System.IO.DirectoryInfo (directory);
						foreach (System.IO.FileInfo file in filesDirectory.GetFiles ( )) {
							//////Debug.Log ("Adding file in directory " + file.Name);
							filesInDirectory.Add (System.IO.Path.GetFileNameWithoutExtension (file.Name));
						}
					}
					//				else {
					//					//////Debug.LogWarning ("path " + directory + " doesn't exist.");
					//				}
					//#endif
					return filesInDirectory;
				}

				public static bool GetFileNamesInDirectory (HashSet <string> fileNames, string directoryName, bool includeExtension, DataType type)
				{
					bool result = false;
					string dataPath = GetDataPath (type);
					string directory	= System.IO.Path.Combine (dataPath, directoryName);

					//////Debug.Log ("Final directory: " + directory);
					if (Directory.Exists (directory)) {
						result = true;
						//////Debug.Log ("Directory " + directory + " exists");
						System.IO.DirectoryInfo filesDirectory = new System.IO.DirectoryInfo (directory);
						foreach (System.IO.FileInfo file in filesDirectory.GetFiles ( )) {
							//////Debug.Log ("Adding file in directory " + file.Name);
							if (!file.Name.StartsWith ("_")) {
								if (includeExtension) {
									fileNames.Add (file.Name);
								} else {
									fileNames.Add (System.IO.Path.GetFileNameWithoutExtension (file.Name));
								}
							}
						}
					} else {
						//////Debug.LogWarning ("path " + directory + " doesn't exist.");
						result = false;
					}
					//#endif
					return result;
				}

				public static bool GetFolderNamesInDirectory (HashSet <string> folderNames, string directoryName, bool includeExtension, DataType type)
				{
					bool result = false;
					string dataPath = GetDataPath (type);
					string directory	= System.IO.Path.Combine (dataPath, directoryName);

					//////Debug.Log ("Final directory: " + directory);
					if (Directory.Exists (directory)) {
						result = true;
						System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo (directory);
						foreach (System.IO.DirectoryInfo folderName in directoryInfo.GetDirectories ( )) {
							//////Debug.Log (folderName.Name);
							if (folderName.Name != "_ignore" && folderName.Name != "_trash") {
								folderNames.Add (folderName.Name);
							}
						}
					} else {
						////////Debug.LogWarning ("path " + directory + " doesn't exist.");
						result = false;
					}
					//#endif
					return result;
				}

				public static List <string> GetFolderNamesInDirectory (string path)
				{
					System.IO.DirectoryInfo profileDirectory = new System.IO.DirectoryInfo (path);
					List <string> folderNames = new List <string> ();
					//////Debug.Log ("Getting names in directory " + path);
					if (Directory.Exists (path)) {
						foreach (System.IO.DirectoryInfo folderName in profileDirectory.GetDirectories ( )) {
							//////Debug.Log (folderName.Name);
							if (!folderName.Name.StartsWith ("_")) {
								folderNames.Add (folderName.Name);
							}
						}
					}
					//#endif
					return folderNames;
				}

				#endregion

				#region load / save world and profile
				//worlds, profiles, preferences & games exist 'outside' of mods so we have functions specifically for saving/loading them
				public static bool SaveWorldSettings (WorldSettings settings)
				{
					string directory = gGlobalWorldsPath;

					if (Directory.Exists (directory) && !string.IsNullOrEmpty (settings.Name)) {
						string path = System.IO.Path.Combine (directory, settings.Name) + gDataExtension;
						SerializeXMLToFile <WorldSettings> (settings, path);
						return true;
					}

					return false;
				}

				public static bool LoadWorld (ref WorldSettings world, string worldName, out string errorMessage)
				{
					errorMessage = string.Empty;

					string path = System.IO.Path.Combine (gGlobalWorldsPath, (worldName + gDataExtension));
					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (File.Exists (path)) {
						try {
							var serializer = new XmlSerializer (typeof(WorldSettings));
							var stream = new FileStream (path, mode, access, share);
							world = (WorldSettings)serializer.Deserialize (stream);
							return true;
						} catch (Exception e) {
							errorMessage = "Couldn't find world because " + e.ToString ();
						}
					}
					errorMessage = "Couldn't find world";
					return false;
				}

				public static void SaveWorld (WorldSettings world)
				{
					string path = System.IO.Path.Combine (gGlobalWorldsPath, (world.Name + gDataExtension));
					FileMode mode = FileMode.CreateNew;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (File.Exists (path)) {
						mode = FileMode.Open;
					}

					var serializer = new XmlSerializer (typeof(PlayerProfile));
					using (var stream = new FileStream (path, mode, access, share)) {
						serializer.Serialize (stream, world);
					}
				}

				public static bool LoadProfile (ref PlayerProfile profile, string profileName, out string errorMessage)
				{
					errorMessage = string.Empty;

					string path = System.IO.Path.Combine (gGlobalProfilesPath, profileName);
					path = System.IO.Path.Combine (path, (profileName + gProfileExtension));
					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (!DeserializeXMLFromFile <PlayerProfile> (ref profile, path)) {
						errorMessage = "Couldn't find profile";
						return false;
					}
					return true;
				}

				public static void SaveProfile (PlayerProfile profile)
				{
					string path = System.IO.Path.Combine (gGlobalProfilesPath, profile.Name);
					path = System.IO.Path.Combine (path, (profile.Name + gProfileExtension));
					FileMode mode = FileMode.CreateNew;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					SerializeXMLToFile <PlayerProfile> (profile, path);
				}

				public static bool LoadPreferences (ref PlayerPreferences prefs, string profileName, out string errorMessage)
				{
					errorMessage = string.Empty;

					string path = System.IO.Path.Combine (gGlobalProfilesPath, profileName);
					path = System.IO.Path.Combine (path, "Preferences" + gPreferencesExtension);

					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (!DeserializeXMLFromFile <PlayerPreferences> (ref prefs, path)) {
						errorMessage = "Couldn't find profile";
						////Debug.Log ("Couldn't find profile at " + path);
						return false;
					}
					return true;
				}

				public static void SavePreferences (string profileName, PlayerPreferences preferences)
				{
					string path = System.IO.Path.Combine (gGlobalProfilesPath, profileName);
					path = System.IO.Path.Combine (path, "Preferences" + gPreferencesExtension);
					SerializeXMLToFile <PlayerPreferences> (preferences, path);
				}

				public static bool LoadGame (ref PlayerGame game, string worldName, string gameName)
				{
					string path = System.IO.Path.Combine (gCurrentProfilePath, worldName);
					path = System.IO.Path.Combine (path, (gameName + gGameExtension));
					return DeserializeXMLFromFile <PlayerGame> (ref game, path);
				}

				public static void SaveGame (PlayerGame game)
				{
					if (string.IsNullOrEmpty (gCurrentProfilePath) || string.IsNullOrEmpty (game.WorldName)) {
						Debug.LogError ("Couldn't save game, profile extension or game world name is empty is empty");
						return;
					}
					string path = System.IO.Path.Combine (gCurrentProfilePath, game.WorldName);
					path = System.IO.Path.Combine (path, (GameData.IO.gLiveGameFolderName + gGameExtension));
					SerializeXMLToFile <PlayerGame> (game, path);
				}

				public static void DeleteGameData (string gameName) 
				{
					string gameDirectoryPath = System.IO.Path.Combine (gCurrentWorldPath, gameName);
					string gamePropsPath = System.IO.Path.Combine (gCurrentWorldPath, gameName + gGameExtension);

					if (Directory.Exists (gameDirectoryPath)) {
						Directory.Delete (gameDirectoryPath, true);
					}
					if (File.Exists (gamePropsPath)) {
						File.Delete (gamePropsPath);
					}
				}

				public static bool CopyGameData (string fromGame, string toGame)
				{
					if (fromGame == toGame) {
						return true;
					}

					string fromPath = System.IO.Path.Combine (gCurrentWorldPath, fromGame);
					string toPath = System.IO.Path.Combine (gCurrentWorldPath, toGame);

					string fromGamePath = System.IO.Path.Combine (gCurrentWorldPath, fromGame + gGameExtension);
					string toGamePath = System.IO.Path.Combine (gCurrentWorldPath, toGame + gGameExtension);

					if (!Directory.Exists (fromPath)) {
						Debug.LogError ("Game data directory " + fromPath + " does not exist!");
						return false;
					}

					try {
						if (Directory.Exists (toPath)) {
							//this SHOULD be an atomic operation...
							Directory.Delete (toPath, true);
						}

						foreach (string dirPath in Directory.GetDirectories (fromPath, "*", SearchOption.AllDirectories)) {
							Directory.CreateDirectory (dirPath.Replace (fromPath, toPath));
						}

						foreach (string newPath in Directory.GetFiles (fromPath, "*.*", SearchOption.AllDirectories)) {
							File.Copy (newPath, newPath.Replace (fromPath, toPath), true);
						}

						if (File.Exists (fromGamePath)) {
							//copy the actual save game too
							if (File.Exists (toGamePath)){
								File.Delete (toGamePath);
							}					
							File.Copy (fromGamePath, toGamePath);
						}
					} catch (Exception e) {
						Debug.LogException (e);
						return false;
					}

					return true;
				}

				#endregion

				public static DateTime GetFileDateTime (string directoryName, string fileName, DataType type)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + gDataExtension));

					if (File.Exists (path)) {
						FileInfo fileInfo = new FileInfo (path);
						return fileInfo.LastWriteTime;
					}
					return new System.DateTime (-1);
				}

				#region folder and file creation

				public static string CleanFileOrFolderName (string fileOrFolderName)
				{
					string replacementChar = "";
					string regexSearch = new string (Path.GetInvalidFileNameChars ()) + new string (Path.GetInvalidPathChars ());
					Regex r = new Regex (string.Format ("[{0}]", Regex.Escape (regexSearch)));
					fileOrFolderName = r.Replace (fileOrFolderName, replacementChar);
					fileOrFolderName = fileOrFolderName.Replace ("Default", "");
					fileOrFolderName = fileOrFolderName.Replace ("_ignore", "");
					fileOrFolderName = fileOrFolderName.Replace ("_trash", "");

					return fileOrFolderName;
				}

				public static string CleanProfileName (string profileName)
				{
					if (profileName == gLiveGameFolderName) {
						return "";
					}

					profileName = CleanFileOrFolderName (profileName);
					//TEMP replace with regex later
					profileName = profileName.Replace (" ", "");
					profileName = profileName.Replace ("_", "");
					profileName = profileName.Replace ("-", "");
					profileName = profileName.Substring (0, Mathf.Min (Globals.MaxProfileNameCharacters, profileName.Length));
					return profileName;
				}

				public static string CreateProfileDirectory (string profileName)
				{
					//we assume that the profile name has been cleaned before attempting this
					string newProfilePath = System.IO.Path.Combine (gGlobalProfilesPath, profileName);
					try {
						Directory.CreateDirectory (newProfilePath);
					} catch (Exception e) {
						//can't remember why i added this - why was i getting exceptions?
						return "Error";
					}
					return profileName;
				}

				public static string CreateGameDirectory (string gameName)
				{
					return IncrementDirectoryName (SanitizeDirectoryName (gameName));
				}

				#endregion

				#region save / load data
				//these are the overloaded functions that MODS uses to save / load mod data
				//data compression isn't implemented - may never be implemented outside of chunk terrain data - but i'm including it just in case
				public static void SaveProfileData <T> (T dataObject, string fileName, DataCompression compression)
				{
					SaveData <T> (dataObject, fileName, DataType.Profile, compression);
				}

				public static void SaveProfileData <T> (T dataObject, string directoryName, string fileName, DataCompression compression)
				{
					SaveData <T> (dataObject, directoryName, fileName, DataType.Profile, compression);
				}

				public static bool LoadProfileData <T> (ref T dataObject, string fileName, DataType type, DataCompression compression)
				{
					return LoadData <T> (ref dataObject, fileName, type, compression);
				}

				public static bool LoadProfileData <T> (ref T dataObject, string directoryName, string fileName, DataCompression compression)
				{
					return LoadData <T> (ref dataObject, directoryName, fileName, DataType.Profile, compression);
				}

				public static void SaveWorldData <T> (T dataObject, string fileName, DataCompression compression)
				{
					SaveData <T> (dataObject, fileName, DataType.World, compression);
				}

				public static void SaveWorldData <T> (T dataObject, string directoryName, string fileName, DataCompression compression)
				{
					SaveData <T> (dataObject, directoryName, fileName, DataType.World, compression);
				}

				public static bool LoadWorldData <T> (ref T dataObject, string fileName, DataCompression compression)
				{
					return LoadData <T> (ref dataObject, fileName, DataType.World, compression);
				}

				public static bool LoadWorldData <T> (ref T dataObject, string directoryName, string fileName, DataCompression compression)
				{
					return LoadData <T> (ref dataObject, directoryName, fileName, DataType.World, compression);
				}

				public static void SaveBaseData <T> (T dataObject, string fileName, DataCompression compression)
				{
					SaveData <T> (dataObject, fileName, DataType.Base, compression);
				}

				public static void SaveBaseData <T> (T dataObject, string directoryName, string fileName, DataCompression compression)
				{
					SaveData <T> (dataObject, directoryName, fileName, DataType.Base, compression);
				}

				public static bool LoadBaseData <T> (ref T dataObject, string fileName, DataCompression compression)
				{
					return LoadData <T> (ref dataObject, fileName, DataType.Base, compression);
				}

				public static bool LoadBaseData <T> (ref T dataObject, string directoryName, string fileName, DataCompression compression)
				{
					return LoadData <T> (ref dataObject, directoryName, fileName, DataType.Base, compression);
				}

				public static void DeleteData (string directoryName, DataType type, DataCompression compression)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					//get a list of all the items in that directory
					if (Directory.Exists (directory)) {
						DirectoryInfo directoryInfo = new DirectoryInfo (directory);
						foreach (FileInfo fileInfo in directoryInfo.GetFiles ( )) {
							string path = System.IO.Path.Combine (directory, fileInfo.Name);
							File.Delete (path);
						}
						foreach (DirectoryInfo subDirInfo in directoryInfo.GetDirectories ()) {
							string path = System.IO.Path.Combine (directory, subDirInfo.Name);
							Directory.Delete (path, true);
						}
					}
				}

				public static void DeleteData (string directoryName, string fileName, DataType type, DataCompression compression)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, fileName + gDataExtension);
					if (File.Exists (path)) {
						File.Delete (path);
					}
				}

				public static void MovePath (string fromPath, string toPath, DataType type, DataCompression compression)
				{
					string dataPath = GetDataPath (type);
					fromPath = System.IO.Path.Combine (dataPath, fromPath);
					toPath = System.IO.Path.Combine (dataPath, toPath);
					System.IO.Directory.Move (fromPath, toPath);
				}

				public static void SaveData <T> (T dataObject, string fileName, DataType type, DataCompression compression)
				{
					string dataPath = GetDataPath (type);
					string path = System.IO.Path.Combine (dataPath, (fileName + gDataExtension));
					if (!Directory.Exists (dataPath)) {
						Directory.CreateDirectory (dataPath);
					}
					SerializeXMLToFile <T> (dataObject, path);
				}

				public static void SaveData <T> (T dataObject, string directoryName, string fileName, DataType type, DataCompression compression)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + gDataExtension));
					if (!Directory.Exists (directory)) {
						Directory.CreateDirectory (directory);
					}
					SerializeXMLToFile <T> (dataObject, path);
				}

				public static bool LoadData <T> (ref T dataObject, string fileName, DataType type, DataCompression compression)
				{
					string dataPath = GetDataPath (type);
					string path = System.IO.Path.Combine (dataPath, (fileName + gDataExtension));
					return DeserializeXMLFromFile <T> (ref dataObject, path);
				}

				public static bool LoadData <T> (ref T dataObject, string directoryName, string fileName, DataType type, DataCompression compression)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + gDataExtension));
					return DeserializeXMLFromFile <T> (ref dataObject, path);
				}

				public static bool SaveBinaryData <T> (T dataObject, string directoryName, string fileName, DataType type)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + gDataExtension));
					FileMode mode = FileMode.CreateNew;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (Directory.Exists (directory) == false) {
						Directory.CreateDirectory (directory);
					}

					if (File.Exists (path)) {
						mode = FileMode.Create;
					}

					try {
						var serializer = new BinaryFormatter ();
						using (var stream = new FileStream (path, mode, access, share)) {
							serializer.Serialize (stream, dataObject);
						}
					} catch (Exception e) {
						Debug.LogWarning ("Couldn't save binary object " + fileName + " because " + e.ToString ());
						return false;
					}

					return true;
				}
				//a detail layer is a compressed array of bytes that stores the density of a single terrain detail layer
				//grass, rocks, etc.
				public static bool LoadDetailLayer (int[,] detailLayer, string directoryName, string fileName, DataType type)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + gDataExtension));
					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (File.Exists (path)) {
						//decompress the bytes
						Byte[] detailLayerAsBytes = CLZF2.Decompress (File.ReadAllBytes (path));
						int x = 0;
						int y = 0;
						int wrap = detailLayer.GetLength (0);
						int totalLength = wrap * wrap;
						for (int i = 0; i < totalLength; i++) {
							detailLayer [x, y] = detailLayerAsBytes [i];//.ReadByte ();
							x++;
							if (x >= wrap) {
								x = 0;
								y++;
							}
						}
						Array.Clear (detailLayerAsBytes, 0, detailLayerAsBytes.Length);
						detailLayerAsBytes = null;
						return true;	
					}
					//#endif
					return false;
				}

				public static void SaveDetailLayer (int[,] detailLayer, string directoryName, string fileName, DataType type)
				{
					int x = 0;
					int y = 0;
					int wrap = detailLayer.GetLength (0);
					int totalLength = wrap * wrap;
					Byte[] detailLayerAsBytes = new byte [totalLength];
					for (int i = 0; i < totalLength; i++) {
						detailLayerAsBytes [i] = (byte)detailLayer [x, y];
						x++;
						if (x >= wrap) {
							x = 0;
							y++;
						}
					}
					//compress the detail layer
					Byte [] detailLayerCompressed = CLZF2.Compress (detailLayerAsBytes);
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + gDataExtension));
					FileMode mode = FileMode.CreateNew;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (File.Exists (path)) {
						File.Delete (path);
					}
					File.WriteAllBytes (path, detailLayerCompressed);
				}

				public static bool LoadBinaryData <T> (ref T dataObject, string directoryName, string fileName, DataType type)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + gDataExtension));
					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (File.Exists (path)) {
						var serializer = new BinaryFormatter ();
						var stream = new FileStream (path, mode, access, share);
						try {
							dataObject = (T)serializer.Deserialize (stream);
							return true;
						} catch (Exception e) {
							Debug.LogError ("Couldn't deserialize " + fileName + ": " + e.ToString ());
						}
					}
					return false;
				}

				#endregion

				#region serialization

				public static bool DeserializeXmlFromArchive <T> (ref T dataObject, string path)
				{
					return false;
				}

				public static bool SerializeXmlToArchive <T> (ref T dataObject, string path)
				{
					return false;
				}

				public static bool DeserializeXMLFromFile <T> (ref T dataObject, string path)
				{
					bool result = false;
					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (File.Exists (path)) {
						try {
							XmlSerializer serializer = new XmlSerializer (typeof(T));
							using (FileStream stream = new FileStream (path, mode, access, share)) {
								dataObject = (T) serializer.Deserialize (stream);
								result = true;
								stream.Close ();
							}
						} catch (Exception e) {
							Debug.Log ("Couldn't load file " + path + " bacuase " + e.InnerException.ToString () + "\n" + e.StackTrace);
						}
					}
					return result;
				}

				public static bool SerializeXMLToFile <T> (T dataObject, string path)
				{
					bool result = false;
					FileMode mode = FileMode.CreateNew;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (File.Exists (path)) {
						//that's right, if it exists before we save, we nuke it
						File.Delete (path);
					}

					try {
						XmlWriterSettings writerSettings = new XmlWriterSettings ();
						writerSettings.NewLineHandling = NewLineHandling.Replace;
						writerSettings.Indent = true;
						writerSettings.NewLineOnAttributes	= false;
						writerSettings.NewLineChars = Environment.NewLine;
						writerSettings.IndentChars = " ";
						//writerSettings.Encoding = Encoding.UTF8; <-WHY does this explode things??

						using (XmlWriter xmlWriter = XmlWriter.Create (path, writerSettings)) {
							var serializer = new XmlSerializer (typeof(T));
							serializer.Serialize (xmlWriter, dataObject);
							xmlWriter.Flush ();
							xmlWriter.Close ();
							result = true;
						}
					} catch (Exception e) {
						Debug.Log ("Couldn't save file " + path + " bacuase " + e.InnerException.ToString () + "\n" + e.StackTrace);
					}
					return result;
				}

				public static bool SaveObjFromMesh (Mesh mesh, Material[] mats, string directoryName, string fileName, DataType type)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + ".obj"));
					FileMode mode = FileMode.CreateNew;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (!Directory.Exists (directory)) {
						Directory.CreateDirectory (directory);
					}
					
					if (File.Exists (path)) {
						mode = FileMode.Create;
					}
					
					ObjExporterScript.MeshToFile (mesh, mats, fileName, path, false);
					return true;
				}

				public static bool LoadObjToGameObjects (ref GameObject[] gameObjects, string directoryName, string fileName, DataType type)
				{
					string dataPath = GetDataPath (type);
					string directory = System.IO.Path.Combine (dataPath, directoryName);
					string path = System.IO.Path.Combine (directory, (fileName + ".obj"));
					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;
	
					if (File.Exists (path)) {
						string objString = File.ReadAllText (path);
						gameObjects = ObjReader.use.ConvertString (objString);
						if (gameObjects == null) {
							return false;
						}
					}
					return true;
				}

				#endregion

				#region load audio
				//i really hate this setup and it doesn't seem to work on some platforms
				public static AudioClip LoadAudio (string clipName)
				{
					string dataPath = GetDataPath (DataType.World);
					string path = System.IO.Path.Combine (dataPath, (clipName + ".mp3"));
					WWW www = new WWW (path);
					return www.GetAudioClip (false, true, AudioType.AIFF);
				}

				#endregion

				#region load terrain data / textures
				//the little texture that shows what world you've selected
				public static bool LoadWorldDescriptionTexture (Texture2D texture, string worldName) {
					string dataPath = System.IO.Path.Combine (gGlobalWorldsPath, worldName);
					string path = System.IO.Path.Combine (dataPath, worldName + gImageExtension);
					return LoadTexture (texture, path);
				}

				public static bool LoadProfileTexture (Texture2D texture, string dataName, string fileName) {
					string dataPath = GetDataPath (DataType.Profile);
					string path = System.IO.Path.Combine (System.IO.Path.Combine (dataPath, dataName), (fileName + gImageExtension));
					return LoadTexture (texture, path);
				}

				public static bool LoadWorldTexture (Texture2D texture, string dataName, string fileName) {
					string dataPath = GetDataPath (DataType.World);
					string path = System.IO.Path.Combine (System.IO.Path.Combine (dataPath, dataName), (fileName + gImageExtension));
					return LoadTexture (texture, path);
				}

				public static bool LoadBaseTexture (Texture2D texture, string dataName, string fileName) {
					string dataPath = GetDataPath (DataType.Base);
					string path = System.IO.Path.Combine (System.IO.Path.Combine (dataPath, dataName), (fileName + gImageExtension));
					return LoadTexture (texture, path);
				}

				public static bool LoadTexture (Texture2D texture, string path)
				{
					if (!File.Exists (path)) {
						return false;
					} else {
						byte[] byteArray = File.ReadAllBytes (path);
						texture.LoadImage (byteArray);
						Array.Clear (byteArray, 0, byteArray.Length);
						byteArray = null;
					}
					return true;
				}

				public static bool LoadCharacterTexture (ref Texture2D texture, string textureName, string textureType, int resolution) {
					string directory = Path.Combine (Path.Combine (gBaseWorldModsPath, "Character"), textureType);
					//textureType is Face, Body, Mask etc., other types may be added later
					string fullPath = System.IO.Path.Combine (directory, textureName + gImageExtension);

					if (gLoadedMaps.TryGetValue (fullPath, out texture)) {
						return true;
					}

					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (!File.Exists (fullPath)) {
						return false;
					} else {
						texture = new Texture2D (resolution, resolution, TextureFormat.RGB24, true, false);
						texture.name = textureName;
						texture.filterMode = FilterMode.Bilinear;
						texture.wrapMode = TextureWrapMode.Clamp;
						texture.anisoLevel = 1;
						byte[] byteArray = File.ReadAllBytes (fullPath);
						texture.LoadImage (byteArray);
						Array.Clear (byteArray, 0, byteArray.Length);
						byteArray = null;
						gLoadedMaps.Add (fullPath, texture);
						return true;
					}
					return false;

					return false;
				}

				public static bool LoadLUT (ref Texture2D lut, string mapName)
				{
					string directory = System.IO.Path.Combine (gBaseWorldModsPath, "CameraLut");
					string fullPath = System.IO.Path.Combine (directory, (mapName + gImageExtension));

					if (gLoadedMaps.TryGetValue (fullPath, out lut)) {
						return true;
					}

					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (!File.Exists (fullPath)) {
						return false;
					} else {
						lut = new Texture2D (1024, 32, TextureFormat.RGB24, false, true);
						lut.name = mapName;
						lut.filterMode = FilterMode.Point;
						lut.wrapMode = TextureWrapMode.Clamp;
						//lut.alphaIsTransparency = false;
						lut.anisoLevel = 0;
						byte[] byteArray = File.ReadAllBytes (fullPath);
						lut.LoadImage (byteArray);
						Array.Clear (byteArray, 0, byteArray.Length);
						byteArray = null;
						gLoadedMaps.Add (fullPath, lut);
						return true;
					}
					return false;
				}

				public static bool LoadTerrainMap (ref Texture2D map, int resolution, TextureFormat format, bool linear, string chunkName, string mapName)
				{
					string directory = System.IO.Path.Combine (gBaseWorldModsPath, "ChunkMap");
					string fullPath = System.IO.Path.Combine (directory, (chunkName + "-" + mapName + gImageExtension));

					//see if we've already loaded this
					if (gLoadedMaps.TryGetValue (fullPath, out map)) {
						return true;
					}

					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (!File.Exists (fullPath)) {
						return false;
					} else {
						map = new Texture2D (resolution, resolution, format, false, linear);
						map.filterMode = FilterMode.Bilinear;
						map.wrapMode = TextureWrapMode.Clamp;
						byte[] byteArray = File.ReadAllBytes (fullPath);
						map.LoadImage (byteArray);
						Array.Clear (byteArray, 0, byteArray.Length);
						byteArray = null;
						gLoadedMaps.Add (fullPath, map);
						return true;
					}
					return false;
				}

				public static bool LoadTerrainHeights (float[,] heights, int resolution, string chunkName, string rawFileName)
				{
					string directory = System.IO.Path.Combine (gBaseWorldModsPath, System.IO.Path.Combine ("Chunk", chunkName));
					string fullPath = System.IO.Path.Combine (directory, (rawFileName + gHeightMapExtension));
					FileMode mode = FileMode.Open;
					FileShare share = FileShare.ReadWrite;
					FileAccess access = FileAccess.ReadWrite;

					if (!File.Exists (fullPath)) {
						return false;
					}

					using (FileStream byteStream = new FileStream (fullPath, mode, access, share)) {
						for (int x = 0; x < resolution; x++) {
							for (int z = 0; z < resolution; z++) {
								heights [x, z] = ((float)((byteStream.ReadByte () << 0x08) | byteStream.ReadByte ())) / 65536f;
							}
						}
						byteStream.Close ();
					}
					return true;
				}

				public static Dictionary <string, Texture2D> gLoadedMaps;// = new Dictionary<string, Texture2D> ();
				public static Dictionary <string, AudioClip> gLoadedAudioClips;// = new Dictionary<string, AudioClip> ();

				#endregion

				private static string SanitizeDirectoryName (string directoryName)
				{
					string invalidChars = System.Text.RegularExpressions.Regex.Escape (new string (System.IO.Path.GetInvalidFileNameChars ()));
					string invalidReStr = string.Format (@"([{0}]*\.+$)|([{0}]+)", invalidChars);
					return System.Text.RegularExpressions.Regex.Replace (directoryName, invalidReStr, "_");
				}

				private static string IncrementDirectoryName (string directoryName)
				{
					string result = directoryName;
					string splitter = " ";
					int currentCount = 1;

					while (System.IO.File.Exists (result)) {
						result = string.Format ("{0} {2}({1})",
							directoryName,
							splitter,
							++currentCount);
					}

					return result;
				}

				public static string GetDataPath (DataType type)
				{
					string dataPath	= string.Empty;
					switch (type) {
					case DataType.Profile:
						dataPath = gCurrentProfileLiveGamePath;
						break;

					case DataType.World:
						dataPath = gCurrentWorldModsPath;
						break;

					case DataType.Base:
					default:
						dataPath = gBaseWorldModsPath;
						break;
					}
					if (string.IsNullOrEmpty (dataPath)) {

					}
					return dataPath;
				}
				//these are set once on startup then never again
				public static string gGlobalProfilesPath = string.Empty;
				public static string gGlobalDataPath = string.Empty;
				public static string gGlobalChangeLogPath = string.Empty;
				public static string gGlobalWorldsPath = string.Empty;
				public static string gBaseWorldPath = string.Empty;
				public static string gBaseWorldModsPath = string.Empty;
				//these change based on the loaded profile / world / game
				public static string gCurrentGamePath = string.Empty;
				public static string gCurrentWorldPath = string.Empty;
				public static string gCurrentWorldModsPath = string.Empty;
				public static string gCurrentProfilePath = string.Empty;
				public static string gCurrentProfileModsPath = string.Empty;
				public static string gCurrentProfileLiveGamePath = string.Empty;
				//these just help us create paths
				public static string gBaseWorldFolderName = "FRONTIERS";
				public static string gFrontiersPrefix = "Frontiers";
				public static string gGlobalWorldFolderName = "Worlds";
				public static string gProfilesFolderName = "Profiles";
				public static string gModsFolderName = "Mods";
				public static string gLiveGameFolderName = "_LiveGame";
				public static string gReferenceExtension = ".mobile";
				public static string gHeightMapExtension = ".raw";
				public static string gImageExtension = ".png";
				public static string gAudioExtension = ".ogv";
				public static string gDataExtension = ".frontiers";
				public static string gProfileExtension = ".player";
				public static string gPreferencesExtension = ".prefs";
				public static string gGameExtension = ".game";
				public static string gAssetBundleExtension = ".unity3d";
			}

			//used in conversations and missions - basically anything where we have variable-driven text
			public static int Evaluate (string evalStatement, Frontiers.Story.Conversations.Conversation source)
			{
				int finalValue = 0;
				//build evaluate statement
				//TODO instead of replacing variables with matches
				//set them to NCalc parameters
				//also maybe re-use NCalc expressions since we're seeing a lot of these over and over
				MatchCollection matches = Regex.Matches (evalStatement, gVarPattern);
				Match match = null;
				for (int i = 0; i < matches.Count; i++) {
					match = matches [i];
					int varValue = GetVariableValue (match.Value, source);
					//this is to prevent it from matching partial variables, eg replacing the first bit of $m_RansomSplit when replacing $m_Ransom
					evalStatement = evalStatement.ReplaceFirst (match.Value, varValue.ToString ());
				}
				NCalc.Expression e = new NCalc.Expression (evalStatement);
				System.Object finalValueObject = e.Evaluate ();
				finalValue = Convert.ToInt32 (finalValueObject);
				return finalValue;
			}

			protected static string gNextValue;
			protected static string gCharacterFirstName;
			protected static string gPlayerFirstName;

			public static string InterpretScripts (string script, PlayerCharacter character, Frontiers.Story.Conversations.Conversation source)
			{
				gCharacterFirstName = "Character";
				gPlayerFirstName = "Player";
				if (Application.isPlaying) {
					if (Player.Local.State.HasSpawned) {
						gPlayerFirstName = Profile.Get.CurrentGame.Character.FirstName;
					}
					if (source != null) {
						gCharacterFirstName = source.SpeakingCharacter.State.Name.FirstName;
					}
				}

				int choiceIndex = 0;
				if (character.Gender == CharacterGender.Female) {
					choiceIndex = 1;
				}

				//now get the tricky stuff
				MatchCollection matches = Regex.Matches (script, gBracketsPattern);
				Match match = null;

				for (int i = 0; i < matches.Count; i++) {
					match = matches [i];
					gNextValue = match.Value.Substring (1, match.Value.Length - 2);//get rid of the brackets
					//okay, regex turns out to be waaaay heavy on the memory/garbage
					//to the point where some operations can cause an out of memory error on really big strings
					//but String.Replace is really ineffcient in other ways
					//look into ways to make this better
					if (gNextValue.StartsWith ("player") || gNextValue.StartsWith ("Player")) {
						if (gNextValue.Contains ("name")) {
							gNextValue = Regex.Replace (gNextValue, "playername", gPlayerFirstName, RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playerfirstname", gPlayerFirstName, RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playerlastname", gPlayerFirstName, RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playerfullname", gPlayerFirstName, RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playernickname", gPlayerFirstName, RegexOptions.IgnoreCase);
						} else {
							gNextValue = Regex.Replace (gNextValue, "playerEyeColor", character.EyeColor.ToString ( ), RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playerSkinColor", character.Ethnicity.ToString ( ), RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playerHairColor", character.HairColor.ToString ( ), RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playerHairLength", character.HairLength.ToString ( ), RegexOptions.IgnoreCase);
							gNextValue = Regex.Replace (gNextValue, "playerAge", character.Age.ToString ( ), RegexOptions.IgnoreCase);
						}
					} else if (gNextValue.StartsWith ("character")) {
						gNextValue = Regex.Replace (gNextValue, "characterfirstname", gCharacterFirstName, RegexOptions.IgnoreCase);
						gNextValue = Regex.Replace (gNextValue, "characterlastname", gCharacterFirstName, RegexOptions.IgnoreCase);
						gNextValue = Regex.Replace (gNextValue, "characterfirstname", gCharacterFirstName, RegexOptions.IgnoreCase);
						gNextValue = Regex.Replace (gNextValue, "characterfullname", gCharacterFirstName, RegexOptions.IgnoreCase);
					} else if (gNextValue.StartsWith ("Eval")) {
						//it's an eval statement
						gNextValue = gNextValue.Replace ("Eval", "");
						int value = Evaluate (gNextValue, source);
						gNextValue = value.ToString ();
					} else if (gNextValue.StartsWith ("$")) {
						//it's a variable statement
						int value = GetVariableValue (gNextValue, source);
						gNextValue = value.ToString ();
					} else if (gNextValue.Contains ("/")) {
						//it's a gender choice
						string[] splitChoice = gNextValue.Split (gGenderSeparators, StringSplitOptions.None);
						gNextValue = splitChoice [choiceIndex];
					} else if (gNextValue.StartsWith ("desc")) {
						//we're being asked to describe an object
						//split up the description request
						string[] splitDescRequest = gNextValue.Split (gSpaceSeparators, StringSplitOptions.None);
						//0 - desc
						//1 - type
						//2 - name

						//TODO this was supposed to work as a generic system - you'd load a MOD which contains a description
						//instead we're stuck loading individual types based on the supplied type string, which is stupid
						//either find a way to make this generic or move it somewhere else
						switch (splitDescRequest [1].ToLower ()) {
						case "book":
							gNextValue = Mods.Get.Description <Frontiers.World.Book> (splitDescRequest [1], splitDescRequest [2].Replace ("_", " "));
							break;

						case "blueprint":
							gNextValue = Mods.Get.Description <WIBlueprint> (splitDescRequest [1], splitDescRequest [2].Replace ("_", " "));
							break;

						case "skill":
							Frontiers.World.Gameplay.Skill skill = null;
							if (Skills.Get.SkillByName (splitDescRequest [2].Replace ("_", " "), out skill)) {
								gNextValue = skill.FullDescription;
							}
							break;

						default:
							Debug.LogWarning ("Can't get description for " + splitDescRequest [1]);
							break;
						}
					}
					script = script.Replace (match.Value, gNextValue);
				}

				//TODO make this not necessary?
				script = script.Replace ("{", "");
				script = script.Replace ("}", "");
				return script;
			}

			public static int GetVariableValue (string variable, Frontiers.Story.Conversations.Conversation source)
			{
				int value = 0;
				gSplitVar = variable.Split (gSeparators, StringSplitOptions.RemoveEmptyEntries);
				gPrefix = gSplitVar [0].Replace ("$", "");
				if (gSplitVar.Length < 3) {
					gVarName = gSplitVar [1];
				} else {
					gObjName = gSplitVar [1];
					gVarName = gSplitVar [2];
				}

				switch (gPrefix) {
				case "g":
				default://global variable
					value = Globals.GetGlobalVariable (gVarName);
					break;

				case "p"://player variable
				case "totalmoney"://wtf is this doing here?
					value = Player.GetPlayerVariable (gVarName);
					break;

				case "m"://mission variable
					Missions.Get.MissionVariable (gObjName, gVarName, ref value);
					break;

				case "c"://conversation variable
					if (source != null) {
						value = source.GetVariableValue (gVarName);
					}
					break;
				}
				return value;
			}

			public static bool CheckVariable (VariableCheckType checkType, int checkValue, int currentValue)
			{	//this bizarre function and its float cousin are used all over the place
				//if i had an embedded scripting language it wouldn't be necessary
				//but whatever it works pretty well
				bool result = false;

				switch (checkType) {
				case VariableCheckType.GreaterThan:
				default:
					result = currentValue > checkValue;
					break;

				case VariableCheckType.GreaterThanOrEqualTo:
					result = currentValue >= checkValue;
					break;

				case VariableCheckType.LessThan:
					result = currentValue < checkValue;
					break;

				case VariableCheckType.LessThanOrEqualTo:
					result = currentValue <= checkValue;
					break;

				case VariableCheckType.EqualTo:
					result = currentValue == checkValue;
					break;
				}

				return result;
			}

			public static bool CheckVariable (VariableCheckType checkType, float checkValue, float currentValue)
			{	
				bool result = false;

				switch (checkType) {
				case VariableCheckType.GreaterThan:
				default:
					result = currentValue > checkValue;
					break;

				case VariableCheckType.GreaterThanOrEqualTo:
					result = currentValue >= checkValue;
					break;

				case VariableCheckType.LessThan:
					result = currentValue < checkValue;
					break;

				case VariableCheckType.LessThanOrEqualTo:
					result = currentValue <= checkValue;
					break;

				case VariableCheckType.EqualTo:
					result = currentValue == checkValue;
					break;
				}

				return result;
			}

			public static string AddSpacesToSentence (string text)
			{
				if (string.IsNullOrEmpty (text))
					return "";
				StringBuilder newText = new StringBuilder(text.Length * 2);
				newText.Append(text[0]);
				for (int i = 1; i < text.Length; i++)
				{
					if (char.IsUpper(text[i]) && text[i - 1] != ' ')
						newText.Append(' ');
					newText.Append(text[i]);
				}
				return newText.ToString();
			}

			public static int WrapIndex (int currentIndex, int count)
			{	//TODO this has been moved into an extension method, get rid of it
				currentIndex++;
				if (currentIndex >= count) {
					currentIndex = 0;
				}
				return currentIndex;
			}

			public static int WrapIndex (int currentIndex, int count, bool forward)
			{	//TODO this has been moved into an extension method, get rid of it
				if (forward) {
					currentIndex++;
					if (currentIndex >= count) {
						currentIndex = 0;
					}
				} else {
					currentIndex--;
					if (currentIndex < 0) {
						currentIndex = count;
					}
				}
				return currentIndex;
			}

			public static void GetTextureFormat (string mapName, ref int resolution, ref TextureFormat format, ref bool linear)
			{	//TODO move actual resolutions into Globals
				resolution = 128;
				format = TextureFormat.RGB24;
				linear = false;

				switch (mapName) {
				case "AboveGroundTerrainType":
					format = TextureFormat.ARGB32;
					linear = true;
					break;

				case "TerrainData":
					format = TextureFormat.ARGB32;
					linear = true;
					resolution = 128;
					break;

				case "Splat1":
				case "Splat2":
					resolution = 1024;
					format = TextureFormat.RGBA32;
					linear = true;
					break;

				case "ColorOverlay":
					resolution = 512;
					break;

				default:
					break;
				}
			}
			//temporary strings
			protected static string [] gSplitVar;
			protected static string gPrefix;
			protected static string gVarName;
			protected static string gObjName;
			//separator arrays
			protected static string[] gGenderSeparators = new string [] { "/" };
			protected static string[] gSeparators = new string[] { "_" };
			protected static string[] gSpaceSeparators = new string [] { " " };
			//reged patterns
			public static string gBracketsPattern = @"{.*?}";
			public static string gVarPattern = @"\$([a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*)";//@"[a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*";
			public static string gMatchPattern = @"\b#\b";
		}

		[Serializable]
		public class MobileReference : IComparable <MobileReference>, IEquatable <MobileReference>, IEqualityComparer <MobileReference>
		{	//this class is a fucking menace, it needs to be eliminated and replaced with paths + static path manipulation functions
			//but i fear it is too entrenched at this point
			public MobileReference (string locationPath)
			{
				FullPath = locationPath;
			}

			public static MobileReference Empty {
				get {
					MobileReference empty = new MobileReference ();
					empty.FileName = string.Empty;
					empty.GroupPath = string.Empty;
					return empty;
				}
			}

			public MobileReference ()
			{

			}

			public MobileReference (string fileName, string groupPath)
			{
				FileName = fileName;
				GroupPath = groupPath;
			}

			public string FileName = string.Empty;
			public string GroupPath = string.Empty;

			public MobileReference AppendLocation (string locationName)
			{
				MobileReference newReference = new MobileReference (locationName, GroupPath + @"\" + FileName);
				return newReference;
			}

			[XmlIgnoreAttribute]
			public string FullPath {
				get {
					if (string.IsNullOrEmpty (mFullPath)) {
						mFullPath = GroupPath + Frontiers.World.WIGroup.gPathJoinString + FileName;
					}
					return mFullPath;
				}
				set {
					mFullPath = value;
					GroupPath = System.IO.Path.GetDirectoryName (mFullPath);
					FileName = System.IO.Path.GetFileName (mFullPath);
				}
			}

			public string ChunkName {
				get {
					if (string.IsNullOrEmpty (mChunkName)) {
						//Root\World\C-0-0-0\WI\etc...
						try {
							mChunkName = GroupPath.Replace (@"Root\World\", "");
							string[] splitChunkName = mChunkName.Split (new string [] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
							mChunkName = splitChunkName [0];
						}
						catch (Exception e) {
							Debug.LogException (e);
						}
					}
					return mChunkName;
				}
			}

			public int ChunkID {
				get {
					if (mChunkID < 0) {
						string[] splitChunkName = ChunkName.Split (new String [] { "-" }, StringSplitOptions.RemoveEmptyEntries);
						//C-0-0-ID#
						//0 1 2 3
						mChunkID = Int32.Parse (splitChunkName [3]);
					}
					return mChunkID;
				}
			}

			public static bool IsNullOrEmpty (MobileReference mr)
			{
				return mr == null || (string.IsNullOrEmpty (mr.FileName) || string.IsNullOrEmpty (mr.GroupPath));
			}

			protected string mChunkName = string.Empty;
			protected string mFullPath = string.Empty;
			protected int mChunkID = -1;

			#region icomparable
			public override bool Equals (object obj)
			{
				if (obj == null) {
					return false;
				}

				MobileReference other = (MobileReference) obj;

				if (other == null) {
					return false;
				}

				return ((this == other) || string.Equals (FullPath, other.FullPath));
			}

			public bool Equals (MobileReference other)
			{
				if (other == null) {
					return false;
				}

				return ((this == other) || string.Equals (FullPath, other.FullPath));
			}

			public int CompareTo (MobileReference other)
			{
				return FullPath.CompareTo (other.FullPath);
			}

			public bool Equals (MobileReference x, MobileReference y) {
				if (x != null) {
					if (y == null) {
						return false;
					}
					return x.Equals (y);
				} else {
					if (y != null) {
						return false;
					}
				}
				//both null
				return true;
			}

			public int GetHashCode (MobileReference mr) {
				if (mr == null) {
					return 0;
				}
				return mr.GetHashCode ();
			}

			public override int GetHashCode ()
			{
				return FullPath.GetHashCode ();
			}
			#endregion
		}

		public class ObjExporterScript
		{
			public static string MeshToString (Mesh m, Material[] mats, string name)
			{
				StringBuilder sb = new StringBuilder ();
				sb.Append ("#Exported from FRONTIERS\n\n");
				sb.Append ("o ").Append (name).Append ("\n\n");
				sb.Append ("g default\n");
				foreach (Vector3 v in m.vertices) {
					sb.Append (string.Format ("v {0} {1} {2}\n", v.x, v.y, v.z));
				}
				sb.Append ("\n");
				foreach (Vector3 v in m.normals) {
					sb.Append (string.Format ("vn {0} {1} {2}\n", v.x, v.y, v.z));
				}
				sb.Append ("\n");
				foreach (Vector2 v in m.uv) {
					sb.Append (string.Format ("vt {0} {1}\n", v.x, v.y));
				}
				sb.Append ("\n");
				foreach (Vector2 v in m.uv1) {
					sb.Append (string.Format ("vt1 {0} {1}\n", v.x, v.y));
				}
				sb.Append ("\n");
				foreach (Vector2 v in m.uv2) {
					sb.Append (string.Format ("vt2 {0} {1}\n", v.x, v.y));
				}
				sb.Append ("\n");
				foreach (Color c in m.colors) {
					sb.Append (string.Format ("vc {0} {1} {2} {3}\n", c.r, c.g, c.b, c.a));
				}
				for (int material = 0; material < m.subMeshCount; material++) {
					sb.Append ("\n");
					sb.Append ("usemtl ").Append (mats [material].name).Append ("\n");
					sb.Append ("usemap ").Append (mats [material].name).Append ("\n");

					int[] triangles = m.GetTriangles (material);
					for (int i = 0; i < triangles.Length; i += 3) {
						sb.Append (string.Format ("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
							triangles [i] + 1, triangles [i + 1] + 1, triangles [i + 2] + 1));
					}
				}
				return sb.ToString ();
			}

			public static void MeshToFile (Mesh mesh, Material[] mats, string name, string path, bool append)
			{
				try {
					using (StreamWriter sw = new StreamWriter (path, append)) {
						sw.WriteLine (MeshToString (mesh, mats, name));
					}
				} catch (System.Exception e) {
					Debug.Log ("Couldn't save object to disk: " + e.ToString ());
				}
			}
		}
	}

	public enum VariableCheckType
	{
		LessThanOrEqualTo,
		LessThan,
		GreaterThanOrEqualTo,
		GreaterThan,
		EqualTo,
	}

	public enum DataCompression
	{
		None,
		GZip,
	}

	public enum DataType
	{
		Base,
		World,
		Profile
	}
}