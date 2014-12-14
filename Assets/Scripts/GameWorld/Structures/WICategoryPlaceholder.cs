using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World
{
		public class WICategoryPlaceholder : MonoBehaviour
		{		//used when editing structures
				//lets the template specify that 'something' is supposed to go here
				//without actually naming the thing
				//allows for variation based on region / owner flags
				//eg a lousy-looking chair will spawn in a bad part of town
				//and a throne will spawn in a good part of town
				public GameObject TempDoppleganger;
				public WICatItem Item = new WICatItem();
				public bool SaveToStructure = false;

				public void OnDrawGizmos()
				{
						bool active = transform.parent.name == "__WORLDITEMS_CATS" || SaveToStructure;
						if (!active) {
								Gizmos.color = Colors.Alpha(Color.white, 0.15f);
								Gizmos.DrawWireSphere(transform.position, 0.1f);
						} else {
								Gizmos.color = Colors.Alpha(Color.red, 0.25f);
								Gizmos.DrawSphere(transform.position, 0.1f);
						}
						gameObject.name = Item.WICategoryName;

						float alpha = 1f;
						if (!active) {
								alpha = 0.25f;
						}
						Gizmos.color = Colors.Alpha(Color.green, alpha);
						Gizmos.DrawLine(transform.position, transform.position + transform.up);
						Gizmos.DrawWireCube(transform.position + transform.up, Vector3.one * 0.1f);
						Gizmos.color = Colors.Alpha(Color.cyan, alpha);
						Gizmos.DrawLine(transform.position, transform.position + transform.forward);
				}

				protected WICategory wiCategory = null;
		}
}