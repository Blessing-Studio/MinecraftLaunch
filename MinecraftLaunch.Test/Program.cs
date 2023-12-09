using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Components.Resolver;
using System.Diagnostics;

string gameFolder = "C:\\Users\\w\\Desktop\\temp\\.minecraft";

YggdrasilAuthenticator authenticator = new("https://littleskin.cn/api/yggdrasil", "3424968114@qq.com","wxysdsb123");
var result = (await authenticator.AuthenticateAsync())
    .ToList();

var account = result.FirstOrDefault();
authenticator = new(account);
var result1 = await authenticator.AuthenticateAsync();
foreach (var item in result1)
    Console.WriteLine(item.Name);

Console.ReadKey();