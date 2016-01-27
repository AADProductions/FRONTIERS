using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System.Globalization;
using Frontiers.Story;
using System.Text.RegularExpressions;
using Frontiers.Story.Conversations;
using Frontiers.World.WIScripts;

namespace Frontiers.Data
{
	//imports books, speeches, conversations and missions from their 'raw' external form
	//convers them into xml
	//then saves them in their 'proper' form in the mods directories
	//this whole class is an ungodly mess
	//i mean seriously just look at it - it was cobbled together rapidly and has grown like cancer
	//thankfully it's only used for convenience and never actually touches anything in-game
	[ExecuteInEditMode]
	public class DataImporter : MonoBehaviour
	{
		public Dictionary <string,string> RecipeImportSubstitutions = new Dictionary<string, string> ();
		public string ImportDirectoryName = "Import";
		public string ImportDirectory;
		public List <string> ImportDataTypes = new List <string> () {
						"Book",
						"Character",
						"Conversation",
						"Mission",
						"Speech",
						"Plant",
						"Blueprint",
						"Sigil",
						"Headstone",
						"Library",
				};
		public List <string> ImportDataNames = new List <string> ();
		public List <KeyValuePair <string,string>> Assets = new List<KeyValuePair<string, string>> ();
		public List <string> Massets = new List <string> ();
		public string CurrentDataType;
		public KeyValuePair <string,string> CurrentAsset;

		public void WordCounter ()
		{
			int convWordCount = 0;
			List <string> conversations = Mods.Get.Available ("Conversation");
			for (int i = 0; i < conversations.Count; i++) {
				ConversationProps conv = null;
				if (Mods.Get.Runtime.LoadMod <ConversationProps> (ref conv, "Conversation", conversations [i])) {
					convWordCount += conv.GetWordCount ();
				}
			}
			DebugConsole.Get.Log.Add ("#" + conversations.Count.ToString () + " conversations, " + convWordCount.ToString () + " words.");
		}

		public void RefreshSubstitutions ()
		{
			RecipeImportSubstitutions.Clear ();
			/*
			Ale
			Apple, green
			Apple, red
			Avocado
			Baking Soda (u)
			Banana
			Beans, dried
			Beef, cooked
			Beef, dried
			Beef, raw
			Beef, salted
			Berries
			Bread, Sourdough
			Bread, Wheat
			Butter
			Cabbage
			Carrots
			Cheese
			Chicken, cooked
			Chicken, dried
			Chicken, raw
			Chocolate (r)
			Cider
			Cinnamon (u)
			Cocoa beans (r)
			Coconut (u)
			Coffee (r)
			Coffee Beans, raw (r)
			Coffee Beans, roasted (r)
			Corn
			Cream
			Eggs
			Fish, cooked
			Fish, dried
			Fish, raw
			Fish, salted
			Flour
			Fruit Juice
			Garlic
			Ginger
			Grapefruit
			Grapes
			Hardtack
			Honey
			Ice cream
			Lemon
			Lettuce
			Lime
			Mead
			Melon
			Milk
			Molasses
			Mushrooms, Ink Cap
			Mushrooms, King Bolete
			Mushrooms, Shaggy Mane
			Oatmeal
			Oats
			Oil
			Onions
			Pancakes
			Parsnips
			Pasta
			Peach
			Pear
			Peas
			Peppercorn
			Peppers
			Pineapple
			Pork, cooked
			Pork, dried
			Pork, raw
			Pork, salted
			Potatoes
			Pumpkin
			Rennet
			Rice
			Salt
			Shellfish, cooked
			Shellfish, dried
			Shellfish, raw
			Simple Cheese
			Sourdough Starter
			Spices
			Sugar
			Sugar beets
			Tea
			Tea Leaves
			Vanilla (r)
			Vinegar
			Water
			Wild Greens
			Wine
			Yeast

			Chicken, cooked
			Chicken, dried
			Chicken, raw
			 */
			RecipeImportSubstitutions.Add ("Chicken, cooked", "Chicken Leg, cooked");
			RecipeImportSubstitutions.Add ("Chicken, dried", "Chicken Leg, dried");
			RecipeImportSubstitutions.Add ("Chicken, raw", "Chicken Leg, raw");
			RecipeImportSubstitutions.Add ("Apple, green", "Green Apple");
			RecipeImportSubstitutions.Add ("Apple, red", "Red Apple");
			RecipeImportSubstitutions.Add ("Beef, cooked", "Beef 1, cooked");
			RecipeImportSubstitutions.Add ("Beef, dried", "Beef 1, dried");
			RecipeImportSubstitutions.Add ("Beef, raw", "Beef 1, raw");
			RecipeImportSubstitutions.Add ("Beef, salted", "Beef 1, salted");
			RecipeImportSubstitutions.Add ("Bread, Sourdough", "Sourdough");
			RecipeImportSubstitutions.Add ("Bread, Wheat", "Wheat Bread");
			RecipeImportSubstitutions.Add ("Berries", "Berries 1");
			RecipeImportSubstitutions.Add ("Mushrooms, Ink Cap", "Ink Cap Mushroom");
			RecipeImportSubstitutions.Add ("Mushrooms, King Bolete", "King Bolete Mushroom");
			RecipeImportSubstitutions.Add ("Mushrooms, Shaggy Mane", "Shaggy Mane Mushroom");
			RecipeImportSubstitutions.Add ("Carrots", "Carrot");
			RecipeImportSubstitutions.Add ("Cocoa beans", "Cocoa Beans");
			RecipeImportSubstitutions.Add ("Peppers", "Red Bell Pepper");
			RecipeImportSubstitutions.Add ("Parsnips", "Parsnip");
			RecipeImportSubstitutions.Add ("Potatoes", "Potato");
			RecipeImportSubstitutions.Add ("Onions", "Onion");
			RecipeImportSubstitutions.Add ("Eggs", "Egg 1");
			RecipeImportSubstitutions.Add ("Sugar beets", "Sugar Beet");
			RecipeImportSubstitutions.Add ("Water", "Water");
			RecipeImportSubstitutions.Add ("Spices", "Spices 1");
			RecipeImportSubstitutions.Add ("Sugar", "Sugar 1");
			RecipeImportSubstitutions.Add ("Pear", "Green Pear");
			RecipeImportSubstitutions.Add ("Rice", "Rice 1");
			RecipeImportSubstitutions.Add ("Oil", "Oil 1");
		}

		public void WriteMassImportAsset (string dataType, string fileName, string fileContents)
		{
			ImportDirectory = System.IO.Path.Combine (GameData.IO.gGlobalDataPath, ImportDirectoryName);
			string dataTypeDirectory = System.IO.Path.Combine (ImportDirectory, dataType);
			string finalPath = System.IO.Path.Combine (dataTypeDirectory, fileName);
			Debug.Log ("Writing masset " + fileName);
			File.WriteAllText (finalPath, fileContents);
		}

		public void ImportAssets ()
		{
			//this script assumes MODS and DATA are awake and initialized
			ImportDirectory = System.IO.Path.Combine (GameData.IO.gGlobalDataPath, ImportDirectoryName);

			//check each data type for assets
			foreach (string dataType in ImportDataTypes) {
				Debug.Log ("Importing data type " + dataType);
				string dataTypeDirectory = System.IO.Path.Combine (ImportDirectory, dataType);
				if (Directory.Exists (dataTypeDirectory)) {
					CurrentDataType = dataType;
					//load all assets
					List <string> filesInDirectory = new List <string> ();
					if (Directory.Exists (ImportDirectory)) {
						Debug.Log ("Directory " + ImportDirectory + " exists");
						foreach (string newPath in Directory.GetFiles (dataTypeDirectory, "*.*", SearchOption.AllDirectories)) {
							FileInfo file = new FileInfo (newPath);
							if (ImportDataNames.Count > 0) {
								//check against the names
								bool added = false;
								foreach (string dataName in ImportDataNames) {
									if (file.Name.ToLower ().Contains (dataName.ToLower ())) {
										Debug.Log ("File name " + file.Name + " contains " + dataName + ", adding");
										filesInDirectory.Add (newPath);
										added = true;
										break;
									}
								}
								if (!added) {
									Debug.Log ("Skipping " + file.Name);
								}
							} else {
								//otherwise add them all
								Debug.Log ("Adding file in directory " + file.Name);
								filesInDirectory.Add (newPath);
							}
						}
					}

					Debug.Log ("Found " + filesInDirectory.Count + " files in directory " + dataTypeDirectory);

					foreach (string fileInDirectory in filesInDirectory) {
						string finalPath = fileInDirectory;//System.IO.Path.Combine (dataTypeDirectory, fileInDirectory);
						if (File.Exists (finalPath)) {
							Debug.Log ("Loading " + fileInDirectory);
							string fileName = fileInDirectory.Replace (".txt", "");
							//see if this is a mass import or a regular import
							//mass import will have the extension csv
							if (System.IO.Path.GetExtension (finalPath).Contains ("csv")) {
								////Debug.Log ("Loading " + fileInDirectory + " as mass import");
								Massets.Add (finalPath);
							} else {
								// ("Loading " + fileInDirectory + " as a regular asset");
								string fileData = File.ReadAllText (finalPath);
								fileName = fileName.Replace (".xml", "");
								Assets.Add (new KeyValuePair <string, string> (System.IO.Path.GetFileNameWithoutExtension (finalPath), fileData));
							}
						} else {
							Debug.Log (finalPath + " didn't exist");
						}
					}

					foreach (string massetPath in Massets) {
						ImportMasset (dataType, massetPath);
					}

					foreach (KeyValuePair <string,string> asset in Assets) {
						CurrentAsset = asset;
						ImportAsset (dataType, asset);
					}
				}
			}
			//destroy this object once we're done
			if (Application.isPlaying) {
				GameObject.Destroy (gameObject, 0.1f);
			} else {
				GameObject.DestroyImmediate (gameObject);
			}
		}

		protected void ImportMasset (string dataType, string path)
		{
			switch (dataType) {
			case "Library":
				MassImportLibrary (path);
				break;

			case "Worlditem":
				MassImportWorldItem (path);
				break;

			case "Book":
				MassImportBook (path);
				break;

			case "Plant":
				MassImportPlant (path);
				break;

			case "Blueprint":
				MassImportBlueprint (path);
				break;

			case "Character":
				MassImportCharacter (path);
				break;

			case "Sigil":
				MassImportSigil (path);
				break;

			case "Headstone":
				MassImportHeadstone (path);
				break;

//			case "Conversation":
//				ImportConversation (masset);
//				break;
//
//			case "Mission":
//				ImportMission (masset);
//				break;
//
//			case "Speech":
//				ImportSpeech (masset);
//				break;

			default:
				Debug.Log ("Unknown data type " + dataType + ", not importing asset.");
				return;
			}
		}

		protected void ImportAsset (string dataType, KeyValuePair <string,string> asset)
		{
			switch (dataType) {
			case "Book":
				ImportBook (asset);
				break;

			case "Conversation":
				ImportConversation (asset);
				break;

			case "Mission":
				ImportMission (asset);
				break;

			case "Speech":
				ImportSpeech (asset);
				break;

			case "Blueprint":
				ImportBlueprint (asset);
				break;

			case "Character":
				ImportCharacter (asset);
				break;

			default:
				////Debug.Log ("Unknown data type " + dataType + ", not importing asset.");
				return;
			}
		}

				#region headstone import

		protected void MassImportHeadstone (string path)
		{
			var MyFile = new CSVFile (path);

			/*			
			0 - Service Guid
			1 - Platform
			2 - Id
			3 - Reward
			4 - Name
			5 - Email Sv
			6 - Address Name
			7 - Address Line 1
			8 - Address Line 2
			9 - Address City
			10 - Address State
			11 - Address Country
			12 - Address Postal Code
			13 - Address Phone Number
			14 - Pledge Amount
			15 - Pledge Status
			16 - Notes
			17 - Public Notes
			18 - Pledged At
			19 - Survey Answered At
			20 - Full Address
			21 - Full Country
			22 - Reward Price
			23 - Reward Description
			24 - Order Placed
			25 - Order Id
			26 - Order
			27 - Order Status
			28 - Order Charged
			29 - Charge Token
			30 - Funds Added In Backer Kit
			31 - Total Spent
			32 - Follow Up: Headstone Text
			33 - Follow Up: Headstone Epitaph   This Text Will Be Seen When A Character Clicks On & Examines A Headstone.  It Could Be A Phrase, A Poem, A Cause Of Death, Etc
			34 - Follow Up: Headstone   Special Instructions
			*/
//			int fieldNum = 0;
//			List <string> fields = new List<string> ();
//			foreach (string field in MyFile.Rows [0].Fields) {
//				fields.Add (fieldNum.ToString ( ) + " - " + field);
//				fieldNum++;
//			}
//			Debug.Log (fields.JoinToString ("\n"));

			for (int rowNum = 1; rowNum < MyFile.Rows.Count; rowNum++) {
				Headstone headstone = new Headstone ();
				headstone.Name = "Headstone" + (rowNum + 1).ToString ();
				//get the data for each one of the book fields
				var row = MyFile.Rows [rowNum];
				string headstoneText = row.Fields [32];
				headstoneText.Replace ("<br>", "\n");
				string[] splitHeadstoneText = headstoneText.Split (new string [] {
					"\n",
					"\n\r"
				}, StringSplitOptions.RemoveEmptyEntries);

				string epitaphText = row.Fields [33];
				epitaphText.Replace ("<br>", "\n");

				if (epitaphText == "NA" || epitaphText == "\"NA\"") {
					epitaphText = string.Empty;
				}
				headstone.Epitaph = epitaphText;

				if (splitHeadstoneText.Length > 0) {
					headstone.Line1 = splitHeadstoneText [0];
				}
				if (splitHeadstoneText.Length > 1) {
					headstone.Line2 = splitHeadstoneText [1];
				}
				if (splitHeadstoneText.Length > 2) {
					headstone.Line3 = splitHeadstoneText [2];
				}
				if (splitHeadstoneText.Length > 3) {
					headstone.Line4 = splitHeadstoneText [3];
				}

				//todo headstone type

				Mods.Get.Editor.SaveMod <Headstone> (headstone, "Headstone", headstone.Name);
			}
		}

				#endregion

				#region sigil import

		protected void MassImportSigil (string path)
		{
//			var MyFile = new CSVFile (path);

			/*
			 0 - "Submission Date"
			 1 - "Number of Symbols"
			 2 - "Trim Color"
			 3 - "Background Color"
			 4 - "Symbol 1"
			 5 - "Symbol 1 Color"
			 6 - "Symbol 2"
			 7 - "Symbol 2 Color"
			 8 - "Symbol 3"
			 9 - "Symbol 3 Color"
			 10 - "Symbol 4"
			 11 - "Symbol 4 Color"
			 12 - "Your Email Address"
			*/
//
//			for (int rowNum = 1; rowNum < MyFile.Rows.Count; rowNum++) {
//				Sigil sigil = new Sigil ();
//				//get the data for each one of the book fields
//				var row = MyFile.Rows [rowNum];
//
//				sigil.Name = "Sigil_" + rowNum.ToString ();
//				sigil.Type = "Sigil";
//				if (row.Fields [2].Contains ("Silver")) {
//					sigil.Royalty = false;
//				} else {
//					sigil.Royalty = true;
//				}
//				sigil.BannerColorIndex = Int32.Parse (row.Fields [3].Remove ("Background "));
//				//get the index of the banner icon names
//				//
//
//				sigil.BannerItemIconIndex = new int [4];
//				sigil.BannerItemIconIndex [0] = Colors.BannerColors.GetIconIndex (row.Fields [4]);
//				sigil.BannerItemIconIndex [1] = Colors.BannerColors.GetIconIndex (row.Fields [6]);
//				sigil.BannerItemIconIndex [2] = Colors.BannerColors.GetIconIndex (row.Fields [8]);
//				sigil.BannerItemIconIndex [3] = Colors.BannerColors.GetIconIndex (row.Fields [10]);
//
//				sigil.BannerItemColorIndex = new int [4];
//				sigil.BannerColorIndex [0] = Int32.Parse (row.Fields [5].Trim ().Remove ("Color "));
//				sigil.BannerColorIndex [1] = Int32.Parse (row.Fields [7].Trim ().Remove ("Color "));
//				sigil.BannerColorIndex [2] = Int32.Parse (row.Fields [9].Trim ().Remove ("Color "));
//				sigil.BannerColorIndex [3] =Int32.Parse (row.Fields [11].Trim ().Remove ("Color "));
//
//				if (row.Fields [1].Contains ("2")) {
//
//				}
//			}
		}

				#endregion

				#region character import

		protected void ImportCharacter (KeyValuePair <string,string> mission)
		{

		}

