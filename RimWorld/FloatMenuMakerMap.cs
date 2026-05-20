using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public static class FloatMenuMakerMap
{
	private static List<FloatMenuOptionProvider> providers;

	public static FloatMenuOptionProvider currentProvider;

	public static Pawn makingFor;

	public static void Init()
	{
		providers = new List<FloatMenuOptionProvider>();
		foreach (Type item in typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract())
		{
			providers.Add((FloatMenuOptionProvider)Activator.CreateInstance(item));
		}
	}

	public static List<FloatMenuOption> GetOptions(List<Pawn> selectedPawns, Vector3 clickPos, out FloatMenuContext context)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		context = null;
		if (!clickPos.InBounds(Find.CurrentMap))
		{
			return list;
		}
		context = new FloatMenuContext(selectedPawns, clickPos, Find.CurrentMap);
		if (!context.allSelectedPawns.Any())
		{
			return list;
		}
		if (!context.ClickedCell.IsValid || !context.ClickedCell.InBounds(Find.CurrentMap))
		{
			return list;
		}
		if (!context.IsMultiselect)
		{
			AcceptanceReport acceptanceReport = ShouldGenerateFloatMenuForPawn(context.FirstSelectedPawn);
			if (!acceptanceReport.Accepted)
			{
				if (!acceptanceReport.Reason.NullOrEmpty())
				{
					Messages.Message(acceptanceReport.Reason, context.FirstSelectedPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return list;
			}
		}
		else
		{
			context.allSelectedPawns.RemoveAll((Pawn selectedPawn) => !ShouldGenerateFloatMenuForPawn(selectedPawn));
			if (!context.allSelectedPawns.Any())
			{
				return list;
			}
		}
		if (!context.IsMultiselect)
		{
			makingFor = context.FirstSelectedPawn;
		}
		GetProviderOptions(context, list);
		makingFor = null;
		return list;
	}

	private static void GetProviderOptions(FloatMenuContext context, List<FloatMenuOption> options)
	{
		foreach (FloatMenuOptionProvider provider in providers)
		{
			try
			{
				currentProvider = provider;
				if (!context.ValidSelectedPawns.Any() || !provider.Applies(context))
				{
					continue;
				}
				options.AddRange(provider.GetOptions(context));
				foreach (Thing clickedThing in context.ClickedThings)
				{
					if (!provider.TargetThingValid(clickedThing, context))
					{
						continue;
					}
					Thing thing = clickedThing;
					if (thing.TryGetComp(out CompSelectProxy comp) && comp.thingToSelect != null)
					{
						thing = comp.thingToSelect;
					}
					foreach (FloatMenuOption item in provider.GetOptionsFor(thing, context))
					{
						FloatMenuOption floatMenuOption = item;
						if (floatMenuOption.iconThing == null)
						{
							floatMenuOption.iconThing = thing;
						}
						item.targetsDespawned = !thing.Spawned;
						options.Add(item);
					}
				}
				foreach (Pawn clickedPawn in context.ClickedPawns)
				{
					if (!provider.TargetPawnValid(clickedPawn, context))
					{
						continue;
					}
					foreach (FloatMenuOption item2 in provider.GetOptionsFor(clickedPawn, context))
					{
						FloatMenuOption floatMenuOption = item2;
						if (floatMenuOption.iconThing == null)
						{
							floatMenuOption.iconThing = clickedPawn;
						}
						item2.targetsDespawned = !clickedPawn.Spawned;
						options.Add(item2);
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error($"Error in FloatMenuWorker {provider.GetType().Name}: {arg}");
			}
		}
		currentProvider = null;
	}

	public static AcceptanceReport ShouldGenerateFloatMenuForPawn(Pawn pawn)
	{
		if (pawn.Map != Find.CurrentMap)
		{
			return false;
		}
		if (pawn.Downed)
		{
			return "IsIncapped".Translate(pawn.LabelCap, pawn);
		}
		if (ModsConfig.BiotechActive && pawn.Deathresting)
		{
			return "IsDeathresting".Translate(pawn.Named("PAWN"));
		}
		Lord lord = pawn.GetLord();
		if (lord != null)
		{
			AcceptanceReport result = lord.AllowsFloatMenu(pawn);
			if (!result.Accepted)
			{
				return result;
			}
		}
		return true;
	}

	public static FloatMenuOption GetAutoTakeOption(List<FloatMenuOption> options)
	{
		bool flag = true;
		FloatMenuOption floatMenuOption = null;
		foreach (FloatMenuOption option in options)
		{
			if (option.Disabled || !option.autoTakeable)
			{
				flag = false;
				break;
			}
			if (floatMenuOption == null || option.autoTakeablePriority > floatMenuOption.autoTakeablePriority)
			{
				floatMenuOption = option;
			}
		}
		if (!flag || floatMenuOption == null)
		{
			return null;
		}
		return floatMenuOption;
	}
}
