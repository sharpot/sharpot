using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Creature : Thing
    {
        
        private ushort health = 100;
        private ushort maxHealth = 100;

        public Creature()
        {
            Mana = 100;
            MaxMana = 100;
            Outfit = new Outfit(128, 0);
            Direction = Direction.North;
            LightLevel = 0;
            LightColor = 0;
            Skull = Skull.None;
            Party = Party.None;
            Speed = 200;
        }

        protected override ushort GetId()
        {
            return 0x63;
        }

        public override string ToString()
        {
            return Name + " [" + Id + "]";
        }

        #region Properties
        public uint Id { get; set; }
        public string Name { get; set; }

        public ushort Health
        {
            get { return health; }
            set { health = value; if (Game != null) { Game.CreatureUpdateHealth(this); } }
        }

        public ushort MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; if (Game != null) { Game.CreatureUpdateHealth(this); } }
        }

        public ushort Mana { get; set; }
        public ushort MaxMana { get; set; }

        public Outfit Outfit { get; set; }
        public Direction Direction { get; set; }
        public byte LightLevel { get; set; }
        public byte LightColor { get; set; }
        public Skull Skull { get; set; }
        public Party Party { get; set; }
        public ushort Speed { get; set; }
        public Tile Tile { get; set; }
        public Game Game { get; set; }
        public bool IsPlayer
        {
            get { return Id > 0x40000000; }
        }
        #endregion

        #region Methods
        public byte GetHealthPercent()
        {
            byte Hp = Convert.ToByte(Math.Floor((double)(Health / MaxHealth) * 100));
            return Hp;
        }

        public void Say(string Text)
        {
            this.Game.CreatureSaySpeech(this, SpeechType.Say, Text);
        }

        public void Yell(string Text)
        {
            this.Game.CreatureYellSpeech(this, SpeechType.Say, Text);
        }

        public void Whisper(string Text)
        {
            this.Game.CreatureYellSpeech(this, SpeechType.Say, Text);
        }

        public void Step(Byte Dir)
        {
            this.Game.CreatureMove(this, (Direction)Dir);
        }

        public void Turn(Byte Dir)
        {
            this.Game.CreatureTurn(this, (Direction)Dir);
        }
        #endregion
    }
}