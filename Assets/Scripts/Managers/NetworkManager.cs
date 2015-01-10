using UnityEngine;
using System.IO;
using System;
using TNet;
using System.Collections.Generic;

namespace Frontiers {
	public class NetworkManager : TNBehaviour
	{
			/// <summary>
			/// AutoSync Object List
			/// </summary>
			public System.Collections.Generic.List<TNAutoSync> AutoSyncObjects = new System.Collections.Generic.List<TNAutoSync> ();
			/// <summary>
			/// Default TCP Port
			/// </summary>
			public const int TCPPort = 27015;
			/// <summary>
			/// Default UDP Port
			/// </summary>
			public const int UDPPort = 27016;
			/// <summary>
			/// Default LAN Port
			/// </summary>
			public const int LANPort = 27017;
			/// <summary>
			/// Maximum Number of Players
			/// </summary>
			public const int MaxPlayers = 4;
			public const float WorldBodyUpdateRate = 0.1f;
			public const float BodyAnimatorUpdateRate = 0.9f;
			static readonly object _syncRoot = new UnityEngine.Object ();
			static volatile NetworkManager _staticInstance;
			bool _checkAutoSyncObjects;

			public static NetworkManager Instance {
					get {
							if (_staticInstance == null) {				
									lock (_syncRoot) {
											_staticInstance = FindObjectOfType (typeof(NetworkManager)) as NetworkManager;
									}
							}
							return _staticInstance;
					}
			}

			/// <summary>
			/// Is the game connected to a server?
			/// </summary>
			/// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
			public bool IsConnected { 
					get {
							return TNManager.isConnected;
					}
			}

			/// <summary>
			/// Is this client the host of a multiplayer game?
			/// </summary>
			/// <value><c>true</c> if this client is the host; otherwise, <c>false</c>.</value>
			public bool IsHost { 
					get { 
							if (TNServerInstance.game != null)
									return true;
							return false;
							//return TNManager.isHosting;
					}
			}

			public int ServerAgentCount {
					get { return TNManager.players.size; }
			}

			public void Awake ()
			{
					//QualitySettings.SetQualityLevel (3);
					//hDebug.Initialize ();
			}

			#region RPC

			public void RPC (string functionName, Target target, params object[] objects)
			{
					Debug.Log ("Calling " + functionName);
					tno.Send (functionName, target, objects);
			}

			public void RPC (string functionName, TNet.Player player, params object[] objects)
			{
					Debug.Log ("Calling " + functionName);
					tno.Send (functionName, player, objects);
			}

			public void RPC_UDP (string functionName, Target target, params object[] objects)
			{
					Debug.Log ("Calling " + functionName);
					tno.SendQuickly (functionName, target, objects);
			}

			public void RPC_UDP (string functionName, TNet.Player player, params object[] objects)
			{
					Debug.Log ("Calling " + functionName);
					tno.SendQuickly (functionName, player, objects);
			}

			#endregion

			#region Server Control

			public void AgentConnect (ConnectionString connection)
			{	
					// Disconnect from current server
					if (IsConnected)
							TNManager.Disconnect ();
							
					TNManager.Connect (connection.IP, connection.Port);
			}

			public void AgentDisconnect ()
			{
					if (IsConnected) {
							SendDestroyRemotePlayer ("Some Data");
							TNManager.Disconnect ();
					}
			}

			public bool ServerStart (string serverName)
			{
					Debug.Log ("Server start " + serverName);
					// Check if we already have a local server running
					if (TNServerInstance.isActive)
							return false;

					// Don't want you being connected to a server and making your own
					if (IsConnected)
							TNManager.Disconnect ();

					// Someone order a name?
					TNServerInstance.serverName = serverName;

					// Start it ... or not ... we'll let it decide.
					TNServerInstance.Start (TCPPort, UDPPort, "", LANPort);

					// Connect to server I created
					AgentConnect (new ConnectionString ("127.0.0.1", TCPPort));

					return true;
			}

			/// <summary>
			/// Stops the Server Instance.
			/// </summary>
			public void ServerStop ()
			{

					// Better disconnect just in case.
					TNManager.Disconnect ();

					// Stop the server.
					TNServerInstance.Stop ();
			}

			#endregion

			#region Request Setup

			System.Collections.Generic.Dictionary<int, TNObject> _requests = new System.Collections.Generic.Dictionary<int, TNObject> ();
			int _requestsCount = 0;

