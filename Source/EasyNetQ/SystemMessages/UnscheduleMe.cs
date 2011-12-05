using System;

namespace EasyNetQ.SystemMessages
{
    [Serializable]
    public class UnscheduleMe : IAssociable
    {
        public UnscheduleMe(string name, string group)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A name must not be null or whitespace only!", "name");

            Name = name;
            Group = group;
        }

        public string Name { get; set; }
        public string Group { get; set; }
    }
}