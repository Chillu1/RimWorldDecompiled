using System;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_Pawn_Social : ITab
{
	public const float Width = 540f;

	public override bool IsVisible
	{
		get
		{
			if (SelPawnForSocialInfo.RaceProps.IsFlesh)
			{
				if (ModsConfig.AnomalyActive)
				{
					return !SelPawnForSocialInfo.RaceProps.IsAnomalyEntity;
				}
				return true;
			}
			return false;
		}
	}

	private Pawn SelPawnForSocialInfo
	{
		get
		{
			if (SelPawn != null)
			{
				return SelPawn;
			}
			if (base.SelThing is Corpse corpse)
			{
				return corpse.InnerPawn;
			}
			throw new InvalidOperationException("Social tab on non-pawn non-corpse " + base.SelThing);
		}
	}

	public ITab_Pawn_Social()
	{
		size = new Vector2(540f, 510f);
		labelKey = "TabSocial";
		tutorTag = "Social";
	}

	protected override void FillTab()
	{
		SocialCardUtility.DrawSocialCard(new Rect(0f, 0f, size.x, size.y), SelPawnForSocialInfo);
	}
}
