using UnityEngine;
using System.Collections;

namespace Frontiers {
	public class ManagedAudioListener : MonoBehaviour {
		
		public static AudioListener	CurrentAudioListener;
		
		public AudioListener 		Listener;
		public AudioManager 		Manager;
		
		void Awake ( )
		{
			Listener = GetComponent<Camera>().GetComponent <AudioListener> ( );
			
			if (CurrentAudioListener != null)
			{
				CurrentAudioListener.enabled = false;
			}
			CurrentAudioListener 			= Listener;
			CurrentAudioListener.enabled 	= true;
		}
	}
}