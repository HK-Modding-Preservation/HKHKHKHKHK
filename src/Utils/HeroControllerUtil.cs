using SFCore.Utils;
using System.Reflection;

namespace HKHKHKHKHK.Utils;

public static class HeroControllerUtil
{
    public static T Get<T>(this HeroController self, string name)
    {
        return self.GetAttr<HeroController, T>(name);
    }

    public static void Set<T>(this HeroController self, string name, T val)
    {
        self.SetAttr(name, val);
    }

    public static int Inc(this HeroController self, string name)
    {
        int tmp = self.Get<int>(name);
        tmp += 1;
        self.Set(name, tmp);
        return tmp;
    }

    public static int Dec(this HeroController self, string name)
    {
        int tmp = self.Get<int>(name);
        tmp -= 1;
        self.Set(name, tmp);
        return tmp;
    }

    public static void Add(this HeroController self, string name, float val)
    {
        float tmp = self.Get<float>(name);
        tmp += val;
        self.Set(name, tmp);
    }
    public static void Sub(this HeroController self, string name, float val)
    {
        float tmp = self.Get<float>(name);
        tmp -= val;
        self.Set(name, tmp);
    }

    public static object Inv(this HeroController self, string name, object[] parameters)
    {
        return self.Inv<object>(name, parameters);
    }
    public static T Inv<T>(this HeroController self, string name, object[] parameters)
    {
        return (T) self.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(self, parameters);
    }
}