using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Neo.VM
{
    /// <summary>
    /// 这个类是从 Helper 重命名 为 Helper_。避免与 Neo.dll中的同名类冲突。不推荐通过类名来访问这些方法。
    /// </summary>
    public static class Helper_
    {
        private static Dictionary<string, uint> method_hashes = new Dictionary<string, uint>();

        internal static byte[] ReadVarBytes(this BinaryReader reader, int max = 0X7fffffc7)
        {
            return reader.ReadBytes((int)reader.ReadVarInt((ulong)max));
        }

        internal static ulong ReadVarInt(this BinaryReader reader, ulong max = ulong.MaxValue)
        {
            byte fb = reader.ReadByte();
            ulong value;
            if (fb == 0xFD)
                value = reader.ReadUInt16();
            else if (fb == 0xFE)
                value = reader.ReadUInt32();
            else if (fb == 0xFF)
                value = reader.ReadUInt64();
            else
                value = fb;
            if (value > max) throw new FormatException();
            return value;
        }

        internal static string ReadVarString(this BinaryReader reader)
        {
            return Encoding.UTF8.GetString(reader.ReadVarBytes());
        }

        public static uint ToInteropMethodHash(this string method)
        {
            if (method_hashes.TryGetValue(method, out uint hash))
                return hash;
            using (SHA256 sha = SHA256.Create())
            {
                hash = BitConverter.ToUInt32(sha.ComputeHash(Encoding.ASCII.GetBytes(method)), 0);
            }
            method_hashes[method] = hash;
            return hash;
        }

        internal static void WriteVarBytes(this BinaryWriter writer, byte[] value)
        {
            writer.WriteVarInt(value.Length);
            writer.Write(value);
        }

        internal static void WriteVarInt(this BinaryWriter writer, long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException();
            if (value < 0xFD)
            {
                writer.Write((byte)value);
            }
            else if (value <= 0xFFFF)
            {
                writer.Write((byte)0xFD);
                writer.Write((ushort)value);
            }
            else if (value <= 0xFFFFFFFF)
            {
                writer.Write((byte)0xFE);
                writer.Write((uint)value);
            }
            else
            {
                writer.Write((byte)0xFF);
                writer.Write(value);
            }
        }

        internal static void WriteVarString(this BinaryWriter writer, string value)
        {
            writer.WriteVarBytes(Encoding.UTF8.GetBytes(value));
        }
    }
}
