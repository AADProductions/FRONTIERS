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
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A drop in implementation of the Hydrogen.Plugins.TestFlight manager. It implements advanced features included with
/// TestFlight allowing for proper session tracking and reporting.
/// </summary>
[AddComponentMenu ("Hydrogen/Modding API")]
public sealed class hMod : MonoBehaviour
{
		private Assembly assembly;
		private Type modType;
		private object modObject;
		private const int kMessage_OnLoad = 0;
		private const int kMessage_OnUnload = 1;
		private const int kMessage_GetInfo = 2;
		private const int kMessage_OnQuit = 3;
		private const int kMessage_Count = 4;
		private MethodInfo[] messages = new MethodInfo[kMessage_Count];
		public String ModName;

		public bool IsLoaded {
				get { return modObject != null; }
		}

		public void Initialise (String path)
		{
				assembly = Assembly.Load (System.IO.File.ReadAllBytes (path));
				modObject = null;
				modType = null;
				ModName = "Unknown";

				if (assembly == null)
						return;

				modType = assembly.GetTypes ().First (t => t.Name == "Mod");

				if (modType == null)
						return;

				modObject = Activator.CreateInstance (modType);

				messages [kMessage_OnLoad] = modType.GetMethod ("OnLoad");
				messages [kMessage_OnUnload] = modType.GetMethod ("OnUnload");
				messages [kMessage_GetInfo] = modType.GetMethod ("GetInfo");
				messages [kMessage_OnQuit] = modType.GetMethod ("OnQuit");

				if (messages [kMessage_GetInfo] != null) {
						object response = messages [kMessage_GetInfo].Invoke (modObject, null);
						if (response != null && response is string) {
								ModName = (String)response;
						}
				}

		}

		void Invoke (int messageType)
		{
				if (IsLoaded && messages [messageType] != null) {
						messages [messageType].Invoke (modObject, null);
				} else {
						Debug.Log ("Cannot invoke mod message " + messageType.ToString ());
				}
		}

		public void LoadMod ()
		{
				Invoke (kMessage_OnLoad);
		}

		public void UnloadMod ()
		{
				Invoke (kMessage_OnUnload);
		}

		void OnDestroy ()
		{
				Invoke (kMessage_OnQuit);
		}
}
/*
 class Mod
 {
    String OnApplicationStart()
    {
        return "Mod v1.0";
    }
    void OnApplicationStop()
    {
    }
    void OnEnable()
    {
    }  
    void OnDisable()
    {
    }  
    void OnUpdate()
    {
    }
 }
*/