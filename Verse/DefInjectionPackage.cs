using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RimWorld;
using RimWorld.IO;

namespace Verse;

public class DefInjectionPackage
{
	public class DefInjection
	{
		public string path;

		public string normalizedPath;

		public string nonBackCompatiblePath;

		public string suggestedPath;

		public string injection;

		public List<string> fullListInjection;

		public List<Pair<int, string>> fullListInjectionComments;

		public string fileSource;

		public bool injected;

		public string replacedString;

		public IEnumerable<string> replacedList;

		public bool isPlaceholder;

		public bool IsFullListInjection => fullListInjection != null;

		public string DefName
		{
			get
			{
				if (!path.NullOrEmpty())
				{
					return path.Split('.')[0];
				}
				return "";
			}
		}

		public bool ModifiesDefFromModOrNullCore(ModMetaData mod, Type defType)
		{
			Def defSilentFail = GenDefDatabase.GetDefSilentFail(defType, DefName);
			if (defSilentFail == null)
			{
				return mod.IsCoreMod;
			}
			if (mod == null)
			{
				return defSilentFail.modContentPack == null;
			}
			if (defSilentFail.modContentPack != null)
			{
				return defSilentFail.modContentPack.FolderName == mod.FolderName;
			}
			return false;
		}
	}

	public Type defType;

	public Dictionary<string, DefInjection> injections = new Dictionary<string, DefInjection>();

	public List<string> loadErrors = new List<string>();

	public List<string> loadSyntaxSuggestions = new List<string>();

	public bool usedOldRepSyntax;

	public const BindingFlags FieldBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public const string RepNodeName = "rep";

	public DefInjectionPackage(Type defType)
	{
		this.defType = defType;
	}

	private string ProcessedPath(string path)
	{
		if (path == null)
		{
			path = "";
		}
		if (!path.Contains('[') && !path.Contains(']'))
		{
			return path;
		}
		return path.Replace("]", "").Replace('[', '.');
	}

	private string ProcessedTranslation(string rawTranslation)
	{
		return rawTranslation.Replace("\\n", "\n");
	}

	public void AddDataFromFile(VirtualFile file, out bool xmlParseError, string preloadedFileContents)
	{
		xmlParseError = false;
		try
		{
			foreach (XElement item in VirtualFileInfoExt.LoadAsXDocument(preloadedFileContents).Root.Elements())
			{
				if (item.Name == "rep")
				{
					string key = ProcessedPath(item.Elements("path").First().Value);
					string translation = ProcessedTranslation(item.Elements("trans").First().Value);
					TryAddInjection(file, key, translation);
					usedOldRepSyntax = true;
					continue;
				}
				string text = ProcessedPath(item.Name.ToString());
				if (text.EndsWith(".slateRef"))
				{
					if (item.HasElements)
					{
						TryAddInjection(file, text, item.GetInnerXml());
						continue;
					}
					string translation2 = ProcessedTranslation(item.Value);
					TryAddInjection(file, text, translation2);
				}
				else if (item.HasElements)
				{
					List<string> list = new List<string>();
					List<Pair<int, string>> list2 = null;
					bool flag = false;
					foreach (XNode item2 in item.DescendantNodes())
					{
						if (item2 is XElement xElement)
						{
							if (xElement.Name == "li")
							{
								list.Add(ProcessedTranslation(xElement.Value));
							}
							else if (!flag)
							{
								loadErrors.Add(text + " has elements which are not 'li' (" + file.Name + ")");
								flag = true;
							}
						}
						if (item2 is XComment xComment)
						{
							if (list2 == null)
							{
								list2 = new List<Pair<int, string>>();
							}
							list2.Add(new Pair<int, string>(list.Count, xComment.Value));
						}
					}
					TryAddFullListInjection(file, text, list, list2);
				}
				else
				{
					string translation3 = ProcessedTranslation(item.Value);
					TryAddInjection(file, text, translation3);
				}
			}
		}
		catch (Exception ex)
		{
			loadErrors.Add("Exception loading translation data from file " + file.Name + ": " + ex);
			xmlParseError = true;
		}
	}

