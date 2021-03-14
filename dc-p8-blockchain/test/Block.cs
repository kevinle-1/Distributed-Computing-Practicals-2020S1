// Filename: JobModel.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Object representation of a Block 
// Author:   Kevin Le (19472960)
//
// Date:     25/05/2020

namespace test
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

        /// <summary>
        /// String that concatenates all fields of the block except the hash of the block.
        /// This is used by functions that generate the hash of the block to store in the block. 
        /// </summary>
        /// <returns>String with all fields concatenated</returns>
        public string ToHashString()
        {
            return blockID.ToString() + walletIDfrom.ToString() + walletIDto.ToString() + amount.ToString() + offset.ToString() + prevHash.ToString();
        }
    }
}