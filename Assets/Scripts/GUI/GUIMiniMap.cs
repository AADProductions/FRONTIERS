using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI {
	public class GUIMiniMap : MonoBehaviour
	{
		public float 		TargetOffset			= -1.0f;
		public UIAnchor 	MiniMapAnchor;
		public GameObject 	CompassObject;
		protected float		CompassMaxWrap			= 0.0f;
		protected float		CompassMinWrap			= -400.0f;
		protected float		CompassFullRotation		= -400.0f;
		protected float		CompassOffset			= 0.0f;
		public GameObject	DaylightObject;
		public UISprite		DaylightBGSprite;
		public UISprite		DaylightBGOverlaySprite;
		protected float		DaylightMinWrap			= -400.0f;
		protected float		IconMaxHeight			= 30.0f;
		protected float		IconOffset				= -24.0f;
		public GameObject	SunIcon;
		public GameObject	MoonIcon;
		public Color		DaylightBGColor;
		public Color		NightBGColor;
		public GameObject	TempObject;
		public UISprite		TempBGSprite;
		protected float		TempMinWrap				= -200.0f;
		public Color		TempWarmBGColor;
		public Color		TempColdBGColor;
		public Color		TempHotBGColor;

		public void Start ( )
		{
			//Player.Get.AvatarActions.Subscribe (AvatarAction.InterfaceMaximize, new ActionListener (InterfaceMaximizeOrMinimize));
			//Player.Get.AvatarActions.Subscribe (AvatarAction.InterfaceMinimize, new ActionListener (InterfaceMaximizeOrMinimize));
			//Player.Get.AvatarActions.Subscribe (AvatarAction.RegionLoad, new ActionListener (RegionLoad));
		}

		public void RegionLoad ( )
		{
			if (PrimaryInterface.IsMaximized ("Map"))
			{
				TargetOffset = -1.0f;
			}
			else
			{
				TargetOffset = 0.0f;
			}
		}

		public void InterfaceMaximizeOrMinimize ( )
		{
			if (PrimaryInterface.IsMaximized ("Map"))
			{
				TargetOffset = -1.0f;
			}
			else
			{
				TargetOffset = 0.0f;
			}
		}

		public void Update ( )
		{
			float currentOffset = Mathf.Lerp (MiniMapAnchor.relativeOffset.y, TargetOffset, 0.5f);
			MiniMapAnchor.relativeOffset = new Vector2 (0f, currentOffset);

	//		float compassHeading = ((Player.Rotation.y / 360.0f) * CompassFullRotation) + CompassOffset;
	//		if (compassHeading > CompassMaxWrap)
	//		{
	//			compassHeading-= 200.0f;
	//		}
	//		else if (compassHeading < CompassMinWrap)
	//		{
	//			compassHeading+= 200.0f;
	//		}
	//
	//		CompassObject.transform.localPosition 	= new Vector3 (compassHeading, 0f, 0f);
	//
	//		float daylightPosition 					= WorldClock.DayCycleCurrentNormalized * DaylightMinWrap;
	//		DaylightObject.transform.localPosition 	= new Vector3 (daylightPosition, 0f, 0f);
	//		float sunPosition 						= Biomes.NormalizedSunPosition * IconMaxHeight + IconOffset;
	//		float moonPosition 						= Biomes.NormalizedMoonPosition * IconMaxHeight + IconOffset;
	//		SunIcon.transform.localPosition 		= new Vector3 (0f, sunPosition, 0f);
	//		MoonIcon.transform.localPosition 		= new Vector3 (0f, moonPosition, 0f);
	//		
	//		DaylightBGSprite.color 					= Color.Lerp (DaylightBGColor, NightBGColor, Biomes.NormalizedMoonPosition);
	//		DaylightBGOverlaySprite.color 			= DaylightBGSprite.color;
	//		
	////		//Debug.Log ("Normalized temp: " + Biomes.NormalizedTemperature + ", cold temp: " + Biomes.NormalizedColdTemperature + ", hot temp: " + Biomes.NormalizedHotTemperature);
	//		TempObject.transform.localPosition		= new Vector3 (Biomes.NormalizedTemperature * TempMinWrap, 0f, 0f);
	//		Color tempBGSpriteColor					= Color.Lerp (TempWarmBGColor, TempColdBGColor, Biomes.NormalizedColdTemperature);
	//		tempBGSpriteColor						= Color.Lerp (tempBGSpriteColor, TempHotBGColor, Biomes.NormalizedHotTemperature);
	//		TempBGSprite.color						= tempBGSpriteColor;
		}
	}
}