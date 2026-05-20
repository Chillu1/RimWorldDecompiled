using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_AddHediff : QuestPart
{
	public List<Pawn> pawns = new List<Pawn>();

	public List<BodyPartDef> partsToAffect;

	public string inSignal;

	public HediffDef hediffDef;

	public bool checkDiseaseContractChance;

	public bool addToHyperlinks;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			for (int i = 0; i < pawns.Count; i++)
			{
				yield return pawns[i];
			}
		}
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
	{
		get
		{
			foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
			{
				yield return hyperlink;
			}
			if (addToHyperlinks)
			{
				yield return new Dialog_InfoCard.Hyperlink(hediffDef);
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			if (!pawns[i].DestroyedOrNull() && (!checkDiseaseContractChance || Rand.Chance(pawns[i].health.immunity.DiseaseContractChanceFactor(hediffDef))))
			{
				HediffGiverUtility.TryApply(pawns[i], hediffDef, partsToAffect);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Collections.Look(ref partsToAffect, "partsToAffect", LookMode.Def);
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Defs.Look(ref hediffDef, "hediffDef");
		Scribe_Values.Look(ref checkDiseaseContractChance, "checkDiseaseContractChance", defaultValue: false);
		Scribe_Values.Look(ref addToHyperlinks, "addToHyperlinks", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		hediffDef = HediffDefOf.Anesthetic;
		pawns.Add(PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
