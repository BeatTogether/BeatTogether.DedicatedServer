using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System.Drawing;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class Color : INetSerializable
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        public Color(float r, float g, float b, float a) 
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public Color() { }

        public static implicit operator Color(System.Drawing.Color c)
        {
            return new Color(Round(Clamp(c.R) * 255f), Round(Clamp(c.G) * 255f), Round(Clamp(c.B) * 255f), Round(Clamp(c.A) * 255f));
        }

        public static implicit operator System.Drawing.Color(Color c)
        {
            return System.Drawing.Color.FromArgb((int)(c.a / 255f), (int)(c.r / 255f), (int)(c.g / 255f), (int)(c.b / 255f));
        }

        public static float Round(float f)
        {
            return (float)Math.Round((double)f);
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
            r = reader.ReadFloat32();
            g = reader.ReadFloat32();
            b = reader.ReadFloat32();
            a = reader.ReadFloat32();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteFloat32(r);
            writer.WriteFloat32(g);
            writer.WriteFloat32(b);
            writer.WriteFloat32(a);
        }
    }
}
