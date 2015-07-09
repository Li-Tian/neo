﻿using AntShares.IO;
using System;
using System.IO;

namespace AntShares.Core
{
    public class TransactionOutput : ISerializable
    {
        public UInt256 AssetType;
        public Int64 Value;
        public UInt160 ScriptHash;

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.AssetType = reader.ReadSerializable<UInt256>();
            this.Value = reader.ReadInt64();
            this.ScriptHash = reader.ReadSerializable<UInt160>();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(AssetType);
            writer.Write(Value);
            writer.Write(ScriptHash);
        }
    }
}