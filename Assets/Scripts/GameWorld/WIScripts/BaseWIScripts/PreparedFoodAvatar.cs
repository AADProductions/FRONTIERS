using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System.Xml.Serialization;
using System;

namespace Frontiers.World.WIScripts
{
		public class PreparedFoodAvatar : WIScript
		{
				public PreparedFoodAvatarState State = new PreparedFoodAvatarState();

				public bool HasProps {
						get {
								return Props != null;
						}
				}

				public PreparedFood Props = null;

				public override void OnInitialized()
				{
						PreparedFoods.InitializeAvatar(this);
						PreparedFoods.InitializePreparedFoodGameObject(gameObject, Props.Name, true, ref worlditem.BaseObjectBounds);
				}
		}

		[Serializable]
		public class PreparedFoodAvatarState
		{
				public string PreparedFoodName = string.Empty;
		}

		[Serializable]
		public class PreparedFood : Mod
		{
				public PreparedFoodType FoodType = PreparedFoodType.PlateOrBowl;
				public PreparedFoodStyle FoodStyle = PreparedFoodStyle.PlateIngredients;
				public bool CanBeRaw = false;
				public bool IsLiquid = false;
				public string BaseTextureName = string.Empty;
				public int BakedGoodsIndex;
				public bool BakedGoodsToppings = false;
				public SColor BaseTextureColor = Color.white;
				public SColor ToppingColor = Color.white;
				public PlayerStatusRestore HungerRestore = PlayerStatusRestore.A_None;
				public float RTCookDuration = 1f;
				[XmlIgnore]
				public FoodStuffProps RawProps = null;
				[XmlIgnore]
				public FoodStuffProps CookedProps = null;
				[XmlIgnore]
				public List <GenericWorldItem> Ingredients = null;
		}
}