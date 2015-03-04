using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class WorldItemLightSource : MonoBehaviour
	{
		public WorldItem	Item;
		public Vector3 		LightSourceOffset;
		public float		LightSourceIntensity	= 1.5f;
		public float		LightSourceRange		= 15.0f;
		public Color		LightSourceColor		= Color.white;
		public bool			CastsShadows			= true;
		public bool			Flickers				= false;
		public float		FlickerAmount			= 0.25f;
		public Light		LightSource;
		
		public void 		Start ( )		
		{
			Item									= gameObject.GetComponent <WorldItem> ( );
			
			GameObject newLightGameObject			= new GameObject ("LightSource");
			LightSource								= newLightGameObject.AddComponent <Light> ( );		
			LightSource.intensity 					= LightSourceIntensity;
			LightSource.color 						= LightSourceColor;
			LightSource.range 						= LightSourceRange;
			LightSource.transform.parent 			= gameObject.transform;
			LightSource.transform.localPosition 	= LightSourceOffset;
			if (CastsShadows)
			{
				LightSource.shadows					= LightShadows.Hard;
			}
			else
			{
				LightSource.shadows					= LightShadows.Soft;
			}
			
			if (Flickers)
			{
				LightFlicker flicker 	= LightSource.gameObject.AddComponent <LightFlicker> ( );
				flicker.baseIntensity 	= LightSourceIntensity;
				flicker.enabled 		= true;
			}
			
			newLightGameObject.transform.parent = transform;
		}
	}
}