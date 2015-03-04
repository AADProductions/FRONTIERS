using UnityEngine;
using System.Collections;
using Frontiers.Data;
using System;

namespace Frontiers.World {
	public class WMIcon : MonoBehaviour {
		public Action OnClick;
		public MobileReference Reference;
		//uuug so circular i hate it
		public void OnClickIcon ( ) {
			OnClick ();
		}
	}
}
