using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class StructureDestructionRadius : MonoBehaviour
	{	
		public void Awake ( )
		{
			gameObject.layer = Globals.LayerNumTrigger;
			gameObject.AddComponent <Rigidbody> ( );
			GetComponent<Rigidbody>().isKinematic = true;
		}
		
		public void Start ( )
		{
			SphereCollider sphereCollider 	= gameObject.AddComponent <SphereCollider> ( );
			sphereCollider.radius			= 1.0f;
			sphereCollider.isTrigger		= true;
			GameObject.Destroy (gameObject, 1.0f);
		}
		
		public void OnTriggerEnter (Collider other)
		{
//			//Debug.Log ("Setting structure destroyed on " + other.name);
			other.gameObject.SendMessage ("OnStructureDestroyed", SendMessageOptions.DontRequireReceiver);
		}
	}
}