# Introduction
This is the project for operating system class, spring 2022. We're designing a VR office system, supporting dynamically importing 3DSaaS files, and operating the model(zoom, rotate, move, set visibility, etc.); controlling remote endpoints with low latency; Operating remote devices(not computers. For example, a plane). All imported objects can be 3D surrounding. Things can be operated solely by hand, which means no handles.
# Requirements:
* python3 and corresponding packages:
    * socket
    * pynput
    * win32api
* Unity 2020
* Oculus Quest 2
* A laptop with NVIDIA graphic card(recommend to use a game laptop)
# Usage
## 3D object importing
* Put your right palm up to make the menu visible.
* Select 3D Object Import.
* Input the **directory** of the 3DSaaS file(folder "obj example" can be used as an example)
* Press "Submit" and see the imported objects.
## Remote control
Attention: don't use the campus network unless testing on 127.0.0.1, because most campus network don't supporting UDP broadcasting.
* Set ScreenShare_server/ScreenShare.exe to be opened using Intel graphic card and open it.
* Open ScreenShare_server/controller.py **using the administrator mode**.
* Open remode desktop in oculus interface.
* The server port is 5901 in default.
* If you want to drag, press the virtual screen for as least 1.5 seconds.
## Model plane
* Open Other Servers/plane_server.py.
* Open plane_client.py.
The default port is 6666.
# Example video
See at http://home.ustc.edu.cn/~xuyichang/share/VRFancyOffice.
# Team Members
* [徐亦昶](https://github.com/Kobe972)(PB20000156)
* [吕泽龙](https://github.com/MaxtirError)(PB20061229)
* [阮继萱](https://github.com/Scarlett0815)(PB20000188)
* [徐笑阳](https://github.com/SkiperDyxx)(PB20000155)
* [王铖潇](https://github.com/start-shine)(PB20000072)

