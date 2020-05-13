using Amazon.CognitoIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAWS
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            CognitoHelper helper = new CognitoHelper();
            Amazon.Extensions.CognitoAuthentication.CognitoUser cognitoUser = null;
            CognitoAWSCredentials credentials = null;
            int menu = 1;
            string username = string.Empty,
                password = string.Empty,
                email = string.Empty;
            while(menu > 0)
            {
                Console.WriteLine("1.Tạo tài khoản\n2.Đăng nhập\n3.Đổi pass\n4.Show credential\n5.Call function\n0.Quit");
                menu = Convert.ToInt32(Console.ReadLine());
                switch (menu)
                {
                    case 1:
                        Console.WriteLine("Username:");
                        username = Console.ReadLine();
                        Console.WriteLine("password:");
                        password = Console.ReadLine();
                        Console.WriteLine("Email:");
                        email = Console.ReadLine();
                        bool success = await  helper.SignUpUser(username, password, email, string.Empty);
                        if (success)
                        {
                            Console.WriteLine("Created successfull!");

                        }
                        else
                        {
                            Console.WriteLine("Unable to add the user");
                        }
                        break;
                    case 2:
                        Console.WriteLine("Username:");
                        username = Console.ReadLine();
                        Console.WriteLine("password:");
                        password = Console.ReadLine();
                        cognitoUser = await helper.ValidateUser(username, password);
                        Console.WriteLine("Login: " + cognitoUser.Username);
                        break;
                    case 3:
                        Console.WriteLine("Username:");
                        string oldpass = Console.ReadLine();
                        Console.WriteLine("password:");
                        string newpass = Console.ReadLine();
                        cognitoUser.ChangePasswordAsync(oldpass, newpass);
                        Console.WriteLine("Login: " + cognitoUser.Username);
                        break;
                    case 4:
                        try
                        {
                            credentials = await helper.ShowCredentialAsync(cognitoUser);
                            StringBuilder bucketlist = new StringBuilder();

                            bucketlist.Append("================Cognito Credentails==================\n");
                            bucketlist.Append("Access Key - " + credentials.GetCredentials().AccessKey);
                            bucketlist.Append("\nSecret - " + credentials.GetCredentials().SecretKey);
                            bucketlist.Append("\nSession Token - \n" + credentials.GetCredentials().Token);

                            bucketlist.Append("\n================User Buckets==================\n");
                            Console.WriteLine(cognitoUser.Username);
                            Console.WriteLine(bucketlist.ToString());
                        }
                        catch
                        {
                            Console.WriteLine("Not Login.");
                        }
                        
                        break;
                    case 5:
                        try
                        {
                            string function = Console.ReadLine();
                            string result = await helper.CallLambdaAsync(credentials, function, new { Name = "Hoàng Hà Linh",username = "halinh", Age=2, Married=true, Job= "Family Star", Website= "HaLinh.LovalyLucky.Love"});
                            Console.WriteLine("Result :" + result);
                        }
                        catch
                        {
                            Console.WriteLine("Error for call function");
                        }

                break;
                    case 0:

                        break;
                    default:
                        menu = 1;
                        Console.WriteLine("1.Tạo tài khoản\n2.Đăng nhập\n3.Đổi pass\n4.Show credential\n5.Call function\n0.Quit");
                        break;
                }
            }
            int age;
            string name;

            
            Console.ReadLine();
        }
    }
}
