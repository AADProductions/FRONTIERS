using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.Story;
using Frontiers.World.BaseWIScripts;
using ExtensionMethods;

namespace Frontiers.World
{
		[ExecuteInEditMode]
		public class Characters : Manager
		{
				public static Characters Get;
				public static bool CharacterShadows;
				public MotileState DefaultMotileState = new MotileState();
				public LookerState DefaultLookerState = new LookerState();
				public DamageableState DefaultDamageableState = new DamageableState();
				//[FrontiersAvailableModsAttribute("Category")]
				public string DefaultInventoryFillCategory;
				public CharacterBody DefaultBody;
				//[FrontiersAvailableModsAttribute("Character/Face")]
				public string DefaultFaceTexture;
				//[FrontiersAvailableModsAttribute("Character/Body")]
				public string DefaultBodyTexture;
				public Material CharacterBodyMaterial;
				public Material CharacterFaceMaterial;
				public Material CharacterHairMaterial;
				public List <CharacterBody> MaleBodies = new List <CharacterBody>();
				public List <CharacterBody> FemaleBodies = new List <CharacterBody>();
				public List <CharacterBody> UnisexBodies = new List <CharacterBody>();
				public List <CharacterBody> ChildBodies = new List <CharacterBody>();
				public CharacterTemplate GenericCharacter = new CharacterTemplate();
				public List <CharacterBody> CharacterBodies = new List <CharacterBody>();
				public List <Character> SpawnedCharacters = new List <Character>();
				public List <Character> SpawnedPilgrims = new List <Character>();
				public List <DailyRoutineState> GenericRoutines = new List <DailyRoutineState>();
				public GameObject CharacterBase;
				public PlayerBody PlayerBodyPrefab;
				public List <Speech> DamageResponseSpeeches = new List<Speech>();
				public List <string> DamageResponseSpeechNames = new List<string>();

				public List <string> AvailableBodyNames {
						get {
								if (mAvailableCharacterBodies == null) {
										mAvailableCharacterBodies = new List<string>();
								} else {
										mAvailableCharacterBodies.Clear();
								}
								foreach (CharacterBody b in MaleBodies) {
										mAvailableCharacterBodies.Add(b.name);
								}
								foreach (CharacterBody b in FemaleBodies) {
										mAvailableCharacterBodies.Add(b.name);
								}
								foreach (CharacterBody b in UnisexBodies) {
										mAvailableCharacterBodies.Add(b.name);
								}
								foreach (CharacterBody b in ChildBodies) {
										mAvailableCharacterBodies.Add(b.name);
								}
								return mAvailableCharacterBodies;
						}
				}

				public List <CharacterBody> Bodies {
						get {
								List <CharacterBody> bodies = new List<CharacterBody>();
								bodies.AddRange(ChildBodies);
								bodies.AddRange(FemaleBodies);
								bodies.AddRange(MaleBodies);
								bodies.AddRange(UnisexBodies);
								return bodies;
						}
				}

				#region initialization

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
				}

				public override void Initialize()
				{
						if (mBodyLookup == null) {
								mBodyLookup = new Dictionary <string, CharacterBody>();
						} else {
								mBodyLookup.Clear();
						}

						mBodyMaterialLookup.Clear();
						mCharacterTemplates.Clear();

						Dictionary <string,string> bodyRenames = new Dictionary<string, string>();

						foreach (CharacterBody bodyTemplate in MaleBodies) {
								mBodyLookup.Add(bodyTemplate.name.Trim(), bodyTemplate);
						}
						foreach (CharacterBody bodyTemplate in FemaleBodies) {
								mBodyLookup.Add(bodyTemplate.name.Trim(), bodyTemplate);
						}
						foreach (CharacterBody bodyTemplate in UnisexBodies) {
								mBodyLookup.Add(bodyTemplate.name.Trim(), bodyTemplate);
						}
						foreach (CharacterBody bodyTemplate in ChildBodies) {
								mBodyLookup.Add(bodyTemplate.name.Trim(), bodyTemplate);
						}

						LoadCharacterTemplates();

						mInitialized = true;
				}

				public void LoadCharacterTemplates()
				{
						CharacterTemplates.Clear();
						GenericCharacterTemplates.Clear();
						mCharacterTemplates.Clear();
						CharacterTemplate characterTemplate = null;

						List <string> characterTemplateNames = Mods.Get.ModDataNames("Character");
						foreach (string characterTemplateName in characterTemplateNames) {
								if (Mods.Get.Runtime.LoadMod(ref characterTemplate, "Character", characterTemplateName)) {
										string templateName = characterTemplateName.Trim().ToLower();
										switch (characterTemplate.TemplateType) {
												case CharacterTemplateType.Generic:
												default:
														GenericCharacterTemplates.Add(characterTemplate);
														mCharacterTemplates.Add(templateName, characterTemplate);
														break;

												case CharacterTemplateType.UniqueAlternate:
												case CharacterTemplateType.UniquePrimary:
														CharacterTemplates.Add(characterTemplate);
														mCharacterTemplates.Add(templateName, characterTemplate);
														break;
										}
								}
						}

						//now link up face possiblities based on the body
						for (int i = 0; i < GenericCharacterTemplates.Count; i++) {
								//clear whatever we started with
								characterTemplate = GenericCharacterTemplates[i];
								if (characterTemplate.TextureKeywords.Count == 0) {
										characterTemplate.TextureKeywords.Add("Settler");
								}
								//clear whatever we started with
								characterTemplate.AvailableFaceTexturesMale.Clear();
								characterTemplate.AvailableFaceTexturesFemale.Clear();
								characterTemplate.AvailableBodyTexturesMale.Clear();
								characterTemplate.AvailableBodyTexturesFemale.Clear();
								CharacterBody maleBody = null;
								CharacterBody femaleBody = null;
								//add the default face and body textures
								if (GetBody(true, characterTemplate.BodyNameMale, out maleBody)) {
										CheckCharacterTextures(maleBody, characterTemplate, characterTemplate.AvailableBodyTexturesMale, characterTemplate.AvailableFaceTexturesMale, "M", characterTemplate.BodyNameMale);
								}
								if (GetBody(true, characterTemplate.BodyNameFemale, out femaleBody)) {
										CheckCharacterTextures(femaleBody, characterTemplate, characterTemplate.AvailableBodyTexturesFemale, characterTemplate.AvailableFaceTexturesFemale, "F", characterTemplate.BodyNameFemale);
								}
						}
				}

