using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// Hydrogen.Threading.Jobs.MeshCombiner Example
/// </summary>
public class MeshCombinerExample : MonoBehaviour
{
	/// <summary>
	/// The Target Meshes
	/// </summary>
	public Transform TargetMeshes;
	/// <summary>
	/// This is used in our example to throttle things a bit when accessing Unity objects.
	/// </summary>
	/// <remakrs>It seems at 180, its a nice sweet spot for these meshes in this scene.</remarks>
	public int ThrottleRate = 1000;
	/// <summary>
	/// The lovely MeshCombiner
	/// </summary>
	Hydrogen.Threading.Jobs.MeshCombiner _meshCombiner = new Hydrogen.Threading.Jobs.MeshCombiner ();
	/// <summary>
	/// Reference for when the actual processing began
	/// </summary>
	float _combinerStartTime;
	/// <summary>
	/// Reference for when the actual processing ended.
	/// </summary>
	float _combinerEndTime;
	
	/// <summary>
	/// Process meshFilters in Unity's main thread, as we are required to by Unity. At least we've rigged it as a 
	/// coroutine! Right? OK I know I really wish we could have used mesh data in a thread but properties die as well.
	/// </summary>
	/// <returns>IEnumartor aka Coroutine</returns>
	/// <remarks>
	/// For the sake of the demo we are going to need to roll over the "Target" to find all the 
	/// meshes that we need to look at, but in theory you could do this without having to load the
	/// object by simply having raw mesh data, or any other means of accessing it.
	/// </remarks>
	public IEnumerator PreProcess ()
	{
		_combinerStartTime = Time.time;
		// Create a new MeshCombiner (we dont want any old data kicking around)
		_meshCombiner = new Hydrogen.Threading.Jobs.MeshCombiner ();
		
		// Yes We Hate This - There Are Better Implementations
		MeshFilter[] meshFilters = TargetMeshes.GetComponentsInChildren<MeshFilter> ();
		
		// Loop through all of our mesh filters and add them to the combiner to be combined.
		for (int x = 0; x < meshFilters.Length; x++) {
			
			if (meshFilters [x].gameObject.activeSelf) {
				_meshCombiner.AddMesh (meshFilters [x], 
				                       meshFilters [x].renderer, 
				                       meshFilters [x].transform.localToWorldMatrix);
			}
			
			// We implemented this as a balance point to try and break some of the processing up.
			// If we were to yield every pass it was taking to long to do nothing.
			if (x > 0 && x % ThrottleRate == 0) {
				yield return new WaitForEndOfFrame ();
			}
		}
		
		// Start the threaded love
		if (_meshCombiner.MeshInputCount > 0) {
			_meshCombiner.Combine (ThreadCallback);
		}
		yield return new WaitForEndOfFrame ();
	}
	
	/// <summary>
	/// Process the MeshDescription data sent back from the Combiner and make it appear!
	/// </summary>
	/// <param name="hash">Instance Hash.</param>
	/// <param name="meshDescriptions">MeshDescriptions.</param>
	/// <param name="materials">Materials.</param>
	public IEnumerator PostProcess (int hash, Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] meshOutputs)
	{
		var go = new GameObject ("Combined Meshes");
		go.transform.position = TargetMeshes.position;
		go.transform.rotation = TargetMeshes.rotation;
		
		// Make our meshes in Unity
		for (int x = 0; x < meshOutputs.Length; x++) {
			var meshObject = new GameObject ();
			
			var newMesh = _meshCombiner.CreateMeshObject (meshOutputs [x]);
			
			meshObject.name = newMesh.Mesh.name;
			meshObject.AddComponent<MeshFilter> ().sharedMesh = newMesh.Mesh;
			meshObject.AddComponent<MeshRenderer> ().sharedMaterials = newMesh.Materials;
			meshObject.transform.parent = go.transform;
			meshObject.transform.position = Vector3.zero;
			meshObject.transform.rotation = Quaternion.identity;
			
			
			// Fake Unity Threading
			if (x > 0 && x % ThrottleRate == 0) {
				yield return new WaitForEndOfFrame ();
			}
		}
		
		// Clear previous data (for demonstration purposes)
		// It could be useful to keep some mesh data in already parsed, then you could use the RemoveMesh function
		// to remove ones that you want changed, without having to reparse mesh data.
		_meshCombiner.ClearMeshes ();
		
		_combinerEndTime = Time.time;
	}
	
	/// <summary>
	/// This function is called in the example after the MeshCombiner has processed the meshes, it starts a Coroutine 
	/// to create the actual meshes based on the flat data. This is the most optimal way to do this sadly as we cannot
	/// create or touch Unity based meshes outside of the main thread.
	/// </summary>
	/// <param name="hash">Instance Hash.</param>
	/// <param name="meshOutputs">.</param>
	public void ThreadCallback (int hash, Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] meshOutputs)
	{
		// This is just a dirty way to see if we can squeeze jsut a bit more performance out of Unity when 
		// making all of the meshes for us (instead of it being done in one call, we use a coroutine with a loop.
		StartCoroutine (PostProcess (hash, meshOutputs));
	}
	
	/// <summary>
	/// Unity's LateUpdate Event
	/// </summary>
	void LateUpdate ()
	{
		// If we have a MeshCombiner lets run the Check()
		if (_meshCombiner != null) {
			// Funny thing about this method of doing this; lots of Thread based solutions in Unity have an
			// elaborate manager that does this for you ... just saying.
			_meshCombiner.Check ();
		}
	}
	
	/// <summary>
	/// Unity's OnGUI Event
	/// </summary>
	void OnGUI ()
	{
		// A clever little trick to only show the button when nothing is going on.
		// Obviously, in a real world setting this would probably not look like this.
		if (_meshCombiner != null && _meshCombiner.MeshInputCount == 0) {
			
			if (GUI.Button (new Rect (5, 5, 200, 35), "MeshCombiner")) {
				
				// Get rid of any existing combined mesh data
				GameObject go = GameObject.Find ("Combined Meshes");
				if (go != null)
					Object.Destroy (go);
				
				// Enable our example content
				TargetMeshes.gameObject.SetActive (true);
				
				// Do it!
				StartCoroutine (PreProcess ());
				
				// Disable our example data
				TargetMeshes.gameObject.SetActive (false);
			}
		}
		
		// Some debug output, helpful for determining if we block the main thread much.
		UnityEngine.GUI.color = Color.black;
		GUI.Label (new Rect (15, Screen.height - 40, 200, 20), "Time.time: " + Time.time.ToString ());
		GUI.Label (new Rect (15, Screen.height - 25, 200, 20), "MeshCombiner: " + (_combinerEndTime - _combinerStartTime).ToString ());
	}
}