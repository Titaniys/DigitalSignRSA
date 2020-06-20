using System;
using System.Collections.Generic;
using System.Text;


namespace RSA
{
    /// <summary>
    /// Класс Длинной арифметики
    /// </summary>
    public class BigInt32
    {
        /// <summary>
        /// Максимальная (const) длина BigInt32 в массиве uint[] data (4 байта)
        /// </summary>
        private const int maxLength = 50;     

        /// <summary>
        /// Массив типа uint с помощью которого представляется число BigInt32
        /// </summary>
        public uint[] data = null;             

        /// <summary>
        /// Число элементов в BigInt32
        /// </summary>
        public int dataLength;                 

        /// <summary>
        /// Конструктор без параметра
        /// </summary>
        public BigInt32()
        {
            data = new uint[maxLength];
            dataLength = 1;
        }

        /// <summary>
        /// Конструктор с параметром типа long
        /// </summary>
        public BigInt32(long value)
        {
            data = new uint[maxLength];
            
            dataLength = 0;
            while (value != 0 && dataLength < maxLength)
            {
                data[dataLength] = (uint)(value & 0xFFFFFFFF); // помещаем 32 бита в элемент массива
                value >>= 32; //аналогично целочисленному делению на 2^32
                dataLength++;
            }
            
            if (dataLength == 0) //для маленьких значений
                dataLength = 1;
        }
        
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        public BigInt32(BigInt32 bi)
        {
            data = new uint[maxLength];

            dataLength = bi.dataLength;

            for (int i = 0; i < dataLength; i++)
                data[i] = bi.data[i];
        }

        /// <summary>
        /// Представляет длинное число из строки
        /// </summary>
        /// <param name="value">Исходная строка</param>
        /// <param name="radix">Основание</param>
        public BigInt32(string value)
        {
            BigInt32 multiplier = new BigInt32(1);
            BigInt32 result = new BigInt32();
            
            for (int i = value.Length - 1; i >= 0; i--)
            {
                int posVal = value[i];

                if (posVal >= '0' && posVal <= '9')
                    posVal -= '0';
                else
                    posVal = 9999999;    

                if (value[0] == '-')
                    posVal = -posVal;

                result = result + (multiplier * posVal);

                if ((i - 1) >= 0)
                    multiplier = multiplier * 10;
            }
            
            data = new uint[maxLength];
            for (int i = 0; i < result.dataLength; i++)
                data[i] = result.data[i];

            dataLength = result.dataLength;
        }

        /// <summary>
        /// Конструктор BigInt32 из массива байтов
        /// </summary>
        public BigInt32(byte[] inData)
        {
            dataLength = inData.Length >> 2;

            int leftOver = inData.Length & 0x3;
            if (leftOver != 0)         // длинна не кратна 4
                dataLength++;

            data = new uint[maxLength];

            for (int i = inData.Length - 1, j = 0; i >= 3; i -= 4, j++)
                data[j] = (uint)((inData[i - 3] << 24) + (inData[i - 2] << 16) + (inData[i - 1] << 8) + inData[i]);
            
            if (leftOver == 1)
                data[dataLength - 1] = (uint)inData[0];
            else if (leftOver == 2)
                data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);
            else if (leftOver == 3)
                data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);

