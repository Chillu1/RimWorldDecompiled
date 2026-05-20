using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public sealed class GameInfo : IExposable
{
	public bool permadeathMode;

	public string permadeathModeUniqueName;

	private float realPlayTimeInteracting;

	public PlanetTile startingTile = PlanetTile.Invalid;

	public List<Pawn> startingAndOptionalPawns = new List<Pawn>();

	private float lastInputRealTime;

	public float RealPlayTimeInteracting => realPlayTimeInteracting;

	public void GameInfoOnGUI()
	{
		if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseMove || Event.current.type == EventType.KeyDown)
		{
			lastInputRealTime = Time.realtimeSinceStartup;
		}
	}

	public void GameInfoUpdate()
	{
		if (Time.realtimeSinceStartup < lastInputRealTime + 90f && Find.MainTabsRoot.OpenTab != MainButtonDefOf.Menu && Current.ProgramState == ProgramState.Playing && !Find.WindowStack.IsOpen<Dialog_Options>())
		{
			realPlayTimeInteracting += RealTime.realDeltaTime;
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref realPlayTimeInteracting, "realPlayTimeInteracting", 0f);
		Scribe_Values.Look(ref permadeathMode, "permadeathMode", defaultValue: false);
		Scribe_Values.Look(ref permadeathModeUniqueName, "permadeathModeUniqueName");
		Scribe_Values.Look(ref startingTile, "startingTile", PlanetTile.Invalid);
		Scribe_Collections.Look(ref startingAndOptionalPawns, "startingAndOptionalPawns", LookMode.Reference);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		for (int num = startingAndOptionalPawns.Count - 1; num >= 0; num--)
		{
			if (startingAndOptionalPawns[num] == null)
			{
				startingAndOptionalPawns.RemoveAt(num);
			}
		}
	}
}
