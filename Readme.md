See [Document](https://macacagames.github.io/GameSystem/) for more detail.

# Welcome to Macaca GameSystem

Macaca GameSystem is a framework to build game life cycle in Unity3D.

## Features
- Life cycle management and callback.
- Replaceable gamelogic implement via Unity's ScriptableObject.
- Contains a lightweight dependency injection system.

## Installation

### Option 1: Unity Package manager
Add it to your editor's `manifest.json` file like this:
```json
{
    "dependencies": {
        "com.macacagames.gamesystem": "https://github.com/MacacaGames/GameSystem.git",
        "com.rayark.mast": "https://github.com/MacacaGames/mast.git",
    }
}
```

### Option 2: Git SubModule
Note: GameSystem is dependency with Rayark.Mast, so also add it in git submodule.

```bash
git submodule add https://github.com/MacacaGames/GameSystem.git Assets/MacacaGameSystem

git submodule add https://github.com/MacacaGames/mast.git Assets/Mast
```

## GamePlayData
GamePlayData is the main game logic implemention. Do your logic in the callbacks to complete your game. For each callback's detail please see [Document](https://macacagames.github.io/GameSystem/api/MacacaGames.GameSystem.GamePlayData.html) 

```csharp
public abstract IEnumerator GamePlay();

public abstract IEnumerator GameResult();

public abstract void OnGameValueReset();

public abstract IEnumerator OnEnterGame();

public abstract void OnLeaveGame();

public abstract void OnGameEnd();

public abstract void OnGameSuccess();

public abstract void OnGameLose();

public abstract void OnGameFaild();

public abstract IEnumerator OnContinueFlow(IReturn<bool> result);

public abstract bool IsContinueAvailable { get; }

public abstract void OnContinue();

public abstract void Init();

public abstract void OnApplicationBeforeGamePlay();

public abstract void OnGUI();
```

## Injection
Game System contains a lightweight dependency inject system, help you to resolve the reference problem in the game.

Use the ``[ResloveTarget]`` attribute to mark a class to become a injectable object.

Each MonoBehaviourLifeCycle and ScriptableObjectLifeCycle in the scene or referenced on ApplicationController will also injectable.

And use ```[Inject]`` attribute on the target field/property. Finally use ResolveInjection to complete injection.

All MonoBehaviourLifeCycle, ScriptableObjectLifeCycle and GamePlayData will complete the injtection automatically on each instance during Init phase.

Example 
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
On a GameObject you can attach ``ApplicationAutoInjecter`` component which will complete the injtection automatically on all component on the GameObject when the GameObject is created.
