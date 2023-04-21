using BruTile.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Xsl;

namespace LAMA.Communicator
{
    internal class HuffmanTree
    {
        public static char specialCharacter = 'ƽ';
        private List<HuffmanNode> nodes = new List<HuffmanNode>();
        private bool littleEndian = true;
        public HuffmanNode Root { get; set; }

        public void Build(Dictionary<char, int> frequencies)
        {
            //Check if bytes are little or big endian
            byte endianTest = (byte)1;
            byte[] endianTestBytes = new byte[1];
            endianTestBytes[0] = endianTest;
            BitArray bits = new BitArray(endianTestBytes);
            if (bits[0]) littleEndian = true;
            else littleEndian = false;


            foreach (KeyValuePair<char, int> symbol in frequencies)
            {
                nodes.Add(new HuffmanNode() { Symbol = symbol.Key, Frequency = symbol.Value });
            }
            while (nodes.Count > 1)
            {
                List<HuffmanNode> orderedNodes = nodes.OrderBy(node => node.Frequency).ToList<HuffmanNode>();

                if (orderedNodes.Count >= 2)
                {
                    // Take first two items
                    List<HuffmanNode> taken = orderedNodes.Take(2).ToList<HuffmanNode>();

                    // Create a parent node by combining the frequencies
                    HuffmanNode parent = new HuffmanNode()
                    {
                        Symbol = '*',
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }
                this.Root = nodes.FirstOrDefault();
            }
        }

        public byte[] Encode(string source)
        {
            List<bool> encodedSource = new List<bool>();
            for (int i = 0; i < source.Length; i++)
            {
                List<bool> encodedSymbol = this.Root.Traverse(source[i], new List<bool>());
                if (encodedSymbol == null)
                {
                    encodedSymbol = this.Root.Traverse(HuffmanTree.specialCharacter, new List<bool>());
                    byte[] symbol = Encoding.UTF8.GetBytes(source[i].ToString());
                    BitArray symbolUTF8 = new BitArray(symbol);
                    for (int j = 0; j < symbolUTF8.Length; j++)
                    {
                        encodedSymbol.Add(symbolUTF8[j]);
                    }
                }
                encodedSource.AddRange(encodedSymbol);
            }

            BitArray bits = new BitArray(encodedSource.ToArray());
            //Debug.WriteLine("[{0}]", string.Join(", ", bits));
            byte[] result = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(result, 0);
            return result;
        }

        public string Decode(byte[] bytes)
        {
            BitArray bits = new BitArray(bytes);
            HuffmanNode current = this.Root;
            string decoded = "";

            int i = 0;
            while (i < bits.Length)
            {
                bool bit = bits[i];
                if (bit)
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                    }
                }
                else
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                    }
                }

