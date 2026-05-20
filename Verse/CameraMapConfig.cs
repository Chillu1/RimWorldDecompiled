using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Steam;

namespace Verse;

public abstract class CameraMapConfig : IExposable
{
	public float dollyRateKeys = 50f;

	public float dollyRateScreenEdge = 35f;

	public float camSpeedDecayFactor = 0.85f;

	public float moveSpeedScale = 2f;

	public float zoomSpeed = 2.6f;

	public FloatRange sizeRange = new FloatRange(SteamDeck.IsSteamDeck ? 7.2f : 11f, 60f);

	public float zoomPreserveFactor;

	public bool smoothZoom;

	public bool followSelected;

	public string fileName;

	public bool autoPanWhilePaused;

	public float autoPanTargetAngle;

	public float autoPanAngle;

	public float autoPanSpeed;

	public bool gravshipFreeCam;

	public bool gravshipPanOnCutsceneStart = true;

	public bool gravshipEnableOverrideSizeRange;

	public FloatRange gravshipOverrideSizeRange = new FloatRange(15f, 60f);

	public virtual void ConfigFixedUpdate_60(ref Vector3 rootPos, ref Vector3 velocity)
	{
		if (followSelected)
		{
			List<Pawn> selectedPawns = Find.Selector.SelectedPawns;
			if (selectedPawns.Count > 0)
			{
				Vector3 zero = Vector3.zero;
				int num = 0;
				foreach (Pawn item in selectedPawns)
				{
					if (item.MapHeld == Find.CurrentMap)
					{
						zero += item.TrueCenter();
						num++;
					}
				}
				if (num > 0)
				{
					zero /= (float)num;
					zero.y = rootPos.y;
					rootPos = Vector3.MoveTowards(rootPos, zero, 0.02f * Mathf.Max(Find.TickManager.TickRateMultiplier, 1f) * moveSpeedScale);
				}
			}
		}
		if (autoPanSpeed > 0f && (Find.TickManager.CurTimeSpeed != TimeSpeed.Paused || autoPanWhilePaused))
		{
			velocity.x = Mathf.Cos(autoPanAngle) * autoPanSpeed;
			velocity.z = Mathf.Sin(autoPanAngle) * autoPanSpeed;
		}
	}

	public virtual void ConfigOnGUI()
	{
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref moveSpeedScale, "moveSpeedScale", 0f);
		Scribe_Values.Look(ref zoomSpeed, "zoomSpeed", 0f);
		Scribe_Values.Look(ref sizeRange, "sizeRange");
		Scribe_Values.Look(ref zoomPreserveFactor, "zoomPreserveFactor", 0f);
		Scribe_Values.Look(ref smoothZoom, "smoothZoom", defaultValue: false);
		Scribe_Values.Look(ref followSelected, "followSelected", defaultValue: false);
		Scribe_Values.Look(ref autoPanTargetAngle, "autoPanTargetAngle", 0f);
		Scribe_Values.Look(ref autoPanSpeed, "autoPanSpeed", 0f);
		Scribe_Values.Look(ref fileName, "fileName");
		Scribe_Values.Look(ref autoPanWhilePaused, "autoPanWhilePaused", defaultValue: false);
	}
}
