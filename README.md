  

# RemoteAssistantAR

  This application is a Proof of Concept (POC) App for how to build a remote assistant app (similar to Vuforia Chalk) using AR Foundation and Agora.io's Video SDK. A complete tutorial is available on [this blog](https://medium.com/@rcsw.devel/video-chat-with-unity3d-and-ar-foundation-chapter-3-remote-assistant-app-c7e14a7b0527).

  

#### Dependencies:

- Unity 2019

- Unity Asset [Agora Video SDK](https://assetstore.unity.com/packages/tools/video/agora-video-sdk-for-unity-134502)

- Agora AppId [Project page](https://console.agora.io/projects)



#### Project Integration:

1. Clone this project

2. Open the project from Unity 2019.2 or Above.  (2019.3 recommended)

3. Download and import the Agora Video SDK from Asset Store.  Unselect everything in "demo" folder when importing, except the README files.

4. Use Package Manager from Unity Editor to install ARFoundation /ARKit /ARCore.  You may have to try and figure out what version of ARFoundation works for your Unity Editor version.  Here are some verified version combinations.


| Editor |ARFoundation   |ARKit   |ARCore   |
| ------------ | ------------ | ------------ | ------------ |
|   2019.2 to 2019.4.18|3.0.1   |3.0.1   |3.0.1   |
|   2020.1.6| 3.1.6| 3.1.7 | 3.1.7 |
| 2020.2.5| 4.0.12| 4.0.12 | 4.0.12 |
| 2021.1.0| 4.1.17| 4.1.17 | 4.1.17 |

**Note**: in 2020.2.5, make sure to go into project settings XR Plug-in Management and select the XR plug-in providers you need for your project，otherwise you may get a blank camera screen.

5. Open Main scene and input your AppID to the **GameController** game object.  Note that we don't use AppID with certificate in this demo.  But it is highly recommended you use one and implement tokens in your actual application for better security measures.

6. Following the settings in **README_ARFoundation**, build and run on compatible iOS and Android devices to test.

#### Device Requirements
This app works with any iOS device that supports ARKit 
- iPhone (6S or newer)
- iPad (5th Generation or newer)
Or Android device that supports ARCore, which is Android 7 or above.


## How To Use
The AR Remote Assistant app is meant to be used by two users who are in two separate physical locations. One user will input a channel name and CREATE the channel. This will launch a back facing AR enable camera. 
The second user will input the same channel name as the first user and JOIN the channel. Once both users are in the channel, the user that "JOINED" the channel has the ability to draw on their screen, and the touch input is sent to the other user and displayed in Augmented Reality.
![RemoteAssistantAR](https://user-images.githubusercontent.com/1261195/77468506-e3bf2e00-6dca-11ea-88fe-a82aa527f5a0.png)

### Update 1

 - For testing with LiveBroadcasting Mode:

![Screen Shot 2020-11-04 at 6 24 44 PM](https://user-images.githubusercontent.com/1261195/98190089-2f8e9580-1ecb-11eb-9073-73706d0723e3.png)

 - Tap the Logo to switch modes.  The Viewer button will only appear for
   LIVE_BROADCASTING mode.

  


### Disclaimer
- This is not the only approach to do AR Screen Sharing.  Please contribute your ideas and make pull request if you have a better solution.
- Agora assumes no responsibility of this demo app, working or not.

### Known Issues
- The drawn outlines may not be on the surface of target object; this is due to hardcoded Z-Positions of the placement.  Please **do not ask me how to solve this problem** and instead show me how you solved it if you could.
- There may be a few seconds delay when the two clients join the channel and start streaming.
- You may experience long delays on lower end devices (e.g. iPhone 6s) if you try to screen record the CastAR Client. 
- This project is designed for running in 1-to-1 Communication mode. Please make your own modification for more users.

## License
The MIT License (MIT).
