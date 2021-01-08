See [Document](https://macacagames.github.io/GameSystem/) for more detail.

# Welcome to Macaca GameSystem

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
