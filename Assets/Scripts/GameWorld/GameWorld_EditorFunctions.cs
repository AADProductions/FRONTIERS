using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;

using Frontiers.World.Gameplay;
using Pathfinding;
using ExtensionMethods;
using Frontiers.GUI;
using Pathfinding.RVO;

public partial class GameWorld : Manager
{
		#if UNITY_EDITOR
		public void EditorNormalizeBiomes()
		{
				for (int i = 0; i < Biomes.Count; i++) {
						Biomes[i].WeatherAutumn.Normalize();
						Biomes[i].WeatherSpring.Normalize();
						Biomes[i].WeatherSummer.Normalize();
						Biomes[i].WeatherWinter.Normalize();

				}
		}

		public void EditorSortFlags()
		{
				WorldFlags.Sort();
		}

		public void EditorLoadSettings()
		{
				if (!Manager.IsAwake <Mods>()) {
						Manager.WakeUp <Mods>("__MODS");
				}
				Mods.Get.Editor.InitializeEditor();

				WorldFlags.Clear();
				WorldStartupPositions.Clear();
				Biomes.Clear();
				Regions.Clear();
				AudioProfiles.Clear();

				string errorMessage = string.Empty;
				GameData.IO.LoadWorld(ref Settings, "FRONTIERS", out errorMessage);
				Mods.Get.Editor.LoadAvailableMods <FlagSet>(WorldFlags, "FlagSet");
				Mods.Get.Editor.LoadAvailableMods <Biome>(Biomes, "Biome");
				Mods.Get.Editor.LoadAvailableMods <Region>(Regions, "Region");
				Mods.Get.Editor.LoadAvailableMods <AudioProfile>(AudioProfiles, "AudioProfile");
				Mods.Get.Editor.LoadAvailableMods <PlayerStartupPosition>(WorldStartupPositions, "PlayerStartupPosition");

				UnityEditor.EditorUtility.SetDirty(gameObject);
				UnityEditor.EditorUtility.SetDirty(this);
		}

		public void EditorSaveSettings()
		{
				if (!Manager.IsAwake <Mods>()) {
						Manager.WakeUp <Mods>("__MODS");
				}
				Mods.Get.Editor.InitializeEditor();
				Mods.Get.Editor.SaveMods <FlagSet>(WorldFlags, "FlagSet");
				Mods.Get.Editor.SaveMods <Biome>(Biomes, "Biome");
				Mods.Get.Editor.SaveMods <Region>(Regions, "Region");
				Mods.Get.Editor.SaveMods <AudioProfile>(AudioProfiles, "AudioProfile");
				Mods.Get.Editor.SaveMods <PlayerStartupPosition>(WorldStartupPositions, "PlayerStartupPosition");
				Settings.Version = GameManager.VersionString;
				GameData.IO.SaveWorldSettings(Settings);
		}
		#endif
}