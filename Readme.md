See [Document](https://macacagames.github.io/GameSystem/) for more detail.

# Welcome to Macaca GameSystem

Macaca GameSystem is a framework to build game life cycle in Unity3D.

### Note: start from 1.0.3, this package include a precompiled Rayark.Mask  Dlls.

-----

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
        "com.macacagames.gamesystem": "https://github.com/MacacaGames/GameSystem.git",
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

IEnumerator OnEnterGame();

IEnumerator GamePlay();

void OnLeaveGame();

void OnGameSuccess();

void OnGameLose();

IEnumerator GameResult();

void OnGameEnd();

void OnGameFaild();

IEnumerator OnContinueFlow(IReturn<bool> result);

bool IsContinueAvailable { get; }

void OnContinue();

void OnGUI();
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

Use the ``[ResloveTarget]`` attribute to mark a class to become a injectable object.

Each MonoBehaviourLifeCycle and ScriptableObjectLifeCycle in the scene or referenced on ApplicationController will also injectable. (MonoBehaviourLifeCycle and ScriptableObjectLifeCycle doesn't require ``[ResloveTarget]`` attribute)

And use ``[Inject]`` attribute on the target field/property. Finally use ResolveInjection to complete injection.

All MonoBehaviourLifeCycle, ScriptableObjectLifeCycle and GamePlayData will complete the injtection automatically on each instance during Init phase.

Note: Currentlly, MonoBehaviourLifeCycle, ScriptableObjectLifeCycle and the class which has ``[ResloveTarget]`` attribute will only have one instance and managed by ApplicationController.


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
On a GameObject you can attach ``ApplicationAutoInjecter`` component which will complete the injtection automatically on all component on the GameObject when the GameObject is Instantiated.


### Which class can be injected? Do I need to resolve inject manually?
|                                  | Can be inject target?                  | Inject member with ``[Inject]`` | Resolve                                                                  |
| -------------------------------- | -------------------------------------- | ------------------------------- | ------------------------------------------------------------------------ |
| MonoBehaviourLifeCycle           | Yes <br>(Require in Scene in begining) | Yes                             | Auto                                                                     |
| ScriptableObjectLifeCycle        | Yes                                    | Yes                             | Auto                                                                     |
| Classes with ``[ResloveTarget]`` | Yes                                    | Yes                             | Auto                                                                     |
| Classes                          | No                                     | Yes                             | Manual                                                                   |
| MonoBehaviour                    | No                                     | Yes                             | Manual <br>(Or automatically with ``ApplicationAutoInjecter`` component) |
| ScriptableObject                 | No                                     | Yes                             | Manual                                                                   |


