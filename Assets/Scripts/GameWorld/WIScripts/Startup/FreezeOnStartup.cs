using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class FreezeOnStartup : WorldItemStartup
	{
		public override void Awake ( )
		{
			base.Awake ( );
			GetComponent<Rigidbody>().isKinematic = true;
		}
		
		public void Start ( )
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}
		
		public void FixedUpdate ( )
		{
			GetComponent<Rigidbody>().isKinematic = true;
			Finish ( );
		}
	}
}