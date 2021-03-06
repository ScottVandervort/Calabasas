Stuff to do ......
-------------------------------------------

Phase 1
-------------------------------------------

[X] Center Head

Stop using bounding box. Too course of measurement (int). Instead,
Given all points find point closest to center of head. This index is the "center".
The FaceState Center property should just regurgitate the vertex at this index.

[X] Display info on-screen

[X] Mouse

Click should display the face point in on-screen info.

[ ]	Determine which Kinect points/indices map to desired facial features

Create enums for each.
Render eyes, nose, etc.. using enums.

[x] Figure out some way to save / load facestate

[ ] Keyboard

(+/-) should scale face
(r)ender :  All points | Show curated points and lines | Jack-O-Lantern
(b)ounding box/center : Show bounding box and center point.
(s)ave : Saves the current state to file.
(d)ebug info : Show Debug Info.

[ ] Display Kinect state.

[ ] Optimize DirectX

Might need to migrate to Direct3D; Rendering points is expensive.

Phase 2
-------------------------------------------

[ ] Draw Pumpkin Face

[ ] Add a RadialGradient fill to all facial shapes whereas the center of the fill is in the screen center; This should make it look like a candle is backlighting everything the same way.

[ ] Make the RadialGradient "flicker" like a candle.

=======================================

Journal ......
-------------------------------------------

Day 1 - Inspiration
------------------------------------------

Microsoft Face API
https://msdn.microsoft.com/en-us/library/jj130970.aspx

Instructables
http://www.instructables.com/id/How-to-Connect-a-Kinect/?ALLSTEPS"

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


Day 16 - Solution Re-org
-------------------------------------------

Changed PumpkinFaceTracker to FaceTracker. Renderer now just listens to events broadcast from FaceTracer (i.e. it is now just a datasource). "Morphing" of vertices to a jack-o-lantern will now be done entirely in Renderer. This is because the renderer owns the windows form, keyboard/mouse events, etc...

Created a new face chart in Gimp with color-coded points that map to Kinnect FaceTracker indices. Microsoft did a real BAD job of this in the FeaturePoint enum that shipped with the xample code - it is missing a lot of key points.

Need to ...
1) Finish map.
2) Figure out how to draw lines in Renderer.
3) Draw each face component independently and verify mappings.

Day 17 - Researching how to draw polygons
-------------------------------------------

Found some more examples on how to render polygons and gradients using DirectX 2D

https://github.com/RobyDX/SharpDX_Demo/blob/master/SharpDXTutorial/TutorialD1
http://stackoverflow.com/questions/27289470/draw-and-fill-polygon-using-float-points-sharpdx
https://github.com/sharpdx/SharpDX-Samples/tree/master/Desktop/Direct2D1/MiniRect

Also, I think I can use a radial gradient to fill the facial shapes. I am hoping I can center the gradient at the screen center (i.e., the "candle" backlighting the face).

Day 18 - How to reset transformation
-------------------------------------------

I am going to need a computer, the Kinnect, a webcam (for watching/listening to the audience), a speaker (for talking to the audience), and a small projector. The computer is going to need to be relatively close.

You can save / restore changes to the rendering target by using SaveDrawingState() and RestoreDrawingState(). This resets andthing applied to the render target - such as transformattions.

Day 19 - Finished the onorous task of mapping
-------------------------------------------

Mapped 120 facial points tracked by Kinnect to a .png.

FaceTracking toolkit provides only 71 in Microsoft.Kinect.Toolkit.FaceTracking.FeaturePoint - and the vergage of the enum values is very very cryptic.

Day 20 - Time for a new sensor
-------------------------------------------

This weekend I tested the project against Jodie and myself. Eye blinks weren't being picked up. Nose scrunches weren't being picked up. Iris movements weren't being picked up. Mouth movement detection was at best, subpar. Sigh. I was about ready to shit can the whole project until I staretd researching the XBox One Kinect Sensor. I watched videos feeds of simple projects. The new sensor is much much improved. I didn't go this route originally because of the cost ( roughly $100 for the sensor and Windows 10 adapter used ). However, after thinking about it and talking it over with Jodie I am going to splurge and give it one final shot.

I have the new sensor on order from Ebay. Let's see what happens next...

Day 21 - Testing new sensor
-------------------------------------------

It works! Pretty well, actually.

This guy has proved invaluable: https://github.com/Vangos

There are two modes in Kinect Face Tracking 2.0 - normal and hi-def. Normal returns a bounding box and very rudimentary facial data. High-def returns about a thousand plotted points.

The Kinnect 2.0 API is available through NuGet as well which will make distrubuting these thing much much easier.

Day 22 - Bounding Box results in too much jitter for centering
-------------------------------------------

The normal face tracking bounding box is too jitery. I think it is because it is in screen coordinates (integers). This is way too course for the DirectX Device.

Instead, given all of the points in the face I am going to programatically pick the one closest to the center of the face and store the index. The face center will now be the point at this index.

Also, found some good links for picking points on the rendered screen

http://www.gamedev.net/topic/557284-mouse-hit-testknow-when-mouse-is-over-something/
http://stackoverflow.com/questions/38255693/why-is-my-mouse-cursor-coordinates-suddenly-scaled

Day 23 - Working on centering
---------------------------------------------

I created some unit tests for the centering algorithms and they failed to run. This is because Visual Studio needs to be configured to run the tests in 64-bit as
the projects are all 64-bit. More info here: https://msdn.microsoft.com/en-us/library/ee782531.aspx

So, the face center will require a bounding box (which is updated continuously) and the index of the Kinect point corresponding to the top of the head.
I chose the top of the head as it shouldn't change when the subjects jaw moves. The cen

It looks like Kinect WILL attempt to regain face tracking if lost. Best way to recalibrate is to move away from camera about 5 feet and then slowly re-approach it.

Lastly, my DirectX skills are crap. Rendering is taking way way way too long. Need to optimize.

Day 24 - Direct2D Optimization
---------------------------------------------

Drawing 1000 points is freaking slow in Direct2D. I now create a single resuable geometry ( i.e., point) and translate the targets transform to the position of each point when rendering.
I also started drawing rectangles instead of ellipses. It has spec up the frames per second 3x - BUT, 180 FPS while rendering 1000 points is still horribly slow.
Fortunately, dots are only going to be displayed during debug. The actual face will consists of lines/filled polygons.
I am not going to look into migrating to Direct3D unless line/polygon rendering is also abysmal.

Day 25 - Hooking up Keyboard Commands
----------------------------------------------

The most useful thing right now would be to save/restore the state. This would allow me to capture a "still" of the Kinect and then work with it offline.

This link shows how to save/load a struct to file: http://stackoverflow.com/questions/17338571/writing-bytes-from-a-struct-into-a-file-with-c-sharp

Day 26 - Creating a Mock/Dummy Camera
----------------------------------------------

From the command-line you can now load a previosuly saved face by targeting a "state" file. This will bypass the Kinect and render whatever snapshot is stored in the file. 

A "state" file can be created by clicking (s) when running normally. I also have created a UnitTest to dummy up a file - it renders a square.

Why do all this? 

The Kinect will only run on Windows 8+ and I occasionally work off a Windows 7 machine. Also, it would be nice to work with a face that is not "live".

Lastly, early on in this project I noted how to install the SDK. Well, that was for Kinect v1. Kinect v2 has a different SDK here https://www.microsoft.com/en-us/download/details.aspx?id=44561 
as well as a Kinect Runtime (https://www.microsoft.com/en-us/download/details.aspx?id=44559) that can be installed for non-developers.




