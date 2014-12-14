using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Frontiers.World;
using System;
using System.Xml.Serialization;
using Frontiers.World.Gameplay;

namespace Frontiers
{
		public class Books : Manager
		{
				public static Books Get;
				public static string DefaultBookTemplate = "UtilitarianParchment";
				public List <Library> Libraries = new List <Library>();
				public HashSet <string> AquiredBooks = new HashSet <string>();
				public BookTemplate DefaultTemplate = new BookTemplate();
				public List <BookTemplate> Templates = new List <BookTemplate>();
				public List <BookTemplate> GeneralTemplates = new List<BookTemplate>();
				public List <List <BookTemplate>> SkillTemplates = new List <List <BookTemplate>>();
				public List <MeshList> BookMeshes = new List <MeshList>();
				public List <TextureList> BookTextures = new List <TextureList>();
				public Material BooksMaterial;
				public Material ScrollsMaterial;
				public Material ParchmentsMaterial;
				public Material BlueprintsMaterial;
				public Material ScrapsMaterial;
				public List <BookFont> Fonts = new List <BookFont>();
				public List <BookInk> Inks = new List <BookInk>();
				public BookFont DefaultFont;
				public BookFont SimpleFont;
				public BookInk DefaultInk;
				public FlagSet WealthFlagset;
				public BookFlags GuildFlags = new BookFlags();
				public BookFlags MagicFlags = new BookFlags();
				public BookFlags SurvivalFlags = new BookFlags();
				public BookFlags SubterfugeFlags = new BookFlags();
				public BookFlags ArchaeologyFlags = new BookFlags();
				public BookFlags CraftingFlags = new BookFlags();

				#region initialization

				public override void WakeUp()
				{
						Get = this;
				}

				public override void OnModsLoadFinish()
				{
						mTemplateLookup.Clear();
						mLibraryLookup.Clear();
						mBookMaterialLookup.Clear();
						Templates.Clear();
						mRandomGeneralTemplateOrder.Clear();
						mRandomSkillTemplateOrder.Clear();
						Libraries.Clear();

						//set up our libraries first
						Mods.Get.Runtime.LoadAvailableMods(Libraries, "Library");
						for (int i = 0; i < Libraries.Count; i++) {
								mLibraryLookup.Add(Libraries[i].Name, Libraries[i]);
								for (int j = 0; j < Libraries[i].CatalogueEntries.Count; j++) {
										//load the titles into the books so they look right in browsers
										LibraryCatalogueEntry catalogueEntry = Libraries[i].CatalogueEntries[j];
										Book book = null;
										if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", catalogueEntry.BookName)) {
												catalogueEntry.BookObject.DisplayName = book.CleanTitle;
										}
								}
						}

						//now set up our templates
						Mods.Get.Runtime.LoadAvailableMods(Templates, "BookTemplate");
						List <BookTemplate> templateList = null;
						BookTemplate bookTemplate = null;

						for (int i = 0; i < Templates.Count; i++) {
								bookTemplate = Templates[i];
								//add to the lookup for easy access
								mTemplateLookup.Add(bookTemplate.Name, bookTemplate);
								//associate all of this template's books with this template
								for (int j = 0; j < bookTemplate.BookNames.Count; j++) {
										if (!mBookTemplateAssociations.TryGetValue(bookTemplate.BookNames[j], out templateList)) {
												templateList = new List <BookTemplate>();
												mBookTemplateAssociations.Add(bookTemplate.BookNames[j], templateList);
										}
										templateList.Add(bookTemplate);
								}
						}

						//set the basic book item props
						mBookItem.PackName = "Books";
						mBookItem.PrefabName = "BookAvatar";
						mBookItem.State = "Default";

						//we use the wealth flagset a lot so get it now
						GameWorld.Get.FlagSetByName("Wealth", out WealthFlagset);

						//figure out the aquired books
						//associate book names with titles
						//associate skill names with book names
						AquiredBooks.Clear();
						mAvailableBooks.Clear();
						mAvailableBooks.AddRange(Mods.Get.ModDataNames("Book"));
						//load each book type and see if we've aquired it
						//in the future we'll only search for these when populating browser data
						List <string> skillNames = null;
						for (int i = 0; i < mAvailableBooks.Count; i++) {
								Book book = null;
								if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", mAvailableBooks[i])) {
										//associate the title while we're here
										mBookTitleLookup.Add(book.Name, book.CleanTitle);
										//and associate any skills we can learn while we're here
										if (book.SkillsToLearn.Count > 0 || book.SkillsToReveal.Count > 0) {
												if (!mSkillAssociations.ContainsKey(book.Name)) {
														//create a new list since we can't guarantee that the book will stick around
														skillNames = new List <string>();
														skillNames.AddRange(book.SkillsToLearn);
														skillNames.AddRange(book.SkillsToReveal);
														mSkillAssociations.Add(book.Name, skillNames);
												}
										}
										//if this book specifies a template, add the book to the template
										//(skip this if the book only wants to be added to the world manually)
										if (!string.IsNullOrEmpty(book.DefaultTemplate) && !book.ManualPlacementOnly) {
												if (mTemplateLookup.TryGetValue(book.DefaultTemplate, out bookTemplate)) {
														bookTemplate.BookNames.SafeAdd(book.Name);
												}
										}
										//finally, see if we've aquired this book
										if (Flags.Check((uint)book.Status, (uint)BookStatus.Received, Flags.CheckType.MatchAny)) {
												AquiredBooks.Add(book.Name);
										}
								}
								book = null;
						}

