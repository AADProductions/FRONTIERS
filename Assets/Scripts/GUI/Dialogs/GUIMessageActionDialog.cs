using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class GUIMessageActionDialog : GUIEditor <MessageActionDialogResult>
		{
				public UILabel MessageLabel;
				public Action OnDidAction1;
				public Action OnDidAction2;
				public GameObject MiniActionPrefab;
				public GameObject MiniActionParent1;
				public GameObject MiniActionParent2;
				public GUIHudMiniAction MiniAction1;
				public GUIHudMiniAction MiniAction2;

				public override void WakeUp()
				{

				}

				public override bool ActionCancel(double timeStamp)
				{
						//Debug.Log ("Action cancel in " + name);
						mEditObject.Cancelled = true;
						return base.ActionCancel(timeStamp);
				}

				public bool Action1(double timeStamp)
				{
						if (mFinished)
								return true;

						mEditObject.DidAction1 = true;
						OnDidAction1.SafeInvoke();

						if (mEditObject.CloseOnAction1) {
								Finish();
						}
						return true;
				}

				public bool Action2(double timeStamp)
				{
						if (mFinished)
								return true;

						mEditObject.DidAction2 = true;
						OnDidAction2.SafeInvoke();

						if (mEditObject.CloseOnAction2) {
								Finish();
						}
						return true;
				}

				public override void PushEditObjectToNGUIObject()
				{
						UserActions.Behavior = PassThroughBehavior.PassThrough;
						Behavior = PassThroughBehavior.PassThrough;

						//subscribe to the actions
						if (mEditObject.Prompt1.IsUserAction) {
								//UserActions.Behavior = PassThroughBehavior.InterceptByFilter;
								//UserActions.Filter |= mEditObject.Prompt1.UserAction;
								UserActions.Subscribe(mEditObject.Prompt1.UserAction, new ActionListener(Action1));
						} else if (mEditObject.Prompt1.IsInterfaceAction) {
								//Behavior = PassThroughBehavior.InterceptByFilter;
								//Filter |= mEditObject.Prompt2.InterfaceAction;
								Subscribe(mEditObject.Prompt1.InterfaceAction, Action1);
						}

						if (mEditObject.Prompt2.IsUserAction) {
								//UserActions.Behavior = PassThroughBehavior.InterceptByFilter;
								//UserActions.Filter |= mEditObject.Prompt1.UserAction;
								UserActions.Subscribe(mEditObject.Prompt2.UserAction, new ActionListener(Action2));
						} else if (mEditObject.Prompt2.IsInterfaceAction) {
								//Behavior = PassThroughBehavior.InterceptByFilter;
								//Filter |= mEditObject.Prompt2.InterfaceAction;
								Subscribe(mEditObject.Prompt2.InterfaceAction, Action2);
						}

						MessageLabel.text = EditObject.Message;

						GUIHud.GUIHudMode mode = GUIHud.GUIHudMode.MouseAndKeyboard;
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								mode = GUIHud.GUIHudMode.Controller;
						}

						//get the bindings for the prompts
						if (EditObject.Prompt1.Visible) {
								EditObject.Prompt1 = GUIHud.GetBindings(EditObject.Prompt1);
								GameObject miniAction1GameObject = NGUITools.AddChild(MiniActionParent1, MiniActionPrefab);
								MiniAction1 = miniAction1GameObject.GetComponent <GUIHudMiniAction>();
								EditObject.Prompt1 = GUIHud.RefreshHudAction(EditObject.Prompt1, MiniAction1, mode);
						}

						if (EditObject.Prompt2.Visible) {
								EditObject.Prompt2 = GUIHud.GetBindings(EditObject.Prompt2);
								GameObject miniAction2GameObject = NGUITools.AddChild(MiniActionParent2, MiniActionPrefab);
								MiniAction2 = miniAction2GameObject.GetComponent <GUIHudMiniAction>();
								EditObject.Prompt2 = GUIHud.RefreshHudAction(EditObject.Prompt2, MiniAction2, mode);
						}

						if (!EditObject.Prompt2.Visible && EditObject.Prompt1.Visible) {
								//if it's only one && not the other
								//center the one action
								if (EditObject.Prompt1.Visible) {
										Vector3 miniActionPosition = MiniActionParent1.transform.localPosition;
										miniActionPosition.x = 0f;
										MiniActionParent1.transform.localPosition = miniActionPosition;
								} else {
										Vector3 miniActionPosition = MiniActionParent2.transform.localPosition;
										miniActionPosition.x = 0f;
										MiniActionParent2.transform.localPosition = miniActionPosition;
								}
						}
				}
		}

		[Serializable]
		public class MessageActionDialogResult : GenericDialogResult
		{
				public string Message;
				public bool DidAction1;
				public bool DidAction2;
				public bool CloseOnAction1 = true;
				public bool CloseOnAction2 = true;
				public GUIHud.HudPrompt Prompt1;
				public GUIHud.HudPrompt Prompt2;
		}
}