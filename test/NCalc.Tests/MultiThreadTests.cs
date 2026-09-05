using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Multiple Threads")]
public class MultiThreadTests
{
    private List<Exception> _exceptions;

    [Test]
    public void Should_Reuse_Compiled_Expressions_In_Multi_Threaded_Mode()
    {
        for (int cpt = 0; cpt < 20; cpt++)
        {
            const int nbthreads = 30;
            _exceptions = new List<Exception>();
            var threads = new Thread[nbthreads];

            for (int i = 0; i < nbthreads; i++)
            {
                var thread = new Thread(WorkerThread);
                thread.Start();
                threads[i] = thread;
            }

            bool running = true;
            while (running)
            {
                Thread.Sleep(100);
                running = false;
                for (int i = 0; i < nbthreads; i++)
                {
                    if (threads[i].ThreadState == ThreadState.Running)
                        running = true;
                }
            }

            if (_exceptions.Count > 0)
            {
                Console.WriteLine(_exceptions[0].StackTrace);
                Assert.Fail("Assertion failure");
            }
        }
    }

    private void WorkerThread()
    {
        try
        {
            var r1 = new Random((int)DateTime.Now.Ticks);
            var r2 = new Random((int)DateTime.Now.Ticks);
            int n1 = r1.Next(10);
            int n2 = r2.Next(10);

            var exp = n1 + " + " + n2;
            var e = new Expression(exp);
            if (!e.Evaluate().Equals(n1 + n2))
            {
                throw new InvalidOperationException("Expression should evaluate to the expected sum.");
            }
        }
        catch (Exception e)
        {
            _exceptions.Add(e);
        }
    }
}
