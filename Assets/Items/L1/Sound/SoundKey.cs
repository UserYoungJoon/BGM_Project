namespace YoungJoon.L1.Sound
{
    // SFX SoundKey. SoundManager의 SoundAsset[] Key와 매칭.
    public static class SoundKey
    {
        public const string CardRespawn = "CardRespawn";   // card-place-1
        public const string CardPlace = "CardPlace";       // card-slide-4
        public const string RewardAppear = "RewardAppear"; // cards-pack-take-out-2
        public const string Die = "Die";                   // hit02
        public const string Hit = "Hit";                   // hit12
        public const string Counter = "Counter";           // hit18
        public const string Click = "Click";               // click
        public const string TurnChanged = "TurnChanged";   // onTurnChanged
        public const string Win = "Win";                   // winSound
        public const string Lose = "Lose";                 // loseSound
        public const string Block = "Block";               // block (수호 방어도 부여)
    }
}
