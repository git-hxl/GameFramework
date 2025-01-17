// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters.GameServer.Protocol
{
    public sealed class JoinRoomResponseFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::GameServer.Protocol.JoinRoomResponse>
    {
        // RoomID
        private static global::System.ReadOnlySpan<byte> GetSpan_RoomID() => new byte[1 + 6] { 166, 82, 111, 111, 109, 73, 68 };
        // PlayerID
        private static global::System.ReadOnlySpan<byte> GetSpan_PlayerID() => new byte[1 + 8] { 168, 80, 108, 97, 121, 101, 114, 73, 68 };
        // RoomInfo
        private static global::System.ReadOnlySpan<byte> GetSpan_RoomInfo() => new byte[1 + 8] { 168, 82, 111, 111, 109, 73, 110, 102, 111 };
        // PlayerInfos
        private static global::System.ReadOnlySpan<byte> GetSpan_PlayerInfos() => new byte[1 + 11] { 171, 80, 108, 97, 121, 101, 114, 73, 110, 102, 111, 115 };

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::GameServer.Protocol.JoinRoomResponse value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            var formatterResolver = options.Resolver;
            writer.WriteMapHeader(4);
            writer.WriteRaw(GetSpan_RoomID());
            writer.Write(value.RoomID);
            writer.WriteRaw(GetSpan_PlayerID());
            writer.Write(value.PlayerID);
            writer.WriteRaw(GetSpan_RoomInfo());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::GameServer.Protocol.RoomInfo>(formatterResolver).Serialize(ref writer, value.RoomInfo, options);
            writer.WriteRaw(GetSpan_PlayerInfos());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<global::GameServer.Protocol.PlayerInfo>>(formatterResolver).Serialize(ref writer, value.PlayerInfos, options);
        }

        public global::GameServer.Protocol.JoinRoomResponse Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            var formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var ____result = new global::GameServer.Protocol.JoinRoomResponse();

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.Internal.CodeGenHelpers.ReadStringSpan(ref reader);
                switch (stringKey.Length)
                {
                    default:
                    FAIL:
                      reader.Skip();
                      continue;
                    case 6:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 75082159320914UL) { goto FAIL; }

                        ____result.RoomID = reader.ReadInt32();
                        continue;
                    case 8:
                        switch (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey))
                        {
                            default: goto FAIL;
                            case 4920589848032668752UL:
                                ____result.PlayerID = reader.ReadInt32();
                                continue;
                            case 8027224647482175314UL:
                                ____result.RoomInfo = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::GameServer.Protocol.RoomInfo>(formatterResolver).Deserialize(ref reader, options);
                                continue;
                        }
                    case 11:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_PlayerInfos().Slice(1))) { goto FAIL; }

                        ____result.PlayerInfos = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<global::GameServer.Protocol.PlayerInfo>>(formatterResolver).Deserialize(ref reader, options);
                        continue;

                }
            }

            reader.Depth--;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name
