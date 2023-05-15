using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    public class Compression
    {
        private HuffmanTree huffmanTree;
        public Compression()
        {
            Dictionary<char, int> frequencies = new Dictionary<char, int>();
            frequencies.Add(SpecialCharacters.messageSeparator, 500);
            frequencies.Add(SpecialCharacters.messagePartSeparator, 1000);
            frequencies.Add(SpecialCharacters.attributesSeparator, 1000);
            frequencies.Add(Helpers.separator, 1000);

            frequencies.Add(' ', 1000);
            frequencies.Add(',', 100);
            frequencies.Add('.', 100);
            frequencies.Add('-', 100);

            frequencies.Add('0', 500);
            frequencies.Add('1', 500);
            frequencies.Add('2', 500);
            frequencies.Add('3', 500);
            frequencies.Add('4', 500);
            frequencies.Add('5', 500);
            frequencies.Add('6', 500);
            frequencies.Add('7', 500);
            frequencies.Add('8', 500);
            frequencies.Add('9', 500);

            frequencies.Add('a', 216);
            frequencies.Add('á', 56);
            frequencies.Add('b', 65);
            frequencies.Add('c', 68);
            frequencies.Add('č', 24);
            frequencies.Add('d', 112);
            frequencies.Add('ď', 1);
            frequencies.Add('e', 252);
            frequencies.Add('é', 36);
            frequencies.Add('ě', 58);
            frequencies.Add('f', 7);
            frequencies.Add('g', 8);
            frequencies.Add('h', 70);
            frequencies.Add('i', 120);
            frequencies.Add('í', 76);
            frequencies.Add('j', 69);
            frequencies.Add('k', 107);
            frequencies.Add('l', 152);
            frequencies.Add('m', 98);
            frequencies.Add('n', 190);
            frequencies.Add('ň', 1);
            frequencies.Add('o', 238);
            frequencies.Add('ó', 1);
            frequencies.Add('p', 96);
            frequencies.Add('q', 1);
            frequencies.Add('r', 102);
            frequencies.Add('ř', 35);
            frequencies.Add('s', 135);
            frequencies.Add('š', 25);
            frequencies.Add('t', 172);
            frequencies.Add('ť', 1);
            frequencies.Add('u', 80);
            frequencies.Add('ú', 4);
            frequencies.Add('ů', 12);
            frequencies.Add('v', 119);
            frequencies.Add('w', 2);
            frequencies.Add('x', 3);
            frequencies.Add('y', 72);
            frequencies.Add('ý', 24);
            frequencies.Add('z', 55);
            frequencies.Add('ž', 37);

            frequencies.Add('A', 11);
            frequencies.Add('Á', 3);
            frequencies.Add('B', 6);
            frequencies.Add('C', 7);
            frequencies.Add('Č', 2);
            frequencies.Add('D', 11);
            frequencies.Add('Ď', 1);
            frequencies.Add('E', 12);
            frequencies.Add('É', 2);
            frequencies.Add('Ě', 3);
            frequencies.Add('F', 1);
            frequencies.Add('G', 1);
            frequencies.Add('H', 7);
            frequencies.Add('I', 6);
            frequencies.Add('Í', 3);
            frequencies.Add('J', 7);
            frequencies.Add('K', 10);
            frequencies.Add('L', 15);
            frequencies.Add('M', 10);
            frequencies.Add('N', 19);
            frequencies.Add('Ň', 1);
            frequencies.Add('O', 12);
            frequencies.Add('Ó', 1);
            frequencies.Add('P', 9);
            frequencies.Add('Q', 1);
            frequencies.Add('R', 10);
            frequencies.Add('Ř', 3);
            frequencies.Add('S', 13);
            frequencies.Add('Š', 2);
            frequencies.Add('T', 17);
            frequencies.Add('Ť', 1);
            frequencies.Add('U', 4);
            frequencies.Add('Ú', 1);
            frequencies.Add('Ů', 1);
            frequencies.Add('V', 12);
            frequencies.Add('W', 1);
            frequencies.Add('X', 1);
            frequencies.Add('Y', 3);
            frequencies.Add('Ý', 1);
            frequencies.Add('Z', 4);
            frequencies.Add('Ž', 4);

            //Special character marking following character encoded using UTF8
            frequencies.Add(HuffmanTree.specialCharacter, 100);

            huffmanTree = new HuffmanTree();
            huffmanTree.Build(frequencies);
        }

        public byte[] Encode(string source)
        {
            return huffmanTree.Encode(source);
        }

        public string Decode(byte[] bytes)
        {
            return huffmanTree.Decode(bytes);
        }

        public string DecodeFromAESBytes(byte[] bytes, out int offsetEnd)
        {
            return huffmanTree.DecodeFromAESBytes(bytes, out offsetEnd);
        }
    }
}
