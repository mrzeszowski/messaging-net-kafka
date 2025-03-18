using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chat.Messaging.Kafka;

internal class KafkaConsumerWorker : IConsumerWorker
{
    private IConsumer<byte[],byte[]>? _consumer;
    private CancellationToken _cancellationToken;
    
    private readonly ConsumerConfig _consumerConfig;

    private readonly string _consumerGroupId;
    private readonly Subscription _subscription;
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly MessageProcessor _processor;

    public KafkaConsumerWorker(string consumerGroupId, 
        Subscription subscription,
        ILogger<KafkaConsumerWorker> logger,
        MessageProcessor processor,
        IOptions<KafkaOptions> options)
    {
        _consumerGroupId = consumerGroupId;
        _subscription = subscription;
        _logger = logger;
        _processor = processor;
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = consumerGroupId,
            EnableAutoCommit = true, // important
            EnableAutoOffsetStore = false, // important
            SecurityProtocol = options.Value.SecurityProtocol,
            SaslMechanism = options.Value.SaslMechanism,
            SaslUsername = options.Value.SaslUsername,
            SaslPassword = options.Value.SaslPassword,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true,
            // A good introduction to the CooperativeSticky assignor and incremental rebalancing:
            // https://www.confluent.io/blog/cooperative-rebalancing-in-kafka-streams-consumer-ksqldb/
            // PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.RoundRobin,
            // Debug = "consumer,cgrp,topic,fetch"
        };
    }

    public Task ConsumeAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _consumer = new ConsumerBuilder<byte[], byte[]>(_consumerConfig)
            // .SetLogHandler(..)
            // .SetErrorHandler(..)
            // .SetPartitionsRevokedHandler(..)
            // .SetPartitionsLostHandler(..)
            .Build();

        _consumer.Subscribe(_subscription.Topics);
        return Task.Run(ConsumeAsync, CancellationToken.None);
    }
    
    private async Task ConsumeAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer!.Consume(_cancellationToken);
                if (consumeResult.IsPartitionEOF)
                {
                    continue;
                }
                
                var message = new ConsumedMessage(
                    consumeResult.Offset.Value,
                    consumeResult.Partition.Value,
                    Encoding.UTF8.GetString(consumeResult.Message.Key),
                    consumeResult.Message.Value,
                    consumeResult.Message.Headers.ToDictionary(x => x.Key, x => Encoding.UTF8.GetString(x.GetValueBytes())),
                    consumeResult.Topic,
                    _consumerGroupId);
                
                await _processor.ProcessAsync(message, _cancellationToken);

                _consumer.StoreOffset(consumeResult);
            }
            catch (ConsumeException consumeException) when (!consumeException.Error.IsFatal)
            {
                _logger.LogError(consumeException, "Consume exception");
            }
            catch (KafkaException kafkaException) when (kafkaException.Error.Code == ErrorCode.Local_State)
            {
                _logger.LogError(kafkaException, "Kafka exception");
                break;
            }
            catch (OutOfMemoryException)
            {
                _logger.LogError("Out of memory exception   ");
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation canceled exception");
            }
            catch (Exception ex)
            {
                if (ex is KafkaException)
                {
                    _logger.LogError(ex, "Kafka exception");
                }

                _logger.LogError(ex, ex.Message);
                break;
            }
        }

        // close & dispose
        DisposeConsumer();
    }
    
    private void DisposeConsumer()
    {
        try
        {
            _consumer!.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Close consumer exception");
        }

        try
        {
            _consumer!.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dispose consumer exception");
        }
    }
}