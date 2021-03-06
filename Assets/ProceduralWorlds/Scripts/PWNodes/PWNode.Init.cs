﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;
using PW.Core;

using PW.Node;

//Initialization and data baking for PWNode
namespace PW
{
	public partial class PWNode
	{
		static GUIStyle 			renameNodeTextFieldStyle = null;
		static GUIStyle				innerNodePaddingStyle = null;
		static GUIStyle				nodeStyle = null;
		static bool					styleLoadedStatic = false;
		[System.NonSerialized]
		bool						styleLoaded = false;
		
		static Texture2D			editIcon = null;
		static Texture2D			debugIcon = null;

		//anchor 
		[System.Serializable]
		protected class AnchorFieldDictionary : SerializableDictionary< string, PWAnchorField > {}
		[SerializeField]
		protected AnchorFieldDictionary		anchorFieldDictionary = new AnchorFieldDictionary();

		[System.NonSerialized]
		Dictionary< string, FieldInfo >		anchorFieldInfoMap = new Dictionary< string, FieldInfo >();

		[System.NonSerialized]
		List< FieldInfo >					undoableFields = new List< FieldInfo >();

		void LoadAssets()
		{
			editIcon = Resources.Load< Texture2D >("Icons/ic_edit");
			debugIcon = Resources.Load< Texture2D >("Icons/ic_settings");
			
			//set the color scheme name for this node type
			colorSchemeName = PWNodeTypeProvider.GetNodeColor(GetType());
		}

		public void LoadStyles()
		{
			//check if style was already initialized:
			if (innerNodePaddingStyle != null)
				return ;

			renameNodeTextFieldStyle = GUI.skin.FindStyle("RenameNodetextField");
			innerNodePaddingStyle = GUI.skin.FindStyle("WindowInnerPadding");
			nodeStyle = GUI.skin.FindStyle("Node");

			styleLoadedStatic = true;
		}
		
		void LoadFieldAttributes()
		{
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			List< string > actualFields = new List< string >();
				
			foreach (var field in fInfos)
			{
				// reading field informations.

				actualFields.Add(field.Name);
				anchorFieldInfoMap[field.Name] = field;

				if (!anchorFieldDictionary.ContainsKey(field.Name))
					anchorFieldDictionary[field.Name] = CreateAnchorField();
				
				PWAnchorField	anchorField = anchorFieldDictionary[field.Name];
				
				//detect multi-anchor by checking for PWArray<T> type
				if (field.FieldType.IsGenericType)
				{
					if (field.FieldType.GetGenericTypeDefinition() == typeof(PWArray<>))
						anchorField.multiple = true;
				}
				
				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					PWInputAttribute		inputAttr = attr as PWInputAttribute;
					PWOutputAttribute		outputAttr = attr as PWOutputAttribute;
					PWColorAttribute		colorAttr = attr as PWColorAttribute;
					PWOffsetAttribute		offsetAttr = attr as PWOffsetAttribute;
					PWNotRequiredAttribute	notRequiredAttr = attr as PWNotRequiredAttribute;

					if (inputAttr != null)
					{
						anchorField.anchorType = PWAnchorType.Input;
						if (inputAttr.name != null)
							anchorField.name = inputAttr.name;
					}
					if (outputAttr != null)
					{
						anchorField.anchorType = PWAnchorType.Output;
						if (outputAttr.name != null)
							anchorField.name = outputAttr.name;
					}
					if (colorAttr != null)
						anchorField.color = new SerializableColor(colorAttr.color);
					if (offsetAttr != null)
					{
						anchorField.offset = offsetAttr.offset;
						anchorField.padding = offsetAttr.padding;
					}
					if (notRequiredAttr != null)
						anchorField.required = false;
				}
				if (anchorField.anchorType == PWAnchorType.None) //field does not have a PW attribute
					anchorFieldDictionary.Remove(field.Name);
				else
				{
					//create anchor in this anchorField if there is not existing one
					if (anchorField.anchors.Count == 0)
						anchorField.CreateNewAnchor();

					anchorField.colorSchemeName = PWColorTheme.GetAnchorColorSchemeName(field.FieldType);
					anchorField.fieldName = field.Name;
					anchorField.fieldType = (SerializableType)field.FieldType;
					anchorField.fieldValue = field.GetValue(this);
					anchorField.nodeRef = this;
				}
			}

