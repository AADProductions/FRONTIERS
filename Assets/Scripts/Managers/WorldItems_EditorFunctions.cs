using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Runtime.Serialization;
using Frontiers.World;
using Frontiers.World.Gameplay;
using ExtensionMethods;
using Frontiers.Data;
using System.Text;
using System.Reflection;

namespace Frontiers.World
{
	public partial class WorldItems
	{
		#if UNITY_EDITOR
		public string [] CategoryNames {
			get {
				if (categoryNames == null) {
					categoryNames = new string [Categories.Count];
					for (int i = 0; i < Categories.Count; i++) {
						categoryNames[i] = Categories[i].Name;
					}
				}
				return categoryNames;
			}
		}

		protected string[] categoryNames = null;

		public override void Initialize()
		{
			if (!Application.isPlaying && Application.isEditor) {
				int numItems = 0;

				List <WorldItemPack> packs = new List<WorldItemPack>(WorldItemPacks);
				//structures will have set this by now
				packs.AddRange(Structures.Get.DynamicWorldItemPacks);

				foreach (WorldItemPack pack in packs) {
					if (!mWorldItemPackLookup.ContainsKey(pack.Name)) {
						mWorldItemPackLookup.Add(pack.Name, pack);
					}
					pack.RefreshLookup();
					numItems += pack.Prefabs.Count;

					int index = 0;
					foreach (GameObject prefab in pack.Prefabs) {
						if (prefab == null) {
							//////////////Debug.Log ("Prefab was null in pack " + pack.Name + " at index " + index);
						}
						index++;
						WorldItem worlditem = null;
						WIStates states = null;
						if (prefab.HasComponent <WorldItem>(out worlditem)) {
							worlditem.IsTemplate = true;
							worlditem.ClearStackItem();
							worlditem.InitializeTemplate();

							if (string.IsNullOrEmpty(worlditem.Props.Name.PrefabName)) {
								worlditem.Props.Name.PrefabName = prefab.name;
							}
							worlditem.Props.Name.PackName = pack.Name;
							if (string.IsNullOrEmpty(worlditem.Props.Name.FileName)) {
								worlditem.Props.Name.FileName = prefab.name;
							}
							if (string.IsNullOrEmpty(worlditem.Props.Name.DisplayName)) {
								worlditem.Props.Name.DisplayName = WorldItems.CleanWorldItemName(worlditem.Props.Name.PrefabName);
							}
							worlditem.Props.Local.PreviousMode = WIMode.Frozen;
							worlditem.Props.Local.Mode = WIMode.Frozen;
						}
						if (worlditem.rigidbody != null) {
							worlditem.Props.Global.UseRigidBody = true;
						} else {
							worlditem.Props.Global.UseRigidBody = false;
						}

						if (worlditem.gameObject.tag == "Untagged") {
							worlditem.gameObject.tag = Globals.TagGroundStone;
						}
						worlditem.Props.Global.ParentTag = worlditem.gameObject.tag;
						worlditem.Props.Global.ParentLayer = worlditem.gameObject.layer;

						if (worlditem.collider != null) {
							Type type = worlditem.collider.GetType();
							switch (type.ToString()) {
								case "BoxCollider":
									worlditem.Props.Global.ParentColliderType = WIColliderType.Box;
									break;

								case "SphereCollider":
									worlditem.Props.Global.ParentColliderType = WIColliderType.Capsule;
									break;

								case "CapsuleCollider":
									worlditem.Props.Global.ParentColliderType = WIColliderType.Sphere;
									break;

								case "MeshCollider":
									MeshCollider mc = worlditem.GetComponent <MeshCollider>();
									if (mc.convex) {
										worlditem.Props.Global.ParentColliderType = WIColliderType.ConvexMesh;
									} else {
										worlditem.Props.Global.ParentColliderType = WIColliderType.Mesh;
									}
									break;
							}
						} else {
							worlditem.Props.Global.ParentColliderType = WIColliderType.None;
						}
					}
				}

				LoadCategoriesEditor();
				WorldItemPacks.Sort();

				GC.Collect();
			}
			mInitialized = true;
		}

		public Collider TinySizeObject;
		public Collider SmallSizeObject;
		public Collider MediumSizeObject;
		public Collider LargeSizeObject;
		public Collider HugeSizeObject;
		protected bool selectPack = true;
		protected bool waitForMouseUp = false;
		protected GameObject lastPlaced = null;
		protected string categoryEntryName	= string.Empty;
		protected string categoryRename = string.Empty;
		protected bool addWorldItemToCat	= false;
		protected string wiPackSelection = string.Empty;
		protected WICategory selectedCategory	= null;
		protected WICategory deletedCategory = null;
		public string ThumbnailPack;
		public int ThumbnailTimeout = 5;
		public int StartPack = 0;
		public int EndPack = 0;

		public void SaveCategoriesEditor()
		{
			if (!Manager.IsAwake <Mods>()) {
				Manager.WakeUp <Mods>("__MODS");
			}
			Mods.Get.Editor.InitializeEditor();
			Mods.Get.Editor.SaveMods <WICategory>(Categories, "Category");
		}

		public void LoadCategoriesEditor()
		{
			if (!Manager.IsAwake <Mods>()) {
				Manager.WakeUp <Mods>("__MODS");
			}
			Mods.Get.Editor.InitializeEditor();
			//////////////Debug.Log ("Loading categories editor");
			Categories.Clear();
			Mods.Get.Editor.LoadAvailableMods <WICategory>(Categories, "Category"); 
			mWICategoryLookup.Clear();
			foreach (WICategory category in Categories) {
				//////////////Debug.Log ("Adding " + category.Name + " to lookup");
				mWICategoryLookup.Add(category.Name, category);
				category.Initialize();
			}
			//////////////Debug.Log ("lookup count: " + mWICategoryLookup.Count.ToString ());
			Categories.Sort();
			categoryNames = null;
		}

