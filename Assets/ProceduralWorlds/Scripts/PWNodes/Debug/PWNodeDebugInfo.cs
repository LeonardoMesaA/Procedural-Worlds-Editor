﻿using UnityEditor;
using UnityEngine;
using System;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeDebugInfo : PWNode
	{
	
		[PWInput]
		public object		obj;

		//for pass-safe rendering
		[System.NonSerialized]
		bool				firstRender = false;

		public override void OnNodeCreation()
		{
			name = "Debug Info";
			renamable = true;
			obj = "null";
		}

		public override void OnNodeGUI()
		{
			if (obj != null)
			{
				if (!firstRender && e.type != EventType.Layout)
					return ;
				
				firstRender = true;

				Type	objType = obj.GetType();
				EditorGUILayout.LabelField(obj.ToString());
				if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(PWArray<>))
				{
					var pwv = obj as PWArray< object >;

					for (int i = 0; i < pwv.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("[" + i + "] " + pwv.NameAt(i) + ": " + pwv.At(i), GUILayout.Width(300));
						EditorGUILayout.EndHorizontal();
					}
				}
				else if (objType == typeof(Vector2))
					EditorGUILayout.Vector2Field("vec2", (Vector2)obj);
				else if (objType == typeof(Vector3))
					EditorGUILayout.Vector2Field("vec3", (Vector3)obj);
				else if (objType == typeof(Vector4))
					EditorGUILayout.Vector2Field("vec4", (Vector4)obj);
				else if (objType == typeof(Sampler2D))
					PWGUI.Sampler2DPreview(obj as Sampler2D);
				else if (objType == typeof(Sampler3D))
				{
				}
				else if (objType == typeof(BiomeData))
				{
					BiomeUtils.DrawBiomeInfos(rect, obj as BiomeData);
				}
			}
			else
				EditorGUILayout.LabelField("null");
		}

		//no process needed, no output.
	}
}
