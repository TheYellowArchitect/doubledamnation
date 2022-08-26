
//tfw not pause because of push feelsbadman
//kinda like help becoming aid :(
public class SpellStop : Spell
{

	//[Header("Unique Spell Values")]//This header should be a standard for every spell!

	public override void Cast()
	{
		PauseMenu.globalInstance.Pause();
	}
}
