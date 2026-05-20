using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class CompFloorEtchingRambling : CompInteractable
{
	public bool deciphered;

	public TaggedString message;

	private static readonly List<Pawn> PossibleCorpses = new List<Pawn>();

	protected override void OnInteracted(Pawn caster)
	{
		deciphered = true;
		GenerateMessage();
		TaggedString label = "LetterLabelFloorEtchingRamblings".Translate();
		TaggedString text = "LetterFloorEtchingsRamblings".Translate(caster.Named("PAWN"));
		text += $"\n\n{message}";
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, parent);
	}

	protected void GenerateMessage()
	{
		deciphered = true;
		GrammarRequest request = new GrammarRequest
		{
			Includes = { RulePackDefOf.LabyrinthRamblings }
		};
		TaleData_Pawn taleData_Pawn = null;
		foreach (Thing item in parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse))
		{
			if (item is Corpse { InnerPawn: var innerPawn } corpse && innerPawn.RaceProps.Humanlike && !innerPawn.IsColonist && corpse.timeOfDeath < parent.TickSpawned)
			{
				PossibleCorpses.Add(innerPawn);
			}
		}
		taleData_Pawn = ((!PossibleCorpses.Any()) ? TaleData_Pawn.GenerateRandom(humanLike: true) : TaleData_Pawn.GenerateFrom(PossibleCorpses.RandomElement()));
		foreach (Rule rule in taleData_Pawn.GetRules("PAWN", request.Constants))
		{
			request.Rules.Add(rule);
		}
		message = GrammarResolver.Resolve("r_root", request);
		PossibleCorpses.Clear();
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (deciphered)
		{
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.Append(message);
		}
		else
		{
			stringBuilder.Append(base.CompInspectStringExtra());
		}
		return stringBuilder.ToString();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!deciphered)
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Regen text",
				action = GenerateMessage
			};
		}
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (deciphered)
		{
			yield break;
		}
		foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
		{
			yield return item;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref deciphered, "deciphered", defaultValue: false);
		Scribe_Values.Look(ref message, "message");
	}
}
