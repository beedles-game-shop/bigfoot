# Bigfoot the Game

### Build Settings
Scene Order
1. StartMenu
2. Demo



### Setting up Game Event System/UI
Add Following Script components to "EventSystem" object:
- /Assets/Scripts/EventManager/EventManager.cs
- /Assets/Scripts/EventManager/EventLoaders/UIEventLoader.cs

Public variables to set for UIEventLoader:
- Inventory Block: "Inventory"
- Collect Panel: "CollectPanel"
- Victory Message: "VictoryText"
- Items to Collect: Set the number of items, then drag the objects to the list.