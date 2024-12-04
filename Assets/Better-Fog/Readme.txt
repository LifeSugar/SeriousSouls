Support: Discord (https://discord.gg/K88zmyuZFD) or izzynab.publisher@gmail.com

Online Documentation: https://inabstudios.gitbook.io/better-fog/

Offline Docs:

/////////////////////////////


IMPORTING
After downloading the asset, import the appropriate .unitypackage file based on your Unity version and SRP (either Built-in.unitypackage, URP.unitypackage, or URP2022.unitypackage).

HOW TO USE

BUILT-IN

Ensure you have the Post Processing Stack V2 installed in your project. 
You can then easily add the Better Fog effect to your volumes.

You need one of the following in your scene:
A directional light with shadows enabled.
The CameraDepth.cs script added to your camera to enable the camera depth texture.


URP

Add BetterFogFeature to your URP data asset.
Set the Event property to "Before Rendering Post Processing".
Now you can add the Better Fog effect to your post-processing volume.

If you encounter issues setting up, you can use the provided URP settings asset. Adjust it via the Graphics section in your project settings. 
Ensure that the render asset is not overridden in the Quality tab.
Make sure in Quality tab render asset is not overriden


/////////////////////////////

The "Better Fog" asset was developed by building upon and integrating components from the SSMS project, which is licensed under the MIT license. (https://github.com/OCASM/SSMS)

Copyright (C) 2015, 2016 Keijiro Takahashi, OCASM

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR 
THE USE OR OTHER DEALINGS IN THE SOFTWARE.