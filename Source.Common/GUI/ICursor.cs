namespace Source.Common.GUI;

public interface ICursor;

// Kind of a hack for a cursor code, but whatever
public record struct CursorCode(byte value) : ICursor {
	public byte Value => value;
	public static readonly CursorCode User = new(0);
	public static readonly CursorCode None = new(1);
	public static readonly CursorCode Arrow = new(2);
	public static readonly CursorCode IBeam = new(3);
	public static readonly CursorCode Hourglass = new(4);
	public static readonly CursorCode WaitArrow= new(5);
	public static readonly CursorCode Crosshair= new(6);
	public static readonly CursorCode Up= new(7);
	public static readonly CursorCode SizeNWSE= new(8);
	public static readonly CursorCode SizeNESW= new(9);
	public static readonly CursorCode SizeWE= new(10);
	public static readonly CursorCode SizeNS= new(11);
	public static readonly CursorCode SizeAll= new(12);
	public static readonly CursorCode No= new(13);
	public static readonly CursorCode Hand = new(14);
	public static readonly CursorCode Blank= new(15);
	public static readonly CursorCode Last= new(16);
	public static readonly CursorCode AlwaysVisiblePush = new(17);
	public static readonly CursorCode AlwaysVisiblePop= new(18);
}
