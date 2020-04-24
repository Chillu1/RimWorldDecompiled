using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class CompEquippable : ThingComp, IVerbOwner
	{
		public VerbTracker verbTracker;

		private Pawn Holder => PrimaryVerb.CasterPawn;

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
			base.CompTick();
			verbTracker.VerbsTick();
		}

		public void Notify_EquipmentLost()
		{
			List<Verb> allVerbs = AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				allVerbs[i].Notify_EquipmentLost();
			}
		}

		string IVerbOwner.UniqueVerbOwnerID()
		{
			return "CompEquippable_" + parent.ThingID;
		}

		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			Apparel apparel = parent as Apparel;
			if (apparel != null)
			{
				return p.apparel.WornApparel.Contains(apparel);
			}
			return p.equipment.AllEquipmentListForReading.Contains(parent);
		}
	}
}
