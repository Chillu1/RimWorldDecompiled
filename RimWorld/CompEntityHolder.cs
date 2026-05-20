using Verse;

namespace RimWorld;

public abstract class CompEntityHolder : ThingComp
{
	public virtual float ContainmentStrength => parent.GetStatValue(StatDefOf.ContainmentStrength, applyPostProcess: true, 15);

	public CompProperties_EntityHolder Props => (CompProperties_EntityHolder)props;

	protected Building_HoldingPlatform HoldingPlatform => (Building_HoldingPlatform)parent;

	public abstract bool Available { get; }

	public abstract Pawn HeldPawn { get; }

	public abstract ThingOwner Container { get; }

	public abstract void EjectContents();

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		float statValue = parent.GetStatValue(StatDefOf.ContainmentStrength);
		text += $"{StatDefOf.ContainmentStrength.LabelCap}: {statValue:F0}";
		if (!parent.Spawned)
		{
			return text;
		}
		if (parent.IsOutside())
		{
			text += string.Format(" ({0})", "Outdoors".Translate());
		}
		else if (StatWorker_ContainmentStrength.AnyDoorForcedOpen(parent.GetRoom()))
		{
			text += string.Format(" ({0})", "Stat_ContainmentStrength_DoorForcedOpen".Translate());
		}
		return text;
	}
}
