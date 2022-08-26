using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All of this code could look far better don't you think?//Perhaps by simply putting all particles on an empty particle system, then stopping that emission, which will indirectly stop all.
public class StopParticleEmission : MonoBehaviour
{
    private ParticleSystem commonParticleSystem;

    private void Start()
    {
        commonParticleSystem = GetComponent<ParticleSystem>();
    }

    public void Initialize()
    {
        if (commonParticleSystem != null)
        {
            //Stop Emission, but keep the living ones to expire
            commonParticleSystem.enableEmission = false;//Deprecated but the 2 other ways VS suggests me don't work. One is outright wrong, and the other gives me a get/read instead of set/write. fml.

            //If there are particle systems below this, kill dem.
            if (transform.childCount > 0)
                //Stop emitting more particles
                if (transform.GetChild(0).GetComponent<StopParticleEmission>() != null)
                    transform.GetChild(0).GetComponent<StopParticleEmission>().Initialize();
        }
    }
}
