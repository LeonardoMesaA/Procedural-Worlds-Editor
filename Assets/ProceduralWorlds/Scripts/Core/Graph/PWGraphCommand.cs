﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW.Core
{
	public enum PWGraphCommandType
	{
		NewNode,
		NewNodePosition,
		Link,
		LinkAnchor,
		LinkAnchorName,
	}
	
	public class PWGraphCommand
	{
	
		public PWGraphCommandType	type;
		public Vector2				position;
		public bool					forcePositon;
		public string				name;
		public Type					nodeType;
		public string				fromNodeName;
		public string				toNodeName;
		
		public string				attributes;

		public int					fromAnchorIndex;
		public int					toAnchorIndex;
		public string				fromAnchorFieldName;
		public string				toAnchorFieldName;

		//New node constructor
		public PWGraphCommand(Type nodeType, string name, string attributes = null)
		{
			this.type = PWGraphCommandType.NewNode;
			this.nodeType = nodeType;
			this.name = name;
			this.forcePositon = false;
			this.attributes = attributes;
		}

		//New node with position constructor
		public PWGraphCommand(Type nodeType, string name, Vector2 position, string attributes = null)
		{
			this.type = PWGraphCommandType.NewNodePosition;
			this.nodeType = nodeType;
			this.name = name;
			this.position = position;
			this.forcePositon = true;
			this.attributes = attributes;
		}

		//new link constructor
		public PWGraphCommand(string fromNodeName, string toNodeName)
		{
			this.type = PWGraphCommandType.Link;
			this.fromNodeName = fromNodeName;
			this.toNodeName = toNodeName;
		}

		//new link with anchor index constructor
		public PWGraphCommand(string fromNodeName, int fromAnchorIndex, string toNodeName, int toAnchorIndex)
		{
			this.type = PWGraphCommandType.LinkAnchor;
			this.fromNodeName = fromNodeName;
			this.fromAnchorIndex = fromAnchorIndex;
			this.toNodeName = toNodeName;
			this.toAnchorIndex = toAnchorIndex;
		}

		public PWGraphCommand(string fromNodeName, string fromAnchorFieldName, string toNodeName, string toAnchorFieldName)
		{
			this.type = PWGraphCommandType.LinkAnchorName;
			this.fromNodeName = fromNodeName;
			this.fromAnchorFieldName = fromAnchorFieldName;
			this.toNodeName = toNodeName;
			this.toAnchorFieldName = toAnchorFieldName;
		}

		//new link 

		public static bool operator ==(PWGraphCommand cmd1, PWGraphCommand cmd2)
		{
			return	cmd1.type == cmd2.type
					&& cmd1.forcePositon == cmd2.forcePositon
					&& cmd1.fromNodeName == cmd2.fromNodeName
					&& cmd1.toNodeName == cmd2.toNodeName
					&& cmd1.name == cmd2.name
					&& cmd1.position == cmd2.position;
		}

		public static bool operator !=(PWGraphCommand cmd1, PWGraphCommand cmd2)
		{
			return !(cmd1 == cmd2);
		}

		public override bool Equals(object cmd)
		{
			if (cmd as PWGraphCommand == null)
				return false;
			
			return ((cmd as PWGraphCommand) == this);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode()
				+ type.GetHashCode()
				+ forcePositon.GetHashCode()
				+ name.GetHashCode()
				+ nodeType.GetHashCode()
				+ fromNodeName.GetHashCode()
				+ toNodeName.GetHashCode();
		}
	
	}
}