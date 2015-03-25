using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
	public class SpawnOptionsList : MonoBehaviour, IGUIParentEditor <WIListResult>
	{
		public GameObject NGUIObject {
			get {
				return this.gameObject;
			}
			set {
				return;
			}
		}

		public virtual bool IsAvailable {
			get {
				if (RequireManualEnable) {
					return ManuallyEnabled && (OverrideBaseAvailabilty || Options.Count > 0);
				} else {
					return (OverrideBaseAvailabilty || Options.Count > 0);
				}
			}
		}

		public Transform ScreenTarget {
			set {
				if (IsInUse) {
					mChildEditor.ScreenTarget = value;
				} else {
					Debug.Log("Not in use, nothing to set");
				}
			}
		}

		public Camera ScreenTargetCamera {
			set {
				if (IsInUse) {
					mChildEditor.ScreenTargetCamera = value;
				} else {
					Debug.Log("Not in use, nothing to set");
				}
			}
		}

		public string FunctionName = "OnMakeSelection";
		public string SecondaryFunctionName = "OnMakeSecondarySelection";
		public GameObject FunctionTarget;
		public Vector3 PositionTarget;
		public string MessageType = "USE ITEM";
		public string Message = string.Empty;
		//"What do you want to do with this item?";
		public List <WIListOption> Options = new List <WIListOption>();
		public bool OverrideBaseAvailabilty = false;
		public bool PostMessageOnConditionsMet	= false;
		public string PostedMessage = string.Empty;
		public bool RequirePlayerFocus = false;
		public bool PauseWhileOpen = false;
		public bool RequirePlayerTrigger = false;
		public bool RequireManualEnable = false;
		public bool MessageTypeFromTargetName = false;
		public bool ShowDoppleganger = false;
		public bool ForceChoice = false;

		public bool IsInUse {
			get {
				return mChildEditor != null;
			}
		}

		protected virtual bool PlayerFocus {
			get {
				return mPlayerFocus;
			}
			set {
				mPlayerFocus = value;
			}
		}

		protected bool PlayerTrigger = false;
		protected bool ManuallyEnabled = false;
		protected bool ConditionsMet = false;
		protected bool mPlayerFocus = false;

		public void AddOption(string option)
		{
			AddOption(new WIListOption(option));
		}

		public virtual void AddOption(WIListOption option)
		{
			if (WIListOption.IsNullOrInvalid(option)) {
				return;
			}

			if (!ContainsOption(option.Result)) {
				Options.Add(option);
				EnableOptionsList();
				if (PlayerFocus) {
					GUIManager.PostInfo(PostedMessage);
				}
			}
		}

		public virtual void RemoveOption(string optionResult)
		{
			WIListOption optionToRemove = null;
			foreach (WIListOption option in Options) {
				if (option.Result == optionResult) {
					optionToRemove = option;
					break;
				}
			}
			if (optionToRemove != null) {
				Options.Remove(optionToRemove);
			}
		}

		public virtual bool ContainsOption(string optionResult)
		{
			foreach (WIListOption option in Options) {
				if (option.Result == optionResult) {
					return true;
				}
			}
			return false;
		}

		public void ClearOptions()
		{
			Options.Clear();
			DisableOptionsList();
		}

		public void CheckIfConditionsAreMet()
		{
			if (RequirePlayerFocus) {
				if (!PlayerFocus) {
					//				//Debug.Log ("No player focus");
					ConditionsMet = false;
					return;
				}
			}

			if (RequirePlayerTrigger) {
				if (!PlayerTrigger) {
					//				//Debug.Log ("No player trigger");
					ConditionsMet = false;
					return;
				}
			}

			if (RequireManualEnable) {
				if (!ManuallyEnabled) {
					//				//Debug.Log ("Not manually enabled");
					ConditionsMet = false;
					return;
				}
			}

			ConditionsMet = true;
		}

		public virtual void Awake()
		{
			if (FunctionTarget == null) {
				FunctionTarget = this.gameObject;
			}
		}

		public void EnableOptionsList()
		{
			ManuallyEnabled = true;
			CheckIfConditionsAreMet();
		}

		public void DisableOptionsList()
		{
			ManuallyEnabled = false;
			CheckIfConditionsAreMet();
		}

		public virtual bool TryToSpawn(bool forceSpawn, out GUIOptionListDialog childEditor, Camera nguiCamera)
		{
			childEditor = null;
			if (mChildEditor != null) {
				return false;
			}

			childEditor = SpawnOptionsDialog(nguiCamera);
			return true;
		}

		public virtual bool TryToSpawn(Camera nguiCamera)
		{
			if (mChildEditor != null) {
				return false;
			}

			CheckIfConditionsAreMet();
			if (ConditionsMet) {
				SpawnOptionsDialog(nguiCamera);
				return true;
			}
			return false;
		}

		public GUIOptionListDialog SpawnOptionsDialog(Camera nguiCamera)
		{
			WIListResult editObject = new WIListResult();
			if (MessageTypeFromTargetName) {
				editObject.MessageType = FunctionTarget.name;
			} else {
				editObject.MessageType = MessageType;
			}
			PopulateDialogOptions();
			if (string.IsNullOrEmpty(Message)) {
				Message = mMessage.JoinToString("\n");
			}
			editObject.Message = Message;
			editObject.MessageType = MessageType;
			editObject.Options = Options;
			editObject.ForceChoice = ForceChoice;
			editObject.SecondaryOptions	= mOptions;
			//editObject.PositionTarget = PositionTarget;

			GameObject childEditor = GUIManager.SpawnNGUIChildEditor(this.gameObject, GUIManager.Get.NGUIOptionsListDialog, false);
			mChildEditor = childEditor.GetComponent <GUIOptionListDialog>();
			if (nguiCamera != null) {
				Debug.Log("Setting ngui camera in options dialog");
				mChildEditor.NGUICamera = nguiCamera;
			}
			//mChildEditor.Pause = PauseWhileOpen;
			GUIManager.SendEditObjectToChildEditor <WIListResult>(new ChildEditorCallback <WIListResult>(ReceiveFromChildEditor), childEditor, editObject);
			return mChildEditor;
		}

		protected List <WIListOption> mOptions = new List<WIListOption>();
		protected List <string> mMessage = new List<string>();

		public void ReceiveFromChildEditor(WIListResult result, IGUIChildEditor <WIListResult> childEditor)
		{
			if (childEditor == null) {
				return;
			}

			if (FunctionTarget != null) {
				if (!string.IsNullOrEmpty(result.Result)) {
					HandleResult(result, FunctionTarget);
				}
				if (!string.IsNullOrEmpty(result.SecondaryResult)) {
					HandleSecondaryResult(result, FunctionTarget);
				}
			}

			GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);

			mChildEditor = null;
		}

		public virtual void PopulateDialogOptions()
		{

		}

		public virtual void HandleResult(object result, GameObject functionTarget)
		{
			try {
				functionTarget.SendMessage(FunctionName, result, SendMessageOptions.DontRequireReceiver);
			} catch (Exception e) {
				Debug.LogError("Options List: Sending function " + FunctionName + " failed because: " + e.ToString());
			}
		}

		public virtual void HandleSecondaryResult(object secondaryResult, GameObject functionTarget)
		{
			try {
				functionTarget.SendMessage(SecondaryFunctionName, secondaryResult, SendMessageOptions.DontRequireReceiver);
			} catch (Exception e) {
				Debug.LogError("Options List: Sending function " + SecondaryFunctionName + " failed because: " + e.ToString());
			}
		}

		public virtual void OnGainPlayerFocus()
		{
			PlayerFocus = true;
			CheckIfConditionsAreMet();

			if (ConditionsMet && PostMessageOnConditionsMet) {
				GUIManager.PostInfo(PostedMessage);
			}
		}

		public virtual void OnLosePlayerFocus()
		{
			if (mChildEditor != null && RequirePlayerFocus) {
				mChildEditor.Finish();
			}

			PlayerFocus = false;
			CheckIfConditionsAreMet();
		}

		public virtual void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.layer != Globals.LayerNumPlayer) {
				return;
			}

			PlayerTrigger = true;
			CheckIfConditionsAreMet();

			if (ConditionsMet && PostMessageOnConditionsMet) {
				GUIManager.PostInfo(PostedMessage);
			}
		}

		public virtual void OnTriggerExit(Collider other)
		{
			if (other.gameObject.layer != Globals.LayerNumPlayer) {
				return;
			}

			if (mChildEditor != null && RequirePlayerTrigger) {
				GUIManager.ScaleDownEditor(mChildEditor.gameObject).Proceed(true);
			}

			PlayerTrigger = false;
			CheckIfConditionsAreMet();
		}

		protected GUIOptionListDialog mChildEditor;
	}
}