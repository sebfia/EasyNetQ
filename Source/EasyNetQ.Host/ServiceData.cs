namespace EasyNetQ.Host
{
    internal class ServiceData
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        public ServiceData(string name, string displayName, string description)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
        }

        public static ServiceData FromArguments(string[] args)
        {
            var name = args.GetOption("s");
            var displayName = args.GetOption("n");
            var description = args.GetOption("d");

            return new ServiceData(name, displayName, description);
        }

        public bool IsValidForInstall
        {
            get { return Name.NotNullOrWhitespace() && DisplayName.NotNullOrWhitespace(); }
        }

        public bool IsValidForUninstall
        {
            get { return Name.NotNullOrWhitespace(); }
        }
    }
}