using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System.Drawing;
using System;
using Serilog;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class Color : INetSerializable
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        private static readonly ILogger _logger = Log.ForContext<Color>();

        public Color(float r, float g, float b, float a) 
        {
            _logger.Debug($"Creating color with R: {r} G: {g} B: {b} A: {a}");
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public Color() { }

        public static implicit operator Color(System.Drawing.Color c)
        {
            Color temp  = new Color(Clamp(c.R / 255f), Clamp(c.G / 255f), Clamp(c.B / 255f), Clamp(c.A / 255f));
            _logger.Debug($"Converted Drawing.Color");
            _logger.Debug($"Before convert {c.R} {c.G} {c.B} {c.A}");
            _logger.Debug($"After conversion {Clamp(c.R / 255f)} {Clamp(c.G / 255f)} {Clamp(c.B / 255f)} {Clamp(c.A / 255f)}");
            return temp;
        }

        public static implicit operator System.Drawing.Color(Color c)
        {
            _logger.Debug($"Converted A: {c.A} R: {c.R} G: {c.G} B: {c.B} to A: {Round(c.A * 255f)} R: {Round(c.R * 255f)} G: {Round(c.G * 255f)} B: {Round(c.B * 255f)}");
            _logger.Debug($"Conversion without rounding A: {c.A * 255f} R: {c.R * 255f} G: {c.G * 255f} B: {c.B * 255f}");
            return System.Drawing.Color.FromArgb(Round(c.A * 255f), Round(c.R * 255f), Round(c.G * 255f), Round(c.B * 255f));
        }

        public static int Round(float f)
        {
            return (int)Math.Round((double)f);
        }

        public static float Clamp(float value)
        {
            bool flag = value < 0f;
            float num;
            if (flag)
            {
                num = 0f;
            }
            else
            {
                bool flag2 = value > 1f;
                if (flag2)
                {
                    num = 1f;
                }
                else
                {
                    num = value;
                }
            }
            return num;
        }
        public void ReadFrom(ref SpanBuffer reader)
        {
            //Maybe cast to int?
            R = Clamp(reader.ReadUInt8() / 255f);
            G = Clamp(reader.ReadUInt8() / 255f);
            B = Clamp(reader.ReadUInt8() / 255f);
            A = Clamp(reader.ReadUInt8() / 255f);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt8((byte)Round(R * 255f));
            writer.WriteUInt8((byte)Round(G * 255f));
            writer.WriteUInt8((byte)Round(B * 255f));
            writer.WriteUInt8((byte)Round(A * 255f));
        }
    }
}
