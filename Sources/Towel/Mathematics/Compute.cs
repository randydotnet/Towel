﻿using Towel;
using Towel.DataStructures;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Towel.Measurements;
using System.ComponentModel;

namespace Towel.Mathematics
{
	/// <summary>Static Generic Mathematics Computation.</summary>
	public static class Compute
	{
        #region Pi

        public static T Pi<T>(int maxIterations = 100)
        {
            // Series: PI = 2 * (1 + 1/3 * (1 + 2/5 * (1 + 3/7 * (...))))
            // more terms in computation inproves accuracy

            T pi = Constant<T>.One;
            T previous = Constant<T>.Zero;
            for (int i = 1; NotEqual(previous, pi) && i < maxIterations; i++)
            {
                previous = pi;
                pi = Constant<T>.One;
                for (int j = i; j >= 1; j--)
                {
                    #region Without Custom Runtime Compilation

                    //T J = FromInt32<T>(j);
                    //T a = Add(Multiply(Constant<T>.Two, J), Constant<T>.One);
                    //T b = Divide(J, a);
                    //T c = Multiply(b, pi);
                    //T d = Add(Constant<T>.One, c);
                    //pi = d;

                    #endregion

                    pi = AddMultiplyDivideAddImplementation<T>.Function(FromInt32<T>(j), pi);
                }
                pi = Multiply(Constant<T>.Two, pi);
            }
            pi = Maximum(pi, Constant<T>.Three);
            return pi;
        }

        internal static class AddMultiplyDivideAddImplementation<T>
        {
            internal static Func<T, T, T> Function = (T j, T pi) =>
            {
                ParameterExpression J = Expression.Parameter(typeof(T));
                ParameterExpression PI = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Add(
                    Expression.Constant(Constant<T>.One),
                    Expression.Multiply(
                        PI,
                        Expression.Divide(
                            J,
                            Expression.Add(
                                Expression.Multiply(
                                    Expression.Constant(Constant<T>.Two),
                                    J),
                                Expression.Constant(Constant<T>.One)))));
                Function = Expression.Lambda<Func<T, T, T>>(BODY, J, PI).Compile();
                return Function(j, pi);
            };
        }

        #endregion

        #region Epsilon

        private static T ComputeEpsilon<T>()
        {
            if (typeof(T) == typeof(float))
            {
                return (T)(object)1.192092896e-012f;
            }
            throw new System.NotImplementedException();
        }

        #endregion

        #region FromInt32

        public static T FromInt32<T>(int a)
        {
            return FromInt32Implementation<T>.Function(a);
        }
        
        internal static class FromInt32Implementation<T>
        {
            internal static Func<int, T> Function = (int a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(int));
                Expression BODY = Expression.Convert(A, typeof(T));
                Function = Expression.Lambda<Func<int, T>>(BODY, A).Compile();
                return Function(a);
            };
        }
        
        #endregion

        #region ToInt32

        internal static int ToInt32<T>(T a)
        {
            return ToInt32Implementation<T>.Function(a);
        }

        internal static class ToInt32Implementation<T>
        {
            internal static Func<T, int> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Convert(A, typeof(int));
                Function = Expression.Lambda<Func<T, int>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region FromDouble

        public static T FromDouble<T>(double a)
        {
            return FromDoubleImplementation<T>.Function(a);
        }

        internal static class FromDoubleImplementation<T>
        {
            internal static Func<double, T> Function = (double a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(double));
                Expression BODY = Expression.Convert(A, typeof(T));
                Function = Expression.Lambda<Func<double, T>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region ToDouble

        internal static double ToDouble<T>(T a)
        {
            return ToDoubleImplementation<T>.Function(a);
        }

        internal static class ToDoubleImplementation<T>
        {
            internal static Func<T, double> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Convert(A, typeof(double));
                Function = Expression.Lambda<Func<T, double>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region Negate

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T NEGATE<T>(int a)
        {
            return Negate(FromInt32<T>(a));
        }

        public static T Negate<T>(T a)
		{
			return NegateImplementation<T>.Function(a);
		}

        internal static class NegateImplementation<T>
        {
            internal static Func<T, T> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Negate(A);
                Function = Expression.Lambda<Func<T, T>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region Add

