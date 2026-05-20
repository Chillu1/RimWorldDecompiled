using UnityEngine;
using Verse;

public static class PerformanceBenchmarkUtility
{
	public const float BenchmarkSeconds = 30f;

	public static float startBenchmarkTime = -1f;

	public static int startBenchmarkFrames = -1;

	public static int startBenchmarkTicks = -1;

	public static void StartBenchmark()
	{
		startBenchmarkTime = Time.realtimeSinceStartup;
		startBenchmarkTicks = Find.TickManager.TicksGame;
		startBenchmarkFrames = Time.frameCount;
	}

	public static void CheckBenchmark()
	{
		if (startBenchmarkTime > 0f && startBenchmarkTime + 30f < Time.realtimeSinceStartup)
		{
			float num = Time.realtimeSinceStartup - startBenchmarkTime;
			int num2 = Time.frameCount - startBenchmarkFrames;
			int num3 = Find.TickManager.TicksGame - startBenchmarkTicks;
			Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation($"Frames per second: {(float)num2 / num}\n" + $"Ticks per second: {(float)num3 / num}\n" + $"Ticks + Frames per second: {(float)(num3 + num2) / num}\n" + $"Ticks / Frame: {(float)num3 / (float)num2}\n\n" + "----RAW----\n" + $"Time elapsed: {num}s\n" + $"Frames: {num2}\n" + $"Game Ticks: {num3}\n\n" + $"Note: Each frame the game tries to do <tickrate> ticks or as many ticks as it can before {Mathf.RoundToInt(45.454544f)}ms elapses. This means that sometimes tickrate, not framerate will increase as performance improves if the game is consistently not completing <tickrate> ticks per frame. Framerate can increase if performance improves while the game is consistently completing <tickrate> ticks per frame.", delegate
			{
			});
			startBenchmarkTime = -1f;
			Find.WindowStack.Add(window);
		}
	}
}
