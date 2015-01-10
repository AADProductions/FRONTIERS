using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers {
	public class IconManager : MonoBehaviour
	{
		public UIAtlas MapIconAtlas;

		public List <FlagsetIcon> FlagsetIcons = new List <FlagsetIcon> ();

		public Vector2 [] MapIconUVsByType (string type)
		{
			Vector2[] uvs = new Vector2 [4];
			UIAtlas.Sprite sprite = mMapIconByType [type];
			
			uvs [0] = new Vector2 (sprite.outer.xMax / MapIconAtlas.texture.width, sprite.outer.yMin / MapIconAtlas.texture.height); //max/min
			uvs [1] = new Vector2 (sprite.outer.xMin / MapIconAtlas.texture.width, sprite.outer.yMax / MapIconAtlas.texture.height); //min/max
			uvs [2] = new Vector2 (sprite.outer.xMin / MapIconAtlas.texture.width, sprite.outer.yMin / MapIconAtlas.texture.height); //min/min
			uvs [3] = new Vector2 (sprite.outer.xMax / MapIconAtlas.texture.width, sprite.outer.yMax / MapIconAtlas.texture.height); //max/max

			return uvs;
		}

		public void Awake ()
		{
			mMapIconByType.Add ("WorldArea", MapIconAtlas.GetSprite ("MapIconWorldArea"));
			mMapIconByType.Add ("CrossRoads", MapIconAtlas.GetSprite ("MapIconCrossRoads"));
			mMapIconByType.Add ("WayStone", MapIconAtlas.GetSprite ("MapIconWayStone"));
			mMapIconByType.Add ("GatewayEntrance", MapIconAtlas.GetSprite ("MapIconGatewayEntrance"));
			mMapIconByType.Add ("Bridge", MapIconAtlas.GetSprite ("MapIconBridge"));
			mMapIconByType.Add ("SmallStructure", MapIconAtlas.GetSprite ("MapIconSmallStructure"));
			mMapIconByType.Add ("LargeStructure", MapIconAtlas.GetSprite ("MapIconLargeStructure"));
			mMapIconByType.Add ("SmallTemple", MapIconAtlas.GetSprite ("MapIconSmallTemple"));
			mMapIconByType.Add ("LargeTemple", MapIconAtlas.GetSprite ("MapIconLargeTemple"));
			mMapIconByType.Add ("SmallRuins", MapIconAtlas.GetSprite ("MapIconSmallRuins"));
			mMapIconByType.Add ("LargeRuins", MapIconAtlas.GetSprite ("MapIconLargeRuins"));

			foreach (FlagsetIcon flagsetIcon in FlagsetIcons) {
				Dictionary <int, string> lookup = null;
				if (!mFlagsetIconLookup.TryGetValue (flagsetIcon.Flagset, out lookup)) {
					lookup = new Dictionary <int, string> ();
					mFlagsetIconLookup.Add (flagsetIcon.Flagset, lookup);
				}
				lookup.Add (flagsetIcon.Flags, flagsetIcon.IconName);
			}
		}

		public string GetIconNameFromBookType (BookType bookType)
		{
			switch (bookType) {
			case BookType.Book:
				return "BookIcon";

			case BookType.Diary:
				return "DiaryIcon";

			case BookType.Parchment:
				return "ParchmentIcon";

			case BookType.Scrap:
				return "ScrapIcon";

			case BookType.Scroll:
				return "ScrollIcon";

			case BookType.Scripture:
				return "ScriptureIcon";

			default:
				return "BookIcon";
			}
		}

		public string GetIconNameFromLocationType (string locationType)
		{
			switch (locationType) {
			case "WayStone":
				return "MapIconWayStone";

			case "Structure":
				return "MapIconSmallStructure";

			case "City":
				return "MapIconWorldArea";

			case "AnimalDen":
				return "MapIconCrossRoads";

			case "Woods":
				break;

			case "Bridge":
				return "MapIconBridge";

			default:
				break;
			}
			return string.Empty;
		}

		public string GetIconNameFromFlagset (int flag, string flagSet)
		{
			string iconName = "Novice";
			Dictionary <int,string> lookup = null;
			if (mFlagsetIconLookup.TryGetValue (flagSet, out lookup)) {
				lookup.TryGetValue (flag, out iconName);
			}
			return iconName;
		}

		public Icon NeedDirections {
			get {
				Icon newIcon = Icon.Empty;
				newIcon.IconName = "IconDirection";
				newIcon.BGColor = Color.yellow;
				return newIcon;
			}
		}

		[Serializable]
		public class FlagsetIcon
		{
			public string Flagset = "GuildCredentials";
			[FrontiersBitMaskAttribute ("GuildCredentials")]//TODO fix this
			public int Flags = 0;
			public string IconName = string.Empty;
		}

		protected Dictionary <string, Dictionary <int, string>> mFlagsetIconLookup = new Dictionary <string, Dictionary <int, string>> ();
		protected Dictionary <string, UIAtlas.Sprite> mMapIconByType = new Dictionary <string, UIAtlas.Sprite> ();
	}
}