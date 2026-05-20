using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

public class Page_ScenarioEditor : Page
{
	private Scenario curScen;

	private Vector2 infoScrollPosition = Vector2.zero;

	private string seed;

	private bool seedIsValid = true;

	private bool editMode;

	private bool needsSave = true;

	private int initialHash;

	public bool ScenarioEdited
	{
		get
		{
			if (editMode)
			{
				return curScen.GetHashCode() != initialHash;
			}
			return false;
		}
	}

	public override string PageTitle => "ScenarioEditor".Translate() + (ScenarioEdited ? "*" : "");

	public Scenario EditingScenario => curScen;

	public Page_ScenarioEditor(Scenario scen)
	{
		if (scen != null)
		{
			curScen = scen;
			seedIsValid = false;
		}
		else
		{
			RandomizeSeedAndScenario();
		}
		initialHash = curScen.GetHashCode();
	}

	public override void PreOpen()
	{
		base.PreOpen();
		infoScrollPosition = Vector2.zero;
		needsSave = true;
	}

	public override void DoWindowContents(Rect rect)
	{
		DrawPageTitle(rect);
		Rect mainRect = GetMainRect(rect);
		Widgets.BeginGroup(mainRect);
		Rect rect2 = new Rect(0f, 0f, mainRect.width * 0.35f, mainRect.height).Rounded();
		DoConfigControls(rect2);
		Rect rect3 = new Rect(rect2.xMax + 17f, 0f, mainRect.width - rect2.width - 17f, mainRect.height).Rounded();
		if (!editMode)
		{
			ScenarioUI.DrawScenarioInfo(rect3, curScen, ref infoScrollPosition);
		}
		else
		{
			ScenarioUI.DrawScenarioEditInterface(rect3, curScen, ref infoScrollPosition);
		}
		Widgets.EndGroup();
		DoBottomButtons(rect);
	}

	private void RandomizeSeedAndScenario()
	{
		seed = GenText.RandomSeedString();
		curScen = ScenarioMaker.GenerateNewRandomScenario(seed);
	}

	private void DoConfigControls(Rect rect)
	{
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.ColumnWidth = 200f;
		listing_Standard.Begin(rect);
		if (listing_Standard.ButtonText("Load".Translate()))
		{
			Find.WindowStack.Add(new Dialog_ScenarioList_Load(delegate(Scenario loadedScen)
			{
				curScen = loadedScen;
				seedIsValid = false;
			}));
		}
		if (listing_Standard.ButtonText("Save".Translate()) && CheckAllPartsCompatible(curScen))
		{
			Find.WindowStack.Add(new Dialog_ScenarioList_Save(curScen));
		}
		if (listing_Standard.ButtonText("RandomizeSeed".Translate()))
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			RandomizeSeedAndScenario();
			seedIsValid = true;
		}
		if (seedIsValid)
		{
			listing_Standard.Label("Seed".Translate().CapitalizeFirst());
			string text = listing_Standard.TextEntry(seed);
			if (text != seed)
			{
				seed = text;
				curScen = ScenarioMaker.GenerateNewRandomScenario(seed);
			}
		}
		else
		{
			listing_Standard.Gap(Text.LineHeight + Text.LineHeight + 2f);
		}
		listing_Standard.CheckboxLabeled("EditMode".Translate().CapitalizeFirst(), ref editMode);
		if (editMode)
		{
			seedIsValid = false;
			if (listing_Standard.ButtonText("AddPart".Translate()))
			{
				OpenAddScenPartMenu();
			}
			if (SteamManager.Initialized && (curScen.Category == ScenarioCategory.CustomLocal || curScen.Category == ScenarioCategory.SteamWorkshop) && listing_Standard.ButtonText(Workshop.UploadButtonLabel(curScen.GetPublishedFileId())) && CheckAllPartsCompatible(curScen))
			{
				AcceptanceReport acceptanceReport = curScen.TryUploadReport();
				if (!acceptanceReport.Accepted)
				{
					Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSteamWorkshopUpload".Translate(), delegate
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmContentAuthor".Translate(), delegate
						{
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
							Workshop.Upload(curScen);
						}, destructive: true));
					}, destructive: true));
				}
			}
		}
		listing_Standard.End();
	}

	private static bool CheckAllPartsCompatible(Scenario scen)
	{
		foreach (ScenPart allPart in scen.AllParts)
		{
			int num = 0;
			foreach (ScenPart allPart2 in scen.AllParts)
			{
				if (allPart2.def == allPart.def)
				{
					num++;
				}
				if (num > allPart.def.maxUses)
				{
					Messages.Message("TooMany".Translate(allPart.def.maxUses) + ": " + allPart.def.label, MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				if (allPart != allPart2 && !allPart.CanCoexistWith(allPart2))
				{
					Messages.Message("Incompatible".Translate() + ": " + allPart.def.label + ", " + allPart2.def.label, MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
			}
		}
		return true;
	}

	private void OpenAddScenPartMenu()
	{
		FloatMenuUtility.MakeMenu(from p in ScenarioMaker.AddableParts(curScen)
			where p.PlayerAddRemovable
			orderby p.label
			select p, (ScenPartDef p) => p.LabelCap, (ScenPartDef p) => delegate
		{
			AddScenPart(p);
		});
	}

	private void AddScenPart(ScenPartDef def)
	{
		ScenPart scenPart = ScenarioMaker.MakeScenPart(def);
		scenPart.Randomize();
		curScen.parts.Add(scenPart);
	}

	protected override bool CanDoNext()
	{
		if (!base.CanDoNext())
		{
			return false;
		}
		if (curScen == null)
		{
			return false;
		}
		if (!CheckAllPartsCompatible(curScen))
		{
			return false;
		}
		if (!ScenarioEdited || !needsSave)
		{
			Page_SelectScenario.BeginScenarioConfiguration(curScen, this);
			return true;
		}
		Find.WindowStack.Add(new Dialog_MessageBox("ScenarioChangedSavePrompt".Translate(), "Yes".Translate(), delegate
		{
			Find.WindowStack.Add(new Dialog_ScenarioList_Save(curScen, delegate
			{
				Page_SelectScenario.BeginScenarioConfiguration(curScen, this);
				needsSave = false;
				DoNext();
			}));
		}, "No".Translate(), delegate
		{
			Page_SelectScenario.BeginScenarioConfiguration(curScen, this);
			needsSave = false;
			DoNext();
		}, null, buttonADestructive: false, null, delegate
		{
		}));
		return false;
	}
}
