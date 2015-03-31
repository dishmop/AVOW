using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class ExtensionMethods {

	// This should be in a utility file somwhere
	
	public static T[]  RemoveAt<T>(this T[] source, int index)
	{
		T[] dest = new T[source.Length - 1];
		if( index > 0 )
			Array.Copy(source, 0, dest, 0, index);
		
		if( index < source.Length - 1 )
			Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
		
		// Now copy the destination back into the source
		return dest;
	}	
	

	public static void Write(this BinaryWriter bw, Vector3 vector){
		bw.Write(vector[0]);
		bw.Write(vector[1]);
		bw.Write(vector[2]);
		
	}
	
	public static void Write(this BinaryWriter bw, Quaternion quat){
		bw.Write(quat[0]);
		bw.Write(quat[1]);
		bw.Write(quat[2]);
		bw.Write(quat[3]);
	}
	
	public static void Write(this BinaryWriter bw, Color col){
		bw.Write(col.r);
		bw.Write(col.g);
		bw.Write(col.b);
		bw.Write(col.a);
	}
	
	
	public static Vector3 ReadVector3(this BinaryReader br){
		Vector3 vec = new Vector3();
		vec[0] = br.ReadSingle();
		vec[1] = br.ReadSingle();
		vec[2] = br.ReadSingle();
		return vec;
	}
	
	public static Quaternion ReadQuaternion(this BinaryReader br){
		Quaternion quat = new Quaternion();
		quat[0] = br.ReadSingle();
		quat[1] = br.ReadSingle();
		quat[2] = br.ReadSingle();
		quat[3] = br.ReadSingle();
		return quat;
	}
	
	public static Color ReadColor(this BinaryReader br){
		Color col = new Color();
		col.r = br.ReadSingle();
		col.g = br.ReadSingle();
		col.b = br.ReadSingle();
		col.a = br.ReadSingle();
		
		return col;
	}
}