				public void RefreshCharacterShadowSettings(bool objectShadows)
				{
						if (CharacterShadows != objectShadows) {
								CharacterShadows = objectShadows;
								for (int i = 0; i < SpawnedCharacters.Count; i++) {
										if (SpawnedCharacters[i] != null && SpawnedCharacters[i].Body != null) {
												SpawnedCharacters[i].Body.RefreshShadowCasters();
										}
								}
						}
				}

				public override void OnGameLoadStart()
				{
						Speech speech = null;
						DamageResponseSpeeches.Clear();
						foreach (string speechName in DamageResponseSpeechNames) {
								if (Mods.Get.Runtime.LoadMod <Speech>(ref speech, "Speech", speechName)) {
										DamageResponseSpeeches.Add(speech);
								}
						}
				}

				public override void OnGameStart()
				{
						StartCoroutine(UpdatePilgrims());
				}

				public void Reset()
				{
						ResetCharacter("all");
				}

				public void ResetCharacter(string dataName)
				{
						Character character = null;
						if (dataName == "all") {
								foreach (Character spawnedCharacter in Get.SpawnedCharacters) {
										ResetCharacter(spawnedCharacter);
								}
						} else if (mSpawnedCharacters.TryGetValue(dataName.ToLower().Trim(), out character)) {
								ResetCharacter(character);
						}

				}

				protected void ResetCharacter(Character character)
				{
						CharacterTemplate template = null;
						if (mCharacterTemplates.TryGetValue(character.name.Trim().ToLower(), out template)) {
								character.State = ObjectClone.Clone <Frontiers.World.BaseWIScripts.CharacterState>(template.StateTemplate);
								Talkative talkative = character.GetComponent <Talkative>();
								talkative.State = ObjectClone.Clone <TalkativeState>(template.TalkativeTemplate);
						}
				}

				#endregion

				#region getting components

				public PlayerBody PlayerBody(PlayerBase player)
				{
						GameObject playerBodyGameObject = GameObject.Instantiate(PlayerBodyPrefab.gameObject) as GameObject;
						PlayerBody playerBody = playerBodyGameObject.GetComponent <PlayerBody>();
						playerBody.OnSpawn(player);
						playerBody.PlayerObject = player;
						playerBody.Initialize(player);
						//TODO set player body props based on player flags
						return playerBody;
				}

				public static DailyRoutineState RandomGenericDailyRoutine(CharacterFlags flags)
				{
						return ObjectClone.Clone <DailyRoutineState>(Get.GenericRoutines[UnityEngine.Random.Range(0, Get.GenericRoutines.Count)]);
				}

				public bool BodyTemplate(CharacterFlags combinedFlags, int tieBreaker, out CharacterBody characterBody)
				{
						characterBody = null;
						List <CharacterBody> availableTemplates = new List <CharacterBody>();
						//at this point it's assumed that the flag values are locked down
						switch (combinedFlags.GeneralAge) {
								case 1://TODO move these into globals (?)
										//child age
										availableTemplates.AddRange(ChildBodies);
										break;

								default:
										//adult ages or unspecified
										switch (combinedFlags.Gender) {
												case 1:
												default:
														//male
														availableTemplates.AddRange(MaleBodies);
														break;

												case 2:
														//female or unspecified
														availableTemplates.AddRange(FemaleBodies);
														break;
										}
										//for both male and female, add unisex bodies
										availableTemplates.AddRange(UnisexBodies);
										break;
						}
						//shuffle the list randomly using tiebreaker as a seed
						//the first matching template will be used
						//shuffling ensures randomness, using the seed ensures the same result every time
						availableTemplates.Shuffle(new System.Random(tieBreaker));
						bool foundMatchingBody = false;
						//Debug.Log ("Checking available bodies, " + availableTemplates.Count.ToString ());
						for (int i = 0; i < availableTemplates.Count; i++) {
								//first see if the body matches, that's most important
								if (availableTemplates[i].Flags.CharacterBodyLayout == combinedFlags.CharacterBodyLayout) {
										if (combinedFlags.Check(availableTemplates[i].Flags)) {
												//hooray, the flags match we're done
												foundMatchingBody = true;
												characterBody = availableTemplates[i];
												break;
										}
										//if the flags don't match we're still guaranteed to return a body
										//maybe not the RIGHT body but whatever...
								}
						}
						return characterBody != null;
				}

				public static bool GetTemplate(bool generic, string templateName, out CharacterTemplate template)
				{
						return Get.mCharacterTemplates.TryGetValue(templateName.Trim().ToLower(), out template);
				}

				public bool GetBody(bool generic, string bodyName, out CharacterBody body)
				{
						body = null;
						return !string.IsNullOrEmpty(bodyName) && mBodyLookup.TryGetValue(bodyName, out body);
				}

				public void CheckCharacterTextures(CharacterBody body, CharacterTemplate characterTemplate, List <string> bodyTextures, List <string> faceTextures, string gender, string defaultBodyName)
				{
						List <string> availableFaceTextures = Mods.Get.ModDataNames("Character/Face");
						List <string> availableBodyTextures = Mods.Get.ModDataNames("Character/Body");
						//if the body texture is already specified, just use that
						string layout = "A";
						string age = "Young";
						//general age isn't limited by the body so check the character instead
						//we wanted lots of age ranges, instead we have 2... sigh
						switch (characterTemplate.StateTemplate.Flags.GeneralAge) {
								case 1://child to teens
								case 2://twenties to thirties
								default:
										break;
								case 3://forties to fifties
								case 4://sixties to seventies
								case 5://ancient
										age = "Old";
										break;
						}
						switch (body.Flags.CharacterBodyLayout) {
						//body layouts are all-or nothing
								case 1://layout A
								default:
										break;

								case 2://layout B
										layout = "B";
										break;

								case 4://layout C
										layout = "C";
										break;

								case 8://layout D
										layout = "D";
										break;

								case 16://layout E
										layout = "E";
										break;

								case 32://layout F
										layout = "F";
										break;
						}

						for (int b = 0; b < availableBodyTextures.Count; b++) {
								string bodyTextureName = availableBodyTextures[b];
								string[] splitBodyName = bodyTextureName.Split(new String [] { "_" }, StringSplitOptions.RemoveEmptyEntries);
								//body texture is like so:
								//Body_Med_E_SoldierLeather_U_1
								//0:Body_[1:Size]_[2:Layout]_[3:Keyword]_[4:Gender]_[5:Variant]
								//first see if the body layout matches
								//unisex bodies can always be used for all genders
								if (string.Equals(splitBodyName[2], layout) && (string.Equals(splitBodyName[4], gender) || string.Equals(splitBodyName[4], "U"))) {
										//Debug.Log("Body name and gender match");
										for (int k = 0; k < characterTemplate.TextureKeywords.Count; k++) {
												if (splitBodyName[3].Contains(characterTemplate.TextureKeywords[k])) {
														bodyTextures.SafeAdd(bodyTextureName);
														break;
												}
										}
								}
						}
						//even if the face is specified we're still going to check other available faces
						for (int f = 0; f < availableFaceTextures.Count; f++) {
								string faceTextureName = availableFaceTextures[f];
								//if it contains CC that means custom character so it's out
								if (faceTextureName.Contains("_CC_")) {
										continue;
								}
								string[] splitFaceName = faceTextureName.Split(new String [] { "_" }, StringSplitOptions.RemoveEmptyEntries);
								//face texture is like so:
								//Face_A_M_Laborer_A_Old_1
								//0:Face_[1:Layout]_[2:Gender]_[3:Keyword]_[4:SkinColor]_[5:Age]_[6:Variant]
								//first check if the layout matches
								if (string.Equals(splitFaceName[1], layout) && string.Equals(splitFaceName[5], age) && string.Equals(splitFaceName[2], gender)) {
										//if we've got the layout and age right, next check gender
										//unisex characters can use male or female faces
										for (int k = 0; k < characterTemplate.TextureKeywords.Count; k++) {
												if (splitFaceName[3].Contains(characterTemplate.TextureKeywords[k])) {
														faceTextures.SafeAdd(faceTextureName);
														break;
												}
										}
								}
						}
				}

