using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace DialogDesigner
{
	[Serializable]
	public class dialog
	{
		public List <option> 		options;
		public List <variable>		variables;
	}
	
	[Serializable]
	public class variable
	{
		[XmlAttribute 	("name")]
		public string name;
		[XmlAttribute 	("type")]
		public string type;
		[XmlAttribute	("default")]
		public string defaultValue;
		[XmlAttribute	("description")]
		public string description;
	}
	
	[Serializable]
	public class option
	{
		[XmlAttribute 	("condition")]
		public string 	condition;
		[XmlAttribute 	("text")]
		public string 	text;
		[XmlAttribute 	("show")]
		public string 	show;
		[XmlAttribute 	("say")]
		public string 	say;
		public string 	script;
		//for conversion use
		public int		index;
		public List <option> options;
	}
}