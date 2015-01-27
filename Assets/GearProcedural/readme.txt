Procedural Gear
---------------

GearProcedural (folder)
	GearProcedural.cs		-> script to generate a gear object
	
	Editor (folder)
		GearInspector.cs	-> editor script to manipulate the gear.
	
	examples_startingpoints(folder)
		Gear_Standard		-> a prefab of a standard gear. A good starting point for each gear.
		Gear_Cone45		-> a prefab of a 45 degree cone gear.
						(drag&drop these prefabs into the scene view)


Installation:
-------------
Import the ´GearProcedural.package´ into your project: 

 Menü ´Assets -> Import package -> Custom package...´ , choose the ´GearProcedura.package´ and import all of it,
 or download it from Unity Asset Store and impot all files.


Use with the ´TransMission Gear Tool´
----------------------------------------
If you own the TransMission Gear Tool or want to build a project with it and want to use this procedural gear,
you have to delete some comments in the ´GearInspector.cs´ script. Look into the script. It is commented, which // have to delete.
The Proceodural Gear script will update the parameters of the TransMission script immediately, if you change the gears parameters. The TransMission script will be attached to a new gear automatically.


How to use:
-----------

Create a procedural gear:

	Click menu ´GameObject -> Create other -> Procedural Gear´

A gear was created now and you can change its parameters in the inspector or move the vertices (corners) in the scene view.

Select the gear in the ´Hierarchy View´, go with the mouse pointer over the ´Scene View´ and press ´F´ to zoom to the gear.


Gear
----
You see two colored points on the gears surface. You can click on one of this points and hold down the mousebutton to move it around.
You will notice, that the gear changes its look.
If you want to have a more detailed gear or more detailed teeth, add some points by click the ´+´ button in the inspector at ´Body Parts´
or ´Teeth Parts´.
You get a new point. It is a copy of the point, where you have clicked on ´+´.
Move this new point around to fit your desired look.

To move the vertices around, set the ´modul´ to value 0.5 (standard for new gears).
Otherwise the colored points are not exactly on the vertices. After modelling the teeth, set the modul.

Change the count of teeth or the modul (size) of the teeth with the values ´Teeth Count´ and ´Modul´
in the inspector.


Materials
---------
You can add more materials to the ´Mesh Renderer´ by drag&drop a material from the project view into the inspector (to the Mesh Renderer compontent).
In the Gear Inspector you see all materials, that are in the Mesh Renderer.
You can choose a material for each tooth- or body-part by clicking on the number button of each part.


UV´s
----

By default the gear scipt is set to ´Automatic Mapping´.
This means, gear-parts that are flat will be mapped planar and those who are over the ´Ramp Angle´ will be mapped cylindrical.
You can change the ´Ramp Angle´ in the inspector.
With this option you mostly get a good result.
If you want to change the mapping or set an offset to the uv´s/texture or scale them, you also can do it in the inspector.
For this uncheck the ´Automatic Mapping´ and you will see the parameters for each of your gear parts.


+ The procedural gears works together with the ´TransMission Gear Tool´.

+ You can copy, make prefabs, change material and so on like with other gameObjects.

+ Undo and multi-object editing is working.


Have fun with it.



For questions and feedback: support@magizzonapps.de















