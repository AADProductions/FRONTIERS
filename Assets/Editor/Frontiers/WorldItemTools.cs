using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using UnityEditor;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;
using ExtensionMethods;
using Frontiers;


public class WorldItemTools : MonoBehaviour
{
	[MenuItem ("Frontiers/Play Animation")]
	static void PlayAnimation ( )
	{
		UnityEditor.Selection.activeGameObject.GetComponent<Animation>().Play ();
	}

	[MenuItem ("Frontiers/Initialize Template")]
	static void InitializeTemplate ( )
	{
		foreach (GameObject go in Selection.gameObjects) {
			WorldItem wi = null;
			if (go.HasComponent <WorldItem> (out wi)) {
				Debug.Log ("Initializing template in " + wi.name);
				wi.InitializeTemplate ();
			}
		}
	}

	[MenuItem ("Frontiers/Calculate World Item Sizes")]
	static void CalculateWorldItemSizes ( )
	{
		if (!Manager.IsAwake <WorldItems> ( ))
		{
			Manager.WakeUp <WorldItems> ("Frontiers_WorldItems");
		}
		WorldItems.CalculateSizes ();	
	}

	[MenuItem ("Frontiers/Arrange Items in Grid")]
	static void ArrangeItemsInGrid ( )
	{
				foreach (GameObject selected in UnityEditor.Selection.gameObjects) {
						foreach (Transform child in selected.transform) {
								Vector3 localPosition = child.localPosition;
								localPosition.y -= 60f;
								child.localPosition = localPosition;
						}
				}

				/*Vector3 position = Vector3.zero;
		float X = 0;
		float Z = 0;
		int numZ = Selection.gameObjects.Length / 5;
		int numItems = 0;
		foreach (GameObject selectedObject in Selection.gameObjects)
		{
			selectedObject.transform.position = new Vector3 (X, 0f, Z);
			numItems++;
			if (numItems > numZ) {
								Z += 15.5f;
				X = 0f;
				numItems = 0;
			}
						X += 15.5f;
		}	*/
	}

	[MenuItem ("Frontiers/Randomize Y Rotation")]
	static void RandomizeYRotation ( )
	{
		foreach (GameObject selectedObject in Selection.gameObjects)
		{
			Vector3 rotation = selectedObject.transform.rotation.eulerAngles;
			rotation.y = UnityEngine.Random.value * 360f;
			selectedObject.transform.rotation = Quaternion.Euler (rotation);
		}	
	}

	[MenuItem ("Frontiers/Refresh Editor Scene")]
	static void RefreshEditorScene ( )
	{
		foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
		{
		   obj.SendMessage ("Awake");
		}	
	}

	[MenuItem ("Frontiers/WorldItems/Edibles/Create States")]
	static void CreateFoodstuffStates ( )
	{
		foreach (GameObject selectedObject in Selection.gameObjects)
		{
			WIStates states = selectedObject.GetOrAdd <WIStates> ();
			states.EditorCreateStates ();

			FoodStuff foodstuff = selectedObject.GetOrAdd <FoodStuff> ();
			foodstuff.EditorCreateProps ();
		}
	}

	[MenuItem ("Frontiers/WorldItems/Set Location Names")]
	static void SetLocationNames ( )
	{
		foreach (GameObject selectedObject in Selection.gameObjects)
		{
			Location location = null;
			if (selectedObject.HasComponent <Location> (out location)) {
				WorldItem worlditem = selectedObject.GetComponent <WorldItem> ();
				worlditem.Props.Name.DisplayName = location.State.Name.CommonName;
				worlditem.Props.Name.StackName = selectedObject.name;
				worlditem.Props.Name.FileName = selectedObject.name;
				//worlditem.Props.Name.FileNameIncremented = selectedObject.name;

				location.State.Name.FileName = selectedObject.name;
				if (string.IsNullOrEmpty (location.State.Name.CommonName)) {
					location.State.Name.CommonName = selectedObject.name;
				}

				EditorUtility.SetDirty (worlditem);
				EditorUtility.SetDirty (location);
				EditorUtility.SetDirty (location.gameObject);
			}

		}
	}
	
	[MenuItem ("Frontiers/WorldItems/Clear Name Increment")]
	static void ClearNameIncrement ( )
	{
		foreach (GameObject selectedObject in Selection.gameObjects)
		{
			string originalName 	= selectedObject.name;
			string [] splitName		= originalName.Split (new string [] {"_","."}, System.StringSplitOptions.RemoveEmptyEntries);
			if (splitName.Length > 1)
			{
				string cleanName 	= splitName [0];
				selectedObject.name = cleanName;
			}
			
		}
	}