				public static string GetBodyName(CharacterBody body, Dictionary <string,string> existingBodies)
				{
						string bodyName = "Body_";
						int bodyNum = 1;
						switch (body.Flags.CharacterBodyLayout) {
						//body layouts are all-or nothing
								case 1://layout A
								default:
										bodyName += "A_";
										break;

								case 2://layout B
										bodyName += "B_";
										break;

								case 4://layout C
										bodyName += "C_";
										break;

								case 8://layout D
										bodyName += "D_";
										break;

								case 16://layout E
										bodyName += "E_";
										break;

								case 32://layout F
										bodyName += "F_";
										break;
						}
						switch (body.Flags.Gender) {
								case 1:
								default:
										bodyName += "M_";
										break;

								case 2:
										bodyName += "F_";
										break;

								case 3:
										bodyName += "U_";
										break;
						}
						string finalBodyName = bodyName + bodyNum.ToString();
						while (existingBodies.ContainsValue(finalBodyName)) {
								bodyNum++;
								finalBodyName = bodyName + bodyNum.ToString();
						}
						return finalBodyName;
				}

				public static void GetCharacterName(Character character, Region region)
				{
						//TODO also get names from other regions
						if (character.State.Flags.Gender == 0) {
								FirstNames = region.MaleFirstNames;
						} else {
								FirstNames = region.FemaleFirstNames;
						}
						LastNames = region.FamilyNames;
						int characterHashCode = Mathf.Abs(character.worlditem.GetHashCode());
						int firstNameIndex = 0;
						int lastNameIndex = 0;
						if (FirstNames.Count > 0) {
								firstNameIndex = characterHashCode % FirstNames.Count;
								character.State.Name.FirstName = FirstNames[firstNameIndex];
						}
						if (LastNames.Count > 0) {
								lastNameIndex = (characterHashCode + firstNameIndex) % LastNames.Count;
								character.State.Name.LastName = LastNames[lastNameIndex];
						}
						character.State.Name.FileName = characterHashCode.ToString() + "-" + character.State.Name.FirstName;
				}

				protected static List <string> FirstNames = null;
				protected static List <string> LastNames = null;

				public static void GetCharacterName(Character character, ActionNode node)
				{
						Region region = null;
						if (!GameWorld.Get.RegionAtPosition(node.Position, out region)) {
								region = GameWorld.Get.CurrentRegion;
						}
						GetCharacterName(character, region);
				}

				#endregion

				#region spawning

				//used to spawn characters by structures
				public static bool SpawnCharacter(ActionNode node, string characterName, WIFlags flags, WIGroup group, out Character newCharacter)
				{
						characterName = characterName.Trim().ToLower();
						CharacterTemplate template = null;
						newCharacter = null;
						if (Get.mCharacterTemplates.TryGetValue(characterName, out template)) {
								if (template.TemplateType == CharacterTemplateType.Generic) {
										return SpawnRandomCharacter(node, template, flags, group, out newCharacter);
								} else {
										return GetOrSpawnCharacter(node, characterName, group, out newCharacter);
								}
						}
						return false;
				}
				//used to mass-spawn characters by cities
				public static void SpawnRandomCharacter(ActionNode node, List <string> templateNames, WIFlags locationFlags, WIGroup group, out Character newCharacter)
				{
						string templateName = node.State.OccupantName;
						if (string.IsNullOrEmpty(templateName)) {
								if (templateNames.Count > 0) {
										int templateIndex = Mathf.Abs(node.State.GetHashCode()) % templateNames.Count;
										templateName = templateNames[templateIndex];
								} else {
										templateName = "Random";
								}
						}
						Character character = null;
						SpawnRandomCharacter(node, templateName, locationFlags, group, out newCharacter);
				}

				protected IEnumerator DespawnCharacterOverTime(Character character, bool fade)
				{
						if (fade) {
								bool waitingForFade = true;
								Frontiers.GUI.CameraFade.StartAlphaFade(Color.black, false, 1f, 0f, () => {
										waitingForFade = false;
								});
								waitingForFade = true;
								//once the fade out leading into the fade in is ready
								//start the actual fade in
								while (waitingForFade) {
										yield return null;
								}
						}
						character.worlditem.ActiveState = WIActiveState.Invisible;
						character.worlditem.ActiveStateLocked = true;
						character.worlditem.SetMode(WIMode.Unloaded);
						GameObject.Destroy(character.Body.gameObject);

						if (fade) {
								Frontiers.GUI.CameraFade.StartAlphaFade(Color.black, true, 1f);
						}
						yield break;
				}

				public void DespawnCharacter(Character character, bool fade)
				{
						StartCoroutine(DespawnCharacterOverTime(character, fade));
				}

