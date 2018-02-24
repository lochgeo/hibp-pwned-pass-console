using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PwnedPasswords
{
    class Program
    {
        static HttpClient client = new HttpClient() { BaseAddress = new Uri("https://api.pwnedpasswords.com") };

        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            try
            {
                Console.Write($"Please enter your password : ");
                var sha1 = SHA1Hash(GetConsolePassword());
                var pwnedHashes = await GetPwnedHashesAsync(sha1.Substring(0, 5));

                foreach (string hash in pwnedHashes)
                {
                    if (hash.Contains(sha1.Substring(5, 35)))
                    {
                        Console.WriteLine($"Your password was found in Pwned Passwords {hash.Split(':')[1]} times");
                        Console.WriteLine($"Change your password immediately!");
                        Console.ReadKey();
                        return;
                    }
                }

                Console.WriteLine($"Congratulations! Your password was not found in Pwned Passwords");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Sorry, there was a problem processing your request. {e.Message}");
            }
        }

        static string GetConsolePassword()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo keyPress = Console.ReadKey(true);

                if (keyPress.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (keyPress.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                        sb.Length--;

                    continue;
                }

                sb.Append(keyPress.KeyChar);
            }

            return sb.ToString();
        }

        static string SHA1Hash(string password)
        {
            var hashBytes = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(password));
            return string.Join("", hashBytes.Select(b => b.ToString("X2")).ToArray());
        }

        static async Task<List<string>> GetPwnedHashesAsync(string hashPrefix)
        {
            string resp = string.Empty;

            HttpResponseMessage response = await client.GetAsync("/range/" + hashPrefix);

            if (response.IsSuccessStatusCode)
                resp = await response.Content.ReadAsStringAsync();

            return resp.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }
    }
}
