

Add Achievements
1. In your Main Menu scene, select the AchievementManager GameObject.
2. Right‑click on it → Create Empty → name it (e.g., Kill10).
3. Add the Achievement script to that child.
4. Fill in the Inspector:
    Achievement ID – unique name (e.g., "Kill10")
    Title – what players see
    Description – details
    Target Progress – number needed (e.g., 10)
    Icon – (optional) a sprite
    isHidden – (optional) check to hide until unlocked

> Repeat for each achievement.



 2. Trigger Achievements (InGame)
 Attach an InvokerTrigger or InvokerCollision to your enemy/coin/zone.
 In its UnityEvent:
   Click +
   Drag the AchievementHelper GameObject (in your scene) into the object field.
   Select AchievementHelper.AddProgress(string).
   Type: "Kill10,1"  (ID, amount)

> That's it – every time the event fires, it adds progress.



 3. See Them in the Menu
 The AchievementListUI on your ScrollView's Content will automatically show all achievements with their progress.
 Unlocks are saved automatically.



 4. Popup Notifications
 When an achievement unlocks, a popup will appear using your AchievementPopup prefab.

