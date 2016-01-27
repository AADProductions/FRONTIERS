using UnityEngine;
using System;
using System.Collections;
using Frontiers;

namespace Frontiers
{		//dev tool
		public class DebugPosition : MonoBehaviour
		{
				public bool show = false;
				public UILabel X;
				public UILabel Y;
				public UILabel Z;
				public UILabel Chunk;
				public UILabel TimeInfo;
				public UILabel MultiplayerInfo;
				public GameObject TerrainTypePlane;
				public GameObject PlayerPosition;
				public UILabel ConsoleLabel;

				public void Update()
				{
						if (GameManager.Get.TestingEnvironment)
								show = false;

						if (Input.GetKeyDown(KeyCode.BackQuote)) {
								show = !show;
								UserActionManager.Suspended = show;
								InterfaceActionManager.Suspended = show;
						}
			
						if (show) {
								X.enabled = true;
								Y.enabled = true;
								Z.enabled = true;
								Chunk.enabled = true;
								TimeInfo.enabled = true;
								PlayerPosition.SetActive(true);
								TerrainTypePlane.SetActive(true);
								ConsoleLabel.enabled = true;
								MultiplayerInfo.enabled = true;


								//add console keys
								foreach (char c in Input.inputString) {
										if (c == '\b') {
												if (ConsoleLabel.text.Length > 0) {
														ConsoleLabel.text = ConsoleLabel.text.Substring(0, ConsoleLabel.text.Length - 1);
												}
										} else if ((c != '\n') && (c != '`') && (c != '\t')) {
												ConsoleLabel.text += c;
										}
								}

								if (Input.GetKeyDown(KeyCode.Return)) {
										DebugConsole.ConsoleCommand(ConsoleLabel.text);
										ConsoleLabel.text = string.Empty;
								}

								Vector2 PlayerUVPosition = GameWorld.Get.SplatmapUVFromInGamePosition(Player.Local.Position, GameWorld.Get.PrimaryChunk);
								Vector3 PlayerMapPosition = new Vector3((1.0f - PlayerUVPosition.x) * 10.0f, 0f, (1.0f - PlayerUVPosition.y) * 10.0f) - new Vector3(5f, 0f, 5f);
								PlayerPosition.transform.localPosition = PlayerMapPosition;
				
								Vector3 position = GameManager.Get.GameCamera.transform.position;
								X.text = "X: " + position.x.ToString("0.###");
								Y.text = "Y: " + position.y.ToString("0.###");
								Z.text = "Z: " + position.z.ToString("0.###");

								string timeInfo = WorldClock.AdjustedRealTime.ToString("0.#") + " - Time of day: " + WorldClock.DayCycleCurrentNormalized.ToString("0.##");
								timeInfo += (" - Timescale: " + WorldClock.Get.TimeScale.ToString("0.##") + "\n"
								+ "IsDaylight: " + WorldClock.IsDay.ToString() + "\n"
										+ "StatusTemp: " + GameWorld.Get.StatusTemperature(Player.Local.Position, WorldClock.TimeOfDayCurrent, WorldClock.TimeOfYearCurrent).ToString());// + ", Meter Temp: " + Player.Local.Status.WarmthStatusKeeper.NormalizedValue.ToString ("0.###"));

								TimeInfo.text = timeInfo;

								if (Player.Local.HasSpawned) {
										try {
												TerrainTypePlane.renderer.material.SetTexture("_MainTex", GameWorld.Get.PrimaryChunk.ChunkDataMaps["AboveGroundTerrainType"]);
										} catch {
												TerrainTypePlane.renderer.material.SetTexture("_MainTex", null);
										
												//whoops didn't have terrain type
										}
										Color currentColor = GameWorld.Get.TerrainTypeAtInGamePosition(Player.Local.Position, Player.Local.Surroundings.State.IsUnderground);
										if (!Player.Local.Surroundings.State.IsUnderground) {
												Chunk.text = "Primary Chunk (Above Ground): " + GameWorld.Get.PrimaryChunk.Name + "\n"
												+ "[FF0000]Coastal: [-]" + currentColor.r.ToString("0.###") + " (Player Coastal: " + Player.Local.Surroundings.TerrainType.r.ToString("0.###") + "\n"
												+ "[00FF00]Forest: [-]" + currentColor.g.ToString("0.###") + "\n"
												+ "[0000FF]Civilized: [-]" + currentColor.g.ToString("0.###") + "\n"
												+ "[FFFFFF]OpenField: [-]" + currentColor.a.ToString("0.###");
										} else {
												Chunk.text = "Primary Chunk (Underground): " + GameWorld.Get.PrimaryChunk.Name + "\n"
												+ "[FF0000]Shallow: [-]" + currentColor.r.ToString("0.###") + " (Player Shallow: " + Player.Local.Surroundings.TerrainType.r.ToString("0.###") + "\n"
												+ "[00FF00]Deep: [-]" + currentColor.g.ToString("0.###") + "\n"
												+ "[0000FF]Enclosed: [-]" + currentColor.g.ToString("0.###") + "\n"
												+ "[FFFFFF]Open: [-]" + currentColor.a.ToString("0.###");
										}
								}

								MultiplayerInfo.text = ("Host state: " + GameManager.HostState.ToString()
								+ "\nClient state: " + GameManager.ClientState.ToString()
								+ "\nIs connected? " + NetworkManager.Instance.IsConnected.ToString()
								+ "\nIs host? " + NetworkManager.Instance.IsHost.ToString());


						} else {
								X.enabled = false;
								Y.enabled = false;
								Z.enabled = false;
								TimeInfo.enabled = false;
								Chunk.enabled = false;
								PlayerPosition.SetActive(false);
								TerrainTypePlane.SetActive(false);
								ConsoleLabel.text = string.Empty;
								ConsoleLabel.enabled = false;
								MultiplayerInfo.enabled = false;
						}
				}
		}
}