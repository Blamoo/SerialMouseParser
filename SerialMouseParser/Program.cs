using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace SerialMouseParser
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort p = new SerialPort("COM1", 1200, Parity.None, 7, StopBits.One);
            p.Handshake = Handshake.None;

            p.Open();
            p.RtsEnable = true;
            bool first = true;
            using (BinaryReader b = new BinaryReader(p.BaseStream))
            {
                var sig = b.ReadBytes(13);
                Console.WriteLine("Signature: {0} ({1} bytes)", ByteArrayToHex(sig), sig.Length);

                byte[] packet = new byte[4];
                int offset = 0;
                while (p.IsOpen)
                {
                    var c = b.ReadByte();

                    var d = (byte)((c >> 6) & 1);

                    if (first)
                    {
                        packet[0] = c;
                        first = false;
                        continue;
                    }

                    if (d == 1)
                    {
                        GoPacketGo(packet);
                        offset = 0;
                        Array.Clear(packet, 0, packet.Length);
                    }

                    packet[offset] = c;
                    offset++;
                }
            }
            p.Close();
        }

        private static void GoPacketGo(byte[] packet)
        {
            BitArray b = new BitArray(packet);
            b[14] = b[0];
            b[15] = b[1];
            b[22] = b[2];
            b[23] = b[3];

            b[0] = b[1] = b[2] = b[3] = false;

            b.CopyTo(packet, 0);

            MousePacket p = new MousePacket();

            p.DeltaX = 0x80 - (packet[1] ^ 0x80);
            p.DeltaY = 0x80 - (packet[2] ^ 0x80);

            //foreach (bool item in b)
            //{
            //    Console.Write(item ? '1' : '0');
            //}
            //Console.WriteLine();

            if (b[4])
                p.PressedButtons |= ButtonState.RightButton;
            if (b[5])
                p.PressedButtons |= ButtonState.LeftButton;
            if (b[29])
                p.PressedButtons |= ButtonState.MiddleButton;

            Console.WriteLine(p);
        }

        private static string ByteArrayToHex(byte[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in array)
            {
                sb.AppendFormat("{0} ", Convert.ToString(item, 16));
            }
            return sb.ToString();
        }
    }

    public struct MousePacket
    {
        public int DeltaX;
        public int DeltaY;
        public ButtonState PressedButtons;

        public override string ToString()
        {
            return string.Format("{{DeltaX: {0}, DeltaY: {1}, PressedButtons: {2}}}", DeltaX, DeltaY, PressedButtons);
        }
    }

    [Flags]
    public enum ButtonState
    {
        None = 0x00,
        LeftButton = 0x01,
        RightButton = 0x02,
        MiddleButton = 0x04,
    }
}
