using UnityEngine;
using System.Collections;
using Frontiers.World;
using Frontiers.World.Locations;
using System.Collections.Generic;

namespace Frontiers
{
	public class Potions : Manager
	{
		public static Potions Get;

		public List <Potion> PotionList = new List <Potion> ( );

		public override void WakeUp ()
		{
			Get = this;
			mPotionLookup = new Dictionary <string, Potion> ( );
			mPotionLookup.Clear ();
			PotionList.Clear ();

			if (!Application.isPlaying) {
				if (!Manager.IsAwake <Mods> ()) {
					Manager.WakeUp <Mods> ("__MODS");
				}
				Mods.Get.Editor.InitializeEditor (true);

				List <string> potionNames = Mods.Get.Available ("Potion");

				for (int i = 0; i < potionNames.Count; i++) {
					Potion potion = null;
					if (Application.isPlaying) {
						if (Mods.Get.Runtime.LoadMod <Potion> (ref potion, "Potion", potionNames [i])) {
							mPotionLookup.Add (potion.Name.ToLower ().Trim ().Replace (" ", ""), potion);
							PotionList.Add (potion);
						}
					} else {
						if (Mods.Get.Editor.LoadMod <Potion> (ref potion, "Potion", potionNames [i])) {
							mPotionLookup.Add (potion.Name.ToLower ().Trim ().Replace (" ", ""), potion);
							PotionList.Add (potion);
						}
					}
				}
			}

			gPotionGenericWorldItem = new GenericWorldItem ();
			gPotionGenericWorldItem.PackName = "MedicalSupplies";
			gPotionGenericWorldItem.PrefabName = "Potion Bottle 1";
			//gPotionGenericWorldItem.State = "Default";
			gPotionGenericWorldItem.DisplayName = "Potion";
			gPotionGenericWorldItem.Subcategory = "HealingPotion";
		}

		public override void OnInitialized ()
		{
			PotionList.Clear ();
			mPotionLookup.Clear ();

			Mods.Get.Runtime.LoadAvailableMods <Potion> (PotionList, "Potion");

			for (int i = 0; i < PotionList.Count; i++) {
				mPotionLookup.Add (PotionList [i].Name, PotionList [i]);
				//Debug.Log ("Added " + PotionList [i].Name + " To lookup");
			}

			mInitialized = true;
		}

		public static void InitializeAvatar (PotionAvatar potionAvatar)
		{
			Potion potion = null;
			if (string.IsNullOrEmpty (potionAvatar.State.PotionName)) {
				//get a potion name - start by getting the potion's flags
				WIFlags flags = null;
				IStackOwner potionOwner = null;
				if (potionAvatar.worlditem.Group.HasOwner (out potionOwner)) {
					WorldItem worlditem = null;
					if (potionOwner.IsWorldItem) {
						Structure structure = null;
						City city = null;
						if (potionOwner.worlditem.Is <Structure> (out structure)) {
							flags = structure.State.StructureFlags;
						} else if (potionOwner.worlditem.Is <City> (out city)) {
							flags = city.State.StructureFlags;
						}
					}
				}
				//if we couldn't find the flags
				if (flags == null) {
					//just get the chunk flags
					WorldChunk chunk = potionAvatar.worlditem.Group.GetParentChunk ();
					flags = GameWorld.Get.CurrentRegion.StructureFlags;
				}
				potion = Get.RandomPotion (flags);
				potionAvatar.Props = potion;
				potionAvatar.State.PotionName = potion.Name;
				potionAvatar.worlditem.Props.Local.Subcategory = potion.Name;
			} else {
				if (mPotionLookup.TryGetValue (potionAvatar.State.PotionName, out potion)) {
					potionAvatar.Props = potion;
				}
			}
			//potion is guaranteed to return a health potion
			//potions are guaranteed to have one renderer
			Renderer renderer = potionAvatar.worlditem.Renderers [0];
			Material potionMaterial = new Material (renderer.sharedMaterial);//instance the material
			potionMaterial.SetColor ("_SkinColor", Colors.Get.ColorFromFlagset (potion.PotionType, "PotionType"));
			potionMaterial.SetColor ("_EyeColor", Colors.Get.ByName (potion.ContainerColor));
			if (potion.UseCustomColor) {
				potionMaterial.SetColor ("_HairColor", potion.CustomPotionColor);
			} else {
				potionMaterial.SetColor ("_EyeColor", Colors.Get.ByName (potion.PotionColor));
			}
		}

		public void AquirePotion (string potionName, int numItems)
		{
			Potion potion = null;
			if (mPotionLookup.TryGetValue (potionName, out potion)) {
				//create a potion stack item from the generic world item
				gPotionGenericWorldItem.Subcategory = potionName;
				gPotionGenericWorldItem.DisplayName = potion.CommonName;
				Debug.Log ("Got potion " + potionName + " now adding to inventory");
				for (int i = 0; i < numItems; i++) {
					StackItem potionStackItem = gPotionGenericWorldItem.ToStackItem ();
					WIStackError error = WIStackError.None;
					if (!Player.Local.Inventory.AddItems (potionStackItem, ref error)) {
						Debug.Log ("Couldn't add potion " + potionName + " to inventory for some reason");
					}
				}
			} else {
				Debug.Log ("Couldn't find potion name " + potionName + " in lookup");
			}
		}

		public Potion RandomPotion (WIFlags flags)
		{
			//default potion is health
			Potion potion = null;

			if (potion == null) {
				potion = mPotionLookup ["healthbooster"];
			}

			return potion;
		}

		#if UNITY_EDITOR
		public void DrawEditor ( ) {

			if (GUILayout.Button ("Save Potions")) {
				if (!Manager.IsAwake <Mods> ()) {
					Manager.WakeUp <Mods> ("__MODS");
				}
				Mods.Get.Editor.InitializeEditor (true);

				foreach (Potion potion in PotionList) {
					Mods.Get.Editor.SaveMod <Potion> (potion, "Potion", potion.Name);
				}
			}
			if (GUILayout.Button ("Load Potions")) {

				if (!Manager.IsAwake <Mods> ()) {
					Manager.WakeUp <Mods> ("__MODS");
				}
				Mods.Get.Editor.InitializeEditor (true);

				PotionList.Clear ();
				mPotionLookup.Clear ();

				List <string> potionNames = Mods.Get.Available ("Potion");

				for (int i = 0; i < potionNames.Count; i++) {
					Potion potion = null;
					if (Mods.Get.Editor.LoadMod <Potion> (ref potion, "Potion", potionNames [i])) {
						mPotionLookup.Add (potion.Name.ToLower ().Trim ().Replace (" ",""), potion);
						PotionList.Add (potion);
					}
				}
			}
		}
		#endif

		protected static GenericWorldItem gPotionGenericWorldItem;
		protected static Dictionary <string, Potion> mPotionLookup;
	}
}
