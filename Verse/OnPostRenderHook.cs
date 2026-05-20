using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class OnPostRenderHook
{
	private struct Callbacks
	{
		public bool preRenderCalled;

		public Action postRender;
	}

	private static Dictionary<Camera, Callbacks> hooks;

	static OnPostRenderHook()
	{
		hooks = new Dictionary<Camera, Callbacks>();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRender));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRender));
	}

	public static void HookOnce(Camera camera, Action postRender)
	{
		hooks.Add(camera, new Callbacks
		{
			postRender = postRender,
			preRenderCalled = false
		});
	}

	private static void OnPreRender(Camera camera)
	{
		if (hooks.TryGetValue(camera, out var value))
		{
			hooks[camera] = new Callbacks
			{
				postRender = value.postRender,
				preRenderCalled = true
			};
		}
	}

	private static void OnPostRender(Camera camera)
	{
		if (hooks.TryGetValue(camera, out var value) && value.preRenderCalled)
		{
			hooks.Remove(camera);
			value.postRender();
		}
	}
}
