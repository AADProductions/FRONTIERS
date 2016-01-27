using UnityEngine;

/// <summary>
/// Attaching this script to an object will make that object face the specified target.
/// The most ideal use for this script is to attach it to the camera and make the camera look at its target.
/// </summary>

[AddComponentMenu("NGUI/Examples/Look At Target")]
public class LookAtTarget : MonoBehaviour
{
	public int level = 0;
	public Transform target;
	public float speed = 8f;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
	}

	void Update ()
	{
		if (target != null)
		{
			Vector3 dir = target.position - mTrans.position;
			float mag = dir.magnitude;

			if (mag > 0.001f)
			{
				mTrans.rotation = Quaternion.identity;
				Quaternion lookRot = Quaternion.LookRotation(dir);
//				mTrans.rotation = Quaternion.Slerp(mTrans.rotation, lookRot, Mathf.Clamp01(speed * Time.deltaTime));
				mTrans.rotation = lookRot;
				mTrans.Rotate (0f, -270f, 0f);
			}
		}
	}
}