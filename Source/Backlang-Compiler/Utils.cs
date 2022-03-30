﻿using System.Text;

namespace Backlang_Compiler;

public class Utils
{
    public string GenerateIdentifier()
    {
        var sb = new StringBuilder();
        const string ALPHABET = "abcdefhijklmnopqrstABCDEFGHIJKLMNOPQRSTUVWXYZ&%$";
        var random = new Random();

        for (int i = 0; i < random.Next(5, 9); i++)
        {
            sb.Append(ALPHABET[random.Next()]);
        }

        return sb.ToString();
    }
}