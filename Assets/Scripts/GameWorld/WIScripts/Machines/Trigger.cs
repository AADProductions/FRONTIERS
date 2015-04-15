using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
	public class Trigger : WIScript
	{
		//used to activate dynamic objects
		//can be intercepted by locks and other scripts
		//can also be used to verify activating something
		public bool RemoteTrigger = false;
		public Action <Trigger> OnTriggerTryToStart;
		public Action <Trigger> OnTriggerFail;
		public Action <Trigger> OnTriggerStart;
		public Action <Trigger> OnTriggerCancel;
		public Action <Trigger> OnTriggerComplete;
		public Action <Trigger> OnTriggerReset;
		public string ActionDescription = "Use";
		public bool LastTriggerFailed = false;
		public List <Dynamic> TriggerFailureSources = new List <Dynamic>();
		public List <string> TriggerFailureMessages = new List <string>();
		public Animation AnimationTarget;

		public override bool CanBeCarried {
			get {
				return false;
			}
		}

		public override bool CanEnterInventory {
			get {
				return false;
			}
		}

		public override bool AutoIncrementFileName {
			get {
				return false;
			}
		}

		public override string GenerateUniqueFileName(int increment)
		{
			#if UNITY_EDITOR
			if (worlditem == null) {
				mWorldItem = gameObject.GetComponent <WorldItem>();
			}
			#endif

			if (RemoteTrigger) {
				return worlditem.Props.Name.FileName;
			} else {
				return worlditem.Props.Global.FileNameBase + "-" + increment.ToString();
			}
		}

		public TriggerState State = new TriggerState();

		public override int OnRefreshHud(int lastHudPriority)
		{
			lastHudPriority++;
			GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Use", worlditem.HudTarget, GameManager.Get.GameCamera);
			return lastHudPriority;
		}

		public override void OnInitialized()
		{
			worlditem.OnPlayerUse += OnPlayerUse;
			//if we're in a parent strucutre, add ourseles to the structure
			if (RemoteTrigger) {
				IStackOwner owner = null;
				if (worlditem.Group.HasOwner(out owner) && owner.IsWorldItem) {
					Structure structure = null;
					if (owner.worlditem.Is <Structure>(out structure)) {
						structure.AddDynamicTrigger(this);
					}
				}
			}
		}

		public void OnPlayerUse()
		{
			TryToTrigger();
		}

		public void OnFinishConfirm(YesNoCancelDialogResult editObject, IGUIChildEditor <YesNoCancelDialogResult> childEditor)
		{
			GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
			mWaitingForConfirmation = false;
		}

		public void TryToTrigger()
		{
			if (mWaitingForConfirmation) {
				return;
			}

			TriggerFailureSources.Clear();
			TriggerFailureMessages.Clear();
			LastTriggerFailed = false;

			if (State.Behavior == TriggerBehavior.Once && State.NumTimesTriggered > 0) {
				OnTriggerFail.SaveInvoke(this);
			}

			if (State.Confirm != ConfirmationBehavior.Never) {
				if ((State.Confirm == ConfirmationBehavior.Once && State.NumTimesTriggered == 0)
				    || State.Confirm == ConfirmationBehavior.Always) {
					YesNoCancelDialogResult result = new YesNoCancelDialogResult();
					result.Message = State.ConfirmationMessage;
					result.MessageType = State.ConfirmationTitle;
					result.CancelButton = false;
					GameObject confirmEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIYesNoCancelDialog, false);
					GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(new ChildEditorCallback <YesNoCancelDialogResult>(OnFinishConfirm),
						confirmEditor,
						result);

					mWaitingForConfirmation = true;
					StartCoroutine(WaitForConfirmation(confirmEditor, result));
					return;
				}
			} else {
				TryToStartTrigger();
			}
		}

		protected void TryToStartTrigger()
		{
			OnTriggerTryToStart.SaveInvoke(this);
			//any dynamics that want to fail the trigger will do so now
			if (LastTriggerFailed) {
				OnTriggerFail.SaveInvoke(this);
				for (int i = 0; i < TriggerFailureSources.Count; i++) {
					string reason = TriggerFailureMessages[i];
					if (!string.IsNullOrEmpty(reason)) {
						reason = CleanTriggerMessage(reason, TriggerFailureSources[i].worlditem.DisplayName);
						GUIManager.PostWarning(TriggerFailureMessages[i]);
					}
				}
			} else {
				OnTriggerStart.SaveInvoke(this);
				State.NumTimesTriggered++;
				if (AnimationTarget != null && !string.IsNullOrEmpty(State.AnimationOnTriggerPressed)) {
					AnimationTarget.Play(State.AnimationOnTriggerPressed);
				}
				if (!string.IsNullOrEmpty(State.SoundOnTriggerPressed)) {
					MasterAudio.PlaySound(State.SoundType, worlditem.tr, State.SoundOnTriggerPressed);
				}
			}
		}

		public void TriggerFail()
		{
			LastTriggerFailed = true;
		}

		public void TriggerFail(Dynamic sourceOfFailure)
		{
			LastTriggerFailed = true;
			TriggerFailureSources.Add(sourceOfFailure);
			TriggerFailureMessages.Add(string.Empty);
		}

		public void TriggerFail(Dynamic sourceOfFailure, string reason)
		{
			LastTriggerFailed = true;
			TriggerFailureMessages.Add(reason);
			TriggerFailureSources.Add(sourceOfFailure);
		}

		public string CleanTriggerMessage(string triggerMessage, string cleanItemName)
		{
			triggerMessage = triggerMessage.Replace("[Target]", cleanItemName);
			return triggerMessage;
		}

		protected IEnumerator WaitForConfirmation(GameObject confirmEditor, YesNoCancelDialogResult result)
		{
			while (confirmEditor != null && mWaitingForConfirmation) {
				yield return null;
			}

			if (result.Result == DialogResult.Yes) {
				//this is the ONE case where we call this ourselves
				TryToStartTrigger();
			}

			yield break;
		}

		protected bool mWaitingForConfirmation = false;

		[Serializable]
		public class TriggerState
		{
			public TriggerBehavior Behavior = TriggerBehavior.Toggle;
			public ConfirmationBehavior Confirm = ConfirmationBehavior.Never;
			public string ConfirmationTitle = string.Empty;
			public string ConfirmationMessage = string.Empty;
			public string LastFailureMessage = string.Empty;
			public ButtonStyle Style = ButtonStyle.ReflectStateToggle;
			public int NumTimesTriggered = 0;
			public string AnimationOnTriggerPressed;
			public string SoundOnTriggerPressed;
			public MasterAudio.SoundType SoundType;
		}
	}
}