	private void TryAddInjection(VirtualFile file, string key, string translation)
	{
		string text = key;
		key = BackCompatibleKey(key);
		if (!CheckErrors(file, key, text, replacingFullList: false))
		{
			DefInjection defInjection = new DefInjection();
			if (translation == "TODO")
			{
				defInjection.isPlaceholder = true;
				translation = "";
			}
			defInjection.path = key;
			defInjection.injection = translation;
			defInjection.fileSource = file.Name;
			defInjection.nonBackCompatiblePath = text;
			injections.SetOrAdd(key, defInjection);
		}
	}

	private void TryAddFullListInjection(VirtualFile file, string key, List<string> translation, List<Pair<int, string>> comments)
	{
		string text = key;
		key = BackCompatibleKey(key);
		if (!CheckErrors(file, key, text, replacingFullList: true))
		{
			if (translation == null)
			{
				translation = new List<string>();
			}
			DefInjection defInjection = new DefInjection();
			defInjection.path = key;
			defInjection.fullListInjection = translation;
			defInjection.fullListInjectionComments = comments;
			defInjection.fileSource = file.Name;
			defInjection.nonBackCompatiblePath = text;
			injections.Add(key, defInjection);
		}
	}

	private string BackCompatibleKey(string key)
	{
		string[] array = key.Split('.');
		if (array.Any())
		{
			array[0] = BackCompatibility.BackCompatibleDefName(defType, array[0], forDefInjections: true);
		}
		key = string.Join(".", array);
		if (defType == typeof(ConceptDef) && key.Contains(".helpTexts.0"))
		{
			key = key.Replace(".helpTexts.0", ".helpText");
		}
		return key;
	}

	private bool CheckErrors(VirtualFile file, string key, string nonBackCompatibleKey, bool replacingFullList)
	{
		if (!key.Contains('.'))
		{
			loadErrors.Add("Error loading DefInjection from file " + file.Name + ": Key lacks a dot: " + key + ((key == nonBackCompatibleKey) ? "" : (" (auto-renamed from " + nonBackCompatibleKey + ")")) + " (" + file.Name + ")");
			return true;
		}
		if (injections.TryGetValue(key, out var value))
		{
			string text = ((key != nonBackCompatibleKey) ? (" (auto-renamed from " + nonBackCompatibleKey + ")") : ((!(value.path != value.nonBackCompatiblePath)) ? "" : (" (" + value.nonBackCompatiblePath + " was auto-renamed to " + value.path + ")")));
			loadErrors.Add("Duplicate def-injected translation key: " + key + text + " (" + file.Name + ")");
		}
		bool flag = false;
		if (replacingFullList)
		{
			if (injections.Any((KeyValuePair<string, DefInjection> x) => !x.Value.IsFullListInjection && x.Key.StartsWith(key + ".")))
			{
				flag = true;
			}
		}
		else if (key.Contains('.') && char.IsNumber(key[key.Length - 1]))
		{
			string key2 = key.Substring(0, key.LastIndexOf('.'));
			if (injections.ContainsKey(key2) && injections[key2].IsFullListInjection)
			{
				flag = true;
			}
		}
		if (flag)
		{
			loadErrors.Add("Replacing the whole list and individual elements at the same time doesn't make sense. Either replace the whole list or translate individual elements by using their indexes. key=" + key + ((key == nonBackCompatibleKey) ? "" : (" (auto-renamed from " + nonBackCompatibleKey + ")")) + " (" + file.Name + ")");
			return true;
		}
		return false;
	}