        #region Syntax Sugar For Generic Constant Declaration

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T ADD<T>(T a, int b)
        {
            return Add(a, FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T ADD<T>(int a, int b)
        {
            return Add(FromInt32<T>(a), FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T ADD<T>(int a, T b)
        {
            return Add(FromInt32<T>(a), b);
        }

        #endregion

        public static T Add<T>(T a, T b)
        {
            return AddImplementation<T>.Function(a, b);
        }

        public static T Add<T>(T a, T b, T c, params T[] d)
        {
            return Add((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Add<T>(Stepper<T> stepper)
        {
            T result = Constant<T>.Zero;
            stepper(a => result = Add(result, a));
            return result;
        }

        internal static class AddImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Add(A, B);
                Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Subtract

        #region Syntax Sugar For Generic Constant Declaration

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T SUBTRACT<T>(T a, int b)
        {
            return Subtract(a, FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T SUBTRACT<T>(int a, int b)
        {
            return Subtract(FromInt32<T>(a), FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T SUBTRACT<T>(int a, T b)
        {
            return Subtract(FromInt32<T>(a), b);
        }

        #endregion

        public static T Subtract<T>(T a, T b)
        {
            return SubtractImplementation<T>.Function(a, b);
        }

        public static T Subtract<T>(T a, T b, T c, params T[] d)
        {
            return Subtract((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Subtract<T>(Stepper<T> stepper)
        {
            T result = Constant<T>.Zero;
            bool assigned = false;
            Step<T> step = (a) =>
            {
                if (assigned)
                {
                    result = Subtract(result, a);
                }
                else
                {
                    result = a;
                    assigned = true;
                }
            };
            stepper(step);
            return result;
        }

        internal static class SubtractImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Subtract(A, B);
                Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Multiply

        #region Syntax Sugar For Generic Constant Declaration

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T MULTIPLY<T>(T a, int b)
        {
            return Multiply(a, FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T MULTIPLY<T>(int a, int b)
        {
            return Multiply(FromInt32<T>(a), FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T MULTIPLY<T>(int a, T b)
        {
            return Multiply(FromInt32<T>(a), b);
        }

        #endregion

        public static T Multiply<T>(T a, T b)
        {
            return MultiplyImplementation<T>.Function(a, b);
        }

        public static T Multiply<T>(T a, T b, T c, params T[] d)
        {
            return Multiply((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Multiply<T>(Stepper<T> stepper)
        {
            T result = Constant<T>.One;
            stepper(a => result = Multiply(result, a));
            return result;
        }

        internal static class MultiplyImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Multiply(A, B);
                Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Divide

        #region Syntax Sugar For Generic Constant Declaration

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T DIVIDE<T>(T a, int b)
        {
            return Divide(a, FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T DIVIDE<T>(int a, int b)
        {
            return Divide(FromInt32<T>(a), FromInt32<T>(b));
        }

        /// <summary>
        /// Syntax sugar for generic constant declaration. I kinda want to keep this "internal."
        /// If made "public," it could be optimized using it's own delegate.
        /// </summary>
        internal static T DIVIDE<T>(int a, T b)
        {
            return Divide(FromInt32<T>(a), b);
        }

        #endregion

        public static T Divide<T>(T a, T b)
        {
            return DivideImplementation<T>.Function(a, b);
        }

        public static T Divide<T>(T a, T b, T c, params T[] d)
        {
            return Divide((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Divide<T>(Stepper<T> stepper)
        {
            T result = Constant<T>.Zero;
            bool assigned = false;
            Step<T> step = (a) =>
            {
                if (assigned)
                {
                    result = Divide(result, a);
                }
                else
                {
                    result = a;
                    assigned = true;
                }
            };
            stepper(step);
            return result;
        }

        internal static class DivideImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Divide(A, B);
                Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Invert

		public static T Invert<T>(T a)
        {
            return InvertImplementation<T>.Function(a);
        }

        internal static class InvertImplementation<T>
        {
            internal static Func<T, T> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Divide(Expression.Constant(Constant<T>.One), A);
                Function = Expression.Lambda<Func<T, T>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region Modulo

        public static T Modulo<T>(T a, T b)
        {
            return ModuloImplementation<T>.Function(a, b);
        }

        public static T Modulo<T>(T a, T b, T c, params T[] d)
        {
            return Modulo((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Modulo<T>(Stepper<T> operands)
        {
            T result = Constant<T>.Zero;
            bool assigned = false;
            Step<T> step = (a) =>
            {
                if (assigned)
                {
                    result = Modulo(result, a);
                }
                else
                {
                    result = a;
                    assigned = true;
                }
            };
            operands(step);
            return result;
        }

        internal static class ModuloImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Modulo(A, B);
                Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Power

        public static T Power<T>(T a, T b)
        {
            return PowerImplementation<T>.Function(a, b);
        }

        public static T Power<T>(T a, T b, T c, params T[] d)
        {
            return Power((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Power<T>(Stepper<T> operands)
        {
            T result = Constant<T>.Zero;
            Step<T> step = (a) =>
            {
                result = a;
                step = b => Power(result, b);
            };
            operands(step);
            return result;
        }

        internal static class PowerImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                // Note: this code needs to die.. but this works until it gets a better version

                // optimization for specific known types
                if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
                {
                    ParameterExpression A = Expression.Parameter(typeof(T));
                    ParameterExpression B = Expression.Parameter(typeof(T));
                    Expression BODY = Expression.Convert(Expression.Call(typeof(Math).GetMethod(nameof(Math.Pow)), Expression.Convert(A, typeof(double)), Expression.Convert(B, typeof(double))), typeof(T));
                    Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                }
                else
                {
                    Function = (T A, T B) =>
                    {
                        if (IsInteger(B) && IsPositive(B))
                        {
                            T result = A;
                            int power = ToInt32(B);
                            for (int i = 0; i < power; i++)
                            {
                                result = Multiply(result, A);
                            }
                            return result;
                        }
                        else
                        {
                            throw new NotImplementedException("This feature is still in development.");
                        }
                    };
                }
                return Function(a, b);
            };
        }

        #endregion

        #region SquareRoot

        public static T SquareRoot<T>(T a)
        {
            return Root(a, Constant<T>.Two);
        }
        
        //internal static class SquareRootImplementation<T>
        //{
        //    internal static Func<T, T> Function = (T a) =>
        //    {
        //        // optimization for specific known types
        //        if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
        //        {
        //            ParameterExpression A = Expression.Parameter(typeof(T));
        //            Expression BODY = Expression.Convert(Expression.Call(typeof(Math).GetMethod(nameof(Math.Sqrt)), Expression.Convert(A, typeof(double))), typeof(T));
        //            Function = Expression.Lambda<Func<T, T>>(BODY, A).Compile();
        //            return Function(a);
        //        }
        //        throw new NotImplementedException();
        //    };
        //}

        #endregion

        #region Root

        public static T Root<T>(T @base, T root)
		{
            return Power(@base, Invert(root));
        }

        #endregion

        #region Logarithm

        public static T Logarithm<T>(T value, T @base)
        {
            return LogarithmImplementation<T>.Function(value, @base);
        }

        internal static class LogarithmImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                throw new NotImplementedException();

                //ParameterExpression A = Expression.Parameter(typeof(T));
                //ParameterExpression B = Expression.Parameter(typeof(T));
                //Expression BODY = ;
                //Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                //return Function(a, b);
            };
        }

        #endregion

        #region IsInteger

        public static bool IsInteger<T>(T a)
        {
            return IsIntegerImplementation<T>.Function(a);
        }

        internal static class IsIntegerImplementation<T>
        {
            internal static Func<T, bool> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Equal(
                    Expression.Modulo(A, Expression.Constant(Constant<T>.One)),
                    Expression.Constant(Constant<T>.Zero));
                Function = Expression.Lambda<Func<T, bool>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region IsNonNegative

        public static bool IsNonNegative<T>(T a)
        {
            return IsNonNegativeImplementation<T>.Function(a);
        }

        internal static class IsNonNegativeImplementation<T>
        {
            internal static Func<T, bool> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.GreaterThanOrEqual(A, Expression.Constant(Constant<T>.Zero));
                Function = Expression.Lambda<Func<T, bool>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region IsNegative

        public static bool IsNegative<T>(T a)
        {
            return IsNegativeImplementation<T>.Function(a);
        }

        internal static class IsNegativeImplementation<T>
        {
            internal static Func<T, bool> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                LabelTarget RETURN = Expression.Label(typeof(bool));
                Expression BODY = Expression.LessThan(A, Expression.Constant(Constant<T>.Zero));
                Function = Expression.Lambda<Func<T, bool>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region IsPositive

        public static bool IsPositive<T>(T a)
        {
            return IsPositiveImplementation<T>.Function(a);
        }

        internal static class IsPositiveImplementation<T>
        {
            internal static Func<T, bool> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.GreaterThan(A, Expression.Constant(Constant<T>.Zero));
                Function = Expression.Lambda<Func<T, bool>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region IsEven

        public static bool IsEven<T>(T a)
        {
            return IsEvenImplementation<T>.Function(a);
        }

        internal static class IsEvenImplementation<T>
        {
            internal static Func<T, bool> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Equal(Expression.Modulo(A, Expression.Constant(Constant<T>.Two)), Expression.Constant(Constant<T>.Zero));
                Function = Expression.Lambda<Func<T, bool>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region IsOdd

        public static bool IsOdd<T>(T a)
        {
            return IsOddImplementation<T>.Function(a);
        }

        internal static class IsOddImplementation<T>
        {
            internal static Func<T, bool> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                Expression BODY = Expression.NotEqual(Expression.Modulo(A, Expression.Constant(Constant<T>.Two)), Expression.Constant(Constant<T>.Zero));
                Function = Expression.Lambda<Func<T, bool>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region IsPrime

        public static bool IsPrime<T>(T value)
        {
            if (Equal(Modulo(value, Constant<T>.One), Constant<T>.Zero))
            {
                if (Equal(value, Constant<T>.Two))
                    return true;
                T squareRoot = SquareRoot(value);
                int squareRootInt = ToInt32(squareRoot);
                for (int divisor = 3; divisor <= squareRootInt; divisor += 2)
                    if (Equal(Modulo<T>(value, FromInt32<T>(divisor)), Constant<T>.Zero))
                        return false;
                return true;
            }
            else
                return false;
            
            //return IsPrimeImplementation<T>.Function(value);
        }

        internal static class IsPrimeImplementation<T>
        {
            internal static Func<T, bool> Function = (T a) =>
            {
                if (IsInteger(a))
                {
                    if (Equal(a, Constant<T>.Two))
                    {
                        return true;
                    }
                    T squareRoot = SquareRoot(a);
                    for (T divisor = Constant<T>.Three; LessThanOrEqual(divisor, squareRoot); divisor = Add(divisor, Constant<T>.Two))
                    {
                        if (Equal(Modulo(a, divisor), Constant<T>.Zero))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            };
        }

        #endregion

        #region AbsoluteValue

        public static T AbsoluteValue<T>(T a)
        {
            return AbsoluteValueImplementation<T>.Function(a);
        }
        
        internal static class AbsoluteValueImplementation<T>
        {
            internal static Func<T, T> Function = (T a) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                LabelTarget RETURN = Expression.Label(typeof(T));
                Expression BODY = Expression.Block(
                    Expression.IfThenElse(
                        Expression.LessThan(A, Expression.Constant(Constant<T>.Zero)),
                        Expression.Return(RETURN, Expression.Negate(A)),
                        Expression.Return(RETURN, A)),
                    Expression.Label(RETURN, Expression.Constant(default(T))));
                Function = Expression.Lambda<Func<T, T>>(BODY, A).Compile();
                return Function(a);
            };
        }

        #endregion

        #region Maximum

        public static T Maximum<T>(T a, T b)
        {
            return MaximumImplementation<T>.Function(a, b);
        }

        public static T Maximum<T>(T a, T b, T c, params T[] d)
        {
            return Maximum((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Maximum<T>(Stepper<T> stepper)
        {
            T result = Constant<T>.Zero;
            Step<T> step = (a) =>
            {
                result = a;
                step = b => Maximum(result, a);
            };
            stepper(step);
            return result;
        }

        internal static class MaximumImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                LabelTarget RETURN = Expression.Label(typeof(T));
                Expression BODY = Expression.Block(
                    Expression.IfThenElse(
                        Expression.LessThan(A, B),
                        Expression.Return(RETURN, B),
                        Expression.Return(RETURN, A)),
                    Expression.Label(RETURN, Expression.Constant(default(T))));
                Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Minimum

        public static T Minimum<T>(T a, T b)
        {
            return MinimumImplementation<T>.Function(a, b);
        }

        public static T Minimum<T>(T a, T b, T c, params T[] d)
        {
            return Minimum((Step<T> step) => { step(a); step(b); step(c); d.Stepper()(step); });
        }

        public static T Minimum<T>(Stepper<T> stepper)
        {
            T result = Constant<T>.Zero;
            Step<T> step = (a) =>
            {
                result = a;
                step = b => Minimum(result, a);
            };
            stepper(step);
            return result;
        }

        internal static class MinimumImplementation<T>
        {
            internal static Func<T, T, T> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                LabelTarget RETURN = Expression.Label(typeof(T));
                Expression BODY = Expression.Block(
                    Expression.IfThenElse(
                        Expression.GreaterThan(A, B),
                        Expression.Return(RETURN, B),
                        Expression.Return(RETURN, A)),
                    Expression.Label(RETURN, Expression.Constant(default(T))));
                Function = Expression.Lambda<Func<T, T, T>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Clamp

        public static T Clamp<T>(T value, T minimum, T maximum)
        {
            return ClampImplementation<T>.Function(value, minimum, maximum);
        }

        internal static class ClampImplementation<T>
        {
            internal static Func<T, T, T, T> Function = (T value, T minimum, T maximum) =>
            {
                ParameterExpression VALUE = Expression.Parameter(typeof(T));
                ParameterExpression MINIMUM = Expression.Parameter(typeof(T));
                ParameterExpression MAXIMUM = Expression.Parameter(typeof(T));
                LabelTarget RETURN = Expression.Label(typeof(T));
                Expression BODY = Expression.Block(
                    Expression.IfThenElse(
                        Expression.LessThan(VALUE, MINIMUM),
                        Expression.Return(RETURN, MINIMUM),
                        Expression.IfThenElse(
                            Expression.GreaterThan(VALUE, MAXIMUM),
                            Expression.Return(RETURN, MAXIMUM),
                            Expression.Return(RETURN, VALUE))),
                    Expression.Label(RETURN, Expression.Constant(default(T))));
                Function = Expression.Lambda<Func<T, T, T, T>>(BODY, VALUE, MINIMUM, MAXIMUM).Compile();
                return Function(value, minimum, maximum);
            };
        }

        #endregion

        #region EqualLeniency

        public static bool EqualLeniency<T>(T a, T b, T leniency)
        {
            return EqualLeniencyImplementation<T>.Function(a, b, leniency);
        }

        internal static class EqualLeniencyImplementation<T>
        {
            internal static Func<T, T, T, bool> Function = (T a, T b, T c) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                ParameterExpression C = Expression.Parameter(typeof(T));
                ParameterExpression D = Expression.Variable(typeof(T));
                LabelTarget RETURN = Expression.Label(typeof(bool));
                Expression BODY = Expression.Block(new ParameterExpression[] { D },
                    Expression.Assign(D, Expression.Subtract(A, B)),
                    Expression.IfThenElse(
                        Expression.LessThan(D, Expression.Constant(Constant<T>.Zero)),
                        Expression.Assign(D, Expression.Negate(D)),
                        Expression.Assign(D, D)),
                    Expression.Return(RETURN, Expression.LessThanOrEqual(D, C), typeof(bool)),
                    Expression.Label(RETURN, Expression.Constant(default(bool))));
                Function = Expression.Lambda<Func<T, T, T, bool>>(BODY, A, B, C).Compile();
                return Function(a, b, c);
            };
        }

        #endregion

        #region Compare

        public static Comparison Compare<T>(T a, T b)
        {
            return CompareImplementation<T>.Function(a, b);
        }
        
        internal static class CompareImplementation<T>
        {
            internal static Func<T, T, Comparison> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                LabelTarget RETURN = Expression.Label(typeof(Comparison));
                Expression BODY = Expression.Block(
                    Expression.IfThen(
                            Expression.LessThan(A, B),
                            Expression.Return(RETURN, Expression.Constant(Comparison.Less))),
                        Expression.IfThen(
                            Expression.GreaterThan(A, B),
                            Expression.Return(RETURN, Expression.Constant(Comparison.Greater))),
                        Expression.Return(RETURN, Expression.Constant(Comparison.Equal)),
                        Expression.Label(RETURN, Expression.Constant(default(Comparison), typeof(Comparison))));
                //Expression.IfThenElse(
                //    Expression.LessThan(A, B),
                //    Expression.Return(RETURN, Expression.Constant(Comparison.Less)),
                //    Expression.IfThenElse(
                //        Expression.GreaterThan(A, B),
                //        Expression.Return(RETURN, Expression.Constant(Comparison.Greater)),
                //        Expression.Return(RETURN, Expression.Constant(Comparison.Equal)))),
                //Expression.Label(RETURN, Expression.Constant(default(Comparison), typeof(Comparison))));
                Function = Expression.Lambda<Func<T, T, Comparison>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region Equal

        public static bool Equal<T>(T a, T b)
        {
            return EqualImplementation<T>.Function(a, b);
        }

        public static bool Equal<T>(params T[] operands)
        {
            for (int i = 1; i < operands.Length; i++)
            {
                if (!Equal(operands[i - 1], operands[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Equal<T>(Stepper<T> stepper)
        {
            bool result = true;
            T a;
            Step<T> step = (b) =>
            {
                a = b;
                step = c =>
                {
                    if (!Equal(a, c))
                    {
                        result = false;
                    }
                    a = c;
                };
            };
            stepper(step);
            return result;
        }

        internal static class EqualImplementation<T>
        {
            internal static Func<T, T, bool> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.Equal(A, B);
                Function = Expression.Lambda<Func<T, T, bool>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region NotEqual

        public static bool NotEqual<T>(T a, T b)
        {
            return NotEqualImplementation<T>.Function(a, b);
        }

        public static bool NotEqual<T>(params T[] operands)
        {
            for (int i = 1; i < operands.Length; i++)
            {
                if (!NotEqual(operands[i - 1], operands[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool NotEqual<T>(Stepper<T> stepper)
        {
            bool result = true;
            T a;
            Step<T> step = (b) =>
            {
                a = b;
                step = c =>
                {
                    if (!NotEqual(a, c))
                    {
                        result = false;
                    }
                    a = c;
                };
            };
            stepper(step);
            return result;
        }

        internal static class NotEqualImplementation<T>
        {
            internal static Func<T, T, bool> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.NotEqual(A, B);
                Function = Expression.Lambda<Func<T, T, bool>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region LessThan

        public static bool LessThan<T>(T a, T b)
        {
            return LessThanImplementation<T>.Function(a, b);
        }

        public static bool LessThan<T>(params T[] operands)
        {
            for (int i = 1; i < operands.Length; i++)
            {
                if (!LessThan(operands[i - 1], operands[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool LessThan<T>(Stepper<T> stepper)
        {
            bool result = true;
            T a;
            Step<T> step = (b) =>
            {
                a = b;
                step = c =>
                {
                    if (!LessThan(a, c))
                    {
                        result = false;
                    }
                    a = c;
                };
            };
            stepper(step);
            return result;
        }

        internal static class LessThanImplementation<T>
        {
            internal static Func<T, T, bool> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.LessThan(A, B);
                Function = Expression.Lambda<Func<T, T, bool>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region GreaterThan

        public static bool GreaterThan<T>(T a, T b)
        {
            return GreaterThanImplementation<T>.Function(a, b);
        }

        public static bool GreaterThan<T>(params T[] operands)
        {
            for (int i = 1; i < operands.Length; i++)
            {
                if (!GreaterThan(operands[i - 1], operands[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool GreaterThan<T>(Stepper<T> stepper)
        {
            bool result = true;
            T a;
            Step<T> step = (b) =>
            {
                a = b;
                step = c =>
                {
                    if (!GreaterThan(a, c))
                    {
                        result = false;
                    }
                    a = c;
                };
            };
            stepper(step);
            return result;
        }

        internal static class GreaterThanImplementation<T>
        {
            internal static Func<T, T, bool> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.GreaterThan(A, B);
                Function = Expression.Lambda<Func<T, T, bool>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region LessThanOrEqual

        public static bool LessThanOrEqual<T>(T a, T b)
        {
            return LessThanOrEqualImplementation<T>.Function(a, b);
        }

        public static bool LessThanOrEqual<T>(params T[] operands)
        {
            for (int i = 1; i < operands.Length; i++)
            {
                if (!LessThanOrEqual(operands[i - 1], operands[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool LessThanOrEqual<T>(Stepper<T> stepper)
        {
            bool result = true;
            T a;
            Step<T> step = (b) =>
            {
                a = b;
                step = c =>
                {
                    if (!LessThanOrEqual(a, c))
                    {
                        result = false;
                    }
                    a = c;
                };
            };
            stepper(step);
            return result;
        }

        internal static class LessThanOrEqualImplementation<T>
        {
            internal static Func<T, T, bool> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.LessThanOrEqual(A, B);
                Function = Expression.Lambda<Func<T, T, bool>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region GreaterThanOrEqual

        public static bool GreaterThanOrEqual<T>(T a, T b)
        {
            return GreaterThanOrEqualImplementation<T>.Function(a, b);
        }

        public static bool GreaterThanOrEqual<T>(params T[] operands)
        {
            for (int i = 1; i < operands.Length; i++)
            {
                if (!GreaterThanOrEqual(operands[i - 1], operands[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool GreaterThanOrEqual<T>(Stepper<T> stepper)
        {
            bool result = true;
            T a;
            Step<T> step = (b) =>
            {
                a = b;
                step = c =>
                {
                    if (!GreaterThanOrEqual(a, c))
                    {
                        result = false;
                    }
                    a = c;
                };
            };
            stepper(step);
            return result;
        }

        internal static class GreaterThanOrEqualImplementation<T>
        {
            internal static Func<T, T, bool> Function = (T a, T b) =>
            {
                ParameterExpression A = Expression.Parameter(typeof(T));
                ParameterExpression B = Expression.Parameter(typeof(T));
                Expression BODY = Expression.GreaterThanOrEqual(A, B);
                Function = Expression.Lambda<Func<T, T, bool>>(BODY, A, B).Compile();
                return Function(a, b);
            };
        }

        #endregion

        #region GreatestCommonFactor

        public static T GreatestCommonFactor<T>(T a, T b, params T[] c)
        {
            return GreatestCommonFactor<T>((Step<T> step) => { step(a); step(b); c.Stepper()(step); });
        }

        public static T GreatestCommonFactor<T>(Stepper<T> stepper)
        {
            if (stepper == null)
            {
                throw new ArgumentNullException(nameof(stepper));
            }
            bool assigned = false;
            T answer = Constant<T>.Zero;
            stepper((T n) =>
            {
                if (n == null)
                {
                    throw new ArgumentNullException(nameof(stepper), nameof(stepper) + " contains null value(s).");
                }
                else if (Equal(n, Constant<T>.Zero))
                {
                    throw new MathematicsException("Encountered Zero (0) while computing the " + nameof(GreatestCommonFactor));
                }
                else if (!IsInteger(n))
                {
			        throw new MathematicsException(nameof(stepper) + " contains non-integer value(s).");
                }
                if (!assigned)
                {
                    answer = AbsoluteValue(n);
                    assigned = true;
                }
                else
                {
                    if (GreaterThan(answer, Constant<T>.One))
                    {
                        T a = answer;
                        T b = n;
                        while (NotEqual(b, Constant<T>.Zero))
                        {
                            T remainder = Modulo(a, b);
                            a = b;
                            b = remainder;
                        }
                        answer = AbsoluteValue(a);
                    }
                }
            });
            if (!assigned)
            {
                throw new ArgumentNullException(nameof(stepper), nameof(stepper) + " is empty.");
            }
            return answer;
        }

        #endregion

        #region LeastCommonMultiple

        public static T LeastCommonMultiple<T>(T a, T b, params T[] c)
        {
            return LeastCommonMultiple((Step<T> step) => { step(a); step(b); c.Stepper()(step); });
        }

        public static T LeastCommonMultiple<T>(Stepper<T> stepper)
        {
            if (stepper == null)
            {
                throw new ArgumentNullException(nameof(stepper));
            }
            bool assigned = false;
            T answer = Constant<T>.Zero;
            stepper((T n) =>
            {
                if (Equal(n, Constant<T>.Zero))
                {
                    answer = Constant<T>.Zero;
                    return;
                }
                if (!IsInteger(n))
                {
                    throw new MathematicsException(nameof(stepper) + " contains non-integer value(s).");
                }
                if (!assigned)
		        {
			        answer = AbsoluteValue(n);
			        assigned = true;
		        }
                if (GreaterThan(answer, Constant<T>.One))
                {
                    answer = AbsoluteValue(Multiply(Divide(answer, GreatestCommonFactor((Step<T> step) => { step(answer); step(n); })), n));
                }
            });
            if (!assigned)
            {
                throw new ArgumentNullException(nameof(stepper), nameof(stepper) + " is empty.");
            }
            return answer;
        }

        #endregion

        #region LinearInterpolation

        public static T LinearInterpolation<T>(T x, T x0, T x1, T y0, T y1)
        {
            if (GreaterThan(x0, x1) ||
                GreaterThan(x, x1) ||
                LessThan(x, x0))
            {
                throw new MathematicsException("Arguments out of range !(" + nameof(x0) + " <= " + nameof(x) + " <= " + nameof(x1) + ") [" + x0 + " <= " + x + " <= " + x1 + "].");
            }
            if (Equal(x0, x1))
            {
                if (NotEqual(y0, y1))
                {
                    throw new MathematicsException("Arguments out of range (" + nameof(x0) + " == " + nameof(x1) +") but !(" + nameof(y0) + " != " + nameof(y1) + ") [" + y0 + " != " + y1 + "].");
                }
                else
                {
                    return y0;
                }
            }
            return Add(y0, Divide(Multiply(Subtract(x, x0), Subtract(y1, y0)), Subtract(x1, x0)));
        }

        #endregion

        #region Factorial

        public static T Factorial<T>(T a)
        {
            if (!IsInteger(a))
            {
                throw new ArgumentOutOfRangeException(nameof(a), a, "!" + nameof(a) + "." + nameof(IsInteger));
            }
            if (LessThan(a, Constant<T>.Zero))
            {
                throw new ArgumentOutOfRangeException(nameof(a), a, "!(" + nameof(a) + " >= 0)");
            }
            T result = Constant<T>.One;
            for (; GreaterThan(a, Constant<T>.One); a = Subtract(a, Constant<T>.One))
                result = Multiply(a, result);
            return result;
        }

        #endregion

        #region Combinations

        public static T Combinations<T>(T N, T[] n)
        {
            if (!IsInteger(N))
            {
                throw new ArgumentOutOfRangeException(nameof(N), N, "!(" + nameof(N) + "." + nameof(IsInteger) + ")");
            }
            T result = Factorial(N);
            T sum = Constant<T>.Zero;
            for (int i = 0; i < n.Length; i++)
            {
                if (!IsInteger(n[i]))
                {
                    throw new ArgumentOutOfRangeException(nameof(n) + "[" + i + "]", n[i], "!(" + nameof(n) + "[" + i + "]." + nameof(IsInteger) + ")");
                }
                result = Divide(result, Factorial(n[i]));
                sum = Add(sum, n[i]);
            }
            if (GreaterThan(sum, N))
            {
                throw new MathematicsException("Aurguments out of range !(" + nameof(N) + " < Add(" + nameof(n) + ") [" + N + " < " + sum + "].");
            }
            return result;
        }

        #endregion

        #region Choose

        public static T Choose<T>(T N, T n)
        {
            if (LessThan(N, Constant<T>.Zero))
            {
                throw new ArgumentOutOfRangeException(nameof(N), N, "!(" + nameof(N) + " >= 0)");
            }
            if (!IsInteger(N))
            {
                throw new ArgumentOutOfRangeException(nameof(N), N, "!(" + nameof(N) + "." + nameof(IsInteger) + ")");
            }
            if (!IsInteger(n))
            {
                throw new ArgumentOutOfRangeException(nameof(n), n, "!(" + nameof(n) + "." + nameof(IsInteger) + ")");
            }
            if (LessThan(N, n))
            {
                throw new MathematicsException("Arguments out of range !(" + nameof(N) + " <= " + nameof(n) + ") [" + N + " <= " + n + "].");
            }
            return Divide(Factorial(N), Factorial(Subtract(N, n)));
        }

        #endregion

        #region Mode

        public static Heap<Link<T, int>> Mode<T>(T a, params T[] b)
        {
            return Mode((Step<T> step) => { step(a); b.Stepper()(step); });
        }

        public static Heap<Link<T, int>> Mode<T>(Stepper<T> stepper)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Mean

        public static T Mean<T>(T a, params T[] b)
        {
            return Mean((Step<T> step) => { step(a); b.Stepper()(step); });
        }

        public static T Mean<T>(Stepper<T> stepper)
        {
            T i = Constant<T>.Zero;
            T sum = Constant<T>.Zero;
            stepper((T step) => { i = Add(i, Constant<T>.One); sum = Add(sum, step); });
            return Divide(sum, i);
        }

        #endregion

        #region Median

        public static T Median<T>(Compare<T> compare, Hash<T> hash, Equate<T> equate, params T[] values)
        {
            //// this is an optimized median algorithm, but it only works on odd sets without duplicates
            //if (hash != null && equate != null && values.Length % 2 == 1 && !values.Stepper().ContainsDuplicates(equate, hash))
            //{
            //    int medianIndex = 0;
            //    OddNoDupesMedianImplementation(values, values.Length, ref medianIndex, compare);
            //    return values[medianIndex];
            //}

            // standard algorithm (sort and grab middle value)
            Algorithms.Sort<T>.Merge(compare, values);
            if (values.Length % 2 == 1) // odd... just grab middle value
            {
                return values[values.Length / 2];
            }
            else // even... must perform a mean of the middle two values
            {
                T leftMiddle = values[(values.Length / 2) - 1];
                T rightMiddle = values[values.Length / 2];
                return Divide(Add(leftMiddle, rightMiddle), Constant<T>.Two);
            }
        }

        public static T Median<T>(Compare<T> compare, Hash<T> hash, Equate<T> equate, Stepper<T> stepper)
        {
            return Median(compare, hash, equate, stepper.ToArray());
        }

        public static T Median<T>(Compare<T> compare, params T[] values)
        {
            return Median(compare, Hash.Default, Equate.Default, values);
        }

        public static T Median<T>(Compare<T> compare, Stepper<T> stepper)
        {
            return Median<T>(compare, Hash.Default, Equate.Default, stepper.ToArray());
        }

        public static T Median<T>(params T[] values)
        {
            return Median(Towel.Compare.Default, Hash.Default, Equate.Default, values);
        }

        public static T Median<T>(Stepper<T> stepper)
        {
            return Median(Towel.Compare.Default, Hash.Default, Equate.Default, stepper.ToArray());
        }

        /// <summary>Fast algorithm for median computation, but only works on data with an odd number of values without duplicates.</summary>
        private static void OddNoDupesMedianImplementation<T>(T[] a, int n, ref int k, Compare<T> compare)
        {
            int L = 0;
            int R = n - 1;
            k = n / 2;
            int i; int j;
            while (L < R)
            {
                T x = a[k];
                i = L; j = R;
                OddNoDupesMedianImplementation_Split(a, n, x, ref i, ref j, compare);
                if (j <= k) L = i;
                if (i >= k) R = j;
            }
        }

        private static void OddNoDupesMedianImplementation_Split<T>(T[] a, int n, T x, ref int i, ref int j, Compare<T> compare)
        {
            do
            {
                while (compare(a[i], x) == Comparison.Less) i++;
                while (compare(a[j], x) == Comparison.Greater) j--;
                T t = a[i];
                a[i] = a[j];
                a[j] = t;
            } while (i < j);
        }

        #endregion

        #region GeometricMean

        public static T GeometricMean<T>(Stepper<T> stepper)
        {
            T multiple = Constant<T>.One;
            T count = Constant<T>.Zero;
            stepper(i =>
	        {
                count = Add(count, Constant<T>.One);
                multiple = Multiply(multiple, i);
            });
            return Root(multiple, count);
        }

        #endregion

        #region Variance

        public static T Variance<T>(Stepper<T> stepper)
        {
            T mean = Mean(stepper);
            T variance = Constant<T>.Zero;
            T count = Constant<T>.Zero;
            stepper(i =>
            {
                T i_minus_mean = Subtract(i, mean);
                variance = Add(variance, Multiply(i_minus_mean, i_minus_mean));
                count = Add(count, Constant<T>.One);
            });
            return Divide(variance, count);
        }

        #endregion

        #region StandardDeviation
        
        public static T StandardDeviation<T>(Stepper<T> stepper)
        {
            return SquareRoot(Variance(stepper));
        }

        #endregion

        #region MeanDeviation

        public static T MeanDeviation<T>(Stepper<T> stepper)
        {
            T mean = Mean(stepper);
            T temp = Constant<T>.Zero;
            T count = Constant<T>.Zero;
            stepper(i =>
            {
                temp = Add(temp, AbsoluteValue(Subtract(i, mean)));
                count = Add(count, Constant<T>.One);
            });
            return Divide(temp, count);
        }

        #endregion

        #region Range

        public static void Range<T>(out T minimum, out T maximum, Stepper<T> stepper)
        {
            T MINIMUM = default(T);
            T MAXIMUM = default(T);
            bool assigned = false;
            Step<T> step = i =>
            {
                if (assigned)
                {
                    MINIMUM = LessThan(i, MINIMUM) ? i : MINIMUM;
                    MAXIMUM = LessThan(MAXIMUM, i) ? i : MAXIMUM;
                }
                else
                {
                    MINIMUM = i;
                    MAXIMUM = i;
                    assigned = true;
                }
            };
            stepper(step);
            minimum = MINIMUM;
            maximum = MAXIMUM;
        }

        #endregion

        #region Quantiles

        public static T[] Quantiles<T>(int quantiles, Stepper<T> stepper)
        {
            if (quantiles < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantiles), quantiles, "!(" + nameof(quantiles) + " >= 1)");
            }
            int count = stepper.Count();
            T[] ordered = new T[count];
            int a = 0;
            stepper(i => { ordered[a++] = i; });
            Algorithms.Sort<T>.Quick(Compare, ordered);
            T[] resultingQuantiles = new T[quantiles + 1];
            resultingQuantiles[0] = ordered[0];
            resultingQuantiles[resultingQuantiles.Length - 1] = ordered[ordered.Length - 1];
            T QUANTILES_PLUS_1 = FromInt32<T>(quantiles + 1);
            T ORDERED_LENGTH = FromInt32<T>(ordered.Length);
            for (int i = 1; i < quantiles; i++)
            {
                T I = FromInt32<T>(i);
                T temp = Divide(ORDERED_LENGTH, Multiply<T>(QUANTILES_PLUS_1, I));
                if (IsInteger(temp))
                {
                    resultingQuantiles[i] = ordered[ToInt32(temp)];
                }
                else
                {
                    resultingQuantiles[i] = Divide(Add(ordered[ToInt32(temp)], ordered[ToInt32(temp) + 1]), Constant<T>.Two);
                }
            }
            return resultingQuantiles;
        }

        #endregion

        #region Correlation

        //        /// <summary>Computes the median of a set of values.</summary>
        //        private static Compute.Delegates.Correlation Correlation_private = (Stepper<T> a, Stepper<T> b) =>
        //        {
        //            throw new System.NotImplementedException("I introduced an error here when I removed the stepref off of structure. will fix soon");

        //            Compute.Correlation_private =
        //        Meta.Compile<Compute.Delegates.Correlation>(
        //        string.Concat(
        //        @"(Stepper<", Meta.ConvertTypeToCsharpSource(typeof(T)), "> _a, Stepper<", Meta.ConvertTypeToCsharpSource(typeof(T)), @"> _b) =>
        //{
        //	", Meta.ConvertTypeToCsharpSource(typeof(T)), " a_mean = Compute<", Meta.ConvertTypeToCsharpSource(typeof(T)), @">.Mean(_a);
        //	", Meta.ConvertTypeToCsharpSource(typeof(T)), " b_mean = Compute<", Meta.ConvertTypeToCsharpSource(typeof(T)), @">.Mean(_b);
        //	List<", Meta.ConvertTypeToCsharpSource(typeof(T)), "> a_temp = new List_Linked<", Meta.ConvertTypeToCsharpSource(typeof(T)), @">();
        //	_a((", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i) => { a_temp.Add(i - b_mean); });
        //	List<", Meta.ConvertTypeToCsharpSource(typeof(T)), "> b_temp = new List_Linked<", Meta.ConvertTypeToCsharpSource(typeof(T)), @">();
        //	_b((", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i) => { b_temp.Add(i - a_mean); });
        //	", Meta.ConvertTypeToCsharpSource(typeof(T)), "[] a_cross_b = new ", Meta.ConvertTypeToCsharpSource(typeof(T)), @"[a_temp.Count * b_temp.Count];
        //	int count = 0;
        //	a_temp.Stepper((", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i_a) =>
        //	{
        //		b_temp.Stepper((", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i_b) =>
        //		{
        //			a_cross_b[count++] = i_a * i_b;
        //		});
        //	});
        //	a_temp.Stepper((ref ", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i) => { i *= i; });
        //	b_temp.Stepper((ref ", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i) => { i *= i; });
        //	", Meta.ConvertTypeToCsharpSource(typeof(T)), @" sum_a_cross_b = 0;
        //	foreach (", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i in a_cross_b)
        //		sum_a_cross_b += i;
        //	", Meta.ConvertTypeToCsharpSource(typeof(T)), @" sum_a_temp = 0;
        //	a_temp.Stepper((", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i) => { sum_a_temp += i; });
        //	", Meta.ConvertTypeToCsharpSource(typeof(T)), @" sum_b_temp = 0;
        //	b_temp.Stepper((", Meta.ConvertTypeToCsharpSource(typeof(T)), @" i) => { sum_b_temp += i; });
        //	return sum_a_cross_b / Compute<", Meta.ConvertTypeToCsharpSource(typeof(T)), @">.sqrt(sum_a_temp * sum_b_temp);
        //}"));

        //            return Compute.Correlation_private(a, b);
        //        };

        //        public static T Correlation(Stepper<T> a, Stepper<T> b)
        //        {
        //            return Correlation_private(a, b);
        //        }
        #endregion

        #region Exponential
        
        /// <summary>Computes: [ e ^ a ].</summary>
        public static T Exponential<T>(T a)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region NaturalLogarithm

        public static T NaturalLogarithm<T>(T a)
        {
            return NaturalLogarithmImplementation<T>.Function(a);
        }

        /// <summary>Computes (natrual log): [ ln(n) ].</summary>
        internal static class NaturalLogarithmImplementation<T>
        {
            internal static Func<T, T> Function = (T a) =>
            {
                // optimization for specific known types
                if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
                {
                    ParameterExpression A = Expression.Parameter(typeof(T));
                    Expression BODY = Expression.Call(typeof(Math).GetMethod("Log"), A);
                    Function = Expression.Lambda<Func<T, T>>(BODY, A).Compile();
                    return Function(a);
                }
                throw new NotImplementedException();
            };
        }

        #endregion

        #region Sine

        /// <summary>Computes the sine ratio of an angle using the system's sine function. WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the sine ratio of.</param>
        /// <returns>The sine ratio of the provided angle.</returns>
        /// <remarks>WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</remarks>
        public static T SineSystem<T>(Angle<T> a)
        {
            return FromDouble<T>(Math.Sin(ToDouble(a.Radians)));
        }

        /// <summary>Estimates the sine ratio using piecewise quadratic equations. Fast but NOT very accurate.</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the quadratic estimated sine ratio of.</param>
        /// <returns>The quadratic estimation of the sine ratio of the provided angle.</returns>
        public static T SineQuadratic<T>(Angle<T> a)
        {
            // Piecewise Functions:
            // y = (-4/π^2)(x - (π/2))^2 + 1
            // y = (4/π^2)(x - (3π/2))^2 - 1

            T adjusted = Modulo(a.Radians, Constant<T>.Pi2);
            if (IsNegative(adjusted))
            {
                adjusted = Add(adjusted, Constant<T>.Pi2);
            }
            if (LessThan(adjusted, Constant<T>.Pi))
            {
                T xMinusPiOver2 = Subtract(adjusted, Constant<T>.PiOver2);
                T xMinusPiOver2Squared = Multiply(xMinusPiOver2, xMinusPiOver2);
                return Add(Multiply(Constant<T>.Negative4OverPiSquared, xMinusPiOver2Squared), Constant<T>.One);
            }
            else
            {
                T xMinus3PiOver2 = Subtract(adjusted, Constant<T>.Pi3Over2);
                T xMinus3PiOver2Squared = Multiply(xMinus3PiOver2, xMinus3PiOver2);
                return Subtract(Multiply(Constant<T>.FourOverPiSquared, xMinus3PiOver2Squared), Constant<T>.One);
            }
        }

        #endregion

        #region Cosine

        /// <summary>Computes the cosine ratio of an angle using the system's cosine function. WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the cosine ratio of.</param>
        /// <returns>The cosine ratio of the provided angle.</returns>
        /// <remarks>WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</remarks>
        public static T CosineSystem<T>(Angle<T> a)
        {
            return FromDouble<T>(Math.Cos(ToDouble(a)));
        }

        /// <summary>Estimates the cosine ratio using piecewise quadratic equations. Fast but NOT very accurate.</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the quadratic estimated cosine ratio of.</param>
        /// <returns>The quadratic estimation of the cosine ratio of the provided angle.</returns>
        public static T CosineQuadratic<T>(Angle<T> a)
        {
            Angle<T> piOver2Radians = new Angle<T>(Constant<T>.PiOver2, Angle.Units.Radians);
            return SineQuadratic(a - piOver2Radians);
        }

        #endregion

        #region Tangent

        /// <summary>Computes the tangent ratio of an angle using the system's tangent function. WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the tangent ratio of.</param>
        /// <returns>The tangent ratio of the provided angle.</returns>
        /// <remarks>WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</remarks>
        public static T TangentSystem<T>(Angle<T> a)
        {
            return FromDouble<T>(Math.Tan(ToDouble(a)));
        }

        /// <summary>Estimates the tangent ratio using piecewise quadratic equations. Fast but NOT very accurate.</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the quadratic estimated tangent ratio of.</param>
        /// <returns>The quadratic estimation of the tangent ratio of the provided angle.</returns>
        public static T TangentQuadratic<T>(Angle<T> a)
        {
            return Divide(SineQuadratic(a), CosineQuadratic(a));
        }

        #endregion

        #region Cosecant

        /// <summary>Computes the cosecant ratio of an angle using the system's sine function. WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the cosecant ratio of.</param>
        /// <returns>The cosecant ratio of the provided angle.</returns>
        /// <remarks>WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</remarks>
        public static T CosecantSystem<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, SineSystem(a));
        }

        /// <summary>Estimates the cosecant ratio using piecewise quadratic equations. Fast but NOT very accurate.</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the quadratic estimated cosecant ratio of.</param>
        /// <returns>The quadratic estimation of the cosecant ratio of the provided angle.</returns>
        public static T CosecantQuadratic<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, SineQuadratic(a));
        }

        #endregion

        #region Secant

        /// <summary>Computes the secant ratio of an angle using the system's cosine function. WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the secant ratio of.</param>
        /// <returns>The secant ratio of the provided angle.</returns>
        /// <remarks>WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</remarks>
        public static T SecantSystem<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, CosineSystem(a));
        }

        /// <summary>Estimates the secant ratio using piecewise quadratic equations. Fast but NOT very accurate.</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the quadratic estimated secant ratio of.</param>
        /// <returns>The quadratic estimation of the secant ratio of the provided angle.</returns>
        public static T SecantQuadratic<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, CosineQuadratic(a));
        }

        #endregion

        #region Cotangent

        /// <summary>Computes the cotangent ratio of an angle using the system's tangent function. WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the cotangent ratio of.</param>
        /// <returns>The cotangent ratio of the provided angle.</returns>
        /// <remarks>WARNING! CONVERSION TO/FROM DOUBLE (possible loss of significant figures).</remarks>
        public static T CotangentSystem<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, TangentSystem(a));
        }

        /// <summary>Estimates the cotangent ratio using piecewise quadratic equations. Fast but NOT very accurate.</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="a">The angle to compute the quadratic estimated cotangent ratio of.</param>
        /// <returns>The quadratic estimation of the cotangent ratio of the provided angle.</returns>
        public static T CotangentQuadratic<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, TangentQuadratic(a));
        }

        #endregion

        #region InverseSine

        //public static Angle<T> InverseSine<T>(T a)
        //{
        //    return InverseSineImplementation<T>.Function(a);
        //}

        //internal static class InverseSineImplementation<T>
        //{
        //    internal static Func<T, Angle<T>> Function = (T a) =>
        //    {
        //        // optimization for specific known types
        //        if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
        //        {
        //            ParameterExpression A = Expression.Parameter(typeof(T));
        //            Expression BODY = Expression.Call(typeof(Angle<T>).GetMethod(nameof(Angle<T>.Factory_Radians), BindingFlags.Static), Expression.Call(typeof(Math).GetMethod(nameof(Math.Asin)), A));
        //            Function = Expression.Lambda<Func<T, Angle<T>>>(BODY, A).Compile();
        //            return Function(a);
        //        }
        //        throw new NotImplementedException();
        //    };
        //}

        #endregion

        #region InverseCosine

        //public static Angle<T> InverseCosine<T>(T a)
        //{
        //    return InverseCosineImplementation<T>.Function(a);
        //}

        //internal static class InverseCosineImplementation<T>
        //{
        //    internal static Func<T, Angle<T>> Function = (T a) =>
        //    {
        //        // optimization for specific known types
        //        if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
        //        {
        //            ParameterExpression A = Expression.Parameter(typeof(T));
        //            Expression BODY = Expression.Call(typeof(Angle<T>).GetMethod(nameof(Angle<T>.Factory_Radians), BindingFlags.Static), Expression.Call(typeof(Math).GetMethod(nameof(Math.Acos)), A));
        //            Function = Expression.Lambda<Func<T, Angle<T>>>(BODY, A).Compile();
        //            return Function(a);
        //        }
        //        throw new NotImplementedException();
        //    };
        //}

        #endregion

        #region InverseTangent

        //public static Angle<T> InverseTangent<T>(T a)
        //{
        //    return InverseTangentImplementation<T>.Function(a);
        //}

        //internal static class InverseTangentImplementation<T>
        //{
        //    internal static Func<T, Angle<T>> Function = (T a) =>
        //    {
        //        // optimization for specific known types
        //        if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
        //        {
        //            ParameterExpression A = Expression.Parameter(typeof(T));
        //            Expression BODY = Expression.Call(typeof(Angle<T>).GetMethod(nameof(Angle<T>.Factory_Radians), BindingFlags.Static), Expression.Call(typeof(Math).GetMethod(nameof(Math.Atan)), A));
        //            Function = Expression.Lambda<Func<T, Angle<T>>>(BODY, A).Compile();
        //            return Function(a);
        //        }
        //        throw new NotImplementedException();
        //    };
        //}

        #endregion

        #region InverseCosecant

        //public static Angle<T> InverseCosecant<T>(T a)
        //{
        //    return Angle<T>.Factory_Radians(Divide(Constant<T>.One, InverseSine(a).Radians));
        //}

        #endregion

        #region InverseSecant

        //public static Angle<T> InverseSecant<T>(T a)
        //{
        //    return Angle<T>.Factory_Radians(Divide(Constant<T>.One, InverseCosine(a).Radians));
        //}

        #endregion

        #region InverseCotangent

        //public static Angle<T> InverseCotangent<T>(T a)
        //{
        //    return Angle<T>.Factory_Radians(Divide(Constant<T>.One, InverseTangent(a).Radians));
        //}

        #endregion

        #region HyperbolicSine

        public static T HyperbolicSine<T>(Angle<T> a)
        {
            return HyperbolicSineImplementation<T>.Function(a);
        }

        internal static class HyperbolicSineImplementation<T>
        {
            internal static Func<Angle<T>, T> Function = (Angle<T> a) =>
            {
                // optimization for specific known types
                if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
                {
                    ParameterExpression A = Expression.Parameter(typeof(T));
                    Expression BODY = Expression.Call(typeof(Math).GetMethod(nameof(Math.Sinh)), Expression.Convert(Expression.Property(A, typeof(Angle<T>).GetProperty(nameof(a.Radians))), typeof(double)));
                    Function = Expression.Lambda<Func<Angle<T>, T>>(BODY, A).Compile();
                    return Function(a);
                }
                throw new NotImplementedException();
            };
        }

        #endregion

        #region HyperbolicCosine

        public static T HyperbolicCosine<T>(Angle<T> a)
        {
            return HyperbolicCosineImplementation<T>.Function(a);
        }

        internal static class HyperbolicCosineImplementation<T>
        {
            internal static Func<Angle<T>, T> Function = (Angle<T> a) =>
            {
                // optimization for specific known types
                if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
                {
                    ParameterExpression A = Expression.Parameter(typeof(T));
                    Expression BODY = Expression.Call(typeof(Math).GetMethod(nameof(Math.Cosh)), Expression.Convert(Expression.Property(A, typeof(Angle<T>).GetProperty(nameof(a.Radians))), typeof(double)));
                    Function = Expression.Lambda<Func<Angle<T>, T>>(BODY, A).Compile();
                    return Function(a);
                }
                throw new NotImplementedException();
            };
        }

        #endregion

        #region HyperbolicTangent

        public static T HyperbolicTangent<T>(Angle<T> a)
        {
            return HyperbolicTangentImplementation<T>.Function(a);
        }

        internal static class HyperbolicTangentImplementation<T>
        {
            internal static Func<Angle<T>, T> Function = (Angle<T> a) =>
            {
                // optimization for specific known types
                if (TypeDescriptor.GetConverter(typeof(T)).CanConvertTo(typeof(double)))
                {
                    ParameterExpression A = Expression.Parameter(typeof(T));
                    Expression BODY = Expression.Call(typeof(Math).GetMethod(nameof(Math.Tanh)), Expression.Convert(Expression.Property(A, typeof(Angle<T>).GetProperty(nameof(a.Radians))), typeof(double)));
                    Function = Expression.Lambda<Func<Angle<T>, T>>(BODY, A).Compile();
                    return Function(a);
                }
                throw new NotImplementedException();
            };
        }

        #endregion

        #region HyperbolicCosecant

        public static T HyperbolicCosecant<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, HyperbolicSine(a));
        }

        #endregion

        #region HyperbolicSecant

        public static T HyperbolicSecant<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, HyperbolicCosine(a));
        }

        #endregion

        #region HyperbolicCotangent

        public static T HyperbolicCotangent<T>(Angle<T> a)
        {
            return Divide(Constant<T>.One, HyperbolicTangent(a));
        }

        #endregion

        #region InverseHyperbolicSine

        public static Angle<T> InverseHyperbolicSine<T>(T a)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region InverseHyperbolicCosine

        public static Angle<T> InverseHyperbolicCosine<T>(T a)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region InverseHyperbolicTangent

        public static Angle<T> InverseHyperbolicTangent<T>(T a)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region InverseHyperbolicCosecant

        public static Angle<T> InverseHyperbolicCosecant<T>(T a)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region InverseHyperbolicSecant

        public static Angle<T> InverseHyperbolicSecant<T>(T a)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region InverseHyperbolicCotangent

        public static Angle<T> InverseHyperbolicCotangent<T>(T a)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region LinearRegression2D

        /// <summary>Computes the best fit line from a set of points in 2D space [y = slope * x + y_intercept].</summary>
        /// <typeparam name="T">The numeric type of the operation.</typeparam>
        /// <param name="points">The points to compute the best fit line of.</param>
        /// <param name="slope">The slope of the computed best fit line [y = slope * x + y_intercept].</param>
        /// <param name="y_intercept">The y intercept of the computed best fit line [y = slope * x + y_intercept].</param>
        public static void LinearRegression2D<T>(Stepper<T, T> points, out T slope, out T y_intercept)
        {
            if (points is null)
            {
                throw new ArgumentNullException(nameof(points));
            }
            int count = 0;
            T SUMX = Constant<T>.Zero;
            T SUMY = Constant<T>.Zero;
            points((x, y) =>
            {
                SUMX = Add(SUMX, x);
                SUMY = Add(SUMY, y);
                count++;
            });
            if (count < 2)
            {
                throw new MathematicsException("Argument Invalid !(" + nameof(points) + ".Count >= 2)");
            }
            T COUNT = FromInt32<T>(count);
            T MEANX = Divide(SUMX, COUNT);
            T MEANY = Divide(SUMY, COUNT);
            T VARIANCEX = Constant<T>.Zero;
            T VARIANCEY = Constant<T>.Zero;
            points((x, y) =>
            {
                T offset = Subtract(x, MEANX);
                VARIANCEY = Add(VARIANCEY, Multiply(offset, Subtract(y, MEANY)));
                VARIANCEX = Add(VARIANCEX, Multiply(offset, offset));
            });
            slope = Divide(VARIANCEY, VARIANCEX);
            y_intercept = Subtract(MEANY, Multiply(slope, MEANX));
        }

        #endregion

        #region FactorPrimes

        public static void FactorPrimes<T>(T a, Step<T> step)
        {
            T A = a;
            if (!IsInteger(A))
            {
                throw new ArgumentOutOfRangeException(nameof(A), A, "!(" + nameof(A) + "." + nameof(IsInteger) + ")");
            }
            if (IsNegative(A))
            {
                A = AbsoluteValue(A);
                step(FromInt32<T>(-1));
            }
            while (IsEven(A))
        	{
                step(Constant<T>.Two);
                A = Divide(A, Constant<T>.Two);
            }
            for (T i = Constant<T>.Three; LessThanOrEqual(i, SquareRoot(A)); i = Add(i, Constant<T>.Two))
            {
                while (Equal(Modulo(A, i), Constant<T>.Zero))
                {
                    step(i);
                    A = Divide(A, i);
                }
            }
            if (GreaterThan(A, Constant<T>.Two))
            {
                step(A);
            }
        }

        #endregion
    }
}
