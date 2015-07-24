using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	//used to store lists of generic world items
	//that can be used for spawning stuff
	[Serializable]
	public class WICategory : Mod, IComparable <WICategory>
	{
		public static WICategory Empty {
			get {
				if (mEmptyCategory == null) {
					mEmptyCategory = new WICategory ();
				}
				return mEmptyCategory;
			}
		}

		public bool ForceGeneric = false;
		public bool StartupItemsCategory = false;
		public bool StartupClothingCategory = false;
		public List <GenericWorldItem> GenericWorldItems = new List <GenericWorldItem> ();
		public GenericWorldItem DefaultItem = null;
		public bool HasMinInstanceItems = false;

		public SBounds SharedBounds {
			get {
				if (mSharedBounds.IsEmpty) {
					CalculateBounds ();
				}
				return mSharedBounds;
			}
		}

		public void Initialize ()
		{
			HasMinInstanceItems = false;

			if ((DefaultItem == null || DefaultItem.IsEmpty) && GenericWorldItems.Count > 0) {
				DefaultItem = GenericWorldItems [0];
			}

			//get the flags for our items
			WorldItem prefab = null;
			for (int i = GenericWorldItems.LastIndex (); i >= 0; i--) {
				GenericWorldItem genericWorldItem = GenericWorldItems [i];
				if (genericWorldItem == null) {
					GenericWorldItems.RemoveAt (i);
				} else if (WorldItems.Get.PackPrefab (genericWorldItem.PackName, genericWorldItem.PrefabName, out prefab)) {
					genericWorldItem.Flags = prefab.Flags;
				} else {
					GenericWorldItems.RemoveAt (i);
				}
			}

			//generate our lookup tables
			indexTable = new List <int> ();
			//randomTable = new List <uint> ();
			WorldItem template = null;

			for (int i = 0; i < GenericWorldItems.Count; i++) {
				GenericWorldItem gwi = GenericWorldItems [i];
				int itemProbability = Globals.WIRarityCommonProbability;

				if (gwi.MinInstances > 0) {
					HasMinInstanceItems = true;
				}

				if (WorldItems.Get.PackPrefab (gwi.PackName, gwi.PrefabName, out template)) {
					switch (template.Props.Global.Flags.BaseRarity) {
					case WIRarity.Common:
					default:
						break;

					case WIRarity.Uncommon:
						itemProbability = Globals.WIRarityUncommonProbability;
						break;

					case WIRarity.Rare:
						itemProbability = Globals.WIRarityRareProbability;
						break;

					case WIRarity.Exclusive:
						break;
					}

					itemProbability *= gwi.InstanceWeight;

					for (int j = 0; j < itemProbability; j++) {
						indexTable.Add (i);//add this item's index number
					}
				}
			}

			//now shuffle the index list
			indexTable.Shuffle (new System.Random (Name.GetHashCode ()));

			//now build the randomized lookup list
			/*
						for (uint i = 0; i < indexTable.Count; i++) {
							for (uint j = 0; j < Globals.WICategoryRandomTableProbability; j++) {
								//add an instance if the randomized index
								randomTable.Add (indexTable [(int) i]);
							}
						}
						*/

			//finally, randomize the random table list
			//randomTable.Shuffle (new System.Random (Name.GetHashCode ()));

			mInitialized = true;
		}

		public void RefreshDisplayNames ()
		{
			for (int i = 0; i < GenericWorldItems.Count; i++) {
				if (string.IsNullOrEmpty (GenericWorldItems [i].DisplayName)) {
					WorldItem prefab = null;
					if (WorldItems.Get.PackPrefab (GenericWorldItems [i].PackName, GenericWorldItems [i].PrefabName, out prefab)) {
						GenericWorldItems [i].DisplayName = prefab.DisplayName;
					}
				}
			}
		}

		public bool IsEmpty {
			get {
				return GenericWorldItems.Count == 0 || indexTable.Count == 0;
			}
		}

		public void CalculateBounds ()
		{
			if (!Manager.IsAwake <WorldItems> ()) {
				Manager.WakeUp <WorldItems> ("Frontiers_WorldItems");
			}

			Bounds sharedBounds = new Bounds ();
			WorldItem worlditem = null;
			foreach (GenericWorldItem item in GenericWorldItems) {
				if (WorldItems.Get.PackPrefab (item.PackName, item.PrefabName, out worlditem)) {
					Bounds worlditemBounds = new Bounds (Vector3.zero, Vector3.one);
					foreach (Renderer renderer in worlditem.Renderers) {
						//assume the template is at vector3 zero
						worlditemBounds.Encapsulate (renderer.bounds);
					}
					sharedBounds.Encapsulate (worlditemBounds);
				}
			}
			mSharedBounds = sharedBounds;
		}

		public void GetMinInstanceItems (List <GenericWorldItem> minInstanceItems)
		{
			for (int i = 0; i < GenericWorldItems.Count; i++) {
				if (GenericWorldItems [i].MinInstances > 0) {
					minInstanceItems.Add (GenericWorldItems [i]);
				}
			}
		}

		public bool GetItem (int hashCode, out GenericWorldItem genericItem)
		{
			genericItem = null;
			if (GenericWorldItems.Count > 0) {
				genericItem = GenericWorldItems [Mathf.Abs (hashCode) % GenericWorldItems.Count];
			}
			return genericItem != null;
		}

		public bool GetItem (WIFlags flags, int hashCode, ref int lastIndex, out GenericWorldItem genericItem)
		{
			genericItem = null;

			if (IsEmpty) {
				//Debug.Log ("Category was empty");
				return false;
			}
			//use the hashcode and index to get a random lookup table index
			int indexLookup = 0;
			int objectIndex = 0;
			int searchOffset = Mathf.Abs (hashCode + lastIndex);
			GenericWorldItem currentItem = null;

			for (int i = 0; i < indexTable.Count; i++) {
				//go for the entire length of the array
				//get an index - this will increment the last index
				indexLookup = (searchOffset + i) % indexTable.Count;
				objectIndex = indexTable [indexLookup];
				currentItem = GenericWorldItems [objectIndex];

				if (flags.Check (currentItem.Flags)) {
					if (Stacks.Can.Fit (currentItem.Flags.Size, flags.Size)) {
						genericItem = currentItem;
						lastIndex = indexLookup + 1;
						break;
					}
				}
			}

			return genericItem != null;
		}

		public List <GenericWorldItem> GetItems (WIFlags flags)
		{
			Initialize ();

			List <GenericWorldItem> items = new List<GenericWorldItem> ();
			for (int i = 0; i < GenericWorldItems.Count; i++) {
				if (GenericWorldItems [i].HasFlags) {
					if (GenericWorldItems [i].Flags.Check (flags) && Stacks.Can.Fit (GenericWorldItems [i].Flags.Size, flags.Size)) {
						items.Add (GenericWorldItems [i]);
					}
				}
			}
			return items;
		}

		public Dictionary <string, float> GetProbabilities ()
		{
			Dictionary <string, int> rawProbabilities = new Dictionary<string, int> ();
			for (int i = 0; i < GenericWorldItems.Count; i++) {
				int result = 0;
				for (int j = 0; j < indexTable.Count; j++) {
					if (indexTable [j] == i) {
						result++;
					}
				}
				rawProbabilities.Add (GenericWorldItems [i].PrefabName, result);
			}

			//calculate normalized probabilities
			Dictionary <string, float> normalizedProbabilities = new Dictionary<string, float> ();
			foreach (KeyValuePair <string,int> rawProbability in rawProbabilities) {
				float result = ((float)rawProbability.Value) / ((float)indexTable.Count);
				normalizedProbabilities.Add (rawProbability.Key, result);
			}
			return normalizedProbabilities;
		}

		public int NumItems {
			get {
				return GenericWorldItems.Count;
			}
		}

		public int CompareTo (WICategory other)
		{
			return Name.CompareTo (other.Name);
		}

		protected List <int> indexTable = null;
		protected static WICategory mEmptyCategory;
		protected SBounds mSharedBounds;
		protected bool mInitialized = false;
	}

	[Serializable]
	public class WICatItem
	{
		public WICatItem ()
		{
			Flags = new WIFlags ();
			Flags.Size = WISize.NoLimit;
		}

		[FrontiersCategoryName]
		public string WICategoryName = "Default";
		public SpawnerAvailability Availability = SpawnerAvailability.Once;
		public int SpawnCode = -1;
		public int SpawnIndex = -1;
		public int MaxSpawns = 0;
		public int NumTimesSpawned = 0;
		public float MinSpawnTime = 0;
		public float MaxSpawnTime = 0;
		public float DropoutProbability = 0.05f;
		public bool UseSettingsOnSpawnedContainers = false;
		public WIFlags Flags = null;
		public STransform Transform = new STransform ();
		public FillStackContainerState ContainerSettings = null;
	}
}