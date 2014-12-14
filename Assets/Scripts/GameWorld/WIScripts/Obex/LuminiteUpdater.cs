using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class LuminiteUpdater : MonoBehaviour
		{		//TODO this has been absorbed into WIStates class
				public Luminite luminite = null;
				public bool IsDark = false;

				public void Update()
				{
						if (luminite == null) {
								FinishedUpdating();
								return;
						}

						
				}

				public void ConvertToDark(bool overTime)
				{
						if (IsDark) {
								return;
						}	

						IsDark = true;
						if (overTime) {
								StartCoroutine(ConvertToDarkOverTime());
						} else {
								//convert to dark immediately
						}
				}

				protected void FinishedUpdating()
				{
						if (mFinishedUpdating || mConvertingToDark) {
								return;
						}

						mFinishedUpdating = true;
						GameObject.Destroy(this);
				}

				public IEnumerator ConvertToDarkOverTime()
				{
						bool finished = false;
//			Color startColor 	= luminite.CrystalRenderer.material.color;
//			Color endColor		= Colors.Get.DarkLuminiteMaterialColor;

						while (!finished) {
								finished = true;
						}		

						mConvertingToDark = false;
			
						yield break;
				}

				protected float mConversionStartTime	= 0.0f;
				protected float mConversionInterval = 1.0f;
				protected bool mConvertingToDark = false;
				protected bool mFinishedUpdating = false;
		}
}