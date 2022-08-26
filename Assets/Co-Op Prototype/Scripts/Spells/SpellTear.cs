using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The difference of this spell and branch
/// is that it consumes 4 mana and instakills everything around you
/// kinda like the link spell
/// </summary>
public class SpellTear : Spell
{

	//By having it need some mana instead of instant, it promotes playing a bit desynced to kill enemies!
	public override void Cast()
	{
		if (NetworkDamageShare.globalInstance.IsSynchronized())
			NetworkDamageShare.globalInstance.DesynchronizeFully();
	}
}
