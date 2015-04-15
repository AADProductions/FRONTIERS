using UnityEngine;
using System.Collections;

namespace Frontiers.World.WIScripts
{
		[ExecuteInEditMode]
		public class Compass : WIScript
		{
				public Transform NeedleParent;
				public Transform Needle;
				public float Damping = 1f;

				public override bool EnableAutomatically {
						get {
								return true;
						}
				}

				public void Update()
				{
						//still ugly but it works really well
						Needle.parent = null;
						Needle.rotation = Quaternion.identity;//will always face north
						Needle.parent = NeedleParent;
						Needle.localScale = Vector3.one;
						Needle.localPosition = Vector3.zero;
						Vector3 eulerangles = Needle.localRotation.eulerAngles;
						eulerangles.x = 0f;
						eulerangles.z = 0f;
						Needle.localRotation = Quaternion.Euler(eulerangles);
				}
		}
}