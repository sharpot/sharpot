using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Creature : Thing
    {
        public uint Id;
        public string Name;
        
        private ushort _Health = 100;
        private ushort _MaxHealth = 100;
        public ushort Mana = 100;
        public ushort MaxMana = 100;
        
        public Outfit Outfit = new Outfit(128, 0);
        public Direction Direction = Direction.North;
        public byte LightLevel = 0;
        public byte LightColor = 0;
        public Skull Skull = Skull.None;
        public Party Party = Party.None;
        public ushort Speed = 200;
        public Tile Tile;
        public Game game;

        protected override ushort GetId()
        {
            return 0x63;
        }

        public override string ToString()
        {
            return Name + " [" + Id + "]";
        }

#region "Properties"
        public ushort Health
        {
            get { return _Health; }
            set { _Health = value; game.CreatureUpdateHealth(this); }
        }

        public ushort MaxHealth
        {
            get { return _MaxHealth; }
            set { _MaxHealth = value; game.CreatureUpdateHealth(this); }
        }
#endregion

#region "Methods"
        public bool IsPlayer()
        {
            return Id > 0x40000000;
        }

        public byte GetHealthPercent()
        {
            byte Hp = (byte)(((double)Health / (double)MaxHealth) * 100);
            return Hp;
        }

        public void Say(string Text)
        {
            this.game.CreatureSaySpeech(this, SpeechType.Say, Text);
        }

        public void Yell(string Text)
        {
            this.game.CreatureYellSpeech(this, SpeechType.Say, Text);
        }

        public void Whisper(string Text)
        {
            this.game.CreatureYellSpeech(this, SpeechType.Say, Text);
        }

        public void Step(Byte Dir)
        {
            this.game.CreatureMove(this, (Direction)Dir);
        }

        public void Turn(Byte Dir)
        {
            this.game.CreatureTurn(this, (Direction)Dir);
        }
#endregion
    }
}