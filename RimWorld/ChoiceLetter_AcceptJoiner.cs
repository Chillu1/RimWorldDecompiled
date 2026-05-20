using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ChoiceLetter_AcceptJoiner : ChoiceLetter
{
	public string signalAccept;

	public string signalReject;

	public Map overrideMap;

	public override bool CanDismissWithRightClick => false;

	public override bool CanShowInLetterStack
	{
		get
		{
			if (base.CanShowInLetterStack && quest != null)
			{
				if (quest.State != QuestState.Ongoing)
				{
					return quest.State == QuestState.NotYetAccepted;
				}
				return true;
			}
			return false;
		}
	}

	public Map MapToUse => overrideMap ?? lookTargets.PrimaryTarget.Map ?? Find.AnyPlayerHomeMap;

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			if (base.ArchivedOnly)
			{
				yield return base.Option_Close;
				yield break;
			}
			DiaOption diaOption = new DiaOption("AcceptButton".Translate());
			DiaOption optionReject = new DiaOption("RejectLetter".Translate());
			diaOption.action = delegate
			{
				Find.SignalManager.SendSignal(new Signal(signalAccept));
				Find.LetterStack.RemoveLetter(this);
			};
			diaOption.resolveTree = true;
			optionReject.action = delegate
			{
				Find.SignalManager.SendSignal(new Signal(signalReject));
				Find.LetterStack.RemoveLetter(this);
			};
			optionReject.resolveTree = true;
			Map mapToUse = MapToUse;
			if (mapToUse == null)
			{
				diaOption.Disable("CannotAcceptQuestNoMap".Translate());
			}
			else if (mapToUse.Tile.LayerDef.isSpace)
			{
				PlanetLayerDef layerDef = mapToUse.Tile.LayerDef;
				diaOption.Disable("CannotAcceptQuestFromLayer".Translate(layerDef.gerundLabel.Named("GERUND"), layerDef.Named("LAYER")));
			}
			yield return diaOption;
			yield return optionReject;
			if (lookTargets.IsValid())
			{
				yield return base.Option_JumpToLocationAndPostpone;
			}
			yield return base.Option_Postpone;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref signalAccept, "signalAccept");
		Scribe_Values.Look(ref signalReject, "signalReject");
		Scribe_References.Look(ref overrideMap, "overrideMap");
	}
}
