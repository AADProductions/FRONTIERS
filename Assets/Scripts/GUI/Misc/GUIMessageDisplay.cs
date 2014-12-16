using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIMessageDisplay : MonoBehaviour
		{
				public GameObject MessagesParent;
				public GameObject GUIMessagePrefab;

				public void HideImmediately()
				{
						foreach (GUIMessage message in mMessages) {
								if (message != null) {
										NGUITools.Destroy(message.gameObject);
								}
						}
			
						mMessages.Clear();
				}

				public void Start()
				{		
						HideImmediately();
				}

				public void PostMessage(string message, Type messageType)
				{			
						if (StacksWithExistingMessages(message, messageType)) {
								return;
						}
			
						PostMessage(message, messageType, 4.0f);
				}

				public bool StacksWithExistingMessages(string message, Type messageType)
				{
						if (mMessages.Count > 0 && mMessages[0] != null) {
								if (mMessages[0].StacksWith(messageType, message)) {
										return true;
								}
						}
				
						return false;
				}

				public void PostMessage(string message, Type messageType, float fadeLength)
				{
						if (string.IsNullOrEmpty(message)) {
								return;
						}
			
						GameObject newMessageGameObject = NGUITools.AddChild(MessagesParent.gameObject, GUIMessagePrefab);
						GUIMessage newMessage = newMessageGameObject.GetComponent <GUIMessage>();
						newMessage.Initialize(messageType, message);
						mMessages.Insert(0, newMessage);
				}

				public void Update()
				{		
						if (mMessages.Count > 0) {
								ClearOldMessages();
								UpdateMessageOffsets();
						}
				}

				protected void ClearOldMessages()
				{
						for (int i = mMessages.Count - 1; i > 0; i--) {
								if (mMessages[i].ReadyToRemove) {
										NGUITools.Destroy(mMessages[i].gameObject);
										mMessages.RemoveAt(i);
								}
						}
				}

				protected void UpdateMessageOffsets()
				{
						float targetOffset = 0.0f;

						for (int i = 0; i < mMessages.Count; i++) {
								GUIMessage message = mMessages[i];
								targetOffset += message.Height;
								message.TargetOffset = targetOffset;
								message.UpdateFadeAndPosition();
						}
				}

				protected List <GUIMessage> mMessages = new List <GUIMessage>();

				public enum Type
				{
						Info,
						Warning,
						Danger,
						Success
				}
		}
}