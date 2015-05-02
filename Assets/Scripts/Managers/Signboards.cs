using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers
{
		public class Signboards : Manager
		{
				public static Signboards Get;

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
						mCheckingSignboards = false;
				}

				public override void OnGameStart()
				{
						if (!mCheckingSignboards) {
								mCheckingSignboards = true;
								StartCoroutine(CheckSignboards());
						}
				}

				public static WorldItem SignboardWorldItem;
				public Texture2D BarATexture;
				public Texture2D BarBTexture;
				public Texture2D InnATexture;
				public Texture2D InnBTexture;
				public Texture2D ShopATexture;
				public Texture2D ShopBTexture;
				public Texture2D MiscATexture;
				public Texture2D MiscBTexture;
				public Texture2D LandmarkTexture;
				public Signboard BarASignboardClosest;
				public Signboard BarBSignboardClosest;
				public Signboard InnASignboardClosest;
				public Signboard InnBSignboardlosest;
				public Signboard ShopASignboardClosest;
				public Signboard ShopBSignboardClosest;
				public List <Signboard> BarASignboards = new List <Signboard>();
				public List <Signboard> BarBSignboards = new List <Signboard>();
				public List <Signboard> InnASignboards = new List <Signboard>();
				public List <Signboard> InnBSignboards = new List <Signboard>();
				public List <Signboard> ShopASignboards = new List <Signboard>();
				public List <Signboard> ShopBSignboards = new List <Signboard>();
				public List <Signboard> MiscASignboards = new List <Signboard>();
				public List <Signboard> MiscBSignboards = new List <Signboard>();
				public List <Signboard> LandmarkSignboards = new List <Signboard>();

				public static Signboard AddInn(Signboard newSign, WorldItem owner, WIGroup locationGroup, STransform offset, string textureName)
				{
						newSign = GetOrCreateSignboard(newSign, owner, locationGroup, offset, textureName, "Inn");
						switch (newSign.Style) {
								case SignboardStyle.A:
								default:
										Get.InnASignboards.SafeAdd(newSign);
										break;

								case SignboardStyle.B:
										Get.InnBSignboards.SafeAdd(newSign);
										break;
						}
						return newSign;
				}

				public static Signboard AddShop(Signboard newSign, WorldItem owner, WIGroup locationGroup, STransform offset, string textureName)
				{
						newSign = GetOrCreateSignboard(newSign, owner, locationGroup, offset, textureName, "Shop");
						switch (newSign.Style) {
								case SignboardStyle.A:
								default:
										Get.ShopASignboards.SafeAdd(newSign);
										break;

								case SignboardStyle.B:
										Get.ShopBSignboards.SafeAdd(newSign);
										break;
						}
						return newSign;
				}

				public static Signboard AddBar(Signboard newSign, WorldItem owner, WIGroup locationGroup, STransform offset, string textureName)
				{
						newSign = GetOrCreateSignboard(newSign, owner, locationGroup, offset, textureName, "Bar");
						switch (newSign.Style) {
								case SignboardStyle.A:
								default:
										Get.BarASignboards.SafeAdd(newSign);
										break;

								case SignboardStyle.B:
										Get.BarBSignboards.SafeAdd(newSign);
										break;
						}
						return newSign;
				}

				public static Signboard AddLandmark(Signboard newSign, WorldItem owner, WIGroup locationGroup, STransform offset, string textureName)
				{
						newSign = GetOrCreateSignboard(newSign, owner, locationGroup, offset, textureName, "Landmark");
						Get.LandmarkSignboards.SafeAdd(newSign);
						return newSign;
				}

				public static Signboard GetOrCreateSignboard(Signboard sign, WorldItem owner, WIGroup locationGroup, STransform offset, string textureName, string type)
				{
						if (sign == null) {
								WorldItem newSignWorldItem = null;
								WorldItems.CloneWorldItem("Decorations", "Signboard", offset, false, locationGroup, out newSignWorldItem);
								newSignWorldItem.Initialize();
								//offset.ApplyTo (newSignWorldItem.transform, false);
								sign = newSignWorldItem.GetOrAdd <Signboard>();
								sign.Owner = owner;
								sign.TextureName = textureName;
								sign.Style = SignboardStyle.A;
								if (!string.IsNullOrEmpty(textureName)) {
										if (textureName.StartsWith("B_")) {
												sign.Style = SignboardStyle.B;
										}
										//Debug.Log ("Setting signoboard state to " + type + sign.Style.ToString ());
										newSignWorldItem.State = type + sign.Style.ToString();
								}
						}
						return sign;
				}

				public IEnumerator CheckSignboards()
				{
						//here we go through the signboards and check which one is closest
						while (mCheckingSignboards) {
								yield return null;
								//check bars
								VisibilityComparer.PlayerPosition = Player.Local.Position;
								CleanAndSort(BarASignboards);
								CleanAndSort(BarBSignboards);
								yield return null;
								SetClosestTexture(BarASignboards, ref BarASignboardClosest, BarATexture, "Bar");
								SetClosestTexture(BarBSignboards, ref BarBSignboardClosest, BarBTexture, "Bar");
								yield return mWaitForCheckSignboards;

								//check inns
								VisibilityComparer.PlayerPosition = Player.Local.Position;
								CleanAndSort(InnASignboards);
								CleanAndSort(InnBSignboards);
								yield return null;
								SetClosestTexture(InnASignboards, ref InnASignboardClosest, InnATexture, "Inn");
								SetClosestTexture(InnBSignboards, ref InnBSignboardlosest, InnBTexture, "Inn");
								yield return mWaitForCheckSignboards;

								//check shops
								VisibilityComparer.PlayerPosition = Player.Local.Position;
								CleanAndSort(ShopASignboards);
								CleanAndSort(ShopBSignboards);
								yield return null;
								SetClosestTexture(ShopASignboards, ref ShopASignboardClosest, ShopATexture, "Shop");
								SetClosestTexture(ShopBSignboards, ref ShopBSignboardClosest, ShopBTexture, "Shop");
								yield return mWaitForCheckSignboards;

								//check misc

								//check landmarks
						}
						yield break;
				}

				protected WaitForSeconds mWaitForCheckSignboards = new WaitForSeconds(1f);

				protected static void SetClosestTexture(List <Signboard> signs, ref Signboard current, Texture2D texture, string type)
				{
						if (signs.Count > 0) {
								Signboard sign = signs[0];
								if (sign != current) {
										current = sign;
										//Debug.Log ("Closest texture is " + sign.name);
										if (texture.name != sign.TextureName) {
												//get the texture from mods and set pixels
												if (!Mods.Get.Runtime.Texture(texture, "Signboard", sign.TextureName)) {
														////Debug.Log ("COULDN'T LOAD TEXTURE " + sign.TextureName);
												} else {
														//Debug.Log ("LOADED SIGN BOARD TEXTURE " + sign.TextureName);
												}
										}
										//Debug.Log ("Setting state to " + sign.Style.ToString ());
										sign.worlditem.State = type + sign.Style.ToString();
								}
						}
				}

				protected static void CleanAndSort(List <Signboard> signs)
				{
						for (int i = signs.LastIndex(); i >= 0; i--) {
								if (signs[i] == null) {
										signs.RemoveAt(i);
								}
						}
						if (signs.Count > 0) {
								signs.Sort(VisibilityComparer);
						}
				}

				protected static SignboardComparer VisibilityComparer = new SignboardComparer();
				protected bool mCheckingSignboards = false;

				public class SignboardComparer : IComparer <Signboard>
				{
						public Vector3 PlayerPosition;

						public int Compare(Signboard a, Signboard b)
						{
								//this is the first time it's been calculated this cycle
								//aldo check whether we need to refresh our active state
								return a.worlditem.DistanceToPlayer.CompareTo(b.worlditem.DistanceToPlayer);
						}
				}
		}
}
