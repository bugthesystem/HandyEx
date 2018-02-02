namespace HandyEx.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static HandyEx.TaskExtensions;

    /// <summary>
    /// The task extentions test.
    /// </summary>
    [TestClass]
    public class TaskExtentionsTest
    {
        /// <summary>
        /// The number of tasks granted concurrently.
        /// </summary>
        private const int NumberOfTasksGrantedConcurrently = 5;

        /// <summary>
        /// The random gen.
        /// </summary>
        private readonly Random randomGen = new Random(5);

        /// <summary>
        /// The assert task extentions when all by throttling async works.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task AssertTaskExtentionsWhenAllByThrottlingAsyncWorks()
        {
            List<string> numbers = new List<string>();

            for (var i = 0; i < 50; i++)
            {
                numbers.Add(i.ToString());
            }

            var successCount = 0;

            var task = WhenAllByThrottlingAsync(
                numbers,
                async s =>
                    {
                        await Task.Delay(1000 / this.randomGen.Next(1, 5));
                        Console.WriteLine(s);
                        Interlocked.Increment(ref successCount);
                    },
                NumberOfTasksGrantedConcurrently);
            await task;

            Assert.IsNull(task.Exception);
            Assert.IsTrue(successCount == numbers.Count);
        }

        /// <summary>
        /// The assert task extentions when all by throttling async works.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task AssertTaskExtentionsWhenAllByThrottlingAsyncWithHandlingFailuresWorks()
        {
            var successCount = 0;
            var failCount = 0;

            List<string> numbers = new List<string>();

            for (var i = 0; i < 50; i++)
            {
                numbers.Add(i.ToString());
            }

            var failedItems = new ConcurrentDictionary<string, Exception>();

            var task = WhenAllByThrottlingAsync(
                numbers,
                 s =>
                    {
                        try
                        {
                            var next = this.randomGen.Next(1, 5);
                            if (next % 4 == 0)
                            {
                                Interlocked.Increment(ref failCount);
                                return this.ThrowException(s);
                            }

                            Console.WriteLine(s);
                            Interlocked.Increment(ref successCount);
                            return this.DelayAsync(next);
                        }
                        catch (Exception e)
                        {
                            failedItems.TryAdd(s, e);
                            return Task.CompletedTask;
                        }
                    },
                NumberOfTasksGrantedConcurrently);

            var failues = await task;

            Assert.IsTrue(failues.IsEmpty);
            Assert.IsTrue(successCount == numbers.Count - failCount);
            Assert.IsTrue(failCount == failedItems.Count);
        }

        /// <summary>
        /// The assert task extentions when all by throttling async with handling task failures from exc works.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task AssertTaskExtentionsWhenAllByThrottlingAsyncWithHandlingTaskFailuresFromExcWorks()
        {
            var successCount = 0;
            var failCount = 0;

            List<string> numbers = new List<string>();

            for (var i = 0; i < 50; i++)
            {
                numbers.Add(i.ToString());
            }

            var task = WhenAllByThrottlingAsync(
                numbers,
                async s =>
                    {
                        Console.WriteLine(s);
                        var next = this.randomGen.Next(1, 5);
                        if (next % 4 == 0)
                        {
                            Interlocked.Increment(ref failCount);
                            await this.FromException(s);
                        }

                        Interlocked.Increment(ref successCount);
                        await this.DelayAsync(next);
                    },
                NumberOfTasksGrantedConcurrently);

            var failures = await task;

            Assert.IsNull(task.Exception);
            Assert.IsTrue(successCount == numbers.Count - failCount);
            Assert.IsTrue(failures.Count == failCount);
        }

        /// <summary>
        /// The assert task extentions when all by throttling async with handling failures by task or throw works.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task AssertTaskExtentionsWhenAllByThrottlingAsyncWithHandlingFailuresByTaskOrThrowWorks()
        {
            var successCount = 0;
            var failCount = 0;
            var failWithThrowCount = 0;

            List<string> numbers = new List<string>();

            for (var i = 0; i < 50; i++)
            {
                numbers.Add(i.ToString());
            }

            var failedItems = new ConcurrentDictionary<string, Exception>();

            var task = WhenAllByThrottlingAsync(
                numbers,
                s =>
                    {
                        var next = this.randomGen.Next(1, 5);
                        if (next % 4 == 0)
                        {
                            Interlocked.Increment(ref failCount);
                            Interlocked.Increment(ref failWithThrowCount);
                            return this.ThrowException(s);
                        }

                        if (next % 3 == 0)
                        {
                            Interlocked.Increment(ref failCount);
                            return this.FromException(s);
                        }

                        Console.WriteLine(s);
                        Interlocked.Increment(ref successCount);
                        return this.DelayAsync(next);
                    },
                NumberOfTasksGrantedConcurrently);

            var failues = await task;

            Assert.IsTrue(successCount == numbers.Count - failCount);
            Assert.IsTrue(failues.Count == failCount);
        }

        /// <summary>The from exception.</summary>
        /// <param name="s">The string.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task FromException(string s)
        {
            await Task.FromException(new Exception($"Exc: {s}"));
        }

        /// <summary>The delay async.</summary>
        /// <param name="next">The next.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task DelayAsync(int next)
        {
            await Task.Delay(1000 / next);
        }

        /// <summary>The throw exception.</summary>
        /// <param name="s">The string.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private Task ThrowException(string s)
        {
            throw new Exception($"Exc: {s}");
        }
    }
}
