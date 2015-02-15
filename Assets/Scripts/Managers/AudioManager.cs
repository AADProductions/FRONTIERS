using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers
{
		public class AudioManager : Manager
		{
				public static AudioManager Get;
				public AudioListener CurrentAudioListener;
				public MusicType CurrentMusic;
				public MusicVolume CurrentVolume;
				public AmbientAudioManager AmbientAudio;
				public MusicPlaylist PlayList = new MusicPlaylist();
				public MasterAudio Audio;
				public AmbientAudioManager.ChunkAudioSettings CurrentAudioSettings = new AmbientAudioManager.ChunkAudioSettings();
				public List <AmbientAudioClip> AmbientAudioClips = new List <AmbientAudioClip>();

				public float FadeInAudioTargetVolume {
						get {
								return RawFadeInAudioTargetVolume * Profile.Get.CurrentPreferences.Sound.Music;
						}
						set {
								RawFadeInAudioTargetVolume = value;
						}
				}

				public float RawFadeInAudioTargetVolume;
				public AudioSource Music1;
				public AudioSource Music2;
				public AudioSource FadingInAudio;
				public AudioSource FadingOutAudio;
				public WWW loader;

				public override void OnModsLoadFinish()
				{
						PlayList.Combat = GameWorld.Get.Settings.DefaultMusicCombat;
						PlayList.Cutscene = GameWorld.Get.Settings.DefaultMusicCutscene;
						PlayList.MainMenu = GameWorld.Get.Settings.DefaultMusicMainMenu;
						PlayList.Night = GameWorld.Get.Settings.DefaultMusicNight;
						PlayList.Regional = GameWorld.Get.Settings.DefaultMusicRegional;
						PlayList.SafeLocation = GameWorld.Get.Settings.DefaultMusicSafeLocation;
						PlayList.Underground = GameWorld.Get.Settings.DefaultMusicUnderground;
						mModsLoaded = true;
				}

				public static void MakeWorldSound(IItemOfInterest source, MasterAudio.SoundType soundType, string soundName)
				{
						IAudible audibleSource = (IAudible)source.gameObject.GetComponent(typeof(IAudible));
						if (audibleSource != null) {
								MakeWorldSound(audibleSource, soundType, soundName);
						}
				}

				public static void MakeWorldSound(IAudible source, MasterAudio.SoundType soundType, string soundName)
				{
						//use master audio to create a sound
						//attach an audible bubble to the source
						AudibleBubble audibleBubble = null;
						AudioSource sourceAudio = null;
						if (MasterAudio.PlaySound(soundType, source.transform, source.AudibleVolume, ref sourceAudio, soundName)) {
								source.LastSoundType = soundType;
								source.LastSoundName = soundName;
								audibleBubble = sourceAudio.gameObject.GetOrAdd <AudibleBubble>();
								audibleBubble.StartUsing(source, null, 0.25f);
						}
				}

				public static void MakeWorldSound(IAudible source, List <Collider> ignoreColliders, MasterAudio.SoundType soundType, string soundName)
				{
						//use master audio to create a sound
						//attach an audible bubble to the source
						AudibleBubble audibleBubble = null;
						AudioSource sourceAudio = null;
						if (MasterAudio.PlaySound(soundType, source.transform, source.AudibleVolume, ref sourceAudio, soundName)) {
								source.LastSoundType = soundType;
								source.LastSoundName = soundName;
								audibleBubble = sourceAudio.gameObject.GetOrAdd <AudibleBubble>();
								audibleBubble.StartUsing(source, ignoreColliders, 0.25f);
						}
				}

				public static void MakeWorldSound(IAudible Source, List <Collider> ignoreColliders, MasterAudio.SoundType SoundType)
				{
						//use master audio to create a sound
						//attach an audible bubble to the source
						AudibleBubble audibleBubble = null;
						AudioSource sourceAudio = null;
						if (MasterAudio.PlaySound(SoundType, Source.transform, Source.AudibleVolume, ref sourceAudio)) {
								Source.LastSoundType = SoundType;
								audibleBubble = sourceAudio.gameObject.GetOrAdd <AudibleBubble>();
								audibleBubble.StartUsing(Source, ignoreColliders, 0.25f);
						}
				}

				public string [] AudioClipNames {
						get {
								if (audioClipNames == null) {
										List <string> audioClipNameList = new List <string>();
										for (int i = 0; i < AmbientAudioClips.Count; i++) {
												audioClipNameList.Add(AmbientAudioClips[i].Key);
										}
										for (int i = 0; i < Audio.musicPlaylist.Count; i++) {
												if (Audio.musicPlaylist[i].clip != null) {
														audioClipNameList.Add(Audio.musicPlaylist[i].clip.name);
												}
										}
										audioClipNames = audioClipNameList.ToArray();
								}
								return audioClipNames;
						}
				}

				public float MasterAmbientVolume {
						get { return Hydrogen.Core.AudioStackItem.MasterVolume; }
						set {
								Hydrogen.Core.AudioStackItem.MasterVolume = value;
						}
				}

				public float MasterMusicVolume {
						get { return mMasterMusicVolume; }
						set {
								mMasterMusicVolume = value;
								if (Music1.volume != 0f) {
										Music1.volume = mMasterMusicVolume;
								}
								if (Music2.volume != 0f) {
										Music2.volume = mMasterMusicVolume;
								}
						}
				}

				public override void WakeUp()
				{
						Get = this;
						audioClipNames = null;
						if (Application.isPlaying) {
								foreach (AmbientAudioClip clip in AmbientAudioClips) {	//build the lookup table for audio clips
										mAudioClipLookup.Add(clip.Key, clip.Clip);
								}
						}
						mWaitMusic = new WaitForSeconds(mUpdateMusicInterval);
						mWaitAmbient = new WaitForSeconds(mUpdateAmbientInterval);
				}

				public override void OnInitialized()
				{
						WorldClock.Get.TimeActions.Subscribe(TimeActionType.DaytimeStart, new ActionListener(DaytimeChange));
						WorldClock.Get.TimeActions.Subscribe(TimeActionType.NightTimeStart, new ActionListener(DaytimeChange));
						Player.Get.AvatarActions.Subscribe(AvatarAction.LocationStructureEnter, new ActionListener(LocationStructureEnterOrExit));
						Player.Get.AvatarActions.Subscribe(AvatarAction.LocationStructureExit, new ActionListener(LocationStructureEnterOrExit));
						Player.Get.AvatarActions.Subscribe(AvatarAction.LocationUndergroundEnter, new ActionListener(LocationUndergroundEnterOrExit));
						Player.Get.AvatarActions.Subscribe(AvatarAction.LocationUndergroundExit, new ActionListener(LocationUndergroundEnterOrExit));
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalDangerExit, new ActionListener(DangerExit));
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalDangerEnter, new ActionListener(DangerEnter));

						CurrentMusic = MusicType.MainMenu;
						CurrentVolume = MusicVolume.Default;
						//StartCoroutine (PlayMusic (CurrentMusic, CurrentVolume));
				}

				public void Start()
				{
						if (Application.isPlaying) {
								StartCoroutine(PlayMusic(MusicType.MainMenu, MusicVolume.Default));
						}
				}

				public override void OnGameStart()
				{
						GetAmbientAudioSettings();
						AmbientAudio.ChunkSettings = CurrentAudioSettings;
						AmbientAudio.UpdateStackVolumes(Colors.Alpha(Color.black, 0f));
						AmbientAudio.ClearAudio();
						MasterAmbientVolume = 0f;
				}

				public override void OnLocalPlayerSpawn()
				{
						MasterAmbientVolume = Profile.Get.CurrentPreferences.Sound.Ambient;
						UpdateMusicState();
						AmbientAudio.ChunkSettings = CurrentAudioSettings;
						if (!mUpdatingAmbientState) {
								mUpdatingAmbientState = true;
								StartCoroutine(UpdateAmbientStateOverTime());
						}
						if (!mUpdatingMusicState) {
								mUpdatingMusicState = true;
								StartCoroutine(UpdateMusicStateOverTime());
						}
						AmbientAudio.IsDaytime = WorldClock.IsDay;
						AmbientAudio.IsInsideStructure = false;
						AmbientAudio.IsUnderground = Player.Local.Surroundings.IsUnderground;
						AmbientAudio.UpdateStackVolumes(Player.Local.Surroundings.TerrainType);
				}

				public override void OnCutsceneStart()
				{
						if (Cutscene.CurrentCutscene.UseCutsceneMusic) {
								PlayList.Cutscene = Cutscene.CurrentCutscene.CutsceneMusic;
								PlayMusic(MusicType.Cutscene, MusicVolume.Default);
						}
				}

				public override void OnCutsceneFinished()
				{
						UpdateMusicState();
				}

				protected IEnumerator UpdateAmbientStateOverTime()
				{
						while (!GameManager.Is(FGameState.Quitting)) {
								yield return mWaitAmbient;
								UpdateAmbientState();
						}
						yield break;
				}

				protected IEnumerator UpdateMusicStateOverTime()
				{
						while (!GameManager.Is(FGameState.Quitting)) {
								yield return mWaitMusic;
								UpdateMusicState();
						}
						yield break;
				}
				//this is used to manage audio listeners and ensure that only one is active at any time
				public static void ActivateAudioListener(AudioListener newAudioListener)
				{
						if (ManagedAudioListener.CurrentAudioListener != null) {
								ManagedAudioListener.CurrentAudioListener.enabled = false;
						}
						ManagedAudioListener.CurrentAudioListener = newAudioListener;
						ManagedAudioListener.CurrentAudioListener.enabled = true;
				}
				//used to load chunk audio clips
				public AudioClip AmbientClip(string key)
				{
						AudioClip clip = null;
						mAudioClipLookup.TryGetValue(key, out clip);
						return clip;
				}

				protected bool DaytimeChange(double timeStamp)
				{
						if (!GameManager.Is(FGameState.InGame) || !Player.Local.HasSpawned) {
								return true;
						}

						AmbientAudio.IsDaytime = WorldClock.IsDay;
						AmbientAudio.UpdateStackVolumes(Player.Local.Surroundings.TerrainType);
						UpdateMusicState();
						return true;
				}

				protected bool LocationStructureEnterOrExit(double timeStamp)
				{
						if (!GameManager.Is(FGameState.InGame) || !Player.Local.HasSpawned) {
								return true;
						}

						if (Player.Local.Surroundings.IsInsideStructure) {
								AmbientAudio.StructureSettings = Player.Local.Surroundings.LastStructureEntered.State.AmbientAudio;
								AmbientAudio.IsInsideStructure = true;
						} else if (Player.Local.Surroundings.IsOutside) {
								AmbientAudio.IsInsideStructure = false;
								Color terrainType = Player.Local.Surroundings.TerrainType;
								AmbientAudio.UpdateStackVolumes(Player.Local.Surroundings.TerrainType);
						}
						UpdateMusicState();
						return true;
				}

				protected bool DangerEnter(double timeStamp)
				{
						if (!GameManager.Is(FGameState.InGame) || !Player.Local.HasSpawned) {
								return true;
						}

						StartCoroutine(PlayMusic(MusicType.Combat, MusicVolume.Default));
						return true;
				}

				protected bool DangerExit(double timeStamp)
				{
						if (!GameManager.Is(FGameState.InGame) || !Player.Local.HasSpawned) {
								return true;
						}

						UpdateMusicState();
						return true;
				}

				protected bool LocationUndergroundEnterOrExit(double timeStamp)
				{
						if (!GameManager.Is(FGameState.InGame) || !Player.Local.HasSpawned) {
								return true;
						}

						AmbientAudio.IsUnderground = Player.Local.Surroundings.State.IsUnderground;
						Color terrainType = Player.Local.Surroundings.TerrainType;
						AmbientAudio.UpdateStackVolumes(Player.Local.Surroundings.TerrainType);
						UpdateMusicState();
						return true;
				}

				public IEnumerator PlayMusic(MusicType music, MusicVolume volume)
				{
						while (!Manager.IsAwake <Mods>()) {
								yield return null;
						}

						CurrentMusic = music;
						CurrentVolume = volume;

						//figure out which music player needs to fade out
						//and which player needs to can fade in
						if (FadingInAudio == null && FadingOutAudio == null) {
								//this is the first time we've played a track - choose Music1 arbitrarily
								//don't set a fading out track
								FadingInAudio = Music1;
								FadingOutAudio = null;
						} else {
								//we've loaded tracks before
								FadingOutAudio = FadingInAudio;
								//whichever is not in use is the one we want
								if (Music1.isPlaying) {
										FadingInAudio = Music2;
								} else {
										FadingInAudio = Music1;
								}
						}

						//reset fade in audio
						FadingInAudio.Stop();
						FadingInAudio.volume = 0f;
						//if it has a clip destroy it to prevent memory leaks
						if (FadingInAudio.clip != null) {
								GameObject.Destroy(FadingInAudio.clip);
								FadingInAudio.clip = null;
						}

						//load the new clip
						string fileName = PlayList.GetByType(music);
						FadeInAudioTargetVolume = 1.0f;
						if (volume == MusicVolume.Quiet) {
								FadeInAudioTargetVolume = 0.5f;
						}

						string fullPath = Mods.Get.Runtime.FullPath(fileName, "Music", Frontiers.Data.GameData.IO.gAudioExtension);
						if (System.IO.File.Exists(fullPath)) {
								Debug.Log(fullPath);
								loader = new WWW("file:///" + System.Uri.EscapeDataString(fullPath));
								//Debug.Log("AUDIO MANAGER: loading " + fullPath);
								FadingInAudio.clip = loader.GetAudioClip(false, true, AudioType.OGGVORBIS);
								if (!string.IsNullOrEmpty(loader.error)) {
										//Debug.Log("AUDIO MANAGER: ERROR when loading audio clip: " + loader.error);
										mUpdatingMusicState = false;
										yield break;
								}
								FadingInAudio.clip.name = fileName;//convenience
						} else {
								//Debug.Log("AUDIO MANAGER: Couldn't find music " + fullPath);
								mUpdatingMusicState = false;
								yield break;
						}

						while (!loader.isDone) {
								//Debug.Log("AUDIO MANAGER: Fading in audio clip " + fullPath + " is NOT ready to play yet");
								yield return null;
								if (loader == null) {
										Debug.Log("Loader was NULL in audio manager");
										yield break;
								}
						}
						//Debug.Log("AUDIO MANAGER: Playing audio clip " + fullPath);
						FadingInAudio.Play();

						bool doneFadingIn = false;
						bool doneFadingOut = false;
						while (!doneFadingIn || !doneFadingOut) {
								//fade in new audio over time
								if (!GameManager.Is(FGameState.GameLoading)) {
										FadingInAudio.volume = Mathf.Lerp(FadingInAudio.volume, FadeInAudioTargetVolume, (float)(WorldClock.RTDeltaTimeSmooth * Globals.MusicCrossfadeSpeed));
										//fade out existing audio over time
										if (FadingOutAudio != null) {
												FadingOutAudio.volume = Mathf.Lerp(FadingOutAudio.volume, 0f, (float)(WorldClock.RTDeltaTimeSmooth * Globals.MusicCrossfadeSpeed));
										}
										doneFadingIn = Mathf.Approximately(FadingInAudio.volume, FadeInAudioTargetVolume);
										doneFadingOut = (FadingOutAudio == null) || Mathf.Approximately(FadingOutAudio.volume, 0f);
								}
								yield return null;
						}
						//before we leave set everything one last time
						//and destroy the fading out clip
						FadingInAudio.volume = FadeInAudioTargetVolume;
						if (FadingOutAudio != null) {
								FadingOutAudio.Stop();
								FadingOutAudio.volume = 0f;
								if (FadingOutAudio.clip != null) {
										GameObject.Destroy(FadingOutAudio.clip);
										FadingOutAudio.clip = null;
								}
						}
						FadingOutAudio = null;
						//also kill the loader since we don't need it any more
						if (loader != null) {
								loader.Dispose();
						}
						loader = null;
						mUpdatingMusicState = false;
						yield break;
				}

				public AudioSource CreateLocalAmbientAudioSource()
				{
						//TODO for creating local audio sources saved in chunks
						return null;
				}

				public void UpdateAmbientState()
				{
						if (!GameManager.Is(FGameState.InGame | FGameState.Cutscene) || !GameWorld.Get.WorldLoaded) {
								return;
						}

						if (!Player.Local.HasSpawned) {
								AmbientAudioManager.TerrainTypeVolume = 0.001f;
								mCurrentTerrainType = Colors.Alpha(Color.black, 0f);
								if (!Mathf.Approximately(mCurrentTerrainType.a, mLastTerrainType.a) ||
								    !Mathf.Approximately(mCurrentTerrainType.r, mLastTerrainType.r) ||
								    !Mathf.Approximately(mCurrentTerrainType.g, mLastTerrainType.g) ||
								    !Mathf.Approximately(mCurrentTerrainType.b, mLastTerrainType.b)) {
										AmbientAudio.UpdateStackVolumes(mCurrentTerrainType);
										mLastTerrainType = mCurrentTerrainType;
								}
								return;
						}

						if (GameManager.Is(FGameState.Cutscene)) {
								mCurrentTerrainType = Cutscene.CurrentCutscene.TerrainColor;
								AmbientAudio.WindIntensity = Cutscene.CurrentCutscene.WindIntensity;
								AmbientAudio.ThunderIntensity = Cutscene.CurrentCutscene.ThunderIntensity;
								AmbientAudio.RainIntensity = Cutscene.CurrentCutscene.RainIntensity;
								AmbientAudioManager.TerrainTypeVolume = 1f;
						} else {
								if (Player.Local.Status.IsStateActive("Airborne") || Player.Local.Status.IsStateActive("Traveling")) {
										AmbientAudioManager.TerrainTypeVolume = 0.001f;
										AmbientAudio.WindIntensity = 1.0f;
								} else {
										AmbientAudioManager.TerrainTypeVolume = 1.0f;
										AmbientAudio.WindIntensity = Biomes.Get.WindIntensity;
								}
								GetAmbientAudioSettings();
								if (Player.Local.Surroundings.IsUnderground) {
										AmbientAudio.ThunderIntensity = 0f;
										AmbientAudio.RainIntensity = 0f;
								} else {
										AmbientAudio.ThunderIntensity = Biomes.Get.ThunderIntensity;
										AmbientAudio.RainIntensity = Biomes.Get.RainIntensity;
								}
								mCurrentTerrainType = Player.Local.Surroundings.TerrainType;
						}

						if (!Mathf.Approximately(mCurrentTerrainType.a, mLastTerrainType.a) ||
						    !Mathf.Approximately(mCurrentTerrainType.r, mLastTerrainType.r) ||
						    !Mathf.Approximately(mCurrentTerrainType.g, mLastTerrainType.g) ||
						    !Mathf.Approximately(mCurrentTerrainType.b, mLastTerrainType.b)) {
								AmbientAudio.UpdateStackVolumes(mCurrentTerrainType);
								mLastTerrainType = mCurrentTerrainType;
						}
				}

				protected Color mCurrentTerrainType = Colors.Alpha(Color.black, 0f);
				protected Color mLastTerrainType = Colors.Alpha(Color.black, 0f);

				public void UpdateMusicState()
				{
						if (mUpdatingMusicState) {
								return;
						}

						if (Cutscene.IsActive) {
								return;
						}

						//Debug.Log ("Updating music state");
						MusicType shouldBePlaying = MusicType.Regional;
						MusicVolume	musicTargetVolume = MusicVolume.Quiet;

						PlayList.Regional = GameWorld.Get.CurrentRegion.DayMusic;
						PlayList.Night = GameWorld.Get.CurrentRegion.NightMusic;
						PlayList.Underground = GameWorld.Get.CurrentRegion.UndergroundMusic;
						bool isDaytime = WorldClock.IsDay;
						PlayerStatus status = Player.Local.Status;
						PlayerSurroundings surroundings = Player.Local.Surroundings;
						if (surroundings.IsInDanger) {
								//same day or night
								//Debug.Log ("We're in danger - should be playing combat");
								shouldBePlaying = MusicType.Combat;
						} else if (surroundings.IsInSafeLocation) {
								//same day or night
								shouldBePlaying = MusicType.SafeLocation;
						} else if (surroundings.IsUnderground) {
								//same day or night
								shouldBePlaying = MusicType.Underground;
						} else {
								if (isDaytime) {
										shouldBePlaying = MusicType.Regional;
										if (surroundings.IsInsideStructure) {
												musicTargetVolume = MusicVolume.Quiet;
										} else {
												musicTargetVolume = MusicVolume.Default;
										}
								} else {
										//night
										shouldBePlaying = MusicType.Night;
										if (surroundings.IsInsideStructure) {
												musicTargetVolume = MusicVolume.Quiet;
										} else {
												musicTargetVolume = MusicVolume.Default;
										}
								}
						}

						bool updateMusic = false;

						if (shouldBePlaying == MusicType.Regional && mPreviousRegionalTrack != PlayList.Regional) {
								//Debug.Log ("Switching regional music to " + PlayList.Regional);
								mPreviousRegionalTrack = PlayList.Regional;
								updateMusic = true;
						} else if (shouldBePlaying == MusicType.Night && mPreviousNightTrack != PlayList.Night) {
								//Debug.Log ("Switching night music to " + PlayList.Night);
								mPreviousNightTrack = PlayList.Night;
								updateMusic = true;
						} else if (shouldBePlaying != CurrentMusic) {
								updateMusic = true;
						}

						if (updateMusic) {
								//Debug.Log ("Switching music to " + shouldBePlaying.ToString ( ));
								mUpdatingMusicState = true;
								StartCoroutine(PlayMusic(shouldBePlaying, musicTargetVolume));
						} else {
								mUpdatingMusicState = false;
						}
				}

				protected float mLastUpdatedMusicTime = 0f;
				protected string mPreviousRegionalTrack = string.Empty;
				protected string mPreviousNightTrack = string.Empty;

				protected void GetAmbientAudioSettings()
				{
						AmbientAudioManager.ChunkAudioSettings defaults = GameWorld.Get.Settings.DefaultAmbientAudio;
						AmbientAudioManager.ChunkAudioSettings current = GameWorld.Get.CurrentAudioProfile.AmbientAudio;
						if (current != null) {
								CurrentAudioSettings.AgDayCivilized = current.AgDayCivilized.UseDefault ? defaults.AgDayCivilized : current.AgDayCivilized;
								CurrentAudioSettings.AgDayCoastal = current.AgDayCoastal.UseDefault ? defaults.AgDayCoastal : current.AgDayCoastal;
								CurrentAudioSettings.AgDayForest = current.AgDayForest.UseDefault ? defaults.AgDayForest : current.AgDayForest;
								CurrentAudioSettings.AgDayOpen = current.AgDayOpen.UseDefault ? defaults.AgDayOpen : current.AgDayOpen;
								CurrentAudioSettings.AgNightCivilized = current.AgNightCivilized.UseDefault ? defaults.AgNightCivilized : current.AgNightCivilized;
								CurrentAudioSettings.AgNightCoastal = current.AgNightCoastal.UseDefault ? defaults.AgNightCoastal : current.AgNightCoastal;
								CurrentAudioSettings.AgNightForested = current.AgNightForested.UseDefault ? defaults.AgNightForested : current.AgNightForested;
								CurrentAudioSettings.AgNightOpen = current.AgNightOpen.UseDefault ? defaults.AgNightOpen : current.AgNightOpen;
								CurrentAudioSettings.Wind = current.Wind.UseDefault ? defaults.Wind : current.Wind;
								CurrentAudioSettings.Rain = current.Rain.UseDefault ? defaults.Rain : current.Rain;
								CurrentAudioSettings.Thunder = current.Thunder.UseDefault ? defaults.Thunder : current.Thunder;
								CurrentAudioSettings.UgDeep = current.UgDeep.UseDefault ? defaults.UgDeep : current.UgDeep;
								CurrentAudioSettings.UgEnclosed = current.UgEnclosed.UseDefault ? defaults.UgEnclosed : current.UgEnclosed;
								CurrentAudioSettings.UgOpen = current.UgOpen.UseDefault ? defaults.UgOpen : current.UgOpen;
								CurrentAudioSettings.UgShallow = current.UgShallow.UseDefault ? defaults.UgShallow : current.UgShallow;
						}
				}

				protected WaitForSeconds mWaitAmbient;
				protected WaitForSeconds mWaitMusic;
				protected float mMasterMusicVolume = 1.0f;
				protected Dictionary <string, AudioClip> mAudioClipLookup = new Dictionary <string, AudioClip>();
				protected bool mUpdatingMusicState = false;
				protected bool mUpdatingAmbientState = false;
				protected float mUpdateMusicInterval = 2.125f;
				protected float mUpdateAmbientInterval = 2.25f;
				protected int mLastChunkID = -1;
				protected string[] audioClipNames = null;
		}

		[Serializable]
		public class AmbientAudioClip
		{
				public string Key;
				public AudioClip Clip;
				public float BaseVolume	= 1.0f;
		}

		[Serializable]
		public class MusicPlaylist
		{
				public string GetByType(MusicType type)
				{
						string result = string.Empty;
						switch (type) {
								case MusicType.Combat:
										result = Combat;
										break;

								case MusicType.Cutscene:
										result = Cutscene;
										break;

								case MusicType.MainMenu:
										result = MainMenu;
										break;

								case MusicType.Night:
										result = Night;
										break;

								case MusicType.Regional:
										result = Regional;
										break;

								case MusicType.SafeLocation:
										result = SafeLocation;
										break;

								case MusicType.Underground:
										result = Underground;
										break;

								default:
										break;
						}
						return result;
				}

				[FrontiersAvailableModsAttribute("Music")]
				public string MainMenu;
				[FrontiersAvailableModsAttribute("Music")]
				public string Cutscene;
				[FrontiersAvailableModsAttribute("Music")]
				public string Regional;
				[FrontiersAvailableModsAttribute("Music")]
				public string Night;
				[FrontiersAvailableModsAttribute("Music")]
				public string Underground;
				[FrontiersAvailableModsAttribute("Music")]
				public string SafeLocation;
				[FrontiersAvailableModsAttribute("Music")]
				public string Combat;
		}
}