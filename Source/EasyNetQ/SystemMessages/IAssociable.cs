namespace EasyNetQ.SystemMessages
{
    public interface IAssociable
    {
        string Name { get; set; }
        string Group { get; set; }
    }
}