						//now that we know mods are finished loading
						//get the skill levels for books
						SkillTemplates.Clear();
						GeneralTemplates.Clear();

						List <BookTemplate> skillLevelList = null;

						for (int i = 0; i < Templates.Count; i++) {
								bookTemplate = Templates[i];
								bookTemplate.SkillLevel = -1;
								//find the skills associated with this template
								//get their skill levels
								for (int j = 0; j < bookTemplate.BookNames.Count; j++) {
										if (mSkillAssociations.TryGetValue(bookTemplate.BookNames[j], out skillNames)) {
												for (int k = 0; k < skillNames.Count; k++) {
														//use the max associated skill level with this template
														//NOTE: make sure to use different templates for different skill levels
														//or you'll never see low-end skills on shelves!
														bookTemplate.SkillLevel = Mathf.Max(bookTemplate.SkillLevel, Skills.Get.SkillLevelByName(skillNames[k]));
												}
										}
								}
								//now that we have the associated skills on hand
								//separate the templates into generic templates vs. skill book templates
								if (bookTemplate.SkillLevel >= 0) {
										//put this book in each skill template list less than or equal to its skill level
										for (int j = 0; j <= bookTemplate.SkillLevel; j++) {
												//create a new list if necessary
												if (j < SkillTemplates.Count) {
														skillLevelList = SkillTemplates[j];
												} else {
														skillLevelList = new List<BookTemplate>();
														SkillTemplates.Add(skillLevelList);
												}
												skillLevelList.Add(bookTemplate);
										}
								} else {
										//general templates only have one list
										GeneralTemplates.Add(bookTemplate);
								}
						}

						//create randomized template orders
						//shuffle them using a shuffler based on the global seed
						System.Random skillShuffler = new System.Random(Profile.Get.CurrentGame.Seed);
						for (int i = 0; i < GeneralTemplates.Count; i++) {
								mRandomGeneralTemplateOrder.Add(i);
						}
						for (int i = 0; i < SkillTemplates.Count; i++) {
								List <int> newSkillLevel = new List <int>();
								for (int j = 0; j < SkillTemplates[i].Count; j++) {
										newSkillLevel.Add(j);
								}
								newSkillLevel.Shuffle(new System.Random(Profile.Get.CurrentGame.Seed));
								mRandomSkillTemplateOrder.Add(newSkillLevel);
						}

