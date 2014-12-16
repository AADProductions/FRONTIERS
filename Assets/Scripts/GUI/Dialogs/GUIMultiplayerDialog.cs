using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System.Net;
using System.Net.Sockets;

namespace Frontiers.GUI
{
		public class GUIMultiplayerDialog : GUIEditor <MultiplayerSession>
		{
				public GameObject PingPongButton;
				public GameObject JoinButton;
				public GameObject HostButton;
				public GUITabs Tabs;
				//join inputs
				public UIInput JoinIPAddressInput;
				public UIInput JoinServerNameInput;
				public UIInput JoinPasswordInput;
				public UIInput JoinCustomLANPortInput;
				//host inputs
				public UILabel HostIPAddress;
				public UICheckbox HostUseCustomLANPortCheckbox;
				public UIInput HostCustomLANPortInput;
				public UIInput HostServerNameInput;
				public UICheckbox HostRequirePasswordCheckbox;
				public UIInput HostPasswordInput;
				//feedback
				public UILabel HostErrorMessage;
				public UILabel JoinErrorMessage;

				public override void Start()
				{
						UserActions.Subscribe(UserActionType.ActionCancel, new ActionListener(ActionCancel));
						Tabs.Initialize(this);
						base.Start();
				}

				public bool ActionCancel(double timeStamp)
				{
						EditObject.Cancelled = true;
						OnFinish();
						return true;
				}

				public override void PushEditObjectToNGUIObject()
				{
						mEditObject = new MultiplayerSession();
						//TODO get info from network manager
						//TODO saved server info?
				}

				public void OnClickJoinButton()
				{
						//TODO join multiplayer game
						//TODO : THIS IS JUST TEMP
						if (!mTryingToJoinGame) {
								StartCoroutine(TryToJoinGameOverTime());
						}
				}

				public void OnClickHostButton()
				{
						//TODO host multiplayer game
						// TODO DEFAULT SERVER
						if (NetworkManager.Instance.ServerStart(HostServerNameInput.text)) {
								hDebug.Log("Server Started");
						}
						OnFinish();
				}

				public void OnClickPingPongButton()
				{
						NetworkManager.Instance.Ping();
				}

				public void OnJoinInputsChange()
				{
						EditObject.JoinIPAddress = JoinIPAddressInput.text;
						EditObject.JoinServerName = JoinServerNameInput.text;
						EditObject.JoinPassword = JoinPasswordInput.text;
						int.TryParse(JoinCustomLANPortInput.text, out EditObject.JoinCustomLANPort);

						string errorMessage = string.Empty;
						if (VerifyJoinInputs(mEditObject, out errorMessage)) {
								JoinButton.SendMessage("SetEnabled");
						} else {
								JoinButton.SendMessage("SetDisabled");
						}
						JoinErrorMessage.text = errorMessage;
				}

				public void OnHostInputsChange()
				{
						EditObject.JoinIPAddress = TNet.Tools.localAddress.ToString();
						EditObject.HostServerName = HostServerNameInput.text;
						EditObject.HostRequirePassword = HostRequirePasswordCheckbox.isChecked;
						EditObject.HostPassword = HostPasswordInput.text;
						EditObject.HostUseCustomLANPort = HostUseCustomLANPortCheckbox.isChecked;
						int.TryParse(HostCustomLANPortInput.text, out EditObject.HostCustomLanPort);

						string errorMessage = string.Empty;
						if (VerifyHostInputs(mEditObject, out errorMessage)) {
								HostButton.SendMessage("SetEnabled");
						} else {
								HostButton.SendMessage("SetDisabled");
						}
						HostErrorMessage.text = errorMessage;
						HostIPAddress.text = EditObject.JoinIPAddress;
				}

				public bool VerifyJoinInputs(MultiplayerSession session, out string errorMessage)//TODO move this into network manager
				{
						errorMessage = session.JoinIPAddress + "\n" + session.JoinServerName + "\n" + session.JoinPassword + "\n" + session.JoinCustomLANPort;
						return true;
				}

				public bool VerifyHostInputs(MultiplayerSession session, out string errorMessage)//TODO move this into network manager
				{
						errorMessage = session.JoinIPAddress + "\n" + session.HostServerName + "\n" + session.HostPassword + "\n" + session.HostCustomLanPort;
						return true;
				}

				public override void EnableInput()
				{
						PushEditObjectToNGUIObject();		
						base.EnableInput();
				}

				protected IEnumerator TryToJoinGameOverTime()
				{
						mTryingToJoinGame = true;

						GameObject childEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIMessageCancelDialog, false);
						MessageCancelDialogResult editObject = new MessageCancelDialogResult();
						editObject.CanCancel = true;
						editObject.CancelButton = "Cancel";
						editObject.Message = "Connecting to host...";
						GUIManager.SendEditObjectToChildEditor <MessageCancelDialogResult>(childEditor, editObject);

						NetworkManager.Instance.AgentConnect(
								new NetworkManager.ConnectionString(JoinIPAddressInput.text, NetworkManager.TCPPort));

						while (!editObject.EditorFinished) {
								//check to see if we've connected
								if (!NetworkManager.Instance.IsConnected) {
										editObject.Message = "Connecting to host...";
								} else {
										switch (GameManager.HostState) {
												case NHostState.None:
												case NHostState.WaitingToStart:
												default:
														editObject.Message = "Waiting for game to start";
														editObject.RefreshAction.SafeInvoke();
														break;

												case NHostState.Started:
														childEditor.SendMessage("Finish");
														editObject.Message = "Game starting...";
														//it'll scale down soon
														break;
										}
								}
								yield return null;
						}
						GUIManager.ScaleDownEditor(childEditor).Proceed(true);
						//did we cancel? if so disconnect
						if (editObject.Cancelled) {
								NetworkManager.Instance.AgentDisconnect();
						} else {
								//otherwise we're done here as well
								OnFinish();
						}
						mTryingToJoinGame = false;
						yield break;
				}

				protected bool mTryingToJoinGame = false;
		}

		public class MultiplayerSession : GenericDialogResult
		{
				public string JoinIPAddress;
				public string JoinServerName;
				public string JoinPassword;
				public int JoinCustomLANPort;
				public string HostServerName;
				public bool HostRequirePassword;
				public string HostPassword;
				public bool HostUseCustomLANPort;
				public int HostCustomLanPort;
		}
}