using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class StorageGroupUtility
{
	private static readonly Texture2D LinkTex = ContentFinder<Texture2D>.Get("UI/Commands/LinkStorageSettings");

	private static readonly Texture2D UnlinkTex = ContentFinder<Texture2D>.Get("UI/Commands/UnlinkStorageSettings");

	private static readonly Texture2D SelectLinkedTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllLinked");

	public static readonly Material GroupedMat = MaterialPool.MatFrom("UI/Overlays/SelectionBracket", ShaderDatabase.MetaOverlay, new Color(1f, 1f, 0f, 0.2f));

	private static List<IStorageGroupMember> tmpMembers = new List<IStorageGroupMember>();

	public static IEnumerable<Gizmo> StorageGroupMemberGizmos(IStorageGroupMember member)
	{
		bool flag = true;
		StorageGroup storageGroup = null;
		tmpMembers.Clear();
		foreach (object selectedObject in Find.Selector.SelectedObjects)
		{
			if (!(selectedObject is IStorageGroupMember storageGroupMember) || !(storageGroupMember.StorageGroupTag == member.StorageGroupTag) || storageGroupMember is Building_Storage { StorageTabVisible: false } || storageGroupMember.Map != member.Map)
			{
				continue;
			}
			tmpMembers.Add(storageGroupMember);
			if (flag)
			{
				if (storageGroup == null && member.Group != null)
				{
					storageGroup = member.Group;
				}
				if (storageGroup != member.Group)
				{
					flag = false;
				}
			}
		}
		if (storageGroup == null)
		{
			flag = false;
		}
		Command_Action command_Action = new Command_Action
		{
			defaultLabel = "LinkStorageSettings".Translate(),
			defaultDesc = "LinkStorageSettingsDesc".Translate(),
			icon = LinkTex,
			action = delegate
			{
				bool num = tmpMembers[0].Group != null;
				StorageGroup storageGroup2 = tmpMembers[0].Group ?? member.Map.storageGroups.NewGroup();
				if (!num)
				{
					storageGroup2.InitFrom(tmpMembers[0]);
				}
				foreach (IStorageGroupMember tmpMember in tmpMembers)
				{
					tmpMember.SetStorageGroup(storageGroup2);
				}
				if (tmpMembers.Count > 1)
				{
					Messages.Message("SettingsLinkedFor".Translate(tmpMembers.Count), null, MessageTypeDefOf.NeutralEvent, historical: false);
				}
				else
				{
					Messages.Message("SettingsLinkedForSingular".Translate(), null, MessageTypeDefOf.NeutralEvent, historical: false);
				}
			},
			onHover = delegate
			{
				if (tmpMembers.Any() && tmpMembers[0] is Thing thing)
				{
					TargetHighlighter.Highlight(thing);
				}
			}
		};
		if (tmpMembers.Count < 2)
		{
			command_Action.Disable("LinkStorageDisabledSelectTwo".Translate());
		}
		else if (flag)
		{
			command_Action.Disable("AlreadyLinked".Translate());
		}
		yield return command_Action;
		if (member.Group == null)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "UnlinkStorageSettings".Translate(),
			defaultDesc = "UnlinkStorageSettingsDesc".Translate(),
			icon = UnlinkTex,
			action = delegate
			{
				if (member.Group != null)
				{
					StorageSettings storeSettings = member.Group.GetStoreSettings();
					member.Group.RemoveMember(member);
					member.Group = null;
					if (member is Building_Storage building_Storage2)
					{
						building_Storage2.settings.CopyFrom(storeSettings);
					}
				}
				if (tmpMembers.Count > 1)
				{
					Messages.Message("SettingsUnlinkedFor".Translate(tmpMembers.Count), null, MessageTypeDefOf.NeutralEvent, historical: false);
				}
				else
				{
					Messages.Message("SettingsUnlinkedForSingular".Translate(), null, MessageTypeDefOf.NeutralEvent, historical: false);
				}
			}
		};
		yield return new Command_Action
		{
			defaultLabel = "SelectAllLinked".Translate(),
			defaultDesc = "SelectAllLinkedDesc".Translate(),
			icon = SelectLinkedTex,
			action = delegate
			{
				bool flag2 = false;
				foreach (IStorageGroupMember member2 in member.Group.members)
				{
					if (!Find.Selector.IsSelected(member2) && !(member2 is Thing { Destroyed: not false }))
					{
						Find.Selector.Select(member2, playSound: false);
						flag2 = true;
					}
				}
				if (flag2)
				{
					SoundDefOf.ThingSelected.PlayOneShotOnCamera();
				}
			}
		};
	}

	public static void DrawSelectionOverlaysFor(IStorageGroupMember member)
	{
		if (member.Group == null || SelectionDrawer.DrawnStorageGroupThisFrame(member.Group))
		{
			return;
		}
		foreach (IStorageGroupMember member2 in member.Group.members)
		{
			if (member2.DrawConnectionOverlay)
			{
				SelectionDrawer.DrawSelectionBracketFor(member2, GroupedMat);
			}
		}
		SelectionDrawer.Notify_DrawnStorageGroup(member.Group);
	}

	public static void SetStorageGroup(this IStorageGroupMember member, StorageGroup newGroup, bool removeIfEmpty = true)
	{
		if (member.Group == null || member.Group != newGroup)
		{
			if (member.Group != null)
			{
				StorageSettings storeSettings = member.Group.GetStoreSettings();
				member.Group.RemoveMember(member, removeIfEmpty);
				member.Group = null;
				member.StoreSettings.CopyFrom(storeSettings);
			}
			if (newGroup != null)
			{
				member.Group = newGroup;
				member.Group.members.Add(member);
			}
			if (member is IStoreSettingsParent storeSettingsParent)
			{
				storeSettingsParent.Notify_SettingsChanged();
			}
		}
	}

	public static bool IsHopper(this Thing thing)
	{
		if (thing?.def?.building == null)
		{
			return false;
		}
		return thing.def.building.isHopper;
	}
}
