using UnityEngine;
using System.Collections;
using Frontiers;
using System;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
	public class ObexTransmitter : WIScript, IConversationIntermediary
	{
		public static HashSet <ObexTransmitter> VisibleTransmitters = new HashSet <ObexTransmitter> ();
		public static ObexTransmitter ActiveTransmitter;

		public Character SpeakingCharacter {
			get {
				return mSpeakingCharacter;
			}
		}

		public Transform CameraTarget {
			get {
				return mCameraTarget;
			}
		}

		public MeshRenderer[] Renderers;
		public Transform[] RenderTargets;
		public Camera TargetCamera;
		public Transform WigglyBit;
		public Transform CharacterRoom;
		public Transform CharacterRoomCameraTarget;
		public Transform CharacterRoomCharacterTarget;
		public Transform LuminiteGemPivot;
		public Vector3 WigglyBitRotation;
		public LuminitePowered PowerSource;
		public Light ActiveLight;
		public ObexTransmitterState State = new ObexTransmitterState ();
		public Material TransmitterMaterial;
		protected Color mTargetColor;
		protected Color mCurrentColor;

		protected Character mSpeakingCharacter;
		protected Transform mCameraTarget;
		protected ActionNode mCharacterSpawnPoint;

		public override void OnInitialized ()
		{
			if (VisibleTransmitters.Count == 0 || ActiveTransmitter == null) {
				TransmitterMaterial.SetColor ("_RimColor", Color.clear);
			}

			Debug.Log ("On initialized");
			worlditem.OnPlayerUse += OnPlayerUse;
			worlditem.OnPlayerEncounter += OnPlayerEncounter;

			PowerSource = worlditem.GetOrAdd <LuminitePowered> ();
			PowerSource.OnLosePower += OnLosePower;
			PowerSource.OnRestorePower += OnRestorePower;
			PowerSource.OnPowerSourceRemoved += OnPowerSourceRemoved;
			PowerSource.PowerSourceDopplegangerProps.CopyFrom (Orb.OrbGemGenericWorldItem);
			PowerSource.FXOnRestorePower = "ShieldEffectSubtleGold";
			PowerSource.FXOnLosePower = "RipEffect";
			PowerSource.FXOnPowerSourceRemoved = "ShieldEffectSubtleGold";
			PowerSource.PowerSourceDopplegangerParent = LuminiteGemPivot;
			PowerSource.PowerAudio = gameObject.GetComponent <AudioSource> ();
			PowerSource.Refresh ();

			mCameraTarget = worlditem.tr.FindChild ("ConversationCameraTarget");

			DeactivateTransmitter ();
		}

		public void OnLosePower ()
		{
			PowerSource.CanRemoveSource = true;
		}

		public void OnRestorePower ()
		{
			PowerSource.CanRemoveSource = false;
		}

		public void OnPowerSourceRemoved ()
		{

		}

		public void OnActive () {
			VisibleTransmitters.Remove (null);
			VisibleTransmitters.Add (this);
			for (int i = 0; i < Renderers.Length; i++) {
				Renderers [i].enabled = (ActiveTransmitter == this);
			}
		}

		public void OnVisible ()
		{
			VisibleTransmitters.Remove (null);
			VisibleTransmitters.Add (this);
			for (int i = 0; i < Renderers.Length; i++) {
				Renderers [i].enabled = (ActiveTransmitter == this);
			}
		}

		public void OnInvisible ()
		{
			VisibleTransmitters.Remove (null);
			VisibleTransmitters.Remove (this);
			if (ActiveTransmitter == this) {
				ActiveTransmitter = null;
			}
			for (int i = 0; i < Renderers.Length; i++) {
				Renderers [i].enabled = false;
			}
		}

		public void OnPlayerUse ()
		{
			if (PowerSource == null || !PowerSource.HasPower) {
				Frontiers.GUI.GUIManager.PostWarning ("Nothing happens.");
				return;
			}
			Debug.Log ("On player use in obex transmitter");
			ActivateTransmitter ();
			FindNextTarget ();
		}

		public void OnPlayerEncounter ()
		{
			Debug.Log ("On player encounter in obex transmitter");
			ActivateTransmitter ();
		}

		public void ActivateTransmitter () {
			Debug.Log ("Activating transmitter " + name);
			ActiveLight.enabled = true;
			if (ActiveTransmitter != null && ActiveTransmitter != this) {
				ActiveTransmitter.DeactivateTransmitter ();
			}
			for (int i = 0; i < Renderers.Length; i++) {
				Renderers [i].enabled = true;
			}
			if (State.UseCharacterRoom) {
				CharacterRoom.gameObject.SetActive (true);
				State.CharacterRoomPosition.ApplyTo (CharacterRoom, false);
			}
			FindNextTarget ();
			TargetCamera.enabled = true;
			mCurrentColor = Color.clear;
			mTargetColor = Color.white;
			TransmitterMaterial.SetColor ("_RimColor", mTargetColor);
			enabled = true;
		}

		public void FindNextTarget () {
			State.CurrentTarget++;
			if (State.CurrentTarget >= State.Targets.Count) {
				if (State.UseCharacterRoom) {
					State.CurrentTarget = -1;
				} else {
					State.CurrentTarget = 0;
				}
			}
			//if we use the character room (-1) then cycle that
			//otherwise go for the other targets
			if (State.CurrentTarget > 0) {
				ObexTransmitterTarget target = State.Targets [State.CurrentTarget];
			} else {
				//send the camera to the character room
				TargetCamera.transform.position = CharacterRoomCameraTarget.position;
				TargetCamera.transform.rotation = CharacterRoomCameraTarget.rotation;
				//spawn the character
				if (mSpeakingCharacter == null || mSpeakingCharacter.IsDestroyed) {
					if (mCharacterSpawnPoint == null) {
						WorldChunk c = worlditem.Group.GetParentChunk ();
						ActionNodeState actionNodeState = null;
						string nodeName = worlditem.FileName + "TransmitterNode";
						if (!c.GetOrCreateNode (worlditem.Group, worlditem.tr, worlditem.FileName + "TransmitterNode", out actionNodeState)) {
							Debug.LogError ("Couldn't create spawn node " + nodeName + " in transmitter to create character");
							return;
						}
						actionNodeState.actionNode.transform.position = CharacterRoomCharacterTarget.position;
						actionNodeState.CustomConversation = State.CharacterConversation;
						if (!Characters.GetOrSpawnCharacter (actionNodeState.actionNode, State.CharacterName, worlditem.Group, out mSpeakingCharacter)) {
							Debug.LogError ("Couldn't spawn character " + State.CharacterName + " in transmitter");
						} else {
							mSpeakingCharacter.worlditem.ActiveState = WIActiveState.Visible;
							mSpeakingCharacter.worlditem.ActiveStateLocked = true;
							Debug.Log ("Setting character's conversation name to " + State.CharacterConversation);
							//we don't need motile for this character
							Motile m = null;
							if (mSpeakingCharacter.worlditem.Is <Motile> (out m)) {
								m.Finish ();
							}
							mSpeakingCharacter.Body.OnSpawn (mSpeakingCharacter);
						}
					}
				}
				//initiate conversation with the character if it exists
				//TODO make character look in direction of intermediary
				if (mSpeakingCharacter != null) {
					Talkative t = mSpeakingCharacter.worlditem.Get <Talkative> ();
					t.State.ConversationName = State.CharacterConversation;
					t.State.DefaultToDTS = false;
					mSpeakingCharacter.Body.SetVisible (true);
					mSpeakingCharacter.Body.IgnoreCollisions (true);
					mSpeakingCharacter.Body.LockVisible = true;
					t.SpeakThroughIntermediary (this);
				}
			}
		}

		public void FinishConversation () {
			DeactivateTransmitter ();
		}

		public void DeactivateTransmitter () {
			ActiveLight.enabled = false;
			if (ActiveTransmitter == this) {
				ActiveTransmitter = null;
			}
			for (int i = 0; i < Renderers.Length; i++) {
				Renderers [i].enabled = false;
			}
			CharacterRoom.gameObject.SetActive (false);
			if (mSpeakingCharacter != null) {
				mSpeakingCharacter.Body.SetVisible (false);
			}
			TargetCamera.enabled = false;
			enabled = false;
		}

		public void Update ()
		{
			if (PowerSource == null || !PowerSource.HasPower) {
				TransmitterMaterial.SetColor ("_RimColor", Color.clear);
				enabled = false;
				return;
			}
			WigglyBit.Rotate (WigglyBitRotation * Time.deltaTime);
			WigglyBit.localScale = Vector3.one * UnityEngine.Random.Range (0.95f, 1.05f);
			ActiveLight.intensity = UnityEngine.Random.Range (0.8f, 1.2f);
			mCurrentColor = Color.Lerp (mCurrentColor, mTargetColor, (float)(WorldClock.RTDeltaTime * 0.5));
			TransmitterMaterial.SetColor ("_RimColor", mCurrentColor);
		}

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			if (State.UseCharacterRoom) {
				State.CharacterRoomPosition.CopyFrom (CharacterRoom);
			}
		}
		#endif
	}

	[Serializable]
	public class ObexTransmitterState
	{
		public int CurrentTarget = -1;
		public List <ObexTransmitterTarget> Targets = new List <ObexTransmitterTarget> ();
		public bool UseCharacterRoom = true;
		public string CharacterName = string.Empty;
		public string CharacterConversation = string.Empty;
		public STransform CharacterRoomPosition = new STransform ();
	}

	[Serializable]
	public class ObexTransmitterTarget
	{
		TargetType Type = TargetType.Character;
		public string CharacterName;

		public enum TargetType
		{
			Character,
			Location,
			Transmitter,
		}
	}
}