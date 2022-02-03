using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Platform
{

    public enum MouseButton : uint {
        LEFT = 0,
        RIGHT = 1,
        MIDDLE = 2,
        EXTRA1 = 3,
        EXTRA2 = 4
    };

    public enum MessageType : uint {
        MOUSE = 0,
        WIDGET = 1,
        TMT_UNK2 = 2,
        EXIT = 3, // may be exit game, queued on WM_CLOSE and WM_QUIT
        CHAR = 4, // arg1 is the character, in Virtual Key terms
        KEYSTATECHANGE = 5,
        /*
            Send once whenever system message are being processed.
            No arguments, just the create time is set to the time
            the messages were processed.
        */
        UPDATE_TIME = 6,
        TMT_UNK7 = 7,
        KEYDOWN = 8
    }

    [Flags]
    public enum MouseEventFlag
    {
        LeftClick = 0x001,
        RightClick = 0x010,
        MiddleClick = 0x100,
        // Sent every 250ms while a button is being held
        LeftHeld = 0x002,
        RightHeld = 0x020,
        MiddleHeld = 0x200,
        LeftReleased = 0x004, // Left button
        RightReleased = 0x040, // Right button
        MiddleReleased = 0x400, // Middle button
        PosChange = 0x1000,
        // Sent only 35ms after mouse position has stabilized
        PosChangeSlow = 0x2000,
        ScrollWheelChange = 0x4000
    }

    public struct ExitMessageArgs
    {
        public readonly int Code;

        public ExitMessageArgs(int code)
        {
            Code = code;
        }
    }

    public struct MessageMouseArgs
    {
        public int X;
        public int Y;
        public int wheelDelta;
        public MouseEventFlag flags;

        public MessageMouseArgs(int x, int y, int wheelDelta, MouseEventFlag flags)
        {
            X = x;
            Y = y;
            this.wheelDelta = wheelDelta;
            this.flags = flags;
        }
    }

    public enum TigMsgWidgetEvent
    {
        Clicked = 0,
        MouseReleased = 1,
        MouseReleasedAtDifferentButton = 2,
        Entered  = 3,
        Exited = 4,
        Scrolled = 5
    }

    public struct MessageWidgetArgs
    {
        public WidgetBase widgetId;
        public TigMsgWidgetEvent widgetEventType; // 3 - widget entered; 4 - widget left
        public int x;
        public int y;
    }

    public struct MessageKeyStateChangeArgs
    {
        public DIK key;
        public bool down;
    }

    public readonly struct MessageCharArgs
    {
        public readonly char Character;

        public MessageCharArgs(char character)
        {
            Character = character;
        }
    }

    public class Message {
        public readonly TimePoint created;
        public readonly MessageType type;
        private object args;

        public Message(MessageType type)
        {
            this.type = type;
            created = TimePoint.Now;
        }

        public Message(ExitMessageArgs messageArgs) : this(MessageType.EXIT)
        {
            args = messageArgs;
        }

        public Message(MessageCharArgs messageArgs) : this(MessageType.CHAR)
        {
            CharArgs = messageArgs;
        }

        public Message(MessageMouseArgs mouseArgs) : this(MessageType.MOUSE)
        {
            MouseArgs = mouseArgs;
        }

        public Message(MessageKeyStateChangeArgs keyArgs) : this(MessageType.KEYSTATECHANGE)
        {
            KeyStateChangeArgs = keyArgs;
        }

        public Message(MessageWidgetArgs widgetArgs) : this(MessageType.WIDGET)
        {
            WidgetArgs = widgetArgs;
        }

        public MessageMouseArgs MouseArgs
        {
            get
            {
                Trace.Assert(type == MessageType.MOUSE);
                return (MessageMouseArgs) args;
            }
            private init
            {
                Trace.Assert(type == MessageType.MOUSE);
                args = value;
            }
        }

        public MessageWidgetArgs WidgetArgs
        {
            get
            {
                Trace.Assert(type == MessageType.WIDGET);
                return (MessageWidgetArgs) args;
            }
            private init
            {
                Trace.Assert(type == MessageType.WIDGET);
                args = value;
            }
        }

        public MessageKeyStateChangeArgs KeyStateChangeArgs
        {
            get
            {
                Trace.Assert(type == MessageType.KEYSTATECHANGE);
                return (MessageKeyStateChangeArgs) args;
            }
            private init
            {
                Trace.Assert(type == MessageType.KEYSTATECHANGE);
                args = value;
            }
        }

        public MessageCharArgs CharArgs
        {
            get
            {
                Trace.Assert(type == MessageType.CHAR);
                return (MessageCharArgs) args;
            }
            private init
            {
                Trace.Assert(type == MessageType.CHAR);
                args = value;
            }
        }
    }

    public class MessageQueue
    {
        public void Enqueue(Message msg)
        {
            _messages.Enqueue(msg);
        }

        public bool TryGetMessage(out Message unhandledMsgOut)
        {
            if (_messages.TryDequeue(out var message)) {
                unhandledMsgOut = message;
                return true;
            }

            unhandledMsgOut = null;
            return false;
        }

        private readonly Queue<Message> _messages = new(100);
    }
}