		protected void MassImportCharacter (string path)
		{
			var MyFile = new CSVFile (path);

			/*
			0 - Handle
			1 - Template Type
			2 - Generic Identifier
			3 - Prefix
			4 - First Name
			5 - Middle Name
			6 - Last Name
			7 - Nickname
			8 - Postfix
			9 - Knows Player
			10 - Reputation Self
			11 - Reputation Player
			12 - Body Names
			13 - Body Texture Name
			14 - Gender
			15 - Age
			16 - Conversation Name
			17 - Default to DTS
			18 - DTS Name
			*/

			for (int rowNum = 1; rowNum < MyFile.Rows.Count; rowNum++) {
				CharacterTemplate charTemp = new CharacterTemplate ();
				//get the data for each one of the book fields
				var row = MyFile.Rows [rowNum];

				charTemp.Name = row.Fields [0];
				charTemp.Type = "Character";
				charTemp.TemplateType = (CharacterTemplateType)Enum.Parse (typeof(CharacterTemplateType), row.Fields [1]);
				charTemp.StateTemplate.Name.FileName = charTemp.Name;
				charTemp.StateTemplate.Name.GenericIdentifier = row.Fields [2];
				charTemp.StateTemplate.Name.Prefix = row.Fields [3];
				charTemp.StateTemplate.Name.FirstName = row.Fields [4];
				charTemp.StateTemplate.Name.MiddleName = row.Fields [5];
				charTemp.StateTemplate.Name.LastName = row.Fields [6];
				charTemp.StateTemplate.Name.NickName = row.Fields [7];
				charTemp.StateTemplate.Name.PostFix = row.Fields [8];
				charTemp.StateTemplate.KnowsPlayer = bool.Parse (row.Fields [9].ToLower ());
				charTemp.StateTemplate.GlobalReputation = Int32.Parse (row.Fields [10]);
				string[] bodyNames = row.Fields [12].Split (new String [] { " " }, StringSplitOptions.RemoveEmptyEntries);
				string defaultBodyName = bodyNames [0];
				charTemp.StateTemplate.BodyName = defaultBodyName;
				charTemp.StateTemplate.BodyTextureName = row.Fields [13];
				switch (row.Fields [14].ToLower ().Trim ()) {
				case "male":
					charTemp.StateTemplate.Flags.Gender = 1;
					break;

				case "female":
					charTemp.StateTemplate.Flags.Gender = 2;
					break;

				default:
					charTemp.StateTemplate.Flags.Gender = 3;
					break;
				}
				charTemp.StateTemplate.AgeInYears = Int32.Parse (row.Fields [15]);
				charTemp.TalkativeTemplate.ConversationName = GetNameFromDialogName (row.Fields [16]);
				charTemp.TalkativeTemplate.DefaultToDTS = bool.Parse (row.Fields [17]);
				charTemp.TalkativeTemplate.DTSSpeechName = GetNameFromDialogName (row.Fields [18]);

				Mods.Get.Editor.SaveMod <CharacterTemplate> (charTemp, "Character", charTemp.Name);
			}
		}

				#endregion

				#region mission import

		protected void ImportMission (KeyValuePair <string,string> mission)
		{
			objectives.Clear ();
			currentMissionState = new MissionState ();
			currentMissionState.Name = CurrentAsset.Key.Replace (".txt", "").Replace (".xml", "");

			string missionString = CurrentAsset.Value.Replace ("\t", "");

			string[] splitMission = missionString.Split (new string [] { "[StartObjectives]" }, StringSplitOptions.RemoveEmptyEntries);
			string[] splitMissionVars = splitMission [0].Split (new string [] {
								"\n",
								"\n\r",
								"\r\n"
						}, StringSplitOptions.RemoveEmptyEntries);
			string[] splitObjectives = splitMission [1].Split (new string [] { "[Objective]" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string missionVarPart in splitMissionVars) {
				string missionVar = missionVarPart.Trim ();//get rid of tabs and spaces
				if (missionVar.StartsWith ("//")) {
					//do nothing, it's a comment
				} else if (missionVar.StartsWith ("#")) {
					//#Variable Name=VariableName DefaultValue=0
					////Debug.Log ("setting mission variable var " + missionVar);
					string missionVarVar = missionVar.Replace ("#Variable", "");
					string missionVarName = string.Empty;
					int missionDefaultVal = 0;
					string[] splitVarVar = missionVarVar.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string varVar in splitVarVar) {
						////Debug.Log ("varVar " + varVar);
						//this is getting rediculous...
						string[] varVarVar = varVar.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
						string varName = varVarVar [0].Trim ();
						string varVal = varVarVar [1].Trim ();
						switch (varName) {
						case "Name":
							missionVarName = varVal;
							break;

						case "DefaultValue":
							Int32.TryParse (varVal, out missionDefaultVal);
							break;

						default:
							break;
						}
					}
					if (!string.IsNullOrEmpty (missionVarName) && !currentMissionState.Variables.ContainsKey (missionVarName)) {
						////Debug.Log ("Adding mission variable " + missionVarName);
						currentMissionState.Variables.Add (missionVarName, missionDefaultVal);
					}
				} else {
					string[] splitMissionVar = missionVar.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					try {
						string varName = splitMissionVar [0];
						string varVal = splitMissionVar [1];
						SetScriptVar (currentMissionState, typeof(MissionState), varName, varVal);
					} catch (Exception e) {
						Debug.Log ("Had trouble with var " + missionVar + e.ToString ());
					}
				}
			}

			foreach (string splitObjective in splitObjectives) {
				string parentName = string.Empty;
				ObjectiveState state = GetObjectiveState (splitObjective, ref parentName);
				if (state != null) {
					Debug.Log ("Adding " + state.FileName + " to objectives under " + parentName);
					objectives.Add (state.FileName, new KeyValuePair <string, ObjectiveState> (parentName, state));
					currentMissionState.Objectives.Add (state);
				}
			}

			Debug.Log ("Linking up objectives");
			foreach (KeyValuePair <string, KeyValuePair <string, ObjectiveState>> statePair in objectives) {
				string objectiveName = statePair.Key;
				string parentName = statePair.Value.Key;
				ObjectiveState state = statePair.Value.Value;
				ObjectiveState parentState = null;
				////Debug.Log ("Trying to parent " + objectiveName + " under " + parentName);

				if (parentName.Trim ().ToLower () == "mission") {
					Debug.Log ("Paraneting " + objectiveName + " under mission");
					currentMissionState.FirstObjectiveNames.SafeAdd (state.FileName);
				} else {
					Debug.Log ("Looking for " + parentName);
					KeyValuePair <string, ObjectiveState> parentStatePair;
					if (objectives.TryGetValue (parentName, out parentStatePair)) {
						parentState = parentStatePair.Value;
						Debug.Log ("Parenting " + objectiveName + " under " + parentName);
						parentState.NextObjectiveNames.SafeAdd (state.FileName);
					}
				}
			}

			Mods.Get.Editor.SaveMod <MissionState> (currentMissionState, "Mission", currentMissionState.Name);
		}

		protected ObjectiveState GetObjectiveState (string objectiveString, ref string parentName)
		{
			int numVarsSet = 0;
			ObjectiveState objectiveState = new ObjectiveState ();
			string[] splitObjVars = objectiveString.Split (new string [] {
								"\n",
								"\n\r",
								"\r\n"
						}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string objVar in splitObjVars) {
				try {
					if (objVar.Contains ("#ObjectiveScript")) {
						objectiveState.Scripts.Add (GetObjectiveScript (objVar));
					} else {
						string[] splitObjVar = objVar.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
						string varName = splitObjVar [0].Trim ();
						string varVal = splitObjVar [1].Trim ();
						if (varName == "Parent") {
							//check for setting the parent
							parentName = varVal;
						} else {
							numVarsSet++;
							SetScriptVar (objectiveState, typeof(ObjectiveState), varName, varVal);
						}
					}
				} catch (Exception e) {
					Debug.Log ("Had trouble with " + objVar + e.ToString ());
					objectiveState = null;
				}
			}

			if (numVarsSet == 0) {
				Debug.Log ("NO vars set in objective, must've been a dud");
				objectiveState = null;
			}
			return objectiveState;
		}

		protected ObjectiveScript GetObjectiveScript (string objectiveScriptString)
		{
			ObjectiveScript objectiveScript = null;
			objectiveScriptString = objectiveScriptString.Replace ("#ObjectiveScript|", "");

			try {
				//first, intercept and protect any eval statements (anything in parenthesis)
				MatchCollection matches = Regex.Matches (objectiveScriptString, gParenthesisPattern);
				Match match = null;
				for (int i = 0; i < matches.Count; i++) {
					match = matches [i];
					Debug.Log ("Found match " + match.Value);
					//get rid of spaces
					objectiveScriptString = objectiveScriptString.Replace (match.Value, match.Value.Replace (" ", ""));
				}

				string[] splitObjScriptString = objectiveScriptString.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
				string scriptType = splitObjScriptString [0];
				////Debug.Log ("Script type is " + scriptType);
				Type objScriptType = Type.GetType ("Frontiers.World.Gameplay." + scriptType);
				objectiveScript = (ObjectiveScript)Activator.CreateInstance (objScriptType);
				for (int i = 1; i < splitObjScriptString.Length; i++) {
					string objVar = splitObjScriptString [i];
					try {
						string[] splitObjVar = objVar.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
						string varName = splitObjVar [0].Trim ();
						string varVal = splitObjVar [1].Trim ();
						SetScriptVar (objectiveScript as System.Object, objScriptType, varName, varVal);
					} catch (Exception e) {
						////Debug.Log ("Had trouble with obj var " + objVar + e.ToString ());
					}
				}
			} catch (Exception e) {
				////Debug.Log ("Had trouble with objective script string " + objectiveScriptString + ": " + e.ToString ());
			}

			return objectiveScript;
		}

		public Mission missionObject = null;
		public MissionState currentMissionState = null;
		public Dictionary <string, KeyValuePair <string, ObjectiveState>> objectives = new Dictionary <string, KeyValuePair <string, ObjectiveState>> ();

				#endregion

				#region book import

		protected void MassImportLibrary (string path)
		{
			var MyFile = new CSVFile (path);
			/*
			0: Book Title
			1: Book Filename
			2: Library
			3: Default Template
			4: Price Multiplier
			5: Delivery Time
			6: Associated Skills
			7: Associated Blueprints
			*/

			Library library = new Library ();
			if (!Mods.Get.Editor.LoadMod <Library> (ref library, "Library", "GuildLibrary")) {
				library.Name = "GuildLibrary";
				library.RequiredSkill = "GuildLibrary";
				library.DisplayName = "Guild Library";
				library.Motto = "Knowledge is power";
			}
			library.CatalogueEntries.Clear ();

//			int index = 0;
//			List <string> fields = new List<string> ();
//			foreach (string field in MyFile.Rows [0].Fields) {
//				fields.Add (index.ToString ( ) + ": " + field);
//				index++;
//			}
//			Debug.Log (fields.JoinToString ("\n"));

			for (int rowNum = 1; rowNum < MyFile.Rows.Count; rowNum++) {
				//get the data for each one of the book fields
				List <string> finalBookLines = new List <string> ();

				LibraryCatalogueEntry entry = new LibraryCatalogueEntry ();

				var row = MyFile.Rows [rowNum];
				string bookTitle = row.Fields [0];
				string bookFileName = row.Fields [1];
				string libraryName = row.Fields [2];
				string defaultTemplate = row.Fields [3].Replace (" ", "_");
				float priceMultiplier = float.Parse (row.Fields [4]);
				float deliveryTime = float.Parse (row.Fields [5]);
				string associatedBlueprints = string.Empty;
				List <string> associatedBlueprintsList = new List <string> ();
				string associatedSkills = row.Fields [6];
				if (!string.IsNullOrEmpty (row.Fields [7].Trim ())) {
					string[] splitBlueprints = row.Fields [7].Split (new String [] { "," }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string associatedBlueprint in splitBlueprints) {
						string finalBlueprint = associatedBlueprint.Trim ().Replace (" ", "_");
						associatedBlueprintsList.Add (finalBlueprint);
					}
					associatedBlueprints = associatedBlueprintsList.JoinToString (",");
				}

				finalBookLines.Add ("#set Type=Book");
				finalBookLines.Add ("#set Title=" + bookTitle);
				finalBookLines.Add ("#set ContentsSummary=" + bookTitle);
				finalBookLines.Add ("#set Status=Dormant");
				finalBookLines.Add ("#set SkillsToLearn=" + associatedSkills);
				if (!string.IsNullOrEmpty (associatedBlueprints)) {
					finalBookLines.Add ("#set BlueprintsToReveal=" + associatedBlueprints);
				}
				finalBookLines.Add ("#set DefaultTemplate=" + defaultTemplate);
				finalBookLines.Add ("[bookstart]");
				finalBookLines.Add ("[chapterbreak]");
				finalBookLines.Add ("#set HorizontalAlignment=Left");
				finalBookLines.Add ("#set VerticalAlignment=Top");
				finalBookLines.Add ("[chapterstart]");
				finalBookLines.Add ("{desc Skill " + associatedSkills + "}");
				if (!string.IsNullOrEmpty (associatedBlueprints)) {
					foreach (string associatedBlueprint in associatedBlueprintsList) {
						finalBookLines.Add ("[chapterbreak]");
						finalBookLines.Add ("#set HorizontalAlignment=Center");
						finalBookLines.Add ("#set VerticalAlignment=Top");
						finalBookLines.Add ("[chapterstart]");
						finalBookLines.Add ("{desc Blueprint " + associatedBlueprint + "}");//this will already have "_" in place of " "
					}
				}
				finalBookLines.Add ("[bookend]");

				string finalBook = finalBookLines.JoinToString ("\n");

				WriteMassImportAsset ("Book", bookFileName + ".txt", finalBook);

				entry.BookObject.StackName = bookFileName;
				entry.BookObject.State = "Default";
				entry.BookObject.DisplayName = bookTitle;
				entry.BookObject.Subcategory = defaultTemplate;
				entry.BookObject.PackName = "Books";
				entry.BookObject.PrefabName = "BookAvatar";

				entry.RelativeOrderPrice = priceMultiplier;
				//entry.DeliveryTimeInHours = (int)deliveryTime;
				entry.DisplayOrder = rowNum;

				library.CatalogueEntries.Add (entry);
			}

			Mods.Get.Editor.SaveMod <Library> (library, "Library", library.Name);
		}

		protected void MassImportBook (string path)
		{
			var MyFile = new CSVFile (path);

			/*
			0 Submission Date
			1 First Name
			2 Last Name
			3 E-mail
			4 Select Type
			5 Book or Diary Style
			6 Scripture Type
			7 Select Scroll Type
			8 Pick Ribbon Color (for scroll options A or B)
			9 Is the Scroll Sealed?
			10 Select Parchment Design
			11 Select Scrap Type
			12 Author(s*) - Optional
			13 Title - Optional
			14 Summary - Optional
			15 Select Age
			16 Text (Book, Diary, or Scripture)
			17 Text (Scroll, Parchment or Scrap)
			18 Submission Type
			19 Creative Commons Unported License
			*/

			for (int rowNum = 0; rowNum < MyFile.Rows.Count; rowNum++) {
				//get the data for each one of the book fields
				var row = MyFile.Rows [rowNum];

				string bookType = row.Fields [4];
				string bookStyle = row.Fields [5];
				string scriptureType = row.Fields [6];
				string scrollType = row.Fields [7];
				string ribbonColor = row.Fields [8];
				string scrollSealed = row.Fields [9];
				string parchmentDesign = row.Fields [10];
				string scrapType = row.Fields [11];
				string authors = row.Fields [12];
				string bookTitle = row.Fields [13];
				string summary = row.Fields [14];
				string age = row.Fields [15];
				string contentsBook = row.Fields [16];
				string contentsPage = row.Fields [17];
				string defaultTemplate = "GenericBook";

				bool useBookContents = true;

				List <string> newBook = new List<string> ();
				if (bookType.Contains ("Book")) {
					newBook.Add ("#set Type=Book");
					defaultTemplate = "GenericBook";
				} else if (bookType.Contains ("Diary")) {
					newBook.Add ("#set Type=Book");
					defaultTemplate = "Diary";
				} else if (bookType.Contains ("Scripture")) {
					newBook.Add ("#set Type=Scripture");
				} else if (bookType.Contains ("Scroll")) {
					newBook.Add ("#set Type=Scroll");
					useBookContents = false;
					defaultTemplate = "PersonalScroll";
				} else if (bookType.Contains ("Parchment")) {
					newBook.Add ("#set Type=Parchment");
					useBookContents = false;
					defaultTemplate = "PersonalParchment";
				} else if (bookType.Contains ("Scrap")) {
					newBook.Add ("#set TypeOfBook=Scrap");
					useBookContents = false;
					defaultTemplate = "Scrap";
				}

				if (!string.IsNullOrEmpty (defaultTemplate)) {
					newBook.Add ("#set DefaultTemplate=" + defaultTemplate);
				}
				if (!string.IsNullOrEmpty (bookTitle)) {
					newBook.Add ("#set Title=" + bookTitle);
				}
				if (!string.IsNullOrEmpty (summary)) {
					newBook.Add ("#set ContentsSummary=" + summary);
				}
				List <string> finalAuthors = new List<string> ();
				if (!string.IsNullOrEmpty (authors)) {
					string[] splitAuthors = authors.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string splitAuthor in splitAuthors) {
						finalAuthors.Add (splitAuthor.Trim ());
					}
					newBook.Add ("#set Authors=" + finalAuthors.JoinToString (","));
				}
				newBook.Add ("#set Status=Dormant");

				newBook.Add ("[bookstart]");
				string bookContents = contentsBook;
				if (string.IsNullOrEmpty (bookContents)) {
					bookContents = contentsPage;
				}
				if (!bookContents.Contains ("[chapterbreak]")) {
					newBook.Add ("[chapterbreak]");
					newBook.Add ("[chapterstart]");
				}
				newBook.Add (bookContents);
				newBook.Add ("[bookend]");

				string finalBook = newBook.JoinToString ("\n");

				string bookFileName = bookTitle.Replace (" ", "");
				bookFileName = bookFileName.Replace ("'", "");
				bookFileName = bookFileName.Replace ("\"", "");
				bookFileName = bookFileName.Replace (",", "");
				bookFileName = bookFileName.Replace (".", "");
				if (string.IsNullOrEmpty (bookFileName)) {
					bookFileName = "MassImportBook" + rowNum.ToString ();
				}
				bookFileName += ".txt";
				bookFileName = PathSanitizer.SanitizeFilename (bookFileName, '_');

				WriteMassImportAsset ("Book", bookFileName, finalBook);
			}
		}

