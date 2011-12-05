namespace EasyNetQ.Spring
{
    internal interface IComponentConfig
    {
        IComponentConfig ConfigureProperty(string name, object value);
    }
}