	public void InjectIntoDefs(bool errorOnDefNotFound)
	{
		loadSyntaxSuggestions.Clear();
		loadErrors.Clear();
		foreach (KeyValuePair<string, DefInjection> injection in injections)
		{
			if (!injection.Value.injected)
			{
				string normalizedPath;
				string suggestedPath;
				if (injection.Value.IsFullListInjection)
				{
					injection.Value.injected = SetDefFieldAtPath(defType, injection.Key, injection.Value.fullListInjection, typeof(List<string>), errorOnDefNotFound, injection.Value.fileSource, injection.Value.isPlaceholder, out normalizedPath, out suggestedPath, out injection.Value.replacedString, out injection.Value.replacedList);
				}
				else
				{
					injection.Value.injected = SetDefFieldAtPath(defType, injection.Key, injection.Value.injection, typeof(string), errorOnDefNotFound, injection.Value.fileSource, injection.Value.isPlaceholder, out normalizedPath, out suggestedPath, out injection.Value.replacedString, out injection.Value.replacedList);
				}
				injection.Value.normalizedPath = normalizedPath;
				injection.Value.suggestedPath = suggestedPath;
			}
		}
		GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), defType, "ClearCachedData");
	}

	private bool SetDefFieldAtPath(Type defType, string path, object value, Type ensureFieldType, bool errorOnDefNotFound, string fileSource, bool isPlaceholder, out string normalizedPath, out string suggestedPath, out string replacedString, out IEnumerable<string> replacedStringsList)
	{
		replacedString = null;
		replacedStringsList = null;
		string text = path;
		string text2 = path;
		bool flag = TKeySystem.TryGetNormalizedPath(path, out normalizedPath);
		if (flag)
		{
			text2 = text2 + " (" + normalizedPath + ")";
			suggestedPath = path;
			path = normalizedPath;
		}
		else
		{
			normalizedPath = path;
			suggestedPath = path;
		}
		string defName = path.Split('.')[0];
		defName = BackCompatibility.BackCompatibleDefName(defType, defName, forDefInjections: true);
		if (GenDefDatabase.GetDefSilentFail(defType, defName, specialCaseForSoundDefs: false) == null)
		{
			if (errorOnDefNotFound)
			{
				loadErrors.Add("Found no " + defType?.ToString() + " named " + defName + " to match " + text2 + " (" + fileSource + ")");
			}
			return false;
		}
		bool flag2 = false;
		int num = 0;
		List<object> list = new List<object>();
		try
		{
			List<string> list2 = path.Split('.').ToList();
			object obj = GenDefDatabase.GetDefSilentFail(defType, list2[0], specialCaseForSoundDefs: false);
			if (obj == null)
			{
				throw new InvalidOperationException("Def named " + list2[0] + " not found.");
			}
			num++;
			string text3;
			int result;
			DefInjectionPathPartKind defInjectionPathPartKind;
			while (true)
			{
				text3 = list2[num];
				list.Add(obj);
				result = -1;
				if (int.TryParse(text3, out result))
				{
					defInjectionPathPartKind = DefInjectionPathPartKind.ListIndex;
				}
				else if (GetFieldNamed(obj.GetType(), text3) != null)
				{
					defInjectionPathPartKind = DefInjectionPathPartKind.Field;
				}
				else if (obj.GetType().GetProperty("Count") != null)
				{
					if (text3.Contains('-'))
					{
						defInjectionPathPartKind = DefInjectionPathPartKind.ListHandleWithIndex;
						string[] array = text3.Split('-');
						text3 = array[0];
						result = ParseHelper.FromString<int>(array[1]);
					}
					else
					{
						defInjectionPathPartKind = DefInjectionPathPartKind.ListHandle;
					}
				}
				else
				{
					defInjectionPathPartKind = DefInjectionPathPartKind.Field;
				}
				if (num == list2.Count - 1)
				{
					break;
				}
				switch (defInjectionPathPartKind)
				{
				case DefInjectionPathPartKind.Field:
				{
					FieldInfo field = obj.GetType().GetField(text3, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (field == null)
					{
						throw new InvalidOperationException("Field or TKey " + text3 + " does not exist.");
					}
					if (field.HasAttribute<NoTranslateAttribute>())
					{
						throw new InvalidOperationException("Translated untranslatable field " + field.Name + " of type " + field.FieldType?.ToString() + " at path " + text2 + ". Translating this field will break the game.");
					}
					if (field.HasAttribute<UnsavedAttribute>())
					{
						throw new InvalidOperationException("Translated untranslatable field ([Unsaved] attribute) " + field.Name + " of type " + field.FieldType?.ToString() + " at path " + text2 + ". Translating this field will break the game.");
					}
					if (field.HasAttribute<TranslationCanChangeCountAttribute>())
					{
						flag2 = true;
					}
					if (defInjectionPathPartKind == DefInjectionPathPartKind.Field)
					{
						obj = field.GetValue(obj);
						break;
					}
					object value2 = field.GetValue(obj);
					PropertyInfo property2 = value2.GetType().GetProperty("Item");
					if (property2 == null)
					{
						throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
					}
					int num3 = (int)value2.GetType().GetProperty("Count").GetValue(value2, null);
					if (result < 0 || result >= num3)
					{
						throw new InvalidOperationException("Index out of bounds (max index is " + (num3 - 1) + ")");
					}
					obj = property2.GetValue(value2, new object[1] { result });
					break;
				}
				case DefInjectionPathPartKind.ListIndex:
				case DefInjectionPathPartKind.ListHandle:
				case DefInjectionPathPartKind.ListHandleWithIndex:
				{
					object obj2 = obj;
					PropertyInfo property = obj2.GetType().GetProperty("Item");
					if (property == null)
					{
						throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
					}
					bool flag3;
					if (defInjectionPathPartKind == DefInjectionPathPartKind.ListHandle || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandleWithIndex)
					{
						result = TranslationHandleUtility.GetElementIndexByHandle(obj2, text3, result);
						defInjectionPathPartKind = DefInjectionPathPartKind.ListIndex;
						flag3 = true;
					}
					else
					{
						flag3 = false;
					}
					int num2 = (int)obj2.GetType().GetProperty("Count").GetValue(obj2, null);
					if (result < 0 || result >= num2)
					{
						throw new InvalidOperationException("Index out of bounds (max index is " + (num2 - 1) + ")");
					}
					obj = property.GetValue(obj2, new object[1] { result });
					if (flag3)
					{
						string[] array2 = normalizedPath.Split('.');
						array2[num] = result.ToString();
						normalizedPath = string.Join(".", array2);
					}
					else if (!flag)
					{
						string bestHandleWithIndexForListElement = TranslationHandleUtility.GetBestHandleWithIndexForListElement(obj2, obj);
						if (!bestHandleWithIndexForListElement.NullOrEmpty())
						{
							string[] array3 = suggestedPath.Split('.');
							array3[num] = bestHandleWithIndexForListElement;
							suggestedPath = string.Join(".", array3);
						}
					}
					break;
				}
				default:
					loadErrors.Add("Can't enter node " + text3 + " at path " + text2 + ", element kind is " + defInjectionPathPartKind.ToString() + ". (" + fileSource + ")");
					break;
				}
				num++;
			}
			bool flag4 = false;
			foreach (KeyValuePair<string, DefInjection> injection in injections)
			{
				if (!(injection.Key == text) && injection.Value.injected && injection.Value.normalizedPath == normalizedPath)
				{
					string text4 = "Duplicate def-injected translation key. Both " + injection.Value.path + " and " + text2 + " refer to the same field (" + suggestedPath + ")";
					if (injection.Value.path != injection.Value.nonBackCompatiblePath)
					{
						text4 = text4 + " (" + injection.Value.nonBackCompatiblePath + " was auto-renamed to " + injection.Value.path + ")";
					}
					text4 = text4 + " (" + injection.Value.fileSource + ")";
					loadErrors.Add(text4);
					flag4 = true;
					break;
				}
			}
			bool result2 = false;
			if (!flag4)
			{
				switch (defInjectionPathPartKind)
				{
				case DefInjectionPathPartKind.Field:
				{
					FieldInfo fieldNamed = GetFieldNamed(obj.GetType(), text3);
					if (fieldNamed == null)
					{
						throw new InvalidOperationException("Field " + text3 + " does not exist in type " + obj.GetType()?.ToString() + ".");
					}
					if (fieldNamed.HasAttribute<NoTranslateAttribute>())
					{
						loadErrors.Add("Translated untranslatable field " + fieldNamed.Name + " of type " + fieldNamed.FieldType?.ToString() + " at path " + text2 + ". Translating this field will break the game. (" + fileSource + ")");
					}
					else if (fieldNamed.HasAttribute<UnsavedAttribute>())
					{
						loadErrors.Add("Translated untranslatable field (UnsavedAttribute) " + fieldNamed.Name + " of type " + fieldNamed.FieldType?.ToString() + " at path " + text2 + ". Translating this field will break the game. (" + fileSource + ")");
					}
					else if (!isPlaceholder && fieldNamed.FieldType != ensureFieldType)
					{
						loadErrors.Add("Translated non-" + ensureFieldType?.ToString() + " field " + fieldNamed.Name + " of type " + fieldNamed.FieldType?.ToString() + " at path " + text2 + ". Expected " + ensureFieldType?.ToString() + ". (" + fileSource + ")");
					}
					else if (!isPlaceholder && ensureFieldType != typeof(string) && !fieldNamed.HasAttribute<TranslationCanChangeCountAttribute>())
					{
						loadErrors.Add("Tried to translate field " + fieldNamed.Name + " of type " + fieldNamed.FieldType?.ToString() + " at path " + text2 + ", but this field doesn't have [TranslationCanChangeCount] attribute so it doesn't allow this type of translation. (" + fileSource + ")");
					}
					else if (!isPlaceholder)
					{
						if (ensureFieldType == typeof(string))
						{
							replacedString = (string)fieldNamed.GetValue(obj);
						}
						else
						{
							replacedStringsList = fieldNamed.GetValue(obj) as IEnumerable<string>;
						}
						fieldNamed.SetValue(obj, value);
						result2 = true;
					}
					break;
				}
				case DefInjectionPathPartKind.ListIndex:
				case DefInjectionPathPartKind.ListHandle:
				case DefInjectionPathPartKind.ListHandleWithIndex:
				{
					object obj3 = obj;
					if (obj3 == null)
					{
						throw new InvalidOperationException("Tried to use index on null list at " + text2);
					}
					Type type = obj3.GetType();
					PropertyInfo property3 = type.GetProperty("Count");
					if (property3 == null)
					{
						throw new InvalidOperationException("Tried to use index on non-list (missing 'Count' property).");
					}
					if (defInjectionPathPartKind == DefInjectionPathPartKind.ListHandle || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandleWithIndex)
					{
						result = TranslationHandleUtility.GetElementIndexByHandle(obj3, text3, result);
						defInjectionPathPartKind = DefInjectionPathPartKind.ListIndex;
					}
					int num4 = (int)property3.GetValue(obj3, null);
					if (result >= num4)
					{
						throw new InvalidOperationException("Trying to translate " + defType?.ToString() + "." + text2 + " at index " + result + " but the list only has " + num4 + " entries (so max index is " + (num4 - 1) + ").");
					}
					PropertyInfo property4 = type.GetProperty("Item");
					if (property4 == null)
					{
						throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
					}
					Type propertyType = property4.PropertyType;
					if (!isPlaceholder && propertyType != ensureFieldType)
					{
						loadErrors.Add("Translated non-" + ensureFieldType?.ToString() + " list item of type " + propertyType?.ToString() + " at path " + text2 + ". Expected " + ensureFieldType?.ToString() + ". (" + fileSource + ")");
					}
					else if (!isPlaceholder && ensureFieldType != typeof(string) && !flag2)
					{
						loadErrors.Add("Tried to translate field of type " + propertyType?.ToString() + " at path " + text2 + ", but this field doesn't have [TranslationCanChangeCount] attribute so it doesn't allow this type of translation. (" + fileSource + ")");
					}
					else if (result < 0 || result >= (int)type.GetProperty("Count").GetValue(obj3, null))
					{
						loadErrors.Add("Index out of bounds (max index is " + ((int)type.GetProperty("Count").GetValue(obj3, null) - 1) + ")");
					}
					else if (!isPlaceholder)
					{
						replacedString = (string)property4.GetValue(obj3, new object[1] { result });
						property4.SetValue(obj3, value, new object[1] { result });
						result2 = true;
					}
					break;
				}
				default:
					loadErrors.Add("Translated " + text3 + " at path " + text2 + " but it's not a field, it's " + defInjectionPathPartKind.ToString() + ". (" + fileSource + ")");
					break;
				}
			}
			for (int num5 = list.Count - 1; num5 > 0; num5--)
			{
				if (list[num5].GetType().IsValueType && !list[num5].GetType().IsPrimitive)
				{
					FieldInfo fieldNamed2 = GetFieldNamed(list[num5 - 1].GetType(), list2[num5]);
					if (fieldNamed2 != null)
					{
						fieldNamed2.SetValue(list[num5 - 1], list[num5]);
					}
				}
			}
			string tKeyPath;
			if (flag)
			{
				path = suggestedPath;
			}
			else if (TKeySystem.TrySuggestTKeyPath(path, out tKeyPath))
			{
				suggestedPath = tKeyPath;
			}
			if (path != suggestedPath)
			{
				string text5 = ((!(value is IList<string> enumerable)) ? value.ToString() : enumerable.ToStringSafeEnumerable());
				loadSyntaxSuggestions.Add("Consider using " + suggestedPath + " instead of " + text2 + " for translation '" + text5 + "' (" + fileSource + ")");
			}
			return result2;
		}
		catch (Exception ex)
		{
			string text6 = "Couldn't inject " + text2 + " into " + defType?.ToString() + " (" + fileSource + "): " + ex.Message;
			if (ex.InnerException != null)
			{
				text6 = text6 + " -> " + ex.InnerException.Message;
			}
			loadErrors.Add(text6);
			return false;
		}
	}

	private FieldInfo GetFieldNamed(Type type, string name)
	{
		FieldInfo field = type.GetField(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				object[] customAttributes = fields[i].GetCustomAttributes(typeof(LoadAliasAttribute), inherit: false);
				if (customAttributes == null || customAttributes.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (((LoadAliasAttribute)customAttributes[j]).alias == name)
					{
						return fields[i];
					}
				}
			}
		}
		return field;
	}

	public List<string> MissingInjections(List<string> outUnnecessaryDefInjections)
	{
		List<string> missingInjections = new List<string>();
		Dictionary<string, DefInjection> injectionsByNormalizedPath = new Dictionary<string, DefInjection>();
		foreach (KeyValuePair<string, DefInjection> injection in injections)
		{
			if (!injectionsByNormalizedPath.ContainsKey(injection.Value.normalizedPath))
			{
				injectionsByNormalizedPath.Add(injection.Value.normalizedPath, injection.Value);
			}
		}
		DefInjectionUtility.ForEachPossibleDefInjection(defType, delegate(string suggestedPath, string normalizedPath, bool isCollection, string str, IEnumerable<string> collection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def)
		{
			if (!isCollection)
			{
				bool flag = false;
				string text = null;
				if (injectionsByNormalizedPath.TryGetValue(normalizedPath, out var value) && !value.IsFullListInjection)
				{
					if (!translationAllowed)
					{
						outUnnecessaryDefInjections.Add(value.path + " '" + value.injection.Replace("\n", "\\n") + "'");
					}
					else if (value.isPlaceholder)
					{
						flag = true;
						text = value.fileSource;
					}
				}
				else
				{
					flag = true;
				}
				if (flag && translationAllowed && DefInjectionUtility.ShouldCheckMissingInjection(str, fieldInfo, def))
				{
					missingInjections.Add(suggestedPath + " '" + str.Replace("\n", "\\n") + "'" + (text.NullOrEmpty() ? "" : (" (placeholder exists in " + text + ")")));
				}
			}
			else
			{
				if (!injectionsByNormalizedPath.TryGetValue(normalizedPath, out var value2) || !value2.IsFullListInjection)
				{
					int num = 0;
					{
						foreach (string item in collection)
						{
							string key = normalizedPath + "." + num;
							string text2 = suggestedPath + "." + num;
							bool flag2 = false;
							string text3 = null;
							if (injectionsByNormalizedPath.TryGetValue(key, out var value3) && !value3.IsFullListInjection)
							{
								if (!translationAllowed)
								{
									outUnnecessaryDefInjections.Add(value3.path + " '" + value3.injection.Replace("\n", "\\n") + "'");
								}
								else if (value3.isPlaceholder)
								{
									flag2 = true;
									text3 = value3.fileSource;
								}
							}
							else
							{
								flag2 = true;
							}
							if (flag2 && translationAllowed && DefInjectionUtility.ShouldCheckMissingInjection(item, fieldInfo, def))
							{
								if (text3.NullOrEmpty() && injectionsByNormalizedPath.TryGetValue(normalizedPath, out var value4) && value4.isPlaceholder)
								{
									text3 = value4.fileSource;
								}
								missingInjections.Add(text2 + " '" + item.Replace("\n", "\\n") + "'" + (fullListTranslationAllowed ? " (hint: this list allows full-list translation by using <li> nodes)" : "") + (text3.NullOrEmpty() ? "" : (" (placeholder exists in " + text3 + ")")));
							}
							num++;
						}
						return;
					}
				}
				if (!translationAllowed || !fullListTranslationAllowed)
				{
					outUnnecessaryDefInjections.Add(value2.path + " '" + value2.fullListInjection.ToStringSafeEnumerable().Replace("\n", "\\n") + "'");
				}
				else if (value2.isPlaceholder && translationAllowed && !def.generated)
				{
					missingInjections.Add(suggestedPath + (value2.fileSource.NullOrEmpty() ? "" : (" (placeholder exists in " + value2.fileSource + ")")));
				}
			}
		});
		return missingInjections;
	}
}