			//remove inhexistants field dictionary entries (for renamed variables):
			var toRemoveKeys = new List< string >();
			foreach (var kp in anchorFieldDictionary)
				if (!actualFields.Contains(kp.Key))
					toRemoveKeys.Add(kp.Key);
						
			//remove duplicate keys in the dictionary (yes it happends ...)
			HashSet< string > duplicateKeys = new HashSet< string >();
			foreach (var kp in anchorFieldDictionary)
			{
				// if (duplicateKeys.Contains(kp.Key))
					// toRemoveKeys.Add(kp.Key);
				duplicateKeys.Add(kp.Key);
			}
			
			foreach (var toRemoveKey in toRemoveKeys)
				anchorFieldDictionary.Remove(toRemoveKey);
		}

		void UpdateAnchorProperties()
		{
			//we check if an anchor have been update (field name changed)
			// and then remove it from the anchor list and take the new field attributes
			// we do the same for every properties
			foreach (var anchorFieldKP in anchorFieldDictionary)
			{
				var af = anchorFieldKP.Value;

				foreach (var anchor in anchorFields)
				{
					//we find the field in inputAnchors corresponding to af
					if (anchor.anchorType == af.anchorType && anchor.fieldName == af.fieldName)
					{
						//we update the values we have in our input/output anchorFileds with the new fresh field values
						anchor.name = af.name;
						anchor.fieldType = af.fieldType;
						anchor.debug = af.debug;
						anchor.offset = af.offset;
						anchor.padding = af.padding;
						anchor.colorSchemeName = af.colorSchemeName;
						anchor.color = af.color;
						anchor.required = af.required;
						anchor.multiple = af.multiple;

						//we tell that the anchor have been linked to an actual field
						anchor.fieldValidated = true;
						af.fieldValidated = true;
					}
				}
			}
			
			//we remove every anchors that is not anymore linked with a field
			inputAnchorFields.RemoveAll(a => !a.fieldValidated);
			outputAnchorFields.RemoveAll(a => !a.fieldValidated);

			//we add new fields to the input/output anchors list
			foreach (var anchorFieldKP in anchorFieldDictionary)
			{
				var af = anchorFieldKP.Value;
				//if the field was not found in both input and output anchor lists
				if (!af.fieldValidated)
				{
					if (af.anchorType == PWAnchorType.Input)
						inputAnchorFields.Add(af);
					else
						outputAnchorFields.Add(af);
				}
			}
		}

		void LoadUndoableFields()
		{
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			undoableFields.Clear();
			
			foreach (var fInfo in fInfos)
			{
				var attrs = fInfo.GetCustomAttributes(false);

				bool hasSerializeField = false;
				bool hasNonSerialized = false;

				foreach (var attr in attrs)
				{
					if (attr as PWInputAttribute != null || attr as PWOutputAttribute != null)
						goto skipThisField;
					
					if (attr as System.NonSerializedAttribute != null)
						hasNonSerialized = true;
					
					if (attr as SerializeField != null)
						hasSerializeField = true;
				}

				if (fInfo.IsPrivate && !hasSerializeField)
					goto skipThisField;
				
				if (hasNonSerialized)
					goto skipThisField;
				
				if (fInfo.IsNotSerialized)
					goto skipThisField;
				
				undoableFields.Add(fInfo);

				skipThisField:
				continue ;
			}
		}

		//retarget "Reload" button in the editor to the internal event OnReload:
		void GraphReloadCallback() { Reload(null); }

		void ForceReloadCallback() { Reload(null); }

		void ForceReloadOnceCallback() { Debug.Log("force reload once: TODO"); }

		void LinkStartDragCallback(PWAnchor fromAnchor)
		{
			//disable non-linkable anchors:
			if (fromAnchor.nodeRef != this)
				DisableUnlinkableAnchors(fromAnchor);
		}

		void LinkStopDragCallback()
		{
			//reset anchor highlight
			ResetUnlinkableAnchors();

			//reset link highlight
			foreach (var anchorField in anchorFields)
				foreach (var anchor in anchorField.anchors)
					foreach (var link in anchor.links)
						link.ResetHighlight();
		}

		void LinkCanceledCallback()
		{
			//reset the highlight mode on anchors:
		}

