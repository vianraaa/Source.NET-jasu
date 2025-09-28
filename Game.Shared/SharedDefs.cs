#if !CLIENT_DLL && !GAME_DLL
global using EHANDLE = Source.Common.BaseHandle; // < Intellisense compatibility
#endif

global using static Game.Shared.SharedDefs;

using System.Runtime.CompilerServices;
using Source;

namespace Game.Shared;

public static class SharedDefs
{
	public const int SIMULATION_TIME_WINDOW_BITS = 8;

	public const int NOINTERP_PARITY_MAX = 4;
	public const int NOINTERP_PARITY_MAX_BITS = 2;
	public const int ANIMATION_CYCLE_BITS = 15;

	public const int MAX_VIEWMODELS = 3;
	public const int MAX_BEAM_ENTS = 10;
	public const int MAX_WEAPONS = 256;

	public const int NUM_AUDIO_LOCAL_SOUNDS = 8;

	public const int MAX_SUIT_DEVICES = 3;
	public const int MAX_AMMO_SLOTS = 256;
}


public enum HideHudBits
{
	WeaponSelection = 1 << 0,
	Flashlight = 1 << 1,
	All = 1 << 2,
	Health = 1 << 3,
	PlayerDead = 1 << 4,
	NeedSuit = 1 << 5,
	MiscStatus = 1 << 6,
	Chat = 1 << 7,
	Crosshair = 1 << 8,
	VehicleCrosshair = 1 << 9,
	InVehicle = 1 << 10,
	BonusProgress = 1 << 11,

	BitCount = 12
}


[InlineArray(NUM_AUDIO_LOCAL_SOUNDS)] public struct InlineArrayNumLocalAudioSounds<T> { public T item; }
[InlineArray(MAX_AMMO_SLOTS)] public struct InlineArrayMaxAmmoSlots<T> { public T item; }
[InlineArray(Constants.MAX_PLAYERS)] public struct InlineArrayMaxPlayers<T> { public T item; }
[InlineArray(Constants.MAX_PLAYERS + 1)] public struct InlineArrayMaxPlayersPlusOne<T> { public T item; }

[InlineArray(MAX_WEAPONS)]
public struct InlineArrayNewMaxWeapons<T> where T : new()
{
	public T item;
	public InlineArrayNewMaxWeapons() { for (int i = 0; i < MAX_WEAPONS; i++) this[i] = new(); }
}


[InlineArray(MAX_VIEWMODELS)]
public struct InlineArrayNewMaxViewmodels<T> where T : new()
{
	public T item;
	public InlineArrayNewMaxViewmodels() { for (int i = 0; i < MAX_VIEWMODELS; i++) this[i] = new(); }
}
[InlineArray(Constants.MAX_PLAYERS)]
public struct InlineArrayNewMaxPlayers<T> where T : new()
{
	public T item;
	public InlineArrayNewMaxPlayers() { for (int i = 0; i < MAX_VIEWMODELS; i++) this[i] = new(); }
}

[InlineArray(Constants.MAX_PLAYERS + 1)]
public struct InlineArrayNewMaxPlayersPlus1<T> where T : new()
{
	public T item;
	public InlineArrayNewMaxPlayersPlus1() { for (int i = 0; i < MAX_VIEWMODELS; i++) this[i] = new(); }
}
