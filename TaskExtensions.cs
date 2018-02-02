namespace HandyEx
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The task extensions.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// It processses an enumerable collection and creates a task that will complete when all of the <see cref="T:System.Threading.Tasks.Task`1" /> 
        /// objects in an enumerable collection have completed with limited concurrency.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="func">The Task factory.</param>
        /// <param name="numberOfTasksGrantedConcurrently">The number of tasks that granted concurrently. </param>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>The <see cref="Task"/>. </returns>
        public static async Task<ConcurrentDictionary<T, Exception>> WhenAllByThrottlingAsync<T>(IEnumerable<T> items, Func<T, Task> func, int numberOfTasksGrantedConcurrently)
        {
            var failures = new ConcurrentDictionary<T, Exception>();

            using (var semaphore = new SemaphoreSlim(numberOfTasksGrantedConcurrently))
            {
                var tasks = items.Select(async item =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await func(item);
                        }
                        catch (Exception e)
                        {
                            failures.TryAdd(item, e);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                await Task.WhenAll(tasks);

                return failures;
            }
        }
    }
}
