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
    public sealed class RequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::GameServer.Protocol.Request>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::GameServer.Protocol.Request value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(3);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::GameServer.OperationCode>(formatterResolver).Serialize(ref writer, value.OperationCode, options);
            writer.Write(value.Timestamp);
            writer.Write(value.Data);
        }

        public global::GameServer.Protocol.Request Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ____result = new global::GameServer.Protocol.Request();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.OperationCode = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::GameServer.OperationCode>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 1:
                        ____result.Timestamp = reader.ReadInt64();
                        break;
                    case 2:
                        ____result.Data = global::MessagePack.Internal.CodeGenHelpers.GetArrayFromNullableSequence(reader.ReadBytes());
                        break;
                    default:
                        reader.Skip();
                        break;
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
