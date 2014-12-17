using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
//using System.Windows.Forms;
using UnityEngine;
using Frontiers.World;

namespace Frontiers
{
    /// <summary>
    /// A quad tree of elements.
    /// </summary>
    /// <typeparam name="TContent">The content element type which is stored in this tree.</typeparam>
    public class QuadTree<TContent> : QuadNode<TContent> where TContent : IHasPosition
    {
        //======================================================
        //      Constructors
        //======================================================
        /// <summary>
        /// Constructs a new quad tree.
        /// </summary>
        /// <param name="boundaries">The boundaries of the quad tree. The z variable will be ignored.</param>
        /// <param name="maxContent">The maximum content to store in one node before it is subdivided.</param>
        /// <param name="content">An optional list of content for the tree. Can be null.</param>
        public QuadTree(Bounds boundaries, int maxContent, IEnumerable<TContent> content = null)
            : base(null,boundaries,maxContent)
        {
			if (content != null)
            {
				foreach (var c in content) {
					Add (c);
				}
            }
        }

        //======================================================
        //      Public methods
        //======================================================


        //======================================================
        //      Protected elements
        //======================================================

        //======================================================
        //      Properties
        //======================================================

        //======================================================
        //      Events
        //======================================================

        //======================================================
        //      References
        //======================================================
    }

} // End of namespace Frontiers
