namespace Fraggoplex.SDK
{
    public class LaunchArgs
    {
        public string Token { get; private set; }
        public string BuildSha256 { get; private set; }    

        public static LaunchArgs Parse(string[] args)
        {
            LaunchArgs result = new LaunchArgs();

            if (args == null)
                return result;

            for (int i = 0; i <  args.Length - 1; i++)
            {
                if (args[i] == "--fraggo-token")
                {
                    result.Token = args[i + 1];
                    i++;
                }

                else if (args[i] == "--fraggo-build-sha256")
                {
                    result.BuildSha256 = args[i + 1];
                    i++;
                }
            }

            return result;
        }
    }
}