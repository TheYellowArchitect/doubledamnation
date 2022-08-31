# Double Damnation

[Exclusively Multiplayer Co-Op Metroidvania](https://www.youtube.com/watch?v=LHl4EOaHqR4), with the [butter-smooth movement of Smash Bros Melee](https://www.youtube.com/watch?v=JpOaQxrsaqI&list=PL6ko1irv8SJ_n0phlLOCi8hrbJXRDS6ul&index=1).
	
Available on [Steam](https://store.steampowered.com/app/1015190/Double_Damnation/) and [Github](https://github.com/TheYellowArchitect/doubledamnation/releases)

## Building from Source
1. **Download&Install** Unity 2018.1.1f1 (Windows) [[1]](https://download.unity3d.com/download_unity/b8cbb5de9840/Windows64EditorInstaller/UnitySetup64-2018.1.1f1.exe) [[2]](https://unity3d.com/get-unity/download?thank-you=update&download_nid=51155&os=Win)

2. **Open** Unity 2018.1.1f1 and open this repository's code
<!---
3. Open [`SteamManager.cs`](https://github.com/TheYellowArchitect/doubledamnation/blob/master/Assets/Scripts/Steamworks.NET/SteamManager.cs#L10) and remove the '!' to build for non-Steam (aka local build)
-->
3. **Build**

## Contributing
I welcome all pull requests that fix bugs, and refactor ugly parts of the code.
		
If you are new to the project, I suggest you avoid the below classes, because they are by far the most horrible and unreadable in the entire codebase. Even I who wrote them, am having difficulties with them, because they are bloated. That's what happens when you [write big systems without even knowing the basics](https://theyellowarchitect.com/blog/the-art-of-game-development-the-modern-polymath#title).
		
- [`EnemyBehaviour`](https://github.com/TheYellowArchitect/doubledamnation/blob/master/Assets/Co-Op%20Prototype/Scripts/Enemy%20AI/EnemyBehaviour.cs)
- [`WarriorMovement`](https://github.com/TheYellowArchitect/doubledamnation/blob/master/Assets/Co-Op%20Prototype/Scripts/Player/WarriorMovement.cs)
- [`MageBehaviour`](https://github.com/TheYellowArchitect/doubledamnation/blob/master/Assets/Co-Op%20Prototype/Scripts/Player/MageBehaviour.cs)
- [`MultipleTargetCamera`](https://github.com/TheYellowArchitect/doubledamnation/blob/master/Assets/Co-Op%20Prototype/Scripts/Camera/MultipleTargetCamera.cs)

## [Donations](https://theyellowarchitect.com/donate#title)

## Code-License
The code I wrote is licensed under [GNU Public License Version 3](https://lukesmith.xyz/articles/why-i-use-the-gpl-and-not-cuck-licenses/), and is located under the folders [`Assets/Co-Op Prototype/Scripts`](https://github.com/TheYellowArchitect/doubledamnation/tree/master/Assets/Co-Op%20Prototype/Scripts) and [`Assets/Editor/Tools`](https://github.com/TheYellowArchitect/doubledamnation/tree/master/Assets/Editor/Tools) and is always of filetype `.cs`<br>
There may be some code I did not write in the abovementioned path locations, but be ensured that whichever code I did not write myself, the proper author is credited (usually at the first lines)

## Asset-License
The tl;dr is that you can use most assets without permission **as long you don't monetize off them in ANY way.**
So for example, using them for a prototype is ok as long you dont profit off it.
But if you use the sprites in a game where you sell microtransactions, it's illegal.
If you want to be certain, read below.

- The sprites and animations to Player1's character (Fool/Fisherman), were made by [Caitlin G Cooke](https://caitlingcooke.art/) and [me](theyellowarchitect.com/), under the license of [Creative Commons BY-SA v4.0](https://creativecommons.org/licenses/by/4.0/)
- The sprites and animations of all monsters, were made by [Zhelisko](https://zheliskos.artstation.com/) and [me](theyellowarchitect.com), under the license of [Creative Commons BY-SA v4.0](https://creativecommons.org/licenses/by/4.0/)
- The sprites and animations of Player2's character (Fiend/Friend), were made by [Zhelisko](https://zheliskos.artstation.com/), under the license of [Creative Commons BY-SA v4.0](https://creativecommons.org/licenses/by/4.0/)
- The [music](https://www.youtube.com/playlist?list=PLLvViE4qZfoMaayjJk9PRE98np7SwVsNF) is composed and owned by [Kerry Joiner](https://www.youtube.com/watch?v=zEvANt6wRRU), if you wish to use it, ask him for permission.
- The IP of Double Damnation (e.g. Fool&Fiend/Fisherman&Friend, Darkwind) are owned by [The Yellow Architect](https://theyellowarchitect.com).
- Most of the sound effects originate from https://freesound.org
		
If what you seek isn't here, [contact me](https://theyellowarchitect.com/contact#title) , and I will reply. I do not wish any legal issues, since this game is free at its core, with no monetization planned out of anything from it, but I am willing to fight against illegitimate/unwarranted legal action.
