  

# RemoteAssistantAR

  This application is a Proof of Concept (POC) App for how to build a remote assistant app (similar to Vuforia Chalk) using AR Foundation and Agora.io's Video SDK.

  

#### Dependencies:

- Unity 2019

- Unity Asset [Agora Video SDK](https://assetstore.unity.com/packages/tools/video/agora-video-sdk-for-unity-134502)

- Agora AppId [Project page](https://console.agora.io/projects)

  

#### Project Integration:

1. Clone this project

2. Open the project from Unity 2019.2 or Above.  (2019.3 recommended)

3. Download and import the Agora Video SDK from Asset Store.  Unselect everything in "demo" folder when importing, except the README files.

4. Use Package Manager from Unity Editor to install ARFoundation 3.0.1/ARKit 3.0.1/ARCore 3.0.1.

5. Open Main scene and input your AppID to the GameController game object.

6. Following the settings in README_ARFoundation, build and run on compatible iOS and Android devices to test.

#### Device Requirements
This app works with any iOS device that supports ARKit 
- iPhone (6S or newer)
- iPad (5th Generation or newer)
Or Android device that supports ARCore, which is Android 7 or above.


## How To Use
The AR Remote Assistant app is meant to be used by two users who are in two separate physical locations. One user will input a channel name and CREATE the channel. This will launch a back facing AR enable camera. 
The second user will input the same channel name as the first user and JOIN the channel. Once both users are in the channel, the user that "JOINED" the channel has the ability to draw on their screen, and the touch input is sent to the other user and displayed in Augmented Reality.
![RemoteAssistantAR](https://user-images.githubusercontent.com/1261195/77468506-e3bf2e00-6dca-11ea-88fe-a82aa527f5a0.png)

### Disclaimer
- This is not the only approach to do AR Screen Sharing.  Please contribute your ideas and make pull request if you have a better solution.
### Known Issues
- The drawn outlines may not be on the surface of target object; this is due to hardcoded Z-Positions of the placement.  
- There may be a few seconds delay when the two clients join the channel and start streaming.
- You may experience long delays on lower end devices (e.g. iPhone 6s) if you try to screen record the CastAR Client. 
