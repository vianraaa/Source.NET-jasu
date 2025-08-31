using Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Source.Common.Engine;
public interface IEngineAPI : IServiceProvider
{
	public enum Result
	{
		InitFailed = 0,
		InitOK,
		InitRestart,
		RunOK,
		RunRestart
	}

	public Result Run();
	public void SetStartupInfo(in StartupInfo info);
	bool MainLoop();
}
public delegate void PreInject(IServiceCollection services);


/// <summary>
/// Marks a field to be auto populated by the service collection.
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Field)]
public class ImportedAttribute : Attribute
{

}

public delegate void PreInjectInstance<T>(IServiceProvider services);
public static class ImportUtils {
	// TODO: profile this, how bad is this
	// also is this a really bad idea? I don't know... but it sure is convenient
	public static T New<T>(this IServiceProvider services, params object?[] args) {
		Type type = typeof(T);
		return (T)New(services, type, args);
	}

	public static object New(this IServiceProvider services, Type type, params object?[] args) {
		object instance = RuntimeHelpers.GetUninitializedObject(type);
		foreach (var field in type.GetFields()) {
			var attr = field.GetCustomAttribute<ImportedAttribute>();
			if (attr != null) {
				var tArg = field.FieldType;
				object? service;
				if (Nullable.GetUnderlyingType(tArg) != null) 
					service = services.GetService(tArg);
				else
					service = services.GetRequiredService(tArg);

				field.SetValue(instance, service);
			}
		}

		var argTypes = args.Select(a => a?.GetType()).ToArray();
		var ctor = type.GetConstructors()
		.FirstOrDefault(c => {
			var parameters = c.GetParameters();
			if (parameters.Length != argTypes.Length) return false;
			for (int i = 0; i < parameters.Length; i++) {
				if (argTypes[i] == null) {
					if (parameters[i].ParameterType.IsValueType &&
						Nullable.GetUnderlyingType(parameters[i].ParameterType) == null)
						return false;
				}
				else if (!parameters[i].ParameterType.IsAssignableFrom(argTypes[i]))
					return false;
			}
			return true;
		});

		AssertMsg(ctor != null, "EngineAPI.New<T> constructor is null!");
		ctor!.Invoke(instance, args);
		return instance;
	}
}