				public static bool SpawnRandomPilgrim(CharacterTemplate template, Path onPath, PathMarkerInstanceTemplate atMarker, WIFlags locationFlags, out Character newPilgrim)
				{
						newPilgrim = null;
						int spawnValue = Mathf.Abs(locationFlags.GetHashCode());
						CharacterFlags combinedFlags = null;
						CharacterName characterName = new CharacterName();
						CharacterBody bodyTemplate = null;
						List <string> faceTextures = null;
						List <string> bodyTextures = null;

						#region flag creation and texture / body selection
						//start by getting the intersection of the location flags with the template flags
						//this will limit us to something appropriate
						combinedFlags = ObjectClone.Clone <CharacterFlags>(template.StateTemplate.Flags);
						combinedFlags.SafeIntersection(locationFlags);
						//now choose the gender
						combinedFlags.Gender = FlagSet.GetFlagBitValue(combinedFlags.Gender, spawnValue, 0);
						//get a body based on the newly chosen gender
						if (combinedFlags.Gender == 1) {
								//get a male body
								faceTextures = template.AvailableFaceTexturesMale;
								bodyTextures = template.AvailableBodyTexturesMale;
								if (!mBodyLookup.TryGetValue(template.BodyNameMale, out bodyTemplate)) {
										////DebugConsole.Get.Log.Add("#Couldn't find body " + template.BodyNameMale);
										Debug.Log("Couldn't find body " + template.BodyNameMale);
										return false;
								}
						} else {
								faceTextures = template.AvailableFaceTexturesFemale;
								bodyTextures = template.AvailableBodyTexturesFemale;
								if (!mBodyLookup.TryGetValue(template.BodyNameFemale, out bodyTemplate)) {
										////DebugConsole.Get.Log.Add("#Couldn't find body " + template.BodyNameFemale);
										Debug.Log("Couldn't find body " + characterName);
										return false;
								}
								//get a female body
						}
						//once we have a body, lock down the remaining attributes
						combinedFlags.SafeIntersection(bodyTemplate.Flags);
						combinedFlags.ChooseMajorValues(spawnValue);
						combinedFlags.ChooseMinorValues(spawnValue);
						//generate a random name from the final combined flags
						characterName.Generate(combinedFlags);
						#endregion

						#region build body
						//now build the character body
						GameObject newCharacterBase = GameObject.Instantiate(Get.CharacterBase) as GameObject;
						newCharacterBase.name = characterName.FileName;
						newPilgrim = newCharacterBase.GetComponent <Character>();
						//instantiate the body object under the character base object
						GameObject newBodyObject = newCharacterBase.InstantiateUnder(bodyTemplate.gameObject, false);
						newPilgrim.Body = newBodyObject.GetComponent <CharacterBody>();
						#endregion

						#region clone data / assign
						//apply all the states by copying the template data
						//make sure to copy the combined flags!
						newPilgrim.State = ObjectClone.Clone <Frontiers.World.BaseWIScripts.CharacterState>(template.StateTemplate);
						newPilgrim.State.Name = characterName;
						newPilgrim.State.Flags = combinedFlags;
						//save the body and character texture names, they won't change again
						newPilgrim.State.BodyName = bodyTemplate.name;
						newPilgrim.State.BodyTextureName = Get.DefaultBodyTexture;// characterTexture.Name;
						newPilgrim.State.TemplateName = template.Name;
						Talkative talkative = newCharacterBase.GetComponent <Talkative>();
						Motile motile = newCharacterBase.GetComponent <Motile>();
						talkative.State = ObjectClone.Clone <TalkativeState>(template.TalkativeTemplate);

						if (template.UseDefaultMotile) {
								motile.State = ObjectClone.Clone <MotileState>(Get.DefaultMotileState);
						} else {
								motile.State = ObjectClone.Clone <MotileState>(template.MotileTemplate);
						}
						motile.Body = newPilgrim.Body;

						if (string.IsNullOrEmpty(template.InventoryFillCategory)) {
								template.InventoryFillCategory = Get.DefaultInventoryFillCategory;
						}

						FillStackContainer fsc = newCharacterBase.GetComponent <FillStackContainer>();
						if (!string.IsNullOrEmpty(template.InventoryFillCategory)) {
								fsc.State.WICategoryName = template.InventoryFillCategory;
						}

						//get/add final components and initialize
						WorldItem newCharacterWorlditem = newCharacterBase.GetComponent <WorldItem>();
						//set up the file name right away so there's no problem with groups
						newCharacterWorlditem.Props.Name.FileName = characterName.FileName;
						GetCharacterName(newPilgrim, GameWorld.Get.CurrentRegion);
						newCharacterWorlditem.Group = GameWorld.Get.PrimaryChunk.AboveGroundGroup;

						//create a new global reputation
						newPilgrim.State.GlobalReputation = UnityEngine.Random.Range(0, 100);

						newPilgrim.Template = template;

						//apply custom scripts if any
						foreach (string customWiScript in template.CustomWIScripts) {
								newPilgrim.gameObject.AddComponent(customWiScript);
						}
						#endregion

						#region move to final position
						newCharacterWorlditem.transform.position = atMarker.Position;
						newCharacterWorlditem.transform.rotation = Quaternion.identity;
						//Debug.Log ("node local position is " + node.transform.localPosition);
						//parent under group copy new position to the local props so it initializes in the right place - this is messy but it'll get cleaned up later
						newCharacterWorlditem.transform.parent = GameWorld.Get.PrimaryChunk.AboveGroundGroup.tr;
						newCharacterWorlditem.Props.Local.Transform.Position = newCharacterWorlditem.transform.localPosition;
						newCharacterWorlditem.Props.Local.Transform.Rotation = newCharacterWorlditem.transform.localRotation.eulerAngles;
						newPilgrim.Body.transform.position = newCharacterBase.transform.position;
						newPilgrim.Body.transform.rotation = newCharacterBase.transform.rotation;
						#endregion

						//initialize immediately
						newCharacterWorlditem.Initialize();

						//add to lookup arrays
						//Debug.Log ("Adding character file name " + characterName.FileName + " to dictionary for " + templateName);
						Get.mSpawnedCharacters.Add(characterName.FileName, newPilgrim);
						Get.SpawnedCharacters.Add(newPilgrim);
						//Get.SelectedCharacter = Get.SpawnedCharacters.LastIndex();

						motile.IsImmobilized = false;

						#region skin texture
						//apply the skin & clothing mask textures to the body
						Texture2D body = null;
						Texture2D bodyMask = null;
						Texture2D face = null;
						Texture2D faceMask = null;
						string ethnicity = "A";
						switch (combinedFlags.Ethnicity) {
								case 1:
								default:
										break;
								case 2:
										ethnicity = "B";
										break;
								case 3:
										ethnicity = "C";
										break;
								case 4:
										ethnicity = "D";
										break;
						}

						//get a face texture from the available face textures
						template.StateTemplate.FaceTextureName = Get.DefaultFaceTexture;
						if (faceTextures.Count > 0) {
								//we'll choose a face based on the chosen ethnicity
								for (int f = 0; f < faceTextures.Count; f++) {
										string[] splitFaceTexture = faceTextures[f].Split(new String [] { "_" }, StringSplitOptions.RemoveEmptyEntries);
										//0:Face_[1:Layout]_[2:Gender]_[3:Keyword]_[4:SkinColor]_[5:Age]_[6:Variant]
										if (string.Equals(ethnicity, splitFaceTexture[4])) {// && string.Equals (age, splitFaceTexture [5])) {
												template.StateTemplate.FaceTextureName = faceTextures[f];//faceTextures [spawnValue % faceTextures.Count];
												break;
										}
								}
						}
						template.StateTemplate.BodyTextureName = Get.DefaultBodyTexture;
						if (bodyTextures.Count > 0) {
								template.StateTemplate.BodyTextureName = bodyTextures.NextItem(spawnValue);// bodyTextures [spawnValue % bodyTextures.Count];
								//Debug.Log ("Picked " + template.StateTemplate.BodyTextureName + " for body texture in " + template.Name);
						}
						Mods.Get.Runtime.FaceTexture(ref face, template.StateTemplate.FaceTextureName);
						Mods.Get.Runtime.BodyTexture(ref body, template.StateTemplate.BodyTextureName);
						Mods.Get.Runtime.MaskTexture(ref bodyMask, template.StateTemplate.BodyMaskTextureName);
						Mods.Get.Runtime.MaskTexture(ref faceMask, template.StateTemplate.FaceMaskTextureName);
						//create an instance of the body material and store it
						Material bodyMaterial = new Material(Get.CharacterBodyMaterial);
						Material faceMaterial = new Material(Get.CharacterFaceMaterial);
						bodyMaterial.SetTexture("_MainTex", body);
						bodyMaterial.SetTexture("_MaskTex", bodyMask);
						faceMaterial.SetTexture("_MainTex", face);
						faceMaterial.SetTexture("_MaskTex", faceMask);
						//TODO apply body colors
						newPilgrim.Body.MainMaterial = bodyMaterial;
						newPilgrim.Body.FaceMaterial = faceMaterial;
						#endregion

						//whew! done

						return true;
				}

