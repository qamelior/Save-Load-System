# Save-Load System

A DI-based backend framework for your Save-Load system, featuring:
- game session manager that groups save files together into one session (folder) for easier access
- topological hierarchy between save entities allowing to declare entities that should be "loaded before me"
- game version validation, allowing to declare earlier supported game version for loading
- divided save data into 2 files (for faster loading of save file list):
  - .savinfo (for storing key information about save file; displayed in preview)
  - .sav (for storing rest of game data, only accessed during loading process)
- screenshot maker, to capture picture during saving process from game camera for easier save file identification

Dependencies to other plugins:
- Zenject (https://github.com/modesttree/Zenject.git)
- UniTask ([https://github.com/neuecc/UniRx.git](https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask))
- NaughtyAttributes (https://github.com/dbrizov/NaughtyAttributes.git)
