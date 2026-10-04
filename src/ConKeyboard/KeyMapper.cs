using System.Collections.Generic;

namespace ConKeyboard;

public static class KeyMapper
{
    // Your custom glyphs live in the font at U+E000 ('a') ... U+E019 ('z')
    private const char PuaBase = '\uE000';

    private const char Kochi = '\u0308';        // combining ¨ (Kochi / Ad-ochi)
    private const char QuestionStart = '\u00BF'; // ¿

    // YOUR 7 VOWELS: H A M S D N R -> Kochi can NEVER sit on these!
    private static readonly HashSet<char> Vowels = new() { 'a', 'd', 'h', 'm', 'n', 'r', 's' };

    public static string Map(int vk, bool shift)
    {
        // A-Z keys (VK 0x41 - 0x5A)
        if (vk >= 0x41 && vk <= 0x5A)
        {
            char letter = (char)('a' + (vk - 0x41));
            string glyph = ((char)(PuaBase + (letter - 'a'))).ToString();

            // TAB+SHIFT+consonant => add Kochi (pronounced "add")
            // TAB+SHIFT+vowel => BLOCKED by your grammar rule (plain glyph)
            if (shift && !Vowels.Contains(letter))
                glyph += Kochi;

            return glyph;
        }

        // TAB + /  or  TAB + `  =>  ¿  (start of question sentences)
        if (vk == 0xBF || vk == 0xC0)
            return QuestionStart.ToString();

        return null;
    }
}
