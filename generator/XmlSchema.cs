using System;
using System.Xml.Serialization;

namespace OpenGL
{
	[XmlRoot("registry")]
	public class Registry
	{
		[XmlElement("comment")]
		public string Comment { get; set; }

		[XmlArray("types"), XmlArrayItem("type")]
		public Type[] Types { get; set; }

		[XmlArray("groups"), XmlArrayItem("group")]
		public Group[] Groups { get; set; }

		[XmlElement("enums")]
		public Enum[] Enums { get; set; }

		[XmlArray("commands"), XmlArrayItem("command")]
		public Command[] Commands { get; set; }

		[XmlElement("feature")]
		public Feature[] Features { get; set; }

		[XmlArray("extensions"), XmlArrayItem("extension")]
		public Extension[] Extensions { get; set; }
	}

	public class Type
	{
		[XmlAttribute("requires")]
		public string Requires { get; set; }

		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("api")]
		public string API { get; set; }

		[XmlElement("comment")]
		public string Comment { get; set; }

		public override string ToString() => Name;
	}

	public class Group
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlElement("enum")]
		public EnumRef[] Contents { get; set; }

		public override string ToString() => Name;
	}

	public class EnumRef
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		public override string ToString() => Name;
	}

	public class Enum
	{
		[XmlAttribute("group")]
		public string Group { get; set; }

		[XmlAttribute("type")]
		public string Type { get; set; }

		[XmlAttribute("namespace")]
		public string Namespace { get; set; }

		[XmlElement("enum")]
		public EnumMember[] Members { get; set; }

		public override string ToString() => $"{Namespace}:{Group} ({Type})";
	}

	public class EnumMember
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("value")]
		public string Value { get; set; }

		public override string ToString() => $"{Name} = {Value}";
	}

	public class Command
	{
		[XmlElement("proto")]
		public CommandProto Prototype { get; set; }

		[XmlElement("param")]
		public CommandParam[] Params { get; set; }

		public override string ToString()
		{
			return string.Format(
				"{0} {1}({2})",
				Prototype.Proto,
				Prototype.Name,
				string.Join<CommandParam>(", ", Params)
			);
		}
	}

	public class CommandProto
	{
		[XmlElement("name")]
		public string Name { get; set; }

		[XmlText]
		public string Proto { get; set; }
		
		[XmlElement("ptype")]
		public string Type { get; set; }
	}

	public class CommandParam
	{
		[XmlElement("ptype")]
		public string Type { get; set; }

		[XmlElement("name")]
		public string Name { get; set; }

		[XmlText]
		public string Text { get; set; }

		[XmlAttribute("group")]
		public string Group { get; set; }

		[XmlAttribute("len")]
		public string Length { get; set; }

		public override string ToString() => $"{Type} {Text} {Name}";
	}

	public class Feature
	{
		[XmlAttribute("api")]
		public string API { get; set; }

		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("number")]
		public string Number { get; set; }

		[XmlElement("require")]
		public FeatureList Require { get; set; }

		[XmlElement("remove")]
		public FeatureList Remove { get; set; }

		public override string ToString() => $"{Name}";
	}

	public class FeatureList
	{
		[XmlAttribute("profile")]
		public string Profile { get; set; }

		[XmlElement("type", Type = typeof(TypeFeatureItem))]
		[XmlElement("enum", Type = typeof(EnumFeatureItem))]
		[XmlElement("command", Type = typeof(CommandFeatureItem))]
		public FeatureItem[] Items { get; set; }
	}

	public abstract class FeatureItem
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		public override int GetHashCode()
		{
			return this.GetType().GetHashCode() ^ this.Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is FeatureItem item)
			{
				return (this.GetType() == obj.GetType())
					&& (this.Name == item.Name);
			}
			else
			{
				return false;
			}
		}

		public override string ToString() => $"{Name}";
	}

	public class EnumFeatureItem : FeatureItem { }
	public class TypeFeatureItem : FeatureItem { }
	public class CommandFeatureItem : FeatureItem { }

	public class Extension
	{

	}
}