		protected void MassImportWorldItem (string path)
		{
			CSVFile MyFile = new CSVFile (path);
			/*
			=======================
			0 - Name
			1 - Thumbnail
			2 - Weight
			3 - Size
			4 - Rarity
			5 - Material
			6 - Variations
			7 - Average Value
			8 - Average Value
			9 - Value
			10 - FINAL VALUE
			11 - Description
			=======================
			*/

			for (int rowNum = 1; rowNum < MyFile.Rows.Count; rowNum++) {
				//get the data for the plant
				var row = MyFile.Rows [rowNum];
				string prefabName = row.Fields [0];
				float finalValue = 0f;
				if (float.TryParse (row.Fields [10], out finalValue)) {
					foreach (WorldItemPack pack in WorldItems.Get.WorldItemPacks) {
						foreach (GameObject prefab in pack.Prefabs) {
							if (string.Equals (prefab.name, prefabName)) {
								Debug.Log ("Found prefab " + prefabName + ", setting base value to " + finalValue.ToString ());
								WorldItem wi = prefab.GetComponent <WorldItem> ();
								if (!string.IsNullOrEmpty (row.Fields [11])) {
									wi.Props.Global.ExamineInfo.StaticExamineMessage = row.Fields [11].Trim ();
									#if UNITY_EDITOR
																		UnityEditor.EditorUtility.SetDirty(wi);
																		UnityEditor.EditorUtility.SetDirty(wi.gameObject);
									#endif
								}
								//wi.Props.Global.BaseCurrencyValue = finalValue;
								//while we're here make sure our material isn't empty
								//if (wi.Props.Global.MaterialType == WIMaterialType.None) {
								//	wi.Props.Global.MaterialType = WIMaterialType.Stone;
								//	#if UNITY_EDITOR
								//	UnityEditor.EditorUtility.SetDirty (wi);
								//	UnityEditor.EditorUtility.SetDirty (wi.gameObject);
								//	#endif
								//}
							}
						}
					}
				}
			}
		}

		protected void ImportBook (KeyValuePair <string,string> book)
		{
			////Debug.Log ("Importing " + book.Key);
			Book newBook = new Book ();
			newBook.Name = book.Key;
			//newBook.Text			= newBook.Text.Replace ("\r\n", "&#13;");
			//newBook.Text			= newBook.Text.Replace ("\n\r", "&#13;");

			string[] splitSettings = book.Value.Split (new string [] { "[bookstart]" }, StringSplitOptions.RemoveEmptyEntries);
			//set the book's contents here - everything after 'start' will be used to generate pages
			newBook.Text = splitSettings [1];
			string[] splitLines = splitSettings [0].Split (new string [] {
								"\n",
								"\n\r",
								"\r\n"
						}, StringSplitOptions.RemoveEmptyEntries);
			//just add the raw text
			foreach (string splitLine in splitLines) {
				if (splitLine.StartsWith ("#set")) {
					//this is a kludge - we had to reset a lot of books
					string fixedLine = splitLine.Replace ("#set Type=", "#set TypeOfBook=");
					string varLine = fixedLine.Replace ("#set ", "");
					string[] splitVar = varLine.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					string varName = splitVar [0];
					string varVal = splitVar [1];
					Debug.Log ("Setting script var " + varName + ", " + varVal);
					SetScriptVar (newBook as System.Object, typeof(Book), varName, varVal);
				}
			}
			if (!string.IsNullOrEmpty (newBook.DefaultTemplate)) {
				newBook.DefaultTemplate = newBook.DefaultTemplate.Replace ("_", " ");
			}
			////Debug.Log ("Saving book " + newBook.Name);
			Mods.Get.Editor.SaveMod (newBook, "Book", newBook.Name);
		}

				#endregion

				#region speech import

		protected void ImportSpeech (KeyValuePair <string,string> speech)
		{
			Speech newSpeech = new Speech ();
			newSpeech.Name = GetNameFromDialogName (speech.Key);
			newSpeech.Text = speech.Value;

			string[] splitSettings = speech.Value.Split (new string [] { "[start]" }, StringSplitOptions.RemoveEmptyEntries);
			string[] splitLines = splitSettings [0].Split (new string [] {
								"\n",
								"\n\r",
								"\r\n"
						}, StringSplitOptions.RemoveEmptyEntries);
			//just add the raw text
			foreach (string splitLine in splitLines) {
				if (splitLine.StartsWith ("#set")) {
					string varLine = splitLine.Replace ("#set ", "");
					string[] splitVar = varLine.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					string varName = splitVar [0];
					string varVal = splitVar [1];
					SetScriptVar (newSpeech as System.Object, typeof(Speech), varName, varVal);
				}
			}
			Mods.Get.Editor.SaveMod (newSpeech, "Speech", newSpeech.Name);
		}

				#endregion

				#region conversation

		protected DialogDesigner.dialog dialogObject;
		protected string exchangePrefix = "E-";
		protected Dictionary <string, SimpleVar> variables = new Dictionary <string, SimpleVar> ();
		protected Dictionary <int, string> exchangeLookup = new Dictionary <int, string> ();
		protected List <List<string>> postLookupExchangeNameLists = new List<List<string>> ();
		protected Dictionary <ExchangeScript,string> postLookupExchangeNameStrings = new Dictionary<ExchangeScript, string> ();
		protected Dictionary <string, HashSet <int>> manualOptions = new Dictionary <string, HashSet<int>> ();
		protected Dictionary <string, HashSet <int>> manualDisable = new Dictionary <string, HashSet<int>> ();
		protected Conversation convertedDialog;

		public void ImportConversation (KeyValuePair <string,string> conversation)
		{
			if (conversation.Key.Contains ("_DTS")) {
				Debug.Log ("Encountered DTS, importing as speech");
				ImportSpeech (conversation);
				return;
			}

			if (conversation.Key.EndsWith (".xml")) {
				Debug.Log ("Skipping " + conversation.Key);
				return;
			}

			convertedDialog = gameObject.GetOrAdd <Conversation> ();
			convertedDialog.name = conversation.Key;
			variables.Clear ();
			exchangeLookup.Clear ();
			manualOptions.Clear ();
			manualDisable.Clear ();
			postLookupExchangeNameLists.Clear ();
			postLookupExchangeNameStrings.Clear ();

			convertedDialog.State = new ConversationState ();
			convertedDialog.Props = new ConversationProps ();

			convertedDialog.State.ListInAvailable = false;

			Debug.Log ("Importing " + conversation.Key);

			MemoryStream memStream = new MemoryStream (Encoding.ASCII.GetBytes (conversation.Value));
			var serializer = new XmlSerializer (typeof(DialogDesigner.dialog));
			dialogObject = (DialogDesigner.dialog)serializer.Deserialize (memStream);

			//get variables
			foreach (DialogDesigner.variable var in dialogObject.variables) {
				Debug.Log ("Found variable " + var.name + " in dialog designer " + var.type);
				if (var.type == "Boolean") {
					//ExchangeScript globalScript = InterpretCustomScript (var.description, null);
				} else {
					string variableName = var.name;
					int defaultValue = Int32.Parse (var.defaultValue);
					int minValue = 0;
					int maxValue = 0;

					string[] splitMinMaxVars = var.description.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string splitMinMax in splitMinMaxVars) {
						Debug.Log ("min max: " + splitMinMax);
						string[] splitMinMaxVar = splitMinMax.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
						string minMaxVarName = splitMinMaxVar [0];
						string minMaxVarVal = splitMinMaxVar [1];

						switch (minMaxVarName.ToLower ()) {
						case "min":
						default:
							minValue = Int32.Parse (minMaxVarVal);
							break;

						case "max":
							maxValue = Int32.Parse (minMaxVarVal);
							break;
						}
					}

					SimpleVar stateVar = new SimpleVar ();
					stateVar.DefaultValue = defaultValue;
					stateVar.Min = minValue;
					stateVar.Max = maxValue;
					variables.Add (variableName, stateVar);
					convertedDialog.State.ConversationVariables.Add (var.name, stateVar);
					//Debug.Log ("Added variable " + var.name + " count is now " + convertedDialog.State.ConversationVariables.Count.ToString ( ) + " in " + textAsset.name);
				}
			}

			convertedDialog.Props.Name = GetNameFromDialogName (conversation.Key);
			convertedDialog.Props.DefaultOpeningExchange = new Exchange ();

			int outgoingLoop = 0;
			int outgoingDepth = 0;
			int exchangeNumber = 1;

			int ddIndex = 0;
			AssignIndexToOption (dialogObject.options [0], ref ddIndex);

			foreach (DialogDesigner.option opt in dialogObject.options) {
				GenerateExchangesFromOptions (convertedDialog.Props.DefaultOpeningExchange, opt, ref outgoingLoop, outgoingDepth, ref exchangeNumber);
				outgoingLoop++;
			}

			LinkManualOptions (convertedDialog.Props.DefaultOpeningExchange);
			LinkDisableOptions (convertedDialog.Props.DefaultOpeningExchange);

			Debug.Log ("refreshing dialog in " + convertedDialog.name);
			convertedDialog.RefreshImmediately ();

			LinkExchangeNames ();

			foreach (KeyValuePair<int,string> exchangePair in exchangeLookup) {
				convertedDialog.State.ExchangeNames.Add (exchangePair.Key, exchangePair.Value);
			}

