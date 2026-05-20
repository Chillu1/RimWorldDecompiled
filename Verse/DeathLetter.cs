using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public class DeathLetter : ChoiceLetter
{
	protected DiaOption Option_ReadMore
	{
		get
		{
			GlobalTargetInfo target = lookTargets.TryGetPrimaryTarget();
			DiaOption diaOption = new DiaOption("ReadMore".Translate());
			diaOption.action = delegate
			{
				CameraJumper.TryJumpAndSelect(target);
				Find.LetterStack.RemoveLetter(this);
				InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Log));
			};
			diaOption.resolveTree = true;
			if (!target.IsValid)
			{
				diaOption.Disable(null);
			}
			return diaOption;
		}
	}

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			yield return base.Option_Close;
			if (lookTargets.IsValid())
			{
				yield return Option_ReadMore;
			}
			if (quest != null)
			{
				yield return Option_ViewInQuestsTab();
			}
		}
	}

	public override void OpenLetter()
	{
		Pawn targetPawn = lookTargets.TryGetPrimaryTarget().Thing as Pawn;
		TaggedString taggedString = base.Text;
		string text = (from entry in (from entry in Find.BattleLog.Battles.Where((Battle battle) => battle.Concerns(targetPawn)).SelectMany((Battle battle) => battle.Entries.Where((LogEntry entry) => entry.Concerns(targetPawn) && entry.ShowInCompactView()))
				orderby entry.Age
				select entry).Take(5).Reverse()
			select "  " + entry.ToGameStringFromPOV(null)).ToLineList();
		if (text.Length > 0)
		{
			taggedString = string.Format("{0}\n\n{1}\n{2}", taggedString, "LastEventsInLife".Translate(targetPawn.LabelDefinite(), targetPawn.Named("PAWN")).Resolve() + ":", text);
		}
		DiaNode diaNode = new DiaNode(taggedString);
		diaNode.options.AddRange(Choices);
		Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, relatedFaction, delayInteractivity: false, radioMode, title));
	}
}
