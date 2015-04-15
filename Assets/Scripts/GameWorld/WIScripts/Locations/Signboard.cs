using UnityEngine;
using System.Collections;

namespace Frontiers.World.WIScripts
{
	public class Signboard : WIScript {

		public override bool SaveItemOnUnloaded {
			get {
				return false;//the signboard manager will take care of this
			}
		}

		public WorldItem Owner;
		public SignboardStyle Style = SignboardStyle.A;
		public string TextureName = string.Empty;
	}

	public enum SignboardStyle {
		A,
		B,
	}
}