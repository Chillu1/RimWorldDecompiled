using System.Text;
using Verse;

namespace RimWorld;

public class CompFloorEtching : CompFloorEtchingRambling
{
	private Rot4 direction;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (respawningAfterLoad)
		{
			return;
		}
		LabyrinthMapComponent component = parent.Map.GetComponent<LabyrinthMapComponent>();
		if (component != null)
		{
			float angleFlat = (component.labyrinthObelisk.Position - parent.Position).AngleFlat;
			if (angleFlat >= 315f || angleFlat < 45f)
			{
				direction = Rot4.North;
			}
			else if (angleFlat >= 45f && angleFlat < 135f)
			{
				direction = Rot4.East;
			}
			else if (angleFlat >= 135f && angleFlat < 225f)
			{
				direction = Rot4.South;
			}
			else if (angleFlat >= 225f && angleFlat < 315f)
			{
				direction = Rot4.West;
			}
		}
	}

	protected override void OnInteracted(Pawn caster)
	{
		deciphered = true;
		GenerateMessage();
		TaggedString label = "LetterLabelFloorEtchings".Translate();
		TaggedString text = "LetterFloorEtchings".Translate(caster.Named("PAWN"), message.Named("RAMBLINGS"), direction.ToStringHuman().ToLower().Named("DIRECTION"));
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, parent);
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (deciphered)
		{
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.Append(string.Format("{0}: {1}", "FloorEtchingsInspectorExit".Translate(), direction.ToStringHuman()));
		}
		else
		{
			stringBuilder.Append(base.CompInspectStringExtra());
		}
		return stringBuilder.ToString();
	}

	public override string CompTipStringExtra()
	{
		if (!deciphered)
		{
			return "";
		}
		return string.Format("{0}: {1}", "FloorEtchingsInspectorExit".Translate(), direction.ToStringHuman());
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref direction, "direction");
	}
}