            while (dataLength > 1 && data[dataLength - 1] == 0)
                dataLength--;
        }

        /// <summary>
        ///  Конструктор BigInt32 из массива целых чисел
        /// </summary>
        public BigInt32(uint[] inData)
        {
            dataLength = inData.Length;

            data = new uint[maxLength];

            for (int i = dataLength - 1, j = 0; i >= 0; i--, j++)
                data[j] = inData[i];

            while (dataLength > 1 && data[dataLength - 1] == 0)
                dataLength--;
        }

        /// <summary>
        /// Оператор неявного преобразование для long, ulong, int, uint
        /// </summary>
        public static implicit operator BigInt32(long value)
        {
            return (new BigInt32(value));
        }
        
        public static implicit operator BigInt32(ulong value)
        {
            return (new BigInt32(value));
        }
        
        public static implicit operator BigInt32(int value)
        {
            return (new BigInt32((long)value));
        }
        
        public static implicit operator BigInt32(uint value)
        {
            return (new BigInt32((ulong)value));
        }

        /// <summary>
        /// Перегрузка оператора сложения
        /// </summary>
        public static BigInt32 operator +(BigInt32 bi1, BigInt32 bi2)
        {
            BigInt32 result = new BigInt32();

            result.dataLength = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength; //найдет наибольшее

            long carry = 0; //перенос
            for (int i = 0; i < result.dataLength; i++)
            {
                long sum = (long)bi1.data[i] + (long)bi2.data[i] + carry;
                carry = sum >> 32;
                result.data[i] = (uint)(sum & 0xFFFFFFFF);
            }

            if (carry != 0 && result.dataLength < maxLength)
            {
                result.data[result.dataLength] = (uint)(carry);
                result.dataLength++;
            }

            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                result.dataLength--;

            return result;
        }

        /// <summary>
        /// Перегрузка оператора инкримента
        /// </summary>
        public static BigInt32 operator ++(BigInt32 bi1)
        {
            BigInt32 result = new BigInt32(bi1);

            long val, carry = 1;
            int index = 0;

            while (carry != 0 && index < maxLength)
            {
                val = (long)(result.data[index]);
                val++;

                result.data[index] = (uint)(val & 0xFFFFFFFF);
                carry = val >> 32;

                index++;
            }

            if (index > result.dataLength)
                result.dataLength = index;
            else
            {
                while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                    result.dataLength--;
            }

            return result;
        }

        /// <summary>
        /// Перегрузка оператора вычитания
        /// </summary>
        public static BigInt32 operator -(BigInt32 bi1, BigInt32 bi2)
        {
            BigInt32 result = new BigInt32();

            result.dataLength = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength; // ищет наибольший

            long carryIn = 0;
            for (int i = 0; i < result.dataLength; i++)
            {
                long diff;

                diff = (long)bi1.data[i] - (long)bi2.data[i] - carryIn;
                result.data[i] = (uint)(diff & 0xFFFFFFFF);

                if (diff < 0)
                    carryIn = 1;
                else
                    carryIn = 0;
            }

            // взять обратный
            if (carryIn != 0)
            {
                for (int i = result.dataLength; i < maxLength; i++)
                    result.data[i] = 0xFFFFFFFF;
                result.dataLength = maxLength;
            }

            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                result.dataLength--;

            return result;
        }

        /// <summary>
        /// Перегрузка оператора декримента
        /// </summary>
        public static BigInt32 operator --(BigInt32 bi1)
        {
            BigInt32 result = new BigInt32(bi1);

            long val;
            bool carryIn = true;
            int index = 0;

            while (carryIn && index < maxLength)
            {
                val = (long)(result.data[index]);
                val--;

                result.data[index] = (uint)(val & 0xFFFFFFFF);

                if (val >= 0)
                    carryIn = false;

                index++;
            }

            if (index > result.dataLength)
                result.dataLength = index;

            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                result.dataLength--;

            return result;
        }

        /// <summary>
        /// Перегрузка оператора умножения
        /// </summary>
        public static BigInt32 operator *(BigInt32 bi1, BigInt32 bi2)
        {
            int lastPos = maxLength - 1;
            bool bi1Neg = false, bi2Neg = false;

            // берем bi1 и bi2 по модулю 
            
                if ((bi1.data[lastPos] & 0x90000000) != 0)     // bi1 отрицательный
                {
                    bi1Neg = true; bi1 = -bi1;
                }
                if ((bi2.data[lastPos] & 0x90000000) != 0)     // bi2 отрицательный
                {
                    bi2Neg = true; bi2 = -bi2;
                }
            
            BigInt32 result = new BigInt32();

            // Умножение абсолютных велечин
                for (int i = 0; i < bi1.dataLength; i++)
                {
                    if (bi1.data[i] == 0) continue;

                    ulong mcarry = 0;
                    for (int j = 0, k = i; j < bi2.dataLength; j++, k++)
                    {
                        // k = i + j
                        ulong val = ((ulong)bi1.data[i] * (ulong)bi2.data[j]) +
                                     (ulong)result.data[k] + mcarry;

                        result.data[k] = (uint)(val & 0xFFFFFFFF);
                        mcarry = (val >> 32);
                    }

                    if (mcarry != 0)
                        result.data[i + bi2.dataLength] = (uint)mcarry;
                }

            result.dataLength = bi1.dataLength + bi2.dataLength;
            if (result.dataLength > maxLength)
                result.dataLength = maxLength;

            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                result.dataLength--;

            // проверка переполнения
            if ((result.data[lastPos] & 0x90000000) != 0)
            {
                if (bi1Neg != bi2Neg && result.data[lastPos] == 0x90000000)    // bi1 и bi2 разного знака
                {
                    //обрабатывать особый случай, когда умножение производит
                    // максимальное отрицательное число в 2сс

                    if (result.dataLength == 1)
                        return result;
                    else
                    {
                        bool isMaxNeg = true;
                        for (int i = 0; i < result.dataLength - 1 && isMaxNeg; i++)
                        {
                            if (result.data[i] != 0)
                                isMaxNeg = false;
                        }

                        if (isMaxNeg)
                            return result;
                    }
                }

             }

            // Если знаки разные
            if (bi1Neg != bi2Neg)
                return -result;

            return result;
        }

        /// <summary>
        /// Перегрузка оператора сдвига влево <<
        /// </summary>
        /// <param name="bi1"> Папаметр значения </param>
        /// <param name="shiftVal"> Величина сдвига</param>
        public static BigInt32 operator <<(BigInt32 bi1, int shiftVal)
        {
            BigInt32 result = new BigInt32(bi1);
            result.dataLength = shiftLeft(result.data, shiftVal);

            return result;
        }

        /// <summary>
        /// Метод сдвига влево
        /// </summary>
        private static int shiftLeft(uint[] buffer, int shiftVal)
        {
            int shiftAmount = 32;
            int bufLen = buffer.Length;

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            for (int count = shiftVal; count > 0; )
            {
                if (count < shiftAmount)
                    shiftAmount = count;

                ulong carry = 0;
                for (int i = 0; i < bufLen; i++)
                {
                    ulong val = ((ulong)buffer[i]) << shiftAmount;
                    val |= carry;

                    buffer[i] = (uint)(val & 0xFFFFFFFF);
                    carry = val >> 32;
                }

                if (carry != 0)
                {
                    if (bufLen + 1 <= buffer.Length)
                    {
                        buffer[bufLen] = (uint)carry;
                        bufLen++;
                    }
                }
                count -= shiftAmount;
            }
            return bufLen;
        }

        /// <summary>
        /// Перегрузка оператора сдвига вправо >>
        /// </summary>
        /// <param name="bi1"></param>
        /// <param name="shiftVal"></param>
        public static BigInt32 operator >>(BigInt32 bi1, int shiftVal)
        {
            BigInt32 result = new BigInt32(bi1);
            result.dataLength = shiftRight(result.data, shiftVal);


            if ((bi1.data[maxLength - 1] & 0x90000000) != 0) // если отрицательное
            {
                for (int i = maxLength - 1; i >= result.dataLength; i--)
                    result.data[i] = 0xFFFFFFFF;

                uint mask = 0x90000000;
                for (int i = 0; i < 32; i++)
                {
                    if ((result.data[result.dataLength - 1] & mask) != 0)
                        break;

                    result.data[result.dataLength - 1] |= mask;
                    mask >>= 1;
                }
                result.dataLength = maxLength;
            }

            return result;
        }

        /// <summary>
        /// Метод сдвига вправо
        /// </summary>
        private static int shiftRight(uint[] buffer, int shiftVal)
        {
            int shiftAmount = 32;
            int invShift = 0;
            int bufLen = buffer.Length;

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            for (int count = shiftVal; count > 0; )
            {
                if (count < shiftAmount)
                {
                    shiftAmount = count;
                    invShift = 32 - shiftAmount;
                }

                ulong carry = 0;
                for (int i = bufLen - 1; i >= 0; i--)
                {
                    ulong val = ((ulong)buffer[i]) >> shiftAmount;
                    val |= carry;

                    carry = ((ulong)buffer[i]) << invShift;
                    buffer[i] = (uint)(val);
                }

                count -= shiftAmount;
            }

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            return bufLen;
        }
        
        /// <summary>
        /// Перегрузка оператора отрицания
        /// </summary>
        public static BigInt32 operator -(BigInt32 bi1)
        {
            if (bi1.dataLength == 1 && bi1.data[0] == 0) 
                return (new BigInt32(1));

            BigInt32 result = new BigInt32(bi1);

            for (int i = 0; i < maxLength; i++)
                result.data[i] = (uint)(~(bi1.data[i]));

            long val, carry = 1;
            int index = 0;

            while (carry != 0 && index < maxLength)
            {
                val = (long)(result.data[index]);
                val++;

                result.data[index] = (uint)(val & 0xFFFFFFFF);
                carry = val >> 32;

                index++;
            }

            result.dataLength = maxLength;

            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                result.dataLength--;
            return result;
        }

        /// <summary>
        /// Определяет считаются ли равными объекты класса BigInt32
        /// </summary>
        public override bool Equals(object o)
        {
            BigInt32 bi = (BigInt32)o;

            if (this.dataLength != bi.dataLength)
                return false;

            for (int i = 0; i < this.dataLength; i++)
            {
                if (this.data[i] != bi.data[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Перегрузка операторов равенства/неравенства
        /// </summary>
        public static bool operator ==(BigInt32 bi1, BigInt32 bi2)
        {
            return bi1.Equals(bi2);
        }

        public static bool operator !=(BigInt32 bi1, BigInt32 bi2)
        {
            return !(bi1.Equals(bi2));
        }

       
        //Ругается компилятор в случае отсутствия (Т.к метод Equals перегружен в данном классе)
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        
        /// <summary>
        /// Перегрузка операторов строгово/ нестрогово сравнения
        /// </summary>
        public static bool operator >(BigInt32 bi1, BigInt32 bi2)
        {
            int pos = maxLength - 1;

            if ((bi1.data[pos] & 0x90000000) != 0 && (bi2.data[pos] & 0x90000000) == 0)
                return false;

            else if ((bi1.data[pos] & 0x90000000) == 0 && (bi2.data[pos] & 0x90000000) != 0)
                return true;

            int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength; //находим самый длинный bi_
            for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;

            if (pos >= 0)
            {
                if (bi1.data[pos] > bi2.data[pos])
                    return true;
                return false;
            }
            return false;
        }


        public static bool operator <(BigInt32 bi1, BigInt32 bi2)
        {
            int pos = maxLength - 1;

            if ((bi1.data[pos] & 0x90000000) != 0 && (bi2.data[pos] & 0x90000000) == 0)
                return true;

            else if ((bi1.data[pos] & 0x90000000) == 0 && (bi2.data[pos] & 0x90000000) != 0)
                return false;
            
            int len = (bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength; //находим самый длинный bi_
            for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;

            if (pos >= 0)
            {
                if (bi1.data[pos] < bi2.data[pos])
                    return true;
                return false;
            }
            return false;
        }


        public static bool operator >=(BigInt32 bi1, BigInt32 bi2)
        {
            return (bi1 == bi2 || bi1 > bi2);
        }


        public static bool operator <=(BigInt32 bi1, BigInt32 bi2)
        {
            return (bi1 == bi2 || bi1 < bi2);
        }

        /// <summary>
        /// Алгоритм деления двух больших чисел.
        /// Делитель содержит больше 1 цифры
        /// </summary>
        /// <param name="bi1">Делимое</param>
        /// <param name="bi2">Делитель</param>
        /// <param name="Quotient">Частное</param>
        /// <param name="Remainder">Остаток</param>
        private static void AlgorithmOfDivideMultiByte(BigInt32 bi1, BigInt32 bi2, BigInt32 Quotient, BigInt32 Remainder)
        {
            uint[] result = new uint[maxLength];

            int remainderLen = bi1.dataLength + 1;
            uint[] remainder = new uint[remainderLen];

            uint mask = 0x90000000; //битовая маска
            uint val = bi2.data[bi2.dataLength - 1];
            int shift = 0, resultPos = 0;

            while (mask != 0 && (val & mask) == 0)
            {
                shift++; mask >>= 1;
            }

            for (int i = 0; i < bi1.dataLength; i++)
                remainder[i] = bi1.data[i];
            shiftLeft(remainder, shift);
            bi2 = bi2 << shift;

            int j = remainderLen - bi2.dataLength;
            int pos = remainderLen - 1;

            ulong firstDivisorByte = bi2.data[bi2.dataLength - 1];
            ulong secondDivisorByte = bi2.data[bi2.dataLength - 2];

            int divisorLen = bi2.dataLength + 1;
            uint[] dividendPart = new uint[divisorLen];

            while (j > 0)
            {
                ulong dividend = ((ulong)remainder[pos] << 32) + (ulong)remainder[pos - 1];

                ulong q_hat = dividend / firstDivisorByte;
                ulong r_hat = dividend % firstDivisorByte;

                bool done = false;
                while (!done)
                {
                    done = true;

                    if (q_hat == 0x100000000 ||
                       (q_hat * secondDivisorByte) > ((r_hat << 32) + remainder[pos - 2]))
                    {
                        q_hat--;
                        r_hat += firstDivisorByte;

                        if (r_hat < 0x100000000)
                            done = false;
                    }
                }

                for (int h = 0; h < divisorLen; h++)
                    dividendPart[h] = remainder[pos - h];

                BigInt32 kk = new BigInt32(dividendPart);
                BigInt32 ss = bi2 * (long)q_hat;

                while (ss > kk)
                {
                    q_hat--;
                    ss -= bi2;
                
                }
                BigInt32 yy = kk - ss;

                for (int h = 0; h < divisorLen; h++)
                    remainder[pos - h] = yy.data[bi2.dataLength - h];

                result[resultPos++] = (uint)q_hat;

                pos--;
                j--;
            }

            Quotient.dataLength = resultPos;
            int y = 0;
            for (int x = Quotient.dataLength - 1; x >= 0; x--, y++)
                Quotient.data[y] = result[x];
            for (; y < maxLength; y++)
                Quotient.data[y] = 0;

            while (Quotient.dataLength > 1 && Quotient.data[Quotient.dataLength - 1] == 0)
                Quotient.dataLength--;

            if (Quotient.dataLength == 0)
                Quotient.dataLength = 1;

            Remainder.dataLength = shiftRight(remainder, shift);

            for (y = 0; y < Remainder.dataLength; y++)
                Remainder.data[y] = remainder[y];
            for (; y < maxLength; y++)
                Remainder.data[y] = 0;
        }

        /// <summary>
        /// Алгоритм деления числа BigInt32 на однозначное число
        /// </summary>
        private static void AlgorithmOfDivideSingleByte(BigInt32 bi1, BigInt32 bi2, BigInt32 Quotient, BigInt32 Remainder)
        {
            uint[] result = new uint[maxLength];
            int resultPos = 0;

            // Копируем ....
            for (int i = 0; i < maxLength; i++)
                Remainder.data[i] = bi1.data[i];
            Remainder.dataLength = bi1.dataLength;

            while (Remainder.dataLength > 1 && Remainder.data[Remainder.dataLength - 1] == 0)
                Remainder.dataLength--;

            ulong divisor = (ulong)bi2.data[0];
            int pos = Remainder.dataLength - 1;
            ulong dividend = (ulong)Remainder.data[pos];

            if (dividend >= divisor)
            {
                ulong quotient = dividend / divisor;
                result[resultPos++] = (uint)quotient;

                Remainder.data[pos] = (uint)(dividend % divisor);
            }
            pos--;

            while (pos >= 0)
            {
            
                dividend = ((ulong)Remainder.data[pos + 1] << 32) + (ulong)Remainder.data[pos];
                ulong quotient = dividend / divisor;
                result[resultPos++] = (uint)quotient;

                Remainder.data[pos + 1] = 0;
                Remainder.data[pos--] = (uint)(dividend % divisor);
            
            }

            Quotient.dataLength = resultPos;
            int j = 0;
            for (int i = Quotient.dataLength - 1; i >= 0; i--, j++)
                Quotient.data[j] = result[i];
            for (; j < maxLength; j++)
                Quotient.data[j] = 0;

            while (Quotient.dataLength > 1 && Quotient.data[Quotient.dataLength - 1] == 0)
                Quotient.dataLength--;

            if (Quotient.dataLength == 0)
                Quotient.dataLength = 1;

            while (Remainder.dataLength > 1 && Remainder.data[Remainder.dataLength - 1] == 0)
                Remainder.dataLength--;
        }

        /// <summary>
        /// Перегрузка оператора деления
        /// </summary>
        public static BigInt32 operator /(BigInt32 bi1, BigInt32 bi2)
        {
            BigInt32 quotient = new BigInt32(); //частное
            BigInt32 remainder = new BigInt32(); //остаток

            if (bi1 < bi2)
            {
                return quotient; //0
            }

            else
            {
                if (bi2.dataLength == 1)
                    AlgorithmOfDivideSingleByte(bi1, bi2, quotient, remainder);
                else
                    AlgorithmOfDivideMultiByte(bi1, bi2, quotient, remainder);

            return quotient;
            }
        }

        /// <summary>
        /// Перегрузка оператора присваивания остатка
        /// </summary>
        public static BigInt32 operator %(BigInt32 bi1, BigInt32 bi2)
        {
            BigInt32 quotient = new BigInt32();
            BigInt32 remainder = new BigInt32(bi1);

            if (bi1 < bi2)
            {
                return remainder;
            }
            else
            {
                if (bi2.dataLength == 1)
                    AlgorithmOfDivideSingleByte(bi1, bi2, quotient, remainder);
                else
                    AlgorithmOfDivideMultiByte(bi1, bi2, quotient, remainder);

                return remainder;
            }
        }
        
        /// <summary>
        /// Возвращает максимальный BigInt32
        /// </summary>
        public BigInt32 max(BigInt32 bi)
        {
            if (this > bi)
                return (new BigInt32(this));
            else
                return (new BigInt32(bi));
        }

        /// <summary>
        /// Возвращает минимальный BigInt32
        /// </summary>
        public BigInt32 min(BigInt32 bi)
        {
            if (this < bi)
                return (new BigInt32(this));
            else
                return (new BigInt32(bi));
        }

        /// <summary>
        /// Возвращает строку из BigInt32
        /// </summary>
        public override string ToString()
        {
            string result = "";

            BigInt32 a = this;
            BigInt32 quotient = new BigInt32();
            BigInt32 remainder = new BigInt32();

            while (a.dataLength > 1 || (a.dataLength == 1 && a.data[0] != 0))
                {
                    AlgorithmOfDivideSingleByte(a, 10, quotient, remainder);

                    result = remainder.data[0] + result;

                    a = quotient;
                }
            
            return result;
        }

        /// <summary>
        /// Выполняет модульное деление числа, возведенного в степень exp.
        /// </summary>
        /// <param name="exp">экспонента</param>
        /// <param name="n">модуль</param>
        /// <returns></returns>
        public BigInt32 modPow(BigInt32 exp, BigInt32 n)
        {
            
            BigInt32 resultRemainder = 1;      // конечный остаток

            BigInt32 tempRemainder = this % n; // временный остаток
            
            BigInt32 constant = new BigInt32(); //константа "мю" для алгоритма баррета
          
            int i = n.dataLength << 1;
          
            constant.data[i] = 0x00000001;
            
            constant.dataLength = i + 1;
            
            constant = constant / n;
          
            for (int pos = 0; pos < exp.dataLength; pos++)
            {
                uint mask = 0x00000001;
                
                for (int index = 0; index < 32; index++)
                {
                    if ((exp.data[pos] & mask) != 0)
                        resultRemainder = AlgorithmOfBarrettReduction(resultRemainder * tempRemainder, n, constant);

                    mask <<= 1;

                    tempRemainder = AlgorithmOfBarrettReduction(tempRemainder * tempRemainder, n, constant);

                }
            }

            return resultRemainder;
        }

        /// <summary>
        /// Вычисляет return = x mod n. 
        /// Использованная литература.
        /// Анализ арифметических операций современной криптографии и способы их аппаратной реализации Расулов О.Х
        /// </summary>
        /// <param name="x"> Делимое </param>
        /// <param name="n"> модуль </param>
        /// <param name="constant"> константа "мю" </param>
        private BigInt32 AlgorithmOfBarrettReduction(BigInt32 x, BigInt32 n, BigInt32 constant)
        {
            int k = n.dataLength, //кол-во слов в модуле
                kPlusOne = k + 1,
                kMinusOne = k - 1;

            BigInt32 q1 = new BigInt32();

            // q1 = x / b^(k-1)
            for (int i = kMinusOne, j = 0; i < x.dataLength; i++, j++)
                q1.data[j] = x.data[i];

            q1.dataLength = x.dataLength - kMinusOne;
            
            BigInt32 q2 = q1 * constant;
            BigInt32 q3 = new BigInt32();

            // q3 = q2 / b^(k+1)
            for (int i = kPlusOne, j = 0; i < q2.dataLength; i++, j++)
                q3.data[j] = q2.data[i];

            q3.dataLength = q2.dataLength - kPlusOne;
            
            // r1 = x mod b^(k+1)
            BigInt32 r1 = new BigInt32();
            int lengthToCopy = (x.dataLength > kPlusOne) ? kPlusOne : x.dataLength;
            for (int i = 0; i < lengthToCopy; i++)
                r1.data[i] = x.data[i];
            r1.dataLength = lengthToCopy;
            

            // r2 = (q3 * n) mod b^(k+1)
            BigInt32 r2 = new BigInt32();
            for (int i = 0; i < q3.dataLength; i++)
            {
                if (q3.data[i] == 0) continue;

                ulong mcarry = 0;
                int t = i;
                for (int j = 0; j < n.dataLength && t < kPlusOne; j++, t++)
                {
                    // t = i + j
                    ulong val = ((ulong)q3.data[i] * (ulong)n.data[j]) +
                                 (ulong)r2.data[t] + mcarry;

                    r2.data[t] = (uint)(val & 0xFFFFFFFF);
                    mcarry = (val >> 32);
                }

                if (t < kPlusOne)
                    r2.data[t] = (uint)mcarry;
            }
            r2.dataLength = kPlusOne;
            while (r2.dataLength > 1 && r2.data[r2.dataLength - 1] == 0)
                r2.dataLength--;

            r1 -= r2;
            
            while (r1 >= n)
                r1 -= n;

            return r1;
        }

        /// <summary>
        /// Генератор случайных бит
        /// </summary>
        public void GetRandomBits(int bits, Random rand)
        {
            int dwords = bits >> 5;
            int remBits = bits & 0x1F;

            if (remBits != 0)
                dwords++;

            for (int i = 0; i < dwords; i++) //случайные биты
                data[i] = (uint)(rand.NextDouble() * 0x100000000);

            for (int i = dwords; i < maxLength; i++) //остальное нулями
                data[i] = 0;

            if (remBits != 0)
            {
                uint mask = (uint)(0x01 << (remBits - 1));
                data[dwords - 1] |= mask;

                mask = (uint)(0xFFFFFFFF >> (32 - remBits));
                data[dwords - 1] &= mask;
            }
            else
                data[dwords - 1] |= 0x90000000;

            dataLength = dwords;

            if (dataLength == 0)
                dataLength = 1;
        }

        /// <summary>
        /// Возвращает позицию старшего значимого  бита в BigInt32.
        /// </summary>
        public int bitCount()
        {
            while (dataLength > 1 && data[dataLength - 1] == 0)
                dataLength--;
            
            uint value = data[dataLength - 1];
            
            uint mask = 0x90000000;
            int bits = 32;

            while (bits > 0 && (value & mask) == 0)
            {
                bits--;
                mask >>= 1;
            }
            bits += ((dataLength - 1) << 5);

            return bits;
        }
        
        /// <summary>
        /// Вероятностный тест на простоту основанный на алгоритме Милера-Рабина
        /// Для любого p > 0, при p - 1 = 2^s * t
        /// p вероятно простое для любого  a < p,
        /// 1) a^t mod p = 1 или
        /// 2) a^((2^j)*t) mod p = p-1 для некоторого 0 <= j <= s-1
        /// </summary>
        public bool RabinMillerTest(int confidence)
        {
            BigInt32 thisVal = this;

            if (thisVal.dataLength == 1)
            {
                // для малых значений
                if (thisVal.data[0] == 0 || thisVal.data[0] == 1)
                    return false;
                else if (thisVal.data[0] == 2 || thisVal.data[0] == 3)
                    return true;
            }

            if ((thisVal.data[0] & 0x1) == 0)     // четное число
                return false;

            // Вычислим значение s и t
            BigInt32 p_sub1 = thisVal - (new BigInt32(1));   // р-1
            int s = 0;

            for (int index = 0; index < p_sub1.dataLength; index++)
            {
                uint mask = 0x01;

                for (int i = 0; i < 32; i++)
                {
                    if ((p_sub1.data[index] & mask) != 0)
                    {
                        index = p_sub1.dataLength;      // разбить внутренний цикл
                        break;
                    }
                    mask <<= 1;
                    s++;
                }
            }

            BigInt32 t = p_sub1 >> s;

            int bits = thisVal.bitCount();
            BigInt32 a = new BigInt32();
            Random rand = new Random();

            for (int round = 0; round < confidence; round++)
            {
                bool done = false;

                while (!done)		// генерирует a < n
                {
                    int testBits = 0;

                    while (testBits < 2)
                        testBits = (int)(rand.NextDouble() * bits);

                    a.GetRandomBits(testBits, rand);

                    int byteLen = a.dataLength;

                    if (byteLen > 1 || (byteLen == 1 && a.data[0] != 1)) // убедиться, что а != 0
                        done = true;
                }

                // проверка является ли а свидетелем простоты
                BigInt32 gcdTest = a.gcd(thisVal);
                if (gcdTest.dataLength == 1 && gcdTest.data[0] != 1)
                    return false;

                BigInt32 b = a.modPow(t, thisVal); 

                bool result = false;

                if (b.dataLength == 1 && b.data[0] == 1)         // a^t mod p = 1
                    result = true;

                for (int j = 0; result == false && j < s; j++)
                {
                    if (b == p_sub1)         // a^((2^j)*t) mod p = p-1 для 0 <= j <= s-1
                    {
                        result = true;
                        break;
                    }

                    b = (b * b) % thisVal;
                }

                if (result == false)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Алгоритм Евклида
        /// </summary>
        /// <returns> НОД </returns>
        public BigInt32 gcd(BigInt32 bi)
        {
            BigInt32 x = this;
            BigInt32 y = bi;
            BigInt32 g = x;

            while (x.dataLength > 1 || (x.dataLength == 1 && x.data[0] != 0))
            {
                g = x;
                x = y % x;
                y = g;
            }
            return g;
        }

        /// <summary>
        /// Возвращает младшие 4 байта в BigInt32 как int.
        /// </summary>
        public int IntValue()
        {
            return (int)data[0];
        }

//***************************************************************************************        
//***************************************************************************************
        /// <summary>
        /// Генерирует положительный  BigInt32. С большой вероятностью простое.+
        /// </summary>
        public static BigInt32 GetPseudoPrime(int bits, int confidence, Random rand)
        {
            BigInt32 result = new BigInt32();
            bool done = false;

            while (!done)
            {
                result.GetRandomBits(bits, rand);
                result.data[0] = 0x01;		// делает сгенерированное число нечетным

                done = result.RabinMillerTest(confidence); //проверяем на простоту 
            }
            return result;
        }
        
        /// <summary>
        /// Статический НОД чисел а и в
        /// </summary>
        public static BigInt32 NOD(BigInt32 a, BigInt32 b)
        {
            while (a != 0 && b != 0)
            {
                if (a >= b) 
                    a = a % b;
                else 
                    b = b % a;
            }
            return a + b;
        }
        
        /// <summary>
        /// Расширенный алгоритм Евклида (для нахождения обратного элемента по некоторому модулю)
        /// </summary>
        private static void Evklid(BigInt32 a, BigInt32 b, ref BigInt32 x, ref BigInt32 y, ref BigInt32 d) 
        {
            BigInt32 q = new BigInt32();

            BigInt32 r = new BigInt32();
            BigInt32 x1 = new BigInt32();
            BigInt32 x2 = new BigInt32();
            BigInt32 y1 = new BigInt32();
            BigInt32 y2 = new BigInt32();

            BigInt32 one =new BigInt32(1);
            BigInt32 O = new BigInt32(0);
            if (b == O)
            {
                d = a; x = new BigInt32(1); y = new BigInt32(0);
                return;
            }
            x2 = new BigInt32(1); x1 = new BigInt32(0); y2 = new BigInt32(0); y1 = new BigInt32(1);
            while (b > O)
            {
                q = a / b; r = a % b;
                x = x2 - (q * x1); if (q == O || y1 == O) y = y2; else y = y2 - (q * y1);
                a = b; b = r;
                x2 = (x1); x1 = (x); y2 = (y1); y1 = (y);
            }
            d = (a); x = (x2); y = (y2);
        }
        
        /// <summary>
        /// Ищет обратный элемент к e по модулю n 
        /// </summary>
        public static BigInt32 Inverse(BigInt32 е, BigInt32 n) 
        {
            BigInt32 O = new BigInt32(0);
            BigInt32 ONE = new BigInt32(1);
            BigInt32 d = new BigInt32(0);
            BigInt32 x = new BigInt32(0);
            BigInt32 y = new BigInt32(0);
            Evklid(е, n, ref x, ref y, ref d);
            if (d == ONE) 
            { 
                if (x < O) return x + n; 
                else return x; 
            }
            return O;
        }
    }
}
