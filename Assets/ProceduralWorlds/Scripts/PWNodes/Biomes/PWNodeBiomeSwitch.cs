﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSwitch : PWNode
	{
		[PWInput]
		public BiomeData			inputBiome;

		[PWOutput]
		[PWOffset(72, 18)] //hardcoded padding and margin for anchors
		public PWArray< BiomeData >	outputBiomes = new PWArray< BiomeData >();

		[SerializeField]
		public BiomeSwitchList		switchList = new BiomeSwitchList();

		public string				samplerName;
		string[]					samplerNames;

		[SerializeField]
		bool					alreadyModified = false;
		[SerializeField]
		int						selectedBiomeSamplerName;
		[SerializeField]
		bool					error;
		string					errorString;
		Sampler					currentSampler;
		[System.NonSerialized]
		bool					firstPass = true;

		float					relativeMin = float.MinValue;
		float					relativeMax = float.MaxValue;

		const string			delayedUpdateKey = "BiomeSwitchListUpdate";

		public override void OnNodeCreation()
		{
			name = "Biome switch";
		}

		public override void OnNodeEnable()
		{
			switchList.OnEnable();

			samplerNames = BiomeSamplerName.GetNames().ToArray();
			samplerName = samplerNames[selectedBiomeSamplerName];

			OnReload += ReloadCallback;

			delayedChanges.BindCallback(delayedUpdateKey, (unused) => { NotifyReload(); });

			switchList.OnBiomeDataAdded = (unused) => { delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataModified = (unused) => { alreadyModified = true; switchList.UpdateBiomeRepartitionPreview(inputBiome); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataRemoved = () => { delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataReordered = () => { delayedChanges.UpdateValue(delayedUpdateKey, null); };
			
			UpdateSwitchMode();
		}
		
		public override void OnNodeDisable()
		{
			OnReload -= ReloadCallback;
		}

		void UpdateSwitchMode()
		{
			UpdateRelativeBounds();

			if (switchList.Count > 2 || alreadyModified)
				return ;
			
			while (switchList.Count < 2)
				switchList.switchDatas.Add(new BiomeSwitchData(currentSampler, samplerName));

			var d1 = switchList.switchDatas[0];
			var d2 = switchList.switchDatas[1];
			
			d1.min = relativeMin;
			d1.max = relativeMax / 2;
			d2.min = relativeMax / 2;
			d2.max = relativeMax;
			d1.color = (SerializableColor)UnityEngine.Random.ColorHSV();
			d2.color = (SerializableColor)UnityEngine.Random.ColorHSV();

			switch (samplerName)
			{
				case BiomeSamplerName.terrainHeight:
					d1.name = "Low lands";
					d2.name = "High lands";
					break ;
				case BiomeSamplerName.waterHeight:
					if (relativeMin < 0 && relativeMax > 0)
					{
						d1.max = 0;
						d2.min = 0;
					}
					d1.name = "Terrestrial";
					d2.name = "Aquatic";
					break ;
				case BiomeSamplerName.temperature:
					d1.name = "Cold";
					d2.name = "Hot";
					break ;
				case BiomeSamplerName.wetness:
					d1.name = "Dry";
					d2.name = "Wet";
					break ;
			}

			SetMultiAnchor("outputBiomes", switchList.Count, null);
		}

		void UpdateRelativeBounds()
		{
			var inputNodes = GetInputNodes();

			if (inputNodes.Count() == 0)
				return ;
			
			PWNode prevNode = inputNodes.First();
			PWNode currentNode = this;

			while (prevNode.GetType() == typeof(PWNodeBiomeSwitch))
			{
				PWNodeBiomeSwitch prevSwitch = (PWNodeBiomeSwitch)prevNode;

				if (prevSwitch.samplerName != samplerName)
				{
					inputNodes = prevNode.GetInputNodes();

					if (inputNodes.Count() == 0)
						return ;
					
					currentNode = prevNode;
					prevNode = inputNodes.First();
					continue ;
				}

				var prevNodeOutputAnchors = prevSwitch.outputAnchors.ToList();
	
				for (int i = 0; i < prevNodeOutputAnchors.Count; i++)
				{
					var anchor = prevNodeOutputAnchors[i];
	
					if (anchor.links.Any(l => l.toNode == currentNode))
					{
						relativeMin = prevSwitch.switchList.switchDatas[i].min;
						relativeMax = prevSwitch.switchList.switchDatas[i].max;
					}
				}

				break ;
			}

			if (prevNode.GetType() != typeof(PWNodeBiomeSwitch))
			{
				//update currentSampler value:
				if (inputBiome != null)
					currentSampler = inputBiome.GetSampler(samplerName);
				
				if (currentSampler == null)
					return ;
				
				relativeMin = currentSampler.min;
				relativeMax = currentSampler.max;
				return ;
			}
		}

		void CheckForBiomeSwitchErrors()
		{
			error = false;

			var field = inputBiome.GetSampler(samplerName);
			var field3D = inputBiome.GetSampler(samplerName);

			if (field == null || field3D == null)
			{
				errorString = "can't switch on field " + samplerName + ",\ndata not provided !";
				error = true;
			}
			else
				currentSampler = ((field == null) ? field3D : field);

			//Update switchList values:
			switchList.UpdateSampler(samplerName, currentSampler);
			switchList.UpdateBiomeRepartitionPreview(inputBiome);
		}
		
		public override void OnNodeGUI()
		{
			//return if input biome is null
			if (inputBiome == null)
			{
				error = true;
				EditorGUILayout.LabelField("null biome input !");
				return ;
			}

			if (firstPass)
				CheckForBiomeSwitchErrors();

			//display popup field to choose the switch source
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 80;
				selectedBiomeSamplerName = EditorGUILayout.Popup("switch parameter", selectedBiomeSamplerName, samplerNames);
				samplerName = samplerNames[selectedBiomeSamplerName];
			}
			if (EditorGUI.EndChangeCheck())
			{
				CheckForBiomeSwitchErrors();
				UpdateSwitchMode();
			}

			EditorGUILayout.LabelField((currentSampler != null) ? "min: " + relativeMin + ", max: " + relativeMax : "");

			if (error)
			{
				Rect errorRect = EditorGUILayout.GetControlRect(false, GUI.skin.label.lineHeight * 3.5f);
				EditorGUI.LabelField(errorRect, errorString);
				return ;
			}

			switchList.OnGUI(inputBiome);

			firstPass = false;
		}

		void ReloadCallback(PWNode from)
		{
			//load the new sampler if there it was modified
			if (inputBiome != null)
				currentSampler = inputBiome.GetSampler(samplerName);

			UpdateRelativeBounds();

			switchList.UpdateMinMax(relativeMin, relativeMax);
			switchList.UpdateSampler(samplerName, currentSampler);
			switchList.UpdateBiomeRepartitionPreview(inputBiome);
		}

		void AdjustOutputBiomeArraySize()
		{
			int		outputArraySize = switchList.Count;

			//we adjust the size of the outputBiomes array to the size of the switchList
			while (outputBiomes.Count < outputArraySize)
				outputBiomes.Add(inputBiome, "outputBiome");
			while (outputBiomes.Count > outputArraySize)
				outputBiomes.RemoveAt(outputBiomes.Count - 1);
		}

		//no process needed else than assignation, this node only exists for user visual organization.
		public override void OnNodeProcess()
		{
			AdjustOutputBiomeArraySize();

			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}

		public override void OnNodeProcessOnce()
		{
			AdjustOutputBiomeArraySize();

			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}
	}
}