				public static bool SpawnRandomCharacter(ActionNode node, string templateName, WIFlags locationFlags, WIGroup group, out Character newCharacter)
				{
						newCharacter = null;
						CharacterTemplate template = null;
						if (Get.mCharacterTemplates.TryGetValue(templateName.Trim().ToLower(), out template)) {
								return SpawnRandomCharacter(node, template, locationFlags, group, out newCharacter);
						} else {
								Debug.Log("Couldn't find template " + templateName);
								return false;
						}
				}

				public static bool SpawnRandomCharacter(ActionNode node, CharacterTemplate template, WIFlags locationFlags, WIGroup group, out Character newCharacter)
				{
						newCharacter = null;
						int spawnValue = Mathf.Abs(group.GetHashCode() + node.State.GetHashCode());
						CharacterFlags combinedFlags = null;
						CharacterName characterName = new CharacterName();
						CharacterBody bodyTemplate = null;
						List <string> faceTextures = null;
						List <string> bodyTextures = null;

						#region flag creation and texture / body selection
						//start by getting the intersection of the location flags with the template flags
						//this will limit us to something appropriate
						combinedFlags = ObjectClone.Clone <CharacterFlags>(template.StateTemplate.Flags);
						combinedFlags.SafeIntersection(locationFlags);
						//now choose the gender
						combinedFlags.Gender = FlagSet.GetFlagBitValue(combinedFlags.Gender, spawnValue, 0);
						//get a body based on the newly chosen gender
						if (combinedFlags.Gender == 1) {
								//get a male body
								faceTextures = template.AvailableFaceTexturesMale;
								bodyTextures = template.AvailableBodyTexturesMale;
								if (!mBodyLookup.TryGetValue(template.BodyNameMale, out bodyTemplate)) {
										//DebugConsole.Get.Log.Add("#Couldn't find body " + template.BodyNameMale);
										Debug.Log("Couldn't find body " + template.BodyNameMale);
										return false;
								}
						} else {
								faceTextures = template.AvailableFaceTexturesFemale;
								bodyTextures = template.AvailableBodyTexturesFemale;
								if (!mBodyLookup.TryGetValue(template.BodyNameFemale, out bodyTemplate)) {
										//DebugConsole.Get.Log.Add("#Couldn't find body " + template.BodyNameFemale);
										Debug.Log("Couldn't find body " + characterName);
										return false;
								}
								//get a female body
						}
						//once we have a body, lock down the remaining attributes
						combinedFlags.SafeIntersection(bodyTemplate.Flags);
						combinedFlags.ChooseMajorValues(spawnValue);
						combinedFlags.ChooseMinorValues(spawnValue);
						//generate a random name from the final combined flags
						characterName.Generate(combinedFlags);
						#endregion

						#region build body
						//now build the character body
						GameObject newCharacterBase = GameObject.Instantiate(Get.CharacterBase) as GameObject;
						newCharacterBase.name = characterName.FileName;
						newCharacter = newCharacterBase.GetComponent <Character>();
						//instantiate the body object under the character base object
						GameObject newBodyObject = newCharacterBase.InstantiateUnder(bodyTemplate.gameObject, false);
						newCharacter.Body = newBodyObject.GetComponent <CharacterBody>();
						#endregion

						#region clone data / assign
						//apply all the states by copying the template data
						//make sure to copy the combined flags!
						newCharacter.State = ObjectClone.Clone <Frontiers.World.BaseWIScripts.CharacterState>(template.StateTemplate);
						newCharacter.State.Name = characterName;
						newCharacter.State.Flags = combinedFlags;
						//save the body and character texture names, they won't change again
						newCharacter.State.BodyName = bodyTemplate.name;
						newCharacter.State.BodyTextureName = Get.DefaultBodyTexture;// characterTexture.Name;
						newCharacter.State.TemplateName = template.Name;
						Talkative talkative = newCharacterBase.GetComponent <Talkative>();
						Motile motile = newCharacterBase.GetComponent <Motile>();
						talkative.State = ObjectClone.Clone <TalkativeState>(template.TalkativeTemplate);
						if (!string.IsNullOrEmpty(node.State.CustomConversation)) {
								//use the node's custom dialog on this character
								talkative.State.ConversationName = node.State.CustomConversation;
						}
						if (!string.IsNullOrEmpty(node.State.CustomSpeech)) {
								//Debug.Log("Giving character cutsom speech " + node.State.CustomSpeech);
								talkative.State.DTSSpeechName = node.State.CustomSpeech;
								talkative.State.DefaultToDTS = true;
						}

						if (template.UseDefaultMotile) {
								motile.State = ObjectClone.Clone <MotileState>(Get.DefaultMotileState);
						} else {
								motile.State = ObjectClone.Clone <MotileState>(template.MotileTemplate);
						}
						motile.Body = newCharacter.Body;

						if (string.IsNullOrEmpty(template.InventoryFillCategory)) {
								template.InventoryFillCategory = Get.DefaultInventoryFillCategory;
						}

						FillStackContainer fsc = newCharacterBase.GetComponent <FillStackContainer>();
						if (!string.IsNullOrEmpty(template.InventoryFillCategory)) {
								fsc.State.WICategoryName = template.InventoryFillCategory;
						}

						//get/add final components and initialize
						WorldItem newCharacterWorlditem = newCharacterBase.GetComponent <WorldItem>();
						//set up the file name right away so there's no problem with groups
						newCharacterWorlditem.Props.Name.FileName = characterName.FileName;
						GetCharacterName(newCharacter, node);
						newCharacterWorlditem.Group = group;

						//create a new global reputation
						newCharacter.State.GlobalReputation = UnityEngine.Random.Range(0, 100);

						newCharacter.Template = template;

						//apply custom scripts if any
						foreach (string customWiScript in template.CustomWIScripts) {
								newCharacter.gameObject.AddComponent(customWiScript);
						}
						#endregion

						#region move to final position
						newCharacterWorlditem.transform.position = node.transform.position;
						newCharacterWorlditem.transform.rotation = node.transform.rotation;
						//parent under group copy new position to the local props so it initializes in the right place - this is messy but it'll get cleaned up later
						newCharacterWorlditem.transform.parent = group.transform;
						newCharacterWorlditem.Props.Local.Transform.Position = newCharacterWorlditem.transform.localPosition;
						newCharacterWorlditem.Props.Local.Transform.Rotation = newCharacterWorlditem.transform.localRotation.eulerAngles;
						newCharacter.Body.transform.position = newCharacterBase.transform.position;
						newCharacter.Body.transform.rotation = newCharacterBase.transform.rotation;
						#endregion

						//initialize immediately
						newCharacterWorlditem.Initialize();

						//add to lookup arrays
						Get.mSpawnedCharacters.Add(characterName.FileName, newCharacter);
						Get.SpawnedCharacters.Add(newCharacter);
						//Get.SelectedCharacter = Get.SpawnedCharacters.LastIndex();

						//set the node occupant to the new character
						//TODO make this safer
						node.ForceOccupyNode(newCharacter.worlditem);

						motile.IsImmobilized = false;

						//are they supposed to be dead?
						if (node.State.OccupantIsDead) {
								newCharacter.IsDead = true;
						}

						#region skin texture
						//apply the skin & clothing mask textures to the body
						Texture2D body = null;
						Texture2D bodyMask = null;
						Texture2D face = null;
						Texture2D faceMask = null;
						string ethnicity = "A";
						//string age = "Young";
						switch (combinedFlags.Ethnicity) {
								case 1:
								default:
										break;
								case 2:
										ethnicity = "B";
										break;
								case 3:
										ethnicity = "C";
										break;
								case 4:
										ethnicity = "D";
										break;
						}
						//Debug.Log ("Picked ethnicity " + ethnicity);
						/*
						switch (combinedFlags.GeneralAge) {
							case 1:
							case 2:
							case 3:
								break;

							default:
								age = "Old";
								break;
						}
						*/
						//get a face texture from the available face textures
						template.StateTemplate.FaceTextureName = Get.DefaultFaceTexture;
						if (faceTextures.Count > 0) {
								//we'll choose a face based on the chosen ethnicity
								for (int f = 0; f < faceTextures.Count; f++) {
										string[] splitFaceTexture = faceTextures[f].Split(new String [] { "_" }, StringSplitOptions.RemoveEmptyEntries);
										//0:Face_[1:Layout]_[2:Gender]_[3:Keyword]_[4:SkinColor]_[5:Age]_[6:Variant]
										if (string.Equals(ethnicity, splitFaceTexture[4])) {// && string.Equals (age, splitFaceTexture [5])) {
												template.StateTemplate.FaceTextureName = faceTextures[f];//faceTextures [spawnValue % faceTextures.Count];
												break;
										}
								}
						}
						template.StateTemplate.BodyTextureName = Get.DefaultBodyTexture;
						if (bodyTextures.Count > 0) {
								template.StateTemplate.BodyTextureName = bodyTextures.NextItem(spawnValue);// bodyTextures [spawnValue % bodyTextures.Count];
						}
						Mods.Get.Runtime.FaceTexture(ref face, template.StateTemplate.FaceTextureName);
						Mods.Get.Runtime.BodyTexture(ref body, template.StateTemplate.BodyTextureName);
						Mods.Get.Runtime.MaskTexture(ref bodyMask, template.StateTemplate.BodyMaskTextureName);
						Mods.Get.Runtime.MaskTexture(ref faceMask, template.StateTemplate.FaceMaskTextureName);
						//create an instance of the body material and store it
						Material bodyMaterial = new Material(Get.CharacterBodyMaterial);
						Material faceMaterial = new Material(Get.CharacterFaceMaterial);
						bodyMaterial.SetTexture("_MainTex", body);
						bodyMaterial.SetTexture("_MaskTex", bodyMask);
						faceMaterial.SetTexture("_MainTex", face);
						faceMaterial.SetTexture("_MaskTex", faceMask);
						//TODO apply body colors
						newCharacter.Body.MainMaterial = bodyMaterial;
						newCharacter.Body.FaceMaterial = faceMaterial;
						#endregion

						node.TryToOccupyNode(newCharacterWorlditem);
						//whew! done
						return true;
				}

