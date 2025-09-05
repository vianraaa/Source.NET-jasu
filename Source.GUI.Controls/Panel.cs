using CommunityToolkit.HighPerformance;

using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using static Source.Common.Networking.svc_ClassInfo;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Source.GUI.Controls;

public struct OverrideableColorEntry
{
	public delegate Color ColorFunc(in OverrideableColorEntry self);
	public string Name;
	public ColorFunc? Func;
	public Color ColorFromScript;
	public bool Overridden;
	public readonly Color Color() => Func == null ? default : Func(in this);
}

[Flags]
public enum PanelFlags
{
	MarkedForDeletion = 0x0001,
	NeedsRepaint = 0x0002,
	PaintBorderEnabled = 0x0004,
	PaintBackgroundEnabled = 0x0008,
	PaintEnabled = 0x0010,
	PostChildPaintEnabled = 0x0020,
	AutoDeleteEnabled = 0x0040,
	NeedsLayout = 0x0080,
	NeedsSchemeUpdate = 0x0100,
	NeedsDefaultSettingsApplied = 0x0200,
	AllowChainKeybindingToParent = 0x0400,
	InPerformLayout = 0x0800,
	IsProportional = 0x1000,
	TriplePressAllowed = 0x2000,
	DragRequiresPanelExit = 0x4000,
	IsMouseDisabledForThisPanelOnly = 0x8000,
	All = 0xFFFF,
}

public interface IPanelAnimationPropertyConverter
{
	void GetData(Panel panel, KeyValues kv, ref PanelAnimationMapEntry entry);
	void SetData(Panel panel, KeyValues kv, ref PanelAnimationMapEntry entry);
	void InitFromDefault(Panel panel, ref PanelAnimationMapEntry entry);
}

[AttributeUsage(AttributeTargets.Field)]
public class PanelAnimationVarAttribute : Attribute
{
	public readonly string? Name;
	public readonly string DefaultValue;
	public readonly string? Type;

	public PanelAnimationVarAttribute(string defaultValue) {
		DefaultValue = defaultValue;
	}
	public PanelAnimationVarAttribute(string name, string defaultValue) {
		Name = name;
		DefaultValue = defaultValue;
	}

	public static void InitVar(Type t, PanelAnimationVarAttribute attribute, FieldInfo field) {
		PanelGetFunc get;
		PanelSetFunc set;

		{
			DynamicMethod methodBuilder = new($"{t.Name}PanelAnim_GetVar_{field.Name}", typeof(object), [typeof(Panel)]);
			var il = methodBuilder.GetILGenerator();
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, field);
				if (field.FieldType.IsValueType)
					il.Emit(OpCodes.Box, field.FieldType);

				il.Emit(OpCodes.Ret);
			}
			get = methodBuilder.CreateDelegate<PanelGetFunc>();
		}

		{
			DynamicMethod methodBuilder = new($"{t.Name}PanelAnim_SetVar_{field.Name}", typeof(void), [typeof(Panel), typeof(object)]);
			var il = methodBuilder.GetILGenerator();
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);

				if (field.FieldType.IsValueType)
					il.Emit(OpCodes.Unbox_Any, field.FieldType);
				else
					il.Emit(OpCodes.Castclass, field.FieldType);

				il.Emit(OpCodes.Stfld, field);

				il.Emit(OpCodes.Ret);
			}
			set = methodBuilder.CreateDelegate<PanelSetFunc>();
		}

		Panel.AddToAnimationMap(t, attribute.Name ?? field.Name, attribute.Type ?? field.FieldType.Name switch {
			"Single" => "float",
			_ => field.FieldType.Name
		}, field.Name, attribute.DefaultValue, false, get, set);
	}
}

public class Panel : IPanel
{
	public static bool IsValid([NotNullWhen(true)] Panel? panel) => panel != null && !panel.IsMarkedForDeletion();

	readonly static Dictionary<ulong, IPanelAnimationPropertyConverter> AnimationPropertyConverters = [];
	static Panel() {
		ChainToAnimationMap<Panel>();
	}
	public static IPanelAnimationPropertyConverter? FindConverter(ReadOnlySpan<char> typeName) {
		if (AnimationPropertyConverters.TryGetValue(typeName.Hash(), out var converter))
			return converter;
		return null;
	}
	static bool initialized;
	public static void InitPropertyConverters() {
		if (initialized)
			return;
		initialized = true;

		AddPropertyConverter("float", floatConverter);
	}
	static readonly FloatProperty floatConverter = new();

	public static void AddPropertyConverter(ReadOnlySpan<char> typeName, IPanelAnimationPropertyConverter converter) {
		var hash = typeName.Hash();
		if (AnimationPropertyConverters.TryGetValue(hash, out _)) {
			Msg($"Already have VGUI property converter for type {typeName}, ignoring...\n");
			return;
		}

		AnimationPropertyConverters[hash] = converter;
	}

	[Imported] public ISurface Surface;
	[Imported] public ISchemeManager SchemeManager;
	[Imported] public IVGui VGui;
	[Imported] public IVGuiInput Input;
	[Imported] public IEngineAPI EngineAPI;
	[Imported] public ILocalize Localize;
	[Imported] public ILauncherManager Launcher;
	[Imported] public ISystem System;

	private AnimationController? ac;
	public AnimationController GetAnimationController() => ac ??= EngineAPI.GetRequiredService<AnimationController>();

	public void Init(int x, int y, int w, int h) {
		PanelName = null;
		TooltipText = null;
		SetPos(x, y);
		SetSize(w, h);
		Flags |= PanelFlags.NeedsLayout | PanelFlags.NeedsSchemeUpdate | PanelFlags.NeedsDefaultSettingsApplied;
		Flags |= PanelFlags.AutoDeleteEnabled | PanelFlags.PaintBorderEnabled | PanelFlags.PaintBackgroundEnabled | PanelFlags.PaintEnabled;
		Flags |= PanelFlags.AllowChainKeybindingToParent;
		Alpha = 255.0f;
		Visible = true;
		Enabled = true;
		Parent = null;
		Popup = false;
		TopmostPopup = false;

		MouseInput = true;
		KbInput = true;

		Cursor = CursorCode.Arrow;
	}

