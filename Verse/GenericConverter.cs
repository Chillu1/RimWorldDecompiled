using System;
using System.Linq.Expressions;

namespace Verse;

public static class GenericConverter
{
	private static class StaticGenericCache<InType, OutType>
	{
		public static Func<InType, OutType> cachedFunc = GenerateFunc<InType, OutType>();
	}

	public static OutType Convert<InType, OutType>(InType value)
	{
		return StaticGenericCache<InType, OutType>.cachedFunc(value);
	}

	private static Func<InType, OutType> GenerateFunc<InType, OutType>()
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(InType));
		return Expression.Lambda<Func<InType, OutType>>(Expression.Convert(parameterExpression, typeof(OutType)), new ParameterExpression[1] { parameterExpression }).Compile();
	}
}
