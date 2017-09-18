﻿using UnityEngine;
using System;

namespace PW.Core
{
	/*
	**	Vector samplers, same as Samplers but with vectors instead of floats
	*/

	public class Vector2Sampler2D : Sampler
	{
		public Vector2[,]	map;

		public Vector2Sampler2D(int size, float step)
		{
			this.step = step;
			Resize(size);
		}
		
		public override void Resize(int size)
		{
			this.map = new Vector2[size, size];
			this.size = size;
		}
		
		public override Sampler Clone(Sampler reuseObject)
		{
			Sampler2D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as Sampler2D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new Sampler2D(size, step);
			
			System.Buffer.BlockCopy(map, 0, newSampler.map, 0, map.Length);

			return newSampler;
		}
	}
	
	public class Vector3Sampler2D : Sampler
	{
		public Vector3[,]	map;
		
		public Vector3Sampler2D(int size, float step)
		{
			this.step = step;
			Resize(size);
		}

		public override void Resize(int size)
		{
			this.size = size;
			this.map = new Vector3[size, size];
		}
		
		public override Sampler Clone(Sampler reuseObject)
		{
			Sampler2D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as Sampler2D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new Sampler2D(size, step);
			
			System.Buffer.BlockCopy(map, 0, newSampler.map, 0, map.Length);

			return newSampler;
		}
	}
	
	[Serializable]
	public class Vector2Sampler3D : Sampler
	{
		public Vector2[,,]	map;

		public Vector2Sampler3D(int size, float step)
		{
			this.step = step;
			Resize(size);
		}

		public override void Resize(int size)
		{
			this.size = size;
			this.map = new Vector2[size, size, size];
		}
		
		public override Sampler Clone(Sampler reuseObject)
		{
			Sampler2D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as Sampler2D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new Sampler2D(size, step);
			
			System.Buffer.BlockCopy(map, 0, newSampler.map, 0, map.Length);

			return newSampler;
		}
	}
	
	[Serializable]
	public class Vector3Sampler3D : Sampler
	{
		public Vector3[,,]	map;

		public Vector3Sampler3D(int size, float step)
		{
			this.step = step;
			Resize(size);
		}

		public override void Resize(int size)
		{
			this.size = size;
			map = new Vector3[size, size, size];
		}
		
		public override Sampler Clone(Sampler reuseObject)
		{
			Vector3Sampler3D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as Vector3Sampler3D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new Vector3Sampler3D(size, step);
			
			System.Buffer.BlockCopy(map, 0, newSampler.map, 0, map.Length);

			return newSampler;
		}
	}
}
