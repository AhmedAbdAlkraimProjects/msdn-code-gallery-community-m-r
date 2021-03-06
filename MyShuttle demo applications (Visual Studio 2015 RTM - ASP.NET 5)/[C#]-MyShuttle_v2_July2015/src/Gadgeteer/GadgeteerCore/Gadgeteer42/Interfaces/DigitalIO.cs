////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Gadgeteer.Interfaces
{
    using System;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;
    using Gadgeteer.Modules;

    internal class NativeDigitalIO : Socket.SocketInterfaces.DigitalIO
    {
        private TristatePort _port;
        private Socket.SocketInterfaces.IOMode _mode;

        public NativeDigitalIO(Socket socket, Socket.Pin pin, bool initialState, GlitchFilterMode glitchFilterMode, ResistorMode resistorMode, Module module, Cpu.Pin cpuPin)
        {
            if (cpuPin == Cpu.Pin.GPIO_NONE)
            {
                // this is a mainboard error but should not happen since we check for this, but it doesnt hurt to double-check
                throw Socket.InvalidSocketException.FunctionalityException(socket, "DigitalIO");
            }

            _port = new TristatePort(cpuPin, initialState, glitchFilterMode == GlitchFilterMode.On, (Port.ResistorMode)resistorMode);
        }

        public override Socket.SocketInterfaces.IOMode Mode
        {
            get { return _mode; }
            set
            {
                switch (value)
                {
                    case Socket.SocketInterfaces.IOMode.Input:
                        if (_port.Active)
                            _port.Active = false;

                        break;

                    case Socket.SocketInterfaces.IOMode.Output:
                        if (!_port.Active)
                            _port.Active = true;

                        break;
                }

                _mode = value;
            }
        }

        public override void Write(bool state)
        {
            Mode = Socket.SocketInterfaces.IOMode.Output;

            _port.Write(state);
        }

        public override bool Read()
        {
            Mode = Socket.SocketInterfaces.IOMode.Input;

            return _port.Read();
        }

        public override void Dispose()
        {
            _port.Dispose();
        }
    }


    /// <summary>
    /// Represents digital input/output on a single pin.
    /// </summary>
    public class DigitalIO
    {
        internal readonly Socket.SocketInterfaces.DigitalIO Interface;

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socket">The socket for the digital input/output interface.</param>
        /// <param name="pin">The pin used by the digital input/output interface.</param>
        /// <param name="initialState">
        ///  The initial state to set on the digital input/output interface port.  
        ///  This value becomes effective as soon as the port is enabled as an output port.
        /// </param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this digital input/output interface.
        /// </param>
        /// <param name="resistorMode">
        ///  A value from the <see cref="ResistorMode"/> enumeration that establishes a default state for the digital input/output interface. N.B. .NET Gadgeteer mainboards are only required to support ResistorMode.PullUp on interruptable GPIOs and are never required to support ResistorMode.PullDown; consider putting the resistor on the module itself.
        /// </param>
        /// <param name="module">The module using this interface, which can be null if unspecified.</param>
        public DigitalIO(Socket socket, Socket.Pin pin, bool initialState, GlitchFilterMode glitchFilterMode, ResistorMode resistorMode, Module module)
        {
            Cpu.Pin reservedPin = socket.ReservePin(pin, module);

            // native implementation is preferred to an indirected one
            if (reservedPin == Cpu.Pin.GPIO_NONE && socket.DigitalIOIndirector != null)
                Interface = socket.DigitalIOIndirector(socket, pin, initialState, glitchFilterMode, resistorMode, module);

            else
                Interface = new NativeDigitalIO(socket, pin, initialState, glitchFilterMode, resistorMode, module, reservedPin);
        }

        /// <summary>
        /// Represents the possible modes of a digital input/output interface.
        /// </summary>
        public enum IOModes
        {
            /// <summary>
            /// The interface is set for input operations.
            /// </summary>
            Input,
            /// <summary>
            /// The interface is set for output operations.
            /// </summary>
            Output
        }

        /// <summary>
        /// Gets or sets the mode for the digital input/output interface.
        /// </summary>
        /// <value>
        /// <list type="bullet">
        /// <item><see cref="IOModes">IOModes.Input</see> if the interface is currently set for input operations.</item>
        /// <item><see cref="IOModes">IOModes.Output</see> if the interface is currently set for ouput operations.</item>
        /// </list>
        /// </value>
        public IOModes IOMode
        {
            get { return (IOModes)Interface.Mode; }
            set { Interface.Mode = (Socket.SocketInterfaces.IOMode)value; }
        }

        /// <summary>
        /// Sets the interface to <see cref="IOModes">IOModes.Output</see>
        /// and writes the specified value.
        /// </summary>
        /// <param name="state">The value to write.</param>
        public void Write(bool state)
        {
            Interface.Write(state);
        }

        /// <summary>
        /// Sets the interface to <see cref="IOModes">IOModes.Input</see>
        /// and reads a value.
        /// </summary>
        /// <returns>A Boolean value read from the interface.</returns>
        public bool Read()
        {
            return Interface.Read();
        }

        /// <summary>
        /// Returns the <see cref="Socket.SocketInterfaces.DigitalIO" /> for a <see cref="DigitalIO" /> interface.
        /// </summary>
        /// <param name="this">An instance of <see cref="DigitalIO" />.</param>
        /// <returns>The <see cref="Socket.SocketInterfaces.DigitalIO" /> for <paramref name="this"/>.</returns>
        public static explicit operator Socket.SocketInterfaces.DigitalIO(DigitalIO @this)
        {
            return @this.Interface;
        }

        /// <summary>
        /// Releases all resources used by the interface.
        /// </summary>
        public void Dispose()
        {
            Interface.Dispose();
        }
    }
}
