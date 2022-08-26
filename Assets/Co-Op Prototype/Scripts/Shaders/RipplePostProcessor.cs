
//This is by: http://blog.onebyonedesign.com/unity/unity-ripple-or-shock-wave-effect/ found via https://www.youtube.com/watch?v=UsGuN69g2NI
/**
 *    Copyright (c) 2017 Devon O. Wolfgang
 *
 *    Permission is hereby granted, free of charge, to any person obtaining a copy
 *    of this software and associated documentation files (the "Software"), to deal
 *    in the Software without restriction, including without limitation the rights
 *    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *    copies of the Software, and to permit persons to whom the Software is
 *    furnished to do so, subject to the following conditions:
 *
 *    The above copyright notice and this permission notice shall be included in
 *    all copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *    THE SOFTWARE.
 */

using UnityEngine;

public class RipplePostProcessor : MonoBehaviour
{

    public Material RippleMaterial;
    public float MaxAmount = 25f;

    [Range(0, 1)]
    public float Friction = .9f;

    private float Amount = 0f;

    void Update()
    {
        ///if (Input.GetMouseButton(0))
            ///RippleEffect();

        //Tfw i have no idea on shaders, but does it really need to write the values every frame? Perhaps a flag would help performance? also passing a string as a parameter to access a variable? fml, i am trash at shaders, 100% noob :(
        this.RippleMaterial.SetFloat("_Amount", this.Amount);
        this.Amount *= this.Friction;
    }

    //... This is affected by screenspace, so the position should be within screenspace.... There should be a check if position given, is not within screenspace, and do a conversion inside here!
    public void RippleEffect(Vector3 positionToRipple)
    {
        this.Amount = this.MaxAmount;
        this.RippleMaterial.SetFloat("_CenterX", positionToRipple.x);
        this.RippleMaterial.SetFloat("_CenterY", positionToRipple.y);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, this.RippleMaterial);
    }

    //Invoked by classes
    public void SetValues(float targetAmount, float targetFriction)
    {
        MaxAmount = targetAmount;
        Friction = targetFriction;
    }

}
