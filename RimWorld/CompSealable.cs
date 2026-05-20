using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompSealable : ThingComp
{
	public const string SealedSignal = "Sealed";

	private bool isSealed;

	private Texture2D cachedSealTex;

	private CompProperties_Sealable Props => (CompProperties_Sealable)props;

	private MapPortal Portal => (MapPortal)parent;

	private Texture2D SealTex => cachedSealTex ?? (cachedSealTex = ContentFinder<Texture2D>.Get(Props.sealTexPath));

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref isSealed, "isSealed", defaultValue: false);
	}

	public override AcceptanceReport CanEnterPortal()
	{
		if (isSealed)
		{
			return Props.cannotEnterLabel;
		}
		return true;
	}

	public void Seal()
	{
		if (!Portal.PocketMapExists)
		{
			Log.Error("Tried to seal portal but pocket map doesn't exist");
			return;
		}
		isSealed = true;
		PocketMapUtility.DestroyPocketMap(Portal.PocketMap);
		Portal.BroadcastCompSignal("Sealed");
		if (Props.destroyPortal)
		{
			Thing.allowDestroyNonDestroyable = true;
			Portal.Destroy(DestroyMode.Deconstruct);
			Thing.allowDestroyNonDestroyable = false;
		}
		else
		{
			Portal.DirtyMapMesh(Portal.Map);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Portal.PocketMapExists)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = Props.sealCommandLabel + "...",
			defaultDesc = Props.sealCommandDesc,
			icon = SealTex,
			action = delegate
			{
				Find.Targeter.BeginTargeting(TargetingParameters.ForColonist(), delegate(LocalTargetInfo target)
				{
					string text = "";
					List<Pawn> list = new List<Pawn>();
					List<Pawn> list2 = new List<Pawn>();
					foreach (Pawn allPawn in Portal.PocketMap.mapPawns.AllPawns)
					{
						if (allPawn.Faction == Faction.OfPlayer)
						{
							if (allPawn.RaceProps.Humanlike)
							{
								list.Add(allPawn);
							}
							else
							{
								list2.Add(allPawn);
							}
						}
					}
					if (list.Count > 0)
					{
						text = text + "\n\n" + ("Warning".Translate() + ": ").Colorize(ColorLibrary.RedReadable) + "PeopleWillBeLeftBehind".Translate().Resolve() + ":\n" + list.Select((Pawn p) => p.NameFullColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
					}
					if (list2.Count > 0)
					{
						text = text + "\n\n" + ("Warning".Translate() + ": ").Colorize(ColorLibrary.RedReadable) + "AnimalsWillBeLeftBehind".Translate().Resolve() + ":\n" + list2.Select((Pawn p) => p.NameFullColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
					}
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(Props.confirmSealText.Formatted(text), delegate
					{
						target.Pawn?.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Seal, Portal), JobTag.Misc);
					}));
				}, delegate(LocalTargetInfo target)
				{
					Pawn pawn = target.Pawn;
					if (pawn != null && pawn.IsColonistPlayerControlled)
					{
						GenDraw.DrawTargetHighlight(target);
					}
				}, (LocalTargetInfo target) => ValidateSealer(target).Accepted, null, null, SealTex, playSoundOnAction: true, delegate(LocalTargetInfo target)
				{
					AcceptanceReport acceptanceReport = ValidateSealer(target);
					Pawn pawn = target.Pawn;
					if (pawn != null && pawn.IsColonistPlayerControlled && !acceptanceReport.Accepted)
					{
						if (!acceptanceReport.Reason.NullOrEmpty())
						{
							Widgets.MouseAttachedLabel((Props.cannotSealLabel + ": " + acceptanceReport.Reason.CapitalizeFirst()).Colorize(ColorLibrary.RedReadable));
						}
						else
						{
							Widgets.MouseAttachedLabel(Props.cannotSealLabel);
						}
					}
				});
			}
		};
	}

	private AcceptanceReport ValidateSealer(LocalTargetInfo target)
	{
		if (!(target.Thing is Pawn pawn))
		{
			return false;
		}
		if (!pawn.CanReach(Portal, PathEndMode.Touch, Danger.Deadly))
		{
			return "NoPath".Translate();
		}
		if (pawn.Downed)
		{
			return "DownedLower".Translate();
		}
		return true;
	}

	public override string CompInspectStringExtra()
	{
		if (!isSealed)
		{
			return null;
		}
		return Props.sealedInspectString;
	}
}
