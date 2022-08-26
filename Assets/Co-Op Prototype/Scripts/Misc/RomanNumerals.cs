using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Credits to https://shadowplaycoding.com/2017/03/23/romanString-numeral-generator-part-1/ SP_Coding for this class. While there is also this:https://stackoverflow.com/questions/7040289/converting-integers-to-romanString-numerals to copy-pasta the solution with 0 effort
 * and generally its easy to make romanString numbering, using the above and giving the link should help everyone understand why this works :) Admittedly, it could be far far cleaner code, but its not only educational for you, but for me2!
 * 
*/
public class romanStringNumerals
{
    public static string NumeralsOneToTen(int unconvertedIntNumber)
    {
        string romanString;

        switch(unconvertedIntNumber)
        {
            case 1:
                romanString = "I";
                break;
            case 2:
                romanString = "II";
                break;
            case 3:
                romanString = "III";
                break;
            case 4:
                romanString = "IV";
                break;
            case 5:
                romanString = "V";
                break;
            case 6:
                romanString = "VI";
                break;
            case 7:
                romanString = "VII";
                break;
            case 8:
                romanString = "VIII";
                break;
            case 9:
                romanString = "IX";
                break;
            case 10:
                romanString = "X";
                break;
            default:
                romanString = unconvertedIntNumber.ToString();//for now?
                break;
        }

        return romanString;
    }

    public static string romanStringNumeralGenerator(int unconvertedIntNumber)
    {
        string romanString = "";//starts empty

        //The below is an string builder(unofficial, since there is also a class/type of StringBuilder). It is very useful btw, you will definitely encounter it again, aside of this ez example.

        if (unconvertedIntNumber == 0)
            return "o";//the closest thing to Omega.

        while (unconvertedIntNumber > 0)
        {
            //tfw u don't use += and -= for max readability
            if (unconvertedIntNumber >= 1000)
            {
                romanString = romanString + "M";
                unconvertedIntNumber = unconvertedIntNumber - 1000;
            }
            else if (unconvertedIntNumber >= 900)
            {
                romanString = romanString + "CM";
                unconvertedIntNumber = unconvertedIntNumber - 900;
            }
            else if (unconvertedIntNumber >= 500)
            {
                romanString = romanString + "D";
                unconvertedIntNumber = unconvertedIntNumber - 500;
            }
            else if (unconvertedIntNumber >= 400)
            {
                romanString = romanString + "CD";
                unconvertedIntNumber = unconvertedIntNumber - 400;
            }
            else if (unconvertedIntNumber >= 100)
            {
                romanString = romanString + "C";
                unconvertedIntNumber = unconvertedIntNumber - 100;
            }
            else if (unconvertedIntNumber >= 90)
            {
                romanString = romanString + "XC";
                unconvertedIntNumber = unconvertedIntNumber - 90;
            }
            else if (unconvertedIntNumber >= 50)
            {
                romanString = romanString + "L";
                unconvertedIntNumber = unconvertedIntNumber - 50;
            }
            else if (unconvertedIntNumber >= 40)
            {
                romanString = romanString + "XL";
                unconvertedIntNumber = unconvertedIntNumber - 40;
            }
            else if (unconvertedIntNumber > 10)
            {
                romanString = romanString + "X";
                unconvertedIntNumber = unconvertedIntNumber - 10;
            }
            else if (unconvertedIntNumber <= 10)
            {
                romanString = romanString + NumeralsOneToTen(unconvertedIntNumber);
                unconvertedIntNumber = 0;
            }
                
        }
        

        return romanString;
    }


    //=======Reversed========

    public static int NumeralsOneToTwentyFive(string romanString)
    {
        int unconvertedIntNumber = -1;

        if (romanString == "O" || romanString == "o")
            unconvertedIntNumber = 0;
        else if (romanString == "I" || romanString == "i")
            unconvertedIntNumber = 1;
        else if (romanString == "II" || romanString == "ii")
            unconvertedIntNumber = 2;
        else if (romanString == "III" || romanString == "iii")
            unconvertedIntNumber = 3;
       else if (romanString == "IV" || romanString == "iv")
            unconvertedIntNumber = 4;
       else if (romanString == "V" || romanString == "v")
            unconvertedIntNumber = 5;
       else if (romanString == "VI" || romanString == "vi")
            unconvertedIntNumber = 6;
        else if (romanString == "VII" || romanString == "vii")
            unconvertedIntNumber = 7;
        else if (romanString == "VIII" || romanString == "viii")
            unconvertedIntNumber = 8;
        else if (romanString == "IX" || romanString == "ix")
            unconvertedIntNumber = 9;
        else if (romanString == "X" || romanString == "x")
            unconvertedIntNumber = 10;
        else if (romanString == "XI" || romanString == "xi")
            unconvertedIntNumber = 11;
        else if (romanString == "XII" || romanString == "xii")
            unconvertedIntNumber = 12;
        else if (romanString == "XIII" || romanString == "xiii")
            unconvertedIntNumber = 13;
        else if (romanString == "XIV" || romanString == "xiv")
            unconvertedIntNumber = 14;
        else if (romanString == "XV" || romanString == "xv")
            unconvertedIntNumber = 15;
        else if (romanString == "XVI" || romanString == "xvi")
            unconvertedIntNumber = 16;
        else if (romanString == "XVII" || romanString == "xvii")
            unconvertedIntNumber = 17;
        else if (romanString == "XVIII" || romanString == "xviii")
            unconvertedIntNumber = 18;
        else if (romanString == "XIX" || romanString == "xix")
            unconvertedIntNumber = 19;
        else if (romanString == "XX" || romanString == "xx")
            unconvertedIntNumber = 20;
        else if (romanString == "XXI" || romanString == "xxi")
            unconvertedIntNumber = 21;
        else if (romanString == "XXII" || romanString == "xxii")
            unconvertedIntNumber = 22;
        else if (romanString == "XXIII" || romanString == "xxiii")
            unconvertedIntNumber = 23;
        else if (romanString == "XXIV" || romanString == "xxiv")
            unconvertedIntNumber = 24;
        else if (romanString == "XXV" || romanString == "xxv")
            unconvertedIntNumber = 25;

        return unconvertedIntNumber;
    }

    //converting the other would take some effort, I would have to think! Oh the horror!
    //gonna do it manually up to 24, instead of 10, then roman processing

}
