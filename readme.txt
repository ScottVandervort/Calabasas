The primary goal of this project is to use Microsoft's Face API and a v1 Kinnect to record my face live, render it as a jack-o-lantern, and project it onto a pumpkin. The secondary goal of this project is to mess with a lot of kids by Halloween 2017.

Journal
--------------------------------------------------------------------------------------------

Day 1 - Inspiration
------------------------------------------

Microsoft Face API
https://msdn.microsoft.com/en-us/library/jj130970.aspx

Instructables
http://www.instructables.com/id/How-to-Connect-a-Kinect/?ALLSTEPS

Day 2  - Installation
------------------------------------------

Install in this order ...

Kinnect for Windows SDK 1.7
https://www.microsoft.com/en-us/download/details.aspx?id=36996

Kinnect for Windows Developer Toolkit v1.7
https://www.microsoft.com/en-us/download/details.aspx?id=36998

Day 3 - Play with samples
------------------------------------------

Demo browser is available here
C:\Program Files\Microsoft SDKs\Kinect\Developer Toolkit v1.7.0\Tools\ToolkitBrowser

For the samples to work you need to be 4 feet away from the Kinnect. If you have glasses and cannot see shit this could be a problem

The blinked LED is normal.

Day 4 - Create sample project
------------------------------------------

Going C# as I haven't done win32 forever

Using FaceTrackingBasics-WPF as a template

As I just need a 2D mesh it looks like IFTResult.Get2DShapePoints() is what I need. There are some other goodies as well that are covered in the Face API from Day 1.

Day 4 - Yup, definitely going C#
------------------------------------------

Only spent a few minutes messing around today. I installed C/C++ Win32 support in Visual Studio 2015 and created a Solution for Calabasas - and immediately got confused.

Now doing it in C# and using the "Face Tracking 3D WPF" sample as a template. There a couple of open source DirectX wrappers available for C#. Either should work for what I am doing.

Day 5 - Wrote some untested Kinnect initialization code
------------------------------------------

The "Face Tracking 3D WPF" example renders the entire image captured by the Kinnect; I don't think I care about this - at least not for now.

FaceTrackingViewer.xaml.cs is key. Need to figure out what is the relationship between the [Kinnect] Sensor, Skeleton, SkeletonFaceTracker, FaceTracker, and FaceTrackFrame. It looks like the desired points are retrieved from FaceTrackFrame - but I need to know how to generate it.

Next time should test sensor initialization code and research relationship between these classes.

Day 6 - Sensor Initialization Code Works
------------------------------------------

So, my code works. The WPF example hits an exception  when setting the depth of the Kinnect :
 newSensor.DepthStream.Range = DepthRange.Near;

It recovers by setting it to DepthRange.Default. I read that newer Kinnects are better at close range. APparently mine is not one of those.

I am in the process of merging the WPF MainWindow and FaceTrackingViewer into the same file. I don't want this to be tightly coupled to a view of any sort. That will come later. I just want to write out facial vertices to the console for now.

The SkeletonFaceTracker appears to be important - and fortunately, it can be easily ported over.

It looks like AllFramesReady and KinectChanged are the two main entry points for the Kinnect.

Lastly, I picked a bad name for my project and will need to change it as FaceTracker is one of the main assemblies used by Microsofts API. I should have seen this coming.

Day 7 - It compiles
------------------------------------------

SkeletonFaceTracker stores facePoints for each frame. Each point is a location on the face ( FeaturePoint)  and an x,y value (PointF).

Each time a new frame is rendered I want to print location of my forehead to see if it changes when I move.

Day 8 - It runs
------------------------------------------

Added a console out that tracks my chin. 

Needed to reference projects from FaceTrackingBasics-WPF example. The assemblies would not load properly.

Need to expose facePoints from FaceTracker, These will be consumed by renderer.

Need to research a renderer using managed DirectX; Need to draw lines between facepoints at 60 fps.

Day 9 - Working with DirectX ( SharpDX )
------------------------------------------

