using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
	public class Inventory : WIScript
	{
		public WorldItemInventoryType Type {
			get {
				if (worlditem == null) {
					return WorldItemInventoryType.Container;
				}
				
				if (worlditem.Is<Creature> ()) {
					return WorldItemInventoryType.Creature;
				} else if (worlditem.Is<Character> ()) {
					return WorldItemInventoryType.Character;
				} else {
					return WorldItemInventoryType.Container;
				}
			}
		}

		public int GoldPieces	{ get; set; }

		public override void Awake ()
		{
			mStackContainerListner = new WIStackListner (OnStackContainerChange);			
			base.Awake ();
		}

		public void Start ()
		{
			if (worlditem.Is<Character> ()) {
//				WIStackContainer stackContainer 	= gameObject.AddComponent <WIStackContainer> ( );
//				stackContainer.Owner				= gameObject;
//				stackContainer.CharacterOwnerName 	= worlditem.Get<Character> ( ).SafeName;
//				stackContainer.SubscribeToChanges (mStackContainerListner);
//				mStackContainers.Add (stackContainer);
				// = WIStackContainer.CreateStackContainers (1, gameObject, mStackContainerListner, worlditem.Character.SafeName);
			} else {
//				WIStackContainer stackContainer 	= gameObject.AddComponent <WIStackContainer> ( );
//				stackContainer.Owner				= gameObject;
//				stackContainer.SubscribeToChanges (mStackContainerListner);
//				mStackContainers.Add (stackContainer);
			}
//			//Debug.Log ("Created " + mStackContainers.Count + " stack containers for world item inventory");
		}

		public List <WIStackContainer> ActiveStackContainers {
			get {
				List <WIStackContainer> activeStackContainers = new List <WIStackContainer> ();
				activeStackContainers.AddRange (mStackContainers);
				return activeStackContainers;
			}
		}

		public void OnStackContainerChange (WIStack itemStack)
		{
			//Debug.Log ("stack container changed in world item inventory for " + name);
		}

		public bool	AddItems (WIStack stack, int numToAdd, ref int numAdded)
		{
			return false;
		}

		public bool	RemoveItems (out WorldItem topItem)
		{
			topItem = null;
			return false;
		}

		public bool	RemoveItems (List <WorldItem> items, int numToRemove, ref int numRemoved)
		{
			return false;
		}

		public void DumpContentsIntoWorld (Vector3 position)
		{
			
		}

		public bool AddItems (WIStack stack)
		{
//			while (stack.NumItems > 0)
//			{
//				AddItems (stack.Pop ( ));
//			}
			return true;
		}

		public bool AddItems (IWIBase item, int numToAdd, ref int numAdded)
		{
			return false;
		}

		public bool AddItems (IWIBase inventoryItem)
		{
			bool result = false;
			WIStackError error = WIStackError.None;
			foreach (WIStackContainer container in ActiveStackContainers) {
				if (Stacks.Add.Items (inventoryItem, container, ref error)) {
//					//Debug.Log ("Added " + inventoryItem.name + " to stack container in world item inventory");
					result = true;
					break;
				}
			}
			return result;
		}

		public void AddGoldToInventory (int goldPieces)
		{
			GoldPieces += goldPieces;
		}

		public bool RemoveGoldFromInventory (int goldPieces)
		{
			if (GoldPieces >= goldPieces) {
				GoldPieces -= goldPieces;
				return true;
			}
			return false;
		}

		protected List <WIStackContainer> mStackContainers = new List <WIStackContainer> ();
		protected WIStackListner mStackContainerListner = null;
	}
}
