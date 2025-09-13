using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Shared;

/// <summary>
/// Links a class type to a hammer name
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class LinkEntityToClassAttribute : Attribute
{
	public required string LocalName;
}

/// <summary>
/// Manually sets the class index. This is required at the moment for Garry's Mod networking compat
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ManualClassIndexAttribute : Attribute
{
	public required int Index;
}