# unity-example
A basic example Unity game to demonstrate integration with the Game Tester API.

## How To Use
If you want to run the game yourself please install Unity 2018.4.25f1. Later versions might work but will require Unity to upgrade the project which could cause problems.

There are two important files you should be aware of in this example:
<br>
GameTester.cs
<br>
UiManager.cs

### GameTester.cs
This is our API plugin/wrapper that does the heavy lifting of sending and receiving data to and from the Game Tester API. You do not have to make changes to this file yourself but feel free to investigate what it does. Note that the configuration of this file is for Sandbox mode. You can change it to Production mode in the constructor.

The newest version of this file can always be found here:

```
https://github.com/Game-Tester/unity-api-wrapper
```

### UiManager.cs
This is the Unity script that interacts with the game UI and the GameTester.cs file. For example, it will listen to the button click event of the login form and then send the player's pin to the plugin's Auth function.



