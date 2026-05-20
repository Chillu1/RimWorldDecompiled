using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompMechRelay : ThingComp
{
	public const int DestabilizationTicks = 7500;

	private const int CrashFlecksPerCell = 1;

	private bool active = true;

	public bool inert;

	private int destabilizationTick = -1;

	public CompProperties_MechRelay Props => (CompProperties_MechRelay)props;

	public int DestabilizationTick => destabilizationTick;

	public void Deactivate()
	{
		if (active && !inert)
		{
			active = false;
			if (!Props.destabilizationMessage.NullOrEmpty())
			{
				Messages.Message(Props.destabilizationMessage, parent, MessageTypeDefOf.PositiveEvent);
			}
			destabilizationTick = Find.TickManager.TicksGame + 7500;
		}
	}

	public override void CompTick()
	{
		if (inert || destabilizationTick <= 0 || Find.TickManager.TicksGame < destabilizationTick)
		{
			return;
		}
		destabilizationTick = -1;
		Map map = parent.Map;
		IntVec3 position = parent.Position;
		parent.Destroy();
		if (Props.crashedThingDef != null)
		{
			Thing thing = ThingMaker.MakeThing(Props.crashedThingDef);
			GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Direct);
			CellRect cellRect = parent.OccupiedRect();
			for (int i = 0; i < cellRect.Area; i++)
			{
				FleckMaker.ThrowDustPuff(cellRect.RandomVector3.WithY(AltitudeLayer.MoteLow.AltitudeFor()), map, 2f);
			}
			if (thing.Faction != parent.Faction)
			{
				thing.SetFaction(parent.Faction);
			}
			if (!Props.crashedLetterLabel.NullOrEmpty())
			{
				Find.LetterStack.ReceiveLetter(Props.crashedLetterLabel, Props.crashedLetterText, LetterDefOf.NeutralEvent, thing);
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		if (inert)
		{
			return null;
		}
		if (active && !Props.activeInspectString.NullOrEmpty())
		{
			return Props.activeInspectString;
		}
		if (destabilizationTick > 0 && !Props.destabilizationInspectString.NullOrEmpty())
		{
			return Props.destabilizationInspectString + ": " + (destabilizationTick - Find.TickManager.TicksGame).ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
		}
		return base.CompInspectStringExtra();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (!DebugSettings.ShowDevGizmos || inert)
		{
			yield break;
		}
		if (active)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Deactivate",
				action = Deactivate
			};
		}
		else if (destabilizationTick > 0)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Destabilize now",
				action = delegate
				{
					destabilizationTick = Find.TickManager.TicksGame;
				}
			};
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref active, "active", defaultValue: true);
		Scribe_Values.Look(ref inert, "inert", defaultValue: false);
		Scribe_Values.Look(ref destabilizationTick, "destabilizationTick", -1);
	}
}
