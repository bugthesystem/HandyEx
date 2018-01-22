/// <summary>
    /// The task extensions.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// It processes an enumerable collection and creates a task that will complete 
        /// when all of the <see cref="T:System.Threading.Tasks.Task`1" /> 
        /// objects in an enumerable collection have completed with limited concurrency.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="taskFunc">The Task factory.</param>
        /// <param name="numberOfTasksGrantedConcurrently">The number of tasks that granted concurrently. </param>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>The <see cref="Task"/>. </returns>
        public static async Task WhenAllWithLimitedConcurrency<T>(IEnumerable<T> items, Func<T, Task> taskFunc, int numberOfTasksGrantedConcurrently)
        {
            using (var semaphore = new SemaphoreSlim(numberOfTasksGrantedConcurrently))
            {
                var tasks = items.Select(async item =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            return taskFunc(item);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                await Task.WhenAll(tasks);
            }
        }
    }
