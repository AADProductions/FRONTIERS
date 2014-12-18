#region Copyright Notice & License Information
//
// hInput.cs
//
// Author:
//       Robin Southern <betajaen@ihoed.com>
//
// Copyright (c) 2013 dotBunny Inc. (http://www.dotbunny.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A drop in implementation of the Hydrogen.Plugins.TestFlight manager. It implements advanced features included with
/// TestFlight allowing for proper session tracking and reporting.
/// </summary>
[AddComponentMenu ("Hydrogen/Modding API")]
public sealed class hModding : MonoBehaviour
{
	private readonly List<hMod> mods = new List<hMod> ();

	void Awake ()
	{
		AddMod (Application.dataPath + System.IO.Path.DirectorySeparatorChar + "test.dll");

		LoadAllMods ();
	}

	private void LoadAllMods ()
	{
		foreach (hMod mod in mods) {
			mod.LoadMod ();
		}
	}

	public void AddMod (String path)
	{
		GameObject go = new GameObject (System.IO.Path.GetFileNameWithoutExtension (path));
		go.transform.parent = transform.parent;
		hMod mod = go.AddComponent<hMod> ();
		mod.Initialise (path);
		if (mod.IsLoaded) {
			mods.Add (mod);
		}
	}
}