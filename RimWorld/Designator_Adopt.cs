using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Adopt : Designator
{
	private int lastDesignateFrame = -1;

	private static StringBuilder _tmpBuilder = new StringBuilder();

	protected override bool DoTooltip => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_Adopt()
	{
		defaultLabel = "DesignatorAdopt".Translate();
		defaultDesc = "DesignatorAdoptDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Adopt");
		soundSucceeded = SoundDefOf.Designate_Adopt;
		showReverseDesignatorDisabledReason = true;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (c.Fogged(base.Map))
		{
			return false;
		}
		if (!c.GetThingList(base.Map).Any((Thing t) => CanDesignateThing(t).Accepted))
		{
			return false;
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (CanDesignateThing(thingList[i]).Accepted)
			{
				DesignateThing(thingList[i]);
			}
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!(t is Pawn { IsShambler: false }))
		{
			return false;
		}
		_tmpBuilder.Clear();
		if (t.Faction != Faction.OfPlayer && t.AdoptableBy(Faction.OfPlayer, _tmpBuilder))
		{
			return true;
		}
		return _tmpBuilder.ToString();
	}

	public override void DesignateThing(Thing t)
	{
		t.SetFaction(Faction.OfPlayer);
		Pawn pawn = (Pawn)t;
		Building_Bed building_Bed = pawn.CurrentBed();
		if (RestUtility.CanUseBedNow(building_Bed, pawn, checkSocialProperness: false))
		{
			building_Bed.TryGetComp<CompAssignableToPawn_Bed>().TryAssignPawn(pawn);
		}
		if (lastDesignateFrame < RealTime.frameCount)
		{
			TaggedString text = "LetterTextAdopted".Translate(pawn.Named("BABY"));
			if (pawn.babyNamingDeadline >= Find.TickManager.TicksGame)
			{
				text += "\n\n" + "LetterPartNameBabyAdopt".Translate(pawn, 60000.ToStringTicksToPeriod());
			}
			ChoiceLetter_BabyBirth choiceLetter_BabyBirth = (ChoiceLetter_BabyBirth)LetterMaker.MakeLetter("LetterLabelAdopted".Translate(pawn.Named("BABY")), text, LetterDefOf.BabyBirth, pawn);
			choiceLetter_BabyBirth.Start();
			Find.LetterStack.ReceiveLetter(choiceLetter_BabyBirth);
		}
		lastDesignateFrame = RealTime.frameCount;
		foreach (IntVec3 item in t.OccupiedRect())
		{
			FleckMaker.ThrowMetaPuffs(new TargetInfo(item, base.Map));
		}
	}
}