	[MenuItem ("Frontiers/WorldItems/Set Spawn Points in Dropped Item")]
	static void SetSpawnPointsInDroppedItem ( )
	{
		DropItemsOnDie drop = Selection.activeGameObject.GetComponent <DropItemsOnDie> ( );
		drop.SpawnPoints.Clear ( );
		foreach (Transform child in Selection.activeGameObject.transform)
		{
			drop.SpawnPoints.Add (child.transform.localPosition);
		}
	}

	[MenuItem ("Frontiers/WorldItems/Set Properties")]
	static void SetWorldItemProperties ( )
	{
		GameObject worlditems = GameObject.Find ("Frontiers_WorldItems");
		WorldItems worlditemsObject = worlditems.GetComponent <WorldItems> ( );
		WorldItems.Get = worlditemsObject;
		foreach (WorldItemPack pack in WorldItems.Get.WorldItemPacks)
		{
			foreach (GameObject prefab in pack.Prefabs)
			{
				WorldItem worlditem = prefab.GetComponent <WorldItem> ( );
				if (worlditem != null)
				{
					worlditem.Props.Name.PrefabName = prefab.name;
					worlditem.Props.Name.PackName = pack.Name;
				}
			}
		}
	}

	[MenuItem ("Frontiers/WorldItems/Set Proper Names")]
	static void SetProperNames ( )
	{
		foreach (GameObject worlditemGameObject in Selection.gameObjects)
		{
//			string newname = worlditemGameObject.name;
//			newname = newname.Replace ("Ring 4", "Duke Ring");
//			worlditemGameObject.name = newname;
			string gameObjectName 	= worlditemGameObject.name;
			gameObjectName = worlditemGameObject.name.Replace ("_", " ");
			gameObjectName = System.Text.RegularExpressions.Regex.Replace (gameObjectName, @"\b(\w|['-])+\b",  m => m.Value[0].ToString().ToUpper() + m.Value.Substring(1));
			gameObjectName = System.Text.RegularExpressions.Regex.Replace (gameObjectName, "(?<=[0-9])(?=[A-Za-z])|(?<=[A-Za-z])(?=[0-9])", " ");
			worlditemGameObject.name = gameObjectName;

			//Debug.Log (gameObjectName);
		}
	}

	[MenuItem ("Frontiers/WorldItems/Add Required Components/Basic")]
	static void AddRequiredComponents ( )
	{
		foreach (GameObject worlditemGameObject in Selection.gameObjects)
		{
			AddRequiredComponentsToObject (worlditemGameObject);
		}
	}

	[MenuItem ("Frontiers/WorldItems/Add Required Components/Inventory Item")]
	static void AddInventoryItemComponents ( )
	{
		foreach (GameObject worlditemGameObject in Selection.gameObjects)
		{
			AddRequiredComponentsToObject (worlditemGameObject);
			worlditemGameObject.GetOrAdd <Stackable> ( );
			worlditemGameObject.GetOrAdd <Equippable> ( );
		}
	}
	
	[MenuItem ("Frontiers/WorldItems/Add Required Components/Edibles")]
	static void AddInventoryItemComponentsEdible ( )
	{
		foreach (GameObject worlditemGameObject in Selection.gameObjects)
		{
			AddRequiredComponentsToObject (worlditemGameObject);
			worlditemGameObject.GetOrAdd <FoodStuff> ( );
		}
	}

	[MenuItem ("Frontiers/WorldItems/Add Required Components/Container")]
	static void AddContainerComponents ( )
	{
		foreach (GameObject worlditemGameObject in Selection.gameObjects)
		{
			AddRequiredComponentsToObject (worlditemGameObject);
			worlditemGameObject.GetOrAdd <Container> ( );
		}
	}

	static void AddRequiredComponentsToObject (GameObject worlditemGameObject)
	{
		WorldItem worlditem = worlditemGameObject.GetOrAdd <WorldItem> ( );
		if (worlditem.Props == null)
		{
			worlditem.Props = new WIProps ( );
		}

		if (worlditemGameObject.GetComponent<Rigidbody>() == null)
		{
			worlditemGameObject.AddComponent <Rigidbody> ( );
		}
		WorldItems.GenerateCollider (worlditem);
//		worlditemGameObject.GetOrAdd <Stackable> ( );
//		worlditemGameObject.GetOrAdd <Equippable> ( );
	}

}