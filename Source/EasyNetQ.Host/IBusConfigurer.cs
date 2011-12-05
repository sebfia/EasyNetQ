namespace EasyNetQ
{
    internal interface IBusConfigurer
    {
        IConnectionFactory ConnectionFactory { get; set; }
        IConsumerErrorStrategy ConsumerErrorStrategy { get; set; }
        ISerializer Serializer { get; set; }
        IEasyNetQLogger Logger { get; set; }
    }
}