		public static WIState CreateTemplateState(WorldItem worlditem, string stateName, GameObject itemToCopy)
		{
			WIState state = new WIState();
			MeshFilter mf = itemToCopy.GetComponent <MeshFilter>();
			MeshRenderer mr = itemToCopy.GetComponent <MeshRenderer>();

			if (mf == null || mr == null) {
				//Debug.Log ("Mesh filter or mesh renderer was null in item to copy");
				return null;
			}

			GameObject newStateBase = worlditem.gameObject.CreateChild(stateName).gameObject;
			MeshFilter stateMf = newStateBase.AddComponent <MeshFilter>();
			MeshRenderer stateMr = newStateBase.AddComponent <MeshRenderer>();
			stateMf.sharedMesh = mf.sharedMesh;
			stateMr.sharedMaterials = mr.sharedMaterials;
			//add this after the mesh filter so it gets sized right
			BoxCollider stateBc = newStateBase.AddComponent <BoxCollider>();

			state.Name = stateName;
			state.StateObject = newStateBase;
			state.StateRenderer = stateMr;
			state.StateCollider = stateBc;

			if (worlditem.States == null) {
				worlditem.States = worlditem.gameObject.AddComponent <WIStates>();
			}
			worlditem.States.States.Add(state);

			worlditem.Colliders.Clear();
			worlditem.Renderers.Clear();

			return state;
		}

		public void ExportCategoriesDatabase()
		{
			string csvPath = @"Artwork\Packs\WorldItemPacks\Thumbnails\CatsDatabase.csv";
			string delimiter = ",";
			List <List <string>> db = new List<List<string>>();
			List <string> topRow = new List <string>();
			topRow.Add("PACK");
			topRow.Add("ITEM");
			foreach (WICategory cat in Categories) {
				topRow.Add(cat.Name);
			}
			db.Add(topRow);
			foreach (WorldItemPack pack in WorldItemPacks) {
				foreach (GameObject prefab in pack.Prefabs) {
					WorldItem worlditem = prefab.GetComponent <WorldItem>();
					if (worlditem != null) {
						List <string> catWiList = new List<string>();
						catWiList.Add(worlditem.Props.Name.PackName);
						catWiList.Add(worlditem.Props.Name.PrefabName);
						foreach (WICategory cat in Categories) {
							List <string> catEntry = new List<string>();
							foreach (GenericWorldItem gwi in cat.GenericWorldItems) {
								if (gwi.PackName == worlditem.Props.Name.PackName && gwi.PrefabName == worlditem.Props.Name.PrefabName) {
									//check for stack name and display name etc
									if (string.IsNullOrEmpty(gwi.StackName)) {
										gwi.StackName = worlditem.Props.Name.StackName;
									}
									if (string.IsNullOrEmpty(gwi.DisplayName)) {
										gwi.DisplayName = worlditem.Props.Name.DisplayName;
									}
									catEntry.Add(gwi.InstanceWeight.ToString());
								}
							}
							catWiList.Add(catEntry.JoinToString(" "));
						}
						db.Add(catWiList);
					}
				}
			}
			StringBuilder sb = new StringBuilder();  
			for (int i = 0; i < db.Count; i++) {
				sb.AppendLine(db[i].JoinToString(delimiter));
				//Debug.Log ("Added line");
			}
			csvPath = System.IO.Path.Combine(Application.dataPath, csvPath);
			System.IO.File.WriteAllText(csvPath, sb.ToString()); 
		}

		public void ExportDatabase()
		{
			string csvPath = @"Artwork\Packs\WorldItemPacks\Thumbnails\Database.csv";
			string thumbnailPath = @"Artwork\Packs\WorldItemPacks\Thumbnails\";
			thumbnailPath = System.IO.Path.Combine(Application.dataPath, thumbnailPath);
			string delimiter = "\t";
			List <List <string>> db = new List<List<string>>();
			foreach (WorldItemPack pack in WorldItemPacks) {
				foreach (GameObject prefab in pack.Prefabs) {
					string thumbNail = System.IO.Path.Combine(thumbnailPath, prefab.name + ".png");
					if (System.IO.File.Exists(thumbNail)) {
						WorldItem worlditem = prefab.GetComponent <WorldItem>();
						List <string> variations = new List<string>();
						WIStates states = null;
						if (worlditem.gameObject.HasComponent <WIStates>(out states)) {
							foreach (WIState state in states.States) {
								variations.Add(state.Name);
							}
						}
						string variationsString = variations.JoinToString(", ");
						if (variationsString.Contains("_")) {
							variationsString = "";
						}
						string baseValueString = worlditem.Props.Global.BaseCurrencyValue.ToString();
						if (baseValueString == "0") {
							baseValueString = "";
						}
						db.Add(new List<string>() {
							prefab.name,
							"=image(\"http://www.atomicagedog.com/frontiers/worlditemthumbnails/" + System.Uri.EscapeUriString(prefab.name) + ".png\", 1)",
							worlditem.Props.Global.Weight.ToString(),
							worlditem.Props.Global.Flags.Size.ToString(),
							worlditem.Props.Global.Flags.BaseRarity.ToString(),
							worlditem.Props.Global.MaterialType.ToString(),
							variationsString,
							baseValueString
						}
						);
					} else {
						//Debug.Log ("Skipping " + prefab.name + ", not in thumbnails");
					}
				}
			}
			StringBuilder sb = new StringBuilder();  
			for (int i = 0; i < db.Count; i++) {
				sb.AppendLine(db[i].JoinToString(delimiter));
				//Debug.Log ("Added line");
			}
			csvPath = System.IO.Path.Combine(Application.dataPath, csvPath);
			System.IO.File.WriteAllText(csvPath, sb.ToString()); 
		}