				public bool SpawnedCharacter(string characterName, out Character character)
				{
						characterName = characterName.Trim().ToLower();
						return Get.mSpawnedCharacters.TryGetValue(characterName, out character);
				}

				public static bool GetOrSpawnCharacter(ActionNode node, string characterName, WIGroup group, out Character newCharacter)
				{
						characterName = characterName.Trim().ToLower();
						int spawnValue = characterName.GetHashCode();
						CharacterTemplate template = null;
						newCharacter = null;

						if (!Get.mCharacterTemplates.TryGetValue(characterName, out template)) {
								Debug.Log("Couldn't find template " + characterName);
								return false;
						}

						CharacterBody bodyTemplate = null;
						if (!mBodyLookup.TryGetValue(template.StateTemplate.BodyName, out bodyTemplate)) {
								Debug.Log("Couldn't find body " + template.StateTemplate.BodyName);
								return false;
						}

						//now build the character body
						GameObject newCharacterBase = GameObject.Instantiate(Get.CharacterBase) as GameObject;
						newCharacterBase.name = template.StateTemplate.Name.FileName;
						newCharacter = newCharacterBase.GetComponent <Character>();
						//instantiate the body object under the character base object
						GameObject newBodyObject = newCharacterBase.InstantiateUnder(bodyTemplate.gameObject, false);
						newCharacter.Body = newBodyObject.GetComponent <CharacterBody>();

						//apply all the states by copying the template data
						newCharacter.State = ObjectClone.Clone <Frontiers.World.BaseWIScripts.CharacterState>(template.StateTemplate);
						//save the body and character texture names, they won't change again
						newCharacter.State.TemplateName = template.Name;
						Talkative talkative = newCharacterBase.GetComponent <Talkative>();
						Motile motile = newCharacterBase.GetComponent <Motile>();
						talkative.State = ObjectClone.Clone <TalkativeState>(template.TalkativeTemplate);
						if (!string.IsNullOrEmpty(node.State.CustomConversation)) {
								//use the node's custom dialog on this character
								talkative.State.ConversationName = node.State.CustomConversation;
						}
						if (!string.IsNullOrEmpty(node.State.CustomSpeech)) {
								talkative.State.DTSSpeechName = node.State.CustomSpeech;
								talkative.State.DefaultToDTS = true;
						}

						if (template.UseDefaultMotile) {
								motile.State = ObjectClone.Clone <MotileState>(Get.DefaultMotileState);
						} else {
								motile.State = ObjectClone.Clone <MotileState>(template.MotileTemplate);
						}
						motile.Body = newCharacter.Body;

						//get/add final components and initialize
						WorldItem newCharacterWorlditem = newCharacterBase.GetComponent <WorldItem>();
						//set up the file name right away so there's no problem with groups
						newCharacterWorlditem.Props.Name.FileName = newCharacter.State.Name.FileName;
						newCharacterWorlditem.Group = group;

						FillStackContainer fsc = newCharacterBase.GetComponent <FillStackContainer>();
						if (!string.IsNullOrEmpty(template.InventoryFillCategory)) {
								fsc.State.WICategoryName = template.InventoryFillCategory;
						}

						//apply custom scripts if any
						foreach (string customWiScript in template.CustomWIScripts) {
								newCharacter.gameObject.AddComponent(customWiScript);
						}

						newCharacterWorlditem.transform.position = node.transform.position;
						newCharacterWorlditem.transform.rotation = node.transform.rotation;
						//parent under group copy new position to the local props so it initializes in the right place - this is messy but it'll get cleaned up later
						newCharacterWorlditem.transform.parent = group.transform;
						newCharacterWorlditem.Props.Local.Transform.Position = newCharacterWorlditem.transform.localPosition;
						newCharacterWorlditem.Props.Local.Transform.Rotation = newCharacterWorlditem.transform.localRotation.eulerAngles;
						newCharacter.Body.transform.position = newCharacterBase.transform.position;
						newCharacter.Body.transform.rotation = newCharacterBase.transform.rotation;

						newCharacter.Template = template;

						//initialize immediately
						newCharacterWorlditem.Initialize();

						//add to lookup arrays
						if (!Get.mSpawnedCharacters.ContainsKey(characterName)) {
								Get.mSpawnedCharacters.Add(characterName, newCharacter);
								Get.SpawnedCharacters.Add(newCharacter);
								//Get.SelectedCharacter = Get.SpawnedCharacters.LastIndex();
						}

						//set the node occupant to the new character
						//TODO make this safer
						node.ForceOccupyNode(newCharacter.worlditem);

						motile.IsImmobilized = false;

						//are they supposed to be dead?
						if (node.State.OccupantIsDead) {
								newCharacter.IsDead = true;
						}

						#region skin texture
						//apply the skin & clothing mask textures to the body
						Texture2D body = null;
						Texture2D bodyMask = null;
						Texture2D face = null;
						Texture2D faceMask = null;
						Mods.Get.Runtime.FaceTexture(ref face, template.StateTemplate.FaceTextureName);
						Mods.Get.Runtime.BodyTexture(ref body, template.StateTemplate.BodyTextureName);
						Mods.Get.Runtime.MaskTexture(ref bodyMask, template.StateTemplate.BodyMaskTextureName);
						Mods.Get.Runtime.MaskTexture(ref faceMask, template.StateTemplate.FaceMaskTextureName);
						//create an instance of the body material and store it
						Material bodyMaterial = new Material(Get.CharacterBodyMaterial);
						Material faceMaterial = new Material(Get.CharacterFaceMaterial);
						bodyMaterial.SetTexture("_MainTex", body);
						bodyMaterial.SetTexture("_MaskTex", bodyMask);
						faceMaterial.SetTexture("_MainTex", face);
						faceMaterial.SetTexture("_MaskTex", faceMask);
						//TODO apply body colors
						newCharacter.Body.MainMaterial = bodyMaterial;
						newCharacter.Body.FaceMaterial = faceMaterial;
						#endregion

						//whew! done
						return true;
				}

