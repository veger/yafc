using System;
using System.Numerics;

namespace YAFC.Model
{
    public class Bits
    {
        private int _length;
        public int length
        {
            get => _length;
            private set
            {
                Array.Resize(ref data, (int)Math.Ceiling(value / 64f));
                _length = value;
            }
        }
        private ulong[] data = Array.Empty<ulong>();

        public bool this[int i]
        {
            get
            {
                if (length <= i)
                {
                    return false;
                }
                return (data[i / 64] & (1ul << (i % 64))) != 0;
            }
            set
            {
                if (length <= i)
                {
                    length = i + 1;
                }
                if (value)
                {
                    // set bit
                    data[i / 64] |= 1ul << (i % 64);
                }
                else
                {
                    // clear bit
                    data[i / 64] &= ~(1ul << (i % 64));
                }
            }
        }

        public Bits() { }

        public Bits(bool setBit0)
        {
            if (setBit0)
            {
                this[0] = true;
            }
        }

        public static Bits operator &(Bits a, Bits b)
        {
            Bits result = new();

            if (a is null && b is null)
            {
                return result;
            }

            // Make sure that a and b are not null
            a ??= new Bits();
            b ??= new Bits();
            result.length = Math.Max(a.length, b.length);

            for (int i = 0; i < result.data.Length; i++)
            {
                if (a.data.Length <= i || b.data.Length <= i)
                {
                    result.data[i] = 0;
                }
                else
                {
                    result.data[i] = a.data[i] & b.data[i];
                }
            }

            return result;
        }

        public static Bits operator |(Bits a, Bits b)
        {
            Bits result = new();

            if (a is null && b is null)
            {
                return result;
            }

            // Make sure that a and b are not null
            a ??= new Bits();
            b ??= new Bits();
            result.length = Math.Max(a.length, b.length);

            for (int i = 0; i < result.data.Length; i++)
            {
                if (a.data.Length <= i)
                {
                    result.data[i] = b.data[i];
                }
                else if (b.data.Length <= i)
                {
                    result.data[i] = a.data[i];
                }
                else
                {
                    result.data[i] = a.data[i] | b.data[i];
                }
            }

            return result;
        }

        public static Bits operator <<(Bits a, int shift)
        {
            if (shift != 1)
            {
                throw new NotImplementedException("only shifting by 1 is supported");
            }

            Bits result = new()
            {
                length = a.length + 1
            };

            // bits that 'fell off' in the previous shifting operation
            var carrier = 0ul;
            for (int i = 0; i < a.data.Length; i++)
            {
                result.data[i] = (a.data[i] << shift) | carrier;
                carrier = a.data[i] & ~(~0ul >> shift); // Mask with 'shift amount of MSB'
            }
            if (carrier != 0)
            {
                // Reason why only shift == 1 is supported, it is messy to map the separate bits back on data[length] and data[length - 1]
                // Since shift != 1 is never used, its implementation is omitted
                result[result.length] = true;
            }

            return result;
        }

        public static bool operator <(Bits a, Bits b)
        {
            if (b is null)
            {
                // b doesn't have a possible value, so a >= b
                return false;
            }

            if (a is null)
            {
                // true if b has a bit set
                return !b.IsClear();
            }

            var maxLength = Math.Max(a.data.Length, b.data.Length);
            for (int i = maxLength - 1; i >= 0; i--)
            {
                if (a.data.Length <= i)
                {
                    if (b.data[i] != 0)
                    {
                        // b is larger, so a < b
                        return true;
                    }
                }
                else if (b.data.Length <= i)
                {
                    if (a.data[i] != 0)
                    {
                        // a is larger, so a > b
                        return false;
                    }
                }

                if (a.data[i] < b.data[i])
                {
                    return true;
                }

                if (a.data[i] > b.data[i])
                {
                    return false;
                }

                // bits are equal, go to next pair
            }

            // a and b are fully equal, so not a < b
            return false;
        }

        public static bool operator >(Bits a, Bits b)
        {
            if (a is null)
            {
                // a doesn't have a possible value, so b >= a
                return false;
            }

            if (b is null)
            {
                // true if a has a bit set
                return !a.IsClear();
            }

            var maxLength = Math.Max(a.data.Length, b.data.Length);
            for (int i = maxLength - 1; i >= 0; i--)
            {
                if (a.data.Length <= i)
                {
                    if (b.data[i] != 0)
                    {
                        // b is larger, so a < b
                        return false;
                    }
                }
                else if (b.data.Length <= i)
                {
                    if (a.data[i] != 0)
                    {
                        // a is larger, so a > b
                        return true;
                    }
                }

                if (a.data[i] < b.data[i])
                {
                    return false;
                }

                if (a.data[i] > b.data[i])
                {
                    return true;
                }

                // bits are equal, go to next pair
            }

            // a and b are equal, so not a > b
            return false;
        }

        public static Bits operator -(Bits a, ulong b)
        {
            if (a.IsClear())
            {
                throw new ArgumentOutOfRangeException(nameof(a), "a is 0, so the result would become negative!");
            }

            if (b != 1)
            {
                throw new NotImplementedException("only subtracting by 1 is supported");
            }

            Bits result = new()
            {
                length = a.length,
                data = (ulong[])a.data.Clone()
            };

            // Only works for subtracting by 1!
            // subtract by 1: find lowest bit that is set, unset this bit and set all previous bits
            var index = 0;
            while (result[index] == false)
            {
                result[index] = true;
                index++;
            }
            result[index] = false;

            return result;
        }


        // Check if the first ulong of a equals to b, rest of a needs to be 0
        public static bool operator ==(Bits a, ulong b)
        {
            if (a is null || a.length == 0 || a.data[0] != b)
            {
                return false;
            }

            for (int i = 1; i < a.data.Length; i++)
            {
                if (a.data[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(Bits a, Bits b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            if (a.length != b.length)
            {
                return false;
            }

            for (int i = 1; i < a.data.Length; i++)
            {
                if (a.data[i] != b.data[i])
                {
                    return false;
                }
            }

            return true;
        }


        public static bool operator !=(Bits a, Bits b)
        {
            if (a is null && b is null)
            {
                return false;
            }

            if (a is null || b is null)
            {
                // Either a or b is null
                return true;
            }

            if (a.length != b.length)
            {
                return true;
            }

            for (int i = 1; i < a.data.Length; i++)
            {
                if (a.data[i] != b.data[i])
                {
                    return true;
                }
            }

            return false;
        }

        // Check if the first ulong of a does not equals to b or rest of data is not zero
        public static bool operator !=(Bits a, ulong b)
        {
            if (a is null || a.length == 0)
            {
                return false;
            }

            if (a.data[0] != b)
            {
                return true;
            }

            for (int i = 1; i < a.data.Length; i++)
            {
                if (a.data[i] != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsClear()
        {
            return HighestBitSet() == -1;
        }

        public int HighestBitSet()
        {
            int result = -1;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    // data[i] contains a (new) highest bit
                    result = i * 64 + BitOperations.Log2(data[i]);
                }
            }

            return result;
        }

        public int CompareTo(Bits b)
        {
            if (this == b)
            {
                return 0;
            }
            return this < b ? -1 : 1;
        }

        public override string ToString()
        {
            var bitsString = new System.Text.StringBuilder(8);

            foreach (ulong bits in data)
            {
                bitsString.Append(Convert.ToString((long)bits, 2));
            }

            return bitsString.ToString();
        }
    }
}