Dynamic Water System v1.3.2
===========================

Comprehensive documentation can be found at http://cdn.lostpolygon.com/dynamicwater/ 
or by clicking Component -> Lost Polygon -> Dynamic Water System -> Open Documentation In Browser.

In order for package to function, you must add following tags (tags are already added on included examples):
  -- DynamicWater
  -- DynamicWaterObstruction
  -- DynamicWaterObstructionInverted
  -- DynamicWaterPlaneCollider
You must also add the following layers:
  -- DynamicWaterPlaneCollider
  -- DraggableObject (only used for included examples)
This is done automatically, but you may want to do that manually if you encounter problems.

This package includes following examples:
  -- Pool Scene, demonstrating the buoyant objects, interacting with water surface, setting up static obstructions.
  -- Buoyancy demo scene, demonstrating how buoyancy can be applied to the objects of various shapes, and showing how to implement custom ambient waves.
  -- Waterfall Scene, demonstrating usage of SplashZone component to create a waterfall effect.
  -- Boat Scene, demonstrating how to create a simple boat controller and how to use the DynamicWaterSolverAdvancedAmbient solver.
  -- Character Scene, showing an example of how to integrate the system to the character controller.
  -- Obstruction demo scene, demonstrating complex usage of obstruction geomerty and obstruction masks.

You can create DynamicWater object in Gameobject -> Create other -> Dynamic Water.
You can add components by Component -> Lost Polygon -> Water System menu.

--
Free sounds from http://www.freesfx.co.uk
Free boat model from http://www.turbosquid.com