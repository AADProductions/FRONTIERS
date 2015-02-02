using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

namespace Frontiers
{
		public class PreparedFoods : Manager
		{
				public static PreparedFoods Get;
				public List <PreparedFood> PreparedFoodList = new List <PreparedFood>();
				public List <Texture2D> FoodTextures = new List <Texture2D>();
				public Texture2D ChunksTexture;
				public Texture2D ToppingsTexture;
				public Material PlateMaterial;
				public Material BowlMaterial;
				public Material ChunksMaterial;
				public Material ToppingMaterial;
				public Material FoodMaterial;
				public MeshFilter PlatePrefab;
				public MeshFilter BowlPrefab;
				public MeshFilter ChunksPrefab;
				public MeshFilter ToppingsPrefab;
				public MeshFilter MoundFoodPrefab;
				public MeshFilter FlatFoodPrefab;
				public List <Material> BakedGoodsMaterials = new List <Material>();
				public List <MeshFilter> BakedGoodsPrefabs = new List <MeshFilter>();
				public List <Vector3> IngredientOffsets = new List <Vector3>();
				public Vector3 MoundFoodOffsetPlate;
				public Vector3 MoundFoodOffsetBowl;
				public Vector3 FlatFoodOffsetBowl;
				public Vector3 ChunksOffsetBowl;
				public Vector3 ChunksOffsetPlate;
				public Vector3 PlateOffset;
				public Vector3 BowlOffset;
				#if UNITY_EDITOR
				public void DrawEditor()
				{
						UnityEngine.GUI.color = Color.gray;
						if (GUILayout.Button("Fix prepared foods")) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor();
								Mods.Get.Editor.LoadAvailableMods <PreparedFood>(PreparedFoodList, "PreparedFood");
								foreach (PreparedFood pf in PreparedFoodList) {
										if (!string.IsNullOrEmpty(pf.BaseTextureName) && pf.BaseTextureName.Length > 1) {
												pf.BaseTextureName = pf.BaseTextureName.Substring(pf.BaseTextureName.Length - 1, 1);//only take the last character
												Mods.Get.Editor.SaveMod <PreparedFood>(pf, "PreparedFood", pf.Name);
										}
								}
						}
				}
				#endif
				public override void WakeUp()
				{
						Get = this;
						mPreparedFoodLookup = new Dictionary <string, PreparedFood>();
				}

				public static bool GetPreparedFood(string preparedFoodName, out PreparedFood preparedFood)
				{
						return mPreparedFoodLookup.TryGetValue(preparedFoodName, out preparedFood);
				}

				public override void OnModsLoadStart()
				{
						Mods.Get.Runtime.LoadAvailableMods(PreparedFoodList, "PreparedFood");
						for (int i = 0; i < PreparedFoodList.Count; i++) {
								PreparedFood pf = PreparedFoodList[i];
								mPreparedFoodLookup.Add(PreparedFoodList[i].Name, pf);
								//create the foodstuff props
								FoodStuffProps rawProps = null;
								FoodStuffProps cookedProps = new FoodStuffProps();
								if (pf.CanBeRaw) {
										rawProps = new FoodStuffProps();
										rawProps.Name = "Raw";
										rawProps.ConditionName = "FoodPoisoning";
										rawProps.ConditionChance = 0.75f;
										rawProps.Type = FoodStuffEdibleType.Edible;
										rawProps.IsLiquid = pf.IsLiquid;
										rawProps.HungerRestore = PlayerStatusRestore.B_OneFifth;
										pf.RawProps = rawProps;
								}
								cookedProps = new FoodStuffProps();
								cookedProps.Name = "Cooked";
								cookedProps.ConditionName = "WellRested";
								cookedProps.ConditionChance = 0.15f;
								cookedProps.Type = FoodStuffEdibleType.Edible | FoodStuffEdibleType.WellFed;
								cookedProps.IsLiquid = pf.IsLiquid;
								cookedProps.HungerRestore = pf.HungerRestore;
								cookedProps.CustomStatusKeeperRestore = "Strength";
								cookedProps.CustomRestore = PlayerStatusRestore.F_Full;
								pf.CookedProps = cookedProps;
						}

						//initialize the prepared food pieces
						for (int i = 0; i < 9; i++) {//KLUDGE move this to a global so it works for all blueprints regardless of length
								Transform nextIngredientPivot = PlatePrefab.transform.FindChild("Ingredient_" + i.ToString());
								IngredientOffsets.Add(nextIngredientPivot.localPosition);
						}

						for (int i = 0; i < BakedGoodsPrefabs.Count; i++) {
								MeshRenderer mr = BakedGoodsPrefabs[i].GetComponent <MeshRenderer>();
								BakedGoodsMaterials.Add(mr.sharedMaterial);
						}
				}

