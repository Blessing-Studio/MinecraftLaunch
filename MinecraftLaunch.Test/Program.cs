using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Checker;
using MinecraftLaunch.Components.Resolver;
using System.Diagnostics;

string gameFolder = "C:\\Users\\w\\Desktop\\temp\\.minecraft";

GameResolver resolver = new(gameFolder);
var gameEntry = resolver.GetGameEntity("1.12.2");

ResourceChecker checker = new(gameEntry);
var result = await checker.CheckAsync();

Console.WriteLine(result);
Console.ReadKey();