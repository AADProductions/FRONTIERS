using UnityEngine;
using System.Collections;
using Frontiers;
using ExtensionMethods;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
	public class LookerBubble : AwarenessBubble <Looker>
	{
		public List <IVisible> VisibleItems = new List <IVisible> ();

		public override void Awake ()
		{
			base.Awake ();
			gameObject.layer = Globals.LayerNumAwarenessReceiver;
		}

		protected override void OnStartUsing ()
		{
			Collider.height = Looker.AwarenessDistanceTypeToVisibleDistance (ParentObject.State.AwarenessDistance);
			Collider.radius = Collider.height;//TEMP TODO link this to fieldOfView
			Collider.center = Vector3.zero;// new Vector3 (0f, 0f, Collider.height * 0.49f);//49 to give it a smidge of 'behind its back' awareness
			Collider.direction = 2;
		}

		protected override void OnFinishUsing ()
		{
			VisibleItems.Clear ();
		}

		IVisible mVisibleItemCheck;
		IItemOfInterest mIoiCheck;
		protected override void HandleEncounter (UnityEngine.Collider other)
		{
			//this will cover most cases including the player
			mVisibleItemCheck = (IVisible)other.GetComponent (typeof(IVisible));
			if (mVisibleItemCheck == null) {
				//whoops, we have to do some heavy lifting
				//see if it's a world item
				mIoiCheck = null;
				if (WorldItems.GetIOIFromCollider (other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem) {
					mVisibleItemCheck = mIoiCheck.worlditem;
				}
			}

			//make sure our parent object gives a damn
			if (mVisibleItemCheck != null &&
				Flags.Check ((uint)mVisibleItemCheck.IOIType, (uint)ParentObject.State.VisibleTypesOfInterest, Flags.CheckType.MatchAny) &&
				mVisibleItemCheck.HasAtLeastOne (ParentObject.State.ItemsOfInterest)) {
				VisibleItems.SafeAdd (mVisibleItemCheck);
			}
		}

		protected override void OnUpdateAwareness ()
		{
			if (VisibleItems.Count > 0) {
				ParentObject.SeeItemsOfInterest (VisibleItems);
				VisibleItems.Clear ();
			}
		}
	}
}