using Unity.Profiling;
using UnityEngine;

namespace Verse;

public static class MemoryUsageUtility
{
	private static ProfilerRecorder systemMemoryRecorder;

	private static ProfilerRecorder totalMemoryRecorder;

	private static ProfilerRecorder gfxMemoryRecorder;

	private static ProfilerRecorder gcMemoryRecorder;

	private static ProfilerRecorder gcReservedMemoryRecorder;

	private static ProfilerRecorder audioMemoryRecorder;

	public static long TrackedMemoryUsageBytes => totalMemoryRecorder.CurrentValue;

	public static long OsMemoryUsageBytes => systemMemoryRecorder.CurrentValue;

	public static long GraphicsMemoryUsageBytes => gfxMemoryRecorder.CurrentValue;

	public static long ManagedMemoryUsageBytes => gcMemoryRecorder.CurrentValue;

	public static long ManagedMemoryReservedBytes => gcReservedMemoryRecorder.CurrentValue;

	public static long AudioMemoryUsageBytes => audioMemoryRecorder.CurrentValue;

	public static long TextureMemoryUsageBytes => (long)Texture.currentTextureMemory;

	static MemoryUsageUtility()
	{
		systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
		totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
		gfxMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
		gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
		gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
		audioMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Used Memory");
		systemMemoryRecorder.Stop();
		totalMemoryRecorder.Stop();
		gfxMemoryRecorder.Stop();
		gcMemoryRecorder.Stop();
		gcReservedMemoryRecorder.Stop();
		audioMemoryRecorder.Stop();
	}

	public static void SetShouldRecord(bool shouldRecord)
	{
		if (shouldRecord)
		{
			systemMemoryRecorder.Start();
			totalMemoryRecorder.Start();
			gfxMemoryRecorder.Start();
			gcMemoryRecorder.Start();
			gcReservedMemoryRecorder.Start();
			audioMemoryRecorder.Start();
		}
		else
		{
			systemMemoryRecorder.Stop();
			totalMemoryRecorder.Stop();
			gfxMemoryRecorder.Stop();
			gcMemoryRecorder.Stop();
			gcReservedMemoryRecorder.Stop();
			audioMemoryRecorder.Stop();
		}
	}
}
