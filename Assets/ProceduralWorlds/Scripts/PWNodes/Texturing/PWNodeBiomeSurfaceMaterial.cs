﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSurfaceMaterial : PWNode
	{

		[PWOutput]
		public BiomeSurfaceMaterial		surfaceMaterial = new BiomeSurfaceMaterial();

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "Surface material";
		}

		public override void OnNodeEnable()
		{
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)
		}

		public override void OnNodeGUI()
		{
			//your node GUI
		}

		public override void OnNodeProcess()
		{
			//write here the process which take inputs, transform them and set outputs.
		}
		
	}
}