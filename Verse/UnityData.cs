using System;
using System.Threading;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace Verse;

public static class UnityData
{
	private static bool initialized;

	public static bool isEditor;

	public static string dataPath;

	public static RuntimePlatform platform;

	public static string persistentDataPath;

	private static int mainThreadId;

	private static int maximumJobWorkerThreads;

	private static int maximumJobWorkerCount;

	private static bool computeShadersSupported;

	public static bool IsInMainThread => mainThreadId == Thread.CurrentThread.ManagedThreadId;

	public static bool Is32BitBuild => IntPtr.Size == 4;

	public static bool Is64BitBuild => IntPtr.Size == 8;

	public static int MaxJobWorkerThreadCount => maximumJobWorkerThreads;

	public static int MaxJobWorkerCount => maximumJobWorkerCount;

	public static bool ComputeShadersSupported => computeShadersSupported;

	public static event Action DisposeStatic;

	public static int GetIdealBatchCount(int items)
	{
		return Mathf.Max(items / maximumJobWorkerThreads, 4);
	}

	public static void DisposeStaticResources()
	{
		UnityData.DisposeStatic?.Invoke();
		UnityData.DisposeStatic = null;
	}

	static UnityData()
	{
		if (!initialized && !UnityDataInitializer.initializing)
		{
			Log.Warning("Used UnityData before it's initialized.");
		}
	}

	public static void CopyUnityData()
	{
		mainThreadId = Thread.CurrentThread.ManagedThreadId;
		isEditor = Application.isEditor;
		dataPath = Application.dataPath;
		platform = Application.platform;
		persistentDataPath = Application.persistentDataPath;
		maximumJobWorkerThreads = JobsUtility.ThreadIndexCount + 1;
		maximumJobWorkerCount = JobsUtility.JobWorkerCount;
		computeShadersSupported = false;
		initialized = true;
	}

	private static bool IsIntegratedGraphicsCard()
	{
		if (SystemInfo.graphicsDeviceVendorID != 32902 && SystemInfo.graphicsDeviceVendorID != 4203)
		{
			return IsAmdIntegratedGraphics();
		}
		return true;
	}

	private static bool IsAmdIntegratedGraphics()
	{
		if (SystemInfo.graphicsDeviceVendorID != 4098)
		{
			return false;
		}
		string text = SystemInfo.graphicsDeviceName.ToLowerInvariant();
		int num = text.IndexOf("m graphics", StringComparison.InvariantCulture);
		if (num > 1 && char.IsDigit(text[num - 1]))
		{
			return true;
		}
		if (text.Contains("vega"))
		{
			return true;
		}
		if (text.Contains("integrated") || text.Contains("apu"))
		{
			return true;
		}
		if (text.Contains("ryzen"))
		{
			return true;
		}
		string text2 = text.ToLowerInvariant().Trim();
		if (text2 == "amd radeon(tm) graphics" || text2 == "amd radeon graphics")
		{
			return true;
		}
		return false;
	}
}