		void DraggedLinkOverAnchorCallback(PWAnchor anchor)
		{
			if (!PWAnchorUtils.AnchorAreAssignable(editorEvents.startedLinkAnchor, anchor))
				return ;

			//update anchor highlight
			if (anchor.anchorType == PWAnchorType.Input)
			{
				if (anchor.linkCount >= 1)
				{
					//highlight links with delete color
					foreach (var link in anchor.links)
						link.highlight = PWLinkHighlight.Delete;
					anchor.highlighMode = PWAnchorHighlight.AttachReplace;
				}
				else
					anchor.highlighMode = PWAnchorHighlight.AttachNew;
			}
			else
			{
				//highlight our link with delete color
				foreach (var link in editorEvents.startedLinkAnchor.links)
					link.highlight = PWLinkHighlight.Delete;
				
				if (anchor.linkCount > 0)
					anchor.highlighMode = PWAnchorHighlight.AttachAdd;
				else
					anchor.highlighMode = PWAnchorHighlight.AttachNew;
			}
		}

		void DraggedLinkQuitAnchorCallbck(PWAnchor anchor)
		{
			anchor.highlighMode = PWAnchorHighlight.None;

			//reset link hightlight
			foreach (var link in anchor.links)
				link.ResetHighlight();
			foreach (var link in editorEvents.startedLinkAnchor.links)
				link.ResetHighlight();
		}

		void AnchorLinkedCallback(PWAnchor anchor)
		{
			//CreateLink will raise the OnLinkCreated event in the graph and create the link
			graphRef.SafeCreateLink(anchor);
		}

		void AnchorUnlinkedCallback(PWAnchor anchor)
		{
			//TODO: unlink the anchor, remove the link.

			//raise internal event 
			// OnLinkRemoved(link);
		}

		void NodeSelectedCallback(PWNode node)
		{
			if (node == this)
				isSelected = true;
		}

		void NodeUnselectedCallback(PWNode node)
		{
			if (node == this)
				isSelected = false;
		}

		void LinkRemovedCalllback()
		{
			UpdateWorkStatus();
		}

		void LinkCreatedCallback(PWNodeLink link)
		{
			ResetUnlinkableAnchors();
		}

		void BindEvents()
		{
			//graph events:
			graphRef.OnForceReload += ForceReloadCallback;
			graphRef.OnForceReloadOnce += ForceReloadOnceCallback;
			graphRef.OnClickNowhere += OnClickedOutside;
			graphRef.OnLinkStartDragged += LinkStartDragCallback;
			graphRef.OnLinkStopDragged += LinkStopDragCallback;
			graphRef.OnPostLinkRemoved += LinkRemovedCalllback;
			graphRef.OnLinkCreated += LinkCreatedCallback;
			graphRef.OnLinkCanceled += LinkCanceledCallback;
			graphRef.OnNodeSelected += NodeSelectedCallback;
			graphRef.OnNodeUnselected += NodeUnselectedCallback;

			//local node events:
			OnDraggedLinkOverAnchor += DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor += DraggedLinkQuitAnchorCallbck;
			OnAnchorLinked += AnchorLinkedCallback;
			OnAnchorUnlinked += AnchorUnlinkedCallback;
		}

		void UnBindEvents()
		{
			//graph events:
			//null check because this function may be called without the node being initialized
			if (graphRef != null)
			{
				graphRef.OnForceReload -= GraphReloadCallback;
				graphRef.OnForceReloadOnce -= ForceReloadOnceCallback;
				graphRef.OnClickNowhere -= OnClickedOutside;
				graphRef.OnLinkStartDragged -= LinkStartDragCallback;
				graphRef.OnLinkStopDragged -= LinkStopDragCallback;
				graphRef.OnPostLinkRemoved -= LinkRemovedCalllback;
				graphRef.OnLinkCreated -= LinkCreatedCallback;
				graphRef.OnLinkCanceled -= LinkCanceledCallback;
				graphRef.OnNodeSelected -= NodeSelectedCallback;
				graphRef.OnNodeUnselected -= NodeUnselectedCallback;
			}
			
			//local node events:
			OnDraggedLinkOverAnchor -= DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor -= DraggedLinkQuitAnchorCallbck;
			OnAnchorLinked -= AnchorLinkedCallback;
			OnAnchorUnlinked -= AnchorUnlinkedCallback;
		}
	}
}