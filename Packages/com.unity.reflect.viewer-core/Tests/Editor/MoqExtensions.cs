using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

public static class MoqExtensions
{
	private const BindingFlags privateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

	public static ICallbackResult OutCallback(this ICallback mock, Delegate callback)
	{
		RefCallbackHelper.SetCallback(mock, callback);
		return (ICallbackResult)mock;
	}

	public static ICallbackResult RefCallback(this ICallback mock, Delegate callback)
	{
		RefCallbackHelper.SetCallback(mock, callback);
		return (ICallbackResult)mock;
	}

	public static IReturnsThrows<TMock, TResult> OutCallback<TMock, TResult>(this ICallback<TMock, TResult> mock, Delegate callback)
		where TMock : class
	{
		RefCallbackHelper.SetCallback(mock, callback);
		return (IReturnsThrows<TMock, TResult>)mock;
	}

	public static IReturnsThrows<TMock, TResult> RefCallback<TMock, TResult>(this ICallback<TMock, TResult> mock, Delegate callback)
		where TMock : class
	{
		RefCallbackHelper.SetCallback(mock, callback);
		return (IReturnsThrows<TMock, TResult>)mock;
	}

	private static class RefCallbackHelper
	{
		private static readonly Action<object, Delegate> setCallbackWithoutArgumentsAction = CreateSetCallbackWithoutArgumentsAction();

		public static void SetCallback(object mock, Delegate callback)
		{
			setCallbackWithoutArgumentsAction(mock, callback);
		}

		private static Action<object, Delegate> CreateSetCallbackWithoutArgumentsAction()
		{
			ParameterExpression mockParameter = Expression.Parameter(typeof(object));
			ParameterExpression actionParameter = Expression.Parameter(typeof(Delegate));
			Type type = typeof(Mock<>).Assembly.GetType("Moq.MethodCall", true);
			MethodInfo method = type.GetMethod("SetCallbackWithArguments", privateInstanceFlags);
			if (method == null)
				throw new InvalidOperationException();

			return Expression.Lambda<Action<object, Delegate>>(
				Expression.Call(Expression.Convert(mockParameter, type), method, actionParameter),
				mockParameter,
				actionParameter).Compile();
		}
	}
}
