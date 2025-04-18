See [Document](https://macacagames.github.io/GameSystem/) for more detail.

# Welcome to Macaca GameSystem

Macaca GameSystem is a framework to build game life cycle in Unity3D.

### Note: start from 1.0.3, this package include a precompiled Rayark.Mask Dlls.

---

## Features

- Life cycle management and callback.
- Replaceable gamelogic implement via Unity's ScriptableObject.
- Contains a lightweight dependency injection system.

## Installation

### Option 1: Unity Package manager (Recommend)

Add it to your editor's `manifest.json` file like this:

```json
{
  "dependencies": {
    "com.macacagames.gamesystem": "https://github.com/MacacaGames/GameSystem.git"
  }
}
```

### Option 2: Installation via OpenUPM

```sh
openupm add com.macacagames.gamesystem
```

### Option 3: Git SubModule

```bash
git submodule add https://github.com/MacacaGames/GameSystem.git Assets/MacacaGameSystem
```

## ApplicationLifeCycle

```

                                    Unity App Start
                                            |
                                            V
                                ApplicitionController Init
                                            |
                                            V
                            Instance all ScriptableObjectLifeCycle
                    Get all MonoBehaviourLifeCycle instance in Scene
                            Instance all [ResloveTarget] class
                                            |
                                            V
            Init all ScriptableObjectLifeCycle, MonoBehaviourLifeCycle, [ResloveTarget] instance
                                            |
                                            V
                                    Inject all target
                                            |
                                            V
                    ┌─────────────>─────────┐   [OnApplicationBeforeGamePlay]
                    |                       |
                    |                       |─────────────[ApplicitionController.ApplicationTask]
                    |       ┌───────>──┐    |
                    |       |          |    |
                    |       |       [Game Lobby] (A state for waiting for enter gameplay)
                    |       |          |    |
                    |       └──────────┘    |
                    |                       |   [ApplicationController.Instance.StartGame]
                    |                       V
                    |       ┌───────>──┐    |
                    |       |          |    |
                    |       |     [GamePlayData.GamePlay()]
                    |       |          |    |
                    |       └──────────┘    |
                    |                       |
                    |                       |
                    |                       |
                    └───────────────────────┘   [GamePlayController.SuccessGamePlay]

```

## IGamePlayData

IGamePlayData is the main game logic implemention. Do your logic in the callbacks to complete your game. For each callback's detail please see [Document](https://macacagames.github.io/GameSystem/api/MacacaGames.GameSystem.IGamePlayData.html)

Create a cs file which inherit from ScriptableObjectGamePlayData, and referenced it to ApplicationController.

```csharp
void Init();

void OnApplicationBeforeGamePlay();

void OnGameValueReset();

Task OnEnterGame();

IEnumerator GamePlay();

void OnLeaveGame();

void OnGameSuccess();

void OnGameLose();

Task GameResult();

void OnGameEnd();

void OnGameFaild();

Task OnContinueFlow(IReturn<bool> result);

bool IsContinueAvailable { get; }

void OnContinue();

void OnGUI();
```

## Time Manage

For most Unity developer, they will use the built-in [Time](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Time.html) class to control the time in the game, but use the Time class directly will cause some side effet in runtime, e.g. the Animator playback issue etc.

Therefore the GameSystem manage the app time and the gameplay time itself.

### App Time

The app time is same as the [Time](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Time.html) Class, which is the application clock, usually the Unity Developer will using the `MonoBehaviour.Update()` to do some time relative stuff, in GameSystem, it is recommand to use the `IApplicationLifeCycle.OnApplicationUpdate()` instead, but becareful the IApplicationLifeCycle corrently is only suppor singleton instance.

If you would like to use the GameSystem time in multiple instance, there are another APIs to support this feature.

```csharp
// Regist a IResumable(the Coroutine instance in Rayark.Mast) into the Application timer
ApplicationController.Instance.RegistApplicationExecuter(IResumable c);

// UnRegist a IResumable(the Coroutine instance in Rayark.Mast) from the Application timer
ApplicationController.Instance.UnRegistApplicationExecuter(IResumable c);
```

The `IResumable` is a part of [Rayark.Mast](https://github.com/rayark/mast), a powerful Coroutine implement by [Rayark Inc.](https://rayark.com/zh/), go to the repo to know more info about this package.

Here is a simple example:

```csharp
public class MyLogic : MonoBehaviour{

    Rayark.Mast.Coroutine coroutine;

    // Use this to replace the MonoBehaviour.Update()
    IEnumrator MyUpdateRunInApplicationTime(){
        while(true){
            // Do something

            // this loop will excude every frame
            yield return null;
        }
    }

    void OnEnable(){
        coroutine = new Coroutine(MyUpdateRunInApplicationTime());
        ApplicationController.Instance.RegistApplicationExecuter(coroutine);
    }

     void Disable(){
        if(coroutine !=null){
            ApplicationController.Instance.UnRegistApplicationExecuter(coroutine);
            coroutine = null;
        }
    }
}
```

### Game Time

To make more accurate time control in the game, the GameSystem define the Game Time, the Game Time will only tick every when the ApplicatonController is in the GamePlay State.

And there is 2 kind of Game Time, `GameTime` and `Unpause GameTime`, just same as there name, which always tick in the GamePlay State and another one only tick when the Game State is not in `Pause` state.

To switch the GameSystem from UnPause into Pause, you can call this API.

```csharp
ApplicationController.Instance.GetGamePlayController().EnterPause();
```

And also you can call this API, to switch from Pause to Unpause.

```csharp
ApplicationController.Instance.GetGamePlayController().ResumePause();
```

Just like the App time, you can use the APIs to regist the coroutine to GamePlayController.

```csharp

// Same as ApplicationController.RegistApplicationExecuter() but regist into the GameTime
ApplicationController.Instance.GetGamePlayController().AddToUpdateExecuter();
ApplicationController.Instance.GetGamePlayController().RemoveFromUpdateExecuter();

// Same as ApplicationController.RegistApplicationExecuter() but regist into the Unpause GameTime
ApplicationController.Instance.GetGamePlayController().AddToUnpauseUpdateExecuter();
ApplicationController.Instance.GetGamePlayController().RemoveFromUnpauseUpdateExecuter();
```

Here is a simple example:

```csharp
public class MyLogic : MonoBehaviour{

    Rayark.Mast.Coroutine coroutineGameTime, unpauseCoroutineGameTime;

    // Use this to replace the MonoBehaviour.Update(), only excude when the Game State is not in Pause
    IEnumrator MyUpdateRunInGameTime(){
        while(true){
            // this loop will excude every frame (When not Pause)
            yield return null;
        }
    }

    // Use this to replace the MonoBehaviour.Update()
    IEnumrator MyUpdateRunInUnpauseGameTime(){
        while(true){
            // this loop will excude every frame (Alway, in Game State)
            yield return null;
        }
    }

    void OnEnable(){
        coroutineGameTime = new Coroutine(MyUpdateRunInGameTime());
        ApplicationController.Instance.GetGamePlayController().AddToUpdateExecuter(coroutineGameTime);

        unpauseCoroutineGameTime = new Coroutine(MyUpdateRunInUnpauseGameTime());
        ApplicationController.Instance.GetGamePlayController().AddToUnpauseUpdateExecuter(unpauseCoroutineGameTime);
    }

     void Disable(){
        if(coroutineGameTime !=null){
            ApplicationController.Instance.RemoveFromUpdateExecuter(coroutineGameTime);
            coroutineGameTime = null;
        }
        if(unpauseCoroutineGameTime !=null){
            ApplicationController.Instance.GetGamePlayController().RemoveFromUnpauseUpdateExecuter(unpauseCoroutineGameTime);
            unpauseCoroutineGameTime = null;
        }
    }
}
```

## How to pause your game (Best Pratice)

- Always do your main logic in the `IGamePlayData.GamePlay()`
```csharp
IEnumerator GamePlay(){
    while(true){
        // Do Your main Game loop logic here is the best!
        yield return null;
    }
}
```

- Use the `ApplicationController.Instance.GetGamePlayController().AddToUpdateExecuter()` with `IEnumrator()` to replace the update

For instance, 
```csharp
public class Enemy : MonoBehaviour{

    Rayark.Mast.Coroutine unpauseCoroutineGameTime;

    // Use this to replace the MonoBehaviour.Update(), only excude when the Game State is not in Pause
    IEnumrator MyUpdateRunInGameTime(){
        while(true){
            // this loop will excude every frame (When not Pause)
            // The enemy logic 
            yield return null;
        }
    }

    void OnEnable(){
        coroutineGameTime = new Coroutine(MyUpdateRunInGameTime());
        ApplicationController.Instance.GetGamePlayController().AddToUpdateExecuter(coroutineGameTime);
    }

     void Disable(){
        if(coroutineGameTime !=null){
            ApplicationController.Instance.RemoveFromUpdateExecuter(coroutineGameTime);
            coroutineGameTime = null;
        }
    }
}
```

Once follow up the steps, you can very easy control the Game Pause by calling `ApplicationController.Instance.GetGamePlayController().EnterPause()` or `ApplicationController.Instance.GetGamePlayController().ResumePause();`

#### Script Tips
Here is the pre-implement base class to let you very easy to replace the MonoBehaviour and use the powerful GameSystem time manage. 

Create a .cs file and paste the follow content.
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rayark.Mast;
using Coroutine = Rayark.Mast.Coroutine;
using MacacaGames.GameSystem;

public class GameplayUpdateReceiverBase : MonoBehaviour
{
    public virtual void Awake()
    {
        ApplicationController.Instance.ResolveInjection(this);
    }
    public List<Coroutine> coroutines = new List<Coroutine>();

    protected void RegistCoroutine(Coroutine c)
    {
        ApplicationController.Instance.GetGamePlayController.AddToUpdateExecuter(c);
        coroutines.Add(c);
    }

    public void DisposeGamePlayUpdate()
    {
        foreach (Coroutine c in coroutines)
        {
            if (c != null)
                c.Dispose();
        }

        coroutines.Clear();
    }

    public virtual void OnGamePlayUpdate(float deltaTime)
    {
    }

    public virtual void OnEnable()
    {
        RegistCoroutine(new Coroutine(GamePlayUpdate()));
    }

    public virtual void OnDisable()
    {
        DisposeGamePlayUpdate();
    }

    public IEnumerator GamePlayUpdate()
    {
        while (true)
        {
            OnGamePlayUpdate(Coroutine.Delta);
            yield return null;
        }
    }
}
```

Usage

```csharp

public class MyObject : GameplayUpdateReceiverBase{
  
    public override void OnGamePlayUpdate(float deltaTime)
    {
        // Do something, just like using MonoBehaviour.Update()
        transform.Translate(Vector3.Up * deltaTime); // Remember use deltaTime to replace the Time.deltaTime
    }
}

```

## Highlight API

The most useful API in this library.

```csharp
// Call StartGame to start the game.
ApplicationController.Instance.StartGame();

// Get the GamePlayController and control the game state with below apis
GamePlayController gamePlayController = ApplicationController.Instance.GetGamePlayController();

gamePlayController.SuccessGamePlay();
gamePlayController.FailedGamePlay();
gamePlayController.QuitGamePlay();
gamePlayController.EnterPause();
gamePlayController.ResumePause();


// Get the singleton instance managed by ApplicationController
// (Not recommend, use Injection instead)

// Get ScripatableObjectLifeCycle instance
ApplicationController.Instance.GetScriptableLifeCycle(Type type);
ApplicationController.Instance.GetScriptableLifeCycle<T>();

// Get GetMonobehaviourLifeCycle instance
ApplicationController.Instance.GetMonobehaviourLifeCycle(Type type);
ApplicationController.Instance.GetMonobehaviourLifeCycle<T>();

// Get GetRegisterInstance instance
ApplicationController.Instance.GetRegisterInstance(Type type);
```

## Injection

Game System contains a lightweight dependency inject system, help you to resolve the reference problem in the game.

Use the `[ResloveTarget]` attribute to mark a class to become a injectable object.

Each MonoBehaviourLifeCycle and ScriptableObjectLifeCycle in the scene or referenced on ApplicationController will also injectable. (MonoBehaviourLifeCycle and ScriptableObjectLifeCycle doesn't require `[ResloveTarget]` attribute)

And use `[Inject]` attribute on the target field/property. Finally use ResolveInjection to complete injection.

All MonoBehaviourLifeCycle, ScriptableObjectLifeCycle and GamePlayData will complete the injtection automatically on each instance during Init phase.

Note: Currentlly, MonoBehaviourLifeCycle, ScriptableObjectLifeCycle and the class which has `[ResloveTarget]` attribute will only have one instance and managed by ApplicationController.

To get the class instance manually see [Document](https://macacagames.github.io/GameSystem/api/MacacaGames.GameSystem.ApplicationController.html#MacacaGames_GameSystem_ApplicationController_GetMonobehaviourLifeCycle_System_Type_) for more detail.

### Example:

```csharp
[ResloveTarget]
public class MySystem { }

public class MyMonoBehaviourLifeCycle : MonoBehaviourLifeCycle { }

public class MyScriptableObjectLifeCycle : ScriptableObjectLifeCycle { }

public MyMonoBehaviour : MonoBehaviour{
    [Inject]
    MySystem mySystem;

    [Inject]
    MyMonoBehaviourLifeCycle myMonoBehaviourLifeCycle;

    [Inject]
    MyScriptableObjectLifeCycle myScriptableObjectLifeCycle;

    //Call ApplicationController.Instance.ResolveInjection(this); to inject all [Inject] member
    void Awake(){
        ApplicationController.Instance.ResolveInjection(this);
    }
}

public MyOtherClass{
    [Inject]
    MySystem mySystem;

    [Inject]
    MyMonoBehaviourLifeCycle myMonoBehaviourLifeCycle;

    [Inject]
    MyScriptableObjectLifeCycle myScriptableObjectLifeCycle;

    //On a normal class you can call ApplicationController.Instance.ResolveInjection(this); in the constructor
    public MyOtherClass(){
        ApplicationController.Instance.ResolveInjection(this);
    }
}
```

### ApplicationAutoInjecter

On a GameObject you can attach `ApplicationAutoInjecter` component which will complete the injtection automatically on all component on the GameObject when the GameObject is Instantiated.

### Which class can be injected? Do I need to resolve inject manually?

|                                | Can be inject target?                  | Inject member with `[Inject]` | Resolve                                                                |
| ------------------------------ | -------------------------------------- | ----------------------------- | ---------------------------------------------------------------------- |
| MonoBehaviourLifeCycle         | Yes <br>(Require in Scene in begining) | Yes                           | Auto                                                                   |
| ScriptableObjectLifeCycle      | Yes                                    | Yes                           | Auto                                                                   |
| Classes with `[ResloveTarget]` | Yes                                    | Yes                           | Auto                                                                   |
| Classes                        | No                                     | Yes                           | Manual                                                                 |
| MonoBehaviour                  | No                                     | Yes                           | Manual <br>(Or automatically with `ApplicationAutoInjecter` component) |
| ScriptableObject               | No                                     | Yes                           | Manual                                                                 |
