using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class HostilityResponseModeUtility
{
	private static readonly Texture2D IgnoreIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Ignore");

	private static readonly Texture2D AttackIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Attack");

	private static readonly Texture2D FleeIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Flee");

	private static readonly Color IconColor = new Color(0.84f, 0.84f, 0.84f);

	public static Texture2D GetIcon(this HostilityResponseMode response)
	{
		return response switch
		{
			HostilityResponseMode.Ignore => IgnoreIcon, 
			HostilityResponseMode.Attack => AttackIcon, 
			HostilityResponseMode.Flee => FleeIcon, 
			_ => BaseContent.BadTex, 
		};
	}

	public static HostilityResponseMode GetNextResponse(Pawn pawn)
	{
		switch (pawn.playerSettings.hostilityResponse)
		{
		case HostilityResponseMode.Ignore:
			if (pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return HostilityResponseMode.Flee;
			}
			return HostilityResponseMode.Attack;
		case HostilityResponseMode.Attack:
			return HostilityResponseMode.Flee;
		case HostilityResponseMode.Flee:
			return HostilityResponseMode.Ignore;
		default:
			return HostilityResponseMode.Ignore;
		}
	}

	public static string GetLabel(this HostilityResponseMode response)
	{
		return ("HostilityResponseMode_" + response).Translate();
	}

	public static void DrawResponseButton(Rect rect, Pawn pawn, bool paintable)
	{
		Widgets.Dropdown(rect, pawn, IconColor, DrawResponseButton_GetResponse, DrawResponseButton_GenerateMenu, null, pawn.playerSettings.hostilityResponse.GetIcon(), null, null, delegate
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HostilityResponse, KnowledgeAmount.SpecificInteraction);
		}, paintable, 4f);
		UIHighlighter.HighlightOpportunity(rect, "HostilityResponse");
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, "HostilityReponseTip".Translate() + "\n\n" + "HostilityResponseCurrentMode".Translate() + ": " + pawn.playerSettings.hostilityResponse.GetLabel());
		}
	}

	private static HostilityResponseMode DrawResponseButton_GetResponse(Pawn pawn)
	{
		return pawn.playerSettings.hostilityResponse;
	}

	private static IEnumerable<Widgets.DropdownMenuElement<HostilityResponseMode>> DrawResponseButton_GenerateMenu(Pawn p)
	{
		foreach (HostilityResponseMode response in Enum.GetValues(typeof(HostilityResponseMode)))
		{
			if (response != HostilityResponseMode.Attack || !p.WorkTagIsDisabled(WorkTags.Violent))
			{
				yield return new Widgets.DropdownMenuElement<HostilityResponseMode>
				{
					option = new FloatMenuOption(response.GetLabel(), delegate
					{
						p.playerSettings.hostilityResponse = response;
					}, response.GetIcon(), Color.white),
					payload = response
				};
			}
		}
	}
}
