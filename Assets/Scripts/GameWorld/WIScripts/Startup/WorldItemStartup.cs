using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class WorldItemStartup : MonoBehaviour
	{
		public WorldItem 	Item;
		
		public virtual void Awake ( )
		{
			Item = gameObject.GetComponent <WorldItem> ( );
		}
		
		public void Finish ( )
		{
			GameObject.Destroy (this);
		}	
	}
}