				protected void FindIngredients(PreparedFood preparedFood)
				{
						List <GenericWorldItem> ingredients = new List <GenericWorldItem>();
						preparedFood.Ingredients = ingredients;
				}

				public static void InitializeAvatar(PreparedFoodAvatar preparedFoodAvatar)
				{
						if (string.IsNullOrEmpty(preparedFoodAvatar.worlditem.Subcategory)) {
								//get a random category
								preparedFoodAvatar.worlditem.Props.Local.Subcategory = Get.PreparedFoodList[UnityEngine.Random.Range(0, Get.PreparedFoodList.LastIndex())].Name;
						}

						PreparedFood preparedFood = null;
						if (!mPreparedFoodLookup.TryGetValue(preparedFoodAvatar.worlditem.Subcategory, out preparedFood)) {
								Debug.Log("Couldn't initialize prepared food avatar " + preparedFoodAvatar.name);
								return;
						}
						preparedFoodAvatar.Props = preparedFood;
						//set up the prepared food states
						WIState rawState = preparedFoodAvatar.worlditem.States.GetState("Raw");
						WIState cookedState = preparedFoodAvatar.worlditem.States.GetState("Cooked");
						cookedState.Enabled = true;
						if (preparedFood.CanBeRaw) {
								//if it can be raw / cooked
								//indicate this with a suffix
								cookedState.Suffix = "Cooked";
								rawState.Suffix = "Raw";
								rawState.Enabled = true;
								preparedFoodAvatar.worlditem.States.DefaultState = "Raw";
						} else {
								//otherwise just show up as the normal name
								cookedState.Suffix = string.Empty;
								rawState.Enabled = false;
								preparedFoodAvatar.worlditem.States.DefaultState = "Cooked";
						}

						//set up the edible properties
						FoodStuff foodStuff = preparedFoodAvatar.worlditem.Get <FoodStuff>();
						foodStuff.State.ConsumeOnEat = true;
						/*foodStuff.State.PotentialProps.Clear();
						if (preparedFood.CanBeRaw) {
								foodStuff.State.PotentialProps.Add(preparedFood.RawProps);
						}
						//set the cook time regardless since food can still be burned
						*/
						foodStuff.CookTimeRTSeconds = preparedFood.RTCookDuration;
						//foodStuff.State.PotentialProps.Add(preparedFood.CookedProps);
						foodStuff.RefreshFoodStuffProps();
						//the foodstuff will update itself based on whether the state is raw / cooked
				}

				public static bool InitializePreparedFoodGameObject(GameObject preparedFoodParentObject, string preparedFoodName, bool createCollider, ref Bounds itemBounds)
				{
						PreparedFood pf = null;
						if (!mPreparedFoodLookup.TryGetValue(preparedFoodName, out pf)) {
								Debug.Log("Couldn't find prepared food name " + preparedFoodName);
								return false;
						}

						CreateFoodStateChild(preparedFoodParentObject, "Raw", pf, createCollider, ref itemBounds);
						CreateFoodStateChild(preparedFoodParentObject, "Cooked", pf, createCollider, ref itemBounds);
						return true;
				}

