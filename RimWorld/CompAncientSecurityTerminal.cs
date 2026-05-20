using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompAncientSecurityTerminal : CompHackable
{
	private new CompProperties_AncientSecurityTerminalHackable Props => (CompProperties_AncientSecurityTerminalHackable)props;

	protected override void OnHacked(Pawn hacker = null, bool suppressMessages = false)
	{
		base.OnHacked(hacker, suppressMessages);
		CompHackable comp2;
		List<Building> list = (from thing in parent.Map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.AncientBlastDoor).Concat(parent.Map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.Turret_AncientArmoredTurret))
			where thing.TryGetComp<CompHackable>(out comp2) && !comp2.IsHacked
			select thing).ToList();
		if (list.Empty())
		{
			if (hacker != null && !suppressMessages)
			{
				Messages.Message(Props.messageNoValidTarget.Formatted(hacker.Named("HACKER")), hacker, MessageTypeDefOf.NeutralEvent);
			}
			return;
		}
		Building building = list.RandomElementByWeight((Building x) => (x.def != ThingDefOf.Turret_AncientArmoredTurret) ? 0.25f : 0.75f);
		CompHackable comp = building.GetComp<CompHackable>();
		bool flag = false;
		if (comp.parent is Building_Turret && Rand.Chance(0.5f) && hacker != null)
		{
			flag = true;
			comp.parent.SetFaction(hacker.Faction);
			parent.Map.fogGrid.Unfog(comp.parent.Position);
		}
		else
		{
			comp.Hack(comp.defence, hacker, suppressMessages: true);
		}
		if (!(hacker == null || suppressMessages))
		{
			TaggedString label;
			TaggedString text;
			if (flag)
			{
				label = Props.letterTurretHackedLabel.Formatted(hacker.Named("HACKER"));
				text = Props.letterTurretHackedText.Formatted(hacker.Named("HACKER"));
			}
			else if (building.def == ThingDefOf.AncientBlastDoor)
			{
				label = Props.letterDoorOpenedLabel.Formatted(hacker.Named("HACKER"));
				text = Props.letterDoorOpenedText.Formatted(hacker.Named("HACKER"));
			}
			else
			{
				label = Props.letterTurretDisabledLabel.Formatted(hacker.Named("HACKER"));
				text = Props.letterTurretDisabledText.Formatted(hacker.Named("HACKER"));
			}
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, building);
		}
	}
}