			Mods.Get.Editor.SaveMod <ConversationProps> (convertedDialog.Props, "Conversation", convertedDialog.Props.Name);
			convertedDialog.State.ListInAvailable = false;
			convertedDialog.State.Name = convertedDialog.Props.Name + "-State";
			Mods.Get.Editor.SaveMod <ConversationState> (convertedDialog.State, "Conversation", convertedDialog.State.Name);
		}

		public void AssignIndexToOption (DialogDesigner.option option, ref int currentIndex)
		{
			option.index = currentIndex;
			currentIndex++;
			foreach (DialogDesigner.option nextOption in option.options) {
				AssignIndexToOption (nextOption, ref currentIndex);
			}
		}

		public void GenerateExchangesFromOptions (Exchange currentExchange, DialogDesigner.option currentOption, ref int outgoingLoop, int depth, ref int exchangeNumber)
		{
			currentExchange.OutgoingStyle = ExchangeOutgoingStyle.Normal;

			string cleanPlayerDialog = StripCDataText (currentOption.text);
			string[] splitPlayerDialogLines = cleanPlayerDialog.Split (new string [] {
								"\n",
								"\r\n"
						}, StringSplitOptions.RemoveEmptyEntries);
			string finalPlayerDialog = string.Empty;
			foreach (string splitPlayerLine in splitPlayerDialogLines) {
				Debug.Log (splitPlayerLine);
				if (!splitPlayerLine.Contains ("#")) {
					finalPlayerDialog += splitPlayerLine;
				}
			}

			finalPlayerDialog = finalPlayerDialog.Replace ("[PageBreak]", "{pagebreak}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[PlayerName]", "{playername}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[CharacterName]", "{charactername}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[playerfirstname]", "{playerfirstname}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[playerlastname]", "{playerlastname}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[playerfullname]", "{playerfullname}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[playernickname]", "{playernickname}");

			finalPlayerDialog = finalPlayerDialog.Replace ("[characterfirstname]", "{characterfirstname}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[characterlastname]", "{characterlastname}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[characterfirstname]", "{characterfirstname}");
			finalPlayerDialog = finalPlayerDialog.Replace ("[characterfullname]", "{characterfullname}");


			//CHARACTER RESPONSE
			string cleanResponse = StripCDataText (currentOption.script);
			string[] splitResponseLines = cleanResponse.Split (new string [] {
								"\n",
								"\r\n"
						}, StringSplitOptions.RemoveEmptyEntries);
			string finalResponse = string.Empty;
			foreach (string splitResponseNameCheck in splitResponseLines) {
				if (splitResponseNameCheck.Contains ("#exchangename")) {
					string[] splitExchangeName = splitResponseNameCheck.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					string cleanExchangeName = splitExchangeName [1].Trim ();
					currentExchange.Name = cleanExchangeName;
					//Debug.Log ("Giving dialog exchange name " + currentExchange.Name);
				}
			}

			if (string.IsNullOrEmpty (currentExchange.Name)) {	//give it the default name if it hasn't been set yet
				currentExchange.Name = exchangePrefix + exchangeNumber.ToString ("D4");
			}
			if (currentExchange.DisplayOrder < 0) {
				currentExchange.DisplayOrder = exchangeNumber;
			}
			//add this to the lookup so we can manually link stuff up later
			exchangeLookup.Add (currentOption.index, currentExchange.Name);
			//use this to store dd indexes
			HashSet <int> manualOptionIndexes = new HashSet <int> ();
			HashSet <int> manualDisableIndexes = new HashSet <int> ();
			//add this list to lookup to link stuff up later
			try {
				manualOptions.Add (currentExchange.Name, manualOptionIndexes);
				manualDisable.Add (currentExchange.Name, manualDisableIndexes);
			} catch (Exception e) {
				Debug.LogError (currentExchange.Name + " already exists in conversation " + convertedDialog.name);
				//Debug.LogError (e);
			}

			foreach (string splitResponseLine in splitResponseLines) {
				if (!splitResponseLine.StartsWith ("//")) {
					if (!splitResponseLine.Contains ("#")) {//it's not a script
						if (String.Equals (splitResponseLine.ToLower ().Trim (), "stop")) {	//special case, this stops the conversation
							//this also overrides ExchangeOutgoingStyle.SiblingsOff
							currentExchange.OutgoingStyle = ExchangeOutgoingStyle.Stop;
						} else if (splitResponseLine.ToLower ().Contains ("alloff")) {
							currentExchange.OutgoingStyle = ExchangeOutgoingStyle.ManualOnly;
						} else if (splitResponseLine.Contains ("option-off-forever")) {
							string script = splitResponseLine.Replace ("option-off-forever", "").Trim ();
							string[] offForeverExchanges = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
							List <int> offForeverIndexes = new List <int> ();
							foreach (string offForeverExchange in offForeverExchanges) {
								////Debug.Log ("setting off forever " + offForeverExchange);
								offForeverIndexes.Add (Int32.Parse (offForeverExchange));
							}
							SetExchangeEnabled see = new SetExchangeEnabled ();
							see.Enabled = false;
							see.Exchanges = new List<string> (offForeverExchanges);
							//add this list to the post lookup so it gets converted into names
							postLookupExchangeNameLists.Add (see.Exchanges);
							currentExchange.Scripts.Add (see);
						} else {
							finalResponse += splitResponseLine;
						}
					} else {//it is a script or a state setting
						if (splitResponseLine.Contains ("#state|")) {
							InterpretConversationStateVariable (splitResponseLine);
						} else {
							InterpretScript (splitResponseLine, currentExchange, manualOptionIndexes, manualDisableIndexes);
						}
					}
				}
			}

			finalResponse = finalResponse.Replace ("<", "{");
			finalResponse = finalResponse.Replace (">", "}");

			/*
						finalResponse = Regex.Replace (finalResponse, "{playername}", "{playername}", RegexOptions.IgnoreCase);
						finalResponse = Regex.Replace (finalResponse, "{playerfirstname}", "{playerfirstname}", RegexOptions.IgnoreCase);
						finalResponse = Regex.Replace (finalResponse, "{playerlastname}", "{playerlastname}", RegexOptions.IgnoreCase);
						finalResponse = Regex.Replace (finalResponse, "{playerfullname}", "{playerfullname}", RegexOptions.IgnoreCase);
						finalResponse = Regex.Replace (finalResponse, "{playernickname}", "{playernickname}", RegexOptions.IgnoreCase);

						finalResponse = Regex.Replace (finalResponse, "{characterfirstname}", "{characterfirstname}", RegexOptions.IgnoreCase);
						finalResponse = Regex.Replace (finalResponse, "{characterlastname}", "{characterlastname}", RegexOptions.IgnoreCase);
						finalResponse = Regex.Replace (finalResponse, "{characterfirstname}", "{characterfirstname}", RegexOptions.IgnoreCase);
						finalResponse = Regex.Replace (finalResponse, "{characterfullname}", "{characterfullname}", RegexOptions.IgnoreCase);
						*/

			finalResponse = finalResponse.Replace ("[PageBreak]", "{pagebreak}");
			finalResponse = finalResponse.Replace ("[PlayerName]", "{playername}");
			finalResponse = finalResponse.Replace ("[CharacterName]", "{charactername}");
			finalResponse = finalResponse.Replace ("[playerfirstname]", "{playerfirstname}");
			finalResponse = finalResponse.Replace ("[playerlastname]", "{playerlastname}");
			finalResponse = finalResponse.Replace ("[playerfullname]", "{playerfullname}");
			finalResponse = finalResponse.Replace ("[playernickname]", "{playernickname}");

			finalResponse = finalResponse.Replace ("[characterfirstname]", "{characterfirstname}");
			finalResponse = finalResponse.Replace ("[characterlastname]", "{characterlastname}");
			finalResponse = finalResponse.Replace ("[characterfirstname]", "{characterfirstname}");
			finalResponse = finalResponse.Replace ("[characterfullname]", "{characterfullname}");

			currentExchange.PlayerDialog = finalPlayerDialog;
			currentExchange.CharacterResponse = finalResponse;

			if (!string.IsNullOrEmpty (currentOption.condition)) {
				InterpretCondition (currentOption.condition, currentExchange);
			}
			exchangeNumber++;

			int outgoingDepth = depth + 1;
			foreach (DialogDesigner.option outgoingOption in currentOption.options) {
				Exchange outgoingExchange = new Exchange ();
				convertedDialog.Props.Exchanges.Add (outgoingExchange);
				if (currentExchange.OutgoingStyle != ExchangeOutgoingStyle.ManualOnly) {
					currentExchange.OutgoingChoices.Add (outgoingExchange);
				} else {
					//make sure the outgoing choice is included in our 'on' set
					if (manualOptionIndexes.Contains (outgoingOption.index)) {
						Debug.Log ("Manual option contained " + outgoingOption.index.ToString () + " so we're adding it to outgoing choice");
						currentExchange.OutgoingChoices.Add (outgoingExchange);
					} else {
						Debug.Log ("Skipping " + outgoingOption.index.ToString () + " outgoing choice because parent is manual only and it's not included in our 'on' options");
					}
				}
				outgoingExchange.ParentExchangeName = currentExchange.Name;
				GenerateExchangesFromOptions (outgoingExchange, outgoingOption, ref outgoingLoop, outgoingDepth, ref exchangeNumber);
				outgoingLoop++;
			}
		}

		public void InterpretConversationStateVariable (string stateVariable)
		{
			stateVariable = stateVariable.Replace ("#state|", "");
			string[] splitStateVariable = stateVariable.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
			string command = splitStateVariable [0];
			//format: command var=value var=value
			//Debug.Log ("Found state command " + command + " in conversation " + convertedDialog.Props.Name);
			switch (command) {
			case "DefaultToDTS":
				//formant: DefaultToDTS CharacterName DTSName
				string characterName = splitStateVariable [1].Trim ();
				string dtsName = GetNameFromDialogName (splitStateVariable [2].Trim ());
				convertedDialog.State.DTSOverrides.Add (characterName, dtsName);
				//Debug.Log ("Added DTS override " + characterName + ", " + dtsName);
				break;

			default:
				break;
			}
		}

		public void InterpretCondition (string condition, Exchange exchange)
		{
			string[] splitConditions = condition.Split (new string [] { "&&" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string splitCondition in splitConditions) {
				Debug.Log ("Found condition " + splitCondition);
				splitCondition.Replace (" ", "");//get rid of spaces
				string variableName = string.Empty;
				string splitValueCheck = string.Empty;
				int variableValue = 0;
				bool foundVariable = false;
				foreach (string var in variables.Keys) {
					if (splitCondition.Contains (var)) {
						variableName = var;
						foundVariable = true;
						break;
					}
				}
				bool foundCheckType = false;
				VariableCheckType checkType = VariableCheckType.GreaterThan;
				//if we haven't found it, forget it
				if (foundVariable) {
					if (splitCondition.Contains (">=")) {
						checkType = VariableCheckType.GreaterThanOrEqualTo;
						splitValueCheck = ">=";
						foundCheckType = true;
					} else if (splitCondition.Contains ("<=")) {
						checkType = VariableCheckType.LessThanOrEqualTo;
						splitValueCheck = "<=";
						foundCheckType = true;
					} else if (splitCondition.Contains (">")) {
						checkType = VariableCheckType.GreaterThan;
						splitValueCheck = ">";
						foundCheckType = true;
					} else if (splitCondition.Contains ("<")) {
						checkType = VariableCheckType.LessThan;
						splitValueCheck = "<";
						foundCheckType = true;
					}
				}

				bool foundValue = false;
				if (foundVariable && foundCheckType) {
					Debug.Log ("Found check type " + splitValueCheck + " and variable " + variableName);
					string[] splitVariableValue = splitCondition.Split (new string [] { splitValueCheck }, StringSplitOptions.RemoveEmptyEntries);
					if (splitVariableValue.Length > 1) {
						variableValue = Int32.Parse (splitVariableValue [1]);
						foundValue = true;
					}
				}

				//OK we have everything we need
				if (foundCheckType && foundVariable && foundValue) {
					RequireConversationVariable requireConversationVariable = new RequireConversationVariable ();
					requireConversationVariable.CheckType = checkType;
					requireConversationVariable.VariableName = variableName;
					requireConversationVariable.VariableValue = variableValue;
					Debug.Log ("Adding RequireConversationVariable " + requireConversationVariable.CheckType.ToString () + ", " + requireConversationVariable.VariableName + ", " + requireConversationVariable.VariableValue.ToString ());
					exchange.Scripts.Add (requireConversationVariable);
				}
			}
		}

		public void InterpretScript (string script, Exchange exchange, HashSet <int> manualOptionLinks, HashSet <int> manualDisableLinks)
		{
			if (script.Contains ("#frontiers")) {	//this is a custom frontiers script
				InterpretCustomScript (script, exchange);
			} else if (script.Contains ("#rep")) {
				InterpretReputationChange (script, exchange);
			} else if (script.Contains ("#exchangename ")) {
				exchange.Name = script.Replace ("#exchangename ", "");
			} else if (
								script.ToLower ().Contains ("#exchangeconcluded") ||
				script.ToLower ().Contains ("#exchangenotconcluded") ||
				script.ToLower ().Contains ("#exchangeanyconcluded") ||
				script.ToLower ().Contains ("#exchangeanynotconcluded")) {
				//it's a require exchange shortcut
				//turn it into a proper tag and then interpret it
				InterpretExchangeConcludedScript (script, exchange);
				Debug.Log ("Interpreting script " + script);
			} else if (script.Contains ("#alwaysinclude")) {
				exchange.AlwaysInclude = true;
			} else if (script.Contains ("#off-forever")) {
				exchange.Availability = AvailabilityBehavior.Once;
			} else if (script.Contains ("#siblings-off")) {
				Debug.Log ("Setting siblings off");
				exchange.OutgoingStyle = ExchangeOutgoingStyle.SiblingsOff;
			} else if (script.Contains ("#off")) {//look for on/off
				script = script.Replace ("#off", "").Trim ();
				string[] manualOptions = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string manualOption in manualOptions) {
					Debug.Log ("Checking manual option " + manualOption);
					int manualOptionIndex = 0;
					if (Int32.TryParse (manualOption, out manualOptionIndex)) {
						Debug.Log ("adding manual option " + manualOption + " (" + manualOptionIndex.ToString () + ")");
						manualDisableLinks.Add (manualOptionIndex);
					}
				}
			} else if (script.Contains ("#on")) {
				Debug.Log ("Checking for manually linked options");
				script = script.Replace ("#on", "").Trim ();
				string[] manualOptions = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string manualOption in manualOptions) {
					Debug.Log ("Checking manual option " + manualOption);
					int manualOptionIndex = 0;
					if (Int32.TryParse (manualOption, out manualOptionIndex)) {
						Debug.Log ("adding manual option " + manualOption + " (" + manualOptionIndex.ToString () + ")");
						manualOptionLinks.Add (manualOptionIndex);
					}
				}
				return;
			} else if (script.Contains ("#substitute")) {
				script = script.Replace ("#substitute", "").Trim ();
				SubstituteConversation sc = new SubstituteConversation ();
				sc.OldConversationName = string.Empty;//will use current by default
				sc.DTSOverride = false;
				//what format is it?
				if (script.Contains ("=")) {
					//OldConversationName=NewConversationName Character
					//OldConversationName=NewConversationName
					if (script.Contains ("Character")) {
						//split by space
						string[] splitChar = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
						script = splitChar [0];
						sc.CharacterName = splitChar [1];
					}
					//now split by =
					string[] splitConv = script.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					sc.OldConversationName = GetNameFromDialogName (splitConv [0]);
					sc.NewConversationName = GetNameFromDialogName (splitConv [1]);
				} else if (script.Contains ("*")) {
					//special substitution
					sc.NewConversationName = GetNameFromDialogName (script);
				} else {
					//boring old substitution
					sc.NewConversationName = GetNameFromDialogName (script);
				}
				if (sc.NewConversationName.Contains ("-Dts-")) {
					sc.DTSOverride = true;
				}
				exchange.Scripts.Add (sc);
			} else if (script.Contains ("#hidevar")) {
				script = script.Replace ("#hidevar", "").Trim ();
				ShowVariable sv = new ShowVariable ();
				sv.Show = false;
				sv.VariableName = script;
				exchange.Scripts.Add (sv);
			} else if (script.Contains ("#showvar")) {
				script = script.Replace ("#showvar", "").Trim ();
				;
				ShowVariable sv = new ShowVariable ();
				sv.Show = true;
				sv.VariableName = script;
				exchange.Scripts.Add (sv);
			} else {
				//if we've gotten this far
				//look for set variables
				if (script.Contains ("#set")) {	//we're setting variables, so add scripts
					Debug.Log ("Script " + script + " contains #set");
					script = script.Replace (" = ", "=");

					//first, intercept and protect any eval statements (anything in parenthesis)
					MatchCollection matches = Regex.Matches (script, gParenthesisPattern);
					Match match = null;
					for (int i = 0; i < matches.Count; i++) {
						match = matches [i];
						Debug.Log ("Found match " + match.Value);
						//get rid of spaces
						script = script.Replace (match.Value, match.Value.Replace (" ", ""));
					}

					string[] setPieces = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
					//first one will be #set, next will be variable
					string variableSetCommand = setPieces [1];
					foreach (KeyValuePair <string, SimpleVar> variable in variables) {
						string incrementCheck = variable.Key + "++";
						string decrementCheck = variable.Key + "--";
						string setCheck = "=";
						Debug.Log ("Looking for " + incrementCheck + " and " + decrementCheck);
						if (variableSetCommand.Contains (incrementCheck)) {
							//check for increments
							bool alreadySet = false;
							foreach (ExchangeScript exchangeScript in exchange.Scripts) {	//if we've already got a variable changer
								if (exchangeScript.GetType ().Name == "ChangeConversationVariable") {	//see if it's already incrementing this variable - if so increment it more
									ChangeConversationVariable changeConversationVariable = (ChangeConversationVariable)exchangeScript;
									if (changeConversationVariable.VariableName == variable.Key
										&& changeConversationVariable.ChangeType == ChangeVariableType.Increment) {	//increment it once more
										changeConversationVariable.SetValue++;
										alreadySet = true;
									}
								}
							}
							//if we got this far and didn't set it, make a new one
							if (!alreadySet) {
								ChangeConversationVariable newChangeConvoVariable = new ChangeConversationVariable ();
								newChangeConvoVariable.CallOn = ExchangeAction.Choose;
								newChangeConvoVariable.VariableName = variable.Key;
								newChangeConvoVariable.ChangeType = ChangeVariableType.Increment;
								newChangeConvoVariable.SetValue = 1;
								exchange.Scripts.Add (newChangeConvoVariable);
							}
						} else if (variableSetCommand.Contains (decrementCheck)) {
							bool alreadySet = false;
							foreach (ExchangeScript exchangeScript in exchange.Scripts) {	//if we've already got a variable changer
								if (exchangeScript.GetType ().Name == "ChangeConversationVariable") {	//see if it's already incrementing this variable - if so increment it more
									ChangeConversationVariable changeConversationVariable = (ChangeConversationVariable)exchangeScript;
									if (changeConversationVariable.VariableName == variable.Key
										&& changeConversationVariable.ChangeType == ChangeVariableType.Decrement) {	//increment it once more
										changeConversationVariable.SetValue++;
										alreadySet = true;
									}
								}
							}
							//if we got this far and didn't set it, make a new one
							if (!alreadySet) {
								ChangeConversationVariable newChangeConvoVariable = new ChangeConversationVariable ();
								newChangeConvoVariable.CallOn = ExchangeAction.Choose;
								newChangeConvoVariable.VariableName = variable.Key;
								newChangeConvoVariable.ChangeType = ChangeVariableType.Decrement;
								newChangeConvoVariable.SetValue = 1;
								exchange.Scripts.Add (newChangeConvoVariable);
							}
						} else if (variableSetCommand.Contains (setCheck)) {
							//replace spaces so we can split along the =
							variableSetCommand = variableSetCommand.Replace (" ", "");
							string[] splitSetArray = variableSetCommand.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
							bool alreadySet = false;
							if (splitSetArray.Length > 1) {
								int newValue = Int32.Parse (splitSetArray [1]);
								foreach (ExchangeScript exchangeScript in exchange.Scripts) {	//if we've already got a variable changer
									if (exchangeScript.GetType ().Name == "ChangeConversationVariable") {	//see if it's already setting this variable - if so replace the set value
										ChangeConversationVariable changeConversationVariable = (ChangeConversationVariable)exchangeScript;
										if (changeConversationVariable.VariableName == variable.Key
											&& changeConversationVariable.ChangeType == ChangeVariableType.SetValue) {
											changeConversationVariable.SetValue = newValue;
											alreadySet = true;
										}
									}
								}
								//if we got this far and didn't set it, make a new one
								if (!alreadySet) {
									ChangeConversationVariable newChangeConvoVariable = new ChangeConversationVariable ();
									newChangeConvoVariable.CallOn = ExchangeAction.Choose;
									newChangeConvoVariable.VariableName = variable.Key;
									newChangeConvoVariable.ChangeType = ChangeVariableType.SetValue;
									newChangeConvoVariable.SetValue = newValue;
									exchange.Scripts.Add (newChangeConvoVariable);
								}
							}
						}
					}
				}
			}
		}

		public ExchangeScript InterpretExchangeConcludedScript (string script, Exchange exchange)
		{
			RequireExchangesConcluded rec = new RequireExchangesConcluded ();

			string[] splitScript = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
			string operation = splitScript [0].Replace ("#", "");
			string convoName = splitScript [1];
			if (!convoName.Contains ("*")) {
				convoName = GetNameFromDialogName (convoName);
			}
			bool convertExchanges = false;
			if (convoName == convertedDialog.Props.Name) {
				//we only want to convert them if they ARE in this convo
				//otherwise leave them alone
				convertExchanges = true;
			} else {
				Debug.Log ("Exchange concluded script in convo " + convoName + " - not linking");
			}
			List <string> exchanges = new List <string> ();
			for (int i = 2; i < splitScript.Length; i++) {
				exchanges.Add (splitScript [i]);
			}

			rec.ConversationName = convoName;
			rec.Exchanges = exchanges;

			switch (operation.ToLower ()) {
			case "exchangeconcluded":
				rec.RequireAllExchanges = true;
				rec.RequireConcluded = true;
				break;

			case "exchangeconcludedifinit":
				rec.RequireAllExchanges = true;
				rec.RequireConcluded = true;
				rec.RequireConversationInitiated = false;
				break;

			case "exchangeanyconcluded":
				rec.RequireAllExchanges = false;
				rec.RequireConcluded = true;
				break;

			case "exchangeanyconcludedifinit":
				rec.RequireAllExchanges = false;
				rec.RequireConcluded = true;
				rec.RequireConversationInitiated = false;
				break;

			case "exchangenotconcluded":
				rec.RequireAllExchanges = true;
				rec.RequireConcluded = false;
				break;

			case "exchangenotconcludedifinit":
				rec.RequireAllExchanges = true;
				rec.RequireConcluded = false;
				rec.RequireConversationInitiated = false;
				break;

			case "exchangeanynotconcluded":
				rec.RequireAllExchanges = false;
				rec.RequireConcluded = false;
				break;

			case "exchangeanynotconcludedifinit":
				rec.RequireAllExchanges = false;
				rec.RequireConcluded = false;
				rec.RequireConversationInitiated = false;
				break;

			default:
				Debug.Log ("Malformed exchange concluded script: " + script);
				break;
			}

			if (exchange != null) {
				exchange.Scripts.Add (rec);
				if (convertExchanges) {
					postLookupExchangeNameLists.Add (rec.Exchanges);
				}
			}
			return rec;
		}

		public ExchangeScript InterpretReputationChange (string script, Exchange exchange)
		{
			ChangeRepuation changeReputation = new ChangeRepuation ();
			changeReputation.ReputationChangeSize = WISize.NoLimit;//none
			string[] splitRep = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
			//0 - #rep
			//1 - operation+value
			//2 - characterName
			string operation = splitRep [1];
			string characterName = string.Empty;
			if (splitRep.Length > 2) {
				characterName = splitRep [2].Trim ();
			}

			if (operation.Contains ("++")) {
				changeReputation.ReputationAmount = 1;
				changeReputation.ChangeType = ChangeVariableType.Increment;
			} else if (operation.Contains ("--")) {
				changeReputation.ReputationAmount = 1;
				changeReputation.ChangeType = ChangeVariableType.Decrement;
			} else if (operation.Contains ("=")) {
				int repValue = 0;
				Int32.TryParse (operation.Replace ("=", "").Trim (), out repValue);
				changeReputation.ReputationAmount = repValue;
				changeReputation.ChangeType = ChangeVariableType.SetValue;
			} else {
				//we could either have a numeric value or a string value
				bool addition = operation.Contains ("+");
				operation = operation.Replace ("+", "");
				operation = operation.Replace ("-", "").Trim ().ToLower ();
				//this should leave us with just the number and/or value
				switch (operation) {
				case "tiny":
					changeReputation.ReputationChangeSize = WISize.Tiny;
					changeReputation.ChangeType = addition ? ChangeVariableType.Increment : ChangeVariableType.Decrement;
					break;

				case "small":
					changeReputation.ReputationChangeSize = WISize.Small;
					changeReputation.ChangeType = addition ? ChangeVariableType.Increment : ChangeVariableType.Decrement;
					break;

				case "medium":
					changeReputation.ReputationChangeSize = WISize.Medium;
					changeReputation.ChangeType = addition ? ChangeVariableType.Increment : ChangeVariableType.Decrement;
					break;

				case "large":
					changeReputation.ReputationChangeSize = WISize.Large;
					changeReputation.ChangeType = addition ? ChangeVariableType.Increment : ChangeVariableType.Decrement;
					break;

				case "huge":
					changeReputation.ReputationChangeSize = WISize.Huge;
					changeReputation.ChangeType = addition ? ChangeVariableType.Increment : ChangeVariableType.Decrement;
					break;

				default:
					//it must be a numeric value
					int changeAmount = Int32.Parse (operation);
					changeReputation.ChangeType = addition ? ChangeVariableType.Increment : ChangeVariableType.Decrement;
					changeReputation.ReputationAmount = changeAmount;
					break;
				}
			}
			if (exchange != null) {
				exchange.Scripts.Add (changeReputation);
			}
			return changeReputation;
		}

		public static string gParenthesisPattern = @"\(([^)]*)\)";

		public ExchangeScript InterpretCustomScript (string script, Exchange exchange)
		{
			ExchangeScript exchangeScript = null;
			//creates a script from sett
			string scriptType = string.Empty;
			Dictionary <string, string> values = new Dictionary <string, string> ();

			//first, intercept and protect any eval statements (anything in parenthesis)
			MatchCollection matches = Regex.Matches (script, gParenthesisPattern);
			Match match = null;
			for (int i = 0; i < matches.Count; i++) {
				match = matches [i];
				Debug.Log ("Found match " + match.Value);
				//get rid of spaces
				script = script.Replace (match.Value, match.Value.Replace (" ", ""));
			}

			string[] splitScript = script.Split (new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
			//#frontiers|NameOfType, var=value, var=value

			foreach (string splitScriptPiece in splitScript) {
				if (splitScriptPiece.Contains ("#frontiers")) {
					//this declares the name of the type
					//#frontiers, NameOfType
					string[] splitScriptType = splitScript [0].Split (new string [] { "|" }, StringSplitOptions.RemoveEmptyEntries);
					Debug.Log ("Checking split script type " + splitScriptPiece + " in " + convertedDialog.name + " : " + splitScript);
					scriptType = splitScriptType [1];
				} else if (splitScriptPiece.Contains ("=")) {
					Debug.Log ("Checking split script value " + splitScriptPiece);
					//this contains a value
					string[] splitScriptValue = splitScriptPiece.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					try {
						values.Add (splitScriptValue [0], splitScriptValue [1]);
					} catch (Exception e) {
						Debug.Log ("Had trouble with split script value (length " + splitScriptValue.Length.ToString () + e.ToString ());
					}
				}
			}

			//did we get what we need?
			if (!string.IsNullOrEmpty (scriptType)) {
				Debug.Log ("Creating script of type " + scriptType);
				Type exchangeScriptType = Type.GetType ("Frontiers.Story.Conversations." + scriptType);
				if (exchangeScriptType != null) {
					exchangeScript = (ExchangeScript)Activator.CreateInstance (exchangeScriptType);
					if (values.Count > 0) {
						//set values using reflection
						foreach (KeyValuePair <string, string> checkPair in values) {
							KeyValuePair <string, string> valuePair = checkPair;
							//make sure ConversationName variables are conformed!
							if (valuePair.Key.Contains ("ConversationName") || valuePair.Key.Contains ("DTSConversationName")) {
								valuePair = new KeyValuePair<string, string> (valuePair.Key, GetNameFromDialogName (valuePair.Value));
							}

							System.Reflection.FieldInfo fieldInfo = exchangeScriptType.GetField (valuePair.Key);
							if (fieldInfo != null) {
								System.ComponentModel.StringConverter converter = new System.ComponentModel.StringConverter ();
								Debug.Log ("Found field " + valuePair.Key + ", setting to " + valuePair.Value);
								Type fieldType = fieldInfo.FieldType;
								System.Object convertedValue = null;
								switch (fieldType.Name) {	//custom for custom enums
								case "Int32":
									convertedValue = (int)Int32.Parse (valuePair.Value);
									break;

								case "Boolean":
									convertedValue = (bool)Boolean.Parse (valuePair.Value);
									break;

								case "Single":
									convertedValue = (float)Single.Parse (valuePair.Value);
									break;

								case "Double":
									convertedValue = (double)Double.Parse (valuePair.Value);
									break;

								case "String":
									convertedValue = valuePair.Value;
									if (valuePair.Key == "ExchangeName") {
										//if it's an exchange name we'll want to look it up later
										postLookupExchangeNameStrings.Add (exchangeScript, valuePair.Value);
									}
									break;
								//there HAS to be a better f'ing way to do this...
								case "VariableCheckType":
								case "MissionStatus":
								case "MissionOriginType":
								case "EmotionalState":
								case "LiveTargetType":
								case "MotileActionPriority":
								case "AvailabilityBehavior":
								case "ExchangeAction":
								case "ChangeVariableType":
								case "BookStatus":
								case "WICurrencyType":
									Debug.Log ("Parsing enum " + valuePair.Value);
									convertedValue = Enum.Parse (fieldType, valuePair.Value, true);
									break;

								case "MotileAction":
																				//this is a biggie...
																				//we have to split up the values and assign them separately
									MotileAction action = new MotileAction ();
									MobileReference mobileReference = new MobileReference ();
									string[] splitActionVars = valuePair.Value.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
									switch (splitActionVars [0]) {
									case "GoToQuestActionNode":
																								//should be 3 more variables - mobileReference, expiration and a float value IF expiration is not Never
										string[] splitMobileReference = splitActionVars [1].Split (new string [] { "|" }, StringSplitOptions.RemoveEmptyEntries);
										mobileReference.GroupPath = splitMobileReference [0];
										mobileReference.FileName = splitMobileReference [1];
										action.Target = mobileReference;

										action.Expiration = (MotileExpiration)Enum.Parse (typeof(MotileExpiration), splitActionVars [1]);
										switch (action.Expiration) {
										case MotileExpiration.Duration:
											action.RTDuration = float.Parse (splitActionVars [2]);
											break;

										case MotileExpiration.TargetInRange:
											action.Range = float.Parse (splitActionVars [2]);
											break;

										case MotileExpiration.TargetOutOfRange:
											action.OutOfRange = float.Parse (splitActionVars [2]);
											break;

										case MotileExpiration.Never:
										default:
																												//no last value
											break;
										}
										break;

									case "FollowPlayer":
										action.Type = MotileActionType.FollowTargetHolder;
										action.FollowType = MotileFollowType.Follower;
										action.Expiration = MotileExpiration.Never;
										break;

									default:
										break;
									}
									convertedValue = action;
									break;

								case "List`1":
									string[] splitValues = valuePair.Value.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
									List <string> stringListValue = new List<string> (splitValues);
									convertedValue = stringListValue;
									if (valuePair.Key == "Exchanges") {
										////Debug.Log ("Putting exchange names in the lookup for later for type " + exchangeScript.GetType ().Name + " with " + stringListValue.Count.ToString ( ) + " items");
										//we need to look these up later
										postLookupExchangeNameLists.Add (stringListValue);
										//afterwards we'll convert these from DD nums to exchange names
									}
									break;

								default:
									Debug.Log ("Type of : " + fieldType.Name);
									convertedValue = converter.ConvertTo (valuePair.Value, fieldType);
									break;
								}
								fieldInfo.SetValue (exchangeScript as System.Object, convertedValue);
							}
						}
					}
					if (exchange != null) {
						exchange.Scripts.Add (exchangeScript);
					}
				} else {
					Debug.Log ("type " + scriptType + " returned no system type");
				}
			}
			return exchangeScript;
		}

		public void LinkManualOptions (Exchange currentExchange)
		{
			HashSet <int> manualOptionsList = null;
			if (manualOptions.TryGetValue (currentExchange.Name, out manualOptionsList)) {
				foreach (int manualOptionIndex in manualOptionsList) {
					string exchangeName = string.Empty;
					if (exchangeLookup.TryGetValue (manualOptionIndex, out exchangeName)) {
						if (!currentExchange.LinkedOutgoingChoices.Contains (exchangeName)) {
							Debug.Log ("Adding manual optino " + exchangeName + " (index " + manualOptionIndex.ToString () + ") to exchange " + currentExchange.Name);
							currentExchange.LinkedOutgoingChoices.SafeAdd (exchangeName);
						}
					}
				}
			}
			foreach (Exchange childExchange in currentExchange.OutgoingChoices) {
				currentExchange.OutgoingChoiceNames.Add (childExchange.Name);
				LinkManualOptions (childExchange);
			}
		}

		public void LinkDisableOptions (Exchange currentExchange)
		{
			HashSet <int> manualOptionsList = null;
			if (manualDisable.TryGetValue (currentExchange.Name, out manualOptionsList)) {
				foreach (int manualOptionIndex in manualOptionsList) {
					string exchangeName = string.Empty;
					if (exchangeLookup.TryGetValue (manualOptionIndex, out exchangeName)) {
						if (!currentExchange.LinkedOutgoingChoices.Contains (exchangeName)) {
							Debug.Log ("Adding manual optino " + exchangeName + " (index " + manualOptionIndex.ToString () + ") to exchange " + currentExchange.Name);
							currentExchange.DisabledIncomingChoices.Add (exchangeName);
						}
					}
				}
			}
			foreach (Exchange childExchange in currentExchange.OutgoingChoices) {
				LinkDisableOptions (childExchange);
			}
		}

		public void LinkExchangeNames ()
		{
			foreach (KeyValuePair <ExchangeScript,string> exchangePair in postLookupExchangeNameStrings) {
				//the exchange script has a field called "ExchangeName"
				//and MAY have an integer representing an exchange number
				int exchangeNumber = 0;
				if (!exchangePair.Value.Contains ("E-")) {
					if (Int32.TryParse (exchangePair.Value, out exchangeNumber)) {
						//looks like it's a number!
						//get the link
						string exchangeName = string.Empty;
						if (exchangeLookup.TryGetValue (exchangeNumber, out exchangeName)) {
							//set the value in the script
							SetScriptVar (exchangePair.Key, exchangePair.Key.GetType (), "ExchangeName", exchangeName);
						}
					}
				}
			}

			foreach (List <string> exchangeNameList in postLookupExchangeNameLists) {
				//we don't have to do any actual set values here since we're working with lists
				//just look at the first value and see if it's numeric
				//if it is, replace each integer with a lookup
				////Debug.Log ("Checking exchange name list..." + exchangeNameList.Count.ToString ( ) + " in this list");
				if (exchangeNameList.Count > 0) {
					string firstExchangeName = exchangeNameList [0];
					int firstExchangeNum = 0;
					if (Int32.TryParse (firstExchangeName, out firstExchangeNum)) {
						//it's a list of nums
						////Debug.Log ("Looks like a list of nums based on first exchange " + firstExchangeName);
						for (int i = 0; i < exchangeNameList.Count; i++) {
							string exchangeName = exchangeNameList [i];
							int exchangeNum = Int32.Parse (exchangeName);
							if (exchangeLookup.TryGetValue (exchangeNum, out exchangeName)) {
								////Debug.Log ("Replacing exchange num " + exchangeNum.ToString () + " with " + exchangeName);
								exchangeNameList [i] = exchangeName;
							} else {
								////Debug.Log ("COULDN'T FIND EXCHANGE ASSOCIATED WITH NUMBER " + exchangeNum.ToString ());
							}
						}
					}
				}
			}
		}

		public string StripCDataText (string optionText)
		{
			if (string.IsNullOrEmpty (optionText)) {
				return string.Empty;
			}

			string strippedText = optionText.Replace ("]]>", "");
			strippedText = strippedText.Replace ("![CDATA[", "");
			return strippedText;
		}

				#endregion

				#region plants

		public enum PField
		{
			_Plant_Common_Name = 0,
			_Plant_Nick_Name,
			_Plant_Scientific_Name,
			_Submission_Date,
			_Climate,
			_Above_Below_Ground,
			_Elevations,
			_Low,
			_Med,
			_High,
			_Plant_Body_Type,
			_Flowers,
			_Flower_Type,
			_Root_Type,
			_Root_Size,
			_Root_Color,
			_Thorns,
			_Plant_Seasonality_SSFW,
			_Spring,
			_Summer,
			_Fall,
			_Winter,
			_Height_Spr,
			_Height_Sum,
			_Height_Fall,
			_Height_Win,
			_Color_Spr,
			_Color_Sum,
			_Color_Fall,
			_Color_Win,
			_Flowers_in_Spr,
			_Flowers_in_Sum,
			_Flowers_in_Fall,
			_Flowers_in_Win,
			_Flower_Size_Spring,
			_Flower_Density_Spring,
			_Flower_Size_Summer,
			_Flower_Density_Summer,
			_Flower_Size_Fall,
			_Flower_Density_Fall,
			_Flower_Size_Winter,
			_Flower_Density_Winter,
			_Flower_Color_Spring,
			_Flower_Color_Summer,
			_Flower_Color_Fall,
			_Flower_Color_Winter,
			_Plant_Seasonality_DW,
			_Wet_Season,
			_Dry_Season,
			_Height_Wet,
			_Height_Dry,
			_Color_Wet,
			_Color_Dry,
			_Flowers_in_Wet,
			_Flowers_in_Dry,
			_Flower_Size_Wet_Season,
			_Flower_Density_Wet_Season,
			_Flower_Size_Dry_Season,
			_Flower_Density_Dry_Season,
			_lower_Color_Wet_Season,
			_Flower_Color_Dry_Season,
			_Edibility_Raw,
			_How_Filling_Raw,
			_Other_Properties_Raw,
			_Poison_Raw,
			_Hallucin_Raw,
			_Med_Raw,
			_How_Poisonous_Raw,
			_Poison_Duration_Raw,
			_How_Hallucinogenic_Raw,
			_Med_Strength_Raw,
			_Edibility_Cooked,
			_How_Filling_Cooked,
			_Other_Properties_Cooked,
			_Poison_Cooked,
			_Hallucin_Cooked,
			_Med_Cooked,
			_How_Poisonous_Cooked,
			_Poison_Duration_Cooked,
			_How_Hallucinogenic_Cooked,
			_Medicine_Strength_Cooked,
			_Additional_Notes_and_Requests,
			_First_Name,
			_Last_Name,
			_E_mail_Address,
		}

		public void MassImportPlant (string path)
		{
			CSVFile MyFile = new CSVFile (path);


			/*
			=======================
			0 - Plant Common Name
			1 - Plant Nick Name
			2 - Plant Scientific Name
			3 - Submission Date
			4 - Climate
			5 - Above/Below Ground
			6 - Elevations ?
			7 - Low	Med	High
			8 - Plant Body Type
			9 - Flowers ?
			10 - Flower Type
			11 - Root Type
			12 - Root Size
			13 - Root Color
			14 - Thorns?
			15 - Plant Seasonality
			16 - Spring
			17 - Summer
			18 - Fall
			19 - Winter
			20 - Height - Spr
			21 - Height - Sum
			22 - Height - Fall
			23 - Height - Win
			24 - Color - Spr
			25 - Color - Sum
			26 - Color - Fall
			27 - Color - Win
			28 - Flowers in Spr
			29 - Flowers in Sum
			30 - Flowers in Fall
			31 - Flowers in Win
			32 - Flower Size- Spring
			33 - Flower Density - Spring
			34 - Flower Size - Summer
			35 - Flower Density - Summer
			36 - Flower Size - Fall
			37 - Flower Density - Fall
			38 - Flower Size - Winter
			39 - Flower Density - Winter
			40 - Flower Color - Spring
			41 - Flower Color - Summer
			42 - Flower Color - Fall
			43 - Flower Color - Winter
			44 - Plant Seasonality
			45 - Wet Season
			46 - Dry Season
			47 - Height - Wet
			48 - Height - Dry
			49 - Color - Wet
			50 - Color - Dry
			51 - Flowers in Wet
			52 - Flowers in Dry
			53 - Flower Size - Wet Season
			54 - Flower Density - Wet Season
			55 - Flower Size - Dry Season
			56 - Flower Density - Dry Season
			57 - lower Color - Wet Season
			58 - Flower Color - Dry Season
			59 - Edibility (Raw)
			60 - How Filling (Raw)
			61 - Other Properties (Raw)
			62 - Poison (Raw)
			63 - Hallucin (Raw)	Med (Raw)
			64 - How Poisonous (Raw)
			65 - Poison Duration (Raw)
			66 - How Hallucinogenic (Raw)
			67 - Med Strength (Raw)
			68 - Edibility (Cooked)
			69 - How Filling (Cooked)
			70 - Other Properties (Cooked)
			71 - Poison (Cooked)
			72 - Hallucin (Cooked)
			73 - Med (Cooked)
			74 - How Poisonous (Cooked)
			75 - Poison Duration (Cooked)
			76 - How Hallucinogenic (Cooked)
			77 - Medicine Strength (Cooked)
			78 - Additional Notes & Requests
			79 - First Name
			80 - Last Name
			81 - E-mail Address
			=======================
			*/

//			List <string> fields = new List<string> ();
//			fields.Add ("=======================");
//			var topRow = MyFile.Rows [0];
//			for (int i = 0; i < topRow.Fields.Count; i++) {
//				fields.Add (i + " - " + topRow.Fields [i]);
//			}
//			fields.Add ("=======================");
//			string allFields = fields.JoinToString ("\n");
//			////Debug.Log (allFields);

			///skip rows 0 and 1 they're just headings
			for (int rowNum = 2; rowNum < MyFile.Rows.Count; rowNum++) {
				//get the data for the plant
				var row = MyFile.Rows [rowNum];

				Plant plant = new Plant ();

				#region basic props

				plant.CommonName = row.Fields [(int)PField._Plant_Common_Name].Trim ();
				plant.Name = plant.CommonName.Replace (" ", "").Trim ();
				plant.Name = plant.Name.Replace ("'", "");
				plant.Name = PathSanitizer.SanitizeFilename (plant.Name, '_');
				plant.NickName = row.Fields [(int)PField._Plant_Nick_Name].Trim ();
				plant.ScientificName = row.Fields [(int)PField._Plant_Scientific_Name].Trim ();

				Debug.Log ("importing plant " + plant.Name);

				if (string.IsNullOrEmpty (plant.Name)) {
					////Debug.Log ("Plant name was empty, not saving");
				} else {
					//non-season stuff
					plant.AboveGround = row.Fields [(int)PField._Above_Below_Ground].Contains ("ABOVE");
					plant.HasThorns = row.Fields [(int)PField._Thorns].Contains ("Yes");
					plant.HasFlowers = !row.Fields [(int)PField._Flowers].Contains ("No");

					switch (row.Fields [(int)PField._Root_Type].ToLower ().Trim ()) {
					case "thin fibrous":
						plant.RootType = PlantRootType.ThinFibrous;
						break;

					case "typical branched":
					default:
						plant.RootType = PlantRootType.TypicalBranched;
						break;

					case "thick taproot":
						plant.RootType = PlantRootType.ThickTaproot;
						break;
					}

					switch (row.Fields [(int)PField._Root_Size].ToLower ().Trim ()) {
					case "small":
						plant.RootSize = PlantRootSize.Small;
						break;

					case "medium":
					default:
						plant.RootSize = PlantRootSize.Medium;
						break;

					case "large":
						plant.RootSize = PlantRootSize.Large;
						break;
					}

					int rootColorInt = 127;
					if (Int32.TryParse (row.Fields [(int)PField._Root_Color], out rootColorInt)) {
						rootColorInt -= 127;
					}
					plant.RootHueShift = ((float)rootColorInt) / 127f;

					switch (row.Fields [(int)PField._Plant_Body_Type].ToLower ().Trim ()) {
					case "shrub 1":
					default:
						plant.BodyType = 0;
						break;

					case "shrub 2":
						plant.BodyType = 1;
						break;

					case "shrub 3":
						plant.BodyType = 2;
						break;

					case "cactus 1":
						plant.BodyType = 3;
						break;

					case "cactus 2":
						plant.BodyType = 4;
						break;

					case "cactus 3":
						plant.BodyType = 5;
						break;

					case "mushroom 1":
						plant.BodyType = 6;
						break;

					case "mushroom 2":
						plant.BodyType = 7;
						break;

					case "mushroom 3":
						plant.BodyType = 8;
						break;

					case "mushroom 4":
						plant.BodyType = 9;
						break;

					case "vegetation 1":
						plant.BodyType = 10;
						break;

					case "vegetation 2":
						plant.BodyType = 11;
						break;

					case "vegetation 3":
						plant.BodyType = 12;
						break;

					case "vegetation 4":
						plant.BodyType = 13;
						break;

					case "vegetation 5":
						plant.BodyType = 14;
						break;

					case "vegetation 6":
						plant.BodyType = 15;
						break;

					case "vegetation 7":
						plant.BodyType = 16;
						break;

					case "vegetation 8":
						plant.BodyType = 17;
						break;

					case "vegetation 9":
						plant.BodyType = 18;
						break;

					case "vegetation 10":
						plant.BodyType = 19;
						break;
					}

					if (plant.HasFlowers) {
						////Debug.Log("Flower type: " + row.Fields [6]);
						switch (row.Fields [(int)PField._Flower_Type].ToLower ().Trim ()) {
						case "petals 1":
						default:
							plant.FlowerType = 0;
							break;

						case "petals 2":
							plant.FlowerType = 1;
							break;

						case "petals 3":
							plant.FlowerType = 2;
							break;

						case "petals 4":
							plant.FlowerType = 3;
							break;

						case "petals 5":
							plant.FlowerType = 4;
							break;

						case "petals 6":
							plant.FlowerType = 5;
							break;

						case "petals 7":
							plant.FlowerType = 6;
							break;

						case "petals 8":
							plant.FlowerType = 7;
							break;

						case "petals 9":
							plant.FlowerType = 8;
							break;

						case "petals 10":
							plant.FlowerType = 9;
							break;

						case "petals 11":
							plant.FlowerType = 10;
							break;

						case "petals 12":
							plant.FlowerType = 11;
							break;

						case "petals 13":
							plant.FlowerType = 12;
							break;

						case "petals 14":
							plant.FlowerType = 13;
							break;

						case "thistle style":
							plant.FlowerType = 14;
							break;

						case "puffball":
							plant.FlowerType = 15;
							break;

						case "seeds":
							plant.FlowerType = 16;
							break;

						case "rose":
							plant.FlowerType = 17;
							break;

						case "twisted rose":
							plant.FlowerType = 18;
							break;
						}
					}

					#endregion

					#region climate stuff

					//season stuff
					PlantSeasonalSettings Spring = null;
					PlantSeasonalSettings Summer = null;
					PlantSeasonalSettings Autumn = null;
					PlantSeasonalSettings Winter = null;
					PlantSeasonalSettings Dry = null;
					PlantSeasonalSettings Wet = null;
					FoodStuffProps RawProps = new FoodStuffProps ();
					FoodStuffProps CookedProps = new FoodStuffProps ();

					bool growsInSpring = false;
					bool growsInSummer = false;
					bool growsInAutumn = false;
					bool growsInWinter = false;
					bool growsInDry = false;
					bool growsInWet = false;

					//get the climate first
					switch (row.Fields [(int)PField._Climate].ToLower ().Trim ()) {
					case "temparate":
					default:
						plant.Climate = ClimateType.Temperate;
						break;

					case "wetland":
						plant.Climate = ClimateType.Wetland;
						break;

					case "tropical coast":
						plant.Climate = ClimateType.TropicalCoast;
						break;

					case "desert":
						plant.Climate = ClimateType.Desert;
						break;

					case "rainforest":
						plant.Climate = ClimateType.Rainforest;
						break;

					case "arctic":
						plant.Climate = ClimateType.Arctic;
						break;
					}

					//use climate to determine what kind of seasons to use
					growsInSpring = row.Fields [(int)PField._Spring].Contains ("Yes");
					growsInSummer = row.Fields [(int)PField._Summer].Contains ("Yes");
					growsInAutumn = row.Fields [(int)PField._Fall].Contains ("Yes");
					growsInWinter = row.Fields [(int)PField._Winter].Contains ("Yes");

					//now do each season
					if (growsInSpring) {
						Spring = new PlantSeasonalSettings ();
						Spring.Seasonality = TimeOfYear.SeasonSpring;
						plant.SeasonalSettings.Add (Spring);
						ImportPlantSeason (
														Spring,
														plant.HasFlowers,
														row.Fields [(int)PField._Height_Spr],
														row.Fields [(int)PField._Color_Spr],
														row.Fields [(int)PField._Flowers_in_Spr],
														row.Fields [(int)PField._Flower_Size_Spring],
														row.Fields [(int)PField._Flower_Density_Spring],
														row.Fields [(int)PField._Flower_Color_Spring]);
					}

					if (growsInSummer) {
						Summer = new PlantSeasonalSettings ();
						Summer.Seasonality = TimeOfYear.SeasonSummer;
						plant.SeasonalSettings.Add (Summer);
						ImportPlantSeason (
														Summer,
														plant.HasFlowers,
														row.Fields [(int)PField._Height_Sum],
														row.Fields [(int)PField._Color_Sum],
														row.Fields [(int)PField._Flowers_in_Sum],
														row.Fields [(int)PField._Flower_Size_Summer],
														row.Fields [(int)PField._Flower_Density_Summer],
														row.Fields [(int)PField._Flower_Color_Summer]);
					}

					if (growsInAutumn) {
						Autumn = new PlantSeasonalSettings ();
						Autumn.Seasonality = TimeOfYear.SeasonAutumn;
						plant.SeasonalSettings.Add (Autumn);
						ImportPlantSeason (
														Autumn,
														plant.HasFlowers,
														row.Fields [(int)PField._Height_Fall],
														row.Fields [(int)PField._Color_Fall],
														row.Fields [(int)PField._Flowers_in_Fall],
														row.Fields [(int)PField._Flower_Size_Fall],
														row.Fields [(int)PField._Flower_Density_Fall],
														row.Fields [(int)PField._Flower_Color_Fall]);
					}

					if (growsInWinter) {
						Winter = new PlantSeasonalSettings ();
						Winter.Seasonality = TimeOfYear.SeasonWinter;
						plant.SeasonalSettings.Add (Winter);
						ImportPlantSeason (
														Winter,
														plant.HasFlowers,
														row.Fields [(int)PField._Height_Win],
														row.Fields [(int)PField._Color_Win],
														row.Fields [(int)PField._Flowers_in_Win],
														row.Fields [(int)PField._Flower_Size_Winter],
														row.Fields [(int)PField._Flower_Density_Winter],
														row.Fields [(int)PField._Flower_Color_Winter]);
					}

					if (plant.SeasonalSettings.Count == 0) {
						Debug.Log ("PLANT SEASONAL SETTINGS WAS ZERO IN  " + plant.Name + ":1 - ");
					}
					#endregion

					#region food stuff

					/*
					49 - Edibility (Raw)
					50 - How Filling (Raw)
					51 - Other Properties (Raw)
					52 - How Poisonous (Raw)
					53 - Poison Duration (Raw)
					54 - How Hallucinogenic (Raw)
					55 - Medicine Strength (Raw)
					56 - Edibility (Cooked)
					57 - How Filling (Cooked)
					58 - Other Properties (Cooked)
					59 - How Poisonous (Cooked)
					60 - Poison Duration (Cooked)
					61 - How Hallucinogenic (Cooked)
					62 - Medicine Strength (Cooked)
					*/

					ImportPlantFoodstuffProps (
												RawProps,
												row.Fields [(int)PField._Edibility_Raw],
												row.Fields [(int)PField._How_Filling_Raw],
												row.Fields [(int)PField._Other_Properties_Raw],
												row.Fields [(int)PField._How_Poisonous_Raw],
												row.Fields [(int)PField._Poison_Duration_Raw],
												row.Fields [(int)PField._How_Hallucinogenic_Raw],
												row.Fields [(int)PField._Med_Strength_Raw]);

					ImportPlantFoodstuffProps (
												CookedProps,
												row.Fields [(int)PField._Edibility_Cooked],
												row.Fields [(int)PField._How_Filling_Cooked],
												row.Fields [(int)PField._Other_Properties_Cooked],
												row.Fields [(int)PField._How_Poisonous_Cooked],
												row.Fields [(int)PField._Poison_Duration_Cooked],
												row.Fields [(int)PField._How_Hallucinogenic_Cooked],
												row.Fields [(int)PField._Medicine_Strength_Cooked]);

					#endregion

					Mods.Get.Editor.SaveMod <Plant> (plant, "Plant", plant.Name);
				}

			}
		}

		protected void ImportPlantFoodstuffProps (
						FoodStuffProps props,
						string edibile,
						string howFilling,
						string otherProperties,
						string howPoisonous,
						string poisonDuration,
						string howHallucinogenic,
						string medicineStrength)
		{
			if (edibile.Contains ("Inedible")) {
				//can't eat it! we're done here
				props.Type = FoodStuffEdibleType.None;
				return;
			}

			props.HungerRestore = PlayerStatusRestore.A_None;
			int howFillingInt = 0;
			Int32.TryParse (howFilling, out howFillingInt);
			switch (howFillingInt) {
			case 0:
			case 1:
			default:
				props.HungerRestore = PlayerStatusRestore.B_OneFifth;
				break;
			case 2:
				props.HungerRestore = PlayerStatusRestore.C_TwoFifths;
				break;
			case 3:
				props.HungerRestore = PlayerStatusRestore.D_ThreeFifths;
				break;
			case 4:
				props.HungerRestore = PlayerStatusRestore.E_FourFifths;
				break;
			case 5:
				props.HungerRestore = PlayerStatusRestore.F_Full;
				break;
			}

			bool poisonous = otherProperties.Contains ("Poisonous");
			bool medicinal = otherProperties.Contains ("Medicinal");
			bool hallucinogenic = otherProperties.Contains ("Hallucinogenic");

			string conditionName = string.Empty;
			/*
			 * mild 1 hour - MildFoodPoisoning
			 * mild half day - MildListeria
			 * mild day - MildNorwalkVirus
			 * mild week - MildStaph
			 *
			 * moderate 1 hour - ModerateFoodPoisoning
			 * moderate half day - ModerateListeria
			 * moderate day - ModerateNorwalkVirus
			 * moderate week - ModerateStaph
			 *
			 * severe 1 hour - SevereFoodPoisoning
			 * severe half day - SevereListeria
			 * severe day - Botulusm
			 * severe week - Cholera
			*/
			props.ConditionName = string.Empty;
			props.ConditionChance = UnityEngine.Random.Range (0.75f, 1f);

			if (poisonous) {
				switch (howPoisonous.ToLower ().Trim ()) {
				case "mild":
				default:
					switch (poisonDuration.ToLower ().Trim ()) {
					case "1 hour":
					default:
						props.ConditionName = "MildPlantPoisonShort";
						break;

					case "half day":
						props.ConditionName = "MildPlantPoison";
						break;

					case "day":
						props.ConditionName = "MildPlantPoisonLong";
						break;

					case "week":
						props.ConditionName = "MildPlantPoisonReallyLong";
						break;
					}
					break;

				case "moderate":
					switch (poisonDuration.ToLower ().Trim ()) {
					case "1 hour":
					default:
						props.ConditionName = "ModeratePlantPoisonShort";
						break;

					case "half day":
						props.ConditionName = "ModeratePlantPoison";
						break;

					case "day":
						props.ConditionName = "ModeratePlantPoisonLong";
						break;

					case "week":
						props.ConditionName = "ModeratePlantPoisonReallyLong";
						break;
					}
					break;

				case "severe":
					switch (poisonDuration.ToLower ().Trim ()) {
					case "1 hour":
					default:
						props.ConditionName = "SeverePlantPoisonShort";
						break;

					case "half day":
						props.ConditionName = "SeverePlantPoison";
						break;

					case "day":
						props.ConditionName = "SeverePlantPoisonLong";
						break;

					case "week":
						props.ConditionName = "SeverePlantPoisonReallyLong";
						break;
					}
					break;
				}
			}
			props.HealthRestore = PlayerStatusRestore.A_None;
			if (medicinal) {
				int medStrengthInt = Int32.Parse (medicineStrength);
				switch (medStrengthInt) {
				case 0:
				case 1:
				default:
					props.HealthRestore = PlayerStatusRestore.B_OneFifth;
					break;
				case 2:
					props.HealthRestore = PlayerStatusRestore.C_TwoFifths;
					break;
				case 3:
					props.HealthRestore = PlayerStatusRestore.D_ThreeFifths;
					break;
				case 4:
					props.HealthRestore = PlayerStatusRestore.E_FourFifths;
					break;
				case 5:
					props.HealthRestore = PlayerStatusRestore.F_Full;
					break;
				}
			}
		}

		protected void ImportPlantSeason (
						PlantSeasonalSettings settings,
						bool hasFlowers,
						string plantHeight,
						string plantColor,
						string flowers,
						string flowerSize,
						string flowerDensity,
						string flowerColor)
		{
			switch (plantHeight.ToLower ().Trim ()) {
			case "extra short":
				settings.BodyHeight = PlantBodyHeight.ExtraShort;
				break;

			case "short":
				settings.BodyHeight = PlantBodyHeight.Short;
				break;

			case "medium":
			default:
				settings.BodyHeight = PlantBodyHeight.Medium;
				break;

			case "tall":
				settings.BodyHeight = PlantBodyHeight.Tall;
				break;

			case "extra tall":
				settings.BodyHeight = PlantBodyHeight.ExtraTall;
				break;
			}

			int bodyColorInt = 127;
			if (Int32.TryParse (plantColor, out bodyColorInt)) {
				bodyColorInt -= 127;
			}
			settings.FlowerHueShift = ((float)bodyColorInt) / 127f;

			//if the 'NOT' is in there then it doesn't flower
			settings.Flowers = (!flowers.Contains ("NOT") && hasFlowers);

			if (settings.Flowers) {
				switch (flowerSize.ToLower ().Trim ()) {
				case "tiny (1 cm)":
					settings.FlowerSize = PlantFlowerSize.Tiny;
					break;

				case "small":
					settings.FlowerSize = PlantFlowerSize.Small;
					break;

				case "medium":
				default:
					settings.FlowerSize = PlantFlowerSize.Medium;
					break;

				case "large":
					settings.FlowerSize = PlantFlowerSize.Large;
					break;

				case "giant (20 cm)":
					settings.FlowerSize = PlantFlowerSize.Giant;
					break;
				}

				int densityInt = 100;
				Int32.TryParse (flowerDensity, out densityInt);
				settings.FlowerDensity = ((float)densityInt) / 100f;

				//same as body hue shift
				int flowerColorInt = 127;
				if (Int32.TryParse (flowerColor, out flowerColorInt)) {
					flowerColorInt -= 127;
				}
				settings.FlowerHueShift = ((float)flowerColorInt) / 127f;
			}
		}

		public void ImportPlant (KeyValuePair <string,string> plant)
		{

		}

				#endregion

				#region blueprint

		public string BlueprintSpecialOutput;

		protected void ImportBlueprint (KeyValuePair <string,string> asset)
		{
			WIBlueprint blueprint = new WIBlueprint ();
			blueprint.Clear (true);
			blueprint.Name = asset.Key;//use the file name for the name
			//the title will be the Name value

			string varLines = asset.Value;
			string[] splitDescription = asset.Value.Split (new string [] { "[DescriptionStart]" }, StringSplitOptions.RemoveEmptyEntries);
			if (splitDescription.Length > 1) {
				varLines = splitDescription [0];
				blueprint.Description = splitDescription [1];
			}

			string[] splitLines = varLines.Split (new string [] {
								"\n",
								"\n\r",
								"\r\n"
						}, StringSplitOptions.RemoveEmptyEntries);

			foreach (string splitLine in splitLines) {
				if (splitLine.StartsWith ("#")) {
					if (splitLine.Contains ("CustomResult")) {
						string customResultLine = splitLine.Replace ("#set CustomResult=", "");
						ConvertCustomResultToScriptState (blueprint, customResultLine);
					} else if (splitLine.Contains ("GenericResult")) {
						string genericResultLine = splitLine.Replace ("#set GenericResult=", "");
						blueprint.GenericResult = ConvertRowSpotToGenericWorldItem (genericResultLine);
						blueprint.UseGenericResult = true;
					} else {
						string[] splitVar = splitLine.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
						string varName = splitVar [0].Replace ("#set ", "").Trim ();
						if (splitVar.Length == 1) {
							////Debug.Log ("Weird, split var " + varName + " doesn't have a value. Raw output:\n" + splitLine);
						} else {
							string varVal = splitVar [1].Trim ();
							//handle the tricky ones ourselves
							switch (varName) {
							case "Row1":
								Debug.Log ("Row 1 for " + varName);
								ConvertBlueprintRowToGenericWorlditems (blueprint, varVal, 1);
								break;
							case "Row2":
								Debug.Log ("Row 2 for " + varName);
								ConvertBlueprintRowToGenericWorlditems (blueprint, varVal, 2);
								break;
							case "Row3":
								Debug.Log ("Row 3 for " + varName);
								ConvertBlueprintRowToGenericWorlditems (blueprint, varVal, 3);
								break;

							default:
								SetScriptVar (blueprint, blueprint.GetType (), varName, varVal);
								break;
							}
						}
					}
				}
			}

			//done and done
			Mods.Get.Editor.SaveMod <WIBlueprint> (blueprint, "Blueprint", blueprint.Name);
		}

		protected void ConvertCustomResultToScriptState (WIBlueprint blueprint, string customResult)
		{
			//get rid of parenthesis
			customResult = customResult.Replace (")", "").Replace ("(", "");
			//figure out which script to create
			string[] splitResult = customResult.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
			System.Object customResultState = null;
			Type stateObjectType = null;
			List <KeyValuePair <string,string>> varValPairs = new List<KeyValuePair<string, string>> ();
			foreach (string resultVar in splitResult) {
				Debug.Log ("Checking " + resultVar + " in blueprint " + customResult);
				string[] splitVar = resultVar.Split (new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
				string varName = splitVar [0];
				string varVal = splitVar [1];
				if (varName == "Script") {
					//this is the custom script we'll be using
					//depending on the type, we'll be using different custom world item bases
					blueprint.CustomResultScript = varVal;
					//create the custom result state
					//all state class names for WIScripts have 'State' at the end
					string stateObjectTypeName = varVal + "State";
					stateObjectType = Type.GetType (stateObjectTypeName);
					if (stateObjectType == null) {
						Debug.Log ("Had trouble getting type with string " + stateObjectTypeName);
					} else {
						customResultState = Activator.CreateInstance (stateObjectType);
					}
				} else {
					varValPairs.Add (new KeyValuePair<string, string> (varName, varVal));
				}
			}
			if (customResultState != null) {
				foreach (KeyValuePair <string, string> varValPair in varValPairs) {
					SetScriptVar (customResultState, stateObjectType, varValPair.Key, varValPair.Value);
				}
				//finally, serialize the object's state to the blueprint
				blueprint.CustomResultScriptState = WIScript.XmlSerializeToString (customResultState);
			} else {
				Debug.Log ("Couldn't create custom result state in blueprint");
			}
		}

		protected void ConvertBlueprintRowToGenericWorlditems (WIBlueprint blueprint, string row, int rowNumber)
		{
			string[] splitRow = row.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
			List <GenericWorldItem> blueprintRow = null;
			switch (rowNumber) {
			case 1:
			default:
				blueprintRow = blueprint.Row1;
				break;

			case 2:
				blueprintRow = blueprint.Row2;
				break;

			case 3:
				blueprintRow = blueprint.Row3;
				break;
			}

			blueprintRow [0] = ConvertRowSpotToGenericWorldItem (splitRow [0]);
			blueprintRow [1] = ConvertRowSpotToGenericWorldItem (splitRow [1]);
			blueprintRow [2] = ConvertRowSpotToGenericWorldItem (splitRow [2]);
			//the length of splitSpot1 will tell us a lot
		}

		protected GenericWorldItem ConvertRowSpotToGenericWorldItem (string rowSpot)
		{
			if (rowSpot == "[Empty]") {
				return GenericWorldItem.Empty;
			}
			GenericWorldItem newItem = new GenericWorldItem ();
			string[] splitRowSpot = rowSpot.Split (new string [] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			switch (splitRowSpot [0]) {
			case "Plant":
				newItem.PackName = "Plants";
				newItem.PrefabName = "WorldPlant";
				newItem.StackName = splitRowSpot [1].Replace ("_", " ");
				newItem.Subcategory = splitRowSpot [1].Replace ("_", " ");
				Debug.Log ("PLANT: " + rowSpot);
				break;

			case "Special":
				newItem.PackName = "Special";
				newItem.PrefabName = splitRowSpot [1].Replace ("_", " ");
				newItem.StackName = splitRowSpot [1].Replace ("_", " ");
				Debug.Log ("SPECIAL: " + rowSpot);
				//TEMP
				break;

			case "Generic":
				////Debug.Log ("Row spot is generic: " + rowSpot);
				newItem.PackName = splitRowSpot [1].Replace ("_", " ");
				newItem.PrefabName = splitRowSpot [2].Replace ("_", " ");
				newItem.StackName = WorldItems.CleanWorldItemName (splitRowSpot [3].Replace ("_", " "));
				newItem.State = splitRowSpot [4].Replace ("_", " ");
				if (splitRowSpot.Length > 5) {
					newItem.Subcategory = splitRowSpot [5].Replace ("_", " ");
				}
				break;


			default:
				Debug.Log ("Couldn't figure out what row spot was: " + rowSpot);
				break;
			}
			return newItem;
		}

		protected void MassImportBlueprint (string path)
		{
			BlueprintSpecialOutput = string.Empty;
			RefreshSubstitutions ();
			var MyFile = new CSVFile (path);
			/*
			=======================
			0 - Submission Date
			1 - First Name
			2 - Last Name
			3 - E-mail
			4 - Grid Spot 1
			5 - Grid Spot 2
			6 - Grid Spot 3
			7 - Grid Spot 4
			8 - Grid Spot 5
			9 - Grid Spot 6
			10 - Grid Spot 7
			11 - Grid Spot 8
			12 - Grid Spot 9
			13 - Click to edit...
			14 - Special Ingredient Request:
			15 - I want my Special Request to REPLACE the item in:
			16 - Click to edit...
			17 - For Botanists Only:
			18 - Name of Plant
			19 - Recipe Preparation
			20 - How many in-game hours does your meal take to cook?  (1 in-game hour = 1 real-time minute)
			21 - How Filling is Your Meal?
			22 - Recipe Rarity (Recipes have to be found and read before they can be used)
			23 - Recipe Name
			24 - Recipe Description (optional) ASCII Characters Only
			25 - Food Presentation
			26 - Click to edit...
			27 - Click to edit...
			28 - Click to edit...
			29 - Food Appearance
			30 - Baked Goods - Select a Model
			31 - Choose a Color for your Topping
			32 - Choose a Base Texture
			33 - Do you want 2 additional chunks in your meal?
			34 - Choose the Color of the Chunks
			35 - Do you want your Base texture Mounded or Flat?
			36 - Notes (Optional)
			-----additional fields-----
			37 - Liquid
			38 - Substitutions
			39 - Plant Name
			40 - Cooked plant
			=======================
			*/
//			List <string> fields = new List<string> ();
//			fields.Add ("=======================");
//			var fieldRows = MyFile.Rows [0];
//			for (int i = 0; i < fieldRows.Fields.Count; i++) {
//				fields.Add (i.ToString ( ) + " - " + fieldRows.Fields [i]);
//			}
//			fields.Add ("=======================");
//			////Debug.Log (fields.JoinToString ("\n"));

			for (int rowNum = 1; rowNum < MyFile.Rows.Count; rowNum++) {
				List <string> newBlueprint = new List <string> ();
				//get the data for each one of the book fields
				var row = MyFile.Rows [rowNum];

				newBlueprint.Add ("#set Title=" + row.Fields [23]);
				newBlueprint.Add ("#set RequiredSkill=PrepareFood");
				newBlueprint.Add ("#set UseGenericResult=True");
				newBlueprint.Add ("#set Rarity=" + StringOrAlt (row.Fields [22], "Common"));
				newBlueprint.Add ("#set BaseCraftTime=" + StringOrAlt (row.Fields [20], "1"));

				bool usesSpecialIngredient = !string.IsNullOrEmpty (row.Fields [14]) && !string.IsNullOrEmpty (row.Fields [15]);
				bool usesBotanistPlant = !string.IsNullOrEmpty (row.Fields [17]) && !string.IsNullOrEmpty (row.Fields [18]);

				if (!usesSpecialIngredient && !usesBotanistPlant) {
					//get the generic result
					newBlueprint.Add ("#set Row1=" + GetGenericBlueprintRow (row.Fields [4], row.Fields [5], row.Fields [6]));
					newBlueprint.Add ("#set Row2=" + GetGenericBlueprintRow (row.Fields [7], row.Fields [8], row.Fields [9]));
					newBlueprint.Add ("#set Row3=" + GetGenericBlueprintRow (row.Fields [10], row.Fields [11], row.Fields [12]));
				} else {
					Debug.Log ("Special ingredient in " + row.Fields [23]);
					//otherwise we've got to work this out...
					int specialIngredientIndex = -1;
					if (usesSpecialIngredient) {
						string spotString = row.Fields [15].ToLower ().Trim ().Replace ("grid spot ", "");
						if (!Int32.TryParse (spotString, out specialIngredientIndex)) {
							specialIngredientIndex = -1;
						}
					}
					List <string> spots = new List <string> ();
					int currentField = 4;
					List <int> emptyIndexes = new List<int> ();
					for (int i = 0; i < 9; i++) {
						string currentSpot = row.Fields [currentField];

						string substitution = string.Empty;
						if (RecipeImportSubstitutions.TryGetValue (currentSpot.Trim (), out substitution)) {
							Debug.Log ("Substituting " + substitution + " for " + currentSpot);
							currentSpot = substitution;
						} else {
							Debug.Log ("Required no substitution for " + currentSpot);
						}

						if (string.IsNullOrEmpty (currentSpot) || currentSpot.ToLower ().Contains ("empty")) {
							emptyIndexes.Add (i);
							currentSpot = "[Empty]";
						} else {
							string spotState = "Default";
							currentSpot = GetBlueprintSpotState (currentSpot, ref spotState);
							if (currentSpot != "[Empty]") {
								currentSpot = "Generic|Edibles|" + currentSpot + "|" + currentSpot + "|" + spotState;
							}
						}
						spots.Add (currentSpot);
						currentField++;
					}
					//substitute the plant for a random empty spot
					if (usesBotanistPlant && emptyIndexes.Count > 0) {
						int randomEmptyIndex = emptyIndexes [UnityEngine.Random.Range (0, emptyIndexes.Count)];
						string plantName = row.Fields [39];
						plantName = plantName.Replace (" ", "").Trim ();
						plantName = plantName.Replace ("'", "");
						plantName = PathSanitizer.SanitizeFilename (plantName, '_');
						string plantState = "Raw";
						if (row.Fields [40].Contains ("y")) {
							plantState = "Cooked";
						}
						//Generic|PackName|PrefabName|StackName|State|Subcategory|DisplayName
						spots [randomEmptyIndex] = "Generic|Plants|WorldPlant|" + plantName + "|" + plantState + "|" + plantName;
						BlueprintSpecialOutput += ("\n " + row.Fields [23] + " Plant: " + plantName);
					}
					//create the rows
					newBlueprint.Add ("#set Row1=" + spots [0] + "," + spots [1] + "," + spots [2]);
					newBlueprint.Add ("#set Row2=" + spots [3] + "," + spots [4] + "," + spots [5]);
					newBlueprint.Add ("#set Row3=" + spots [6] + "," + spots [7] + "," + spots [8]);
				}
				//gets a stack item
				string foodPresentation = row.Fields [25];
				string foodAppearance = row.Fields [29];
				string bakedGoods = row.Fields [30];
				string toppingColor = row.Fields [31];
				string baseTexture = row.Fields [32];
				if (!string.IsNullOrEmpty (baseTexture)) {
					baseTexture = baseTexture.Substring (row.Fields [32].Length - 1, 1);
				}
				string additionalChunks = row.Fields [33];
				string chunkColor = row.Fields [34];
				string mounded = row.Fields [35];
				string howFilling = row.Fields [21];
				string cookedOrCold = row.Fields [19];
				string cookLength = row.Fields [20];

				string blueprintName = row.Fields [23].Replace (" ", "");//TEMP
				blueprintName = blueprintName.Replace ("'", "");
				blueprintName = blueprintName.Replace ("\"", "");
				blueprintName = blueprintName.Replace (",", "");
				blueprintName = blueprintName.Replace (".", "");
				if (string.IsNullOrEmpty (blueprintName)) {
					blueprintName = "UnnamedRecipe" + rowNum.ToString ();
				}
				blueprintName = PathSanitizer.SanitizeFilename (blueprintName, '_');

				string recipeResult = GetBlueprintResult (
										                  foodPresentation,
										                  foodAppearance,
										                  bakedGoods,
										                  toppingColor,
										                  baseTexture,
										                  additionalChunks,
										                  chunkColor,
										                  mounded,
										                  howFilling,
										                  cookedOrCold,
										                  blueprintName);
				newBlueprint.Add ("#set GenericResult=" + recipeResult);

				if (!string.IsNullOrEmpty (row.Fields [24])) {
					newBlueprint.Add ("[DescriptionStart]");
					newBlueprint.Add (row.Fields [24]);
				}

				string finalBlueprint = newBlueprint.JoinToString ("\n");

				WriteMassImportAsset ("Blueprint", blueprintName + ".txt", finalBlueprint);

				//create a new prepared food
				PreparedFood preparedFood = new PreparedFood ();
				preparedFood.Name = blueprintName;
				preparedFood.BaseTextureName = baseTexture;
				int bakedGoodsIndex = 0;
				Int32.TryParse (bakedGoods, out bakedGoodsIndex);
				preparedFood.BakedGoodsIndex = bakedGoodsIndex;
				preparedFood.BakedGoodsToppings = !additionalChunks.Contains ("No");
				preparedFood.CanBeRaw = !cookedOrCold.Contains ("Cold");

				preparedFood.HungerRestore = PlayerStatusRestore.A_None;
				int howFillingInt = 0;
				Int32.TryParse (howFilling, out howFillingInt);
				switch (howFillingInt) {
				case 0:
				case 1:
				default:
					preparedFood.HungerRestore = PlayerStatusRestore.B_OneFifth;
					break;
				case 2:
					preparedFood.HungerRestore = PlayerStatusRestore.C_TwoFifths;
					break;
				case 3:
					preparedFood.HungerRestore = PlayerStatusRestore.D_ThreeFifths;
					break;
				case 4:
					preparedFood.HungerRestore = PlayerStatusRestore.E_FourFifths;
					break;
				case 5:
					preparedFood.HungerRestore = PlayerStatusRestore.F_Full;
					break;
				}

				float cookDuration = 0f;
				float.TryParse (cookLength, out cookDuration);
				preparedFood.RTCookDuration = (float)WorldClock.GameHoursToRTSeconds (cookDuration);
				if (foodPresentation.Contains ("Bowl")) {
					preparedFood.FoodType = PreparedFoodType.PlateOrBowl;
					if (!string.IsNullOrEmpty (chunkColor)) {
						preparedFood.ToppingColor = Colors.HexToColor (chunkColor.Replace ("#", ""));
					}
					if (mounded.Contains ("Mounded")) {
						if (additionalChunks.Contains ("No")) {
							preparedFood.FoodStyle = PreparedFoodStyle.BowlMound;		
						} else {
							preparedFood.FoodStyle = PreparedFoodStyle.BowlMoundToppings;
						}
					} else {
						if (additionalChunks.Contains ("No")) {
							preparedFood.FoodStyle = PreparedFoodStyle.BowlFlat;		
						} else {
							preparedFood.FoodStyle = PreparedFoodStyle.BowlFlatToppings;
						}
					}
				} else if (foodPresentation.Contains ("Plate")) {
					preparedFood.FoodType = PreparedFoodType.PlateOrBowl;
					if (!string.IsNullOrEmpty (chunkColor)) {
						preparedFood.ToppingColor = Colors.HexToColor (chunkColor.Replace ("#", ""));
					}
					if (foodAppearance.Contains ("Compilation")) {
						preparedFood.FoodStyle = PreparedFoodStyle.PlateIngredients;
					} else if (additionalChunks.Contains ("No")) {
						preparedFood.FoodStyle = PreparedFoodStyle.PlateMound;		
					} else {
						preparedFood.FoodStyle = PreparedFoodStyle.PlateMoundToppings;
					}
				} else if (foodPresentation.Contains ("Baked")) {
					preparedFood.FoodType = PreparedFoodType.BakedGoods;
					if (!string.IsNullOrEmpty (toppingColor)) {
						preparedFood.ToppingColor = Colors.HexToColor (toppingColor.Replace ("#", ""));
					}
				}
				Mods.Get.Editor.SaveMod <PreparedFood> (preparedFood, "PreparedFood", preparedFood.Name);
			}

			Debug.Log (BlueprintSpecialOutput);
		}

		public static string StringOrAlt (string s, string alt)
		{
			if (string.IsNullOrEmpty (s)) {
				return alt;
			}
			return s;
		}

		public string GetGenericBlueprintRow (
						string spot1,
						string spot2,
						string spot3)
		{
			//PackName|PrefabName|StackName
			string spot1State = "Default";
			string spot2State = "Default";
			string spot3State = "Default";
			spot1 = GetBlueprintSpotState (spot1, ref spot1State);
			spot2 = GetBlueprintSpotState (spot2, ref spot2State);
			spot3 = GetBlueprintSpotState (spot3, ref spot3State);

			if (spot1 != "[Empty]") {
				spot1 = "Generic|Edibles|" + spot1 + "|" + spot1 + "|" + spot1State;
			}
			if (spot2 != "[Empty]") {
				spot2 = "Generic|Edibles|" + spot2 + "|" + spot2 + "|" + spot2State;
			}
			if (spot3 != "[Empty]") {
				spot3 = "Generic|Edibles|" + spot3 + "|" + spot3 + "|" + spot3State;
			}
			return spot1 + "," + spot2 + "," + spot3;
		}

		protected string GetBlueprintSpotState (string spot, ref string state)
		{
			if (string.IsNullOrEmpty (spot)) {
				return "[Empty]";
			}
			string substitution = string.Empty;
			if (RecipeImportSubstitutions.TryGetValue (spot, out substitution)) {
				spot = substitution;
			}
			//get rid of the r / u / c
			spot = spot.Replace ("(r)", "");
			spot = spot.Replace ("(u)", "");
			spot = spot.Replace ("(c)", "");
			spot = spot.Replace ("|", "");
			//special handler for mushroom items and bread items
			//the formatting on these is weird so we'll just brute force it
			if (spot.Contains (",")) {
				string[] splitSpot = spot.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
				spot = splitSpot [0].Trim ().Replace ("_", "");
				state = splitSpot [1].Trim ().Replace ("_", "");
				spot = CultureInfo.CurrentCulture.TextInfo.ToTitleCase (spot.ToLower ());
				//not sure if title case can handle parantheticals, so doing this just in case
				state = CultureInfo.CurrentCulture.TextInfo.ToTitleCase (state.ToLower ());
			} else {
				//fix capitalization (known to be formatted wrong)
				spot = spot.Trim ().Replace ("_", "");
				spot = CultureInfo.CurrentCulture.TextInfo.ToTitleCase (spot.ToLower ());
			}
			spot = spot.Replace (",", "");
			//get rid of spaces
			spot = spot.Replace (" ", "_");
			return spot;
		}

		public string GetBlueprintResult (
						string foodPresentation,
						string foodAppearance,
						string bakedGoods,
						string toppingColor,
						string baseTexture,
						string additionalChunks,
						string chunkColor,
						string mounded,
						string howFilling,
						string cookedOrCold,
						string preparedFoodName)
		{
			return "Generic|PreparedFoods|PreparedFood|Default|Default|" + preparedFoodName;
//			List <string> blueprintResult = new List<string> ();
//
//			switch (foodPresentation.ToLower ().Trim ()) {
//			case "bowl of food":
//			default:
//				blueprintResult.Add ("Script=PreparedFood");
//				switch (foodAppearance.ToLower ().Trim ()) {
//				case "custom option":
//				default:
//					if (!additionalChunks.ToLower ().Contains ("No")) {
//						if (mounded.Contains ("Flat")) {
//							blueprintResult.Add ("Style=BowlFlatToppings");
//						} else {
//							blueprintResult.Add ("Style=BowlMoundToppings");
//						}
//					} else {
//						if (mounded.Contains ("Flat")) {
//							blueprintResult.Add ("Style=BowlFlat");
//						} else {
//							blueprintResult.Add ("Style=BowlFlatToppings");
//						}
//					}
//					break;
//
//				case "compilation of ingredients":
//					blueprintResult.Add ("Style=BowlIngredients");
//					break;
//				}
//				break;
//
//			case "plate of food":
//				blueprintResult.Add ("Script=PreparedFood");
//				switch (foodAppearance.ToLower ().Trim ()) {
//				case "custom option":
//				default:
//					if (!additionalChunks.ToLower ().Contains ("No")) {
//						blueprintResult.Add ("Style=PlateMoundToppings");
//					} else {
//						blueprintResult.Add ("Style=PlateMound");
//					}
//					break;
//
//				case "compilation of ingredients":
//					blueprintResult.Add ("Style=PlateIngredients");
//					break;
//				}
//				break;
//
//			case "baked good":
//				blueprintResult.Add ("Script=BakedGood");
//				int bakedGoodTypeInt = 0;
//				Int32.TryParse (bakedGoods.ToLower ().Trim (), out bakedGoodTypeInt);
//				switch (bakedGoodTypeInt) {
//				case 1:
//				default:
//					blueprintResult.Add ("Style=LoafOfBread");
//					break;
//
//				case 2:
//					blueprintResult.Add ("Style=FrostedCake");
//					break;
//
//				case 3:
//					blueprintResult.Add ("Style=Cheesecake");
//					break;
//
//				case 4:
//					blueprintResult.Add ("Style=Cookie");
//					break;
//
//				case 5:
//					blueprintResult.Add ("Style=Pie");
//					break;
//
//				case 6:
//					blueprintResult.Add ("Style=FrostedCakeWithToppings");
//					break;
//
//				case 7:
//					blueprintResult.Add ("Style=CheesecakeWithToppings");
//					break;
//				}
//				break;
//
//			}
//			switch (baseTexture.Trim ().ToLower ()) {
//			case "a":
//			default:
//				blueprintResult.Add ("BaseTexture=BakedBeans");
//				break;
//
//			case "b":
//				blueprintResult.Add ("BaseTexture=BeefStew");
//				break;
//
//			case "c":
//				blueprintResult.Add ("BaseTexture=ChickenCasserole");
//				break;
//
//			case "d":
//				blueprintResult.Add ("BaseTexture=ChickenStew");
//				break;
//
//			case "e":
//				blueprintResult.Add ("BaseTexture=ChickpeaBroccoli");
//				break;
//
//			case "f":
//				blueprintResult.Add ("BaseTexture=ChopSuey");
//				break;
//
//			case "g":
//				blueprintResult.Add ("BaseTexture=EggNoodles");
//				break;
//
//			case "h":
//				blueprintResult.Add ("BaseTexture=FriedRice");
//				break;
//
//			case "i":
//				blueprintResult.Add ("BaseTexture=PanFriedPotatoes");
//				break;
//
//			case "j":
//				blueprintResult.Add ("BaseTexture=PeaSoup");
//				break;
//
//			case "k":
//				blueprintResult.Add ("BaseTexture=Raspberries");
//				break;
//
//			case "l":
//				blueprintResult.Add ("BaseTexture=RedBeanChili");
//				break;
//
//			case "m":
//				blueprintResult.Add ("BaseTexture=Salad");
//				break;
//
//			case "n":
//				blueprintResult.Add ("BaseTexture=TunaCasserole");
//				break;
//
//			case "o":
//				blueprintResult.Add ("BaseTexture=Peas");
//				break;
//			}
//			if (!string.IsNullOrEmpty (toppingColor)) {
//				//replace the default / unfilled value with white
//				toppingColor = toppingColor.Replace ("#123456", "#ffffff");
//				blueprintResult.Add ("ToppingColor=" + toppingColor.Trim ());
//			}
//			int howFillingInt = 0;
//			Int32.TryParse (howFilling, out howFillingInt);
//			blueprintResult.Add ("HungerRestore=" + howFillingInt.ToString ());
//
//			if (cookedOrCold.Contains ("Cooked")) {
//				//this recipe will have a raw and cooked state
//				blueprintResult.Add ("CanBeRaw=True");
//			}
//
//			return blueprintResult.JoinToString (",");
		}

				#endregion

		public static string GetNameFromDialogName (string fileName)
		{
			fileName = fileName.Replace (".dlg", "");
			fileName = fileName.Replace (".xml", "");
			fileName = fileName.Replace ("_", " ");
			fileName = fileName.Replace ("-", " ");
			fileName = fileName.ToLower ();
			//now we're ready to capitalize all first letters
			TextInfo textInfo = new CultureInfo ("en-US", false).TextInfo;
			fileName = textInfo.ToTitleCase (fileName);
			//fix the number at the end
			Regex regex = new Regex (@"(\d+)$",
								           RegexOptions.CultureInvariant);
			Match match = regex.Match (fileName);
			if (!match.Success) {
				//if we didn't find a number
				//add one
				fileName += "1";
			}
			//this is a cheat - fix any 'acts' in there

			//add leading zeros to any numbers that already exist
			string withLeading = Regex.Replace (fileName, @"\d+", m => m.Value.PadLeft (2, '0'));
			//add spaces between everything
			string withSpaces = Regex.Replace (withLeading, @"(?!^)(?:[A-Z](?:[a-z]+|(?:[A-Z\d](?![a-z]))*)|\d+)", " $0");
			//replace spaces (including earlier spaces) with dashes
			string trimmedSpaces = Regex.Replace (withSpaces, @"\s+", " ");
			fileName = trimmedSpaces.Replace (" ", "-");
			return fileName;
		}

		protected void SetScriptVar (System.Object obj, Type type, string varName, string varVal)
		{
			System.Reflection.FieldInfo fieldInfo = type.GetField (varName);
			if (fieldInfo != null) {
				System.ComponentModel.StringConverter converter = new System.ComponentModel.StringConverter ();
				Type fieldType = fieldInfo.FieldType;
				System.Object convertedValue = null;

				switch (fieldType.Name) {	//custom for custom enums
				case "Int32":
					convertedValue = (int)Int32.Parse (varVal);
					break;

				case "Boolean":
					convertedValue = (bool)Boolean.Parse (varVal);
					break;

				case "List`1":
					string[] splitValues = varVal.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
					convertedValue = new List <string> (splitValues);
					break;

				case "Single":
					convertedValue = float.Parse (varVal);
					break;

				case "String":
					convertedValue = varVal;
					break;

				case "SColor":
					convertedValue = new SColor (Colors.HexToColor (varVal.Replace ("#", "")));
					break;

				case "MobileReference":
					////Debug.Log ("Setting movile reference " + varVal);
					MobileReference mr = new MobileReference ();
					string[] splitMr = varVal.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
					mr.GroupPath = splitMr [0];
					mr.FileName = splitMr [1];
					convertedValue = mr;
					break;

				default:
					Debug.Log ("Type of : " + fieldType.Name);
					convertedValue = Enum.Parse (fieldType, varVal, true);
					break;
				}
				fieldInfo.SetValue (obj, convertedValue);
			}
		}
	}
}
public static class PathSanitizer
{
	/// <summary>
	/// The set of invalid filename characters, kept sorted for fast binary search
	/// </summary>
	private readonly static char[] invalidFilenameChars;
	/// <summary>
	/// The set of invalid path characters, kept sorted for fast binary search
	/// </summary>
	private readonly static char[] invalidPathChars;

	static PathSanitizer ()
	{
		// set up the two arrays -- sorted once for speed.
		invalidFilenameChars = System.IO.Path.GetInvalidFileNameChars ();
		invalidPathChars = System.IO.Path.GetInvalidPathChars ();
		Array.Sort (invalidFilenameChars);
		Array.Sort (invalidPathChars);

	}

	/// <summary>
	/// Cleans a filename of invalid characters
	/// </summary>
	/// <param name="input">the string to clean</param>
	/// <param name="errorChar">the character which replaces bad characters</param>
	/// <returns></returns>
	public static string SanitizeFilename (string input, char errorChar)
	{
		return Sanitize (input, invalidFilenameChars, errorChar);
	}

	/// <summary>
	/// Cleans a path of invalid characters
	/// </summary>
	/// <param name="input">the string to clean</param>
	/// <param name="errorChar">the character which replaces bad characters</param>
	/// <returns></returns>
	public static string SanitizePath (string input, char errorChar)
	{
		return Sanitize (input, invalidPathChars, errorChar);
	}

	/// <summary>
	/// Cleans a string of invalid characters.
	/// </summary>
	/// <param name="input"></param>
	/// <param name="invalidChars"></param>
	/// <param name="errorChar"></param>
	/// <returns></returns>
	private static string Sanitize (string input, char[] invalidChars, char errorChar)
	{
		// null always sanitizes to null
		if (input == null) {
			return null;
		}
		StringBuilder result = new StringBuilder ();
		foreach (var characterToTest in input) {
			// we binary search for the character in the invalid set. This should be lightning fast.
			if (Array.BinarySearch (invalidChars, characterToTest) >= 0) {
				// we found the character in the array of
				result.Append (errorChar);
			} else {
				// the character was not found in invalid, so it is valid.
				result.Append (characterToTest);
			}
		}

		// we're done.
		return result.ToString ();
	}
}