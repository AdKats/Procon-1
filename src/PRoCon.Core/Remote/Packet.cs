// Copyright 2010 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of PRoCon Frostbite.
// 
// PRoCon Frostbite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PRoCon Frostbite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with PRoCon Frostbite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PRoCon.Core.Remote
{
    public class Packet : ICloneable
    {

        public static readonly int PacketHeaderSize = 12;

        public bool OriginatedFromServer { get; private set; }

        public bool IsResponse { get; private set; }

        public UInt32 SequenceNumber { get; set; }

        public List<string> Words { get; private set; }

        public UInt32 PacketSize { get; private set; }

        public DateTime Stamp { get; private set; }


        // Used if we have recieved a packet and need it decoded..
        public Packet(byte[] rawPacket)
        {
            this.NullPacket();

            this.Stamp = DateTime.Now;

            this.DecodePacket(rawPacket);
        }

        // Used if we'll be using EncodePacket to send to the server.
        public Packet(bool isOriginatedFromServer, bool isResponse, UInt32 sequenceNumber, List<string> words)
        {
            this.OriginatedFromServer = isOriginatedFromServer;
            this.IsResponse = isResponse;
            this.SequenceNumber = sequenceNumber;
            this.Words = words;

            this.Stamp = DateTime.Now;
        }

        public Packet(bool isOriginatedFromServer, bool isResponse, UInt32 sequenceNumber, string commandString)
            : this(isOriginatedFromServer, isResponse, sequenceNumber, Packet.Wordify(commandString))
        {

        }

        private void NullPacket()
        {
            this.OriginatedFromServer = false;
            this.IsResponse = false;
            this.SequenceNumber = 0;
            this.Words = new List<string>();
        }


        // Veeerrryy basic replacment for CommandLineToArgvW, since
        // I wasn't using anything that advanced in it anyway.
        public static List<String> Wordify(String command)
        {
            List<String> list = new List<String>();

            String fullWord = String.Empty;
            int quoteStack = 0;
            bool escaped = false;

            foreach (char c in command)
            {

                if (c == ' ')
                {
                    if (quoteStack == 0)
                    {
                        list.Add(fullWord);
                        fullWord = String.Empty;
                    }
                    else
                    {
                        fullWord += ' ';
                    }
                }
                else if (c == 'n' && escaped == true)
                {
                    fullWord += '\n';
                    escaped = false;
                }
                else if (c == 'r' && escaped == true)
                {
                    fullWord += '\r';
                    escaped = false;
                }
                else if (c == 't' && escaped == true)
                {
                    fullWord += '\t';
                    escaped = false;
                }
                else if (c == '"')
                {
                    if (escaped == false)
                    {
                        if (quoteStack == 0)
                        {
                            quoteStack++;
                        }
                        else
                        {
                            quoteStack--;
                        }
                    }
                    else
                    {
                        fullWord += '"';
                        escaped = false;
                    }
                }
                else if (c == '\\')
                {
                    if (escaped == true)
                    {
                        fullWord += '\\';
                        escaped = false;
                    }
                    else
                    {
                        escaped = true;
                    }
                }
                else
                {
                    fullWord += c;
                    escaped = false;
                }
            }

            list.Add(fullWord);

            return list;
        }

        public static string Bltos(bool blBoolean)
        {
            return (blBoolean == true ? "true" : "false");
        }

        public static UInt32 DecodePacketSize(byte[] rawPacket)
        {
            UInt32 ui32ReturnPacketSize = 0;

            if (rawPacket.Length >= Packet.PacketHeaderSize)
            {
                ui32ReturnPacketSize = BitConverter.ToUInt32(rawPacket, 4);
            }

            return ui32ReturnPacketSize;
        }

        public byte[] EncodePacket()
        {

            // Construct the header uint32
            UInt32 ui32Header = this.SequenceNumber & 0x3fffffff;

            if (this.OriginatedFromServer == true)
            {
                ui32Header += 0x80000000;
            }

            if (this.IsResponse == true)
            {
                ui32Header += 0x40000000;
            }

            // Construct the remaining packet headers
            UInt32 ui32PacketSize = Convert.ToUInt32(Packet.PacketHeaderSize);
            UInt32 ui32Words = Convert.ToUInt32(this.Words.Count);

            // Encode each word (WordLength, Word Bytes, Null Byte)
            byte[] encodedWords = new byte[] { };
            foreach (string word in this.Words)
            {

                string strWord = word;

                // Truncate words over 64 kbs (though the string is Unicode it gets converted below so this does make sense)
                if (strWord.Length > UInt16.MaxValue - 1)
                {
                    strWord = strWord.Substring(0, UInt16.MaxValue - 1);
                }

                // Convert the word into a null terminated utf-8 string
                byte[] byteWord = Encoding.UTF8.GetBytes(strWord + Convert.ToChar(0x00));

                byte[] appendEncodedWords = new byte[encodedWords.Length + byteWord.Length + 4];

                encodedWords.CopyTo(appendEncodedWords, 0);

                // append the actual word size (ignoring the null terminated character) and word
                BitConverter.GetBytes(byteWord.Length - 1).CopyTo(appendEncodedWords, encodedWords.Length);
                byteWord.CopyTo(appendEncodedWords, encodedWords.Length + 4);

                encodedWords = appendEncodedWords;
            }

            // Get the full size of the packet.
            ui32PacketSize += Convert.ToUInt32(encodedWords.Length);

            // Now compile the whole packet.
            byte[] encodedPacket = new byte[ui32PacketSize];
            this.PacketSize = ui32PacketSize;
            BitConverter.GetBytes(ui32Header).CopyTo(encodedPacket, 0);
            BitConverter.GetBytes(ui32PacketSize).CopyTo(encodedPacket, 4);
            BitConverter.GetBytes(ui32Words).CopyTo(encodedPacket, 8);
            encodedWords.CopyTo(encodedPacket, Packet.PacketHeaderSize);

            return encodedPacket;
        }

        public void DecodePacket(byte[] rawPacket)
        {

            this.NullPacket();

            UInt32 ui32Header = BitConverter.ToUInt32(rawPacket, 0);
            this.PacketSize = BitConverter.ToUInt32(rawPacket, 4);
            //UInt32 ui32PacketSize = BitConverter.ToUInt32(a_bRawPacket, 4); // Unused here.
            UInt32 ui32Words = BitConverter.ToUInt32(rawPacket, 8);

            this.OriginatedFromServer = Convert.ToBoolean(ui32Header & 0x80000000);
            this.IsResponse = Convert.ToBoolean(ui32Header & 0x40000000);
            this.SequenceNumber = ui32Header & 0x3fffffff;

            int iWordOffset = 0;

            for (UInt32 ui32WordCount = 0; ui32WordCount < ui32Words; ui32WordCount++)
            {
                UInt32 ui32WordLength = BitConverter.ToUInt32(rawPacket, Packet.PacketHeaderSize + iWordOffset);

                this.Words.Add(Encoding.UTF8.GetString(rawPacket, Packet.PacketHeaderSize + iWordOffset + 4, (int)ui32WordLength));

                iWordOffset += Convert.ToInt32(ui32WordLength) + 5; // WordLength + WordSize + NullByte
            }
        }

        public static string Compress(string text)
        {

            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();

            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            //MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);

            ms.Close();
            ms.Dispose();

            return Convert.ToBase64String(gzBuffer);
        }

        public static string Decompress(string compressedText)
        {

            byte[] gzBuffer = Convert.FromBase64String(compressedText);

            using (MemoryStream ms = new MemoryStream())
            {

                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        public string ToDebugString()
        {

            string strReturn = String.Empty;

            for (int i = 0; i < this.Words.Count; i++)
            {
                if (i > 0)
                {
                    strReturn += " ";
                }

                strReturn += String.Format("[{0}-{1}]", i, this.Words[i]);
            }

            return strReturn;
        }

        public override string ToString()
        {

            string strReturn = String.Empty;

            for (int i = 0; i < this.Words.Count; i++)
            {
                if (i > 0)
                {
                    strReturn += " ";
                }

                strReturn += this.Words[i];
            }

            return strReturn;
        }

        public object Clone()
        {
            return new Packet(this.OriginatedFromServer, this.IsResponse, this.SequenceNumber, new List<string>(this.Words));
        }
    }
}