				protected static void CreateFoodStateChild(GameObject preparedFoodParentObject, string stateChildName, PreparedFood pf, bool createCollider, ref Bounds itemBounds)
				{
						List <Renderer> renderers = new List <Renderer>();

						preparedFoodParentObject.layer = Globals.LayerNumWorldItemActive;
						GameObject preparedFoodGameObject = preparedFoodParentObject.FindOrCreateChild(stateChildName).gameObject;

						switch (pf.FoodType) {
								default:
								case PreparedFoodType.BakedGoods:
										GameObject bakedGood = Get.CreateBakedGood(preparedFoodGameObject, pf.BakedGoodsIndex, renderers);
										if (pf.BakedGoodsToppings) {
												Vector3 offset = Vector3.zero;
												Transform offsetTransform = bakedGood.transform.FindChild("ToppingsOffset");
												if (offsetTransform != null) {
														offset = offsetTransform.localPosition;
												}
												Get.CreateToppings(Get.ToppingsPrefab, Get.ToppingMaterial, bakedGood, pf.ToppingColor, offset, renderers);
										}
										break;

								case PreparedFoodType.PlateOrBowl:
										switch (pf.FoodStyle) {
												case PreparedFoodStyle.PlateIngredients:
												case PreparedFoodStyle.BowlIngredients:
												//if we haven't already looked up the ingredients
												//based on the blueprint
												//do that now before sending it over
														if (pf.Ingredients == null) {
																Get.FindIngredients(pf);
														}
														Get.CreateIngredients(Get.CreatePlate(preparedFoodGameObject, renderers), pf.Ingredients, renderers);
														break;

												case PreparedFoodStyle.PlateMound:
														Get.CreateFood(Get.MoundFoodPrefab, Get.CreatePlate(preparedFoodGameObject, renderers), pf.BaseTextureName, pf.BaseTextureColor, Get.MoundFoodOffsetPlate, renderers);
														break;

												case PreparedFoodStyle.PlateMoundToppings:
														GameObject plateMoundToppings = Get.CreateBowl(preparedFoodGameObject, renderers);
														Get.CreateFood(Get.MoundFoodPrefab, plateMoundToppings, pf.BaseTextureName, pf.BaseTextureColor, Get.MoundFoodOffsetPlate, renderers);
														Get.CreateToppings(Get.ChunksPrefab, Get.ChunksMaterial, plateMoundToppings, pf.ToppingColor, Get.ChunksOffsetBowl, renderers);
														break;

												case PreparedFoodStyle.BowlFlat:
														Get.CreateFood(Get.FlatFoodPrefab, Get.CreateBowl(preparedFoodGameObject, renderers), pf.BaseTextureName, pf.BaseTextureColor, Get.FlatFoodOffsetBowl, renderers);
														break;

												case PreparedFoodStyle.BowlMound:
														Get.CreateFood(Get.MoundFoodPrefab, Get.CreateBowl(preparedFoodGameObject, renderers), pf.BaseTextureName, pf.BaseTextureColor, Get.MoundFoodOffsetBowl, renderers);
														break;

												case PreparedFoodStyle.BowlFlatToppings:
														GameObject bowlFlatToppings = Get.CreateBowl(preparedFoodGameObject, renderers);
														Get.CreateFood(Get.FlatFoodPrefab, bowlFlatToppings, pf.BaseTextureName, pf.BaseTextureColor, Get.FlatFoodOffsetBowl, renderers);
														Get.CreateToppings(Get.ChunksPrefab, Get.ChunksMaterial, bowlFlatToppings, pf.ToppingColor, Get.ChunksOffsetBowl, renderers);
														break;

												case PreparedFoodStyle.BowlMoundToppings:
														GameObject bowlMoundToppings = Get.CreateBowl(preparedFoodGameObject, renderers);
														Get.CreateFood(Get.MoundFoodPrefab, bowlMoundToppings, pf.BaseTextureName, pf.BaseTextureColor, Get.MoundFoodOffsetBowl, renderers);
														Get.CreateToppings(Get.ChunksPrefab, Get.ChunksMaterial, bowlMoundToppings, pf.ToppingColor, Get.ChunksOffsetBowl, renderers);
														break;

												default:
														break;

										}
										break;
						}
						preparedFoodGameObject.layer = Globals.LayerNumWorldItemActive;
						itemBounds.center = preparedFoodGameObject.transform.position;
						itemBounds.size = Vector3.zero;
						for (int i = 0; i < renderers.Count; i++) {
								itemBounds.Encapsulate(renderers[i].bounds);
						}
						if (createCollider) {
								BoxCollider bc = preparedFoodGameObject.GetOrAdd <BoxCollider>();
								bc.size = itemBounds.size;
						}
				}

				protected void CreateToppings(MeshFilter toppingsPrefab, Material toppingsMaterial, GameObject toppingsParent, Color toppingColor, Vector3 offset, List <Renderer> renderers)
				{
						GameObject toppingsObject = toppingsParent.FindOrCreateChild("Toppings").gameObject;
						MeshFilter mf = toppingsObject.GetOrAdd <MeshFilter>();
						MeshRenderer mr = toppingsObject.GetOrAdd <MeshRenderer>();
						mf.sharedMesh = toppingsPrefab.sharedMesh;
						mr.material = new Material(toppingsMaterial);
						mr.material.SetColor("_Color", toppingColor);
						renderers.Add(mr);
						toppingsObject.transform.localScale = toppingsPrefab.transform.localScale;
						toppingsObject.transform.localRotation = Quaternion.identity;
						toppingsObject.transform.localPosition = offset;
						toppingsObject.transform.Rotate(toppingsPrefab.transform.localRotation.eulerAngles);
				}

