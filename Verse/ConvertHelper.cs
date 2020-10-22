using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class ConvertHelper
	{
		public static bool CanConvert<T>(object obj)
		{
			return CanConvert(obj, typeof(T));
		}

		public static bool CanConvert(object obj, Type to)
		{
			if (obj == null)
			{
				return true;
			}
			if (to.IsAssignableFrom(obj.GetType()))
			{
				return true;
			}
			if (to == typeof(string))
			{
				return true;
			}
			if (obj is string && !to.IsPrimitive && ParseHelper.CanParse(to, (string)obj))
			{
				return true;
			}
			if (obj is string && typeof(Def).IsAssignableFrom(to))
			{
				return true;
			}
			if (obj is string && to == typeof(Faction))
			{
				return true;
			}
			if (CanConvertBetweenDataTypes(obj.GetType(), to))
			{
				return true;
			}
			if (IsXml(obj) && !to.IsPrimitive)
			{
				return true;
			}
			if (to.IsGenericType && (to.GetGenericTypeDefinition() == typeof(IEnumerable<>) || to.GetGenericTypeDefinition() == typeof(List<>)) && to.GetGenericArguments().Length >= 1 && (!(to.GetGenericArguments()[0] == typeof(string)) || !(obj is string)))
			{
				IEnumerable enumerable = obj as IEnumerable;
				if (enumerable != null)
				{
					Type to2 = to.GetGenericArguments()[0];
					bool flag = true;
					foreach (object item in enumerable)
					{
						if (!CanConvert(item, to2))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						return true;
					}
				}
			}
			if (obj is IEnumerable && !(obj is string))
			{
				IEnumerable e = (IEnumerable)obj;
				if (GenCollection.Count_EnumerableBase(e) == 1 && CanConvert(GenCollection.FirstOrDefault_EnumerableBase(e), to))
				{
					return true;
				}
			}
			if (typeof(IList).IsAssignableFrom(to))
			{
				Type[] genericArguments = to.GetGenericArguments();
				if (genericArguments.Length >= 1)
				{
					return CanConvert(obj, genericArguments[0]);
				}
				return true;
			}
			if (to == typeof(IEnumerable))
			{
				return true;
			}
			if (to.IsGenericType && to.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				Type[] genericArguments2 = to.GetGenericArguments();
				if (genericArguments2.Length >= 1)
				{
					return CanConvert(obj, genericArguments2[0]);
				}
				return true;
			}
			IConvertible convertible = obj as IConvertible;
			if (convertible == null)
			{
				return false;
			}
			Type left = Nullable.GetUnderlyingType(to) ?? to;
			if (left != typeof(bool) && left != typeof(byte) && left != typeof(char) && left != typeof(DateTime) && left != typeof(decimal) && left != typeof(double) && left != typeof(short) && left != typeof(int) && left != typeof(long) && left != typeof(sbyte) && left != typeof(float) && left != typeof(string) && left != typeof(ushort) && left != typeof(uint) && left != typeof(ulong))
			{
				return false;
			}
			try
			{
				ConvertToPrimitive(convertible, to, null);
			}
			catch (FormatException)
			{
				return false;
			}
			return true;
		}

		public static T Convert<T>(object obj)
		{
			return (T)Convert(obj, typeof(T), default(T));
		}

		public static object Convert(object obj, Type to)
		{
			if (to.IsValueType)
			{
				return Convert(obj, to, Activator.CreateInstance(to));
			}
			return Convert(obj, to, null);
		}

		public static object Convert(object obj, Type to, object defaultValue)
		{
			if (obj == null)
			{
				return defaultValue;
			}
			if (to.IsAssignableFrom(obj.GetType()))
			{
				return obj;
			}
			if (to == typeof(string))
			{
				return obj.ToString();
			}
			string text = obj as string;
			if (text != null && !to.IsPrimitive && ParseHelper.CanParse(to, (string)obj))
			{
				if (text == "")
				{
					return defaultValue;
				}
				return ParseHelper.FromString(text, to);
			}
			if (text != null && typeof(Def).IsAssignableFrom(to))
			{
				if (text == "")
				{
					return defaultValue;
				}
				return GenDefDatabase.GetDef(to, text);
			}
			if (text != null && to == typeof(Faction))
			{
				if (text == "")
				{
					return defaultValue;
				}
				List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
				for (int i = 0; i < allFactionsListForReading.Count; i++)
				{
					if (allFactionsListForReading[i].GetUniqueLoadID() == text)
					{
						return allFactionsListForReading[i];
					}
				}
				for (int j = 0; j < allFactionsListForReading.Count; j++)
				{
					if (allFactionsListForReading[j].HasName && allFactionsListForReading[j].Name == text)
					{
						return allFactionsListForReading[j];
					}
				}
				for (int k = 0; k < allFactionsListForReading.Count; k++)
				{
					if (allFactionsListForReading[k].def.defName == text)
					{
						return allFactionsListForReading[k];
					}
				}
				return defaultValue;
			}
			if (CanConvertBetweenDataTypes(obj.GetType(), to))
			{
				return ConvertBetweenDataTypes(obj, to);
			}
			if (IsXml(obj) && !to.IsPrimitive)
			{
				try
				{
					Type type = to;
					if (type == typeof(IEnumerable))
					{
						type = typeof(List<string>);
					}
					if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) && type.GetGenericArguments().Length >= 1)
					{
						type = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
					}
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<root>\n" + text + "\n</root>");
					object result = DirectXmlToObject.GetObjectFromXmlMethod(type)(xmlDocument.DocumentElement, arg2: true);
					DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
					return result;
				}
				finally
				{
					DirectXmlCrossRefLoader.Clear();
				}
			}
			if (to.IsGenericType && (to.GetGenericTypeDefinition() == typeof(IEnumerable<>) || to.GetGenericTypeDefinition() == typeof(List<>)) && to.GetGenericArguments().Length >= 1 && (!(to.GetGenericArguments()[0] == typeof(string)) || !(obj is string)))
			{
				IEnumerable enumerable = obj as IEnumerable;
				if (enumerable != null)
				{
					Type type2 = to.GetGenericArguments()[0];
					bool flag = true;
					foreach (object item in enumerable)
					{
						if (!CanConvert(item, type2))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type2));
						{
							foreach (object item2 in enumerable)
							{
								list.Add(Convert(item2, type2));
							}
							return list;
						}
					}
				}
			}
			if (obj is IEnumerable && !(obj is string))
			{
				IEnumerable e = (IEnumerable)obj;
				if (GenCollection.Count_EnumerableBase(e) == 1)
				{
					object obj2 = GenCollection.FirstOrDefault_EnumerableBase(e);
					if (CanConvert(obj2, to))
					{
						return Convert(obj2, to);
					}
				}
			}
			if (typeof(IList).IsAssignableFrom(to))
			{
				IList list2 = (IList)Activator.CreateInstance(to);
				Type[] genericArguments = to.GetGenericArguments();
				if (genericArguments.Length >= 1)
				{
					list2.Add(Convert(obj, genericArguments[0]));
				}
				else
				{
					list2.Add(obj);
				}
				return list2;
			}
			if (to == typeof(IEnumerable))
			{
				return Gen.YieldSingleNonGeneric(obj);
			}
			if (to.IsGenericType && to.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				Type[] genericArguments2 = to.GetGenericArguments();
				if (genericArguments2.Length >= 1)
				{
					IList obj3 = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArguments2[0]));
					obj3.Add(Convert(obj, genericArguments2[0]));
					return obj3;
				}
				return Gen.YieldSingleNonGeneric(obj);
			}
			IConvertible convertible = obj as IConvertible;
			if (convertible == null)
			{
				return defaultValue;
			}
			try
			{
				return ConvertToPrimitive(convertible, to, defaultValue);
			}
			catch (FormatException)
			{
				return defaultValue;
			}
		}

		public static bool IsXml(object obj)
		{
			if (obj is TaggedString)
			{
				return false;
			}
			string text = obj as string;
			if (text == null || text.IndexOf('<') < 0 || text.IndexOf('>') < 0)
			{
				return false;
			}
			string text2 = text.Trim();
			if (text2[0] == '<')
			{
				return text2[text2.Length - 1] == '>';
			}
			return false;
		}

		private static object ConvertToPrimitive(IConvertible obj, Type to, object defaultValue)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			Type left = Nullable.GetUnderlyingType(to) ?? to;
			if (left == typeof(bool))
			{
				return System.Convert.ToBoolean(obj, invariantCulture);
			}
			if (left == typeof(byte))
			{
				return System.Convert.ToByte(obj, invariantCulture);
			}
			if (left == typeof(char))
			{
				return System.Convert.ToChar(obj, invariantCulture);
			}
			if (left == typeof(DateTime))
			{
				return System.Convert.ToDateTime(obj, invariantCulture);
			}
			if (left == typeof(decimal))
			{
				return System.Convert.ToDecimal(obj, invariantCulture);
			}
			if (left == typeof(double))
			{
				return System.Convert.ToDouble(obj, invariantCulture);
			}
			if (left == typeof(short))
			{
				return System.Convert.ToInt16(obj, invariantCulture);
			}
			if (left == typeof(int))
			{
				return System.Convert.ToInt32(obj, invariantCulture);
			}
			if (left == typeof(long))
			{
				return System.Convert.ToInt64(obj, invariantCulture);
			}
			if (left == typeof(sbyte))
			{
				return System.Convert.ToSByte(obj, invariantCulture);
			}
			if (left == typeof(float))
			{
				return System.Convert.ToSingle(obj, invariantCulture);
			}
			if (left == typeof(string))
			{
				return System.Convert.ToString(obj, invariantCulture);
			}
			if (left == typeof(ushort))
			{
				return System.Convert.ToUInt16(obj, invariantCulture);
			}
			if (left == typeof(uint))
			{
				return System.Convert.ToUInt32(obj, invariantCulture);
			}
			if (left == typeof(ulong))
			{
				return System.Convert.ToUInt64(obj, invariantCulture);
			}
			return defaultValue;
		}

		private static bool CanConvertBetweenDataTypes(Type from, Type to)
		{
			if (!(from == typeof(IntRange)) || !(to == typeof(FloatRange)))
			{
				if (from == typeof(FloatRange))
				{
					return to == typeof(IntRange);
				}
				return false;
			}
			return true;
		}

		private static object ConvertBetweenDataTypes(object from, Type to)
		{
			if (from is IntRange)
			{
				IntRange intRange = (IntRange)from;
				if (to == typeof(FloatRange))
				{
					return new FloatRange(intRange.min, intRange.max);
				}
			}
			if (from is FloatRange)
			{
				FloatRange floatRange = (FloatRange)from;
				if (to == typeof(IntRange))
				{
					return new IntRange(Mathf.RoundToInt(floatRange.min), Mathf.RoundToInt(floatRange.max));
				}
			}
			return null;
		}
	}
}
