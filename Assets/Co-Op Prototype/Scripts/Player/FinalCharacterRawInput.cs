
//tfw you realize the below could be represented as a Vector3 (with just 2 bytes of difference)
[System.Serializable]
public struct FinalCharacterRawInput
{
	public float movementX;
	public float movementY;
	public short attackDirection;
}
