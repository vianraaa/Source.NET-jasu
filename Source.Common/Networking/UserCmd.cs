using Source.Common.Bitbuffers;
using Source.Common.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using static Source.Constants;

namespace Source.Common.Networking;

public struct UserCmd
{
	public void Reset() {
		CommandNumber = 0;
		TickCount = 0;
		ViewAngles = new();
		ForwardMove = 0.0f;
		Sidemove = 0.0f;
		Upmove = 0.0f;
		Buttons = 0;
		Impulse = 0;
		WeaponSelect = 0;
		WeaponSubtype = 0;
		RandomSeed = 0;
		MouseDeltaX = 0;
		MouseDeltaY = 0;

		HasBeenPredicted = false;

		if (ButtonsPressed == null) ButtonsPressed = new byte[5];
		for (int i = 0; i < ButtonsPressed.Length; i++) ButtonsPressed[i] = 0;
		ScrollWheelSpeed = 0;
		WorldClicking = false;
		WorldClickDirection = new(0);
		IsTyping = false;
		if (MotionSensorPositions == null) MotionSensorPositions = new Vector3[20];
		for (int i = 0; i < MotionSensorPositions.Length; i++) MotionSensorPositions[i] = new(0);
		Forced = false;
	}
	public const int WEAPON_SUBTYPE_BITS = 6;

	/// <summary>
	/// For matching server and client commands for debugging
	/// </summary>
	public int CommandNumber;

	/// <summary>
	/// the tick the client created this command
	/// </summary>
	public int TickCount;

	/// <summary>
	/// Player instantaneous view angles.
	/// </summary>
	public QAngle ViewAngles;



	/// <summary>
	/// forward velocity.
	/// </summary>
	public float ForwardMove;
	/// <summary>
	///  sideways velocity.
	/// </summary>
	public float Sidemove;
	/// <summary>
	///  upward velocity.
	/// </summary>
	public float Upmove;

	/// <summary>
	/// Attack button states
	/// </summary>
	public int Buttons;

	/// <summary>
	/// Impulse command issued.
	/// </summary>
	public byte Impulse;

	/// <summary>
	/// Current weapon id
	/// </summary>
	public int WeaponSelect;
	public int WeaponSubtype;

	/// <summary>
	/// For shared random functions
	/// </summary>
	public int RandomSeed;

	/// <summary>
	/// mouse accum in x from create move
	/// </summary>
	public short MouseDeltaX;
	/// <summary>
	/// mouse accum in y from create move
	/// </summary>
	public short MouseDeltaY;

	/// <summary>
	/// Client only, tracks whether we've predicted this command at least once
	/// </summary>
	public bool HasBeenPredicted;

	/// <summary>
	/// Holds current buttons pressed, and sends to server used for PlayerButtonDown and other hooks serverside.
	/// </summary>
	public byte[] ButtonsPressed;

	/// <summary>
	/// Scroll wheel speed.
	/// </summary>
	public sbyte ScrollWheelSpeed;

	/// <summary>
	/// Currently world clicking? Used for context menu aiming.
	/// </summary>
	public bool WorldClicking;

	/// <summary>
	/// World click direction.
	/// </summary>
	public Vector3 WorldClickDirection;

	/// <summary>
	/// Performs the hand -> ear animation
	/// </summary>
	public bool IsTyping;

	/// <summary>
	/// Kinect stuff
	/// </summary>
	public Vector3[] MotionSensorPositions;

	/// <summary>
	/// Gmod wiki
	/// </summary>
	public bool Forced;

	public static void ReadUsercmd(bf_read buf, ref UserCmd move, ref UserCmd from) {

	}

	static bool HasChanged<T>(T[] from, T[] to) where T : IEquatable<T> {
		if (from == null && to == null)
			return false;

		if (from == null || to == null) {
			// which ones not null
			T[] notNull = from ?? to ?? throw new Exception();
			for (int i = 0; i < notNull.Length; i++) {
				if (!notNull[i].Equals(default))
					return true;
			}

			return false;
		}

		if (from.Length != to.Length) return true;

		for (int i = 0; i < from.Length; i++) {
			if (!from[i].Equals(to[i]))
				return true;
		}

		return false;
	}

