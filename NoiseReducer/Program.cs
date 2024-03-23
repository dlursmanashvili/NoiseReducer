using NoiseReducer;

Console.WriteLine("please enter Json Full Path:");
var JsonFilePath = Console.ReadLine();

Console.WriteLine("please enter Png Full Path:");
var DiagramImgSavePath = Console.ReadLine();

if (File.Exists(JsonFilePath) && File.Exists(DiagramImgSavePath))
{
    // Check file extensions
    string jsonExtension = Path.GetExtension(JsonFilePath);
    string pngExtension = Path.GetExtension(DiagramImgSavePath);

    if (jsonExtension.Equals(".json", StringComparison.OrdinalIgnoreCase) && pngExtension.Equals(".png", StringComparison.OrdinalIgnoreCase))
    {
        DataSmoothering.Init(JsonFilePath, DiagramImgSavePath);
        // Proceed with your logic here
    }
    else
    {
        Console.WriteLine("One or both files exist, but one or both have incorrect extensions.");
        // Optionally, you can prompt the user to re-enter the paths or handle the situation accordingly
    }
}
else
{
    Console.WriteLine("One or both files do not exist.");
    // Optionally, you can prompt the user to re-enter the paths or handle the situation accordingly
}