				protected void CreateIngredients(GameObject ingredientsParent, List <GenericWorldItem> ingredients, List <Renderer> renderers)
				{
						for (int i = 0; i < ingredients.Count; i++) {
								if (ingredients[i] != null && !ingredients[i].IsEmpty) {
										//if it's not an empty spot
										//get an avatar for the item
										GenericWorldItem ingredient = ingredients[i];
										Transform ingredientParent = ingredientsParent.CreateChild("Ingredient_" + i.ToString());
										if (ingredientParent != null) {
												ingredientParent.localPosition = IngredientOffsets[i];
												GameObject ingredientDoppleganger = WorldItems.GetDoppleganger(
														                              ingredient,
														                              ingredientParent,
														                              null,
														                              WIMode.World);
										}
								}
						}
				}

				protected GameObject CreatePlate(GameObject plateParent, List <Renderer> renderers)
				{
						GameObject plateGameobject = plateParent.FindOrCreateChild("Plate").gameObject;
						MeshFilter mf = plateGameobject.GetOrAdd <MeshFilter>();
						MeshRenderer mr = plateGameobject.GetOrAdd <MeshRenderer>();
						mf.sharedMesh = PlatePrefab.sharedMesh;
						mr.sharedMaterial = PlateMaterial;
						renderers.Add(mr);
						plateGameobject.transform.localPosition = PlateOffset;
						plateGameobject.transform.localScale = PlatePrefab.transform.localScale;
						plateGameobject.transform.localRotation = PlatePrefab.transform.localRotation;
						return plateGameobject;
				}

				protected GameObject CreateBowl(GameObject bowlParent, List <Renderer> renderers)
				{
						GameObject bowlGameobject = bowlParent.FindOrCreateChild("Bowl").gameObject;
						MeshFilter mf = bowlGameobject.GetOrAdd <MeshFilter>();
						MeshRenderer mr = bowlGameobject.GetOrAdd <MeshRenderer>();
						mf.sharedMesh = BowlPrefab.sharedMesh;
						mr.sharedMaterial = BowlMaterial;
						renderers.Add(mr);
						bowlGameobject.transform.localPosition = BowlOffset;
						bowlGameobject.transform.localScale = BowlPrefab.transform.localScale;
						bowlGameobject.transform.localRotation = BowlPrefab.transform.localRotation;
						return bowlGameobject;
				}

				protected void CreateFood(MeshFilter foodPrefab, GameObject flatFoodParent, string textureName, Color textureColor, Vector3 offset, List <Renderer> renderers)
				{
						GameObject flatFoodGameobject = flatFoodParent.FindOrCreateChild(foodPrefab.name).gameObject;
						MeshFilter mf = flatFoodGameobject.GetOrAdd <MeshFilter>();
						MeshRenderer mr = flatFoodGameobject.GetOrAdd <MeshRenderer>();
						mf.sharedMesh = foodPrefab.sharedMesh;
						mr.material = new Material(FoodMaterial);
						renderers.Add(mr);
						for (int i = 0; i < FoodTextures.Count; i++) {
								if (FoodTextures[i].name == textureName) {
										mr.material.SetTexture("_MainTex", FoodTextures[i]);
										mr.material.SetColor("_EyeColor", textureColor);
										break;
								}
						}
						flatFoodGameobject.transform.localScale = foodPrefab.transform.localScale;
						flatFoodGameobject.transform.localPosition = offset;
						flatFoodGameobject.transform.localRotation = foodPrefab.transform.localRotation;
						//add a box collider
				}

				protected GameObject CreateBakedGood(GameObject bakedGoodParent, int bakedGoodsIndex, List <Renderer> renderers)
				{
						GameObject bakedGoodGameobject = bakedGoodParent.FindOrCreateChild("BakedGood").gameObject;
						MeshFilter mf = bakedGoodGameobject.GetOrAdd <MeshFilter>();
						MeshRenderer mr = bakedGoodGameobject.GetOrAdd <MeshRenderer>();
						MeshFilter bakedGoodsPrefab = BakedGoodsPrefabs[bakedGoodsIndex];
						mf.sharedMesh = bakedGoodsPrefab.sharedMesh;
						mr.material = BakedGoodsMaterials[bakedGoodsIndex];
						renderers.Add(mr);
						bakedGoodGameobject.transform.localPosition = bakedGoodsPrefab.transform.localPosition;
						bakedGoodGameobject.transform.localScale = bakedGoodsPrefab.transform.localScale;
						bakedGoodGameobject.transform.localRotation = bakedGoodsPrefab.transform.localRotation;
						return bakedGoodGameobject;
				}

				protected static Dictionary <string, PreparedFood> mPreparedFoodLookup;
		}
}