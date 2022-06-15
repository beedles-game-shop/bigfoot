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

### Sprite Mapping for Collecting or Grabable objects

The object name should get mapped to a sprite resources under the /Assets/Resources/Images/ folder. The map is defined in the UIEventLoader.cs(/Assets/Scripts/EventManager/EventLoaders/UIEventLoader.cs) file.
i.e.:
spriteMap = new Dictionary<string, Sprite>(){
    {"cube", Resources.Load<Sprite>("Images/box")},
    {"ball", Resources.Load<Sprite>("Images/ball")},
    {"chair", Resources.Load<Sprite>("Images/chair")}
};