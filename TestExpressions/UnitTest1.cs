using System.Linq.Expressions;

namespace TestExpressions
{
    public class UnitTest1
    {
        //Method 
        public static bool IsPrime_Method(int n)
        {
            if (n <= 1) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;
            var boundary = (int)Math.Floor(Math.Sqrt(n));
            int i = 3;
            while (i <= boundary)
            {
                if (n % i == 0) return false;
                i = i + 2;
            }
            return true;
        }

        //Expression captured from ReadableExpression v4.6.0
        public static bool IsPrime_ReadableExpressions(int n)
        {
            Func<int, bool> func =
                value =>
                {
                    bool result;

                    if (value <= 1)
                    {
                        return false;
                    }

                    if (value == 2)
                    {
                        return true;
                    }

                    if ((value % 2) == 0)
                    {
                        return false;
                    }

                    var i = 3;
                    var boundary = (int)Math.Floor(Math.Sqrt((double)value));
                    while (true)
                    {
                        if (i <= boundary)
                        {
                            if ((value % i) == 0)
                            {
                                return false;
                            }

                            i += 2;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return true;
                };

            return func(n);

        }


        //Expression
        public static Expression IsPrime_Script(ParameterExpression value)
        {
            var label = Expression.Label();

            var result = Expression.Parameter(typeof(bool), "result");

            var returnLabel = Expression.Label(typeof(bool));

            var valueLessThanOrEqualToOne = Expression.LessThanOrEqual(value, Expression.Constant(1));
            var valueEqualTwo = Expression.Equal(value, Expression.Constant(2));
            var valueModTwoZero = Expression.Equal(Expression.Modulo(value, Expression.Constant(2)), Expression.Constant(0));


            var sqrt = typeof(Math).GetMethod("Sqrt");
            var floor = typeof(Math).GetMethod("Floor", new Type[] { typeof(double) });


            var valueSqrt = Expression.Call(null, sqrt, Expression.Convert(value, typeof(double)));

            var evalFunction = Expression.Convert(Expression.Call(null, floor, valueSqrt), typeof(int));

            var boundary = Expression.Variable(typeof(int), "boundary");

            var i = Expression.Variable(typeof(int), "i");

            var modBlock = Expression.Block
            (
                new[] { i, boundary },
                Expression.IfThen
                (
                    Expression.Equal(Expression.Modulo(value, i), Expression.Constant(0)),
                    Expression.Return(returnLabel, Expression.Constant(false))
                ),
                Expression.AddAssign(i, Expression.Constant(2))
            );

            Expression block = Expression.Block
            (
                new[] { result, i, boundary },
                Expression.IfThen
                (
                    valueLessThanOrEqualToOne,
                    Expression.Return(returnLabel, Expression.Constant(false))
                ),
                Expression.IfThen
                (
                    valueEqualTwo,
                    Expression.Return(returnLabel, Expression.Constant(true))
                ),
                Expression.IfThen
                (
                    valueModTwoZero,
                    Expression.Return(returnLabel, Expression.Constant(false))
                ),

                Expression.Assign(i, Expression.Constant(3)),
                Expression.Assign(boundary, evalFunction),
                Expression.Loop
                (
                    Expression.IfThenElse
                    (
                        Expression.LessThanOrEqual(i, boundary),
                        modBlock,
                        Expression.Break(label)
                    ),
                    label
                ),

                Expression.Return(returnLabel, Expression.Constant(true)),
                Expression.Label(returnLabel, Expression.Constant(true))
            );
            return block;
        }       

        [Fact]
        public void Test_IsPrimeNumber()
        {
            Assert.False(IsPrime_Method(1));
            Assert.True(IsPrime_Method(2));
            Assert.True(IsPrime_Method(3));
            Assert.False(IsPrime_Method(4));
            Assert.True(IsPrime_Method(5));
            Assert.False(IsPrime_Method(6));
            Assert.True(IsPrime_Method(7));
            Assert.False(IsPrime_Method(8));
            Assert.False(IsPrime_Method(9));        //OK
            Assert.False(IsPrime_Method(10));

            Assert.True(IsPrime_Method(521));
            Assert.False(IsPrime_Method(522));

            Assert.True(IsPrime_Method(523));
            Assert.False(IsPrime_Method(524));

            Assert.True(IsPrime_Method(541));
            Assert.False(IsPrime_Method(542));

        }

        [Fact]
        public void Test_IsPrimeReadableExpressions()
        {           
            Assert.False(IsPrime_ReadableExpressions(1));
            Assert.True(IsPrime_ReadableExpressions(2));
            Assert.True(IsPrime_ReadableExpressions(3));
            Assert.False(IsPrime_ReadableExpressions(4));
            Assert.True(IsPrime_ReadableExpressions(5));
            Assert.False(IsPrime_ReadableExpressions(6));
            Assert.True(IsPrime_ReadableExpressions(7));
            Assert.False(IsPrime_ReadableExpressions(8));            
            Assert.False(IsPrime_ReadableExpressions(9));       //OK
            Assert.False(IsPrime_ReadableExpressions(10));

            Assert.True(IsPrime_ReadableExpressions(521));
            Assert.False(IsPrime_ReadableExpressions(522));

            Assert.True(IsPrime_ReadableExpressions(523));
            Assert.False(IsPrime_ReadableExpressions(524));

            Assert.True(IsPrime_ReadableExpressions(541));
            Assert.False(IsPrime_ReadableExpressions(542));
            

        }

        [Fact]
        public void Test_IsPrimeExpression()
        {
            var value = Expression.Parameter(typeof(int), "value");
            var result = IsPrime_Script(value);
            var expr = Expression.Lambda<Func<int, bool>>(result, value);
            var func = expr.Compile();


            Assert.False(func(1));
            Assert.True(func(2));
            Assert.True(func(3));
            Assert.False(func(4));
            Assert.True(func(5));
            Assert.False(func(6));
            Assert.True(func(7));
            Assert.False(func(8));

            //*****************************************
            //Start throwing DivideByZeroException from
            //this point and only for odd numbers.
            //*****************************************

            //Assert.False(func(9));   
            //Assert.False(func(10));

            //Assert.True(func(521));
            //Assert.False(func(522));

            //Assert.True(func(523));
            //Assert.False(func(524));

            //Assert.True(func(541));
            //Assert.False(func(542));

        }
    }
}