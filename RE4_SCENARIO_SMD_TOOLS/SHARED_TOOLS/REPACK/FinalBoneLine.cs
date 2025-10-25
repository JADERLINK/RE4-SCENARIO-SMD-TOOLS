using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleEndianBinaryIO;

namespace SHARED_TOOLS.REPACK
{
    public class FinalBoneLine
    {
        private Endianness endianness;

        public FinalBoneLine(byte[] line, Endianness endianness)
        {
            this.endianness = endianness;
            Line = line;
        }

        public FinalBoneLine(byte boneId, byte boneParent, float posX, float posY, float posZ, Endianness endianness)
        {
            this.endianness = endianness;
            BoneId = boneId;
            BoneParent = boneParent;
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
        }

        private byte[] _line = new byte[16];

        public byte[] Line
        {
            get
            {
                return _line.ToArray();
            }
            set
            {
                for (int i = 0; i < value.Length && i < _line.Length; i++)
                {
                    _line[i] = value[i];
                }
            }
        }

        public byte BoneId
        {
            get
            {
                return _line[0];
            }
            set
            {
                _line[0] = value;
            }
        }

        public byte BoneParent
        {
            get
            {
                return _line[1];
            }
            set
            {
                _line[1] = value;
            }
        }

        public float PosX
        {
            get
            {
                return EndianBitConverter.ToSingle(_line, 0x4, endianness);
            }
            set
            {
                var bvalue = EndianBitConverter.GetBytes(value, endianness);
                bvalue.CopyTo(_line, 0x4);
            }
        }

        public float PosY
        {
            get
            {
                return EndianBitConverter.ToSingle(_line, 0x8, endianness);
            }
            set
            {
                var bvalue = EndianBitConverter.GetBytes(value, endianness);
                bvalue.CopyTo(_line, 0x8);
            }
        }

        public float PosZ
        {
            get
            {
                return EndianBitConverter.ToSingle(_line, 0xC, endianness);
            }
            set
            {
                var bvalue = EndianBitConverter.GetBytes(value, endianness);
                bvalue.CopyTo(_line, 0xC);
            }
        }

    }
}