						mModsLoaded = true;
				}

				public static void ClearLog()
				{
						Mods.Get.Runtime.ResetProfileData("Book");
						Get.AquiredBooks.Clear();
				}

				#endregion

				#region guild librarian stuff

				public bool Library(string libraryName, out Library library)
				{
						return mLibraryLookup.TryGetValue(libraryName, out library);
				}

				public bool HasPlacedOrder(string libraryName, out LibraryCatalogueEntry order)
				{
						Library library = null;
						order = null;
						if (mLibraryLookup.TryGetValue(libraryName, out library)) {
								for (int i = 0; i < library.CatalogueEntries.Count; i++) {
										LibraryCatalogueEntry catalogueEntry = library.CatalogueEntries[i];
										if (catalogueEntry.HasBeenPlaced && !catalogueEntry.HasBeenDelivered) {
												order = catalogueEntry;
												break;
										}
								}
						}
						return order != null;
				}

				public void DeliverBookOrder(string libraryName)
				{
						LibraryCatalogueEntry order = null;
						if (HasPlacedOrder(libraryName, out order) && order.HasArrived && !order.HasBeenDelivered) {
								order.ARTPickUpTime = WorldClock.AdjustedRealTime;
								AquireBook(order.BookName);
						}
				}

				public bool TryToPlaceBookOrder(string libraryName, LibraryCatalogueEntry order, out string error)
				{
						error = string.Empty;
						LibraryCatalogueEntry existingOrder = null;
						if (HasPlacedOrder(libraryName, out existingOrder)) {
								error = "You've already placed an order at this library.";
								return false;
						}
						if (order.HasBeenDelivered) {
								error = "You've already ordered this book once.";
								return false;
						}
						if (!Player.Local.Inventory.InventoryBank.CanAfford(order.OrderPrice, order.CurrencyType)) {
								error = "You can't afford to place this order.";
								return false;
						}

						int numRemoved = 0;
						if (!Player.Local.Inventory.InventoryBank.TryToRemove(order.OrderPrice, ref numRemoved, order.CurrencyType)) {
								error = "You can't afford to place this order.";
								return false;
						}

						order.ARTOrderedTime = WorldClock.AdjustedRealTime;
						return true;
				}

				#endregion

				public IEnumerator BookStackItemsByFlagsAndOwner(int numBooksNeeded, BookFlags flags, int skillLevel, WorldItem owner, bool addToInstances, List <StackItem> avatarStackItems)
				{	//this is a big ugly function that gets books for a bookcase
						//this technique will probably change a lot over time
						//for now it works like this: chunks have a data map (C-X-X-X-DistributionData.png) indicating how many books should appear per bookshelf (blue channel)
						//as well has what proportion of those books should be SKILL books vs. regular books (red channel)
						//skills are arbitrarily rated from 0 - 4

						//this function is supposed to be 100% deterministic
						//so 4 co-op players can all generate the same books for the same shelf without having to pass data back & forth

						//use local variables, they create garbage but since we're using a coroutine they're necessary
						List <int> skillBookRandomOrder = null;
						List <string> booksFound = new List<string>();
						BookTemplate currentTemplate = null;
						string nextBookName = null;
						int numBooksSelected = 0;
						int shuffleIndexSkill = 0;
						int tieBreaker = Mathf.Abs(owner.GetHashCode());
						int numSkillBooks = 0;
						int numGeneralBooks = numBooksNeeded;
						int shuffleIndexGeneral = tieBreaker % mRandomGeneralTemplateOrder.Count;
						float wealthLevel = 0f;
						//distribution data tells us which skill levels to use and the ratio of general books to skill books
						Color32 distribution = GameWorld.Get.DistributionDataAtPosition(owner.Position);
						//wealth level controls how many books we actually get per shelf - 'books needed' is a desired amount
						//get a normalized wealth level - wealth is an int from 0 to x / total wealth flags (low to high)
						try {
								wealthLevel = ((float)FlagSet.GetFlagValue(flags.Wealth, tieBreaker, 1) / WealthFlagset.Flags.Count);
								if (skillLevel < 0) {//-1 means we didn't specify a skill level, so use the global distribution data
										skillLevel = distribution.r;
								}
								if (skillLevel > 0) {
										numSkillBooks = Mathf.Clamp(Mathf.CeilToInt(numSkillBooks * wealthLevel), 0, numSkillBooks);
										numGeneralBooks = Mathf.Clamp(Mathf.CeilToInt(numGeneralBooks * wealthLevel), 1, numGeneralBooks);
										//split between general & skill books - only do this if we actually need skill books
										numSkillBooks = Mathf.Clamp(Mathf.FloorToInt(numBooksNeeded * ((float)distribution.b / 255)), 0, numBooksNeeded);
										numGeneralBooks = Mathf.Clamp(numBooksNeeded - numSkillBooks, 0, numBooksNeeded);
										//get the random order list based on the skill level
										skillBookRandomOrder = mRandomSkillTemplateOrder[skillLevel];
										shuffleIndexSkill = tieBreaker % skillBookRandomOrder.Count;
								}
						} catch (Exception e) {
								Debug.LogError("Exception in book stack items, proceeding normally: " + e.ToString());
						}

						//take a break since getting the distribution probably took a while
						yield return null;

						//this can create garbage fast so make sure to get rid of it
						List <BookTemplate> templatesFound = new List<BookTemplate>();
						bool hasLooped = false;
						int startShuffleIndex = 0;
						int currentshuffleIndex = 0;
						int numBooksFound = 0;
						int maxLoops = 0;
						int currentLoop = 0;
						int currentTemplateIndex = 0;

						try {
								startShuffleIndex = skillBookRandomOrder[shuffleIndexSkill];
								currentshuffleIndex = startShuffleIndex;
								//we're going to go through the available book templates
								//and check the flags
								//each template has multiple books associated with it
								//so we'll check until the total number of books associated with the templates
								//is greater than or equal to the number of books we need
								while (!hasLooped && numBooksFound < numSkillBooks) {
										int templateIndex = skillBookRandomOrder[currentshuffleIndex];
										currentTemplate = SkillTemplates[skillLevel][templateIndex];
										if (currentTemplate.Flags.Check(flags) && templatesFound.SafeAdd(currentTemplate)) {
												//if the flags meet our criteria then set our generic items
												numBooksFound += currentTemplate.BookNames.Count;
										}
										//get the next shuffle index
										//this will cause a break if it loops
										currentshuffleIndex = skillBookRandomOrder.NextIndex(currentshuffleIndex, startShuffleIndex, out hasLooped);
								}
								//reset this to use in the next bit
								numBooksFound = 0;
								//now that we have our templates, get our book items
								//they're already in a random order so just get them in order
								numBooksFound = 0;
								maxLoops = numSkillBooks * 2;
								currentLoop = 0;
								currentTemplateIndex = 0;
								while (numBooksFound < numSkillBooks) {
										//go through each book template, grabbing one book along the way
										currentTemplate = templatesFound.NextItem(currentTemplateIndex);
										nextBookName = currentTemplate.BookNames.NextItem(tieBreaker + currentLoop);
										if (booksFound.SafeAdd(nextBookName)) {
												mBookItem.StackName = nextBookName;
												mBookItem.Subcategory = currentTemplate.Name;
												mBookItem.DisplayName = BookTitle(mBookItem.StackName);
												avatarStackItems.Add(mBookItem.ToStackItem());
										}
										currentTemplateIndex++;
										currentLoop++;
										if (currentLoop > maxLoops) {
												break;
										}
								}
								//creating the stack items was labor intensive
								//so yield here before moving on to the general books
								//we don't have to worry about our templates array getting nuked any more
						} catch (Exception e) {
								Debug.LogError("Exception in book stack items, proceeding normally: " + e.ToString());
						}
						yield return null;
						templatesFound.Clear();

						try {
								//same procedure with general books
								hasLooped = false;
								startShuffleIndex = mRandomGeneralTemplateOrder[shuffleIndexGeneral];
								currentshuffleIndex = startShuffleIndex;
								numBooksFound = 0;
								while (!hasLooped && numBooksFound < numGeneralBooks) {
										if (currentshuffleIndex < mRandomGeneralTemplateOrder.Count) {
												currentTemplate = GeneralTemplates[mRandomGeneralTemplateOrder[currentshuffleIndex]];
												if (currentTemplate.Flags.Check(flags) && templatesFound.SafeAdd(currentTemplate)) {
														numBooksFound += (currentTemplate.BookNames.Count / 2);//don't count on everything to come from one template
												}
										}
										currentshuffleIndex = mRandomGeneralTemplateOrder.NextIndex(currentshuffleIndex, startShuffleIndex, out hasLooped);
								}
								numBooksFound = 0;
								maxLoops = numGeneralBooks * 2;
								currentLoop = 0;
								currentTemplateIndex = 0;
								while (numBooksFound < numGeneralBooks) {
										//go through each book template, grabbing one book along the way
										currentTemplate = templatesFound.NextItem(currentTemplateIndex);
										nextBookName = currentTemplate.BookNames.NextItem(tieBreaker + currentLoop);
										if (booksFound.SafeAdd(nextBookName)) {
												mBookItem.StackName = nextBookName;
												mBookItem.Subcategory = currentTemplate.Name;
												mBookItem.DisplayName = BookTitle(mBookItem.StackName);
												avatarStackItems.Add(mBookItem.ToStackItem());
										}
										currentTemplateIndex++;
										currentLoop++;
										if (currentLoop > maxLoops) {
												break;
										}
								}
								//clear this before heading out
								templatesFound.Clear();
								templatesFound = null;
								//again that was pretty labor intensive
								//so wait a tick before shuffling and returning
						} catch (Exception e) {
								Debug.LogError("Exception in book stack items, proceeding normally: " + e.ToString());
						}

						yield return null;
						//shuffle the results to prevent the avatar stack items from going 'skill, skill, skill, general, general' etc
						//make sure to use the tiebreaker to get consistent results!
						System.Random shuffler = new System.Random(tieBreaker);
						avatarStackItems.Shuffle(shuffler);
						yield break;
				}

				public void InitializeBookAvatarGameObject(GameObject bookAvatarGameObject, string bookName, string templateName)
				{	//used to create dopplegangers for books
						BookTemplate template = null;
						if (!mTemplateLookup.TryGetValue(templateName, out template)) {
								template = DefaultTemplate;
						}
						int meshIndex = FlagSet.GetFlagValue(template.MeshIndex, bookName, 0);
						int textureIndex = FlagSet.GetFlagValue(template.TextureIndex, bookName, 0);
						int hashCode = Mathf.Abs(bookName.GetHashCode());
						int subTextureIndex = hashCode % BookTextures[textureIndex].Textures.Count;
						int submeshIndex = hashCode % BookMeshes[meshIndex].Meshes.Count;
						MeshFilter bookMf = bookAvatarGameObject.GetOrAdd <MeshFilter>();
						bookMf.sharedMesh = BookMeshes[meshIndex].Meshes[submeshIndex];
						MeshRenderer bookMr = bookAvatarGameObject.GetOrAdd <MeshRenderer>();
						Material bookMaterial = null;
						string matLookup = templateName + "_" + meshIndex.ToString() + "_" + textureIndex.ToString();

						if (!mBookMaterialLookup.TryGetValue(matLookup, out bookMaterial)) {
								//get textures
								Texture2D mainText = BookTextures[textureIndex].Textures[subTextureIndex];
								Texture2D maskText = BookTextures[textureIndex].MaskTextures[subTextureIndex];
								//get colors
								Color bindingColor = Colors.Get.ByName(template.BindingColor);
								Color coverColor = Colors.Get.ByName(template.CoverColor);
								Color decoCover = Colors.Get.ByName(template.DecoColor);
								//create and set up new material
								bookMaterial = new Material(BooksMaterial);
								bookMaterial.SetTexture("_MainTex", mainText);
								bookMaterial.SetTexture("_MaskTex", maskText);

								bookMaterial.SetColor("_EyeColor", bindingColor);
								bookMaterial.SetColor("_SkinColor", Colors.ColorFromString(bookName, 127));
								bookMaterial.SetColor("_HairColor", decoCover);
								//add to lookup so we can use it later
								mBookMaterialLookup.Add(matLookup, bookMaterial);
						}
						bookMr.sharedMaterial = bookMaterial;
				}

				public bool BookTemplateFromBookName(string bookName, out BookTemplate bookTemplate)
				{
						bookTemplate = null;
						List <BookTemplate> templateList = null;
						if (mBookTemplateAssociations.TryGetValue(bookName, out templateList)) {
								if (templateList.Count > 0) {
										int hashCode = Mathf.Abs(bookName.GetHashCode());
										bookTemplate = templateList[hashCode % templateList.Count];
								} else {
										bookTemplate = templateList[0];
								}
						}
						return bookTemplate != null;
				}

				public bool BookTemplateByName(string templateName, out BookTemplate template)
				{	//do we really need this any more?
						return mTemplateLookup.TryGetValue(templateName, out template);
				}

				public void BookStackItemsByName(List <string> specificBooks, List <StackItem> avatarStackItems)
				{	//used by fill bookshelf to fill with specific books
						Book book = null;
						for (int i = 0; i < specificBooks.Count; i++) {
								if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", specificBooks[i])) {
										//wuhoo, we found our book
										//now get the template
										BookTemplate template = null;
										if (BookTemplateFromBookName(specificBooks[i], out template)) {
												mBookItem.StackName = book.Name;
												mBookItem.Subcategory = template.Name;
												StackItem avatarStackItem = mBookItem.ToStackItem();
												avatarStackItems.Add(avatarStackItem);
										}
								}
						}
				}

				public bool BookByName(string bookName, out Book book)
				{
						//convenience function, probably unnecessary
						book = null;
						if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", bookName)) {
								return true;
						}
						return false;
				}

				public string BookTitle(string bookName)
				{
						string bookTitle = null;
						if (!mBookTitleLookup.TryGetValue(bookName, out bookTitle)) {
								bookTitle = Book.gDefaultBookTitle;
						}
						return bookTitle;
				}

				public bool BookAvatarStackItem(string bookName, out StackItem avatarStackItem)
				{
						Book book = null;
						avatarStackItem = null;
						if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", bookName)) {
								//wuhoo, we found our book
								//now get the template
								BookTemplate template = null;
								if (BookTemplateFromBookName(bookName, out template)) {
										mBookItem.StackName = book.Name;
										mBookItem.Subcategory = template.Name;
										avatarStackItem = mBookItem.ToStackItem();

										System.Object bookAvatarStateObject = null;
										if (avatarStackItem.GetStateOf <BookAvatar>(out bookAvatarStateObject)) {	//get state using generic method since it may be a stackitem
												BookAvatarState bookAvatarState = (BookAvatarState)bookAvatarStateObject;
												bookAvatarState.BookName = bookName;
												bookAvatarState.TemplateName = template.Name;
										}

								}
						}
						return avatarStackItem != null;
				}

				public StackItem RandomBookAvatarStackItem()
				{
						BookTemplate randomTemplate = Templates[UnityEngine.Random.Range(0, Templates.Count)];
						string randomBookName = randomTemplate.BookNames[UnityEngine.Random.Range(0, randomTemplate.BookNames.Count)];
						mBookItem.StackName = randomBookName;
						mBookItem.Subcategory = randomTemplate.Name;
						StackItem avatarStackItem = mBookItem.ToStackItem();
						return avatarStackItem;
				}

				public IEnumerator BookNamesByFlagsAndOwner(int numBooks, BookFlags flags, WorldItem owner, bool addToInstances, List <string> bookNames)
				{
						for (int i = 0; i < numBooks; i++) {
								bookNames.Add(mAvailableBooks[UnityEngine.Random.Range(0, mAvailableBooks.Count)]);
						}
						yield break;
				}

				public List <Book> BooksByStatusAndType(BookStatus bookStatus, BookType bookType)
				{
						List <Book> books = new List <Book>();
						foreach (string aquiredBook in AquiredBooks) {
								Book book = null;
								if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", aquiredBook)) {
										if (Flags.Check((uint)book.Status, (uint)bookStatus, Flags.CheckType.MatchAny)
										&&	Flags.Check((uint)book.TypeOfBook, (uint)bookType, Flags.CheckType.MatchAny)) {
												books.Add(book);
										}
								}
						}
						return books;
				}

				public static void ReadBook(string bookName, Action callBack)
				{
						Book book = null;
						if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", bookName)) {
								Get.AquiredBooks.Add(bookName);
								book.Status &= ~BookStatus.Dormant;
								//book.Status |= BookStatus.Received;//not received, only read
								book.Status |= BookStatus.PartlyRead;
								book.SealStatus = BookSealStatus.Broken;
								book.NumCopiesReceived++;
								//now that we've altered it, save it!
								Mods.Get.Runtime.SaveMod <Book>(book, "Book", bookName);
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.BookRead, WorldClock.Time);
								//launch the book reader
								Get.LaunchBookReader(book, callBack);
						}
				}

				public static void AquireBook(string bookName)
				{
						Book book = null;
						if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", bookName)) {
								Get.AquiredBooks.Add(bookName);
								book.Status &= ~BookStatus.Dormant;
								book.Status |= BookStatus.Received;
								book.NumCopiesReceived++;
								//now that we've altered it, save it!
								Mods.Get.Runtime.SaveMod <Book>(book, "Book", bookName);
								GUIManager.PostGainedItem(book);
								Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.BookAquire), WorldClock.Time);
						}
				}

				public static void ReadAll()
				{	//mainly a dev function to put all books in log and stress-test interface
						foreach (string availableBook in Get.mAvailableBooks) {
								Get.AquiredBooks.Add(availableBook);
						}
				}

				public static bool GetBookStatus(string bookName, out BookStatus status)
				{
						status = BookStatus.Dormant;
						Book book = null;
						bool foundBook = false;
						if (Mods.Get.Runtime.LoadMod <Book>(ref book, "Book", bookName)) {
								status = book.Status;
								foundBook = true;
						}
						return foundBook;
				}

				public BookFont GetFont(string fontName)
				{
						BookFont font = DefaultFont;
						foreach (BookFont currentFont in Fonts) {
								if (currentFont.FontName == fontName) {
										font = currentFont;
										break;
								}
						}
						return font;
				}

				public BookInk GetInk(string inkName)
				{
						BookInk ink = DefaultInk;
						foreach (BookInk currentInk in Inks) {
								if (currentInk.InkName == inkName) {
										ink = currentInk;
										break;
								}
						}
						return ink;			
				}

				protected void LaunchBookReader(Book book, Action callBack)
				{
						if (mBookReader != null) {
								GameObject.Destroy(mBookReader.gameObject);
						}
						GameObject bookReaderGameObject = GUIManager.SpawnNGUIChildEditor(this.gameObject, GUIManager.Get.NGUIBookReader, false);
						mBookReader = bookReaderGameObject.GetComponent <GUI.GUIBookReader>();
						mBookReader.OnFinishReading += callBack;
						GUIManager.SendEditObjectToChildEditor(bookReaderGameObject, book);
				}

				public void InstantiateBookVariations(BookTemplate prototype)
				{
						//this turns to be unnecessary?
				}
				//mostly lookups for quick access to book properties
				protected List <int> mRandomGeneralTemplateOrder = new List <int>();
				protected List <List <int>> mRandomSkillTemplateOrder = new List <List <int>>();
				protected GenericWorldItem mBookItem = new GenericWorldItem();
				protected List <string> mAvailableBooks = new List <string>();
				protected Dictionary <string, List <string>> mSkillAssociations = new Dictionary <string, List<string>>();
				protected Dictionary <string, string> mBookTitleLookup = new Dictionary <string, string>();
				protected Dictionary <string, List <BookTemplate>> mBookTemplateAssociations = new Dictionary <string, List <BookTemplate>>();
				protected Dictionary <string, BookTemplate> mTemplateLookup = new Dictionary <string, BookTemplate>();
				protected Dictionary <string, Material> mBookMaterialLookup = new Dictionary <string, Material>();
				protected Dictionary <string, Library> mLibraryLookup = new Dictionary <string, Library>();
				protected GUI.GUIBookReader mBookReader = null;

				#region editor functions

				#if UNITY_EDITOR
				public List <BookTemplate> SkillSubgroupTemplates = new List<BookTemplate>();
				public List <Book> SkillSubgroupBooks = new List<Book>();

				public void DrawEditor()
				{
						UnityEngine.GUI.color = Color.cyan;
						if (GUILayout.Button("\nSave Libraries to Disk\n", UnityEditor.EditorStyles.miniButton)) {
								EditorSaveLibraries();
						}
						if (GUILayout.Button("\nLoad Libraries from Disk\n", UnityEditor.EditorStyles.miniButton)) {
								EditorLoadLibraries();
						}
						if (GUILayout.Button("\nLoad Templates from Disk\n", UnityEditor.EditorStyles.miniButton)) {
								EditorLoadTemplates();
						}
						if (GUILayout.Button("\nSave Templates to Disk\n", UnityEditor.EditorStyles.miniButton)) {
								EditorSaveTemplates();
						}
						if (GUILayout.Button("\nCreate Template Books\n", UnityEditor.EditorStyles.miniButton)) {

								EditorCreateTemplateBooks();
						}
						if (GUILayout.Button("\nCreate books for skill subtypes and level\n", UnityEditor.EditorStyles.miniButton)) {

								CreateBooksForSkillSubtypesAndLevel();
						}
						if (GUILayout.Button("\nSave skill subgroup templates & books\n", UnityEditor.EditorStyles.miniButton)) {

								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
										Mods.Get.Editor.InitializeEditor();
								}

								foreach (BookTemplate template in SkillSubgroupTemplates) {
										//template.BindingColor = template.CoverColor;
										//template.DecoColor = template.BindingColor;
										Mods.Get.Editor.SaveMod(template, "BookTemplate", template.Name);
								}
								foreach (Book book in SkillSubgroupBooks) {
										Mods.Get.Editor.SaveMod(book, "Book", book.Name);
								}
						}
						if (GUILayout.Button("\nAuto-apply skill subgroup template flags\n")) {
								foreach (BookTemplate template in SkillSubgroupTemplates) {
										if (template.Name.Contains("Guild")) {
												template.Flags = ObjectClone.Clone <BookFlags>(GuildFlags);
										} else if (template.Name.Contains("Magic")) {
												template.Flags = ObjectClone.Clone <BookFlags>(MagicFlags);
										} else if (template.Name.Contains("Survival")) {
												template.Flags = ObjectClone.Clone <BookFlags>(SurvivalFlags);
										} else if (template.Name.Contains("Archaeology")) {
												template.Flags = ObjectClone.Clone <BookFlags>(ArchaeologyFlags);
										} else if (template.Name.Contains("Crafting")) {
												template.Flags = ObjectClone.Clone <BookFlags>(CraftingFlags);
										} else if (template.Name.Contains("Subterfuge")) {
												template.Flags = ObjectClone.Clone <BookFlags>(SubterfugeFlags);
										}
								}
						}

						if (GUILayout.Button("Edit Library")) {
								showLibrary = !showLibrary;
						}

						if (showLibrary) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
										Mods.Get.Editor.InitializeEditor();
								}

								if (GUILayout.Button("Add entry")) {
										Libraries[0].CatalogueEntries.Add(new LibraryCatalogueEntry());
								}
								int index = 0;
								foreach (LibraryCatalogueEntry entry in Libraries [0].CatalogueEntries) {
										index++;
										DrawCatalogueEntry(entry, index);
								}
						}
				}

				protected void DrawCatalogueEntry(LibraryCatalogueEntry entry, int index)
				{
						GUILayout.Label("Entry " + index.ToString());

						GUILayout.BeginHorizontal();
						entry.BookObject.StackName = Mods.ModsEditor.GUILayoutAvailable(entry.BookObject.StackName, "Book", true, "(None)", 200);
						entry.BookObject.DisplayName = GUILayout.TextField(entry.BookObject.DisplayName);
						entry.BookObject.Subcategory = Mods.ModsEditor.GUILayoutAvailable(entry.BookObject.Subcategory, "BookTemplate", 200);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label("Price in grains " + index.ToString());
						//entry.OrderPrice = UnityEditor.EditorGUILayout.IntField (entry.OrderPrice);
						GUILayout.Label("Delivery time (Real time) " + index.ToString());
						entry.DeliveryTimeInHours = UnityEditor.EditorGUILayout.IntField((int)entry.DeliveryTimeInHours);
						GUILayout.EndHorizontal();
				}

				protected void CreateBooksForSkillSubtypesAndLevel()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
								Mods.Get.Editor.InitializeEditor();
						}

						SkillSubgroupTemplates.Clear();
						SkillSubgroupBooks.Clear();
						List <SkillSaveState> skills = new List <SkillSaveState>();
						Dictionary <string,List <List <SkillSaveState>>> groupSort = new Dictionary<string, List<List<SkillSaveState>>>();
						List <string> skillNames = Mods.Get.Editor.Available("Skill");
						foreach (string skillName in skillNames) {
								//Debug.Log ("Checking skill name " + skillName);
								SkillSaveState skill = null;
								if (Mods.Get.Editor.LoadMod <SkillSaveState>(ref skill, "Skill", skillName)) {
										skills.Add(skill);
								} else {
										Debug.Log("Couldn't load skill save state " + skill);
								}
						}
						//go through each skill and sort them by category
						foreach (SkillSaveState skill in skills) {
								int skillLevel = skill.Info.SkillLevel;
								if (skillLevel < 99) {
										string group = skill.Info.SkillGroup;
										if (!string.IsNullOrEmpty(skill.Info.SkillSubgroup)) {
												group = group + "-" + skill.Info.SkillSubgroup;
										}
										List <List<SkillSaveState>> groupList = null;
										//sort the skills into group/subgroups
										if (!groupSort.TryGetValue(group, out groupList)) {
												groupList = new List<List<SkillSaveState>>();
												groupSort.Add(group, groupList);
										}
										//create a list for each skill level within the subgroup
										for (int i = 0; i <= skillLevel; i++) {
												if (i <= groupList.Count) {
														groupList.Add(new List<SkillSaveState>());
												}
										}
										//add the skill to the group
										groupList[skillLevel].Add(skill);
								}
						}
						//okay now we're sorted by category / subcategory and skill level
						//for each of these we need to create a template
						//and for each individual skill we need to create a book
						foreach (KeyValuePair <string, List <List<SkillSaveState>>> templatePair in groupSort) {
								string templateName = templatePair.Key;
								string description = "Skill Template for " + templatePair.Key;
								bool hasVolumes = false;
								int volumeNumber = -1;
								List <List <SkillSaveState>> skillList = templatePair.Value;
								//see if there's more than one skill level in here
								HashSet <int> levels = new HashSet <int>();
								for (int i = 0; i < skillList.Count; i++) {
										//if a list has no skills then skip it
										if (skillList[i].Count > 0) {
												levels.Add(i);
										}
								}
								if (levels.Count > 1) {
										hasVolumes = true;
										volumeNumber = 1;
								}
								List <Book> booksInThisTemplate = new List<Book>();
								foreach (int skillLevel in levels) {
										BookTemplate skillLevelTemplate = new BookTemplate();

										SkillSubgroupTemplates.Add(skillLevelTemplate);

										skillLevelTemplate.Name = templateName;
										skillLevelTemplate.Description = description;
										if (hasVolumes) {
												skillLevelTemplate.Name = templateName + " Vol. " + (skillLevel + 1).ToString();
												skillLevelTemplate.Description = description + " (Level " + skillLevel.ToString() + ")";
										}
										//create a book for each skill
										foreach (SkillSaveState skillState in skillList [skillLevel]) {
												Book skillBook = new Book();

												SkillSubgroupBooks.Add(skillBook);

												skillBook.Name = skillState.Name;
												skillBook.Title = skillState.Info.GivenName;
												if (string.IsNullOrEmpty(skillBook.Title)) {
														skillBook.Title = skillState.Name;
												}
												if (string.IsNullOrEmpty(skillState.Info.SkillSubgroup)) {
														skillBook.Title = skillState.Info.SkillGroup + ": " + skillBook.Title;
												} else {
														skillBook.Title = skillState.Info.SkillGroup + " (" + skillState.Info.SkillSubgroup + "): " + skillBook.Title;
												}
												skillBook.SkillsToLearn.Add(skillState.Name);
												if (!string.IsNullOrEmpty(skillState.Requirements.PrerequisiteSkillName)) {
														skillBook.SkillsToReveal.Add(skillState.Requirements.PrerequisiteSkillName);
												}
												skillBook.TypeOfBook = BookType.Book;
												skillBook.Type = "Book";
												//add the book name to the template
												skillLevelTemplate.BookNames.Add(skillBook.Name);
												skillLevelTemplate.CoverColor = "Skill" + skillState.Info.SkillGroup;					
										}
								}
						}
				}

				public int EditorTextureIndex(string textureName)
				{
						if (!Manager.IsAwake <GameWorld>()) {
								Manager.WakeUp <GameWorld>("__WORLD");
						}

						for (int i = 0; i < BookTextures.Count; i++) {
								for (int j = 0; j < BookTextures[i].Textures.Count; j++) {
										if (BookTextures[i].Textures[j].name == textureName) {
												return 1 << i;
										}
								}
						}
						return 0;
				}

				public int EditorMeshIndex(string meshName)
				{
						if (!Manager.IsAwake <GameWorld>()) {
								Manager.WakeUp <GameWorld>("__WORLD");
						}

						for (int i = 0; i < BookMeshes.Count; i++) {
								for (int j = 0; j < BookMeshes[i].Meshes.Count; j++) {
										if (BookMeshes[i].Meshes[j].name == meshName) {
												return 1 << i;
										}
								}
						}
						return 0;
				}

				public void EditorSaveLibraries()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
								Mods.Get.Editor.InitializeEditor();
						}

						foreach (Library library in Libraries) {
								Mods.Get.Editor.SaveMod <Library>(library, "Library", library.Name);
						}
				}

				public void EditorLoadLibraries()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
								Mods.Get.Editor.InitializeEditor();
						}

						Libraries.Clear();
						List <string> availableLibraries = Mods.Get.Editor.Available("Library");
						foreach (string availableLibrary in availableLibraries) {
								Library library = null;
								if (Mods.Get.Editor.LoadMod <Library>(ref library, "Library", availableLibrary)) {
										Libraries.Add(library);
								}
						}
				}

				public void EditorSaveTemplates()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
								Mods.Get.Editor.InitializeEditor();
						}

						foreach (BookTemplate template in Templates) {
								Mods.Get.Editor.SaveMod <BookTemplate>(template, "BookTemplate", template.Name);
						}
				}

				public void EditorLoadTemplates()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
								Mods.Get.Editor.InitializeEditor();
						}

						Templates.Clear();
						List <string> availableTemplates = Mods.Get.Editor.Available("BookTemplate");
						foreach (string availableTemplate in availableTemplates) {
								BookTemplate bt = null;
								if (Mods.Get.Editor.LoadMod <BookTemplate>(ref bt, "BookTemplate", availableTemplate)) {
										Templates.Add(bt);
								}
						}
				}

				public void EditorCreateTemplateBooks()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
								Mods.Get.Editor.InitializeEditor();
						}

						List <string> availableBooks = Mods.Get.Editor.Available("Book");

						for (int i = 0; i < Templates.Count; i++) {
								for (int j = 0; j < Templates[i].BookNames.Count; j++) {
										string existingBookName = Templates[i].BookNames[j];
										if (!availableBooks.Contains(existingBookName)) {
												Book newBook = new Book();
												newBook.Name = existingBookName;
												Mods.Get.Editor.SaveMod <Book>(newBook, "Book", existingBookName);
										}
								}
						}
				}

				protected bool showLibrary = false;
				#endif
				#endregion

		}

		[Serializable]
		public class Library : Mod
		{
				public string DisplayName = "Guild Library";
				public string RequiredSkill = "GuildLibrary";
				public string Motto = string.Empty;
				public List <LibraryCatalogueEntry> CatalogueEntries = new List <LibraryCatalogueEntry>();
		}

		[Serializable]
		public class LibraryCatalogueEntry
		{
				public LibraryCatalogueEntry()
				{
						//StackName is book name;
						//Subcategory is template name;
						BookObject = new GenericWorldItem("Books", "BookAvatar");
						BookObject.State = "Default";
						BookObject.Subcategory = Books.DefaultBookTemplate;
				}

				public string BookName {
						get {
								return BookObject.StackName;
						}
				}

				public GenericWorldItem BookObject = null;
				public float RelativeOrderPrice = 0f;

				public int OrderPrice {
						get {
								return Mathf.FloorToInt(Globals.GuildLibraryBasePrice * RelativeOrderPrice);
						}
				}

				public int DisplayOrder = 0;
				public WICurrencyType CurrencyType = WICurrencyType.A_Bronze;
				public int DeliveryTimeInHours = 1;
				public double ARTOrderedTime = -1;
				public double ARTPickUpTime = -1;

				public bool HasBeenPlaced {
						get {
								return ARTOrderedTime > 0;
						}
				}

				public bool HasArrived {
						get {
								if (HasBeenPlaced && !HasBeenDelivered) {
										return WorldClock.AdjustedRealTime > (ARTOrderedTime + WorldClock.HoursToSeconds (DeliveryTimeInHours));
								}
								return false;
						}
				}

				public bool HasBeenDelivered {
						get {
								return ARTPickUpTime > 0;
						}
				}
		}

		[Serializable]
		public class BookTemplate : Mod
		{
				public string RandomBookName(int tieBreaker)
				{
						return BookNames[UnityEngine.Random.Range(0, BookNames.Count)];
				}

				public bool CanBeReadWithoutAquiring {
						get {
								switch (TemplateType) {
										case BookType.Map:
										case BookType.Parchment:
										case BookType.Scrap:
												return true;

										case BookType.Envelope:
										case BookType.Scroll:
										default:
												return false;
								}
						}
				}

				public List <string> BookNames = new List <string>();
				public int NumInstances = 10;
				[FrontiersColorAttribute]
				public string BindingColor;
				[FrontiersColorAttribute]
				public string CoverColor;
				[FrontiersColorAttribute]
				public string DecoColor;
				public BookFlags Flags = new BookFlags();
				[FrontiersBitMaskAttribute("BookMesh")]
				public int MeshIndex;
				[FrontiersBitMaskAttribute("BookTexture")]
				public int TextureIndex;
				public int SkillLevel = 0;
				public BookType TemplateType = BookType.Book;
				[XmlIgnore]
				[NonSerialized]
				public GameObject Prototype;
		}

		[Serializable]
		public class BookFlags : WIFlags
		{
				public bool Check(BookFlags otherFlags)
				{
						//check our flags against the other flags
						return// (true);//TEMP
				Flags.Check(Occupation, otherFlags.Occupation, Flags.CheckType.MatchAny)
						&& Flags.Check(Alignment, otherFlags.Alignment, Flags.CheckType.MatchAny)
						&& Flags.Check(Faction, otherFlags.Faction, Flags.CheckType.MatchAny)
						&& Flags.Check(Region, otherFlags.Region, Flags.CheckType.MatchAny)
						&& Flags.Check(Subject, otherFlags.Subject, Flags.CheckType.MatchAny)
						&& Flags.Check(SkillSubject, otherFlags.SkillSubject, Flags.CheckType.MatchAny)
						&& Flags.Check(Wealth, otherFlags.Wealth, Flags.CheckType.MatchAny);
				}

				[FrontiersBitMaskAttribute("SkillSubject")]
				public int SkillSubject;
		}
}