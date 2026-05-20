using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class HediffWithParents : HediffWithComps
{
	private Pawn father;

	private Pawn mother;

	public GeneSet geneSet;

	public Pawn Father => father;

	public Pawn Mother => mother;

	public void SetParents(Pawn mother, Pawn father, GeneSet geneSet)
	{
		this.mother = mother;
		this.father = father;
		this.geneSet = geneSet;
		Find.WorldPawns.AddPreservedPawnHediff(mother, this);
		Find.WorldPawns.AddPreservedPawnHediff(father, this);
	}

	public override void PreRemoved()
	{
		base.PreRemoved();
		Find.WorldPawns.RemovePreservedPawnHediff(mother, this);
		Find.WorldPawns.RemovePreservedPawnHediff(father, this);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		if (geneSet == null)
		{
			yield break;
		}
		foreach (StatDrawEntry item2 in geneSet.SpecialDisplayStats())
		{
			yield return item2;
		}
	}

	public override void CopyFrom(Hediff other)
	{
		base.CopyFrom(other);
		if (other is HediffWithParents { Mother: var pawn } hediffWithParents)
		{
			if (hediffWithParents.Mother == hediffWithParents.pawn)
			{
				pawn = base.pawn;
			}
			SetParents(pawn, hediffWithParents.Father, hediffWithParents.geneSet?.Copy());
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (ModsConfig.BiotechActive && !pawn.Drafted)
		{
			yield return new Command_Action
			{
				defaultLabel = "InspectBabyGenes".Translate() + "...",
				defaultDesc = "InspectGenesHediffDesc".Translate(),
				icon = GeneSetHolderBase.GeneticInfoTex.Texture,
				action = delegate
				{
					InspectPaneUtility.OpenTab(typeof(ITab_GenesPregnancy));
				}
			};
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref father, "father", saveDestroyedThings: true);
		Scribe_References.Look(ref mother, "mother", saveDestroyedThings: true);
		Scribe_Deep.Look(ref geneSet, "geneSet");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			Find.WorldPawns.AddPreservedPawnHediff(mother, this);
			Find.WorldPawns.AddPreservedPawnHediff(father, this);
		}
	}
}
