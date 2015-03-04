using UnityEngine;
using System.Collections;
using Frontiers.World;

namespace Frontiers {
	public class FXPieceTemplate : MonoBehaviour {

		public void Initialize (FXPiece piece)
		{
			FXName = piece.FXName;
			SoundName = piece.SoundName;
			SoundType = piece.SoundType;
			FXDelay = piece.Delay;
			FXDuration = piece.Duration;
			FXColor = piece.FXColor;
			Explosion = piece.Explosion;
			JustForShow = piece.JustForShow;

			transform.localPosition = piece.Position;
			transform.localRotation = Quaternion.Euler (piece.Rotation);
			transform.localScale = Vector3.one * piece.Scale;
		}

		//[FrontiersFXAttribute]
		public string FXName;
		public MasterAudio.SoundType SoundType = MasterAudio.SoundType.None;
		public string SoundName;
		public float FXDelay = 0f;
		public float FXDuration = -1f;
		public Color FXColor = Color.white;
		public bool Explosion = false;
		public bool JustForShow = true;

		public void OnDrawGizmos ( ) {
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere (transform.position, 0.5f);
		}
	}
}
