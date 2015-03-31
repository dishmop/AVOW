using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SerialisationFactory{


	public static int GetCommandCode(AVOWCommand command){
		int code = 0;
		if (command is AVOWCommandAddComponent){
			code = 1;
		}
		else if (command is AVOWCommandRemove){
			code = 2;
		}
		else if (command is AVOWCommandSplitAddComponent){
			code = 3;
		}
		return code;
	}
	
	public static AVOWCommand ConstructCommandFromCode(int code){
		switch (code){
			case 1:{
				return new AVOWCommandAddComponent();
			}
			case 2:{
				return new AVOWCommandRemove();
			}
			case 3:{
				return new AVOWCommandSplitAddComponent();
			}
		}
		return null;
	}
	
}