		public void GenerateAssetThumbnails()
		{
			foreach (WorldItemPack pack in WorldItemPacks) {
				if (pack.Name == ThumbnailPack) {
					EndPack = Mathf.Min(EndPack, pack.Prefabs.Count);
					for (int i = StartPack; i <= EndPack; i++) {
						GameObject prefab = pack.Prefabs[i];
						string path = @"Artwork\Packs\WorldItemPacks\Thumbnails\";
						Texture2D assetPreview = UnityEditor.AssetPreview.GetAssetPreview(prefab);
						string assetName = prefab.name;
						bool timeout = false;
						int counter = 0;
						while (assetPreview == null) {
							counter++;
							System.Threading.Thread.Sleep(ThumbnailTimeout);
							if (counter > 15) {
								timeout = true;
								break;
							}
						}
						if (timeout) {
							//Debug.Log ("ASSET PREVIEW WAS NULL FOR " + prefab.name);
						} else {
							//Debug.Log ("WRITING ASSET PREVIEW WAS FOR " + prefab.name);
							//Texture2D flippedTexture = new Texture2D (assetPreview.width, assetPreview.height);
							//flippedTexture.SetPixels (assetPreview.GetPixels ());
							//Hydrogen.Texture.FlipVertically (flippedTexture);
							var bytes = assetPreview.EncodeToPNG();
							path = System.IO.Path.Combine(Application.dataPath, path);
							path = System.IO.Path.Combine(path, assetName + ".png");
							////Debug.Log (path);
							if (System.IO.File.Exists(path)) {
								System.IO.File.Delete(path);
								System.Threading.Thread.Sleep(15);
							}
							System.IO.File.WriteAllBytes(path, bytes);
						}
					}
				}
			}
		}

