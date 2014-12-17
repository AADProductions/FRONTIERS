using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

//using System.Windows.Forms;
using System.Collections;
using UnityEngine;
using Frontiers.World;

namespace Frontiers
{
	/// <summary>
	/// A quadtree node.
	/// </summary>
	/// <typeparam name="TContent">The type of content to store in the quad tree.</typeparam>
	public class QuadNode<TContent> where TContent : IHasPosition
	{
		//======================================================
		//      Constructors
		//======================================================
		/// <summary>
		/// Instanciates a new quadtree node.
		/// </summary>
		/// <param name="boundaries">The boundaries of the node.</param>
		/// <param name="maxContent">The maximum number of elements in Content before the node is subdivided.</param>
		/// <param name="parent">The parent node.</param>
		public QuadNode (QuadNode<TContent> parent, Bounds boundaries, int maxContent)
		{
			Parent = parent;
			MaxContent = maxContent;
			Boundaries = boundaries;
		}

		public void ClearAll ()
		{
			if (Children != null) {
				for (int i = 0; i < Children.Length; i++) {
					Children [i].ClearAll ();
					Children [i] = null;
				}
				Array.Clear (Children, 0, Children.Length);
				Children = null;
			}
		}
		//======================================================
		//      Public methods
		//======================================================
		const int ChildrenCount = 4;

		/// <summary>
		/// Adds a new content element. Subdivides the node, if required.
		/// </summary>
		/// <param name="content">The content to add.</param>
		/// <returns>True if the content could be added, false if not.</returns>
		public bool Add (TContent content)
		{
			//if (WorldChunk.gDebug) { //Debug.Log ("Trying to add " + content.Position.ToString ()); }
			// Check if the content can be put in this node, location-wise
			if (!Boundaries.Contains (content.Position)) {
				//if (WorldChunk.gDebug) {  //Debug.Log ("Boundaries don't contain object"); }
				return false;
			}

			// If there are no children yet...
			if (Children == null) {
				if (Content == null)
					Content = new List<TContent> ();

				// ... and the content still fits in
				if (Content.Count < MaxContent) {
					// Add the content.
					Content.Add (content);
					return true;
				}
			}

			// The content does not fit in. Subdivide into 4 nodes, if not yet done
			if (Children == null) {
				//if (WorldChunk.gDebug) { //Debug.Log ("The content does not fit in. Subdivide into 4 nodes"); }
				Subdivide ();
			}

			// Try to add the content to each of the four children. One must accept it.
			for (int i = 0; i < Children.Length; i++) {
				//if (WorldChunk.gDebug) { //Debug.Log ("Trying to add to child " + i.ToString ()); }
				if (Children [i].Add (content)) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Recursively finds all content in this node and subnodes within the given bounds.
		/// </summary>
		/// <param name="bounds">The bounds.</param>
		/// <returns>An enumerable list of content.</returns>
		public IEnumerable<TContent> FindContentIn (Bounds bounds)
		{
			if (!Boundaries.Intersects (bounds))
				return Enumerable.Empty<TContent> ();

			return Content ?? (Children != null ? Children.SelectMany (c => c.FindContentIn (bounds)) : Enumerable.Empty<TContent> ());
		}

		/// <summary>
		/// Recursively finds all nodes whose bounds intersect with a given bounding box.
		/// </summary>
		/// <param name="bounds">The bounds.</param>
		/// <returns>An enumerable list of nodes.</returns>
		public IEnumerable<QuadNode<TContent>> FindNodesIntersecting (Bounds bounds)
		{
			if (!Boundaries.Intersects (bounds))
				return Enumerable.Empty<QuadNode<TContent>> ();

			if (Children != null)
				return Children.SelectMany (c => c.FindNodesIntersecting (bounds));
			else
				return Yield (this);
		}
		//======================================================
		//      Protected elements
		//======================================================
		/// <summary>
		/// Subdivides the node into 4 children.
		/// </summary>
		protected void Subdivide ()
		{
			if (Children != null) {
				//if (WorldChunk.gDebug) { //Debug.Log ("Already have children, not subdividing..."); } 
				return;
			}

			// Subdivide into 4 equal sized bounded nodes
			Children = new QuadNode<TContent>[ChildrenCount];
			Vector3 halfSize = Boundaries.size * 0.5f;
			halfSize.y = Boundaries.size.y;
			Vector3 offset = halfSize * 0.5f;
			for (int i = 0; i < Children.Length; i++) {
				int x = (i / 1) % 2;
				int z = (i / 2) % 2;
				Vector3 newCenter = Boundaries.min + new Vector3 (x * halfSize.x, 0, z * halfSize.z) + offset;
				newCenter.y = 0f;//whuh?
				var c = Children [i] = new QuadNode<TContent> (this, new Bounds (newCenter, halfSize), MaxContent);
			}
			int maxDepth = 0;
			// Add each content element to the first child node which will accept it
			foreach (var content in Content) {
				bool added = false;
				for (int i = 0; i < Children.Length; i++) {
					if (Children [i].Add (content)) {
						added = true;
						break;
					}
				}

				if (!added)
					/*throw new NotImplementedException*/Debug.LogWarning ("QuadTree nodes too small! This should not happen!");
			}

			Content = null;
		}

		protected IEnumerable<T> Yield<T> (T item)
		{
			yield return item;
		}
		//======================================================
		//      Properties
		//======================================================
		/// <summary>
		/// Gets the child nodes.
		/// </summary>
		public QuadNode<TContent>[] Children {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the boundaries of this node.
		/// </summary>
		public Bounds Boundaries {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the parent node.
		/// </summary>
		public QuadNode<TContent> Parent {
			get;
			protected set;
		}

		/// <summary>
		/// Gets the content of the node. Do not add to this list directly. Use the Add() - method instead.
		/// </summary>
		public List<TContent> Content {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the maximum number of elements in the Content list.
		/// </summary>
		public int MaxContent {
			get;
			set;
		}
	}
}
 // End of namespace Frontiers
