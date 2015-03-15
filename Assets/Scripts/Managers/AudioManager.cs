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
				public MusicType LastMusic;
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

				public AudioSource ActiveMusic {
						get {
								if (Music1.clip != null && Music1.isPlaying && Music1.volume > 0f) {
										return Music1;
								} else if (Music2.clip != null && Music2.isPlaying) {
										return Music2;
								}
								return null;
						}
				}

				public AudioSource Music1;
				public AudioSource Music2;
				public AudioSource FadingInAudio;
				public AudioSource FadingOutAudio;
				//public WWW loader;
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
						base.WakeUp();

						FadingInAudio = null;
						FadingOutAudio = null;
						Get = this;
						audioClipNames = null;
						if (Application.isPlaying) {
								foreach (AmbientAudioClip clip in AmbientAudioClips) {	//build the lookup table for audio clips
										mAudioClipLookup.Add(clip.Key, clip.Clip);
								}
						}
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

				public override void OnLocalPlayerDespawn()
				{
						GetAmbientAudioSettings();
						AmbientAudio.ChunkSettings = CurrentAudioSettings;
						AmbientAudio.UpdateStackVolumes(Colors.Alpha(Color.black, 0f));
						AmbientAudio.ClearAudio();
						MasterAmbientVolume = 0f;
				}

				public override void OnLocalPlayerSpawn()
				{
						GetAmbientAudioSettings();
						AmbientAudio.ChunkSettings = CurrentAudioSettings;
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
								double waitUntil = WorldClock.RealTime + mUpdateAmbientInterval;
								while (WorldClock.RealTime < waitUntil) {
										yield return null;
								}
								UpdateAmbientState();
						}
						yield break;
				}

				protected IEnumerator UpdateMusicStateOverTime()
				{
						while (!GameManager.Is(FGameState.Quitting)) {
								double waitUntil = WorldClock.RealTime + mUpdateMusicInterval;
								while (WorldClock.RealTime < waitUntil) {
										yield return null;
								}
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

						UpdateMusicState();
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

						while (mUpdatingMusicFade) {
								yield return null;
						}

						//see if we're still needed
						if (CurrentMusic != music) {
								//Debug.Log("AUDIOMANAGER: whoops, we got changed in the meantime, no longer trying to play " + music.ToString());
								yield break;
						}
						//start updating!
						mUpdatingMusicFade = true;

						//figure out which clip we're using
						string fileName = PlayList.GetByType(music);

						//this is the audio source we're fading out
						FadingOutAudio = ActiveMusic;
						if (FadingOutAudio == Music2) {
								FadingInAudio = Music1;
						} else {
								FadingInAudio = Music2;
						}
						if (music == MusicType.MainMenu) {
								FadingInAudio.loop = false;
						} else {
								FadingInAudio.loop = true;
						}
						yield return null;

						//load the new clip
						FadeInAudioTargetVolume = 1.0f;
						if (volume == MusicVolume.Quiet) {
								FadeInAudioTargetVolume = 0.5f;
						}

						//reset fade in audio
						FadingInAudio.volume = 0f;
						//if it has a clip destroy it to prevent memory leaks
						if (FadingInAudio.clip != null && FadingInAudio.clip.name != fileName) {
								FadingInAudio.Stop();
								//Debug.Log("Destroying clip " + FadingInAudio.clip.name + " because it doesn't match " + fileName);
								GameObject.Destroy(FadingInAudio.clip);
								FadingInAudio.clip = null;
						}

						if (FadingInAudio.clip == null) {
								string fullPath = Mods.Get.Runtime.FullPath(fileName, "Music", Frontiers.Data.GameData.IO.gAudioExtension, DataType.World);
								if (!System.IO.File.Exists(fullPath)) {
										fullPath = Mods.Get.Runtime.FullPath(fileName, "Music", Frontiers.Data.GameData.IO.gAudioExtension, DataType.Base);
								}
								if (!System.IO.File.Exists(fullPath)) {
										Debug.Log("AUDIO MANAGER: Couldn't find music " + fullPath);
										mUpdatingMusicFade = false;
										yield break;
								}

								WWW loader = null;
								Debug.Log(fullPath);
								loader = new WWW("file:///" + System.Uri.EscapeDataString(fullPath));
								//Debug.Log("AUDIO MANAGER: loading " + fullPath);
								FadingInAudio.clip = loader.GetAudioClip(false, true, AudioType.OGGVORBIS);
								if (!string.IsNullOrEmpty(loader.error)) {
										Debug.Log("AUDIO MANAGER: ERROR when loading audio clip: " + loader.error);
										loader.Dispose();
										//GameObject.Destroy(loader);
										loader = null;
										mUpdatingMusicFade = false;
										yield break;
								}
								FadingInAudio.clip.name = fileName;//convenience

								while (!loader.isDone) {
										if (loader == null) {
												Debug.Log("AUDIO MANAGER: Loader was NULL in audio manager, quitting");
												mUpdatingMusicFade = false;
												yield break;
										}
										double waitUntil = WorldClock.RealTime + 0.1f;
										while (WorldClock.RealTime < waitUntil) {
												yield return null;
										}
										yield return null;
								}
								//also kill the loader since we don't need it any more
								if (loader != null) {
										loader.Dispose();
										//GameObject.Destroy(loader);
										loader = null;
								}
						}
						if (!FadingInAudio.isPlaying) {
								//Debug.Log("AUDIO MANAGER: Playing audio clip " + FadingInAudio.clip.name);
								FadingInAudio.Play();
						}

						bool doneFadingIn = false;
						bool doneFadingOut = false;
						float fadeInTargetAudioVolume = FadeInAudioTargetVolume;
						while (!(doneFadingIn && doneFadingOut)) {
								//fade in new audio over time
								if (!GameManager.Is(FGameState.GameLoading)) {
										float musicCrossFadeSpeed = Globals.MusicCrossfadeSpeed;
										if (CurrentMusic != music) {
												//whoops, we're waiting on something to load, make it fade faster
												musicCrossFadeSpeed *= 5;
										}

										FadingInAudio.volume = Mathf.Clamp01(Mathf.Lerp(FadingInAudio.volume, fadeInTargetAudioVolume, (float)(WorldClock.RTDeltaTimeSmooth * musicCrossFadeSpeed)));
										doneFadingIn = Mathf.Abs(FadingInAudio.volume - fadeInTargetAudioVolume) < 0.01f;
										//fade out existing audio over time
										if (FadingOutAudio != null) {
												FadingOutAudio.volume = Mathf.Clamp01(Mathf.Lerp(FadingOutAudio.volume, 0f, (float)(WorldClock.RTDeltaTimeSmooth * musicCrossFadeSpeed)));
												doneFadingOut = (FadingOutAudio.volume < 0.01f);
										} else {
												doneFadingOut = true;
										}
								}
								yield return null;
						}
						//before we leave set everything one last time
						//and destroy the fading out clip
						FadingInAudio.volume = FadeInAudioTargetVolume;
						if (FadingOutAudio != null) {
								FadingOutAudio.volume = 0f;
						}
						FadingOutAudio = null;
						mUpdatingMusicFade = false;
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
						//Debug.Log ("Updating music state");
						MusicType shouldBePlaying = MusicType.Regional;
						MusicVolume musicTargetVolume = MusicVolume.Quiet;

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
								Debug.Log("AUDIO MANAGER: Trying to switch music to " + shouldBePlaying.ToString());
								StartCoroutine(PlayMusic(shouldBePlaying, musicTargetVolume));
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

				protected float mMasterMusicVolume = 1.0f;
				protected Dictionary <string, AudioClip> mAudioClipLookup = new Dictionary <string, AudioClip>();
				protected bool mUpdatingMusicState = false;
				protected bool mUpdatingMusicFade = false;
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