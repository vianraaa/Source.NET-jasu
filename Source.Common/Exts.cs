using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;

using Microsoft.Extensions.DependencyInjection;

using Source.Common.Engine;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Source
{
	public static class ClassUtils
	{
		public static void EnsureCount<T>(this List<T> list, int ensureTo) where T : class, new() {
			while (list.Count < ensureTo) {
				list.Add(new T());
			}
		}

		private static bool ParametersMatch(ParameterInfo[] parameters, ImmutableArray<Type> argTypes) {
			if (parameters.Length != argTypes.Length)
				return false;

			for (int i = 0; i < parameters.Length; i++) {
				Type paramType = parameters[i].ParameterType;
				Type argType = argTypes[i];

				// Allow for null arguments and assignable types
				if (argType == typeof(object))
					continue;

				if (!paramType.IsAssignableFrom(argType))
					return false;
			}

			return true;
		}

		public static T? InitSubsystem<T>(this IEngineAPI api, params object?[] parms) where T : class
			=> InitSubsystem<T>(api, out _, parms);
		public static T? InitSubsystem<T>(this IEngineAPI api, out string? error, params object?[] parms)
			where T : class {
			// Argument types for the method call.
			var argTypes = parms.Select(arg => arg?.GetType() ?? typeof(object)).ToImmutableArray();
			// The instance from IEngineAPI
			var instance = api.GetRequiredService<T>();
			// Find a method that has
			//     1. "Init" as the name.
			//     2. Matches the parameters the callee provided.
			var method = typeof(T)
				.GetMethods()
				.Where(x => x.Name == "Init")
				.FirstOrDefault(m => ParametersMatch(m.GetParameters(), argTypes));
			// No method, no success
			if (method == null) {
				error = $"The subsystem '{typeof(T).Name}' does not contain method Init({string.Join(", ", argTypes.Select(x => x.Name))})";
				return null;
			}
			// If the method returns booleans, return whatever the call provides
			if (method.ReturnType == typeof(bool)) {
				bool ok = (bool)(method.Invoke(instance, parms) ?? true);
				error = ok ? null : $"The subsystem '{typeof(T).Name}' failed to initialize.";
				return ok ? instance : null;
			}
			// Method invoke, return true
			method.Invoke(instance, parms);
			error = null;
			return instance;
		}
	}
	public static class UnmanagedUtils
	{
		public static void EnsureCount<T>(this List<T> list, int ensureTo) where T : unmanaged {
			while (list.Count < ensureTo) {
				list.Add(new T());
			}
		}

		public static string[] Split(this ReadOnlySpan<char> input, char separator) {
			Span<Range> ranges = stackalloc Range[64];
			var splits = input.Split(ranges, ' ');
			string[] array = new string[splits];
			for (int i = 0; i < splits; i++) {
				array[i] = new(input[ranges[i]]);
			}
			return array;
		}
	}
}