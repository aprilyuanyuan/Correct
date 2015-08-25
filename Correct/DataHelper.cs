using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Lon.Util
{
    public class DataHelper
    {
        public static int[] NumASCIITable
            = new int[] { '0', '1', '2', '3','4',
                          '5','6','7','8','9',
                          'A','B','C','E','F'};
        /// <summary>
        /// 从源数据的指定位置填充bitCount位数据到目标字节
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="startBit"></param>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        public static bool FillBit(ref int dst, int src, int startBit, int bitCount)
        {
            int tempByte = dst;
            int marker = 0x01;
            for (int i = 0; i < bitCount; i++)
            {
                marker <<= 1;
                marker += 0x01;
            }
            src &= marker;
            marker <<= startBit;
            tempByte &= ~marker;
            src <<= startBit;
            dst = tempByte + src;
            return true;
        }
        /// <summary>
        /// 从源数据的指定位置填充bitCount位数据到目标字节
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="startBit"></param>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        public static bool FillBit(ref byte dst, int src, int startBit, int bitCount)
        {
            int temp = dst;
            bool ret;
            ret = FillBit(ref temp, src, startBit, bitCount);
            dst = (byte)temp;
            return ret;
        }

        public static bool CheckBitValue(byte[] bytes,long index)
        {
            if (bytes.Length <= index / 8) throw (new Exception("超出范围"));
            byte temp=bytes[index/8];
            return (((long)temp >> (int)(index % 8)) & 0x01)==0x01;

        }

        public static byte GetValueFromASCII(byte pbASCII)
        {
            String tempStr = new String((char)pbASCII, 1);
            int temp = 0;
            if (int.TryParse(tempStr, System.Globalization.NumberStyles.HexNumber, null, out temp))
            {
                return (byte)temp;
            }

            return 0;
        }

        public static bool GetValueFormASCIIArr(byte[] bytes,int startIndex,int Length,ref int retVal)
        {
            StringBuilder sb = new StringBuilder();
            for(int i=startIndex;i<bytes.Length&&(i-startIndex)<Length;i++)
            {
                sb.Append((char)bytes[i]);
            }
            int val = 0;
            if (!int.TryParse(sb.ToString(), out val)) return false;
            retVal = val;
            return true;
            
        }

        public static bool GetValueFormASCIIArr(byte[] bytes, int startIndex, int Length, ref float retVal)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i < bytes.Length && (i - startIndex) < Length; i++)
            {
                sb.Append((char)bytes[i]);
            }
            float val = 0;
            if (!float.TryParse(sb.ToString(), out val)) return false;
            retVal = val;
            return true;

        }

        public static int GetIntFormHexStr(String txt, int defaultVal)
        {
            int result;
            int.TryParse(txt, System.Globalization.NumberStyles.HexNumber, null,out result);
            return result;
        }

        public static int CalcByteArrySum(byte[] bytes,int startIndex,int Length)
        {
            int sum=0;
            for(int i=startIndex;i<bytes.Length&&(i-startIndex)<Length;i++)
            {
                sum += bytes[i];
            }
            return sum;
        }

        /// <summary>
        /// 整数填充到指定数组的指定位置
        /// </summary>
        /// <param name="val"></param>
        /// <param name="dstArr"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static bool FillIntToByteArr(int val,byte[] dstArr,int startIndex)
        {
            if (dstArr == null) return false;
            if (dstArr.Length <= startIndex) return false;
            for(int i=0;i<4&&(startIndex+i)<dstArr.Length;i++)
            {
                dstArr[startIndex+i] = (byte)((val >> (i*8))&0xff);
            }
            return true;
        }

        /// <summary>
        /// 整数填充到指定数组的指定位置
        /// </summary>
        /// <param name="val"></param>
        /// <param name="dstArr"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static bool FillIntToByteArr(uint val, byte[] dstArr, int startIndex)
        {
            return FillIntToByteArr((int)val, dstArr, startIndex);
        }
        /// <summary>
        /// 整数填充到指定数组的指定位置
        /// </summary>
        /// <param name="val"></param>
        /// <param name="dstArr"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static bool FillIntToByteArr(Int16 val, byte[] dstArr, int startIndex)
        {
            if (dstArr == null) return false;
            if (dstArr.Length <= startIndex) return false;
            for (int i = 0; i < 2&(startIndex+i)<startIndex; i++)
            {
                dstArr[startIndex] =(byte)((val >> (i * 8)) & 0xff);
            }
            return true;
        }
        public static bool FillIntToByteArr(UInt16 val, byte[] dstArr, int startIndex)
        {
            return FillIntToByteArr((Int16)val, dstArr, startIndex);
        }
        
        public static Int32 Int32FormBCD(int val)
        {
            int res=0;
            for(int i=0;i<8;i++)
            {
                res *= 10;
                res += (int)((val & 0xf0000000)>>(4*7));
                val <<= 4;
            }
            return res;
        }

       
        public static void WriteToStream(BinaryWriter bw,String text)
        {
            if(String.IsNullOrEmpty(text))
            {
                bw.Write((UInt16)0);
                return;
            }
            byte[] buf=StringHelper.CP936.GetBytes(text);
            bw.Write((UInt16)buf.Length);
            bw.Write(buf);
        }
        public static String ReadStrFormStream(BinaryReader br)
        {
            int bytesCount = br.ReadUInt16();
            if (bytesCount == 0)
            {
                return "";
            }

            byte[] buf = br.ReadBytes(bytesCount);
            return StringHelper.CP936.GetString(buf);
         
        }
        public static void WriteToStream(BinaryWriter bw, bool val)
        {
            byte iVal = (byte)(val ? 1 : 0);
            bw.Write(iVal);
         
        }
        public static bool ReadBoolFormStream(BinaryReader br)
        {
            int val = br.ReadByte();
            if (val == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
          

        }
    }
}
