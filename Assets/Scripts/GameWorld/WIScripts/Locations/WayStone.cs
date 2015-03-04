using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;

namespace Frontiers.World
{	
	public class WayStone : WIScript
	{			
		public GameObject FrontWayStone;
		public GameObject BackWayStone;
		public GameObject EyePosition;

		public override bool CanEnterInventory
		{
			get
			{
				return false;
			}
		}
		
		public override bool CanBeCarried
		{
			get
			{
				return false;
			}
		}
		
		public void OnReveal ( )
		{
			//reveal WayStone in front and behind
		}

		public void OnDrawGizmos ( )
		{
			if (FrontWayStone != null)
			{
				WayStone fws = FrontWayStone.GetComponent <WayStone> ( );
				RaycastHit hitInfo;
				if (Physics.Linecast (fws.EyePosition.transform.position, EyePosition.transform.position, out hitInfo, Globals.LayersActive))
				{
					if (hitInfo.collider.gameObject == gameObject)
					{
						Gizmos.color = Color.green;
					}
					else
					{
						Gizmos.color = Color.red;
					}
				}
				Gizmos.DrawLine (fws.EyePosition.transform.position, EyePosition.transform.position);
			}
//			if (BackWayStone != null)
//			{
//				WayStone bws = BackWayStone.GetComponent <WayStone> ( );
//				Gizmos.DrawLine (bws.EyePosition.transform.position, EyePosition.transform.position);
//			}
		}
	}
}