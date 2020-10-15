using System;
using System.Numerics;

namespace Incapsulation.RationalNumbers
{
    public class Rational
    {
        public Rational(int numerator)
        {
            Numerator = numerator;
            Denominator = 1;
        }

        public Rational(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                IsNan = true;
                return;
            }

            Numerator = numerator;
            Denominator = denominator;

            Simplify();
            MoveMinusToNumerator();
        }

        private void Simplify()
        {
            var gcd = (int)BigInteger.GreatestCommonDivisor(Numerator, Denominator);

            Numerator /= gcd;
            Denominator /= gcd;
        }

        private void MoveMinusToNumerator()
        {
            if (Denominator < 0)
            {
                Numerator *= -1;
                Denominator *= -1;
            }
        }

        public int Numerator { get; private set; }
        public int Denominator { get; private set; }
        public bool IsNan { get; }

        public static Rational Nan => new Rational(0, 0);

        public Rational Reverse()
        {
            return new Rational(Denominator, Numerator);
        }

        public static Rational operator +(Rational n1, Rational n2)
        {
            if (n1.IsNan || n2.IsNan) return Nan;

            var gcd = (int) BigInteger.GreatestCommonDivisor(n1.Denominator, n2.Denominator);
            var lcm = n1.Denominator * n2.Denominator / gcd;

            var m1 = n2.Denominator / gcd;
            var m2 = n1.Denominator / gcd;

            return new Rational(m1 * n1.Numerator + m2 * n2.Numerator, lcm);
        }

        public static Rational operator *(Rational n1, Rational n2)
        {
            if (n1.IsNan || n2.IsNan) return Nan;

            return new Rational(n1.Numerator * n2.Numerator, n1.Denominator * n2.Denominator);
        }

        public static Rational operator /(Rational n1, Rational n2)
        {
            if (n1.IsNan || n2.IsNan) return Nan;

            return n1 * n2.Reverse();
        }

        public static Rational operator -(Rational n)
        {
            return -1 * n;
        }

        public static Rational operator -(Rational n1, Rational n2)
        {
            return n1 + -n2;
        }

        public static implicit operator Rational(int numerator)
        {
            return new Rational(numerator);
        }

        public static explicit operator int(Rational number)
        {
            if (number.Numerator % number.Denominator != 0) throw new Exception();

            return number.Numerator / number.Denominator;
        }

        public static implicit operator double(Rational number)
        {
            if (number.IsNan) return double.NaN;

            return number.Numerator / (double)number.Denominator;
        }
    }
}