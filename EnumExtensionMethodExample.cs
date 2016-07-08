enum Stuff
{
    Thing1,
    Thing2
}

static class StuffMethods
{

    public static String GetString(this Stuff s1)
    {
        switch (s1)
        {
            case Stuff.Thing1:
                return "Yeah!";
            case Stuff.Thing2:
                return "Okay!";
            default:
                return "What?!";
        }
    }
}

class Program
{


    static void Main(string[] args)
    {
        Stuff thing = Stuff.Thing1;
        String str = thing.GetString();
    }
}