				#endregion

				#region convenience functions

				public static bool KnowsPlayer(string characterName)
				{
						CharacterTemplate template = null;
						if (Get.mCharacterTemplates.TryGetValue(characterName, out template)) {
								return template.StateTemplate.KnowsPlayer;
						}
						return false;
				}

				public bool CharacterHasReachedQuestNode(string characterName, string nodeName, out float time)
				{
						bool result = false;
						time = 0.0f;
						MotileState motileState = null;

						Character character = null;
						if (mSpawnedCharacters.TryGetValue(characterName, out character)) {
								//first see if the character has been spawned
								Motile motile = null;
								if (character.worlditem.Is <Motile>(out motile)) {
										motileState = motile.State;
								}
						} else {
								//if it's not spawned, load the state from disk
								StackItem characterStackItem = null;
								if (Mods.Get.Runtime.LoadMod <StackItem>(ref characterStackItem, "Character", characterName)) {
										object stateData = null;
										if (characterStackItem.GetStateOf <Motile>(out stateData)) {
												motileState = (MotileState)stateData;
										}
								}
						}

						//do we have the motile data?
						if (motileState != null) {
								//check to see if the quest points contain the node
								result = motileState.QuestPointsReached.Contains(nodeName);
						}

						return result;
				}

				#endregion

