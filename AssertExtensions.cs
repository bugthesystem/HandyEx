 public static class AssertExtensions {
 
     public static async Task AssertUntilTimeout(int timeoutMs, Func <Task> body, int delayMs = 1000) {
         DateTime startTime = DateTime.UtcNow;
         DateTime endTime = startTime + TimeSpan.FromMilliseconds(timeoutMs);
         int count = 0;

         do {
          await Task.Delay(delayMs);
          count++;
          await body();
         }
         while (DateTime.UtcNow < endTime);

         Console.Out.WriteLine($ "Passed {count} times");
     }
     
 }
