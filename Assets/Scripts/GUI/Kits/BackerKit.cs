using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;

public class BackerKit : MonoBehaviour
{
	public string 			url;
	public string			posturl;
	public string			systemInfoUrl;
	public string			kitName;
	
	public float			versionNumber	= 1.0f;
	
	public string			submitError = 	"\n\nDon't worry this hasn't blown your chance to submit your kit." +
											"\nThe kit has created a file called error.txt, find that and send it to frontiers.game@gmail.com, and we'll figure this out." +
											"\n\n(Or just try submitting again. You never know.)" +
											"\n\nHit RETURN to return to the kit.";
	
	public string			display;
	public string 			submission;
	public string			systemInfoSubmission;
	public string			docName;
	public string			submissionKickstarterEmail;
	
	public bool				SaveToDisk		= false;
	public bool				IsWorking 		= false;
	public bool				HasSubmitted	= false;
	public KitState			State;
	public LocalKitState	LocalState;
	
	public void				Update ( )
	{
		if (Input.GetKeyDown (KeyCode.Return) && IsWorking)
		{
			StopCoroutine ("SubmitToServerOverTime");
			IsWorking = false;
		}
		
		if (IsWorking && string.IsNullOrEmpty (display))
		{
			display = "Working...";
		}
	}
	
	public void 			CheckKitState ( )
	{
//		//Debug.Log ("Checking kit state");
		if (!IsWorking)
		{
			StartCoroutine (CheckKitStateOverTime ( ));
		}
	}
	
	public void				SubmitToServer ( )
	{
//		//Debug.Log ("Submitting to server");
		if (!IsWorking)
		{
			StartCoroutine (SubmitToServerOverTime ( ));
			if (!string.IsNullOrEmpty (systemInfoSubmission))
			{
				StartCoroutine (SubmitSystemInfoToServerOverTime ( ));
			}
			IsWorking = true;
			if (SaveToDisk)
			{
				SaveSubmissionToDisk ( );
			}
		}
	}
	
	public void				SaveSubmissionToDisk ( )
	{
		#if UNITY_STANDALONE_WIN
		string filePath = Path.Combine (Application.dataPath, docName);
		File.WriteAllText (filePath, submission);
		display += ("\n(Saved local file to " + filePath + ")\n");
		#endif
	}

	IEnumerator				SubmitToServerOverTime ( )
	{
//		//Debug.Log ("Submitting to server over time");
		display = "Connecting...";
		
		string invalidChars = System.Text.RegularExpressions.Regex.Escape( new string( System.IO.Path.GetInvalidFileNameChars() ) );
   		string invalidReStr = string.Format( @"([{0}]*\.+$)|([{0}]+)", invalidChars );
   		string cleanDocName = System.Text.RegularExpressions.Regex.Replace( docName, invalidReStr, "_" );
		
		yield return new WaitForSeconds (1.0f);
		
		WWWForm submitForm = new WWWForm ( );
		submitForm.AddField ("KickstarterEmail", submissionKickstarterEmail);
		submitForm.AddField ("Submission", submission);
		submitForm.AddField ("DocName", cleanDocName);
		var formData = submitForm.data;
		var headers = submitForm.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (System.Text.Encoding.ASCII.GetBytes("BackerKit:cn4t9#3h4"));
		
		WWW submitWww = new WWW (posturl, formData, headers);
		
		display += "\n\nSubmitting data...";
		
		while (!submitWww.isDone)
		{
			yield return new WaitForSeconds (0.1f);
		}
		
		if (submitWww.error == null)
		{
			display += "\nSuccessful!\n\n(Hit RETURN to return to the kit)";
			HasSubmitted = true;
			yield break;
		}
		else
		{
			display += ("\nHmm there was an error: " + submitWww.error + submitError);
			HasSubmitted = false;
			yield break;
		}
	}
	
	IEnumerator				SubmitSystemInfoToServerOverTime ( )
	{
		WWWForm submitForm = new WWWForm ( );
		submitForm.AddField ("KickstarterEmail", submissionKickstarterEmail);
		submitForm.AddField ("SystemInfo", systemInfoSubmission);
		submitForm.AddField ("KitName", kitName);
		var formData = submitForm.data;
		var headers = submitForm.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (System.Text.Encoding.ASCII.GetBytes("BackerKit:cn4t9#3h4"));
		
		WWW submitWww = new WWW (systemInfoUrl, formData, headers);
		
		while (!submitWww.isDone)
		{
			yield return new WaitForSeconds (0.1f);
		}
	}

	IEnumerator 			CheckKitStateOverTime ( )
	{
		IsWorking = true;
		display = "Connecting...";
	
		WWW www = new WWW(url);
		
	    while(!www.isDone)
	    {
	       yield return new WaitForSeconds(0.1f);
	    }
	
		MemoryStream xmlStream = new MemoryStream (System.Text.Encoding.UTF8.GetBytes (www.text));
	
		var serializer 							= new XmlSerializer (typeof (KitState));
		var xmlReader							= new XmlTextReader (xmlStream);
		State									= (KitState) serializer.Deserialize (xmlReader);
	
		if (State.IsAvailable)
		{
			display += "\nThis kit is still active";
		}
		else
		{
			display += "\nThis kit is NOT active";
			yield break;
		}
		
		if (State.VersionNumber > versionNumber)
		{
			display += "\n\nThis kit is outdated! (Version:" + versionNumber.ToString ( ) + ", Latest Release: " + State.VersionNumber.ToString ( );
		}
		else
		{
			display += "\n\nKit is up to date.\n";
		}
	
		yield return new WaitForSeconds (1.0f);
	
		display = string.Empty;
		IsWorking = false;
	
		yield break;
	}
}

[Serializable]
public class LocalKitState
{
	public string 	KickstarterEmail	= string.Empty;
	public bool		BackerVersion		= true;
}

[Serializable]
public class KitState
{
	public bool 	IsAvailable 	= true;
	public float	VersionNumber	= 1.0f;
}

[Serializable]
public class KitTest
{
	public string Submission	= string.Empty;
}