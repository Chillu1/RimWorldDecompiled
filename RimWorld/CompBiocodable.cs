using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompBiocodable : ThingComp
{
	protected bool biocoded;

	protected string codedPawnLabel;

	protected Pawn codedPawn;

	public bool Biocoded => biocoded;

	public Pawn CodedPawn => codedPawn;

	public string CodedPawnLabel => codedPawnLabel;

	public CompProperties_Biocodable Props => (CompProperties_Biocodable)props;

	public virtual bool Biocodable => true;

	public static bool IsBiocoded(Thing thing)
	{
		return thing.TryGetComp<CompBiocodable>()?.Biocoded ?? false;
	}

	public static bool IsBiocodedFor(Thing thing, Pawn pawn)
	{
		CompBiocodable compBiocodable = thing.TryGetComp<CompBiocodable>();
		if (compBiocodable != null)
		{
			return compBiocodable.CodedPawn == pawn;
		}
		return false;
	}

	public virtual void CodeFor(Pawn p)
	{
		if (Biocodable)
		{
			biocoded = true;
			codedPawn = p;
			codedPawnLabel = p.Name.ToStringFull;
			OnCodedFor(p);
		}
	}

	protected virtual void OnCodedFor(Pawn p)
	{
	}

	public virtual void UnCode()
	{
		biocoded = false;
		codedPawn = null;
		codedPawnLabel = null;
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		if (Biocodable && Props.biocodeOnEquip)
		{
			CodeFor(pawn);
		}
	}

	public override string TransformLabel(string label)
	{
		if (!biocoded)
		{
			return label;
		}
		return "Biocoded".Translate(label, parent.def).Resolve();
	}

	public override string CompInspectStringExtra()
	{
		if (!biocoded)
		{
			return string.Empty;
		}
		return "CodedFor".Translate(codedPawnLabel.ApplyTag(TagType.Name)).Resolve();
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (biocoded)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Stat_Thing_Biocoded_Name".Translate(), codedPawnLabel, "Stat_Thing_Biocoded_Desc".Translate(), 1104);
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref biocoded, "biocoded", defaultValue: false);
		Scribe_Values.Look(ref codedPawnLabel, "biocodedPawnLabel");
		if (Scribe.mode == LoadSaveMode.Saving && codedPawn != null && codedPawn.Discarded)
		{
			codedPawn = null;
		}
		Scribe_References.Look(ref codedPawn, "codedPawn", saveDestroyedThings: true);
	}
}
