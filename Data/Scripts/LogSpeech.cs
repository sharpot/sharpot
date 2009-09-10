using SharpOT;
using SharpOT.Scripting;

public class LogSpeech : IScript
{
    Game game;
    public bool Start(Game game)
    {
        this.game = game;
        game.BeforeCreatureSpeech += BeforeCreatureSpeech;
        return true;
    }

    public bool BeforeCreatureSpeech(Creature creature, Speech speech)
    {
        Server.Log(speech.Message);
        return false;
    }

    public bool Stop()
    {
        game.BeforeCreatureSpeech -= BeforeCreatureSpeech;
        return true;
    }
}