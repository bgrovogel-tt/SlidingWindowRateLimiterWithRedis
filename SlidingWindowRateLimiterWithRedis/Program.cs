using System;

class Program
{
    static void Main()
    {
        string redisConnectionString = "localhost"; // Adjust according to your Redis server configuration
        SlidingWindowRateLimiter rateLimiter = new SlidingWindowRateLimiter(redisConnectionString);

        // Add an IP address to the whitelist
        rateLimiter.AddToWhitelist("192.168.1.1");

        string ipAddress = "192.168.1.2";
        int minuteLimit = 5;
        int hourLimit = 100;
        int dayLimit = 1000;

        TimeSpan minuteWindow = TimeSpan.FromMinutes(1);
        TimeSpan hourWindow = TimeSpan.FromHours(1);
        TimeSpan dayWindow = TimeSpan.FromDays(1);

        try
        {
            if (!rateLimiter.IsRateLimited(ipAddress, "minute", minuteLimit, minuteWindow))
            {
                rateLimiter.IncrementRequestCount(ipAddress, "minute", minuteWindow);
                Console.WriteLine("Request allowed (minute).");
            }
            else
            {
                Console.WriteLine("Rate limit exceeded (minute).");
            }

            if (!rateLimiter.IsRateLimited(ipAddress, "hour", hourLimit, hourWindow))
            {
                rateLimiter.IncrementRequestCount(ipAddress, "hour", hourWindow);
                Console.WriteLine("Request allowed (hour).");
            }
            else
            {
                Console.WriteLine("Rate limit exceeded (hour).");
            }

            if (!rateLimiter.IsRateLimited(ipAddress, "day", dayLimit, dayWindow))
            {
                rateLimiter.IncrementRequestCount(ipAddress, "day", dayWindow);
                Console.WriteLine("Request allowed (day).");
            }
            else
            {
                Console.WriteLine("Rate limit exceeded (day).");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}