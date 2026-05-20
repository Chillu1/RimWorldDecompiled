using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Building_MusicalInstrument : Building
{
	private Pawn currentPlayer;

	private Sustainer soundPlaying;

	public bool IsBeingPlayed => currentPlayer != null;

	public FloatRange SoundRange
	{
		get
		{
			if (soundPlaying == null)
			{
				return FloatRange.Zero;
			}
			if (soundPlaying.def.subSounds.NullOrEmpty())
			{
				return FloatRange.Zero;
			}
			return soundPlaying.def.subSounds.First().distRange;
		}
	}

	public static bool IsAffectedByInstrument(ThingDef instrumentDef, IntVec3 instrumentPos, IntVec3 pawnPos, Map map)
	{
		if (instrumentPos.DistanceTo(pawnPos) < instrumentDef.building.instrumentRange)
		{
			return instrumentPos.GetRoom(map) == pawnPos.GetRoom(map);
		}
		return false;
	}

	public void StartPlaying(Pawn player)
	{
		if (ModLister.CheckRoyaltyOrIdeology("Musical instrument"))
		{
			currentPlayer = player;
		}
	}

	protected override void Tick()
	{
		base.Tick();
		if (currentPlayer != null)
		{
			if (def.soundPlayInstrument != null && soundPlaying == null)
			{
				soundPlaying = def.soundPlayInstrument.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(base.Position, base.Map), MaintenanceType.PerTick));
			}
		}
		else
		{
			soundPlaying = null;
		}
		soundPlaying?.Maintain();
	}

	public void StopPlaying()
	{
		currentPlayer = null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref currentPlayer, "currentPlayer");
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (!ModLister.CheckRoyaltyOrIdeology("Musical instrument"))
		{
			yield break;
		}
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Toggle is playing";
			command_Action.action = delegate
			{
				currentPlayer = ((currentPlayer == null) ? PawnsFinder.AllMaps_FreeColonists.FirstOrDefault() : null);
			};
			yield return command_Action;
		}
	}
}