	public static void WriteUsercmd(bf_write buf, in UserCmd to, in UserCmd from) {
		if (to.CommandNumber != from.CommandNumber + 1) {
			buf.WriteOneBit(1);
			buf.WriteUBitLong((uint)to.CommandNumber, 32);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.TickCount != from.TickCount + 1) {
			buf.WriteOneBit(1);
			buf.WriteUBitLong((uint)to.TickCount, 32);
		}
		else {
			buf.WriteOneBit(0);
		}


		if (to.ViewAngles[0] != from.ViewAngles[0]) {
			buf.WriteOneBit(1);
			buf.WriteFloat(to.ViewAngles[0]);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.ViewAngles[1] != from.ViewAngles[1]) {
			buf.WriteOneBit(1);
			buf.WriteFloat(to.ViewAngles[1]);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.ViewAngles[2] != from.ViewAngles[2]) {
			buf.WriteOneBit(1);
			buf.WriteFloat(to.ViewAngles[2]);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.ForwardMove != from.ForwardMove) {
			buf.WriteOneBit(1);
			buf.WriteFloat(to.ForwardMove);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.Sidemove != from.Sidemove) {
			buf.WriteOneBit(1);
			buf.WriteFloat(to.Sidemove);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.Upmove != from.Upmove) {
			buf.WriteOneBit(1);
			buf.WriteFloat(to.Upmove);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.Buttons != from.Buttons) {
			buf.WriteOneBit(1);
			buf.WriteUBitLong((uint)to.Buttons, 32);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.Impulse != from.Impulse) {
			buf.WriteOneBit(1);
			buf.WriteUBitLong(to.Impulse, 8);
		}
		else {
			buf.WriteOneBit(0);
		}


		if (to.WeaponSelect != from.WeaponSelect) {
			buf.WriteOneBit(1);
			buf.WriteUBitLong((uint)to.WeaponSelect, MAX_EDICT_BITS);

			if (to.WeaponSubtype != from.WeaponSubtype) {
				buf.WriteOneBit(1);
				buf.WriteUBitLong((uint)to.WeaponSubtype, WEAPON_SUBTYPE_BITS);
			}
			else {
				buf.WriteOneBit(0);
			}
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.MouseDeltaX != from.MouseDeltaX) {
			buf.WriteOneBit(1);
			buf.WriteShort(to.MouseDeltaX);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.MouseDeltaY != from.MouseDeltaY) {
			buf.WriteOneBit(1);
			buf.WriteShort(to.MouseDeltaY);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (HasChanged(to.ButtonsPressed, from.ButtonsPressed)) {
			buf.WriteOneBit(1);
			for (int i = 0; i < to.ButtonsPressed.Length; i++) {
				buf.WriteUBitLong(to.ButtonsPressed[i], 8);
			}
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.ScrollWheelSpeed != from.ScrollWheelSpeed) {
			buf.WriteOneBit(1);
			buf.WriteSBitLong(to.ScrollWheelSpeed, 8);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.WorldClicking != from.WorldClicking) {
			buf.WriteOneBit(1);
			buf.WriteOneBit(to.WorldClicking ? 1 : 0);
		}
		else {
			buf.WriteOneBit(0);
		}

		if (to.WorldClickDirection != from.WorldClickDirection) {
			buf.WriteOneBit(1);
			buf.WriteBitVec3Normal(to.WorldClickDirection);
		}
		else {
			buf.WriteOneBit(0);
		}

		buf.WriteOneBit(to.IsTyping ? 1 : 0);

		if (HasChanged(to.MotionSensorPositions, from.MotionSensorPositions)) {
			buf.WriteOneBit(1);
			for (int i = 0; i < to.ButtonsPressed.Length; i++) {
				buf.WriteBitVec3Coord(to.MotionSensorPositions[i]);
			}
		}
		else {
			buf.WriteOneBit(0);
		}

		buf.WriteOneBit(to.Forced ? 1 : 0);
	}
}