I found a managed wrapper around DirectX called SharpDX (https://github.com/sharpdx).I forgot how much fun it is working with low-level DirectX. So much stuff that can be relegated to boiler-plate code. Sigh.

I found a pretty good tutorial (https://github.com/mrjfalk/SharpDXTutorials) that draws a simple Triangle using vertices and some shaders. This is pretty much all I need given the 2D points that the Kinnect Face Tracker is going to produce.
One problem with the tutorial is that it is using a slightly older version SharpDX so I have stumbled a few times. 3.0.1.0 versus 3.1.1.0.

I am [trying] to use nugget for dependencies. Unfortunately, as I am using a v1 Kinnect (Xbox 360) I am stuck with the 1.7 SDK. The 1.7 SDK is NOT kept in nugget and requires an installer. So much for portable code....

Day 10 - Sample DirectX Project Working
------------------------------------------

This required pulling in a BUNCH of dependencies for SharpDX from Nuget. I almost switched over to using MonoGame - but I think it will be way overkill for the first version of this project. 

Next up ...

Clean-up projects / solution and get uploaded to Github.

Add a new buffer for each facial feature.

Expand the number of vertices in each buffer to coincide with the number of points in each facial feature.

Update the vertices in each buffer whenever kinnect changes.

Day 10 - Part 2 - Cleaning up the project for Github
------------------------------------------

So, .csproj refers to KINECTSDK10_DIR; This token (and others) are environment variables set up by SDK installers. Can use them to add non-nuget dependencies to the Calabasas solution and projects,

I finally was able to ditch the Toolkit and Toolkit.Facetracking projects and just use the .dll's provided by the SDK. This is what needed to be done ... 

- My projects need to be compiled as amd64 (64-bit).
- Need to copy facetrackdata.dll and facetracklib.dll from the SDK's \Redist\amd64 folder.

This helped a lot: https://channel9.msdn.com/Forums/Coffeehouse/kinect-face-tracking-tutorial

Day 11 - Architecture
------------------------------------------

PumpkinFaceTracker.Program (i.e., the Kinnect) spawns a new PumpkinFaceTracker object for every frame and stores it in array. 
Each PumpkinFaceTracker does some logic and generates facial features.
These feature need to be rendered using DirectX.

Does renderer get injected into PumpkinFaceTracker; PumpkinFaceTracker would call renderer directly.
Or, does PumpkinFaceTracker broadcast an event that renderer listens for? Or, calls renderer method directly based on interface (faster).

Day 12 - Face API facial point enumeration is incomplete!
------------------------------------------

It looks like the FeaturePoint enumeration does not include all of the facial points that I have seen displayed in the Kinnect propaganda online. 
	Or, the enumeration is in a different order.

Next step is to create a 2nd Draw() method in Renderer that just takes all points from Kinnect, Renders the point as their index/number on the screen.

Ugh.....

Day 13 - Forget D3D, I am using D2D ... and DirectWrite
------------------------------------------

	SharpDx has better examples than the one I was originally working from:
		https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct2D1/MiniRect/Program.cs
		
	It is going to be esier to use Direct2D than Direct3D. It looks like it supports bitmaps, shaders, ... just about everything I need to render a 2D jack-o-lantern face.
	
	Furthermore DirectWrite will make it easier to render numbers on the screen that coordinate with the facial positions.
	
Day 14 - Now displaying facial points in DirectX as their respective indices
-------------------------------------------

	... but they are scrunched. Now I need to figure out how to create and apply transforms to D2D. I would like to scale and transform the points that I display.

	This will come in handy when I start "connecting the dots" as well.

Day 15 - Direct2D/SharpDx supports matrix transformations
-------------------------------------------

	Example: https://github.com/sharpdx/SharpDX-Samples/blob/master/StoreApp/XAML%20SurfaceImageSource%20DirectX%20interop%20sample/C%23/Scenario1Component/Scenario1ImageSource.cs

	Now need to scale and center the rendered face using a translation and scaling matrix.

	I am a bit rusty so need to figure out how to go about doing this (again).


=======================================

Stuff to do ......
-------------------------------------------
		
[ ] Draw Kinnect Face using DirectX	
	[ ] Determine how points map to facial features.
		[ ] Figure out how to render text using DirectX.
			[ ] Plot labeled points using DirectX whereas each points corresponds to it's index in the face array generated by Kinnect.
				[ ] Figure out how to transform face array so that they can be viewed in DirectX.
					[ ] Document / comment transformation process in Journal.
					[X] Take a screen shot of DirectX.		
	[ ] Create a enum that maps facial points necessary for Jack-O-Lantern; Eyes, Nose, Mouth, Eyebrows, and possibly top of head, sides of head, and nose as they should remain static for centering purposes ( the jaw moves ).
		[ ] enum needs to be in a common assembly as it will be used by Renderer and Tracker.
		[ ] In Kinnect, re-process facial points array to only include those of enum.
			[ ] Send points to Renderer.
				[ ] Figure out how to draw lines/polygons in renderer.
					[ ] Allow key press to toggle between labeled points, lines, and both.
				[ ] Render points as polygons.

[ ] Renderer should accept Tracker as an argument; Renderer owns the Form and as such is the only one that can detect keyboard input; Render can display Kinnect status while loading,
				
[ ] Add Log4Net
	[ ] Log Kinnect status.
	[ ] Occassionally log FPS.
	[ ] Occassionally log points generated by Kinnect
		
[ ] Draw Pumpkin Face 

