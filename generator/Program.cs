using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using OpenGL;

namespace generator
{
	class MainClass
	{
		private static readonly Dictionary<string, string> typePatchTable = new Dictionary<string, string>()
		{
			["const void *"] = "?*const c_void",
			["void *"] = "?*c_void",
			["void **"] = "?**c_void",
		};

		private static string PatchType(string ptype, string rest)
		{
			ptype = ptype?.Trim();
			rest = rest?.Trim();
			if (!string.IsNullOrWhiteSpace(rest))
			{
				if (ptype != null)
					rest = rest + " " + ptype;
					
				if (typePatchTable.TryGetValue(rest, out var result))
					return result;
				else
					return rest.Replace("*", "[*c]");
			}
			else if (!string.IsNullOrWhiteSpace(ptype))
			{
				return ptype;
			}
			else
			{
				throw new NotSupportedException();
			}
		}
	
		public static void Main(string[] args)
		{
			var ser = new XmlSerializer(typeof(OpenGL.Registry));
			OpenGL.Registry registry;
			using (var fs = File.Open(args[0], FileMode.Open, FileAccess.Read))
			{
				registry = (OpenGL.Registry)ser.Deserialize(fs);
			}

			HashSet<OpenGL.FeatureItem> features = new HashSet<FeatureItem>();

			foreach (var feature in registry.Features)
			{
				if (feature.Require?.Items != null)
					features.UnionWith(feature.Require.Items);
				if (feature.Remove?.Items != null)
					features.ExceptWith(feature.Remove.Items);

				using (var sw = new StreamWriter(args[1] + "/" + feature.Name + ".zig", false, new UTF8Encoding(false, true)))
				{
					sw.WriteLine(@"// generated code
pub const GLenum = c_uint;
pub const GLboolean = c_uint;
pub const GLuint = c_uint;
pub const GLint = c_int;
pub const GLsizei = isize;
pub const GLfloat = f32;
pub const GLdouble = f64;
pub const GLbitfield = c_uint;
pub const GLubyte = u8;
pub const GLbyte = i8;
pub const GLushort = u16;
pub const GLshort = i16;

pub const VERSION: GLenum = 0x1F02;
");
				
					foreach (var feat in features)
					{
						if (feat is CommandFeatureItem cmdFeature)
						{
							var cmd = registry.Commands.Single(c => (c.Prototype.Name == cmdFeature.Name));

							var name = cmd.Prototype.Name;
							if (!name.StartsWith("gl", StringComparison.Ordinal))
								throw new InvalidDataException("Function name does not start with gl!");
							name = name.Substring(2, 1).ToLower() + name.Substring(3);

							sw.Write("{0}: extern fn (", name);
							if (cmd.Params != null)
							{
								for (int i = 0; i < cmd.Params.Length; i++)
								{
									var p = cmd.Params[i];
									if (i > 0)
										sw.Write(", ");
									sw.Write(
										"{0}: {1}",
										p.Name,
										PatchType(p.Type, p.Text)
									);
								}
							}
							sw.WriteLine(") {0},", PatchType(cmd.Prototype.Type, cmd.Prototype.Proto));

						}
						else if (feat is EnumFeatureItem enumFeature)
						{
							// var cmd = registry.Enums.Single(c => (c.Name == cmdFeature.Name));

						}
						else if (feat is TypeFeatureItem typeFeature)
						{

						}
						else
						{
							throw new NotSupportedException();
						}
					}
				}
			}

			Console.WriteLine("done");
		}
	}
}
