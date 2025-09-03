using CommunityToolkit.HighPerformance;

using Source.Common.Commands;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Launcher;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Source.GUI;

public struct MessageItem
{
	public KeyValues Params;
	public IPanel? To;
	public IPanel? From;
	public double ArrivalTime;
	public ulong MessageID;
}

public struct Tick
{
	public IPanel? Panel;
	public long Interval;
	public long NextTick;
	public bool MarkDeleted;
}

public class MessageItemComparer : IComparer<double>
{
	public int Compare(double x, double y) => x.CompareTo(y);
}

public class VGui : IVGui
{
	readonly List<Tick> TickSignals = [];
	readonly Queue<MessageItem> MessageQueue = [];
	readonly Queue<MessageItem> SecondaryQueue = [];
	readonly PriorityQueue<MessageItem, double> DelayedMessageQueue = new(new MessageItemComparer());

	public readonly VGuiInput Input;
	public readonly ISurface surface;
	public readonly ISystem system;

	public VGui(ICommandLine commandLine, ISurface surface, ISystem system) {
		Input = new(commandLine, this, surface);
		this.surface = surface;
		this.system = system;
	}

	public void Quit() {

	}

	bool InDispatcher;
	bool CanRemoveTickSignal;
	int ReentrancyCount;
	bool IsReentrant() => ReentrancyCount > 0;

	public void RunFrame() {
		bool isReentrant = InDispatcher;
		if (isReentrant)
			ReentrancyCount++;
		// Generate all key and mouse events
		surface.RunFrame();
		// Let the system process what it needs
		system.RunFrame();

		// Update the mouse cursor
		if (!IsReentrant()) {
			Input.GetCursorPosition(out int x, out int y);
			Input.UpdateMouseFocus(x, y);
		}

		// Update the input state
		if (!isReentrant)
			Input.RunFrame();

		// Message dispatch
		if (!isReentrant) {
			DispatchMessages();

			long time = system.GetTimeMillis();
			CanRemoveTickSignal = false;
			{
				Span<Tick> ticks = TickSignals.AsSpan();
				for (int count = ticks.Length, i = count - 1; i >= 0; i--) {
					ref Tick tick = ref ticks[i];
					if (tick.MarkDeleted)
						continue;

					if (tick.Interval != 0) {
						if (time < tick.NextTick)
							continue;
						tick.NextTick = time + tick.Interval;
					}

					tick.Panel?.OnTick();
				}
			}
			CanRemoveTickSignal = true;

			for (int count = TickSignals.Count, i = count - 1; i >= 0; i--) {
				Tick tick = TickSignals[i];
				if (tick.MarkDeleted)
					TickSignals.RemoveAt(i);
			}
		}

		surface.SolveTraverse(surface.GetEmbeddedPanel());
		surface.ApplyChanges();

		if (isReentrant)
			ReentrancyCount--;
	}

	public bool DispatchMessages() {
		long time = system.GetTimeMillis();
		InDispatcher = true;
		bool doneWork = MessageQueue.Count > 12;
		bool usingDelayedQueue = DelayedMessageQueue.Count > 0;

		int passCount = 0;
		while(passCount < 2) {
			while(MessageQueue.Count > 0 || SecondaryQueue.Count > 0 || usingDelayedQueue) {
				MessageItem messageItem;

				bool usingSecondaryQueue = SecondaryQueue.Count > 0;
				if (usingSecondaryQueue) {
					doneWork = true;
					SecondaryQueue.TryDequeue(out messageItem);
				}
				else if (usingDelayedQueue) {
					if (!DelayedMessageQueue.TryDequeue(out messageItem, out _)) 
						usingDelayedQueue = false;
				}
				else {
					MessageQueue.TryDequeue(out messageItem);
				}

				KeyValues parms = messageItem.Params;
				// Cursor pos messages? What the hell
				messageItem.To?.SendMessage(parms, messageItem.From);
			}
			passCount += 1;
			if (passCount == 1)
				Input.PostCursorMessage();
		}
		Input.HandleExplicitSetCursor();
		InDispatcher = false;
		return doneWork;
	}

	public void PostMessage(IPanel? target, KeyValues parms, IPanel? from, double delay = 0) {
		if (IsReentrant()) {
			Assert(false);
			return;
		}

		if (target == null)
			return;

		MessageItem messageItem = new();
		messageItem.To = target;

		messageItem.Params = parms;
		messageItem.From = from;
		messageItem.ArrivalTime = 0;
		messageItem.MessageID = CurrentMessageID++;

		if(delay > 0) {
			messageItem.ArrivalTime = system.GetTimeMillis() + (delay * 1000);
			DelayedMessageQueue.Enqueue(messageItem, messageItem.ArrivalTime);
		}
		else if(InDispatcher)
			SecondaryQueue.Enqueue(messageItem);
		else
			MessageQueue.Enqueue(messageItem);
	}

	public void ClearMessageQueues() {
		Assert(!InDispatcher);

		MessageQueue.Clear();
		SecondaryQueue.Clear();
		DelayedMessageQueue.Clear();
	}

	ulong CurrentMessageID;

	public void MarkPanelForDeletion(IPanel? panel) {
		PostMessage(panel, new("Delete"), null);
	}

	public void Stop() {

	}

	public IVGuiInput GetInput() => Input;
}