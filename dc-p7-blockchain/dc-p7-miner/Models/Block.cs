namespace dc_p7_miner.Models
{
    public class Block
    {
        public uint blockID;
        public uint walletIDfrom;
        public uint walletIDto;
        public float amount;
        public uint offset;
        public string prevHash;
        public string hash;

        public string ToHashString()
        {
            return blockID.ToString() + walletIDfrom.ToString() + walletIDto.ToString() + amount.ToString() + offset.ToString() + prevHash.ToString();
        }
    }
}