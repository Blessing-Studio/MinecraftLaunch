using MinecraftLaunch.Components.Authenticator;

MicrosoftAuthenticator authenticator = new("9fd44410-8ed7-4eb3-a160-9f1cc62c824c");

var result = await authenticator.DeviceFlowAuthAsync(x => {
    Console.WriteLine(x.UserCode);
});

if(result != null) {
    var account = await authenticator.AuthenticateAsync();
    Console.WriteLine(account.Name);
    Console.WriteLine(account.Uuid);
}

Console.ReadKey();