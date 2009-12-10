using SharpOT;
using SharpOT.Scripting;
using System;

public class WaveSpell : ISpell
{
    public string GetWords()
    {
        return "exevo hur";
    }

    public bool CanBeUsedBy(Player player)
    {
        return true;
    }

    public bool Action(Game game, Player player, string args)
    {
        

        return false;
    }
}