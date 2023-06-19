using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LAMA.Communicator
{
    /// <summary>
    /// Class representing a single node of a Huffman tree
    /// </summary>
    public class HuffmanNode
    {
        /// <summary>
        /// Symbol stored in the node
        /// </summary>
        public char Symbol { get; set; }
        /// <summary>
        /// Frequency of the symbol / weight of the node
        /// </summary>
        public int Frequency { get; set; }
        /// <summary>
        /// Right child of the node (if any)
        /// </summary>
        public HuffmanNode Right { get; set; }
        /// <summary>
        /// Left child of the node (if any)
        /// </summary>
        public HuffmanNode Left { get; set; }

        /// <summary>
        /// Find the path of a symbol of in the subtree
        /// </summary>
        /// <param name="symbol">symbol we are searching for</param>
        /// <param name="data">path to this node</param>
        /// <returns>path to the symbol (which is also its encoding) or null</returns>
        public List<bool> Traverse(char symbol, List<bool> data)
        {
            // Leaf
            if (Right == null && Left == null)
            {
                if (symbol.Equals(this.Symbol))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<bool> left = null;
                List<bool> right = null;

                if (Left != null)
                {
                    List<bool> leftPath = new List<bool>();
                    leftPath.AddRange(data);
                    leftPath.Add(false);

                    left = Left.Traverse(symbol, leftPath);
                }

                if (Right != null)
                {
                    List<bool> rightPath = new List<bool>();
                    rightPath.AddRange(data);
                    rightPath.Add(true);
                    right = Right.Traverse(symbol, rightPath);
                }

                if (left != null)
                {
                    return left;
                }
                else
                {
                    return right;
                }
            }
        }
    }
}
