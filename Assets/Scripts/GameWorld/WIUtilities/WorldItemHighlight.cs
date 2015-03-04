using UnityEngine;
using System.Collections;
using Frontiers;


public class WorldItemHighlight : MonoBehaviour {
	
	public int 				FocusMatIndex	= 1;
	public int				DamageMatIndex	= 2;
	
	public Material[ ] 		Normal;
	public Material[ ] 		Highlight;
	
	public float			HighlightCoolDown;
	public float			DamageCoolDown;	
	public float			CoolDownSpeed;

	void 					Start ( )
	{
//		Normal				= new Material [1];
//		HighLight 			= new Material [3];
//		
//		Normal [0] 			= renderer.material;
//		
//		Highlight [0] 		= renderer.material;
//		Highlight [1] 		= Mats.FocusHighlightMaterial;
//		Highlight [2] 		= Mats.DamageHighlightMaterial;
	}
	
	public IEnumerator		StartDamageHighlight ( )
	{
		yield break;
	}
	
	public IEnumerator		EndFocusHighlight ( )
	{
		yield break;
	}
	
	public void 			OnEnterPlayerFocus ( )
	{
		HighlightCoolDown = 1.0f;
	}
	
	public void 			OnExitPlayerFocus ( )
	{
		HighlightCoolDown = 1.0f;
	}
	
	public void 			OnTakeDamage ( )
	{
//		DamageCoolDown = 1.0f;
//		
//		if (HighlightCoolDown <= 0.0f)
//		{
//			renderer.materials = FocusAndDamage;
//		}			
	}
}