	public IBorder? GetBorder() => Border;
	public void SetBorder(IBorder? border) => Border = border;

	public void MakeReadyForUse() {
		Surface.SolveTraverse(this, true);
	}
	public float GetAlpha() => Alpha;
	public void SetAlpha(float value) => Alpha = value;


	public Panel() {
		Init(0, 0, 64, 24);
	}

	public Panel(Panel? parent) {
		Init(0, 0, 64, 24);
		SetParent(parent);
	}

	public Panel(Panel? parent, ReadOnlySpan<char> panelName) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
	}
	public Panel(Panel? parent, string? panelName, bool showTaskbarIcon = true) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
	}
	public Panel(Panel? parent, string? panelName, IScheme scheme) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
		SetScheme(scheme);
	}
	public Panel(Panel? parent, ReadOnlySpan<char> panelName, IScheme scheme) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
		SetScheme(scheme);
	}

	private void SetScheme(IScheme scheme) {
		throw new NotImplementedException();
	}

	Panel? Parent;

	string? PanelName;
	string? TooltipText;
	short X, Y;
	short W, H;
	short MinW, MinH;
	short InsetLeft, InsetTop, InsetRight, InsetBottom;
	short ClipRectX, ClipRectY, ClipRectW, ClipRectH;
	short AbsX, AbsY;
	short ZPos;

	bool Visible;
	bool Enabled;
	bool Popup;
	bool MouseInput;
	bool KbInput;
	bool TopmostPopup;
	[PanelAnimationVar("alpha", "255")] float Alpha;
	IBorder? Border;
	IScheme? Scheme;
	PanelFlags Flags;
	readonly List<Panel> Children = [];
	readonly List<OverrideableColorEntry> OverrideableColorEntries = [];

	IPanel? SkipChild;
	bool IsSilent;

	Color BgColor;
	Color FgColor;

	PaintBackgroundType PaintBackgroundType;
	public PaintBackgroundType GetPaintBackgroundType() => PaintBackgroundType;
	public void SetPaintBackgroundType(PaintBackgroundType type) => PaintBackgroundType = type;

	public void SetDropEnabled(bool enabled, float hoverContextTime = 0) {
		// TODO
	}

	public void SetSilentMode(bool silent) => IsSilent = silent;

	List<IPanel> ActionSignalTargets = [];
	HashSet<IPanel> ActionSignalTargetsLookup = [];

	public void AddActionSignalTarget(IPanel? messageTarget) {
		if (messageTarget == null)
			return;

		if (ActionSignalTargetsLookup.Add(messageTarget))
			ActionSignalTargets.Add(messageTarget);
	}

	public void RemoveActionSignalTarget(IPanel? messageTarget) {
		if (messageTarget == null)
			return;

		if (ActionSignalTargetsLookup.Remove(messageTarget))
			ActionSignalTargets.Remove(messageTarget);
	}

	public void PostActionSignal(KeyValues message) {
		if (!IsSilent) {
			message.SetPtr("panel", this);
			int i;
			for (i = ActionSignalTargets.Count - 1; i > 0; i--) {
				IPanel? panel = ActionSignalTargets[i];
				if (panel != null)
					VGui.PostMessage(panel, message.MakeCopy(), this);
			}

			if (i == 0) {
				IPanel? panel = ActionSignalTargets[i];
				if (panel != null) {
					VGui.PostMessage(panel, message, this);
					return;
				}
			}
		}
	}

	public void PostMessage(IPanel target, KeyValues message, double delay = 0) {
		VGui.PostMessage(target, message, this, delay);
	}

	protected bool PassUnhandledInput = true;
	public void GetAbsPos(out int x, out int y) {
		x = AbsX;
		y = AbsY;
		Surface.OffsetAbsPos(ref x, ref y);
	}

	public Panel GetChild(int index) {
		return Children[index];
	}

	public int GetChildCount() {
		return Children.Count;
	}

	public IEnumerable<IPanel> GetChildren() {
		foreach (IPanel child in Children) {
			yield return child;
		}
	}

	public ReadOnlySpan<char> GetClassName() => GetType().Name;

	public void GetClipRect(out int x0, out int y0, out int x1, out int y1) {
		x0 = ClipRectX;
		y0 = ClipRectY;
		x1 = ClipRectW;
		y1 = ClipRectH;
	}


	public void GetPaintSize(out int wide, out int tall) {
		GetSize(out wide, out tall);
		if (Border != null) {
			Border.GetInset(out int left, out int top, out int right, out int bottom);
			wide -= left + right;
			tall -= top + bottom;
		}
	}

	public virtual void ApplySettings(KeyValues resourceData) {

	}

	public virtual IPanel? GetCurrentKeyFocus() {
		return null;
	}

	public void GetInset(out int left, out int top, out int right, out int bottom) {
		left = InsetLeft;
		top = InsetTop;
		right = InsetRight;
		bottom = InsetBottom;
	}

	public void GetMinimumSize(out int wide, out int tall) {
		wide = MinW;
		tall = MinH;
	}

	public ReadOnlySpan<char> GetName() {
		if (PanelName != null)
			return PanelName;

		return "";
	}

	public Panel? GetParent() => Parent;
	IPanel? IPanel.GetParent() => Parent;
	

	public void GetPos(out int x, out int y) {
		x = this.X;
		y = this.Y;
	}

	public IScheme? GetScheme() {
		if (Scheme != null)
			return Scheme;
		if (Parent != null)
			return Parent.GetScheme();

		return SchemeManager.GetDefaultScheme();
	}

	public void GetSize(out int wide, out int tall) {
		wide = W;
		tall = H;
	}

	public int GetTabPosition() {
		throw new NotImplementedException();
	}

	public void SetSkipChildDuringPainting(Panel child) => SkipChild = child;

	public int GetZPos() {
		return ZPos;
	}

	public bool HasParent(IPanel potentialParent) {
		IPanel? parent = this.Parent;

		while (parent != null) {
			if (parent == potentialParent)
				return true;
			parent = parent.GetParent();
		}

		return false;
	}

	public virtual void InternalFocusChanged(bool lost) {

	}

	public bool IsEnabled() {
		return Enabled;
	}

	public bool IsFullyVisible() {
		Panel? panel = this;
		while (panel != null) {
			if (!panel.Visible) {
				return false;
			}

			panel = panel.Parent;
		}

		return true;
	}

	public void SetProportional(bool state) {
		if (state != Flags.HasFlag(PanelFlags.IsProportional)) {
			Flags |= PanelFlags.IsProportional;
			for (int i = 0; i < GetChildCount(); i++) {
				GetChild(i).SetProportional(IsProportional());
			}
		}
		InvalidateLayout();
	}

	public bool IsKeyboardInputEnabled() => KbInput;

	public bool IsMouseInputEnabled() => MouseInput;

	public bool IsPopup() {
		return Popup;
	}

	public bool IsProportional() {
		return (Flags & PanelFlags.IsProportional) != 0;
	}

	public bool IsTopmostPopup() => TopmostPopup;

	public bool IsVisible() {
		return Visible;
	}

	public IPanel? IsWithinTraverse(int x, int y, bool traversePopups) {
		if (!IsVisible() || !IsMouseInputEnabled())
			return null;

		if (traversePopups) {
			int i;
			IList<Panel> children = Children;
			int childCount = children.Count();
			for (i = childCount - 1; i >= 0; i--) {
				IPanel? panel = children[i];
				if (panel.IsPopup()) {
					panel = panel.IsWithinTraverse(x, y, true);
					if (panel != null)
						return panel;
				}
			}

			for (i = childCount - 1; i >= 0; i--) {
				IPanel? panel = children[i];
				if (!panel.IsPopup()) {
					panel = panel.IsWithinTraverse(x, y, true);
					if (panel != null)
						return panel;
				}
			}

			if (!IsMouseInputDisabledForThisPanel() && IsWithin(x, y)) {
				return this;
			}
		}
		else {
			if (IsWithin(x, y)) {
				IList<Panel> children = Children;
				int childCount = children.Count();
				for (int i = childCount - 1; i >= 0; i--) {
					IPanel? panel = children[i];
					if (!panel.IsPopup()) {
						panel = panel.IsWithinTraverse(x, y, false);
						if (panel != null)
							return panel;
					}
				}

				if (!IsMouseInputDisabledForThisPanel())
					return this;
			}
		}

		return null;
	}

	private bool IsWithin(int x, int y) {
		if (x < ClipRectX)
			return false;

		if (y < ClipRectY)
			return false;

		if (x >= ClipRectW)
			return false;

		if (y >= ClipRectH)
			return false;

		return true;
	}

	private bool IsMouseInputDisabledForThisPanel() {
		return (Flags & PanelFlags.IsMouseDisabledForThisPanelOnly) != 0;
	}

	// These implementations break everything. Fix later

	public void MoveToBack() {
		/*
		if (Parent != null) {
			Parent.Children.Remove(this);
			Parent.Children.Insert(0, this);

			int i = 1;
			while (i < Parent.Children.Count) {
				if (Parent.Children[i].ZPos < ZPos) {
					Parent.Children[i - 1] = Parent.Children[i];
					Parent.Children[i] = this;
					i++;
				}
				else 
					break;
			}
		}
		*/
	}

	public void MoveToFront() {
		/*
		Surface.MovePopupToFront(this);

		if (Parent != null) {
			Parent.Children.Remove(this);
			Parent.Children.Add(this);

			int i = Parent.Children.Count - 2;
			while (i >= 0) {
				if (Parent.Children[i].ZPos > ZPos) {
					Parent.Children[i + 1] =  Parent.Children[i];
					Parent.Children[i]  = this;
					i--;
				}
				else 
					break;
			}
		}
		*/
	}


	public void InvalidateLayout(bool layoutNow = false, bool reloadScheme = false) {
		Flags |= PanelFlags.NeedsLayout;

		if (reloadScheme) {
			// make all our children reload the scheme
			Flags |= PanelFlags.NeedsSchemeUpdate;

			for (int i = 0; i < GetChildCount(); i++) {
				IPanel? panel = GetChild(i);
				if (panel != null) {
					panel.InvalidateLayout(layoutNow, true);
				}
			}

			PerformApplySchemeSettings();
		}

		if (layoutNow) {
			InternalPerformLayout();
			Repaint();
		}
	}

	private void InternalPerformLayout() {
		if (Flags.HasFlag(PanelFlags.NeedsSchemeUpdate))
			return;

		Flags |= PanelFlags.InPerformLayout;
		Flags &= ~PanelFlags.NeedsLayout;
		PerformLayout();
		Flags &= ~PanelFlags.InPerformLayout;
	}

	public virtual void PerformLayout() {

	}


	public void PaintTraverse(bool repaint, bool allowForce = true) {
		if (!IsVisible())
			return;

		float oldAlphaMultiplier = Surface.DrawGetAlphaMultiplier();
		float newAlphaMultiplier = oldAlphaMultiplier * Alpha * 1.0f / 255.0f;

		if (!repaint && allowForce && Flags.HasFlag(PanelFlags.NeedsRepaint)) {
			repaint = true;
			Flags &= ~PanelFlags.NeedsRepaint;
		}

		bool bPushedViewport = false;

		Span<int> clipRect = stackalloc int[4];
		GetClipRect(out clipRect[0], out clipRect[1], out clipRect[2], out clipRect[3]);
		if ((clipRect[2] <= clipRect[0]) || (clipRect[3] <= clipRect[1]))
			repaint = false;

		Surface.DrawSetAlphaMultiplier(newAlphaMultiplier);

		bool bBorderPaintFirst = Border != null ? Border.PaintFirst() : false;

		if (bBorderPaintFirst && repaint && Flags.HasFlag(PanelFlags.PaintBorderEnabled) && (Border != null)) {
			Surface.PushMakeCurrent(this, false);
			PaintBorder();
			Surface.PopMakeCurrent(this);
		}

		if (repaint) {
			if (Flags.HasFlag(PanelFlags.PaintBackgroundEnabled)) {
				Surface.PushMakeCurrent(this, false);
				PaintBackground();
				Surface.PopMakeCurrent(this);
			}

			if (Flags.HasFlag(PanelFlags.PaintEnabled)) {
				Surface.PushMakeCurrent(this, true);
				Paint();
				Surface.PopMakeCurrent(this);
			}
		}

		for (int i = 0, childCount = Children.Count; i < childCount; i++) {
			IPanel child = Children[i];
			bool bVisible = child.IsVisible();

			if (Surface.ShouldPaintChildPanel(child)) {
				if (bVisible) {
					child.PaintTraverse(repaint, allowForce);
				}
			}
			else {
				Surface.Invalidate(child);

				if (bVisible)
					child.PaintTraverse(false, false);
			}
		}

		if (repaint) {
			if (!bBorderPaintFirst && Flags.HasFlag(PanelFlags.PaintBorderEnabled) && (Border != null)) {
				Surface.PushMakeCurrent(this, false);
				PaintBorder();
				Surface.PopMakeCurrent(this);
			}

			if (Flags.HasFlag(PanelFlags.PostChildPaintEnabled)) {
				Surface.PushMakeCurrent(this, false);
				PostChildPaint();
				Surface.PopMakeCurrent(this);
			}
		}

		Surface.DrawSetAlphaMultiplier(oldAlphaMultiplier);

		Surface.SwapBuffers(this);

		if (bPushedViewport) {
			// surface()->PopFullscreenViewport();
			// ^^ todo: later
		}
	}

	public virtual void PostChildPaint() {

	}

	public virtual void Paint() {

	}

	public void GetBgColor(in Color c) => BgColor = c;
	public void GetFgColor(in Color c) => FgColor = c;
	public Color GetBgColor() => BgColor;
	public Color GetFgColor() => FgColor;

	public virtual void SetTabPosition(int pos) { }

	public virtual void PaintBackground() {
		GetSize(out int wide, out int tall);
		if (SkipChild != null && SkipChild.IsVisible()) {
			if (GetPaintBackgroundType() == PaintBackgroundType.Box) {
				GetCornerTextureSize(out int cornerWide, out int cornerTall);

				Color col = GetBgColor();
				DrawHollowBox(0, 0, wide, tall, col, 1.0f);

				wide -= 2 * cornerWide;
				tall -= 2 * cornerTall;

				FillRectSkippingPanel(GetBgColor(), cornerWide, cornerTall, wide, tall, SkipChild);
			}
			else {
				FillRectSkippingPanel(GetBgColor(), 0, 0, wide, tall, SkipChild);
			}
		}
		else {
			Color col = GetBgColor();

			switch (PaintBackgroundType) {
				default:
				case PaintBackgroundType.Filled: {
						Surface.DrawSetColor(col);
						Surface.DrawFilledRect(0, 0, wide, tall);
					}
					break;
				case PaintBackgroundType.Textured: {
						DrawTexturedBox(0, 0, wide, tall, col, 1.0f);
					}
					break;
				case PaintBackgroundType.Box: {
						DrawBox(0, 0, wide, tall, col, 1.0f);
					}
					break;
				case PaintBackgroundType.BoxFade: {
						DrawBoxFade(0, 0, wide, tall, col, 1.0f, 255, 0, true);
					}
					break;
			}
		}
	}

	private void DrawBox(int v1, int v2, int wide, int tall, Color col, float v3) {
		throw new NotImplementedException();
	}

	private void DrawBoxFade(int v1, int v2, int wide, int tall, Color col, float v3, int v4, int v5, bool v6) {
		throw new NotImplementedException();
	}

	private void DrawTexturedBox(int v1, int v2, int wide, int tall, Color col, float v3) {
		throw new NotImplementedException();
	}

	private void FillRectSkippingPanel(in Color color, int cornerWide, int cornerTall, int wide, int tall, IPanel skipChild) {
		throw new NotImplementedException();
	}

	private void DrawHollowBox(int v1, int v2, int wide, int tall, Color col, float v3) {
		throw new NotImplementedException();
	}

	private void GetCornerTextureSize(out int cornerWide, out int cornerTall) {
		throw new NotImplementedException();
	}

	private void PaintBorder() {
		Border!.Paint(this);
	}

	public void PerformApplySchemeSettings() {
		if (Flags.HasFlag(PanelFlags.NeedsDefaultSettingsApplied)) {
			// InternalInitDefaultValues(GetAnimMap());
		}

		if (Flags.HasFlag(PanelFlags.NeedsSchemeUpdate)) {
			IScheme? scheme = GetScheme();
			Assert(scheme != null);
			if (scheme != null) {
				ApplySchemeSettings(scheme);
				ApplyOverridableColors();
			}
		}
	}

	public void SetBgColor(in Color color) => BgColor = color;
	public void SetFgColor(in Color color) => FgColor = color;

	// This in theory will replicate the pointer logic?
	private void ApplyOverridableColors() {
		Span<OverrideableColorEntry> entries = CollectionsMarshal.AsSpan(OverrideableColorEntries);
		for (int i = 0, c = entries.Length; i < c; i++) {
			ref OverrideableColorEntry entry = ref entries[i];
			if (entry.Overridden)
				entry.Func = (in OverrideableColorEntry e) => e.ColorFromScript;
		}
	}

	public Color GetSchemeColor(ReadOnlySpan<char> keyName, IScheme scheme) {
		return scheme.GetColor(keyName, new(255, 255, 255, 255));
	}
	public Color GetSchemeColor(ReadOnlySpan<char> keyName, Color defaultColor, IScheme scheme) {
		return scheme.GetColor(keyName, defaultColor);
	}
	public virtual void ApplySchemeSettings(IScheme scheme) {
		SetFgColor(GetSchemeColor("Panel.FgColor", scheme));
		SetBgColor(GetSchemeColor("Panel.BgColor", scheme));

		Flags &= ~PanelFlags.NeedsSchemeUpdate;
	}

	public void Repaint() {
		Flags |= PanelFlags.NeedsRepaint;
		Surface?.Invalidate(this);
	}

	public void RequestFocus(int direction = 0) {
		OnRequestFocus(this, null);
	}

	public void CallParentFunction(KeyValues message) => GetParent()?.SendMessage(message, this);

	public virtual void OnRequestFocus(Panel subFocus, Panel? defaultPanel) 
		=> CallParentFunction(new KeyValues("OnRequestFocus").AddSubKey(new("subFocus", subFocus)).AddSubKey(new("defaultPanel", defaultPanel)));

	public bool RequestFocusNext(IPanel? existingPanel = null) {
		return false;
	}

	public bool RequestFocusPrev(IPanel? existingPanel = null) {
		return false;
	}

	public void SetBounds(int x, int y, int wide, int tall) {
		SetPos(x, y);
		SetSize(wide, tall);
	}

	CursorCode Cursor;
	public void SetCursor(CursorCode cursor) {
		this.Cursor = cursor;
	}

	public void SetEnabled(bool state) {
		if (state != IsEnabled()) {
			Enabled = state;
			InvalidateLayout();
			Repaint();
		}
	}

	public void SetInset(int left, int top, int right, int bottom) {
		throw new NotImplementedException();
	}

	public void SetKeyboardInputEnabled(bool state) {
		KbInput = state;
	}

	public void SetMinimumSize(int wide, int tall) {
		MinW = (short)wide;
		MinH = (short)tall;

		int currentWidth = W;
		if (currentWidth < wide)
			currentWidth = wide;

		int currentHeight = H;
		if (currentHeight < tall)
			currentHeight = tall;

		if (currentWidth != W || currentHeight != H)
			SetSize(currentWidth, currentHeight);
	}

	public void SetMouseInputEnabled(bool state) {
		MouseInput = state;
	}

	public void SetName(ReadOnlySpan<char> panelName) {
		if (this.PanelName != null && panelName != null && !panelName.Equals(this.PanelName, StringComparison.Ordinal))
			return;

		if (this.PanelName != null)
			panelName = null;

		if (panelName != null)
			this.PanelName = new(panelName);
	}

	public void SetParent(IPanel? newParent) {
		if (this == newParent)
			return;

		if (Parent == newParent)
			return;

		if (Parent != null) {
			Parent?.Children.Remove(this);
			Parent = null;
		}

		if (newParent != null) {
			Parent = (Panel)newParent!;
			Parent.Children.Add(this);
			SetZPos(ZPos);
			Parent.OnChildAdded(this);
		}
	}

	public void SetPopup(bool enabled) {
		Popup = enabled;
	}
	public void MakePopup(bool showTaskbarIcon, bool disabled = false) {
		Surface.CreatePopup(this, false, showTaskbarIcon, disabled);
	}

	public void SetPos(int x, int y) {
		this.X = (short)x;
		this.Y = (short)y;
	}

	public void SetSize(int wide, int tall) {
		if (wide < MinW)
			wide = MinW;
		if (tall < MinH)
			tall = MinH;

		if (W == wide && H == tall)
			return;

		W = (short)wide;
		H = (short)tall;

		OnSizeChanged(wide, tall);
	}

	public void SetTopmostPopup(bool state) {
		throw new NotImplementedException();
	}

	public virtual void SetVisible(bool state) {
		if (Visible == state)
			return;

		// need to tell the surface later... UGH... HOW DO WE GET THE SURFACE RELIABLY HERE??
		Visible = state;
	}

	public void SetZPos(int z) {
		ZPos = (short)z;
		if (Parent != null) {
			int childCount = Parent.GetChildCount();
			int i;
			for (i = 0; i < childCount; i++) {
				if (Parent.GetChild(i) == this)
					break;
			}

			if (i == childCount)
				return;

			while (true) {
				Panel? prevChild = null, nextChild = null;
				if (i > 0)
					prevChild = Parent.GetChild(i - 1);
				if (i < (childCount - 1))
					nextChild = Parent.GetChild(i + 1);

				if (i > 0 && prevChild != null && prevChild.ZPos > ZPos) {
					// Swap with lower
					Parent.Children[i] = prevChild;
					Parent.Children[i - 1] = this;
				}
				else if (i < (childCount - 1) && nextChild != null && nextChild.ZPos < ZPos) {
					Parent.Children[i] = nextChild;
					Parent.Children[i + 1] = this;
				}
				else
					break;
			}
		}
	}

	public void Solve() {
		GetPos(out int x, out int y);
		GetSize(out int w, out int h);

		IPanel? parent = GetParent();
		if (IsPopup()) {
			// if we're a popup, draw at the highest level
			parent = Surface.GetEmbeddedPanel();
		}

		int pabsX = 0, pabsY = 0;
		parent?.GetAbsPos(out pabsX, out pabsY);

		// TODO: pin siblings.
		// Need to look into how those work 

		int absX = x;
		int absY = y;
		AbsX = (short)x;
		AbsY = (short)y;

		// put into parent space
		int pinsetX = 0, pinsetY = 0, pinsetW = 0, pinsetH = 0;
		if (parent != null) {
			parent.GetInset(out pinsetX, out pinsetY, out pinsetW, out pinsetH);

			absX += pabsX + pinsetX;
			absY += pabsY + pinsetY;

			AbsX = (short)Math.Clamp(absX, short.MinValue, short.MaxValue);
			AbsY = (short)Math.Clamp(absY, short.MinValue, short.MaxValue);
		}

		// set initial bounds
		ClipRectX = AbsX;
		ClipRectY = AbsY;

		int absX2 = absX + w;
		int absY2 = absY + h;
		ClipRectW = (short)Math.Clamp(absX2, short.MinValue, short.MaxValue);
		ClipRectH = (short)Math.Clamp(absY2, short.MinValue, short.MaxValue);

		// clip to parent, if we're not a popup
		if (parent != null && !IsPopup()) {
			parent.GetClipRect(out int pclipX, out int pclipY, out int pclipW, out int pclipH);

			if (ClipRectX < pclipX) {
				ClipRectX = (short)pclipX;
			}

			if (ClipRectY < pclipY) {
				ClipRectY = (short)pclipY;
			}

			if (ClipRectW > pclipW) {
				ClipRectW = (short)(pclipW - pinsetW);
			}

			if (ClipRectH > pclipH) {
				ClipRectH = (short)(pclipH - pclipH);
			}

			if (ClipRectX > ClipRectW) {
				ClipRectW = ClipRectX;
			}

			if (ClipRectY > ClipRectH) {
				ClipRectH = ClipRectY;
			}
		}

		Assert(ClipRectX <= ClipRectW);
		Assert(ClipRectY <= ClipRectH);
	}

	public void Think() {
		if (IsVisible()) {
			// TODO: Tooltips layout
			if ((Flags & PanelFlags.NeedsLayout) != 0)
				InternalPerformLayout();
		}

		OnThink();
	}

	public void SetPanelBorderEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintEnabled : Flags &= ~PanelFlags.PaintEnabled;
	public void SetPaintBackgroundEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintBackgroundEnabled : Flags &= ~PanelFlags.PaintBackgroundEnabled;
	public void SetPaintBorderEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintBorderEnabled : Flags &= ~PanelFlags.PaintBorderEnabled;
	public void SetPaintEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintEnabled : Flags &= ~PanelFlags.PaintEnabled;
	public void SetPostChildPaintEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PostChildPaintEnabled : Flags &= ~PanelFlags.PostChildPaintEnabled;

	IPanel IPanel.GetChild(int index) => GetChild(index);

	public int GetX() => X;
	public int GetY() => Y;
	public int GetWide() => W;
	public int GetTall() => H;
	public void SetWide(int wide) => SetSize(wide, GetTall());
	public void SetTall(int tall) => SetSize(GetWide(), tall);

	public void TraverseLevel(int v) {

	}

	public bool IsMarkedForDeletion() => Flags.HasFlag(PanelFlags.MarkedForDeletion);
	public void MarkForDeletion() {
		if (Flags.HasFlag(PanelFlags.MarkedForDeletion))
			return;

		Flags |= PanelFlags.MarkedForDeletion;
		Flags &= ~PanelFlags.AutoDeleteEnabled;
		VGui.MarkPanelForDeletion(this);
	}

	public bool HasFocus() => Input.GetFocus() == this;

	public virtual void OnCommand(ReadOnlySpan<char> command) { }
	public virtual void OnMouseCaptureLost() {

	}
	public virtual void OnSetFocus() {
		Repaint();
	}
	public virtual void OnKillFocus(Panel? newPanel) {
		Repaint();
	}
	public virtual void OnThink() { }
	public virtual void OnChildAdded(IPanel child) { }
	public virtual void OnSizeChanged(int newWide, int newTall) {
		InvalidateLayout();
	}
	public virtual void OnCursorMoved(int x, int y) { }
	public virtual void OnCursorEntered() { }
	public virtual void OnCursorExited() { }
	public virtual void OnMousePressed(ButtonCode code) { }
	public virtual void OnMouseDoublePressed(ButtonCode code) { }
	public virtual void OnMouseReleased(ButtonCode code) { }
	public virtual void OnMouseMismatchedRelease(ButtonCode code, IPanel? pressedPanel) { }
	public virtual void OnMouseWheeled(int delta) { }
	bool TriplePressAllowed;
	public virtual void SetTriplePressAllowed(bool state) => TriplePressAllowed = state;
	public virtual bool IsTriplePressAllowed() => TriplePressAllowed;
	public virtual void OnMouseTriplePressed(ButtonCode code) { }

	public CursorCode GetCursor() => Cursor;

	public void InternalSetCursor() {
		bool visible = IsVisible();
		if (visible) {
			IPanel? p = GetParent();
			while (p != null) {
				visible &= p.IsVisible();
				p = p.GetParent();
			}

			if (visible && HasParent(Surface.GetEmbeddedPanel())) {
				CursorCode cursor = GetCursor();
				if (Input.GetCursorOveride() != 0)
					cursor = Input.GetCursorOveride();
				Surface.SetCursor((HCursor)cursor);
			}
		}
	}

	bool AutoDelete;
	public virtual void SetAutoDelete(bool state) {
		AutoDelete = state;
	}
	public bool IsAutoDeleteSet() => AutoDelete;

	public virtual void Dispose() {
		Flags &= ~PanelFlags.AutoDeleteEnabled;
		Flags |= PanelFlags.MarkedForDeletion;

		SetParent(null);
		while (GetChildCount() > 0) {
			IPanel child = GetChild(0);
			if (child.IsAutoDeleteSet())
				child.DeletePanel();
			else
				child.SetParent(null);
		}

		GC.SuppressFinalize(this);
	}

	public void DeletePanel() {
		Flags |= PanelFlags.MarkedForDeletion;
		Flags &= ~PanelFlags.AutoDeleteEnabled;
		Dispose();
	}

	public virtual void OnKeyCodePressed(ButtonCode code) { }
	public virtual void OnKeyCodeTyped(ButtonCode code) { }
	public virtual void OnKeyTyped(char unichar) { }
	public virtual void OnKeyCodeReleased(ButtonCode code) { }
	public virtual void OnUnhandledMouseClick(ButtonCode code) { }
	public void InternalKeyFocusTicked() {
		OnKeyFocusTicked();
	}
	public virtual void OnKeyFocusTicked() { }
	public void InternalMouseFocusTicked() {
		InternalSetCursor();
		OnMouseFocusTicked();
	}
	public virtual void OnMouseFocusTicked() {
		// Send it to the parent
		GetParent()?.OnMouseFocusTicked();
	}
	public virtual void OnClose() { }
	public virtual void OnDelete() {
		Dispose();
	}

	public void LocalToScreen(ref int x, ref int y) {
		GetAbsPos(out int px, out int py);
		x += px;
		y += py;
	}

	public void ScreenToLocal(ref int x, ref int y) {
		GetAbsPos(out int px, out int py);
		x -= px;
		y -= py;
	}
	public void ParentLocalToScreen(ref int x, ref int y) {
		int px = 0, py = 0;
		GetParent()?.GetAbsPos(out px, out py);
		x += px;
		y += py;
	}

	static ConVar vgui_print_messages = new("0", FCvar.None);

	public void InternalKeyCodePressed(ButtonCode code) {
		if (!ShouldHandleInputMessage())
			return;

		if (IsKeyboardInputEnabled()) 
			OnKeyCodePressed(code);
		else 
			CallParentFunction(new KeyValues("KeyCodePressed", "code", (int)code));
	}

	static bool SuppressRebindChecks = false;

	public bool ShouldHandleInputMessage() {
		if (Input.GetModalSubTree() == null)
			return true;

		bool childOfModal = false;
		IPanel? subTree = Input.GetModalSubTree();
		if (subTree == null)
			childOfModal = true;
		else if (HasParent(subTree)) 
			childOfModal = true;

		if (Input.ShouldModalSubTreeReceiveMessages())
			return childOfModal;

		return !childOfModal;
	}

	public void InternalKeyCodeTyped(ButtonCode code) {
		if (!ShouldHandleInputMessage()) {
			Input.OnKeyCodeUnhandled(code);
			return;
		}

		if (IsKeyboardInputEnabled()) {
			bool shift = Input.IsKeyDown(ButtonCode.KeyLShift) || Input.IsKeyDown(ButtonCode.KeyRShift);
			bool ctrl =  Input.IsKeyDown(ButtonCode.KeyLControl) || Input.IsKeyDown(ButtonCode.KeyRControl);
			bool alt =   Input.IsKeyDown(ButtonCode.KeyLAlt) || Input.IsKeyDown(ButtonCode.KeyRAlt);

			KeyModifier modifiers = 0;
			if (shift) 
				modifiers |= KeyModifier.Shift;
			if (ctrl) 
				modifiers |= KeyModifier.Control;
			if (alt) 
				modifiers |= KeyModifier.Alt;
			

			if (!SuppressRebindChecks && IsKeyRebound(code, modifiers)) {
				return;
			}

			bool oldVal = SuppressRebindChecks;
			SuppressRebindChecks = true;
			OnKeyCodeTyped(code);
			SuppressRebindChecks = oldVal;
		}
		else {
			if (this == Surface.GetEmbeddedPanel()) 
				Input.OnKeyCodeUnhandled(code);
			
			CallParentFunction(new KeyValues("KeyCodeTyped", "code", (int)code));
		}
	}

	private bool IsKeyRebound(ButtonCode code, KeyModifier modifiers) {
		return false;
	}

	public void InternalKeyCodeReleased(ButtonCode code) {
		if (!ShouldHandleInputMessage())
			return;

		if (IsKeyboardInputEnabled()) 
			OnKeyCodeReleased(code);
		else 
			CallParentFunction(new KeyValues("KeyCodeReleased", "code", (int)code));
	}

	public bool IsConsoleStylePanel() => false; // todo

	public virtual void OnMessage(KeyValues message, IPanel? from) {
		switch (message.Name) {
			case "KeyCodePressed": InternalKeyCodePressed((ButtonCode)message.GetInt("code")); break;
			case "KeyCodeTyped": InternalKeyCodeTyped((ButtonCode)message.GetInt("code")); break;
			case "KeyTyped": OnKeyTyped((char)message.GetInt("unichar")); break;
			case "KeyCodeReleased": InternalKeyCodeReleased((ButtonCode)message.GetInt("code")); break;
			case "CursorEntered": OnCursorEntered(); break;
			case "CursorExited": OnCursorExited(); break;
			case "CursorMoved": OnCursorMoved(message.GetInt("xpos"), message.GetInt("ypos")); break;
			case "MouseFocusTicked": InternalMouseFocusTicked(); break;
			case "KeyFocusTicked": InternalKeyFocusTicked(); break;
			case "MouseCaptureLost": OnMouseCaptureLost(); break;
			case "MousePressed": OnMousePressed((ButtonCode)message.GetInt("code")); break;
			case "MouseReleased": OnMouseReleased((ButtonCode)message.GetInt("code")); break;
			case "UnhandledMouseClick": OnUnhandledMouseClick((ButtonCode)message.GetInt("code")); break;
			case "SetFocus": InternalSetFocus(); break;
			case "KillFocus": InternalKillFocus((Panel?)message.GetPtr("newPanel")); break;
			case "TextChanged": OnTextChanged((Panel)message.GetPtr("panel")!); break;
			case "Delete": OnDelete(); break;
			case "Close": OnClose(); break;
			case "OnRequestFocus": OnRequestFocus(message.GetPtr<Panel>("subFocus")!, message.GetPtr<Panel>("defaultPanel")); break;
			case "Command": OnCommand(message.GetString("command")); break;
		}
		if (vgui_print_messages.GetBool())
			if (vgui_print_messages.GetInt() == 2 || !message.Name.Contains("Ticked"))
				Msg($"Message from {from} to {this}: {message.Name}\n");
	}

	public virtual void OnTextChanged(Panel from) {

	}

	private void InternalSetFocus() {
		OnSetFocus();
	}

	private void InternalKillFocus(Panel? newPanel) {
		OnKillFocus(newPanel);
	}


	public void OnTick() {
		throw new NotImplementedException();
	}

	public void SendMessage(KeyValues parms, IPanel? from) {
		OnMessage(parms, from);
	}

	public void SetAllowKeyBindingChainToParent(bool state) {
		if (state)
			Flags |= PanelFlags.AllowChainKeybindingToParent;
		else
			Flags &= ~PanelFlags.AllowChainKeybindingToParent;
	}

	public virtual bool RequestInfo(KeyValues outputData) {
		return InternalRequestInfo(GetAnimMap(), outputData) || (((Panel?)GetParent())?.RequestInfo(outputData) ?? false);
	}

	private bool InternalRequestInfo(PanelAnimationMap? map, KeyValues outputData) {
		if (map == null)
			return false;

		Assert(outputData);

		ReadOnlySpan<char> name = outputData.Name;

		ref PanelAnimationMapEntry e = ref FindPanelAnimationEntry(name, map);
		if (!Unsafe.IsNullRef(ref e)) {
			IPanelAnimationPropertyConverter? converter = FindConverter(e.Type);
			if (converter != null) {
				converter.GetData(this, outputData, ref e);
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Note: modifying the animation map may result in this handle being invalid!
	/// </summary>
	/// <param name="name"></param>
	/// <param name="map"></param>
	/// <returns></returns>
	private ref PanelAnimationMapEntry FindPanelAnimationEntry(ReadOnlySpan<char> scriptname, PanelAnimationMap map) {
		if (map == null)
			return ref Unsafe.NullRef<PanelAnimationMapEntry>();

		int c = map.Entries.Count;
		Span<PanelAnimationMapEntry> entries = map.Entries.AsSpan();
		for (int i = 0; i < c; i++) {
			ref PanelAnimationMapEntry e = ref entries[i];

			if (((ReadOnlySpan<char>)e.ScriptName).Equals(scriptname, StringComparison.OrdinalIgnoreCase)) {
				return ref e;
			}
		}

		if (map.BaseMap != null)
			return ref FindPanelAnimationEntry(scriptname, map.BaseMap);

		return ref Unsafe.NullRef<PanelAnimationMapEntry>();
	}

	public virtual bool SetInfo(KeyValues inputData) {
		if (InternalSetInfo(GetAnimMap(), inputData))
			return true;
		return false;
	}

	private bool InternalSetInfo(PanelAnimationMap? map, KeyValues inputData) {
		if (map == null)
			return false;

		Assert(inputData != null);
		ReadOnlySpan<char> name = inputData.Name;

		ref PanelAnimationMapEntry e = ref FindPanelAnimationEntry(name, map);
		if (!Unsafe.IsNullRef(ref e)) {
			IPanelAnimationPropertyConverter? converter = FindConverter(e.Type);
			if (converter != null) {
				converter.SetData(this, inputData, ref e);
				return true;
			}
		}

		return false;
	}

	IPanel? IPanel.FindChildByName(ReadOnlySpan<char> childName, bool recurseDown) => FindChildByName(childName, recurseDown);

	public Panel? FindChildByName(ReadOnlySpan<char> childName, bool recurseDown) {
		return null; // todo: impl
	}

	public void GetBounds(out int x, out int y, out int w, out int h) {
		GetPos(out x, out y);
		GetSize(out w, out h);
	}

	public static void AddToAnimationMap(Type t, ReadOnlySpan<char> scriptName, ReadOnlySpan<char> type, ReadOnlySpan<char> var, ReadOnlySpan<char> defaultValue, bool array, PanelGetFunc get, PanelSetFunc set) {
		PanelAnimationMap map = PanelAnimationDictionary.FindOrAddPanelAnimationMap(t.Name);

		PanelAnimationMapEntry entry = new() {
			ScriptName = new string(scriptName),
			Variable = new string(var),
			Type = new string(type),
			DefaultValue = new string(defaultValue),
			Array = array,
			Get = get,
			Set = set
		};
		map.Entries.Add(entry);
	}
	public static void ChainToAnimationMap<T>() where T : Panel => ChainToAnimationMap(typeof(T));
	public static void ChainToAnimationMap(Type t) {
		foreach (var field in t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
			PanelAnimationVarAttribute? attr = field.GetCustomAttribute<PanelAnimationVarAttribute>();
			if (attr == null)
				continue;

			PanelAnimationVarAttribute.InitVar(t, attr, field);
		}

		PanelAnimationMap map = PanelAnimationDictionary.FindOrAddPanelAnimationMap(t.Name);
		map.ClassName = t.Name;
		Type? baseClass = t.BaseType;
		if (baseClass?.IsAssignableTo(typeof(Panel)) ?? false)
			map.BaseMap = PanelAnimationDictionary.FindOrAddPanelAnimationMap(baseClass.Name);
	}

	public virtual PanelAnimationMap GetAnimMap() => PanelAnimationDictionary.FindOrAddPanelAnimationMap(GetType().Name);

	public static void InitializeControls() {
		var types = ReflectionUtils.GetLoadedTypes().Where(type => typeof(Panel).IsAssignableFrom(type));
		foreach (var type in types) {
			ChainToAnimationMap(type);
			Msg($"VGUI: Initializing {type.Name}\n");
		}
	}

	public virtual IPanel? GetDragPanel() {
		return GetParent()?.GetDragPanel();
	}
}

class FloatProperty : IPanelAnimationPropertyConverter
{
	public void GetData(Panel panel, KeyValues kv, ref PanelAnimationMapEntry entry) {
		object? data = entry.Get(panel);
		if (data == null) return;
		kv.SetFloat(entry.ScriptName, (float)data);
	}

	public void SetData(Panel panel, KeyValues kv, ref PanelAnimationMapEntry entry) {
		entry.Set(panel, kv.GetFloat(entry.ScriptName));
	}

	public void InitFromDefault(Panel panel, ref PanelAnimationMapEntry entry) {
		entry.Set(panel, float.TryParse(entry.DefaultValue, out float r) ? r : 0);
	}
}