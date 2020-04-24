using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class OverlayDrawHandler
	{
		private static int lastPowerGridDrawFrame;

		private static int lastZoneDrawFrame;

		public static bool ShouldDrawPowerGrid => lastPowerGridDrawFrame + 1 >= Time.frameCount;

		public static bool ShouldDrawZones
		{
			get
			{
				if (Find.PlaySettings.showZones)
				{
					return true;
				}
				if (Time.frameCount <= lastZoneDrawFrame + 1)
				{
					return true;
				}
				return false;
			}
		}

		public static void DrawPowerGridOverlayThisFrame()
		{
			lastPowerGridDrawFrame = Time.frameCount;
		}

		public static void DrawZonesThisFrame()
		{
			lastZoneDrawFrame = Time.frameCount;
		}
	}
}
