public class SpellModify : Spell
{

	//[Header("Unique Spell Values")]//This header should be a standard for every spell!

	public override void Cast()
	{
		DarkwindMenu.globalInstance.ActivateDarkwindMenu();
	}
}
