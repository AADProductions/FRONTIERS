using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class DynamicPower : WIScript
		{
				public AudioClip PowerUpClip;
				public AudioClip PowerDownClip;
				public HashSet <GameObject>	PoweredObjects = new HashSet <GameObject>();

				public bool 				HasPower {
						get {
								return CheckPower();
						}
				}

				public void					Start()
				{
						if (HasPower) {
								mHadPowerLastFrame = true;
								foreach (GameObject poweredObject in PoweredObjects) {
										poweredObject.SendMessage("OnPowerSourceGainPower", SendMessageOptions.DontRequireReceiver);
								}
						} else {
								mHadPowerLastFrame = false;
								foreach (GameObject poweredObject in PoweredObjects) {
										poweredObject.SendMessage("OnPowerSourceLosePower", SendMessageOptions.DontRequireReceiver);
								}
						}
				}

				public void					Update()
				{
						if (mHadPowerLastFrame) {
								if (!HasPower) {
										foreach (GameObject poweredObject in PoweredObjects) {
												poweredObject.SendMessage("OnPowerSourceLosePower", SendMessageOptions.DontRequireReceiver);
												worlditem.audio.PlayOneShot(PowerDownClip);
										}
										mHadPowerLastFrame = false;
								}
								return;
						}
			
						if (!mHadPowerLastFrame) {
								if (HasPower) {
										foreach (GameObject poweredObject in PoweredObjects) {
												poweredObject.SendMessage("OnPowerSourceGainPower", SendMessageOptions.DontRequireReceiver);
												worlditem.audio.PlayOneShot(PowerUpClip);
										}
										mHadPowerLastFrame = true;
								}
								return;
						}
				}

				protected virtual bool 		CheckPower()
				{
						return true;
				}

				protected bool mHadPowerLastFrame = false;
		}
}