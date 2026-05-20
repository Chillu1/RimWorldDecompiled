using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class CompEquippable : ThingComp, IVerbOwner
{
	public VerbTracker verbTracker;

	protected Pawn Holder => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;

	public List<Verb> AllVerbs => verbTracker.AllVerbs;

	public Verb PrimaryVerb => verbTracker.PrimaryVerb;

	public VerbTracker VerbTracker => verbTracker;

	public List<VerbProperties> VerbProperties => parent.def.Verbs;

	public List<Tool> Tools => parent.def.tools;

	Thing IVerbOwner.ConstantCaster => null;

	ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Weapon;

	public CompEquippable()
	{
		verbTracker = new VerbTracker(this);
	}

	public IEnumerable<Command> GetVerbsCommands()
	{
		return verbTracker.GetVerbsCommands();
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		if (Holder != null && Holder.equipment != null && Holder.equipment.Primary == parent)
		{
			Holder.equipment.Notify_PrimaryDestroyed();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
	}

	public override void CompTick()
	{
		verbTracker.VerbsTick();
	}

	public override void Notify_Unequipped(Pawn p)
	{
		List<Verb> allVerbs = AllVerbs;
		for (int i = 0; i < allVerbs.Count; i++)
		{
			allVerbs[i].Notify_EquipmentLost();
		}
	}

	public virtual IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
	{
		return Enumerable.Empty<Gizmo>();
	}

	string IVerbOwner.UniqueVerbOwnerID()
	{
		return "CompEquippable_" + parent.ThingID;
	}

	bool IVerbOwner.VerbsStillUsableBy(Pawn p)
	{
		if (parent is Apparel item)
		{
			return p.apparel.WornApparel.Contains(item);
		}
		return p.equipment.AllEquipmentListForReading.Contains(parent);
	}
}