				#region updates

				public Speech SpeechInResponseToDamage (float normalizedDamage, bool knowsPlayer, int characterRepWithPlayer, int characterRepInGeneral)
				{
						//TODO make this not just random
						return DamageResponseSpeeches[UnityEngine.Random.Range(0, DamageResponseSpeeches.Count)];
				}

				protected WaitForSeconds mWaitForUpdatePilgrims = new WaitForSeconds (1f);

				public IEnumerator UpdatePilgrims()
				{
						mUpdatingPilgrims = true;
						while (mUpdatingPilgrims) {
								while (!GameManager.Is(FGameState.InGame)) {
										yield return null;
								}
								//check to see if any pilgrims are out of range
								for (int i = SpawnedPilgrims.LastIndex(); i >= 0; i--) {
										if (SpawnedPilgrims[i].worlditem.Is(WIActiveState.Invisible)) {
												Debug.Log("Spawned pilgrim was invisible, despawing now");
												DespawnCharacter(SpawnedPilgrims[i], false);
												SpawnedPilgrims.RemoveAt(i);
										}
								}
								//wait a moment
								yield return null;
								if (Player.Local.Status.IsStateActive("Traveling")) {
										//move the spawned pilgrims about on their paths
										for (int i = 0; i < SpawnedPilgrims.Count; i++) {
												Pilgrim pilgrim = SpawnedPilgrims[i].worlditem.Get<Pilgrim>();
												pilgrim.OnFastTravelFrame();
										}
								} else {
										//check to see if we need to spawn any more
										System.Random random = new System.Random(Profile.Get.CurrentGame.Seed);
										while (SpawnedPilgrims.Count < Globals.MaxSpawnedPilgrims) {
												if (GameManager.Is(FGameState.InGame)) {
														//get a nearby path
														Path onPath = null;
														if (Paths.Get.PathNearPlayer(out onPath, random.Next())) {
																Character newPilgrim = null;
																WIFlags locationFlags = GameWorld.Get.CurrentRegion.ResidentFlags;
																PathMarkerInstanceTemplate atMarker = Paths.Get.FirstMarkerWithinRange(Globals.PlayerEncounterRadius * 2, Globals.PlayerColliderRadius, onPath, Player.Local.Position);
																CharacterTemplate template = GenericCharacterTemplates[random.Next(0, GenericCharacterTemplates.Count)];
																try {
																		if (SpawnRandomPilgrim(template, onPath, atMarker, locationFlags, out newPilgrim)) {
																				SpawnedPilgrims.Add(newPilgrim);
																				Pilgrim pilgrim = newPilgrim.worlditem.GetOrAdd <Pilgrim>();
																				pilgrim.LastMarker = atMarker;
																				pilgrim.ActivePath = onPath;
																		}
																} catch (Exception e) {
																		Debug.LogError("Exception while spawning pilgrim - continuing normally: " + e.ToString());
																}
														}
												}
												yield return mWaitForUpdatePilgrims;
										}
										yield return mWaitForUpdatePilgrims;
								}
						}
						mUpdatingPilgrims = false;
						yield break;
				}

				protected bool mUpdatingPilgrims = true;

				public void Update()
				{
						while (mRecentlySpawnedCharacters.Count > 0) {
								KeyValuePair <Motile, ActionNode> charNodePair = mRecentlySpawnedCharacters.Dequeue();
								charNodePair.Key.PushMotileAction(MotileAction.GoTo(charNodePair.Value.State), MotileActionPriority.ForceTop);
						}
				}

				#endregion

				#if UNITY_EDITOR
				public int SelectedCharacter {
						get {
								return mSelectedCharacter;
						}
						set {
								mSelectedCharacter = value;
								if (mSelectedCharacter < 0) {
										mSelectedCharacter = SpawnedCharacters.LastIndex();
								} else if (mSelectedCharacter >= SpawnedCharacters.Count) {
										mSelectedCharacter = 0;
								}
								if (SpawnedCharacters.Count > 0) {
										//DebugConsole.Get.Log.Add("#Selecting character " + SpawnedCharacters[mSelectedCharacter].name.ToString());
								}
						}
				}

				protected bool checkTextures = false;

				public void SortTemplates()
				{
						GenericCharacterTemplates.Sort();
				}

				public void DrawEditor()
				{
						UnityEngine.GUI.color = Color.yellow;
						if (GUILayout.Button("\nSort templates\n")) {
								SortTemplates();
						}
						if (GUILayout.Button("\nSave Character Templates\n")) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor();
								foreach (CharacterTemplate template in CharacterTemplates) {
										Mods.Get.Editor.SaveMod <CharacterTemplate>(template, "Character", template.Name);
								}
								foreach (CharacterTemplate genericTemplate in GenericCharacterTemplates) {
										Mods.Get.Editor.SaveMod <CharacterTemplate>(genericTemplate, "Character", genericTemplate.Name);
								}
						}
						if (GUILayout.Button("\nLoad Character Templates\n")) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Initialize();//this will sort the bodies AND load templates
						}
				}
				#endif
				protected int mSelectedCharacter = 0;
				protected List <string> mAvailableCharacterBodies;
				protected Dictionary <string, Character> mSpawnedCharacters = new Dictionary <string, Character>();
				protected Queue <KeyValuePair <Motile, ActionNode>> mRecentlySpawnedCharacters = new Queue <KeyValuePair <Motile, ActionNode>>();
				public List <CharacterTemplate> GenericCharacterTemplates = new List <CharacterTemplate>();
				public List <CharacterTemplate> CharacterTemplates = new List <CharacterTemplate>();
				protected Dictionary <string, CharacterTemplate> mCharacterTemplates = new Dictionary <string, CharacterTemplate>();
				protected Dictionary <int, Material> mBodyMaterialLookup = new Dictionary <int, Material>();
				protected static Dictionary <string, CharacterBody> mBodyLookup;
		}
}