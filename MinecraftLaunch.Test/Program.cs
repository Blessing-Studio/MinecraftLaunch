using MinecraftLaunch.Components.Resolver;

GameResolver gameResolver = new("C:\\Users\\w\\Downloads\\.minecraft");

var result = gameResolver.GetGameEntitys();
foreach (var item in result) {
    Console.WriteLine(item.Id);
}