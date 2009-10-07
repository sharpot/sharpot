using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpOT.OpenTibia
{
    public class FileLoader
    {
        byte[] buffer = null;
        FileStream fileStream;
        BinaryReader reader;
        Node root = null;

        public Node GetRootNode()
        {
            return root;
        }

        public bool OpenFile(string fileName)
        {
            fileStream = File.Open(fileName, FileMode.Open);
            reader = new BinaryReader(fileStream);

            uint version = reader.ReadUInt32();

            root = new Node();
            root.Start = 4;

            if (reader.ReadByte() == Constants.NodeStart)
            {
                return ParseNode(root);
            }
            else
            {
                return false;
            }
        }

        private bool ParseNode(Node node)
        {
            Node currentNode = node;
            int val;
            while (true)
            {
                // read node type
                val = fileStream.ReadByte();
                if (val != -1)
                {
                    currentNode.Type = val;
                    bool setPropSize = false;
                    while (true)
                    {
                        // search child and next node
                        val = fileStream.ReadByte();
                        if (val == -1)
                        {
                            break;
                        }
                        else if (val == Constants.NodeStart)
                        {
                            Node childNode = new Node();
                            childNode.Start = fileStream.Position;
                            setPropSize = true;
                            currentNode.PropsSize = fileStream.Position - currentNode.Start - 2;
                            currentNode.Child = childNode;
                            if (!ParseNode(childNode))
                            {
                                return false;
                            }
                        }
                        else if (val == Constants.NodeEnd)
                        {
                            if (!setPropSize)
                            {
                                currentNode.PropsSize = fileStream.Position - currentNode.Start - 2;
                            }
                            else
                            {
                                return false;
                            }

                            val = fileStream.ReadByte();

                            if (val != -1)
                            {
                                if (val == Constants.NodeStart)
                                {
                                    // start next node
                                    Node nextNode = new Node();
                                    nextNode.Start = fileStream.Position;
                                    currentNode.Next = nextNode;
                                    currentNode = nextNode;
                                    break;
                                }
                                else if (val == Constants.NodeEnd)
                                {
                                    // up 1 level and move 1 position back
                                    // safeTell(pos) && safeSeek(pos)
                                    return true;
                                }
                                else
                                {
                                    // bad format
                                    return false;
                                }
                            }
                            else
                            {
                                // end of file?
                                return true;
                            }
                        }
                        else if (val == Constants.Escape)
                        {
                            fileStream.ReadByte();
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public byte[] GetProps(Node node, out long size)
        {
            if (buffer == null || buffer.Length < node.PropsSize)
            {
                buffer = new byte[node.PropsSize];
            }
            fileStream.Seek(node.Start + 1, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, (int)node.PropsSize);
            uint j = 0;
            bool escaped = false;
            for (uint i = 0; i < node.PropsSize; ++i, ++j)
            {
                if (buffer[i] == Constants.Escape)
                {
                    ++i;
                    buffer[j] = buffer[i];
                    escaped = true;
                }
                else if (escaped)
                {
                    buffer[j] = buffer[i];
                }
            }
            size = j;
            return buffer;
        }

        public bool GetProps(Node node, out PropertyReader props)
        {
            long size;
            byte[] buff = GetProps(node, out size);
            if (buff == null)
            {
                props = null;
                return false;
            }
            else
            {
                props = new PropertyReader(new MemoryStream(buff, 0, (int)size));
                return true;
            }
        }
    }

    public class Node
    {
        public long Start = 0;
        public long PropsSize = 0;
        public long Type = 0;
        public Node Next = null;
        public Node Child = null;
    }
}