			public void RequestData (TNObject callOrigin, Target target, FrontiersData action)
			{
					_requests.Add (_requestsCount, callOrigin);

					// Tells us who wants it back
					action.ResponseTargetID = _requestsCount;
					action.ResponsePlayerID = GetMyDetails ().id;
					_requestsCount++;

					tno.Send ("GetData", target, action);
			}

			[RFC]
			public void GetData (string stringDataset)
			{

					// This is where you'd have to do something to call out specific objects?

					FrontiersData dataset = NetworkManager.FrontiersData.FromString (stringDataset);

					var responseData = new FrontiersData ();

					switch (dataset.Data) {
					case "GetColor":
							responseData.Data = "Blue";
							break;
					}


					responseData.ResponsePlayerID = dataset.ResponsePlayerID;
					responseData.ResponseTargetID = dataset.ResponseTargetID;

					// I dont like this, cause it means its not direct to the player
					if (dataset.ResponsePlayerID != 0) {
							tno.Send ("ResponseData", Target.Others, responseData.ToString ());
					}

			}

			[RFC]
			public void ResponseData (string stringDataset)
			{
					// Create a more usable version of things
					FrontiersData dataset = NetworkManager.FrontiersData.FromString (stringDataset);

					if (dataset.ResponsePlayerID == GetMyDetails ().id) {
							// This was meant for you

							// Use reference?
							// Do something
							//Debug.Log (dataset.Data);


							// Remove reference
							_requests.Remove (dataset.ResponseTargetID);
					}
			}

			#endregion

			#region Remote Function Calls

			/// <summary>
			/// Called to create a remote copy of yourself on everyone elses computers (and future joiners)
			/// </summary>
			/// <param name="playerData">Player data.</param>
			public void SendCreateRemotePlayer (string playerData)
			{
					tno.Send ("CreateRemotePlayer", Target.OthersSaved, playerData);
			}

			[RFC]
			public void CreateRemotePlayer (string playerData)
			{
					// do something with the data
			}

			/// <summary>
			/// Called when you are quitting, sending your player data out to be removed from the game world.
			/// </summary>
			/// <param name="playerData">Player data.</param>
			public void SendDestroyRemotePlayer (string playerData)
			{
					tno.Send ("DestroyRemotePlayer", Target.Others, playerData);
			}

			[RFC]
			public void DestroyRemotePlayer (string playerData)
			{
					// You would probably destroy the equavlent player here, and assign anything they owned as objects to the server
			}

			/// <summary>
			/// Send a raw dataset around to all other connected players in the channel.
			/// </summary>
			/// <param name="dataset">Dataset.</param>
			public void SendSerializedData (FrontiersData dataset)
			{

					tno.Send ("ReceiveSerializedData", Target.Others, dataset.ToString ());
			}

			[RFC]
			public void ReceiveSerializedData (string stringDataset)
			{
					FrontiersData dataset = NetworkManager.FrontiersData.FromString (stringDataset);
					//Debug.Log ("Received Data:" + dataset.Data);
					if (dataset.Type == 0) {
							// do something with dataset.data
							// maybe its a type that needs to spawn based on the data.
					}
			}

			[RFC]
			public void EchoPacket (string data)
			{
					Debug.Log (data);
			}

			[RFC]
			public void Ping ()
			{
					Debug.Log ("Pong");
			}

			#endregion

			#region Networking Helpers

			public object GetPlayerData (int playerID)
			{
					return TNManager.GetPlayer (playerID).data;
			}

			public string GetPlayerName (int playerID)
			{
					return TNManager.GetPlayer (playerID).name;
			}

			public TNet.Player GetPlayerDetails (int playerID)
			{
					return TNManager.GetPlayer (playerID);
			}

			public int PlayerID {
					get { return TNManager.playerID; }
			}

			public void SetMyDetails (string name, object data)
			{
					TNManager.player.name = name;
					TNManager.player.data = data;
			}

			public TNet.Player GetMyDetails ()
			{
					return TNManager.GetPlayer (TNManager.playerID);
			}

			#endregion

			#region TNet Events

			/// <summary>
			/// Triggered when there is an error.
			/// </summary>
			/// <param name="error">Error.</param>
			public void OnNetworkError (string error)
			{
					//Debug.Log ("NetworkError:" + error);
			}

