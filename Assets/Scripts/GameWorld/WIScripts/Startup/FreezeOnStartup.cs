using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class FreezeOnStartup : WorldItemStartup
	{
		public override void Awake ( )
		{
			base.Awake ( );
			rigidbody.isKinematic = true;
		}
		
		public void Start ( )
		{
			rigidbody.isKinematic = true;
		}
		
		public void FixedUpdate ( )
		{
			rigidbody.isKinematic = true;
			Finish ( );
		}
	}
}