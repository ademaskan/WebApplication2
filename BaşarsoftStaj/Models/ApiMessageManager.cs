using System.Resources;
using System.Reflection;

namespace BaşarsoftStaj.Models;

public static class ApiMessageManager
{
    private static readonly ResourceManager _resourceManager = new ResourceManager("BaşarsoftStaj.Resources.ApiMessages", Assembly.GetExecutingAssembly());
    
    public static string GetMessage(string key)
    {
        return _resourceManager.GetString(key) ?? key;
    }
}