                if (IsLeaf(current))
                {
                    if (current.Symbol != HuffmanTree.specialCharacter)
                    {
                        decoded += current.Symbol;
                    }
                    else
                    {
                        int counter = 0;
                        if (littleEndian)
                        {
                            int position = i + 8;
                            while (bits[position] != false)
                            {
                                position--;
                                counter++;
                            }
                        }
                        else
                        {
                            int position = i + 1;
                            while (bits[position] != false)
                            {
                                position++;
                                counter++;
                            }
                        }
                        if (counter == 0) counter = 1;
                        byte[] utf8Bytes = new byte[counter];
                        BitArray utf8Bits = new BitArray(counter * 8);
                        for (int j = 0; j < utf8Bits.Length; j++)
                        {
                            utf8Bits[j] = bits[i + j + 1];
                        }
                        utf8Bits.CopyTo(utf8Bytes, 0);
                        char[] symbols = Encoding.UTF8.GetString(utf8Bytes).ToCharArray();
                        decoded += symbols[0];
                        i += counter * 8;
                    }
                    current = this.Root;
                }
                i++;
            }
            return decoded;
        }

        public string DecodeFromAESBytes(byte[] bytes, out int offset)
        {
            HuffmanNode current = this.Root;
            string decoded = "";
            BitArray carryoverBits = new BitArray(0);
            BitArray utf8Bits = new BitArray(8);

            int i = 0;
            while (i < bytes.Length / 16) {
                int j = 0;
                byte[] byteBlock = new byte[16];
                byteBlock[0] = bytes[16 * i];
                byteBlock[1] = bytes[16 * i + 1];
                byteBlock[2] = bytes[16 * i + 2];
                byteBlock[3] = bytes[16 * i + 3];
                byteBlock[4] = bytes[16 * i + 4];
                byteBlock[5] = bytes[16 * i + 5];
                byteBlock[6] = bytes[16 * i + 6];
                byteBlock[7] = bytes[16 * i + 7];
                byteBlock[8] = bytes[16 * i + 8];
                byteBlock[9] = bytes[16 * i + 9];
                byteBlock[10] = bytes[16 * i + 10];
                byteBlock[11] = bytes[16 * i + 11];
                byteBlock[12] = bytes[16 * i + 12];
                byteBlock[13] = bytes[16 * i + 13];
                byteBlock[14] = bytes[16 * i + 14];
                byteBlock[15] = bytes[16 * i + 15];
                BitArray bits = new BitArray(byteBlock);
                while (j < bits.Length)
                {
                    if (carryoverBits.Count > 0)
                    {
                        BitArray firstByte = new BitArray(8);
                        if (carryoverBits.Count >= 8)
                        {
                            for (int iter = 0; iter < 8; iter++)
                            {
                                firstByte[iter] = carryoverBits[iter];
                            }
                        }
                        else
                        {
                            for (int iter = 0; iter < carryoverBits.Count; iter++)
                            {
                                firstByte[iter] = carryoverBits[iter];
                            }
                            for (int iter = 0; iter < 8 - carryoverBits.Count; iter++)
                            {
                                firstByte[iter + carryoverBits.Count] = bits[iter];
                            }
                        }
                        int counter = getUTFCharacterByteLength(firstByte);
                        byte[] utf8Bytes = new byte[counter];
                        utf8Bits = new BitArray(counter * 8);
                        for (int k = 0; k < carryoverBits.Count; k++)
                        {
                            utf8Bits[k] = carryoverBits[k];
                        }
                        for (int k = 0; k < utf8Bits.Length - carryoverBits.Count; k++)
                        {
                            utf8Bits[k + carryoverBits.Count] = bits[k];
                        }
                        utf8Bits.CopyTo(utf8Bytes, 0);
                        char[] symbols = Encoding.UTF8.GetString(utf8Bytes).ToCharArray();
                        decoded += symbols[0];
                        j += (utf8Bits.Length - carryoverBits.Count);
                        current = this.Root;
                        carryoverBits = new BitArray(0);
                    }
                    bool bit = bits[j];
                    if (bit)
                    {
                        if (current.Right != null)
                        {
                            current = current.Right;
                        }
                    }
                    else
                    {
                        if (current.Left != null)
                        {
                            current = current.Left;
                        }
                    }

                    if (IsLeaf(current))
                    {
                        if (current.Symbol == Separators.messageSeparator)
                        {
                            decoded += current.Symbol;
                            //skip the rest of the block modify offset
                            offset = i + 1;
                            return decoded;
                        }
                        else if (current.Symbol != HuffmanTree.specialCharacter)
                        {
                            decoded += current.Symbol;
                        }
                        else
                        {
                            int counter;
                            if (j + 8 < bits.Count)
                            {
                                BitArray firstByte = new BitArray(8);
                                for (int iter = 1; iter < 9; iter++)
                                {
                                    firstByte[iter-1] = bits[j + iter];
                                }
                                counter = getUTFCharacterByteLength(firstByte);
                                if (counter * 8 + j >= bits.Length)
                                {
                                    carryoverBits = new BitArray(bits.Length - (j + 1));
                                    for (int iter = j + 1; iter < bits.Length; iter++)
                                    {
                                        carryoverBits[iter - (j + 1)] = bits[iter];
                                    }
                                    break;
                                }
                                byte[] utf8Bytes = new byte[counter];
                                utf8Bits = new BitArray(counter * 8);
                                for (int k = 0; k < utf8Bits.Length; k++)
                                {
                                    utf8Bits[k] = bits[j + k + 1];
                                }
                                utf8Bits.CopyTo(utf8Bytes, 0);
                                char[] symbols = Encoding.UTF8.GetString(utf8Bytes).ToCharArray();
                                decoded += symbols[0];
                                j += counter * 8;
                            }
                            else
                            {
                                carryoverBits = new BitArray(bits.Length - (j + 1));
                                for (int iter = j + 1; iter < bits.Length; iter++)
                                {
                                    carryoverBits[iter - (j + 1)] = bits[iter];
                                }
                                break;
                            }
                        }
                        current = this.Root;
                    }
                    j++;
                }
                i++;
            }
            offset = i;
            return decoded;
        }

        private int getUTFCharacterByteLength(BitArray bits)
        {
            int counter = 0;
            if (littleEndian)
            {
                int position = 7;
                while (bits[position] != false)
                {
                    position--;
                    counter++;
                }
            }
            else
            {
                int position = 0;
                while (bits[position] != false)
                {
                    position++;
                    counter++;
                }
            }
            if (counter == 0) counter = 1;
            return counter;
        }

        public bool IsLeaf(HuffmanNode node)
        {
            return (node.Left == null && node.Right == null);
        }
    }
}
