using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBranch : Spell
{

	//By having it need some mana instead of instant, it promotes playing a bit desynced to kill enemies!
	public override void Cast()
	{
		//If is desynchronized
		if (NetworkDamageShare.globalInstance.IsSynchronized())
			NetworkDamageShare.globalInstance.DesynchronizeFully();
	}
}
