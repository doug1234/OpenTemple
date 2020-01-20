using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenTemple.Core;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Protos;
using OpenTemple.Core.TigSubsystems;

namespace ConvertMapToText
{
    internal class ProtosConverter
    {
        internal static void Convert(string toeeDir)
        {
            using var game = HeadlessGame.Start(toeeDir);

            Directory.CreateDirectory("protos");

            foreach (var protosFile in ProtoFileParser.EnumerateProtoFiles(Tig.FS))
            {
                var protos = ProtoFileParser.Parse(protosFile);

                foreach (var proto in protos)
                {
                    var dir = Path.Join("protos", proto.type.ToString());
                    Directory.CreateDirectory(dir);

                    var file = Path.Join(dir, proto.id.protoId + ".json");
                    var displayName = GameSystems.MapObject.GetDisplayName(proto);

                    using var stream = new FileStream(file, FileMode.Create);
                    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                    {
                        Indented = true
                    });

                    writer.WriteStartObject();
                    writer.WriteString("$comment", displayName);
                    writer.WriteString("type", proto.type.ToString());
                    writer.WriteNumber("id", proto.id.protoId);

                    proto.ForEachField((field, value) =>
                    {
                        if (value == null)
                        {
                            // ForEachField will return ALL fields for a proto, unset fields will be null
                            return true;
                        }
                        writer.WriteField(field, value);
                        return true;
                    });

                    writer.WriteEndObject();
                }
            }
        }
    }
}