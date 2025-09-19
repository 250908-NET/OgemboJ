



using System;

namespace _6_FlowControl
{
    public class Program
    {
        static void Main(string[] args)
        {
        }

        /// <summary>
        /// This method gets a valid temperature between -40 asnd 135 inclusive from the user
        /// and returns the valid int. 
        /// </summary>
        /// <returns></returns>
        public static int GetValidTemperature()
        {
            int temp = int.Parse(Console.ReadLine());

            if (temp < -40 || temp > 135)
            {
                Console.WriteLine("Invalid temp");
            }
            return temp;

        }

        /// <summary>
        /// This method has one int parameter
        /// It prints outdoor activity advice and temperature opinion to the console 
        /// based on 20 degree increments starting at -20 and ending at 135 
        /// n < -20, Console.Write("hella cold");
        /// -20 <= n < 0, Console.Write("pretty cold");
        ///  0 <= n < 20, Console.Write("cold");
        /// 20 <= n < 40, Console.Write("thawed out");
        /// 40 <= n < 60, Console.Write("feels like Autumn");
        /// 60 <= n < 80, Console.Write("perfect outdoor workout temperature");
        /// 80 <= n < 90, Console.Write("niiice");
        /// 90 <= n < 100, Console.Write("hella hot");
        /// 100 <= n < 135, Console.Write("hottest");
        /// </summary>
        /// <param name="temp"></param>
        public static void GiveActivityAdvice(int temp)
        {
            //throw new NotImplementedException($"GiveActivityAdvice() has not been implemented.");

            switch (temp)
            {
                case < -20:
                    Console.WriteLine("hella cold");
                    break;
                case >= -20 and < 0:
                    Console.WriteLine("pretty cold");
                    break;
                case >= 0 and < 20:
                    Console.WriteLine("cold");
                    break;
                case >= 20 and < 40:
                    Console.WriteLine("thawed out");
                    break;
                case >= 40 and < 60:
                    Console.WriteLine("feels like Autumn");
                    break;
                case >= 60 and < 80:
                    Console.WriteLine("perfect outdoor workout temperature");
                    break;
                case >= 80 and < 90:
                    Console.WriteLine("niiice");
                    break;
                case >= 90 and < 100:
                    Console.WriteLine("hella hot");
                    break;
                case >= 100 and <= 135:
                    Console.WriteLine("hottest");
                    break;
                default:
                    Console.WriteLine("Crazy");
                    break;
            }
        }

        /// <summary>
        /// This method gets a username and password from the user
        /// and stores that data in the global variables of the 
        /// names in the method.
        /// </summary>
        ///
        public static string username;
        public static string password;
        public static void Register()
        {
            //throw new NotImplementedException($"Register() has not been implemented.");

            username = Console.ReadLine();
            password = Console.ReadLine();

            Console.WriteLine($"{username} saved");
        }

        /// <summary>
        /// This method gets username and password from the user and
        /// compares them with the username and password names provided in Register().
        /// If the password and username match, the method returns true. 
        /// If they do not match, the user is reprompted for the username and password
        /// until the exact matches are inputted.
        /// </summary>
        /// <returns></returns>
        public static bool Login()
        {
            while (true)
            {

                Console.Write("Enter username: ");
                string usern = Console.ReadLine();

                Console.Write("Enter password: ");
                string passwordn = Console.ReadLine();

                if (usern == username && passwordn == password)
                {

                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid credentials. Try again.");
                }
            }

        }


        /// <summary>
        /// This method has one int parameter.
        /// It checks if the int is <=42, Console.WriteLine($"{temp} is too cold!");
        /// between 43 and 78 inclusive, Console.WriteLine($"{temp} is an ok temperature");
        /// or > 78, Console.WriteLine($"{temp} is too hot!");
        /// For each temperature range, a different advice is given. 
        /// </summary>
        /// <param name="temp"></param>
        public static void GetTemperatureTernary(int temp)
        {
            if (temp <= 42)
            {
                Console.WriteLine($"{temp} is too cold!");
            }
            else if (temp <= 78)
            {
                Console.WriteLine($"{temp} is an ok temperature");
            }
            else
            {
                Console.WriteLine($"{temp} is too hot!");
            }
        }
    }//EoP
}//EoN
