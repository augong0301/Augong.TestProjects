using Augong.CSharp.Contract;

namespace Augong.CSharp.TestClass.Tasks
{
    public class TimerTest : ITest
    {
        public TimerTest() { }

        public void DoTest()
        {
            var timer = new Timer(delegate
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine(i);
                }
            }, null, Timeout.Infinite, Timeout.Infinite); // 都为 infinite 则不调用

            // 3000 是callback调用前的延迟 
            //timer.Change(3000, Timeout.Infinite);

            // 3000 是两次调用之间的 timespan
            timer.Change(0, 3000);


            Console.ReadKey();
        }
    }
}