		public void HandleSceneGUI()
		{
			bool drawStructureEditor = true;
			if (EditorStructureParent == null ||
			       EditorSelectedItem == string.Empty ||
			       EditorSepectedPack == string.Empty) {
				drawStructureEditor = false;
			}

			deletedCategory = null;

			if (drawStructureEditor) {
				Event e = Event.current;
				if (e.type == EventType.mouseDown && e.button == 0) {

					Vector2 mouse = Event.current.mousePosition;
					mouse.y = Camera.current.pixelHeight - mouse.y + 20;
					Ray ray = Camera.current.ScreenPointToRay(mouse);
					RaycastHit hit;
					GameObject thingIHit = null;
					//bool hitSomething = false;
					if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
						thingIHit = hit.collider.gameObject;
						//hitSomething 	= true;
						if (thingIHit.HasComponent <WorldItem>()) {
							lastPlaced = thingIHit;
						}
					}
					if (e.shift && e.control) {
						if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
							GameObject packPrefab = null;
							foreach (WorldItemPack pack in WorldItemPacks) {
								if (pack.Name == EditorSepectedPack) {
									foreach (GameObject prefab in pack.Prefabs) {
										if (prefab.name == EditorSelectedItem) {
											packPrefab = prefab;
										}
									}
								}
							}
							if (packPrefab != null) {
//	//						//Debug.Log ("Instantiating prefab in structure");
//							GameObject instantiatedPrefab 				= PrefabUtility.InstantiatePrefab (packPrefab) as GameObject;
//							instantiatedPrefab.transform.parent 		= worlditems.EditorStructureParent.transform;
//							instantiatedPrefab.transform.localRotation	= Quaternion.identity;
//							WorldItem worlditem = instantiatedPrefab.GetComponent <WorldItem> ( );
//							Vector3 hitPoint = worlditems.EditorStructureParent.transform.InverseTransformPoint (hit.point);
//							hitPoint.x = (float) ((float) Math.Truncate (10 * hitPoint.x) / 10);
//							hitPoint.z = (float) ((float) Math.Truncate (10 * hitPoint.z) / 10);
//							instantiatedPrefab.transform.localPosition = hitPoint + worlditem.Props.PivotOffset;
//
//							instantiatedPrefab.transform.localScale		= Vector3.one * worlditem.Props.Global.ScaleModifier;
//	//						//Debug.Log (worlditem.Props.Global.BaseRotation);
//							instantiatedPrefab.transform.Rotate (worlditem.Props.Global.BaseRotation);
//
//							lastPlaced = instantiatedPrefab;
							}
						}
					} else if (e.control) { //rotate
						if (lastPlaced != null && thingIHit != null && thingIHit == lastPlaced) {
							GameObject rotator = new GameObject("rotator");
							rotator.transform.position = lastPlaced.transform.position;
							Transform lastParent = lastPlaced.transform.parent;
							lastPlaced.transform.parent = rotator.transform;
							rotator.transform.Rotate(0f, 45f, 0f);
							lastPlaced.transform.parent = lastParent;
							GameObject.DestroyImmediate(rotator);
						}
					}
				} else if (e.type == EventType.keyDown) {
					if (lastPlaced == null) {
						return;
					}
					Vector3 movement = new Vector3();
					bool swap = false;
					bool move = false;
					bool next = false;
					bool findFloor = false;
					bool switchFocus	= false;

					switch (e.keyCode) {
						case KeyCode.V:
							switchFocus = true;
							break;
						case KeyCode.W:
							move = true;
							movement = Vector3.forward * 0.1f;
							break;

						case KeyCode.A:
							move = true;
							movement = Vector3.left * 0.1f;
							;
							break;

						case KeyCode.D:
							move = true;
							movement = Vector3.right * 0.1f;
							;
							break;

						case KeyCode.S:
							move = true;
							movement = Vector3.back * 0.1f;
							;
							break;

						case KeyCode.Z:
							swap = true;
							break;

						case KeyCode.X:
							swap = true;
							next = true;
							break;

						case KeyCode.C:
							move = true;
							findFloor = true;
							break;

						default:
							break;
					}

					if (switchFocus) {
						UnityEditor.Selection.activeGameObject = lastPlaced;
						return;
					}

					if (move) {
						GameObject mover = new GameObject("mover");
						mover.transform.position = lastPlaced.transform.position;
						Transform lastParent = lastPlaced.transform.parent;
						lastPlaced.transform.parent = mover.transform;

						if (findFloor) {
							mover.transform.position += Vector3.up;
							RaycastHit floorInfo;
							if (Physics.Raycast(mover.transform.position, Vector3.down, out floorInfo, 15.0f, Globals.LayerSolidTerrain)) {
								Vector3 offset = Vector3.zero;
								if (lastPlaced.collider != null) {
									offset.y = lastPlaced.collider.bounds.extents.y;
								}
								mover.transform.position = floorInfo.point + offset;
							} else {
								mover.transform.position = mover.transform.position + Vector3.up * 0.1f;
							}
							lastPlaced.transform.parent = lastParent;
						} else {
							mover.transform.position += movement;
							lastPlaced.transform.parent = lastParent;
						}
						GameObject.DestroyImmediate(mover);
					} else if (swap) {
						if (lastPlaced == null) {
							return;
						}
						EditorSelectedItem = lastPlaced.name;
						WorldItemPack currentPack = null;
						foreach (WorldItemPack itemPack in WorldItemPacks) {
							foreach (GameObject wiPrefab in itemPack.Prefabs) {
								if (wiPrefab.name == EditorSelectedItem) {
									currentPack = itemPack;
									EditorSepectedPack = currentPack.Name;
									break;
								}
							}
						}
						int currentPrefabIndex = 0;
						if (currentPack.Prefabs.Count == 0) {
							return;
						}
						for (int i = 0; i < currentPack.Prefabs.Count; i++) {
							if (currentPack.Prefabs[i].name == EditorSelectedItem) {
								currentPrefabIndex = i;
								break;
							}
						}
						if (next) {
							currentPrefabIndex++;
							if (currentPrefabIndex >= currentPack.Prefabs.Count) {
								currentPrefabIndex = 0;
							}
						} else {
							currentPrefabIndex--;
							if (currentPrefabIndex < 0) {
								currentPrefabIndex = currentPack.Prefabs.Count - 1;
							}
						}
						GameObject newPrefab = UnityEditor.PrefabUtility.InstantiatePrefab(currentPack.Prefabs[currentPrefabIndex]) as GameObject;
						newPrefab.transform.parent = lastPlaced.transform;
						newPrefab.transform.localPosition = Vector3.zero;
						newPrefab.transform.parent = lastPlaced.transform.parent;
						WorldItem worlditem = newPrefab.GetComponent <WorldItem>();
						newPrefab.transform.Rotate(worlditem.Props.Global.BaseRotation);
						//newPrefab.transform.localScale = Vector3.one * worlditem.Props.Global.ScaleModifier;
						newPrefab.transform.localRotation = Quaternion.identity;
						newPrefab.transform.Rotate(worlditem.Props.Global.BaseRotation);
						GameObject.DestroyImmediate(lastPlaced);
						lastPlaced = newPrefab;
					}
				}
				UnityEditor.Selection.activeGameObject = gameObject;
			}
		}

		public void DrawEditor()
		{
			UnityEditor.EditorStyles.textField.wordWrap = true;
			if (Application.isPlaying) {
				GUILayout.Label ("Last player position: " + LastPlayerPosition.ToString ());

				GUILayout.Label ("----\n");
				if (mInitialized) {
					UnityEngine.GUI.color = Color.green;
					GUILayout.Label ("\n---- Active: " + ActiveWorldItems.Count.ToString () + "----\n");
					int i = 0;
					foreach (WorldItem w in ActiveWorldItems) {
						//for (int i = 0; i < ActiveSet.Count; i++) {
						//WorldItem w = ActiveSet[i];
						if (w != null) {
							if (ActiveRadiusComparer.IsDirty (w)) {
								UnityEngine.GUI.color = Color.magenta;
							} else {
								UnityEngine.GUI.color = Color.green;
							}
							GUILayout.Button (w.name + " - " + w.DistanceToPlayer.ToString ("0.##"));
						} else {
							GUILayout.Button ("(NULL)");
						}
						i++;
					}
					UnityEngine.GUI.color = Color.yellow;
					GUILayout.Label ("\n---- Visible: " + VisibleWorldItems.Count.ToString () + "----\n");
					i = 0;
					foreach (WorldItem w in VisibleWorldItems) {
						//for (int i = 0; i < VisibleSet.Count; i++) {
						//WorldItem w = VisibleSet[i];
						if (w != null) {
							if (VisibleRadiusComparer.IsDirty (w)) {
								UnityEngine.GUI.color = Color.magenta;
							} else {
								UnityEngine.GUI.color = Color.Lerp (Color.green, Color.yellow, 0.25f);
							}
							GUILayout.Button (w.name + " - " + w.DistanceToPlayer.ToString ("0.##"));
						} else {
							GUILayout.Button ("(NULL)");
						}
						i++;
					}
					UnityEngine.GUI.color = Color.red;
					GUILayout.Label ("\n---- Invisible: " + InvisibleWorldItems.Count.ToString () + "----\n");
					i = 0;
					foreach (WorldItem w in InvisibleWorldItems) {
						//for (int i = 0; i < InvisibleSet.Count; i++) {
						//WorldItem w = InvisibleSet[i];
						if (w != null) {
							if (InvisibleRadiusComparer.IsDirty (w)) {
								UnityEngine.GUI.color = Color.magenta;
							} else {
								UnityEngine.GUI.color = Color.Lerp (Color.green, Color.yellow, 0.75f);
							}
							GUILayout.Button (w.name + " - " + w.DistanceToPlayer.ToString ("0.##"));
						} else {
							GUILayout.Button ("(NULL)");
						}
						i++;
					}
					UnityEngine.GUI.color = Color.red;
					GUILayout.Label ("\n---- Locked: " + LockedWorldItems.Count.ToString () + "----\n");
					foreach (WorldItem w in LockedWorldItems) {
						//for (int i = 0; i < LockedSet.Count; i++) {
						//WorldItem w = LockedSet[i];
						if (w != null) {
							if (LockedComparer.IsDirty (w)) {
								UnityEngine.GUI.color = Color.magenta;
							} else {
								UnityEngine.GUI.color = Color.Lerp (Color.blue, Color.green, 0.75f);
							}
							GUILayout.Button (w.name + " - " + w.DistanceToPlayer.ToString ("0.##"));
						} else {
							GUILayout.Button ("(NULL)");
						}
					}

					UnityEditor.EditorUtility.SetDirty (this);
					UnityEditor.EditorUtility.SetDirty (gameObject);
				}
				GUILayout.Label ("\n----");
			}

			if (GUILayout.Button("\n\nSave Categories to CSV\n\n", UnityEditor.EditorStyles.miniButton)) {
				ExportCategoriesDatabase();
			}
			if (GUILayout.Button("\n\nLoad Templates\n\n", UnityEditor.EditorStyles.miniButton)) {
				if (!Manager.IsAwake <Mods>()) {
					Manager.WakeUp <Mods>("__MODS");
					Mods.Get.Editor.InitializeEditor();
				}

				foreach (WorldItemPack pack in WorldItemPacks) {
					WITemplate template = null;
					foreach (GameObject prefab in pack.Prefabs) {
						WorldItem wi = prefab.GetComponent <WorldItem>();
						if (Mods.Get.Editor.LoadMod <WITemplate>(ref template, System.IO.Path.Combine("WorldItem", wi.Props.Name.PackName), wi.Props.Name.PrefabName)) {
							wi.Props = template.Props;
						}
					}
				}
			}
			if (GUILayout.Button("\n\nSave Templates\n\n", UnityEditor.EditorStyles.miniButton)) {
				if (!Manager.IsAwake <Mods>()) {
					Manager.WakeUp <Mods>("__MODS");
					Mods.Get.Editor.InitializeEditor();
				}
				foreach (WorldItemPackPaths packPaths in PackPaths) {
					Mods.Get.Editor.SaveMod <WorldItemPackPaths>(packPaths, "WorldItem", packPaths.Name);
				}

				foreach (WorldItemPack pack in WorldItemPacks) {
					foreach (GameObject prefab in pack.Prefabs) {
						WorldItem wi = prefab.GetComponent <WorldItem>();
						WITemplate template = new WITemplate(wi);
						//Debug.Log ("Saving template " + wi.name);
						Mods.Get.Editor.SaveMod <WITemplate>(
							template,
							System.IO.Path.Combine("WorldItem", wi.Props.Name.PackName),
							template.Name);
					}
				}
				if (Structures.Get == null) {
					Debug.Log("STRUCTURES GET WAS NULL");
				}
				if (Structures.Get != null) {
					foreach (WorldItemPack pack in Structures.Get.DynamicWorldItemPacks) {
						foreach (GameObject prefab in pack.Prefabs) {
							WorldItem wi = prefab.GetComponent <WorldItem>();
							WITemplate template = new WITemplate(wi);
							Debug.Log("Saving template " + wi.name);
							Mods.Get.Editor.SaveMod <WITemplate>(
								template,
								System.IO.Path.Combine("WorldItem", wi.Props.Name.PackName),
								template.Name);
						}
					}
				}
			}
			if (GUILayout.Button("\n\nImport Databaser\n\n", UnityEditor.EditorStyles.miniButton)) {
				DebugConsole.ConsoleCommand("import Worlditem all");
			}
			if (GUILayout.Button("\n\nExport Databaser\n\n", UnityEditor.EditorStyles.miniButton)) {
				ExportDatabase();
			}
			if (GUILayout.Button("\n\nRefresh world items manager\n\n", UnityEditor.EditorStyles.miniButton)) {
				if (!Manager.IsAwake <Structures>()) {
					Manager.WakeUp <Structures>("Frontiers_Structures");
					Structures.Get.RebuildPacks();
				}
				RefreshWorldItemsManager();
				Initialize();
			}
			if (GUILayout.Button("\n\nGenerate asset thumbnails\n\n", UnityEditor.EditorStyles.miniButton)) {
				GenerateAssetThumbnails();
			}
			UnityEngine.GUI.color = Color.cyan;

			if (GUILayout.Button("Find interior worlditems parent (var 0)", UnityEditor.EditorStyles.miniButton)) {
				GameObject intParent = GameObject.Find("__INTERIOR");
				GameObject var0 = intParent.transform.FindChild("=VARIANT_0=").gameObject;
				GameObject worlditemsParent = var0.transform.FindChild("__WORLDITEMS").gameObject;
				EditorStructureParent	= worlditemsParent;
			}
			if (GUILayout.Button("Find exterior worlditems parent")) {
				GameObject intParent = GameObject.Find("__EXTERIOR");
				GameObject worlditemsParent = intParent.transform.Find("__WORLDITEMS").gameObject;
				EditorStructureParent	= worlditemsParent;
			}

			#region structure editor stuff
			if (EditorStructureParent != null) {
				if (selectPack) {
					UnityEngine.GUI.color = Color.yellow;
					GUILayout.Label("PACKS:");
					foreach (WorldItemPack pack in WorldItemPacks) {
						if (GUILayout.Button("\n" + pack.Name + "\n")) {
							EditorSepectedPack = pack.Name;
							selectPack = false;
						}
					}
				} else {
					UnityEngine.GUI.color = Color.yellow;
					GUILayout.Label("SELECTED PACK: " + EditorSepectedPack);
					if (GUILayout.Button("\n(Select different pack)\n")) {
						selectPack = true;
					}
					UnityEngine.GUI.color = Color.cyan;
					foreach (WorldItemPack pack in WorldItemPacks) {
						if (pack.Name == EditorSepectedPack) {
							foreach (GameObject prefab in pack.Prefabs) {
								if (prefab.name == EditorSelectedItem) {
									UnityEngine.GUI.color = Color.green;
									if (GUILayout.Button(prefab.name + " (SELECTED)")) {

									}
								} else {
									UnityEngine.GUI.color = Color.cyan;
									if (GUILayout.Button(prefab.name)) {
										EditorSelectedItem = prefab.name;
									}
								}
							}
						}
					}
				}
			}
			#endregion


			UnityEngine.GUI.color = Color.Lerp(Color.white, Color.gray, 0.25f);
			GUILayout.Label("\n CATEGORIES: \n");
			GUILayout.BeginHorizontal();
			bool canCreate = true;
			foreach (WICategory catNameCheck in Categories) {
				if (catNameCheck.Name == categoryEntryName) {
					canCreate = false;
				}
			}
			if (canCreate) {
				UnityEngine.GUI.color = Color.Lerp(Color.white, Color.gray, 0.25f);
				categoryEntryName = GUILayout.TextField(categoryEntryName);
				if (GUILayout.Button("\n Create New Category \n")) {
					WICategory newCategory = new WICategory();
					newCategory.Name = categoryEntryName;
					Categories.Add(newCategory);
					categoryEntryName = string.Empty;
				}			
			} else {
				UnityEngine.GUI.color = Color.red;
				categoryEntryName = GUILayout.TextField(categoryEntryName);
				if (GUILayout.Button("\n Can't create, already exists! \n")) {
					WICategory category = new WICategory();
					category.Name = categoryEntryName;
				}
			}
			GUILayout.EndHorizontal();

			UnityEngine.GUI.color = Color.Lerp(Color.white, Color.gray, 0.25f);
			GUILayout.Label("Existing categories:");
			foreach (WICategory cateogry in Categories) {
				GUILayout.BeginHorizontal();
				if (selectedCategory != null && cateogry.Name == selectedCategory.Name) {
					UnityEngine.GUI.color = Color.green;
					if (GUILayout.Button(cateogry.Name + " (SELECTED)")) {

					}
				} else {
					UnityEngine.GUI.color = Color.Lerp(Color.white, Color.gray, 0.25f);
					if (GUILayout.Button(cateogry.Name)) {
						selectedCategory = cateogry;
					}
					UnityEngine.GUI.color = Color.red;
					if (GUILayout.Button("Delete")) {
						deletedCategory = cateogry;
					}
				}
				GUILayout.EndHorizontal();
			}

			if (selectedCategory != null) {
				List <GenericWorldItem> templatesToRemove = new List <GenericWorldItem>();
				UnityEngine.GUI.color = Color.Lerp(Color.white, Color.gray, 0.25f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("EDITING: " + selectedCategory.Name);
				selectedCategory.StartupItemsCategory = GUILayout.Toggle(selectedCategory.StartupItemsCategory, "Startup items");
				selectedCategory.StartupClothingCategory = GUILayout.Toggle(selectedCategory.StartupClothingCategory, "Startup clothing");
				//selectedCategory.ForceGeneric = GUILayout.Toggle(selectedCategory.ForceGeneric, "Force Generic");
				categoryRename = GUILayout.TextField(categoryRename);
				bool canRename = true;
				if (categoryRename == string.Empty) {
					canRename = false;
				} else {
					foreach (WICategory catNameCheck in Categories) {
						if (catNameCheck.Name == categoryRename) {
							canRename = false;
						}
					}
				}
				if (canRename) {
					if (GUILayout.Button("Rename Category")) {
						selectedCategory.Name = categoryRename;
					}			
				}
				GUILayout.EndHorizontal();
				Color yellowColor = Color.Lerp(Color.yellow, Color.gray, 0.5f);
				foreach (GenericWorldItem template in selectedCategory.GenericWorldItems) {
					UnityEngine.GUI.color = yellowColor;
					GUILayout.BeginHorizontal();
					GUILayout.Button(template.PackName);
					UnityEngine.GUI.color = Color.yellow;
					if (GUILayout.Button(template.PrefabName)) {
//					foreach (KeyValuePair <string, string []> arg in template.StartupArguments.Args)
//					{
//						//Debug.Log (template.PrefabName + " - arg " + arg.Key);
//					}
					}

					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();

					GUILayout.Label("State:");
					template.State = GUILayout.TextField(template.State);
					GUILayout.Label("Subcat:");
					template.Subcategory = GUILayout.TextField(template.Subcategory);

					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();

					GUILayout.Label("Inst: " + template.InstanceWeight);
					if (GUILayout.Button("+", GUILayout.Width(20f))) {
						template.InstanceWeight++;
					}
					if (GUILayout.Button("-", GUILayout.Width(20f))) {
						if (template.InstanceWeight > 0) {
							template.InstanceWeight--;
						}
					}
					GUILayout.Label("Min Inst: " + template.MinInstances);
					if (GUILayout.Button("+", GUILayout.Width(20f))) {
						template.MinInstances++;
					}
					if (GUILayout.Button("-", GUILayout.Width(20f))) {
						if (template.MinInstances > 0) {
							template.MinInstances--;
						}
					}
					UnityEngine.GUI.color = Color.red;
					if (GUILayout.Button(" X ", GUILayout.Width(20f))) {
						templatesToRemove.Add(template);
					}
					UnityEngine.GUI.color = yellowColor;
					GUILayout.EndHorizontal();
				}
				if (templatesToRemove.Count > 0) {
					foreach (GenericWorldItem templateToRemove in templatesToRemove) {
						selectedCategory.GenericWorldItems.Remove(templateToRemove);
					}
				}
				UnityEngine.GUI.color = Color.cyan;
				if (!addWorldItemToCat) {
					if (GUILayout.Button("Add items to category")) {
						addWorldItemToCat = true;
					}
				} else {
					List <GenericWorldItem> templatesToAdd = new List <GenericWorldItem>();
					foreach (WorldItemPack wiPack in WorldItemPacks) {
						if (wiPackSelection != string.Empty && wiPackSelection == wiPack.Name) {
							UnityEngine.GUI.color = Color.cyan;
							GUILayout.Label("Pack: " + wiPack.Name);
							foreach (GameObject packPrefab in wiPack.Prefabs) {
								bool isIncluded = false;
								foreach (GenericWorldItem template in selectedCategory.GenericWorldItems) {
									if (template.PrefabName == packPrefab.name) {
										isIncluded = true;
									}
								}
								if (isIncluded) {
									UnityEngine.GUI.color = Color.green;
									if (GUILayout.Button(packPrefab.name + " (Included)")) {
										WorldItem worlditem = packPrefab.GetComponent <WorldItem>();
										templatesToAdd.Add(new GenericWorldItem(worlditem));
									}
								} else {
									UnityEngine.GUI.color = Color.cyan;
									if (GUILayout.Button(packPrefab.name)) {
										WorldItem worlditem = packPrefab.GetComponent <WorldItem>();
										templatesToAdd.Add(new GenericWorldItem(worlditem));
									}
								}
							}
						} else {
							UnityEngine.GUI.color = Color.gray;
							if (GUILayout.Button("SELECT " + wiPack.Name)) {
								wiPackSelection = wiPack.Name;
							}
						}
					}
					foreach (GenericWorldItem templateToAdd in templatesToAdd) {
						selectedCategory.GenericWorldItems.Add(templateToAdd);
					}
				}
			}

			if (deletedCategory != null) {
				Categories.Remove(deletedCategory);
			}

			UnityEngine.GUI.color = Color.yellow;
			if (GUILayout.Button("\n Save categories to disk \n")) {
				SaveCategoriesEditor();
			}
			if (GUILayout.Button("\n Load categories from disk \n")) {
				LoadCategoriesEditor();
			}

			if (UnityEngine.GUI.changed) {
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}

		public void ExportPrefabThumbnails()
		{

		}

		public void RefreshWorldItemsManager()
		{
			if (!Manager.IsAwake <WorldItems>()) {
				Manager.WakeUp <WorldItems>("Frontiers_WorldItems");
			}
			string worlditemPacksPath = WorldItems.Get.LocalWIPacksPath;
			worlditemPacksPath = System.IO.Path.Combine(Application.dataPath, worlditemPacksPath);
			if (System.IO.Directory.Exists(worlditemPacksPath)) {
				////Debug.Log ("Found path " + worlditemPacksPath);
			} else {
				////Debug.Log ("Path " + worlditemPacksPath + " does not exist");
				return;
			}

			string[] pathNames = System.IO.Directory.GetDirectories(worlditemPacksPath);

			WorldItems.Get.PackPaths.Clear();

			foreach (string packPath in pathNames) {
				////Debug.Log ("Checking worlditem pack " + packPath);
				string[] subDirectoryNames = System.IO.Directory.GetDirectories(packPath);
				if (subDirectoryNames.Length == 0) {
					////Debug.Log ("No subfolders in worlditem pack - skipping");
				} else {
					WorldItemPackPaths packPaths = new WorldItemPackPaths();
					packPaths.PackPath = packPath.Replace(Application.dataPath, string.Empty);
					packPaths.Name = System.IO.Path.GetFileName(packPaths.PackPath);
					int numAssets = 0;
					foreach (string subDirectory in subDirectoryNames) {
						////Debug.Log ("Checking sub directory " + subDirectory);
						string currentFolderName = System.IO.Path.GetFileName(subDirectory);
						string[] assetPaths = System.IO.Directory.GetFiles(subDirectory);
						List <string> ListToAddTo	= null;

						switch (currentFolderName) {
							case "Prefabs":
								ListToAddTo = packPaths.Prefabs;
								foreach (string assetPath in assetPaths) {
									string extension = System.IO.Path.GetExtension(assetPath);
									////Debug.Log ("Extension: " + extension);
									if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".prefab", string.Empty);
										string prefabAssetPath = assetPath.Replace(Application.dataPath, string.Empty);
										if (!ListToAddTo.Contains(prefabAssetPath)) {
											ListToAddTo.Add(prefabAssetPath);
										}
										numAssets++;
									}
								}
								break;

							case "Meshes":
								ListToAddTo = packPaths.Meshes;
								foreach (string assetPath in assetPaths) {
									string extension = System.IO.Path.GetExtension(assetPath);
									if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".FBX", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".fbx", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".lxo", string.Empty);
										string prefabAssetPath = assetPath.Replace(Application.dataPath, string.Empty);
										if (!ListToAddTo.Contains(prefabAssetPath)) {
											ListToAddTo.Add(prefabAssetPath);
										}
										numAssets++;
									}
								}
								break;

							case "Textures":
								ListToAddTo = packPaths.Textures;
								foreach (string assetPath in assetPaths) {
									string extension = System.IO.Path.GetExtension(assetPath);
									if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".PSD", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".psd", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".jpg", string.Empty);
										string prefabAssetPath = assetPath.Replace(Application.dataPath, string.Empty);
										if (!ListToAddTo.Contains(prefabAssetPath)) {
											ListToAddTo.Add(prefabAssetPath);
										}
										numAssets++;
									}
								}
								break;

							case "Materials":
								ListToAddTo = packPaths.Materials;
								foreach (string assetPath in assetPaths) {
									string extension = System.IO.Path.GetExtension(assetPath);
									if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".material", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".mat", string.Empty);
										string prefabAssetPath = assetPath.Replace(Application.dataPath, string.Empty);
										if (!ListToAddTo.Contains(prefabAssetPath)) {
											ListToAddTo.Add(prefabAssetPath);
										}
										numAssets++;
									}
								}
								break;
							default:
							////Debug.Log ("Folder name " + currentFolderName + " not recognized");
								break;
						}
					}
					////Debug.Log ("Num assets in pack: " + numAssets);
					WorldItems.Get.PackPaths.Add(packPaths);
				}
			}

			WorldItems.Get.WorldItemPacks.Clear();
			foreach (WorldItemPackPaths packPaths in WorldItems.Get.PackPaths) {
				//Debug.Log ("Loading pack " + packPaths.PackPath);
				WorldItemPack newPack = new WorldItemPack();
				newPack.Name = System.IO.Path.GetFileName(packPaths.PackPath);

				foreach (string prefabPath in packPaths.Prefabs) {
					string finalPath = ("Assets" + prefabPath);
					//Debug.Log ("Adding dynpre " + finalPath);
					UnityEngine.Object prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(finalPath, typeof(UnityEngine.Object)) as UnityEngine.Object;
					if (prefab != null) {
						GameObject prefabGameObject = prefab as GameObject;
						WorldItem worlditem = prefabGameObject.GetComponent <WorldItem>();
						if (worlditem == null) {
							//AddRequiredComponentsToObject (prefabGameObject);
							worlditem = prefabGameObject.GetComponent <WorldItem>();
						}
						if (worlditem == null) {
							Debug.LogError ("Worlditem was null " + finalPath);
							continue;
						}
						worlditem.Props.Name.PackName = newPack.Name;
						worlditem.Props.Name.PrefabName = prefabGameObject.name;

						//disable colliders and root out dead renderers and colliders
						for (int i = worlditem.Renderers.Count - 1; i >= 0; i--) {
							if (worlditem.Renderers[i] == null) {
								worlditem.Renderers.RemoveAt(i);
							}
						}
						for (int j = worlditem.Colliders.Count - 1; j >= 0; j--) {
							if (worlditem.Colliders[j] == null) {
								worlditem.Colliders.RemoveAt(j);
							} else {
								////Debug.Log ("Setting worlditem collider " + j + " to not enabled");
								worlditem.Colliders[j].enabled = false;
							}
						}

						newPack.Prefabs.Add(prefabGameObject);
					} else {
						////Debug.Log ("PREFAB WAS NULL " + Path.GetFileName (prefabPath));
					}
				}
				////Debug.Log ("Added pack FINISHED");
				WorldItems.Get.WorldItemPacks.Add(newPack);
			}
			UnityEditor.EditorUtility.SetDirty(WorldItems.Get);
		}

		public static void CalculateSizes()
		{
			WorldItem[] worlditems = FindObjectsOfType <WorldItem>();

			foreach (WorldItem worlditem in worlditems) {
				CalculateSize(worlditem);

				////////////Debug.Log ("Calculating size for " + worlditem.name + ", result is " + worlditem.Props.Global.Flags.Size.ToString( ));
			}
		}

		public static void CalculateSize(WorldItem worlditem)
		{
			worlditem.InitializeTemplate();

			worlditem.transform.localPosition = Vector3.zero;
			Bounds worlditemBounds = new Bounds(Vector3.zero, Vector3.one * 0.01f);
			foreach (Collider worlditemCollider in worlditem.Colliders) {
				worlditemCollider.enabled = true;
				worlditemBounds.Encapsulate(worlditemCollider.bounds);
				worlditemCollider.enabled = false;
			}

			//worlditemBounds = new Bounds (worlditemBounds.center, worlditemBounds.size);//recalculate the center

			Bounds tinyBounds = new Bounds(worlditemBounds.center, Get.TinySizeObject.bounds.size);
			Bounds smallBounds = new Bounds(worlditemBounds.center, Get.SmallSizeObject.bounds.size);
			Bounds mediumBounds = new Bounds(worlditemBounds.center, Get.MediumSizeObject.bounds.size);
			Bounds largeBounds = new Bounds(worlditemBounds.center, Get.LargeSizeObject.bounds.size);
			Bounds hugeBounds = new Bounds(worlditemBounds.center, Get.HugeSizeObject.bounds.size);

			if (tinyBounds.Contains(worlditemBounds.min) && tinyBounds.Contains(worlditemBounds.max)) {
				worlditem.Props.Global.Flags.Size = WISize.Tiny;
			} else if (smallBounds.Contains(worlditemBounds.min) && smallBounds.Contains(worlditemBounds.max)) {
				worlditem.Props.Global.Flags.Size = WISize.Small;
			} else if (mediumBounds.Contains(worlditemBounds.min) && mediumBounds.Contains(worlditemBounds.max)) {
				worlditem.Props.Global.Flags.Size = WISize.Medium;
			} else if (largeBounds.Contains(worlditemBounds.min) && largeBounds.Contains(worlditemBounds.max)) {
				worlditem.Props.Global.Flags.Size = WISize.Large;
			} else if (hugeBounds.Contains(worlditemBounds.min) && hugeBounds.Contains(worlditemBounds.max)) {
				worlditem.Props.Global.Flags.Size = WISize.Huge;
			} else {
				worlditem.Props.Global.Flags.Size = WISize.Huge;
				worlditem.Props.Global.Weight = ItemWeight.Unliftable;
			}

			UnityEditor.EditorUtility.SetDirty(worlditem);
		}
		#endif
	}
}