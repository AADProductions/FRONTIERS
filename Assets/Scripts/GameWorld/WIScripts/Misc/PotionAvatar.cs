using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;

namespace Frontiers.World
{
		public class PotionAvatar : WIScript
		{
				public override string DisplayNamer(int increment)
				{
						if (!string.IsNullOrEmpty(Props.CommonName)) {
								return Props.CommonName;
						}
						return State.PotionName;
				}

				public override string FileNamer(int increment)
				{
						return State.PotionName + "-" + increment.ToString();
				}

				public override string StackNamer(int increment)
				{
						return State.PotionName;
				}

				public PotionState State = new PotionState();
				public Potion Props;

				public override void OnInitialized()
				{
						Potions.InitializeAvatar(this);

						//get our properties
						if (Mods.Get.Runtime.LoadMod <Potion>(ref Props, "Potion", State.PotionName)) {

								FoodStuff foodStuff = null;
								if (worlditem.Is <FoodStuff>(out foodStuff)) {
										foodStuff.SetProps(Props.EdibleProps);

										foodStuff.OnEat += OnEat;
								}
						}
				}

				public void OnEat()
				{
						if (!string.IsNullOrEmpty(Props.SpawnFXOnConsume)) {
								FXManager.Get.SpawnFX(Player.Local.Position, Props.SpawnFXOnConsume);
						}
				}
				#if UNITY_EDITOR
				public void EditorSavePotion()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}

						Mods.Get.Editor.SaveMod <Potion>(Props, "Potion", Props.Name);
				}

				public void EditorRefreshColors()
				{
						WorldItem worlditem = gameObject.GetComponent <WorldItem>();
						Renderer renderer = worlditem.Renderers[0];
						Material potionMaterial = renderer.sharedMaterial;//instance the material
						potionMaterial.SetColor("_SkinColor", Colors.Get.ColorFromFlagset(Props.PotionType, "PotionType"));
						potionMaterial.SetColor("_EyeColor", Colors.Get.ByName(Props.ContainerColor));
						if (Props.UseCustomColor) {
								potionMaterial.SetColor("_HairColor", Props.EditorColor);
								Props.CustomPotionColor = new SColor(Props.EditorColor);
						} else {
								potionMaterial.SetColor("_HairColor", Colors.Get.ByName(Props.PotionColor));
						}
				}

				public void EditorLoadPotion()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}

						if (Mods.Get.Editor.LoadMod <Potion>(ref Props, "Potion", State.PotionName)) {
								EditorRefreshColors();
								//Debug.Log ("Loaded");
						}
				}
				#endif
		}

		[Serializable]
		public class PotionState
		{
				[FrontiersAvailableMods("Potion")]
				public string PotionName = string.Empty;
		}

		//TODO move this to Potions_Classes
		[Serializable]
		public class Potion : Mod
		{
				public string CommonName;
				public WIFlags Flags = new WIFlags();
				public FoodStuffProps EdibleProps = new FoodStuffProps();
				[FrontiersBitMask("PotionType")]
				public int PotionType = 0;
				[FrontiersColorAttribute]
				public string ContainerColor;
				[FrontiersColorAttribute]
				public string PotionColor;
				#if UNITY_EDITOR
				[XmlIgnore]
				public Color EditorColor;
				#endif
				public bool UseCustomColor;
				[HideInInspector]
				public SColor CustomPotionColor;
				[FrontiersFXAttribute]
				public string SpawnFXOnConsume;
		}
}