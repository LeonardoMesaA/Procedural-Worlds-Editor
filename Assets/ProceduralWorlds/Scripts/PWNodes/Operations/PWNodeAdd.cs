﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using NUnit.Framework;

namespace PW.Node
{
	public class PWNodeAdd : PWNode
	{
	
		[PWInput]
		public PWArray< float >	values = new PWArray< float >();
	
		[PWOutput]
		public float	fOutput;
	
		bool			intify = false;
	
		public override void OnNodeCreation()
		{
			//override window width
			rect.width = 150;
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 100;

			EditorGUILayout.LabelField("result: " + fOutput);
		}

		public override void OnNodeProcess()
		{
			fOutput = 0;
			foreach (var val in values)
				fOutput += val;
			
			if (intify)
				fOutput = Mathf.RoundToInt(fOutput);
		}

		public override void OnNodeUnitTest()
		{
			values = new PWArray< float >() {10, 30, 40, -20};

			OnNodeProcess();

			Assert.That(fOutput == 60);
		}
	}
}