			/// <summary>
			/// Called with the connection result, on both success and failure. If failed, ‘message’ contains the error message.
			/// </summary>
			/// <param name="success">If set to <c>true</c> success.</param>
			/// <param name="message">Message.</param>
			public void OnNetworkConnect (bool success, string message)
			{
					if (success) {
							//Debug.Log ("Joining Channel");
							TNManager.JoinChannel (0, null, false, 4, "");
							// The "" is the password


					} else {
							//Debug.Log ("Failed To Connect");
					}
			}

			/// <summary>
			/// Called when the network is disconnected
			/// </summary>
			public void OnNetworkDisconnect ()
			{
					DeActivateAutoSyncObjects ();
			}

			/// <summary>
			/// Just like connecting to a server, but this time containing the result of an attempt to join a channel. 
			/// Reason for failure can mean a wrong password or a closed channel, for example — and is explained by the “message” parameter.
			/// </summary>
			/// <param name="success">If set to <c>true</c> success.</param>
			/// <param name="message">Message.</param>
			public void OnNetworkJoinChannel (bool success, string message)
			{
					Debug.Log ("Joined Channel");
					if (success) {

							// Time to activate any deactivated autosyncs (im choosing to activate in here)
							ActivateAutoSyncObjects ();

							//Debug.Log ("Creating remote player");
							//tno.Send ("CreateRemotePlayer", Target.OthersSaved, "PLAYERDATATOBEUSED");
					} else {
							DeActivateAutoSyncObjects ();
					}

			}

			/// <summary>
			/// Happens when another player joins the channel the player is in.
			/// </summary>
			/// <param name="newPlayer">New player.</param>
			public void OnNetworkPlayerJoin (TNet.Player newPlayer)
			{
					//  Bec
					// Create Player -> CreateNetworkPlayer()
				Debug.Log (newPlayer.name);
			}

			/// <summary>
			/// Notification sent when the player leaves the channel for any reason. Also sent just prior to the disconnect notification for consistency’s sake.
			/// </summary>
			public void OnNetworkLeaveChannel ()
			{
					DeActivateAutoSyncObjects ();
					// Destroy me locally?
			}

			/// <summary>
			/// Sent when some player leaves the channel the player is in.
			/// </summary>
			/// <param name="player">Player.</param>
			public void OnNetworkPlayerLeave (TNet.Player player)
			{
					// This could be where you would destroy their avatar, or you could use the DestroyRemotePlayer approach.
			}

			/// <summary>
			/// Notification of some player changing their name (including the current player).
			/// </summary>
			public void OnNetworkPlayerRenamed ()
			{

			}

			#endregion

			#region Auto Sync Fixer

			public void ActivateAutoSyncObjects ()
			{
					foreach (TNAutoSync a in AutoSyncObjects) {
							Debug.Log ("Enabling autosync object " + a.name);
							a.enabled = true;
							a.ShouldSync ();
					}
			}

			public void DeActivateAutoSyncObjects ()
			{
					foreach (TNAutoSync a in AutoSyncObjects) {
							a.TurnOffSync ();
							a.enabled = false;

					}
			}

			#endregion

			public class ConnectionString
			{
					public string IP;
					public int Port;
					public string Password;

					public ConnectionString (string ip, int port)
					{
							//Debug.Log ("Creating new Connection Information ... " + ip + ":" + port);
							IP = ip;
							Port = port;
					}
			}

			public static char[] SerializationSeperator = new char[]{ '.' };

			[System.Serializable]
			public class FrontiersData
			{
					public int Type = 0;
					public string Data = "";
					public int ResponseTargetID = 0;
					public int ResponsePlayerID = 0;

					public string ToString ()
					{
							return Type + "." + ResponsePlayerID + "." + ResponseTargetID + "." + Data;
					}

					public static FrontiersData FromString (string data)
					{
							var newData = data.Split (SerializationSeperator, 4);
							var newFrontiersData = new NetworkManager.FrontiersData ();
							newFrontiersData.Type = int.Parse (newData [0]);
							newFrontiersData.ResponsePlayerID = int.Parse (newData [1]);
							newFrontiersData.ResponseTargetID = int.Parse (newData [2]);
							newFrontiersData.Data = newData [3];

							return newFrontiersData;
					}
			}
	}
}