using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Memory.Timers
{
    [TestFixture]
    public class Timer_Tests
    {
        List<int> Validate(string expected, string actual)
        {
            expected = expected.Replace("#", "(\\d+)");
            var regex = new Regex(expected);
            var match = regex.Match(actual);
            if (!match.Success)
                Assert.Fail($"Your string \n{actual} does not match pattern {expected}");
            var result = match.Groups.Cast<Group>().Skip(1).Select(z => int.Parse(z.Value)).ToList();
            return result;
        }

        [Test]
        public void SingleLineReport()
        {
            var writer = new StringWriter();
            using (Timer.Start(writer))
            {
            }
            Validate(@"\*\s+: (\d+)\n", writer.ToString());
/*Пример ответа
*                   : 0
*/
        }

        [Test]
        public void CustomTimerName()
        {
            var writer = new StringWriter();
            using (Timer.Start(writer, "MyTimer"))
            { }
            Validate(@"MyTimer\s+: (\d+)\n", writer.ToString());
/*Пример ответа
MyTimer             : 0
*/
        }

        [Test]
        public void Nesting()
        {
            var writer = new StringWriter();
            using (var timer = Timer.Start(writer, "A"))
            {
                using (timer.StartChildTimer("B"))
                { }
                using (timer.StartChildTimer("C"))
                { }
            }
            Validate(@"A\s+: (\d+)\n[ ]{4}B\s+: (\d+)\n[ ]{4}C\s+: (\d+)\n[ ]{4}Rest\s+: (\d+)",
                     writer.ToString());
/* Пример ответа
A                   : 0
    B               : 0
    C               : 0
    Rest            : 0
*/
        }

        [Test]
        public void DeepNestingStructure1()
        {
            var writer = new StringWriter();
            using (var timerA = Timer.Start(writer,"A"))
            {
                using (var timerB = timerA.StartChildTimer("B"))
                {
                    using (timerB.StartChildTimer("C"))
                    { }
                }
            }
            Validate(@"A\s+: (\d+)\n[ ]{4}B\s+: (\d+)\n[ ]{8}C\s+: (\d+)\n[ ]{8}Rest\s+: (\d+)\n[ ]{4}Rest\s+: (\d+)\n",
                     writer.ToString());

/* Пример ответа
A                   : 0
    B               : 0
        C           : 0
        Rest        : 0
    Rest            : 0
*/
        }
        
        [Test]
        public void DeepNestingStructure2()
        {
            var writer = new StringWriter();
            using (var timer = Timer.Start(writer, "A"))
            {
                using (timer.StartChildTimer("B"))
                { }
                using (var timer2 = timer.StartChildTimer("C"))
                { 
                    using(timer2.StartChildTimer("D"))
                    {

                    }
                }
            }
            Validate(@"A\s+: (\d+)\n[ ]{4}B\s+: (\d+)\n[ ]{4}C\s+: (\d+)\n[ ]{8}D\s+: (\d+)\n[ ]{8}Rest\s+: (\d+)\n[ ]{4}Rest\s+: (\d+)\n",
                writer.ToString());

/* Пример ответа
A                   : 0
    B               : 0
    C               : 0
        D           : 0
        Rest        : 0
    Rest            : 0
*/
        }
        
        [Test]
        public void DeepNestingStructure3()
        {
            var writer = new StringWriter();
            using (var timer = Timer.Start(writer, "A"))
            {
                using (timer.StartChildTimer("B"))
                { }
                using (var timer2 = timer.StartChildTimer("C"))
                { 
                    using(timer2.StartChildTimer("D"))
                    {

                    }
                }
                using (timer.StartChildTimer("E"))
                { }
            }
            Validate(@"A\s+: (\d+)\n[ ]{4}B\s+: (\d+)\n[ ]{4}C\s+: (\d+)\n[ ]{8}D\s+: (\d+)\n[ ]{8}Rest\s+: (\d+)\n[ ]{4}E\s+: (\d+)\n[ ]{4}Rest\s+: (\d+)\n",
                writer.ToString());

/* Пример ответа
A                   : 0
    B               : 0
    C               : 0
        D           : 0
        Rest        : 0
    E               : 0
    Rest            : 0
*/
        }

        [Test]
        public void DeepNestingTime()
        {
            var writer = new StringWriter();
            using (var timer = Timer.Start(writer,"A"))
            {
                using (timer.StartChildTimer("B"))
                {
                    Thread.Sleep(100);
                }
                using (timer.StartChildTimer("C"))
                {
                    Thread.Sleep(200);
                }
                Thread.Sleep(300);
            }
            var values = Validate(@"A\s+: (\d+)\n[ ]{4}B\s+: (\d+)\n[ ]{4}C\s+: (\d+)\n[ ]{4}Rest\s+: (\d+)",
                                  writer.ToString());
            Assert.True(values[0] == values[1] + values[2] + values[3]);
            Assert.AreEqual(2, (double)values[2] / values[1], 1);
            Assert.AreEqual(3, (double)values[3] / values[1], 1);
/*Пример ответа
A                   : 600
    B               : 100
    C               : 200
    Rest            : 300
*/
        }
    }
}
