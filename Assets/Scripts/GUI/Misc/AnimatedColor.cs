using UnityEngine;

namespace Frontiers.GUI {
	public class AnimatedColor : MonoBehaviour
	{
	    public float alpha = 1f;
	    public UIWidget widget;
	    void Update () { widget.alpha = alpha; }
	}
}